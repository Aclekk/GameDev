using UnityEngine;
using UHFPS.Runtime;

public class GameManagerAutoSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [Tooltip("Akan otomatis mencari dan assign referensi yang missing")]
    public bool autoFindReferences = true;

    void Awake()
    {
        if (autoFindReferences)
        {
            SetupPlayerPresenceManager();
            SetupGameManager();
        }
    }

    void SetupPlayerPresenceManager()
    {
        PlayerPresenceManager presenceManager = GetComponent<PlayerPresenceManager>();
        
        if (presenceManager == null)
        {
            Debug.LogWarning("[GameManagerAutoSetup] PlayerPresenceManager tidak ditemukan!");
            return;
        }

        if (presenceManager.Player == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            if (player != null)
            {
                typeof(PlayerPresenceManager)
                    .GetField("Player", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(presenceManager, player);
                
                Debug.Log("[GameManagerAutoSetup] Player reference di-assign ke: " + player.name);
            }
            else
            {
                Debug.LogError("[GameManagerAutoSetup] Player dengan tag 'Player' tidak ditemukan!");
            }
        }
    }

    void SetupGameManager()
    {
        GameManager gameManager = GetComponent<GameManager>();
        
        if (gameManager == null)
        {
            Debug.LogWarning("[GameManagerAutoSetup] GameManager tidak ditemukan!");
            return;
        }

        Debug.Log("[GameManagerAutoSetup] GameManager setup selesai!");
    }
}
