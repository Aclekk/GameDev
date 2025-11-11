using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

    [Header("Anim Durations")]
    [SerializeField] float titleInTime = 1.2f;
    [SerializeField] float buttonsInTime = 0.8f;
    [SerializeField] float optionsInTime = 0.55f;
    [SerializeField] float optionsOutTime = 0.40f;

    const string PP_MUSIC = "MusicEnabled";
    Vector3 optionsInitialLocalPos;
    bool optionsOpen = false;
    float musicTarget = 1f;

    void Awake()
    {
        Time.timeScale = 1f;

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

        // Hook main buttons
        if (playButton) playButton.onClick.AddListener(OnPlay);
        if (settingsButton) settingsButton.onClick.AddListener(OpenOptions);
        if (quitButton) quitButton.onClick.AddListener(OnQuit);
        if (closeOptionsButton) closeOptionsButton.onClick.AddListener(CloseOptions);

        // Hook sound buttons
        if (soundOnButton)  soundOnButton.onClick.AddListener(() => SetMusic(true));
        if (soundOffButton) soundOffButton.onClick.AddListener(() => SetMusic(false));

        // Init music state from prefs
        bool musicOn = PlayerPrefs.GetInt(PP_MUSIC, 1) == 1;
        ApplyMusicImmediate(musicOn);
        UpdateSoundButtonsUI(musicOn);
    }

    void Start()
    {
        // Entrance anims (iTween)
        if (titleText)
        {
            iTween.MoveFrom(titleText.gameObject, iTween.Hash(
                "islocal", true, "y", titleText.localPosition.y + 700f,
                "time", titleInTime, "easeType", "easeOutBounce"));
        }
        if (playButton)
        {
            iTween.ScaleFrom(playButton.gameObject, iTween.Hash(
                "x", 0f, "y", 0f, "time", buttonsInTime, "delay", 0.15f, "easeType", "easeOutBack"));
        }
        if (settingsButton)
        {
            iTween.MoveFrom(settingsButton.gameObject, iTween.Hash(
                "islocal", true, "x", -800f, "time", buttonsInTime, "delay", 0.25f, "easeType", "easeOutExpo"));
        }
        if (quitButton)
        {
            iTween.MoveFrom(quitButton.gameObject, iTween.Hash(
                "islocal", true, "x", 800f, "time", buttonsInTime, "delay", 0.35f, "easeType", "easeOutExpo"));
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

    // ===== Options open/close with iTween =====
    public void OpenOptions()
    {
        if (optionsOpen || optionsPanel == null) return;
        optionsOpen = true;

        optionsPanel.gameObject.SetActive(true);
        if (optionsCanvasGroup)
        {
            optionsCanvasGroup.blocksRaycasts = true;
            optionsCanvasGroup.interactable = true;
            iTween.ValueTo(optionsPanel.gameObject, iTween.Hash(
                "from", optionsCanvasGroup.alpha, "to", 1f, "time", optionsInTime,
                "onupdate", "OnOptionsFadeUpdate", "onupdatetarget", gameObject));
        }
        optionsPanel.localPosition = optionsInitialLocalPos;
        iTween.MoveFrom(optionsPanel.gameObject, iTween.Hash(
            "islocal", true, "y", optionsInitialLocalPos.y - 900f, "time", optionsInTime, "easeType", "easeOutExpo"));
        iTween.ScaleFrom(optionsPanel.gameObject, iTween.Hash(
            "x", 0.85f, "y", 0.85f, "time", optionsInTime, "easeType", "easeOutBack"));

        SetMainButtonsInteractable(false);
    }

    public void CloseOptions()
    {
        if (!optionsOpen || optionsPanel == null) return;
        optionsOpen = false;

        if (optionsCanvasGroup)
        {
            iTween.ValueTo(optionsPanel.gameObject, iTween.Hash(
                "from", optionsCanvasGroup.alpha, "to", 0f, "time", optionsOutTime,
                "onupdate", "OnOptionsFadeUpdate", "onupdatetarget", gameObject));
        }
        iTween.MoveTo(optionsPanel.gameObject, iTween.Hash(
            "islocal", true, "y", optionsInitialLocalPos.y - 900f, "time", optionsOutTime, "easeType", "easeInBack"));
        iTween.ScaleTo(optionsPanel.gameObject, iTween.Hash(
            "x", 0.85f, "y", 0.85f, "time", optionsOutTime, "easeType", "easeInBack",
            "oncomplete", "OnOptionsClosed", "oncompletetarget", gameObject));

        SetMainButtonsInteractable(true);
    }

    // iTween callbacks
    public void OnOptionsFadeUpdate(float a)
    {
        if (optionsCanvasGroup) optionsCanvasGroup.alpha = a;
    }
    public void OnOptionsClosed()
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
        if (on && soundOnButton)  iTween.PunchScale(soundOnButton.gameObject, new Vector3(0.08f,0.08f,0), 0.2f);
        if (!on && soundOffButton) iTween.PunchScale(soundOffButton.gameObject, new Vector3(0.08f,0.08f,0), 0.2f);

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

        float from = backsound.volume;
        float to = on ? 1f : 0f;

        // Unmute dulu kalau mau fade-in
        if (on && backsound.mute) backsound.mute = false;
        if (on && !backsound.isPlaying) backsound.Play();

        // Tween float manual pakai ValueTo, update ke volume
        iTween.ValueTo(gameObject, iTween.Hash(
            "from", from, "to", to, "time", musicFadeTime,
            "onupdate", "OnMusicFadeUpdate", "onupdatetarget", gameObject,
            "oncomplete", on ? "OnMusicFadeInComplete" : "OnMusicFadeOutComplete",
            "oncompletetarget", gameObject
        ));
    }

    public void OnMusicFadeUpdate(float v)
    {
        if (!backsound) return;
        backsound.volume = v;
    }

    public void OnMusicFadeInComplete()
    {
        if (!backsound) return;
        backsound.mute = false;
        if (!backsound.isPlaying) backsound.Play();
    }

    public void OnMusicFadeOutComplete()
    {
        if (!backsound) return;
        backsound.volume = 0f;
        backsound.mute = true;
        if (backsound.isPlaying) backsound.Pause();
    }

    // ===== Utils =====
    void SetMainButtonsInteractable(bool v)
    {
        if (playButton) playButton.interactable = v;
        if (settingsButton) settingsButton.interactable = v;
        if (quitButton) quitButton.interactable = v;
    }
}
