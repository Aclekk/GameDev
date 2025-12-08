using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Header("References")]
    public GameObject pausedCanvas;              // drag GameObject "Paused"
    public FirstPersonController playerController; // drag script gerak di Player

    private bool isPaused = false;

    void Start()
    {
        // Pastikan canvas mati di awal
        if (pausedCanvas != null)
        {
            pausedCanvas.SetActive(false);
        }

        // Kondisi main game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Cek ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void PauseGame()
    {
        isPaused = true;

        if (pausedCanvas != null)
        {
            // Matikan dulu, hidupkan lagi biar animasi di canvas ke-reset
            pausedCanvas.SetActive(false);
            pausedCanvas.SetActive(true);
        }

        // Player nggak bisa gerak
        if (playerController != null)
            playerController.enabled = false;

        // Cursor bebas buat klik UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ResumeGame()
    {
        isPaused = false;

        // Sembunyikan menu pause
        if (pausedCanvas != null)
            pausedCanvas.SetActive(false);

        // Player bisa gerak lagi
        if (playerController != null)
            playerController.enabled = true;

        // Cursor balik ke mode main game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Dipanggil dari tombol "Continue"
    public void OnContinueButtonClicked()
    {
        ResumeGame();
    }
}
