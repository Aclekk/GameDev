using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HantuJumpscare : MonoBehaviour
{
    [Header("Refs (auto-isi kalau kosong)")]
    public Transform player;                        // root "Player"
    public Camera playerCamera;                     // child "PlayerCamera"
    public Animator enemyAnimator;                  // Trigger "Jumpscare"
    public MonoBehaviour enemyMovementToDisable;    // mis. HantuMove
    public MonoBehaviour playerControllerToDisable; // mis. FirstPersonController/HeroController
    public MonoBehaviour[] extraPlayerScriptsToDisable; // opsional: sway/headbob dll.

    [Header("Trigger & Kamera")]
    public float triggerRadius = 2.2f;    // jarak memicu jumpscare
    public float camApproach   = 0.65f;   // jarak kamera di DEPAN hantu
    public float camHeight     = 1.6f;    // tinggi kepala hantu
    public bool  instantSnap   = true;    // true=langsung snap, false=lerp
    public float camLerpTime   = 0.12f;   // dipakai kalau instantSnap=false
    public float jumpscareDuration = 1.8f;

    [Header("SFX Jumpscare")]
    public AudioSource audioSource;       // dedicated untuk SFX jumpscare
    public AudioClip jumpscareSfxA;       // slot 1
    public AudioClip jumpscareSfxB;       // slot 2
    [Range(0f,1f)] public float jumpscareVolume = 1f;  // kontrol volume 0–1
    public bool playBothSfx = false;      // true: mainkan A lalu B
    public bool randomizeIfSingle = true; // kalau false: prioritas A; kalau true: acak A/B
    public float sfxBDelay = 0.0f;        // jeda mainkan B sesudah A (kalau playBothSfx)

    [Header("UI Kalah")]
    public GameObject loseCanvas;
    public bool pauseTimeOnLose = true;
    public bool fadeLoseUI = true;
    public float fadeTime = 0.35f;
    public string mainMenuSceneName = "MainMenu";

    [Header("Audio Control")]
    public bool muteAllSceneAudioOnJumpscare = true;
    public bool restoreAudioOnRetry = true;

    bool done;
    Rigidbody playerRb;
    CharacterController playerCc;
    Transform camOriginalParent;
    private HantuMove hantuMove;
    private AudioSource[] allSceneAudioSources;
    private float[] originalVolumes;

    public bool IsInProgress => done;   // done=true berarti jumpscare sudah dimulai

    void Awake()
    {
        if (player == null) { var p = GameObject.Find("Player"); if (p) player = p.transform; }
        if (playerCamera == null)
        {
            var pc = player ? player.GetComponentInChildren<Camera>(true) : Camera.main;
            if (pc) playerCamera = pc;
        }
        if (enemyAnimator == null) enemyAnimator = GetComponentInChildren<Animator>();
        if (audioSource == null)   audioSource   = GetComponent<AudioSource>();
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

        hantuMove = GetComponent<HantuMove>();

        if (loseCanvas && loseCanvas.activeSelf) loseCanvas.SetActive(false);
    }

    void Update()
    {
        if (done || player == null || playerCamera == null) return;

        var hm = GetComponent<HantuMove>();
        if (hm != null && !hm.IsGhostActive()) return;   // hidden = ga bisa jumpscare

        // >>> PATCH: kalau hantu sedang kena radius lantern, jangan boleh trigger jumpscare
        if (hantuMove != null && hantuMove.IsInLanternRadius()) return;

        if (Vector3.Distance(transform.position, player.position) <= triggerRadius)
            StartCoroutine(DoJumpscare());
    }

    IEnumerator DoJumpscare()
    {
        done = true;

        if (muteAllSceneAudioOnJumpscare)
        {
            MuteAllSceneAudio();
        }

        if (enemyMovementToDisable) enemyMovementToDisable.enabled = false;
        if (playerControllerToDisable) playerControllerToDisable.enabled = false;
        if (extraPlayerScriptsToDisable != null)
            foreach (var s in extraPlayerScriptsToDisable) if (s) s.enabled = false;

        if (playerRb)
        {
            playerRb.velocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.isKinematic = true;
        }
        if (playerCc) playerCc.enabled = false;

        var hm = GetComponent<HantuMove>();
        if (hm != null) hm.SuppressCrawlAudio(true);
        foreach (var asrc in GetComponentsInChildren<AudioSource>())
        {
            if (audioSource != null && asrc == audioSource) continue;
            if (asrc.isPlaying) asrc.Stop();
            asrc.volume = 0f;
        }
        if (enemyAnimator) enemyAnimator.SetBool("isCrawl", false);

        if (enemyAnimator) enemyAnimator.SetTrigger("Jumpscare");

        PlayJumpscareSfx();

        var camT = playerCamera.transform;
        camOriginalParent = camT.parent;
        Vector3 head = transform.position + Vector3.up * camHeight;
        Vector3 targetPos = transform.position + transform.forward * camApproach + Vector3.up * camHeight;
        Quaternion targetRot = Quaternion.LookRotation((head - targetPos).normalized, Vector3.up);

        if (instantSnap || camLerpTime <= 0.01f)
        {
            camT.position = targetPos; camT.rotation = targetRot;
        }
        else
        {
            Vector3 startPos = camT.position; Quaternion startRot = camT.rotation;
            float t = 0f;
            while (t < camLerpTime)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / camLerpTime);
                camT.position = Vector3.Lerp(startPos, targetPos, k);
                camT.rotation = Quaternion.Slerp(startRot, targetRot, k);
                yield return null;
            }
            camT.position = targetPos; camT.rotation = targetRot;
        }
        camT.SetParent(transform, true);

        yield return new WaitForSeconds(jumpscareDuration);

        ShowLoseUI();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void PlayJumpscareSfx()
    {
        if (audioSource == null) return;

        audioSource.spatialBlend = 1f;
        audioSource.dopplerLevel = 0f;
        audioSource.ignoreListenerPause = true;
        audioSource.volume = 1f;

        if (playBothSfx)
        {
            if (jumpscareSfxA) audioSource.PlayOneShot(jumpscareSfxA, Mathf.Clamp01(jumpscareVolume));
            if (jumpscareSfxB)
            {
                if (sfxBDelay <= 0f)
                    audioSource.PlayOneShot(jumpscareSfxB, Mathf.Clamp01(jumpscareVolume));
                else
                    StartCoroutine(PlayDelayed(jumpscareSfxB, Mathf.Clamp01(jumpscareVolume), sfxBDelay));
            }
        }
        else
        {
            AudioClip chosen = null;
            if (randomizeIfSingle)
            {
                bool pickA = (jumpscareSfxA && jumpscareSfxB) ? (Random.value < 0.5f) : true;
                chosen = pickA ? (jumpscareSfxA ?? jumpscareSfxB) : (jumpscareSfxB ?? jumpscareSfxA);
            }
            else
            {
                chosen = jumpscareSfxA ? jumpscareSfxA : jumpscareSfxB;
            }

            if (chosen) audioSource.PlayOneShot(chosen, Mathf.Clamp01(jumpscareVolume));
        }
    }

    void MuteAllSceneAudio()
    {
        allSceneAudioSources = FindObjectsOfType<AudioSource>();
        originalVolumes = new float[allSceneAudioSources.Length];

        for (int i = 0; i < allSceneAudioSources.Length; i++)
        {
            AudioSource source = allSceneAudioSources[i];
            
            if (source == audioSource) continue;

            originalVolumes[i] = source.volume;
            source.volume = 0f;

            if (source.isPlaying && !source.loop)
            {
                source.Stop();
            }
        }

        Debug.Log($"Muted {allSceneAudioSources.Length} AudioSources in scene for jumpscare");
    }

    void RestoreAllSceneAudio()
    {
        if (allSceneAudioSources == null || originalVolumes == null) return;

        for (int i = 0; i < allSceneAudioSources.Length; i++)
        {
            if (allSceneAudioSources[i] != null)
            {
                allSceneAudioSources[i].volume = originalVolumes[i];
            }
        }

        Debug.Log($"Restored {allSceneAudioSources.Length} AudioSources volumes");
    }

    IEnumerator PlayDelayed(AudioClip clip, float vol, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource) audioSource.PlayOneShot(clip, vol);
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

    public void UI_Retry()
    {
        if (restoreAudioOnRetry)
        {
            RestoreAllSceneAudio();
        }
        
        Time.timeScale = 1f;
        var cur = SceneManager.GetActiveScene();
        SceneManager.LoadScene(cur.buildIndex);
    }

    public void UI_MainMenu()
    {
        if (restoreAudioOnRetry)
        {
            RestoreAllSceneAudio();
        }
        
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
