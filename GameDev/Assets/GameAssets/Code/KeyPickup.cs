using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public string keyId = "Key";
    public float pickupRadius = 1.5f;
    public AudioClip pickupSound;

    Transform playerBody;
    Inventory inventory;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (!playerObj)
        {
            Debug.LogError("[KeyPickup] Player dengan tag 'Player' tidak ditemukan!");
            enabled = false;
            return;
        }

        inventory = playerObj.GetComponent<Inventory>();
        if (!inventory) 
        { 
            Debug.LogError($"[KeyPickup] Inventory tidak ditemukan di {playerObj.name}!"); 
            enabled = false; 
            return;
        }

        playerBody = playerObj.transform;
    }

    void Update()
    {
        if (!playerBody || !inventory) return;

        float distance = HorizontalDistance(playerBody.position, transform.position);
        
        if (distance <= pickupRadius)
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
