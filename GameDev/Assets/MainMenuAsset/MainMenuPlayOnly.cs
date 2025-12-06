using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
// pakai TMP optional

public class MainMenuPlayOnly : MonoBehaviour
{
    [SerializeField] string gameSceneName = "Game";

    [Header("Title & Buttons")]
    [SerializeField] RectTransform titleText;
    [SerializeField] Button playButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button quitButton;

    [Header("Options Panel")]
    [SerializeField] RectTransform optionsPanel;
    [SerializeField] CanvasGroup optionsCanvasGroup;
    [SerializeField] Button closeOptionsButton;

    [Header("Backsound Toggle (two buttons)")]
    [SerializeField] AudioSource backsound;      // drag GO "Backsound" (AudioSource)
    [SerializeField] Button soundOnButton;        // drag GO "Sound on"  (button to TURN ON)
    [SerializeField] Button soundOffButton;       // drag GO "Sound off" (button to TURN OFF)
    [SerializeField] float musicFadeTime = 0.25f; // fade halus

    [Header("Button Hover Effects")]
    [SerializeField] AudioClip hoverSound;        // Sound saat hover button
    [SerializeField] AudioSource uiAudioSource;   // AudioSource untuk UI sounds (optional, auto-create jika null)
    [SerializeField] float hoverScaleAmount = 1.1f; // Scale saat hover (1.1 = 10% lebih besar)
    [SerializeField] float hoverAnimDuration = 0.15f; // Durasi animasi hover
    [SerializeField] bool enableHoverSound = true; // Toggle untuk enable/disable hover sound

    [Header("Anim Durations")]
    [SerializeField] float titleInTime = 1.2f;
    [SerializeField] float buttonsInTime = 0.8f;
    [SerializeField] float optionsInTime = 0.55f;
    [SerializeField] float optionsOutTime = 0.40f;

    const string PP_MUSIC = "MusicEnabled";
    Vector3 optionsInitialLocalPos;
    bool optionsOpen = false;
    float musicTarget = 1f;
    
    // Store original positions/scales for entrance animations
    Vector3 titleOriginalPos;
    Vector3 playButtonOriginalScale;
    Vector3 settingsButtonOriginalPos;
    Vector3 quitButtonOriginalPos;
    
    // Store original scales for hover animations
    Dictionary<Button, Vector3> buttonOriginalScales = new Dictionary<Button, Vector3>();
    Dictionary<Button, Coroutine> buttonHoverCoroutines = new Dictionary<Button, Coroutine>();
    
    // Track running coroutines to prevent conflicts
    Coroutine optionsAnimationCoroutine;
    Coroutine musicFadeCoroutine;

    void Awake()
    {
        Time.timeScale = 1f;

        // Store original positions/scales for entrance animations
        if (titleText) titleOriginalPos = titleText.localPosition;
        if (playButton) playButtonOriginalScale = playButton.transform.localScale;
        if (settingsButton) settingsButtonOriginalPos = settingsButton.transform.localPosition;
        if (quitButton) quitButtonOriginalPos = quitButton.transform.localPosition;

        // OptionsPanel init
        if (optionsPanel != null)
        {
            optionsInitialLocalPos = optionsPanel.localPosition;
            optionsPanel.gameObject.SetActive(false);
        }
        if (optionsPanel && !optionsCanvasGroup) optionsCanvasGroup = optionsPanel.GetComponent<CanvasGroup>();
        if (optionsCanvasGroup)
        {
            optionsCanvasGroup.alpha = 0f;
            optionsCanvasGroup.blocksRaycasts = false;
            optionsCanvasGroup.interactable = false;
        }

        // Setup UI AudioSource jika belum ada
        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
            uiAudioSource.volume = 0.5f;
        }

        // Store original scales for hover animations
        StoreButtonOriginalScales();

        // Hook main buttons
        if (playButton)
        {
            playButton.onClick.AddListener(OnPlay);
            SetupButtonHover(playButton);
        }
        if (settingsButton)
        {
            settingsButton.onClick.AddListener(OpenOptions);
            SetupButtonHover(settingsButton);
        }
        if (quitButton)
        {
            quitButton.onClick.AddListener(OnQuit);
            SetupButtonHover(quitButton);
        }
        if (closeOptionsButton)
        {
            closeOptionsButton.onClick.AddListener(CloseOptions);
            SetupButtonHover(closeOptionsButton);
        }

        // Hook sound buttons
        if (soundOnButton)
        {
            soundOnButton.onClick.AddListener(() => SetMusic(true));
            SetupButtonHover(soundOnButton);
        }
        if (soundOffButton)
        {
            soundOffButton.onClick.AddListener(() => SetMusic(false));
            SetupButtonHover(soundOffButton);
        }

        // Init music state from prefs
        bool musicOn = PlayerPrefs.GetInt(PP_MUSIC, 1) == 1;
        ApplyMusicImmediate(musicOn);
        UpdateSoundButtonsUI(musicOn);
    }

    void StoreButtonOriginalScales()
    {
        Button[] allButtons = { playButton, settingsButton, quitButton, closeOptionsButton, soundOnButton, soundOffButton };
        foreach (Button btn in allButtons)
        {
            if (btn != null && !buttonOriginalScales.ContainsKey(btn))
            {
                buttonOriginalScales[btn] = btn.transform.localScale;
            }
        }
    }

    void SetupButtonHover(Button button)
    {
        if (button == null) return;

        // Get or add EventTrigger component
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // Clear existing entries
        trigger.triggers.Clear();

        // Pointer Enter (Hover Start)
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((data) => { OnButtonHoverEnter(button); });
        trigger.triggers.Add(pointerEnter);

        // Pointer Exit (Hover End)
        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => { OnButtonHoverExit(button); });
        trigger.triggers.Add(pointerExit);
    }

    void OnButtonHoverEnter(Button button)
    {
        if (button == null || !button.interactable) return;

        // Stop existing hover animation if any
        if (buttonHoverCoroutines.ContainsKey(button) && buttonHoverCoroutines[button] != null)
        {
            StopCoroutine(buttonHoverCoroutines[button]);
        }

        // Play hover sound
        if (enableHoverSound && hoverSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(hoverSound);
        }

        // Start hover scale animation
        if (buttonOriginalScales.ContainsKey(button))
        {
            Vector3 originalScale = buttonOriginalScales[button];
            Vector3 targetScale = originalScale * hoverScaleAmount;
            buttonHoverCoroutines[button] = StartCoroutine(AnimateButtonHoverScale(button.transform, button.transform.localScale, targetScale, hoverAnimDuration));
        }
    }

    void OnButtonHoverExit(Button button)
    {
        if (button == null) return;

        // Stop existing hover animation if any
        if (buttonHoverCoroutines.ContainsKey(button) && buttonHoverCoroutines[button] != null)
        {
            StopCoroutine(buttonHoverCoroutines[button]);
        }

        // Return to original scale
        if (buttonOriginalScales.ContainsKey(button))
        {
            Vector3 originalScale = buttonOriginalScales[button];
            buttonHoverCoroutines[button] = StartCoroutine(AnimateButtonHoverScale(button.transform, button.transform.localScale, originalScale, hoverAnimDuration));
        }
    }

    IEnumerator AnimateButtonHoverScale(Transform target, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easedT = EaseOutExpo(t); // Smooth easing
            target.localScale = Vector3.Lerp(from, to, easedT);
            yield return null;
        }
        
        target.localScale = to;
    }

    void Start()
    {
        // Entrance anims (using Coroutines)
        if (titleText)
        {
            Vector3 startPos = titleOriginalPos;
            startPos.y += 700f;
            titleText.localPosition = startPos;
            StartCoroutine(AnimateMoveLocal(titleText.transform, startPos, titleOriginalPos, titleInTime, EaseOutBounce));
        }
        if (playButton)
        {
            playButton.transform.localScale = Vector3.zero;
            StartCoroutine(AnimateScale(playButton.transform, Vector3.zero, playButtonOriginalScale, buttonsInTime, 0.15f, EaseOutBack));
        }
        if (settingsButton)
        {
            Vector3 startPos = settingsButtonOriginalPos;
            startPos.x = -800f;
            settingsButton.transform.localPosition = startPos;
            StartCoroutine(AnimateMoveLocal(settingsButton.transform, startPos, settingsButtonOriginalPos, buttonsInTime, 0.25f, EaseOutExpo));
        }
        if (quitButton)
        {
            Vector3 startPos = quitButtonOriginalPos;
            startPos.x = 800f;
            quitButton.transform.localPosition = startPos;
            StartCoroutine(AnimateMoveLocal(quitButton.transform, startPos, quitButtonOriginalPos, buttonsInTime, 0.35f, EaseOutExpo));
        }
    }

    // ===== Main Buttons =====
    public void OnPlay()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogWarning("Nama scene belum diisi!");
            return;
        }
        Time.timeScale = 1f;
        PlayerPrefs.DeleteKey("TutorialShown");
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnQuit()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // ===== Options open/close with Coroutines =====
    public void OpenOptions()
    {
        if (optionsOpen || optionsPanel == null) return;
        optionsOpen = true;

        // Stop any existing animation
        if (optionsAnimationCoroutine != null)
        {
            StopCoroutine(optionsAnimationCoroutine);
        }

        optionsPanel.gameObject.SetActive(true);
        if (optionsCanvasGroup)
        {
            optionsCanvasGroup.blocksRaycasts = true;
            optionsCanvasGroup.interactable = true;
        }
        
        // Set initial state
        Vector3 startPos = optionsInitialLocalPos;
        startPos.y -= 900f;
        optionsPanel.localPosition = startPos;
        optionsPanel.localScale = new Vector3(0.85f, 0.85f, 1f);
        if (optionsCanvasGroup) optionsCanvasGroup.alpha = 0f;

        // Start animation
        optionsAnimationCoroutine = StartCoroutine(AnimateOptionsOpen());
        SetMainButtonsInteractable(false);
    }

    public void CloseOptions()
    {
        if (!optionsOpen || optionsPanel == null) return;
        optionsOpen = false;

        // Stop any existing animation
        if (optionsAnimationCoroutine != null)
        {
            StopCoroutine(optionsAnimationCoroutine);
        }

        // Start close animation
        optionsAnimationCoroutine = StartCoroutine(AnimateOptionsClose());
        SetMainButtonsInteractable(true);
    }

    IEnumerator AnimateOptionsOpen()
    {
        float elapsed = 0f;
        float startAlpha = optionsCanvasGroup ? optionsCanvasGroup.alpha : 0f;
        Vector3 startPos = optionsPanel.localPosition;
        Vector3 startScale = optionsPanel.localScale;
        Vector3 targetPos = optionsInitialLocalPos;
        Vector3 targetScale = Vector3.one;

        while (elapsed < optionsInTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / optionsInTime;
            
            // Apply easing
            float expoT = EaseOutExpo(t);
            float backT = EaseOutBack(t);
            
            // Animate position
            optionsPanel.localPosition = Vector3.Lerp(startPos, targetPos, expoT);
            
            // Animate scale
            optionsPanel.localScale = Vector3.Lerp(startScale, targetScale, backT);
            
            // Animate alpha
            if (optionsCanvasGroup)
            {
                optionsCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, expoT);
            }
            
            yield return null;
        }

        // Ensure final values
        optionsPanel.localPosition = targetPos;
        optionsPanel.localScale = targetScale;
        if (optionsCanvasGroup) optionsCanvasGroup.alpha = 1f;
        optionsAnimationCoroutine = null;
    }

    IEnumerator AnimateOptionsClose()
    {
        float elapsed = 0f;
        float startAlpha = optionsCanvasGroup ? optionsCanvasGroup.alpha : 1f;
        Vector3 startPos = optionsPanel.localPosition;
        Vector3 startScale = optionsPanel.localScale;
        Vector3 targetPos = optionsInitialLocalPos;
        targetPos.y -= 900f;
        Vector3 targetScale = new Vector3(0.85f, 0.85f, 1f);

        while (elapsed < optionsOutTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / optionsOutTime;
            
            // Apply easing
            float backT = EaseInBack(t);
            
            // Animate position
            optionsPanel.localPosition = Vector3.Lerp(startPos, targetPos, backT);
            
            // Animate scale
            optionsPanel.localScale = Vector3.Lerp(startScale, targetScale, backT);
            
            // Animate alpha
            if (optionsCanvasGroup)
            {
                optionsCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, backT);
            }
            
            yield return null;
        }

        // Finalize
        OnOptionsClosed();
        optionsAnimationCoroutine = null;
    }

    void OnOptionsClosed()
    {
        if (optionsCanvasGroup)
        {
            optionsCanvasGroup.blocksRaycasts = false;
            optionsCanvasGroup.interactable = false;
            optionsCanvasGroup.alpha = 0f;
        }
        optionsPanel.gameObject.SetActive(false);
    }

    // ===== Music control (two buttons) =====
    void SetMusic(bool on)
    {
        // kecil efek klik
        if (on && soundOnButton)  StartCoroutine(PunchScale(soundOnButton.transform, new Vector3(0.08f, 0.08f, 0), 0.2f));
        if (!on && soundOffButton) StartCoroutine(PunchScale(soundOffButton.transform, new Vector3(0.08f, 0.08f, 0), 0.2f));

        ApplyMusicFade(on);
        UpdateSoundButtonsUI(on);

        PlayerPrefs.SetInt(PP_MUSIC, on ? 1 : 0);
        PlayerPrefs.Save();
    }

    void UpdateSoundButtonsUI(bool musicOn)
    {
        // LOGIKA: jika musik ON → tampilkan tombol "Sound off" (untuk mematikannya)
        //          jika musik OFF → tampilkan tombol "Sound on"  (untuk menyalakannya)
        if (soundOnButton)  soundOnButton.gameObject.SetActive(!musicOn);
        if (soundOffButton) soundOffButton.gameObject.SetActive(musicOn);
    }

    void ApplyMusicImmediate(bool on)
    {
        if (!backsound) return;
        backsound.volume = on ? 1f : 0f;
        backsound.mute = !on;
        if (on && !backsound.isPlaying) backsound.Play();
        if (!on && backsound.isPlaying) backsound.Pause();
    }

    void ApplyMusicFade(bool on)
    {
        if (!backsound)
            return;

        // Stop any existing fade
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        float from = backsound.volume;
        float to = on ? 1f : 0f;

        // Unmute dulu kalau mau fade-in
        if (on && backsound.mute) backsound.mute = false;
        if (on && !backsound.isPlaying) backsound.Play();

        // Start fade coroutine
        musicFadeCoroutine = StartCoroutine(AnimateMusicFade(from, to, on));
    }

    IEnumerator AnimateMusicFade(float from, float to, bool isFadeIn)
    {
        float elapsed = 0f;
        
        while (elapsed < musicFadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / musicFadeTime;
            float volume = Mathf.Lerp(from, to, t);
            
            if (backsound)
            {
                backsound.volume = volume;
            }
            
            yield return null;
        }

        // Finalize
        if (backsound)
        {
            backsound.volume = to;
            if (isFadeIn)
            {
                backsound.mute = false;
                if (!backsound.isPlaying) backsound.Play();
            }
            else
            {
                backsound.volume = 0f;
                backsound.mute = true;
                if (backsound.isPlaying) backsound.Pause();
            }
        }
        
        musicFadeCoroutine = null;
    }

    // ===== Utils =====
    void SetMainButtonsInteractable(bool v)
    {
        if (playButton) playButton.interactable = v;
        if (settingsButton) settingsButton.interactable = v;
        if (quitButton) quitButton.interactable = v;
    }

    // ===== Animation Coroutines =====
    IEnumerator AnimateMoveLocal(Transform target, Vector3 from, Vector3 to, float duration, System.Func<float, float> easing)
    {
        return AnimateMoveLocal(target, from, to, duration, 0f, easing);
    }

    IEnumerator AnimateMoveLocal(Transform target, Vector3 from, Vector3 to, float duration, float delay, System.Func<float, float> easing)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easedT = easing(t);
            target.localPosition = Vector3.Lerp(from, to, easedT);
            yield return null;
        }
        target.localPosition = to;
    }

    IEnumerator AnimateScale(Transform target, Vector3 from, Vector3 to, float duration, float delay, System.Func<float, float> easing)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easedT = easing(t);
            target.localScale = Vector3.Lerp(from, to, easedT);
            yield return null;
        }
        target.localScale = to;
        
        // Update stored original scale after entrance animation completes
        Button btn = target.GetComponent<Button>();
        if (btn != null && buttonOriginalScales.ContainsKey(btn))
        {
            buttonOriginalScales[btn] = to;
        }
    }

    IEnumerator PunchScale(Transform target, Vector3 punchAmount, float duration)
    {
        Vector3 originalScale = target.localScale;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Create a punch effect: goes up then back down
            float punchT = Mathf.Sin(t * Mathf.PI); // 0 to 1 to 0
            target.localScale = originalScale + punchAmount * punchT;
            
            yield return null;
        }
        
        target.localScale = originalScale;
    }

    // ===== Easing Functions =====
    float EaseOutBounce(float t)
    {
        if (t < 1f / 2.75f)
        {
            return 7.5625f * t * t;
        }
        else if (t < 2f / 2.75f)
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        else if (t < 2.5f / 2.75f)
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }
    }

    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    float EaseOutExpo(float t)
    {
        return t >= 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
    }

    float EaseInBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }
}
