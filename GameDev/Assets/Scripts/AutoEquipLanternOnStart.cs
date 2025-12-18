using UnityEngine;
using UHFPS.Runtime;
using System.Collections;

public class AutoEquipLanternOnStart : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Berapa detik delay sebelum lantern di-equip")]
    public float equipDelay = 2f;
    
    [Tooltip("Apakah lantern langsung menyala?")]
    public bool autoTurnOnLight = true;
    
    [Header("Info")]
    [SerializeField] private int lanternIndex = -1;

    private PlayerItemsManager itemsManager;
    private bool hasEquipped = false;

    void Start()
    {
        itemsManager = GetComponentInChildren<PlayerItemsManager>();
        
        if (itemsManager == null)
        {
            Debug.LogError("[AutoEquipLanternOnStart] PlayerItemsManager tidak ditemukan!");
            return;
        }

        FindLanternIndex();

        if (lanternIndex >= 0)
        {
            StartCoroutine(EquipLanternAfterDelay());
        }
        else
        {
            Debug.LogWarning("[AutoEquipLanternOnStart] LanternItem tidak ditemukan di PlayerItems list!");
        }
    }

    void FindLanternIndex()
    {
        for (int i = 0; i < itemsManager.PlayerItems.Count; i++)
        {
            if (itemsManager.PlayerItems[i] is LanternItem)
            {
                lanternIndex = i;
                Debug.Log($"[AutoEquipLanternOnStart] Lantern ditemukan di index {i}");
                break;
            }
        }
    }

    IEnumerator EquipLanternAfterDelay()
    {
        yield return new WaitForSeconds(equipDelay);

        if (!hasEquipped && itemsManager != null && lanternIndex >= 0)
        {
            EquipLantern();
        }
    }

    void EquipLantern()
    {
        try
        {
            itemsManager.SwitchPlayerItem(lanternIndex);
            hasEquipped = true;
            Debug.Log("[AutoEquipLanternOnStart] Lantern berhasil di-equip!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[AutoEquipLanternOnStart] Error saat equip lantern: " + e.Message);
        }
    }
}
