using UnityEngine;

namespace CameraDoorScript
{
    public class CameraOpenDoor : MonoBehaviour
    {
        [Header("Deteksi & Prompt")]
        public float DistanceOpen = 3f;          // radius dari kamera
        public GameObject text;                  // UI "E to open door"

        [Header("Escape Door")]
        public string escapeTag = "EscapeDoor";  // tag untuk pintu escape (routing saja)

        void Start()
        {
            if (text) text.SetActive(false);
        }

        void OnDisable()
        {
            if (text) text.SetActive(false);
        }

        void Update()
        {
            // cari collider dalam radius (termasuk trigger)
            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                DistanceOpen,
                ~0,
                QueryTriggerInteraction.Collide
            );

            Transform       nearestEscape = null;
            DoorScript.Door nearestDoor   = null;
            float bestEscapeSqr = float.MaxValue;
            float bestDoorSqr   = float.MaxValue;

            // pilih kandidat terdekat
            for (int i = 0; i < hits.Length; i++)
            {
                Transform t = hits[i].transform;

                // cek EscapeDoor via parent chain & tag
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

                // pintu biasa
                var door = hits[i].GetComponentInParent<DoorScript.Door>();
                if (door == null) continue;

                float dsqr = (door.transform.position - transform.position).sqrMagnitude;
                if (dsqr < bestDoorSqr)
                {
                    bestDoorSqr = dsqr;
                    nearestDoor = door;
                }
            }

            bool hasAny = (nearestEscape != null) || (nearestDoor != null);
            if (text) text.SetActive(hasAny);
            if (!hasAny) return;

            // Interaksi
            if (Input.GetKeyDown(KeyCode.E))
            {
                // pilih yang paling dekat (escape vs normal)
                if (nearestEscape != null && bestEscapeSqr <= bestDoorSqr)
                {
                    // ROUTE ONLY ke EscapeDoor (biar EscapeDoor.cs yang urus key/freeze/win)
                    var esc = nearestEscape.GetComponent<EscapeDoor>();
                    if (esc != null)
                    {
                        // Use SendMessage so this compiles even if EscapeDoor doesn't define the method;
                        // SendMessage will call TryInteractFromCamera if it's implemented, otherwise it's ignored.
                        esc.SendMessage("TryInteractFromCamera", SendMessageOptions.DontRequireReceiver);
                    }
                    else
                    {
                        var escChild = nearestEscape.GetComponentInChildren<EscapeDoor>(true);
                        if (escChild != null)
                        {
                            escChild.SendMessage("TryInteractFromCamera", SendMessageOptions.DontRequireReceiver);
                        }
                        else
                        {
                            // As a fallback, send the message to the escape root so any handler may respond.
                            nearestEscape.SendMessage("TryInteractFromCamera", SendMessageOptions.DontRequireReceiver);
                            Debug.LogWarning("[CameraOpenDoor] Object bertag EscapeDoor tapi komponen EscapeDoor tidak ditemukan.");
                        }
                    }
                }
                else if (nearestDoor != null)
                {
                    nearestDoor.OpenDoor(); // pintu biasa tetap pakai animasi DoorScript.Door
                }
            }
        }

        // Debug radius
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, DistanceOpen);
        }
    }
}
