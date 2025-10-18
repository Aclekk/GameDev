using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HantuJumpscare : MonoBehaviour
{
    [Header("Refs (auto-isi kalau kosong)")]
    public Transform player;                       // root "Player"
    public Camera playerCamera;                    // child "PlayerCamera"
    public Animator enemyAnimator;                 // Trigger "Jumpscare"
    public MonoBehaviour enemyMovementToDisable;   // mis. HantuMove
    public MonoBehaviour playerControllerToDisable;// mis. FirstPersonController/HeroController
    public MonoBehaviour[] extraPlayerScriptsToDisable; // opsional: sway/headbob dsb.

    [Header("Trigger & Kamera")]
    public float triggerRadius   = 2.2f;   // jarak memicu jumpscare
    public float camApproach     = 0.65f;  // jarak kamera di DEPAN hantu
    public float camHeight       = 1.6f;   // tinggi kepala hantu
    public bool  instantSnap     = true;   // true = langsung snap, false = lerp cepat
    public float camLerpTime     = 0.12f;  // dipakai kalau instantSnap=false
    public float jumpscareDuration = 1.8f; // tunggu sebelum game over

    [Header("SFX (opsional)")]
    public AudioSource audioSource;
    public AudioClip jumpscareSfx;

    [Header("UI Kalah")]
    public GameObject loseCanvas;          // Canvas "LoseCanvas" (awal: inactive)
    public bool pauseTimeOnLose = true;
    public bool fadeLoseUI = true;
    public float fadeTime = 0.35f;
    public string mainMenuSceneName = "MainMenu";

    bool done;

    // cache utk unfreeze jika perlu (sini tidak dipakai lagi karena game over)
    Rigidbody playerRb;
    CharacterController playerCc;
    Transform camOriginalParent;

    void Awake()
    {
        if (player == null) { var p = GameObject.Find("Player"); if (p) player = p.transform; }
        if (playerCamera == null)
        {
            var pc = player ? player.GetComponentInChildren<Camera>(true) : Camera.main;
            if (pc) playerCamera = pc;
        }
        if (enemyAnimator == null) enemyAnimator = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (enemyMovementToDisable == null)
        {
            var hm = GetComponent("HantuMove") as MonoBehaviour;
            if (hm) enemyMovementToDisable = hm;
        }
        if (playerControllerToDisable == null && player != null)
        {
            var fpc = player.GetComponent("FirstPersonController") as MonoBehaviour;
            var hero = player.GetComponent("HeroController") as MonoBehaviour;
            if (fpc) playerControllerToDisable = fpc;
            else if (hero) playerControllerToDisable = hero;
        }

        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody>();
            playerCc = player.GetComponent<CharacterController>();
        }

        if (loseCanvas && loseCanvas.activeSelf) loseCanvas.SetActive(false);
    }

    void Update()
    {
        if (done || player == null || playerCamera == null) return;

        // Jarak simple – kalau mau super akurat bisa ganti ke SphereCollider isTrigger
        if (Vector3.Distance(transform.position, player.position) <= triggerRadius)
            StartCoroutine(DoJumpscare());
    }

    IEnumerator DoJumpscare()
    {
        done = true;

        // 1) HENTIKAN semua gerak lebih dulu (biar nggak “kelewatan”)
        if (enemyMovementToDisable) enemyMovementToDisable.enabled = false;

        if (playerControllerToDisable) playerControllerToDisable.enabled = false;
        if (extraPlayerScriptsToDisable != null)
            foreach (var s in extraPlayerScriptsToDisable) if (s) s.enabled = false;

        // freeze fisika player
        if (playerRb)
        {
            playerRb.velocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.isKinematic = true; // stop total
        }
        if (playerCc) playerCc.enabled = false; // stop CharacterController step berikutnya

        // 2) ANIM + SFX
        if (enemyAnimator) enemyAnimator.SetTrigger("Jumpscare");
        if (audioSource && jumpscareSfx)
        {
            audioSource.spatialBlend = 1f;
            audioSource.dopplerLevel = 0f;
            audioSource.PlayOneShot(jumpscareSfx);
        }

        // 3) KAMERA: SNAP / LERP ke DEPAN hantu lalu LOCK
        var camT = playerCamera.transform;
        camOriginalParent = camT.parent; // simpan
        Vector3 head = transform.position + Vector3.up * camHeight;
        Vector3 targetPos = transform.position + transform.forward * camApproach + Vector3.up * camHeight;
        Quaternion targetRot = Quaternion.LookRotation((head - targetPos).normalized, Vector3.up);

        if (instantSnap || camLerpTime <= 0.01f)
        {
            camT.position = targetPos;
            camT.rotation = targetRot;
        }
        else
        {
            Vector3 startPos = camT.position;
            Quaternion startRot = camT.rotation;
            float t = 0f;
            while (t < camLerpTime)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / camLerpTime);
                camT.position = Vector3.Lerp(startPos, targetPos, k);
                camT.rotation = Quaternion.Slerp(startRot, targetRot, k);
                yield return null;
            }
            camT.position = targetPos;
            camT.rotation = targetRot;
        }

        // Parent-kan kamera ke hantu supaya 100% “nempel” & menghadap dia
        camT.SetParent(transform, true);

        // 4) Tunggu durasi jumpscare
        yield return new WaitForSeconds(jumpscareDuration);

        // 5) GAME OVER
        ShowLoseUI();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ShowLoseUI()
    {
        if (!loseCanvas)
        {
            Debug.LogWarning("LoseCanvas belum di-assign.");
            return;
        }

        loseCanvas.SetActive(true);
        if (pauseTimeOnLose) Time.timeScale = 0f;

        var cg = loseCanvas.GetComponent<CanvasGroup>();
        if (!cg) cg = loseCanvas.AddComponent<CanvasGroup>();
        cg.alpha = fadeLoseUI ? 0f : 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        if (fadeLoseUI) StartCoroutine(FadeInCanvasGroup(cg, fadeTime));
    }

    IEnumerator FadeInCanvasGroup(CanvasGroup cg, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(t / dur);
            yield return null;
        }
        cg.alpha = 1f;
    }

    // === Tombol UI ===
    public void UI_Retry()
    {
        Time.timeScale = 1f;
        Scene cur = SceneManager.GetActiveScene();
        SceneManager.LoadScene(cur.buildIndex);
    }

    public void UI_MainMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            Debug.LogWarning("Nama scene MainMenu kosong.");
    }

    public void UI_Quit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
        Vector3 pos = transform.position + transform.forward * camApproach + Vector3.up * camHeight;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawSphere(pos, 0.06f);
    }
}
