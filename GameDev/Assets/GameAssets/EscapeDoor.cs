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

    // runtime
    Inventory inventory;
    bool done;
    Coroutine needKeyCo;

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

        if (winCanvas && winCanvas.activeSelf) winCanvas.SetActive(false);
        if (needKeyText) needKeyText.SetActive(false);
    }

    void Update()
    {
        if (done) return;

        // jarak HORIZONTAL dari kamera ke pintu
        if (HorizontalDistance(playerCamera.transform.position, transform.position) <= interactRadius
            && Input.GetKeyDown(interactKey))
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
        // Tidak ada auto-hide saat keluar radius — durasi diatur coroutine.
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

        // Freeze kontrol player (tanpa sentuh kamera / rigidbody / transform apa pun)
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
