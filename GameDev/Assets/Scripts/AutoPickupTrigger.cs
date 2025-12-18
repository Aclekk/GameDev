using UnityEngine;

public class AutoPickupTrigger : MonoBehaviour
{
    [Header("Auto Pickup Settings")]
    [SerializeField] private float pickupRadius = 2f;
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private bool useLayerMask = false;

    [Header("Visual Settings")]
    [SerializeField] private bool showPickupRadius = true;
    [SerializeField] private Color gizmoColor = new Color(0, 1, 0, 0.3f);

    private void OnTriggerEnter(Collider other)
    {
        TryPickupItem(other.gameObject);
    }

    private void TryPickupItem(GameObject obj)
    {
        if (useLayerMask && !IsInLayerMask(obj))
        {
            return;
        }

        PickupItem pickupItem = obj.GetComponent<PickupItem>();
        if (pickupItem != null)
        {
            pickupItem.Pickup();
        }
    }

    private bool IsInLayerMask(GameObject obj)
    {
        return ((1 << obj.layer) & itemLayer) != 0;
    }

    private void OnDrawGizmos()
    {
        if (!showPickupRadius) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, pickupRadius);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showPickupRadius) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
