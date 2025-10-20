using System.Collections;
using UnityEngine;

public class EscapeDoor : MonoBehaviour
{
    [Header("Key & UI")]
    public string requiredKeyId = "BasementKey";
    public GameObject needKeyText;      // UI "Need a key" (inactive di awal)
    public GameObject winCanvas;        // WinCanvas (inactive di awal)

    [Header("Player Refs")]
    public Transform player;            // drag Player (root)
    public Camera playerCamera;         // drag PlayerCamera (dipakai untuk jarak)
    public MonoBehaviour playerControllerToDisable; // drag controller player (FPC/HeroController)

    [Header("Interact")]
    public float interactRadius = 2.0f; // radius tekan E (horizontal)
    public KeyCode interactKey = KeyCode.E;
    public float winDelay = 0.8f;       // jeda sebelum WinCanvas

    [Header("Teleport Points")]
    public Transform insidePoint;       // Titik spawn di DALAM rumah (drag empty GameObject)
    public Transform outsideCheckPoint; // Titik referensi LUAR rumah (untuk deteksi posisi)
    public float checkDistance = 5f;    // Jarak dari outsideCheckPoint untuk deteksi "di luar"

    [Header("Teleport Settings")]
    public float fadeInDuration = 0.3f; // Durasi fade saat teleport (optional)
    public AudioClip doorSound;         // Sound effect pintu (optional)

    // runtime
    Inventory inventory;
    bool done;
    Coroutine needKeyCo;
    AudioSource audioSource;

    void Start()
    {
        if (!player && GameObject.Find("Player")) player = GameObject.Find("Player").transform;
        if (!playerCamera && Camera.main)         playerCamera = Camera.main;

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

        // Setup audio source untuk sound effect
        audioSource = GetComponent<AudioSource>();
        if (!audioSource && doorSound)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (winCanvas && winCanvas.activeSelf) winCanvas.SetActive(false);
        if (needKeyText) needKeyText.SetActive(false);

        // Validasi teleport points
        if (!insidePoint)
            Debug.LogWarning("[EscapeDoor] InsidePoint tidak di-set! Teleport tidak akan berfungsi.");
        if (!outsideCheckPoint)
            Debug.LogWarning("[EscapeDoor] OutsideCheckPoint tidak di-set! Deteksi posisi tidak akurat.");
    }

    void Update()
    {
        if (done) return;

        // jarak HORIZONTAL dari kamera ke pintu
        if (HorizontalDistance(playerCamera.transform.position, transform.position) <= interactRadius
            && Input.GetKeyDown(interactKey))
        {
            // Deteksi apakah player di luar atau dalam rumah
            bool isPlayerOutside = IsPlayerOutside();

            if (isPlayerOutside)
            {
                // Player di luar → Masuk tanpa kunci
                StartCoroutine(EnterHouse());
            }
            else
            {
                // Player di dalam → Butuh kunci untuk keluar
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

    // Deteksi apakah player berada di luar rumah
    bool IsPlayerOutside()
    {
        if (!outsideCheckPoint)
        {
            // Fallback: cek berdasarkan transform pintu (asumsi pintu menghadap ke luar)
            // Player di luar jika berada di sisi depan pintu (dot product > 0)
            Vector3 toPlayer = (player.position - transform.position).normalized;
            return Vector3.Dot(transform.forward, toPlayer) > 0;
        }

        // Cek jarak dari outsideCheckPoint
        float distanceToOutside = Vector3.Distance(player.position, outsideCheckPoint.position);
        return distanceToOutside <= checkDistance;
    }

    IEnumerator EnterHouse()
    {
        // Disable player control sementara
        if (playerControllerToDisable) playerControllerToDisable.enabled = false;

        // Play sound effect
        if (audioSource && doorSound)
            audioSource.PlayOneShot(doorSound);

        // Optional: Fade out effect
        yield return new WaitForSeconds(0.2f);

        // Teleport player ke dalam rumah
        if (insidePoint)
        {
            // Disable CharacterController jika ada (supaya bisa teleport)
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc)
            {
                cc.enabled = false;
                player.position = insidePoint.position;
                player.rotation = insidePoint.rotation;
                yield return null; // Tunggu 1 frame
                cc.enabled = true;
            }
            else
            {
                player.position = insidePoint.position;
                player.rotation = insidePoint.rotation;
            }

            Debug.Log("[EscapeDoor] Player teleported inside the house.");
        }

        // Optional: Fade in effect
        yield return new WaitForSeconds(fadeInDuration);

        // Re-enable player control
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

        // Freeze kontrol player
        if (playerControllerToDisable) playerControllerToDisable.enabled = false;

        yield return new WaitForSeconds(winDelay);

        if (winCanvas)
        {
            winCanvas.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
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
        // Gizmos untuk radius interaksi pintu
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);

        // Gizmos untuk inside point
        if (insidePoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(insidePoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, insidePoint.position);
            
            // Arrow untuk rotasi
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(insidePoint.position, insidePoint.forward * 2f);
        }

        // Gizmos untuk outside check point
        if (outsideCheckPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(outsideCheckPoint.position, checkDistance);
            Gizmos.DrawLine(transform.position, outsideCheckPoint.position);
        }
        else
        {
            // Fallback: show door forward direction
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward * 3f);
        }
    }
}