using UnityEngine;
using UHFPS.Runtime;
using System.Collections;
using System.Reflection;

public class LanternAutoPickup : MonoBehaviour
{
    public float pickupRadius = 2.0f;
    public AudioClip pickupSound;
    public bool autoEquipOnPickup = true;
    public bool ensureLanternHasFuel = true;
    
    Transform playerBody;
    PlayerItemsManager itemsManager;
    int lanternIndex = -1;
    bool hasPickedUp = false;

    void Start()
    {
        Debug.Log("[LanternAutoPickup] ✅ Script started!");
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (!playerObj)
        {
            Debug.LogError("[LanternAutoPickup] ❌ Player dengan tag 'Player' tidak ditemukan!");
            enabled = false;
            return;
        }
        
        Debug.Log($"[LanternAutoPickup] ✅ Player found: {playerObj.name}");

        itemsManager = playerObj.GetComponentInChildren<PlayerItemsManager>();
        if (!itemsManager) 
        { 
            Debug.LogError("[LanternAutoPickup] ❌ PlayerItemsManager tidak ditemukan di Player!"); 
            enabled = false; 
            return;
        }
        
        Debug.Log($"[LanternAutoPickup] ✅ PlayerItemsManager found with {itemsManager.PlayerItems.Count} items");

        playerBody = playerObj.transform;
        FindLanternIndex();
        
        Debug.Log($"[LanternAutoPickup] ✅ Setup complete! Pickup radius: {pickupRadius}m");
    }

    void FindLanternIndex()
    {
        Debug.Log($"[LanternAutoPickup] 🔍 Mencari LanternItem di {itemsManager.PlayerItems.Count} items...");
        
        for (int i = 0; i < itemsManager.PlayerItems.Count; i++)
        {
            var item = itemsManager.PlayerItems[i];
            Debug.Log($"[LanternAutoPickup] Item[{i}]: {item?.Name ?? "NULL"} (Type: {item?.GetType().Name ?? "NULL"})");
            
            if (item is LanternItem)
            {
                lanternIndex = i;
                Debug.Log($"[LanternAutoPickup] ✅ Lantern ditemukan di index {i}");
                break;
            }
        }

        if (lanternIndex < 0)
        {
            Debug.LogError("[LanternAutoPickup] ❌ LanternItem tidak ditemukan di PlayerItems list!");
        }
    }

    void Update()
    {
        if (!playerBody || !itemsManager || hasPickedUp) return;
        if (lanternIndex < 0) return;

        float distance = HorizontalDistance(playerBody.position, transform.position);
        
        if (distance <= pickupRadius)
        {
            Debug.Log($"[LanternAutoPickup] 🎯 Player in range! Distance: {distance:F2}m");
            PickupLantern();
        }
    }

    void PickupLantern()
    {
        hasPickedUp = true;

        if (pickupSound)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        if (autoEquipOnPickup && lanternIndex >= 0)
        {
            if (ensureLanternHasFuel)
            {
                StartCoroutine(EquipAndSetupLantern());
            }
            else
            {
                itemsManager.SwitchPlayerItem(lanternIndex);
                Debug.Log("[LanternAutoPickup] Lantern berhasil di-pickup dan di-equip!");
                enabled = false;
            }
        }
        else
        {
            enabled = false;
        }
    }

    IEnumerator EquipAndSetupLantern()
    {
        HidePickupObject();
        
        itemsManager.SwitchPlayerItem(lanternIndex);
        yield return new WaitForSeconds(0.5f);

        LanternItem lanternItem = itemsManager.PlayerItems[lanternIndex] as LanternItem;
        if (lanternItem != null)
        {
            SetLanternFuel(lanternItem);
            EnsureFlameVisible(lanternItem);
            ForceUpdateFlame(lanternItem);
            
            FieldInfo isLanternOnField = typeof(LanternItem).GetField("isLanternOn", BindingFlags.NonPublic | BindingFlags.Instance);
            if (isLanternOnField != null)
            {
                isLanternOnField.SetValue(lanternItem, true);
                Debug.Log("[LanternAutoPickup] ✅ isLanternOn di-set ke true");
            }
            
            Debug.Log("[LanternAutoPickup] ✅ Lantern berhasil di-pickup, di-equip, fuel dan flame di-set!");
        }
        
        enabled = false;
    }

    void HidePickupObject()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.enabled = false;
        }
        
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
        
        Debug.Log("[LanternAutoPickup] ✅ Visual dan collider pickup di-nonaktifkan");
    }

    void SetLanternFuel(LanternItem lanternItem)
    {
        FieldInfo currentFuelField = typeof(LanternItem).GetField("currentFuel", BindingFlags.NonPublic | BindingFlags.Instance);
        if (currentFuelField != null)
        {
            float fuelLife = lanternItem.FuelLife;
            float fuelPercentage = lanternItem.FuelPercentage.Value / 100f;
            float fuel = fuelLife * fuelPercentage;
            
            currentFuelField.SetValue(lanternItem, fuel);
            
            MethodInfo updateFuelMethod = typeof(LanternItem).GetMethod("UpdateFuel", BindingFlags.NonPublic | BindingFlags.Instance);
            if (updateFuelMethod != null)
            {
                updateFuelMethod.Invoke(lanternItem, null);
            }
            
            Debug.Log($"[LanternAutoPickup] Lantern fuel di-set ke {fuel} (FuelLife: {fuelLife}, Percentage: {fuelPercentage * 100}%)");
        }
    }

    void EnsureFlameVisible(LanternItem lanternItem)
    {
        if (lanternItem.LanternFlame != null)
        {
            MeshRenderer flameRenderer = lanternItem.LanternFlame;
            GameObject flameObject = flameRenderer.gameObject;
            
            Transform currentParent = flameObject.transform;
            while (currentParent != null)
            {
                if (!currentParent.gameObject.activeSelf)
                {
                    Debug.LogWarning($"[LanternAutoPickup] Parent INACTIVE ditemukan: {currentParent.name} - MENGAKTIFKAN...");
                    currentParent.gameObject.SetActive(true);
                }
                currentParent = currentParent.parent;
            }
            
            flameObject.SetActive(true);
            flameRenderer.enabled = true;
            
            Material flameMat = flameRenderer.material;
            if (flameMat != null)
            {
                if (flameMat.HasProperty("_Fade"))
                {
                    flameMat.SetFloat("_Fade", 1f);
                    Debug.Log("[LanternAutoPickup] ✅ Flame material _Fade di-set ke 1");
                }
                
                if (flameMat.HasProperty("_BaseColor"))
                {
                    flameMat.SetColor("_BaseColor", Color.white);
                }
                
                if (flameMat.HasProperty("_Color"))
                {
                    flameMat.SetColor("_Color", Color.white);
                }
            }
        }

        if (lanternItem.LanternLight != null)
        {
            lanternItem.LanternLight.enabled = true;
            Debug.Log("[LanternAutoPickup] ✅ Lantern light diaktifkan");
        }
    }
    
    void ForceUpdateFlame(LanternItem lanternItem)
    {
        FieldInfo lanternFuelField = typeof(LanternItem).GetField("lanternFuel", BindingFlags.Public | BindingFlags.Instance);
        if (lanternFuelField != null)
        {
            lanternFuelField.SetValue(lanternItem, 1f);
            Debug.Log("[LanternAutoPickup] 🔥 FORCE lanternFuel ke 1.0!");
        }
        
        FieldInfo flameLerpField = typeof(LanternItem).GetField("flameLerp", BindingFlags.NonPublic | BindingFlags.Instance);
        if (flameLerpField != null)
        {
            flameLerpField.SetValue(lanternItem, 1f);
            Debug.Log("[LanternAutoPickup] ✅ flameLerp di-set ke 1");
        }
        
        FieldInfo targetFlameField = typeof(LanternItem).GetField("targetFlame", BindingFlags.NonPublic | BindingFlags.Instance);
        if (targetFlameField != null)
        {
            targetFlameField.SetValue(lanternItem, 1f);
            Debug.Log("[LanternAutoPickup] ✅ targetFlame di-set ke 1");
        }
        
        FieldInfo flameIntensityField = typeof(LanternItem).GetField("flameIntensity", BindingFlags.NonPublic | BindingFlags.Instance);
        if (flameIntensityField != null)
        {
            flameIntensityField.SetValue(lanternItem, lanternItem.FlameLightIntensity);
            Debug.Log($"[LanternAutoPickup] ✅ flameIntensity di-set ke {lanternItem.FlameLightIntensity}");
        }
        
        FieldInfo lanternFlameCanvasField = typeof(LanternItem).GetField("lanternFlame", BindingFlags.NonPublic | BindingFlags.Instance);
        if (lanternFlameCanvasField != null)
        {
            UnityEngine.CanvasGroup flameCanvas = lanternFlameCanvasField.GetValue(lanternItem) as UnityEngine.CanvasGroup;
            if (flameCanvas != null)
            {
                flameCanvas.alpha = 1f;
                Debug.Log("[LanternAutoPickup] ✅ lanternFlame CanvasGroup alpha di-set ke 1");
            }
        }
        
        MethodInfo updateFuelMethod = typeof(LanternItem).GetMethod("UpdateFuel", BindingFlags.NonPublic | BindingFlags.Instance);
        if (updateFuelMethod != null)
        {
            updateFuelMethod.Invoke(lanternItem, null);
            Debug.Log("[LanternAutoPickup] ✅ UpdateFuel() dipanggil");
        }
        
        if (lanternItem.LanternFlame != null)
        {
            Material mat = lanternItem.LanternFlame.sharedMaterial;
            if (mat != null)
            {
                float fadeValue = mat.GetFloat("_Fade");
                Debug.Log($"[LanternAutoPickup] Material _Fade: {fadeValue}, Shader: {mat.shader.name}");
                
                if (fadeValue < 0.9f)
                {
                    mat.SetFloat("_Fade", 1f);
                    Debug.Log("[LanternAutoPickup] ⚠️ FORCE _Fade ke 1!");
                }
            }
        }
        
        if (lanternItem.LanternLight != null)
        {
            lanternItem.LanternLight.intensity = lanternItem.FlameLightIntensity;
            Debug.Log($"[LanternAutoPickup] ✅ Light intensity di-set ke {lanternItem.FlameLightIntensity}");
        }
        
        Debug.Log($"[LanternAutoPickup] 🔥 FINAL CHECK - lanternFuel: {lanternItem.lanternFuel}, Light: {lanternItem.LanternLight.intensity}");
        Debug.Log("[LanternAutoPickup] 🔥 Force update flame SELESAI! API HARUS NYALA SEKARANG!");
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
