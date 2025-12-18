using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private string itemId;
    [SerializeField] private string itemName = "Item";
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private int quantity = 1;
    [SerializeField] private int maxStack = 1;

    [Header("Pickup Settings")]
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private AudioClip pickupSound;

    private bool hasBeenPickedUp = false;

    private void Start()
    {
        if (string.IsNullOrEmpty(itemId))
        {
            itemId = gameObject.name;
        }
    }

    public void Pickup()
    {
        if (hasBeenPickedUp) return;

        if (InventorySystem.Instance == null)
        {
            Debug.LogError("InventorySystem not found! Make sure it exists in the scene.");
            return;
        }

        bool success = InventorySystem.Instance.AddItem(itemId, itemName, itemIcon, quantity, maxStack);

        if (success)
        {
            hasBeenPickedUp = true;

            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(itemName))
        {
            itemName = gameObject.name;
        }
    }
}
