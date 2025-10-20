using System.Collections;
using UnityEngine;

public class EscapeDoor : MonoBehaviour
{
    [Header("Key & UI")]
    public string requiredKeyId = "BasementKey";
    public GameObject needKeyText;      
    public GameObject winCanvas;        

    [Header("Player Refs")]
    public Transform player;            
    public Camera playerCamera;         
    public MonoBehaviour playerControllerToDisable; 

    [Header("Interact")]
    public float interactRadius = 2.0f; 
    public KeyCode interactKey = KeyCode.E;
    public float winDelay = 0.8f;       

    [Header("Teleport Points")]
    public Transform insidePoint;       
    public Transform outsideCheckPoint; 
    public float checkDistance = 5f;    

    [Header("Teleport Settings")]
    public float fadeInDuration = 0.3f; 
    public AudioClip doorSound;         

    // runtime
    Inventory inventory;
    bool done;
    Coroutine needKeyCo;
    AudioSource audioSource;

    void Start()
    {
        if (!player && GameObject.Find("Player")) player = GameObject.Find("Player").transform;
        if (!playerCamera && Camera.main) playerCamera = Camera.main;

        if (!player || !playerCamera)
        {
            Debug.LogError("[EscapeDoor] Player/Camera belum di-assign.");
            enabled = false; return;
        }

        inventory = player.GetComponent<Inventory>();
        if (!inventory)
        {
            Debug.LogError("[EscapeDoor] Inventory tidak ditemukan di Player.");
            enabled = false; return;
        }

        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (!audioSource && doorSound)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (winCanvas && winCanvas.activeSelf) winCanvas.SetActive(false);
        if (needKeyText) needKeyText.SetActive(false);

        if (!insidePoint)
            Debug.LogWarning("[EscapeDoor] InsidePoint tidak di-set!");
        if (!outsideCheckPoint)
            Debug.LogWarning("[EscapeDoor] OutsideCheckPoint tidak di-set!");
    }

    void Update()
    {
        if (done) return;

        if (HorizontalDistance(playerCamera.transform.position, transform.position) <= interactRadius
            && Input.GetKeyDown(interactKey))
        {
            bool isPlayerOutside = IsPlayerOutside();

            if (isPlayerOutside)
            {
                StartCoroutine(EnterHouse());
            }
            else
            {
                if (!inventory.HasKey(requiredKeyId))
                {
                    if (needKeyCo != null) StopCoroutine(needKeyCo);
                    needKeyCo = StartCoroutine(ShowNeedKey());
                }
                else
                {
                    StartCoroutine(WinSequence_FreezeOnly());
                }
            }
        }
    }

    bool IsPlayerOutside()
    {
        if (!outsideCheckPoint)
        {
            Vector3 toPlayer = (player.position - transform.position).normalized;
            return Vector3.Dot(transform.forward, toPlayer) > 0;
        }

        float distanceToOutside = Vector3.Distance(player.position, outsideCheckPoint.position);
        return distanceToOutside <= checkDistance;
    }

    IEnumerator EnterHouse()
    {
        if (playerControllerToDisable) playerControllerToDisable.enabled = false;

        if (audioSource && doorSound)
            audioSource.PlayOneShot(doorSound);

        yield return new WaitForSeconds(0.2f);

        if (insidePoint)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc)
            {
                cc.enabled = false;
                player.position = insidePoint.position;
                player.rotation = insidePoint.rotation;
                yield return null;
                cc.enabled = true;
            }
            else
            {
                player.position = insidePoint.position;
                player.rotation = insidePoint.rotation;
            }

            Debug.Log("[EscapeDoor] Player teleported inside the house.");
        }

        yield return new WaitForSeconds(fadeInDuration);

        if (playerControllerToDisable) playerControllerToDisable.enabled = true;
    }

    IEnumerator ShowNeedKey()
    {
        if (!needKeyText) { Debug.Log("Need a key"); yield break; }
        needKeyText.SetActive(true);
        yield return new WaitForSeconds(1.2f);
        needKeyText.SetActive(false);
        needKeyCo = null;
    }

    IEnumerator WinSequence_FreezeOnly()
    {
        done = true;

        // ✅ Matikan kontrol player
        if (playerControllerToDisable)
            playerControllerToDisable.enabled = false;

        // ✅ Matikan semua hantu di scene (script: HantuMove)
        StopAllEnemies();

        // ✅ Mainkan suara pintu / escape
        if (audioSource && doorSound)
            audioSource.PlayOneShot(doorSound);

        yield return new WaitForSeconds(winDelay);

        // ✅ Nonaktifkan physics biar player gak bisa gerak
        CharacterController cc = player.GetComponent<CharacterController>();
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (cc) cc.enabled = false;
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // ✅ Tampilkan UI menang
        if (winCanvas)
        {
            winCanvas.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Debug.Log("[EscapeDoor] Player escaped successfully. Semua hantu dimatikan.");
    }

    void StopAllEnemies()
    {
        // Cari semua script HantuMove dan matikan
        HantuMove[] semuaHantu = FindObjectsOfType<HantuMove>();

        foreach (HantuMove h in semuaHantu)
        {
            h.enabled = false;

            Animator anim = h.GetComponent<Animator>();
            if (anim) anim.enabled = false;

            AudioSource aud = h.GetComponent<AudioSource>();
            if (aud) aud.Stop();
        }

        Debug.Log($"[EscapeDoor] {semuaHantu.Length} hantu (HantuMove) telah dimatikan.");
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void OnDisable()
    {
        if (needKeyText) needKeyText.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);

        if (insidePoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(insidePoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, insidePoint.position);
        }

        if (outsideCheckPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(outsideCheckPoint.position, checkDistance);
            Gizmos.DrawLine(transform.position, outsideCheckPoint.position);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward * 3f);
        }
    }
}
