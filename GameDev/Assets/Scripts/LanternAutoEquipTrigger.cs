using UnityEngine;

public class LanternAutoEquipTrigger : MonoBehaviour
{
    [Header("Referensi")]
    [Tooltip("GameObject Lantern di tangan player (child dari PlayerCamera)")]
    public GameObject lanternInHand;
    
    [Tooltip("LanternController component")]
    public LanternController lanternController;
    
    [Header("Visual Settings")]
    [Tooltip("Model lantern yang ada di dunia (akan dihilangkan setelah diambil)")]
    public GameObject lanternWorldModel;
    
    [Header("Audio")]
    public AudioClip pickupSound;
    
    [Header("Pengaturan")]
    [Tooltip("Apakah trigger hanya sekali atau bisa diambil berulang kali")]
    public bool oneTimePickup = true;
    
    [Tooltip("Langsung nyalakan lantern setelah diambil")]
    public bool autoTurnOnLight = true;
    
    [Tooltip("Berapa banyak oil yang didapat saat pickup")]
    public float oilAmount = 100f;
    
    private bool hasBeenPickedUp = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (lanternInHand != null)
        {
            lanternInHand.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasBeenPickedUp && oneTimePickup)
            return;
        
        if (other.CompareTag("Player"))
        {
            EquipLantern();
        }
    }

    void EquipLantern()
    {
        if (lanternInHand != null)
        {
            lanternInHand.SetActive(true);
        }
        
        if (lanternController != null)
        {
            lanternController.AddOil(oilAmount);
            
            if (autoTurnOnLight)
            {
                lanternController.enabled = true;
            }
        }
        
        if (lanternWorldModel != null)
        {
            lanternWorldModel.SetActive(false);
        }
        
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
        
        hasBeenPickedUp = true;
        
        Debug.Log("[LanternAutoEquipTrigger] Lantern equipped!");
    }
    
    public void ResetPickup()
    {
        hasBeenPickedUp = false;
        
        if (lanternWorldModel != null)
        {
            lanternWorldModel.SetActive(true);
        }
        
        if (lanternInHand != null)
        {
            lanternInHand.SetActive(false);
        }
    }
}
