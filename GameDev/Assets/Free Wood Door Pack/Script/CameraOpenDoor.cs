using System.Collections;
using UnityEngine;

namespace CameraDoorScript
{
    public class CameraOpenDoor : MonoBehaviour
    {
        [Header("Deteksi & Prompt")]
        public float DistanceOpen = 3f;          // radius dari kamera
        public GameObject text;                  // UI "E to open door"

        [Header("Escape Door (tanpa animasi/snap)")]
        public string escapeTag = "EscapeDoor";  // tag untuk pintu escape
        public string requiredKeyId = "BasementKey";
        public GameObject needKeyText;           // UI "Need a key" (auto-hide via coroutine)
        public GameObject winCanvas;             // UI Win (inactive)
        public MonoBehaviour playerControllerToDisable; // controller player (FPC/HeroController)
        public float winDelay = 0.8f;            // jeda sebelum WinCanvas

        // --- runtime
        Inventory inventory;
        bool winFlow;                // block input setelah menang
        Coroutine needKeyCo;

        void Start()
        {
            inventory = GetComponentInParent<Inventory>();

            if (winCanvas && winCanvas.activeSelf) winCanvas.SetActive(false);
            if (needKeyText) needKeyText.SetActive(false);
            if (text) text.SetActive(false);
        }

        void OnDisable()
        {
            if (text) text.SetActive(false);
            if (needKeyText) needKeyText.SetActive(false);
        }

        void Update()
        {
            if (winFlow) return;

            // cari collider dalam radius (termasuk trigger)
            Collider[] hits = Physics.OverlapSphere(transform.position, DistanceOpen, ~0, QueryTriggerInteraction.Collide);

            // kandidat terdekat
            Transform       nearestEscape = null;
            DoorScript.Door nearestDoor   = null;
            float bestEscapeSqr = float.MaxValue;
            float bestDoorSqr   = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                // cek apakah termasuk EscapeDoor (via tag di parent chain)
                Transform p = hits[i].transform;
                Transform escapeRoot = null;
                while (p != null)
                {
                    if (p.CompareTag(escapeTag)) { escapeRoot = p; break; }
                    p = p.parent;
                }

                if (escapeRoot != null)
                {
                    float sqr = (escapeRoot.position - transform.position).sqrMagnitude;
                    if (sqr < bestEscapeSqr) { bestEscapeSqr = sqr; nearestEscape = escapeRoot; }
                    continue;
                }

                // pintu biasa
                var door = hits[i].GetComponentInParent<DoorScript.Door>();
                if (door == null) continue;

                float dsqr = (door.transform.position - transform.position).sqrMagnitude;
                if (dsqr < bestDoorSqr) { bestDoorSqr = dsqr; nearestDoor = door; }
            }

            bool hasAny = (nearestEscape != null) || (nearestDoor != null);
            if (text) text.SetActive(hasAny);
            if (!hasAny) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                // pilih yang paling dekat (escape vs normal)
                if (nearestEscape != null && bestEscapeSqr <= bestDoorSqr)
                {
                    HandleEscape();
                }
                else if (nearestDoor != null)
                {
                    nearestDoor.OpenDoor(); // normal door tetap pakai animasi
                }
            }
        }

        // ==== Escape Door: tanpa animasi & tanpa sentuh kamera ====
        void HandleEscape()
        {
            if (winFlow) return;

            if (inventory == null || !inventory.HasKey(requiredKeyId))
            {
                if (needKeyCo != null) StopCoroutine(needKeyCo);
                needKeyCo = StartCoroutine(ShowNeedKey());
                return;
            }

            StartCoroutine(WinSequence_FreezeOnly());
        }

        IEnumerator ShowNeedKey()
        {
            if (!needKeyText) { Debug.Log("Need a key"); yield break; }
            needKeyText.SetActive(true);
            yield return new WaitForSeconds(1.2f);
            needKeyText.SetActive(false);
            needKeyCo = null;
        }

        IEnumerator WinSequence_FreezeOnly()
        {
            winFlow = true;

            // Freeze player: cukup matikan kontrol (tidak sentuh kamera/rigidbody/CC)
            if (playerControllerToDisable) playerControllerToDisable.enabled = false;

            yield return new WaitForSeconds(winDelay);

            if (winCanvas)
            {
                winCanvas.SetActive(true);
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
