using UnityEngine;
using UHFPS.Runtime;
using System.Reflection;

public class PetrolOilPickup : MonoBehaviour, IInteractStart
{
    [Header("Pickup Settings")]
    public AudioClip pickupSound;
    [Range(0f, 1f)] public float pickupVolume = 0.8f;

    [Header("Fuel Settings")]
    [Tooltip("Apakah mengisi fuel ke 100% atau menambah sesuai FuelPercentage lantern?")]
    public bool fillToMax = true;

    PlayerItemsManager itemsManager;
    bool hasBeenPickedUp = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (!playerObj)
        {
            Debug.LogError("[PetrolOilPickup] Player dengan tag 'Player' tidak ditemukan!");
            enabled = false;
            return;
        }

        itemsManager = playerObj.GetComponentInChildren<PlayerItemsManager>();
        if (!itemsManager)
        {
            Debug.LogError("[PetrolOilPickup] PlayerItemsManager tidak ditemukan!");
            enabled = false;
            return;
        }
    }

    public void InteractStart()
    {
        if (hasBeenPickedUp) return;

        PickupPetrolOil();
    }

    void PickupPetrolOil()
    {
        hasBeenPickedUp = true;

        if (pickupSound)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
        }

        LanternItem lanternItem = FindLanternItem();
        if (lanternItem != null)
        {
            RefillLanternFuel(lanternItem);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("[PetrolOilPickup] LanternItem tidak ditemukan di PlayerItems!");
        }
    }

    LanternItem FindLanternItem()
    {
        for (int i = 0; i < itemsManager.PlayerItems.Count; i++)
        {
            if (itemsManager.PlayerItems[i] is LanternItem lantern)
            {
                return lantern;
            }
        }
        return null;
    }

    void RefillLanternFuel(LanternItem lanternItem)
    {
        FieldInfo currentFuelField = typeof(LanternItem).GetField("currentFuel", BindingFlags.NonPublic | BindingFlags.Instance);
        MethodInfo updateFuelMethod = typeof(LanternItem).GetMethod("UpdateFuel", BindingFlags.NonPublic | BindingFlags.Instance);

        if (currentFuelField != null && updateFuelMethod != null)
        {
            float newFuel;

            if (fillToMax)
            {
                newFuel = lanternItem.FuelLife;
            }
            else
            {
                float currentFuel = (float)currentFuelField.GetValue(lanternItem);
                float addFuel = lanternItem.FuelPercentage.From(lanternItem.FuelLife);
                newFuel = Mathf.Min(currentFuel + addFuel, lanternItem.FuelLife);
            }

            currentFuelField.SetValue(lanternItem, newFuel);
            updateFuelMethod.Invoke(lanternItem, null);

            float fuelPercent = (newFuel / lanternItem.FuelLife) * 100f;
            Debug.Log($"[PetrolOilPickup] Lantern fuel diisi! Fuel sekarang: {fuelPercent:F0}%");
        }
        else
        {
            Debug.LogError("[PetrolOilPickup] Tidak bisa mengakses currentFuel atau UpdateFuel method!");
        }
    }
}
