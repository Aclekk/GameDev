using UnityEngine;
using UHFPS.Runtime;

public class LanternPickUp : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRadius = 1.5f;
    public AudioClip pickupSound;
    [Range(0f, 1f)] public float pickupVolume = 0.8f;

    [Header("Lantern Item Reference")]
    public int lanternItemIndex = 0;

    Transform playerBody;
    PlayerItemsManager itemsManager;
    bool hasBeenPickedUp = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (!playerObj)
        {
            Debug.LogError("[LanternPickUp] Player dengan tag 'Player' tidak ditemukan!");
            enabled = false;
            return;
        }

        itemsManager = playerObj.GetComponentInChildren<PlayerItemsManager>();
        if (!itemsManager)
        {
            Debug.LogError($"[LanternPickUp] PlayerItemsManager tidak ditemukan di {playerObj.name} atau children-nya!");
            enabled = false;
            return;
        }

        playerBody = playerObj.transform;
    }

    void Update()
    {
        if (!playerBody || !itemsManager || hasBeenPickedUp) return;

        float distance = HorizontalDistance(playerBody.position, transform.position);

        if (distance <= pickupRadius)
        {
            PickupLantern();
        }
    }

    void PickupLantern()
    {
        hasBeenPickedUp = true;

        if (pickupSound)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
        }

        if (itemsManager.PlayerItems.Count > lanternItemIndex)
        {
            PlayerItemBehaviour lanternItem = itemsManager.PlayerItems[lanternItemIndex];
            
            if (lanternItem != null)
            {
                itemsManager.SwitchPlayerItem(lanternItemIndex);
                Debug.Log("[LanternPickUp] Lantern berhasil di-equip!");
            }
            else
            {
                Debug.LogWarning($"[LanternPickUp] Item di index {lanternItemIndex} adalah null!");
            }
        }
        else
        {
            Debug.LogWarning($"[LanternPickUp] Index {lanternItemIndex} melebihi jumlah PlayerItems ({itemsManager.PlayerItems.Count})!");
        }

        Destroy(gameObject);
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f; 
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
