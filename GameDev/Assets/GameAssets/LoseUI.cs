using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseUI : MonoBehaviour
{
    public static LoseUI Instance;

    [Header("Optional SFX")]
    public AudioSource audioSource;
    public AudioClip loseSfx;

    [Header("Fade")]
    public CanvasGroup canvasGroup;    // drag CanvasGroup di LoseCanvas
    public float fadeTime = 0.35f;

    void Awake()
    {
        Instance = this;
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);          // default off
        if (canvasGroup) canvasGroup.alpha = 0f;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
        if (audioSource && loseSfx) audioSource.PlayOneShot(loseSfx);
        // Bebaskan kursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            if (canvasGroup) canvasGroup.alpha = Mathf.Clamp01(t / fadeTime);
            yield return null;
        }
        if (canvasGroup) canvasGroup.alpha = 1f;
    }

    // === Tombol ===
    public void OnClickRetry()
    {
        Time.timeScale = 1f;
        Scene cur = SceneManager.GetActiveScene();
        SceneManager.LoadScene(cur.buildIndex);
    }

    public void OnClickMainMenu(string menuSceneName = "MainMenu")
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    public void OnClickQuit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
