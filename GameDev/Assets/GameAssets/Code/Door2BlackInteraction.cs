using System.Collections;
using UnityEngine;
using TMPro;

public class Door2BlackInteraction : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pressECanvas;          // Canvas "Press E" yang akan muncul
    
    [Header("Interaction Settings")]
    public float interactRadius = 2.0f;       // Jarak maksimal untuk interaksi
    public KeyCode interactKey = KeyCode.E;   // Tombol untuk interaksi
    
    [Header("Player References")]
    public Transform player;                  // Transform player
    public Camera playerCamera;               // Camera player
    
    // Runtime variables
    bool isInRange = false;
    
    void Start()
    {
        // Auto-assign references jika belum di-set
        if (!player && GameObject.Find("Player")) 
            player = GameObject.Find("Player").transform;
        if (!playerCamera) 
            playerCamera = Camera.main;
            
        // Validasi references
        if (!player || !playerCamera)
        {
            Debug.LogError("[Door2BlackInteraction] Player atau Camera belum di-assign!");
            enabled = false;
            return;
        }
        
        // Sembunyikan UI di awal
        SetUIVisibility(false);
    }
    
    void Update()
    {
        // Cek jarak player ke pintu
        bool wasInRange = isInRange;
        isInRange = IsPlayerInRange();
        
        // Update UI visibility berdasarkan jarak
        if (wasInRange != isInRange)
        {
            SetUIVisibility(isInRange);
        }
        
        // Handle input interaksi (jika diperlukan untuk fungsi lain)
        if (isInRange && Input.GetKeyDown(interactKey))
        {
            // Di sini bisa ditambahkan logika interaksi lain
            // Saat ini hanya fokus ke UI display
            Debug.Log("[Door2BlackInteraction] Player menekan E di dekat pintu");
        }
    }
    
    bool IsPlayerInRange()
    {
        if (!playerCamera) return false;
        
        // Hitung jarak horizontal (abaikan axis Y)
        Vector3 playerPos = playerCamera.transform.position;
        Vector3 doorPos = transform.position;
        
        return HorizontalDistance(playerPos, doorPos) <= interactRadius;
    }
    
    void SetUIVisibility(bool visible)
    {
        if (!pressECanvas) return;
        
        // Cari CanvasGroup untuk smooth alpha transition
        CanvasGroup canvasGroup = pressECanvas.GetComponent<CanvasGroup>();
        
        if (canvasGroup)
        {
            // Pastikan canvas aktif
            if (!pressECanvas.activeSelf) 
                pressECanvas.SetActive(true);
                
            // Set alpha dan interactivity
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
        else
        {
            // Fallback: gunakan SetActive
            pressECanvas.SetActive(visible);
        }
    }
    
    // Helper function untuk menghitung jarak horizontal
    float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
    
    void OnDisable()
    {
        // Sembunyikan UI saat script disabled
        SetUIVisibility(false);
    }
    
    void OnDrawGizmosSelected()
    {
        // Visualisasi radius interaksi di Scene view
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
