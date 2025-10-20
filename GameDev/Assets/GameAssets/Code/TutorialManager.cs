using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public GameObject tutorialCanvas;          // Drag Canvas Tutorial kamu
    public MonoBehaviour playerController;     // Drag script gerak player (contoh: HeroController)
    public KeyCode closeKey = KeyCode.E;       // Tombol buat nutup tutorial

    bool tutorialActive = false;

    void Start()
    {
        // Kalau tutorial sudah pernah dilihat
        if (PlayerPrefs.GetInt("TutorialShown", 0) == 1)
        {
            if (tutorialCanvas) tutorialCanvas.SetActive(false);
            if (playerController) playerController.enabled = true;
            tutorialActive = false;
        }
        else
        {
            // Pertama kali main
            tutorialActive = true;
            if (tutorialCanvas) tutorialCanvas.SetActive(true);
            if (playerController) playerController.enabled = false;
            Time.timeScale = 0f; // pause game biar player fokus baca
        }
    }

    void Update()
    {
        if (!tutorialActive) return;

        if (Input.GetKeyDown(closeKey))
        {
            CloseTutorial();
        }
    }

    void CloseTutorial()
    {
        tutorialActive = false;
        PlayerPrefs.SetInt("TutorialShown", 1);
        PlayerPrefs.Save();

        if (tutorialCanvas) tutorialCanvas.SetActive(false);
        if (playerController) playerController.enabled = true;
        Time.timeScale = 1f;

        Debug.Log("[TutorialManager] Tutorial closed — Player control re-enabled.");
    }
}
