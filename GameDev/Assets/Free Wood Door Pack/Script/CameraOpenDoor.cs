using System.Collections.Generic;
using UnityEngine;

namespace CameraDoorScript
{
    public class CameraOpenDoor : MonoBehaviour
    {
        [Header("Interact")]
        [Tooltip("Radius interaksi sekitar kamera (meter).")]
        public float interactRadius = 3f;

        [Tooltip("Opsional: filter layer pintu/prop untuk efisiensi.")]
        public LayerMask doorMask = ~0; // default: semua layer

        [Header("UI")]
        public GameObject text; // "Press E to open"

        // cache door yang sedang paling dekat (biar gak GetComponent terus)
        private DoorScript.Door nearestDoor;

        void Update()
        {
            FindNearestDoorInRadius();

            if (nearestDoor != null)
            {
                if (text && !text.activeSelf) text.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    nearestDoor.OpenDoor();
                }
            }
            else
            {
                if (text && text.activeSelf) text.SetActive(false);
            }
        }

        private void FindNearestDoorInRadius()
        {
            nearestDoor = null;

            // Ambil semua collider di radius
            Collider[] cols = Physics.OverlapSphere(transform.position, interactRadius, doorMask, QueryTriggerInteraction.Collide);
            if (cols == null || cols.Length == 0) return;

            float bestSqr = float.PositiveInfinity;
            Vector3 p = transform.position;

            for (int i = 0; i < cols.Length; i++)
            {
                // Cari komponen Door di collider atau parent-nya
                var door = cols[i].GetComponent<DoorScript.Door>();
                if (door == null) door = cols[i].GetComponentInParent<DoorScript.Door>();
                if (door == null) continue;

                // Jarak terdekat
                float sqr = (cols[i].transform.position - p).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearestDoor = door;
                }
            }
        }

        // Visual bantu di editor
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
