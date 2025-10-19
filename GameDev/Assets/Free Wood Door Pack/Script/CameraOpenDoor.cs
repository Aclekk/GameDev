using System.Collections;
using UnityEngine;

namespace CameraDoorScript
{
    public class CameraOpenDoor : MonoBehaviour
    {
        [Header("Deteksi & Prompt")]
        public float DistanceOpen = 3f;          // radius dari kamera
        public GameObject text;                  // UI "E to open door"

        [Header("Escape Door (tanpa animasi)")]
        public string escapeTag = "EscapeDoor";  // tag untuk pintu escape
        public string requiredKeyId = "BasementKey";
        public GameObject needKeyText;           // UI "Need a key"
        public GameObject winCanvas;             // UI Win (inactive)
        public MonoBehaviour playerControllerToDisable; // controller player

        [Header("Kamera (gaya jumpscare utk Escape)")]
        public float camApproach = 0.65f;        // jarak di DEPAN pintu (arah -forward)
        public float camHeight   = 1.6f;         // tinggi kamera
        public bool  instantSnap = true;         // langsung snap?
        public float camLerpTime = 0.12f;        // dipakai kalau instantSnap=false
        public float winDelay    = 0.8f;         // jeda sebelum WinCanvas

        // --- runtime
        Inventory inventory;
        Rigidbody playerRb;
        CharacterController playerCc;
        bool winFlow; // block input setelah menang

        void Start()
        {
            inventory = GetComponentInParent<Inventory>();
            playerRb  = GetComponentInParent<Rigidbody>();
            playerCc  = GetComponentInParent<CharacterController>();

            if (winCanvas && winCanvas.activeSelf) winCanvas.SetActive(false);
            if (needKeyText) needKeyText.SetActive(false);
            if (text) text.SetActive(false);
        }

        void OnDisable()
        {
            if (text) text.SetActive(false);
        }

        void Update()
        {
            if (winFlow) return;

            // cari collider dalam radius (termasuk trigger)
            Collider[] hits = Physics.OverlapSphere(transform.position, DistanceOpen, ~0, QueryTriggerInteraction.Collide);

            // kandidat terdekat
            Transform nearestEscape = null;
            DoorScript.Door nearestDoor = null;
            float bestEscapeSqr = float.MaxValue;
            float bestDoorSqr   = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                // cek parent chain: apakah ini EscapeDoor?
                Transform t = hits[i].transform;
                Transform p = t;
                Transform escapeRoot = null;
                while (p != null)
                {
                    if (p.CompareTag(escapeTag)) { escapeRoot = p; break; }
                    p = p.parent;
                }

                if (escapeRoot != null)
                {
                    float sqr = (escapeRoot.position - transform.position).sqrMagnitude;
                    if (sqr < bestEscapeSqr)
                    {
                        bestEscapeSqr = sqr;
                        nearestEscape = escapeRoot;
                    }
                    continue;
                }

                // kalau bukan escape, cek door biasa
                var door = hits[i].GetComponentInParent<DoorScript.Door>();
                if (door == null) continue;

                float dsqr = (door.transform.position - transform.position).sqrMagnitude;
                if (dsqr < bestDoorSqr)
                {
                    bestDoorSqr = dsqr;
                    nearestDoor = door;
                }
            }

            bool hasAny = nearestEscape != null || nearestDoor != null;
            if (text) text.SetActive(hasAny);

            if (!hasAny) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                // pilih yang paling dekat; kalau sama2 ada, bandingkan jarak
                if (nearestEscape != null && bestEscapeSqr <= bestDoorSqr)
                {
                    HandleEscape(nearestEscape);
                }
                else if (nearestDoor != null)
                {
                    nearestDoor.OpenDoor(); // normal door pakai animasi DoorScript.Door
                }
            }
        }

        // ==== Escape Door (tanpa animasi pintu) ====
        void HandleEscape(Transform doorT)
        {
            if (winFlow) return;

            if (inventory == null || !inventory.HasKey(requiredKeyId))
            {
                StartCoroutine(ShowNeedKey());
                return;
            }

            StartCoroutine(DoWinSequence(doorT));
        }

        IEnumerator ShowNeedKey()
        {
            if (!needKeyText) { Debug.Log("Need a key"); yield break; }
            needKeyText.SetActive(true);
            yield return new WaitForSeconds(1.2f);
            needKeyText.SetActive(false);
        }

        IEnumerator DoWinSequence(Transform doorT)
        {
            winFlow = true;

            // Matikan kontrol & freeze player
            if (playerControllerToDisable) playerControllerToDisable.enabled = false;
            if (playerRb)
            {
                playerRb.velocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
                playerRb.isKinematic = true;
            }
            if (playerCc) playerCc.enabled = false;

            // Kamera: posisikan di depan pintu, menghadap pintu (pintu TIDAK dianimasikan)
            Transform camT = transform;
            Vector3 doorPos = doorT.position;

            Vector3 targetPos = doorPos - doorT.forward * camApproach + Vector3.up * camHeight;
            Vector3 look      = doorPos + Vector3.up * camHeight;
            Quaternion targetRot = Quaternion.LookRotation((look - targetPos).normalized, Vector3.up);

            if (instantSnap || camLerpTime <= 0.01f)
            {
                camT.position = targetPos; camT.rotation = targetRot;
            }
            else
            {
                Vector3 startPos = camT.position; Quaternion startRot = camT.rotation;
                float t = 0f;
                while (t < camLerpTime)
                {
                    t += Time.deltaTime;
                    float k = Mathf.Clamp01(t / camLerpTime);
                    camT.position = Vector3.Lerp(startPos, targetPos, k);
                    camT.rotation = Quaternion.Slerp(startRot, targetRot, k);
                    yield return null;
                }
                camT.position = targetPos; camT.rotation = targetRot;
            }

            camT.SetParent(doorT, true); // kunci kamera ke pintu (pintu tidak bergerak)

            yield return new WaitForSeconds(winDelay);

            if (winCanvas)
            {
                winCanvas.SetActive(true);
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (text) text.SetActive(false);
        }

        // Debug radius
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, DistanceOpen);
        }
    }
}
