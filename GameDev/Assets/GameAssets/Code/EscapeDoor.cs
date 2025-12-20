using System.Collections;
using UnityEngine;
using TMPro;
using UHFPS.Runtime;

public class EscapeDoor : MonoBehaviour
{
    [Header("Key & UI")]
    public string mainKeyId = "MainKey";
    public string escapeKeyId = "EscapeKey";
    public GameObject needKeyText;      
    public GameObject pressEText;
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
    public bool flipInsideRotation180 = false;    

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

        SetPrompt(pressEText, false, "Press \"E\" to Open");
        SetPrompt(needKeyText, false, "Need a key");
        if (winCanvas && winCanvas.activeSelf) winCanvas.SetActive(false);

        if (!insidePoint)
            Debug.LogWarning("[EscapeDoor] InsidePoint tidak di-set!");
        if (!outsideCheckPoint)
            Debug.LogWarning("[EscapeDoor] OutsideCheckPoint tidak di-set!");
    }

    void Update()
    {
        if (done) return;

        bool inRange = HorizontalDistance(playerCamera.transform.position, transform.position) <= interactRadius;
        SetPrompt(pressEText, inRange, "Press \"E\" to Open");

        if (inRange && Input.GetKeyDown(interactKey))
        {
            bool isPlayerOutside = IsPlayerOutside();

            if (isPlayerOutside)
            {
                if (!inventory.HasKey(mainKeyId))
                {
                    if (needKeyCo != null) StopCoroutine(needKeyCo);
                    needKeyCo = StartCoroutine(ShowNeedKey());
                }
                else
                {
                    StartCoroutine(EnterHouse());
                }
            }
            else
            {
                if (!inventory.HasKey(escapeKeyId))
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
            TeleportPlayer(insidePoint, flipInsideRotation180);
            Debug.Log("[EscapeDoor] Player teleported inside the house.");
        }

        yield return new WaitForSeconds(fadeInDuration);

        if (playerControllerToDisable) playerControllerToDisable.enabled = true;
    }

    void TeleportPlayer(Transform target, bool flip180 = false)
    {
        if (!target) return;

        LookController lookController = player.GetComponentInChildren<LookController>();
        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc)
        {
            cc.enabled = false;
        }

        player.position = target.position;
        player.rotation = Quaternion.identity;

        if (cc)
        {
            cc.enabled = true;
        }

        if (lookController != null)
        {
            float targetRotation = target.eulerAngles.y;
            if (flip180)
            {
                targetRotation += 180f;
                if (targetRotation >= 360f) targetRotation -= 360f;
            }
            
            lookController.LookRotation = new Vector2(targetRotation, 0f);
        }
    }

    IEnumerator ShowNeedKey()
    {
        SetPrompt(needKeyText, true, "Need a key");
        yield return new WaitForSecondsRealtime(1.2f);
        SetPrompt(needKeyText, false, "Need a key");
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
        SetPrompt(pressEText, false, "Press \"E\" to Open");
        SetPrompt(needKeyText, false, "Need a key");
    }

    void SetPrompt(GameObject root, bool visible, string message)
    {
        if (!root) return;

        // cari TMP + CanvasGroup di dalam (punya kamu ada di child)
        var tmp = root.GetComponentInChildren<TMP_Text>(true);
        var cg  = root.GetComponentInChildren<CanvasGroup>(true);

        if (tmp) tmp.text = message;

        // kalau ada CanvasGroup: kontrol alpha (lebih aman daripada SetActive doang)
        if (cg)
        {
            if (!root.activeSelf) root.SetActive(true);
            cg.alpha = visible ? 1f : 0f;
            cg.interactable = visible;
            cg.blocksRaycasts = visible;
        }
        else
        {
            root.SetActive(visible);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        DrawCircleGizmo(transform.position, interactRadius);

        if (insidePoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(insidePoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, insidePoint.position);
        }

        if (outsideCheckPoint)
        {
            Gizmos.color = Color.red;
            DrawCircleGizmo(outsideCheckPoint.position, checkDistance);
            Gizmos.DrawLine(transform.position, outsideCheckPoint.position);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward * 3f);
        }
    }

    void DrawCircleGizmo(Vector3 center, float radius)
    {
        const int segments = 36;
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
