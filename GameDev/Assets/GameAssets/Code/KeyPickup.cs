using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public string keyId = "Key";
    public float pickupRadius = 1.5f;
    public AudioClip pickupSound;

    Transform playerCam;
    Inventory inventory;

    void Start()
    {
        playerCam = Camera.main ? Camera.main.transform : null;
        if (!playerCam) { Debug.LogError("[KeyPickup] Camera.main tidak ditemukan."); enabled = false; return; }

        inventory = playerCam.GetComponentInParent<Inventory>();
        if (!inventory) { Debug.LogError("[KeyPickup] Inventory tidak ditemukan di parent Player."); enabled = false; return; }
    }

    void Update()
    {
        if (!playerCam || !inventory) return;

        if (HorizontalDistance(playerCam.position, transform.position) <= pickupRadius)
        {
            PickupKey();
        }
    }

    void PickupKey()
    {
        inventory.AddKey(keyId);

        if (pickupSound)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        Destroy(gameObject);
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
