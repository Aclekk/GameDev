using UnityEngine;
using Michsky.UI.Dark;

public class PauseManager : MonoBehaviour
{
    [Header("References")]
    public GameObject pausedCanvas;              // drag GameObject "Paused"
    public FirstPersonController playerController; // drag script gerak di Player
    public MainPanelManager panelManager;        // drag MainPanelManager dari "Paused"
    public ModalWindowManager exitModal;         // drag ModalWindowManager dari exit window

    private bool isPaused = false;
    private bool isExitModalActive = false;

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
            {
                // Prioritas 1: Jika exit modal aktif, tutup modal
                if (isExitModalActive)
                {
                    CloseExitModal();
                }
                // Prioritas 2: Jika di panel selain home (index > 0), kembali ke home
                else if (panelManager != null && panelManager.currentPanelIndex > 0)
                {
                    panelManager.PanelAnim(0); // Kembali ke home panel
                }
                // Prioritas 3: Jika di home panel, BUKA EXIT MODAL (bukan resume)
                else if (panelManager != null && panelManager.currentPanelIndex == 0)
                {
                    ShowExitModal(); // Buka exit modal
                }
                else
                {
                    // Fallback jika panelManager null
                    ResumeGame();
                }
            }
            else
            {
                PauseGame();
            }
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
            
            // Reset ke home panel saat pause
            if (panelManager != null)
            {
                panelManager.currentPanelIndex = 0;
                panelManager.OpenFirstTab();
            }
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

    // Fungsi khusus untuk resume game dari home panel (bypass modal)
    public void ResumeFromHome()
    {
        if (isExitModalActive)
        {
            CloseExitModal();
        }
        ResumeGame();
    }

    // Fungsi untuk menampilkan exit modal
    public void ShowExitModal()
    {
        if (exitModal != null)
        {
            exitModal.ModalWindowIn();
            isExitModalActive = true;
        }
    }

    // Fungsi untuk menutup exit modal
    public void CloseExitModal()
    {
        if (exitModal != null)
        {
            exitModal.ModalWindowOut();
            isExitModalActive = false;
        }
    }

    // Fungsi untuk keluar dari game (dipanggil dari tombol Yes di exit modal)
    public void OnExitConfirmed()
    {
        // Untuk di Unity Editor
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        // Untuk build
        #else
            Application.Quit();
        #endif
    }
}
