using UnityEngine;
using UHFPS.Runtime;
using UHFPS.Input;

public class LanternPickUp : MonoBehaviour, IInteractStart
{
    [Header("Pickup Settings")]
    public AudioClip pickupSound;
    [Range(0f, 1f)] public float pickupVolume = 0.8f;

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
            Debug.LogError("[LanternPickUp] PlayerItemsManager tidak ditemukan!");
            enabled = false;
            return;
        }
    }

    public void InteractStart()
    {
        if (hasBeenPickedUp) return;

        PickupLantern();
    }

    void PickupLantern()
    {
        hasBeenPickedUp = true;

        if (pickupSound)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
        }

        for (int i = 0; i < itemsManager.PlayerItems.Count; i++)
        {
            if (itemsManager.PlayerItems[i] is LanternItem)
            {
                itemsManager.SwitchPlayerItem(i);
                Debug.Log($"[LanternPickUp] Lantern berhasil di-equip di index {i}!");
                Destroy(gameObject);
                return;
            }
        }

        Debug.LogWarning("[LanternPickUp] LanternItem tidak ditemukan di PlayerItems!");
    }
}
