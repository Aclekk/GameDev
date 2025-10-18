using System.Collections;
using UnityEngine;

public class JumpscareTrigger : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;                 // drag Player root
    public Camera playerCamera;              // drag kamera player
    public Animator enemyAnimator;           // drag Animator Hantu (punya Trigger "Jumpscare")

    [Tooltip("Script gerak musuh (mis. HantuMove). Akan dimatikan saat jumpscare.")]
    public MonoBehaviour enemyMovementToDisable;

    [Tooltip("Script kontrol player (mis. FirstPersonController/CharacterController). Akan dimatikan saat jumpscare.")]
    public MonoBehaviour playerControllerToDisable;

    [Header("Trigger & Timing")]
    public float triggerRadius = 2.2f;       // jarak memicu jumpscare
    public float cameraDistance = 0.65f;     // jarak kamera dari muka hantu (sepanjang forward hantu)
    public float cameraHeightOffset = 1.6f;  // tinggi kamera (sesuaikan tinggi kepala hantu)
    public float camMoveTime = 0.35f;        // durasi lerp kamera ke posisi jumpscare
    public float jumpscareDuration = 1.8f;   // lama anim sebelum kalah (atau samakan dgn clip)

    [Header("SFX (opsional)")]
    public AudioSource audioSource;          // pasang di Hantu
    public AudioClip jumpscareSfx;

    [Header("Game Over")]
    public GameObject loseCanvas;            // drag Canvas "U LOSE"
    public bool pauseTimeOnLose = true;      // freeze game saat kalah

    private bool _done;

    void Reset()
    {
        // bantu auto-isi saat Add Component
        playerCamera = Camera.main;
        enemyAnimator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (_done || player == null || playerCamera == null) return;

        float d = Vector3.Distance(transform.position, player.position);
        if (d <= triggerRadius)
        {
            StartCoroutine(DoJumpscare());
        }
    }

    IEnumerator DoJumpscare()
    {
        _done = true;

        // Matikan gerak musuh & kontrol player
        if (enemyMovementToDisable) enemyMovementToDisable.enabled = false;
        if (playerControllerToDisable) playerControllerToDisable.enabled = false;

        // Trigger anim
        if (enemyAnimator) enemyAnimator.SetTrigger("Jumpscare");

        // SFX
        if (audioSource && jumpscareSfx)
        {
            audioSource.spatialBlend = 1f; // 3D
            audioSource.dopplerLevel = 0f;
            audioSource.PlayOneShot(jumpscareSfx);
        }

        // Simpan state kamera
        Transform camT = playerCamera.transform;
        Vector3 startPos = camT.position;
        Quaternion startRot = camT.rotation;

        // Hitung target pos/rot (di depan hantu, menghadap hantu)
        Vector3 targetPos = transform.position + transform.forward * cameraDistance + Vector3.up * cameraHeightOffset;
        Vector3 lookTarget = transform.position + Vector3.up * cameraHeightOffset;
        Quaternion targetRot = Quaternion.LookRotation((lookTarget - targetPos).normalized, Vector3.up);

        // Lerp kamera → posisi jumpscare
        float t = 0f;
        while (t < camMoveTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / camMoveTime);
            camT.position = Vector3.Lerp(startPos, targetPos, k);
            camT.rotation = Quaternion.Slerp(startRot, targetRot, k);
            yield return null;
        }
        camT.position = targetPos;
        camT.rotation = targetRot;

        // Tunggu sampai anim selesai (atau pakai durasi manual)
        float wait = Mathf.Max(0.01f, jumpscareDuration);
        yield return new WaitForSeconds(wait);

        // Kalah: tampilkan UI
        if (loseCanvas) loseCanvas.SetActive(true);

        // Freeze game & kursor
        if (pauseTimeOnLose) Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);

        // Preview posisi kamera
        Vector3 pos = transform.position + transform.forward * cameraDistance + Vector3.up * cameraHeightOffset;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f);
        Gizmos.DrawSphere(pos, 0.05f);
    }
}
