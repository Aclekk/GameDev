using UnityEngine;
using Michsky.UI.Dark;
using UHFPS.Runtime;

public class PauseManager : MonoBehaviour
{
    [Header("References")]
    public GameObject pausedCanvas;
    public MainPanelManager panelManager;
    public ModalWindowManager exitModal;

    private bool isPaused = false;
    private bool isExitModalActive = false;

    void Start()
    {
        if (pausedCanvas != null)
        {
            pausedCanvas.SetActive(false);
        }

        DisableUHFPSPauseAndInventory();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void DisableUHFPSPauseAndInventory()
    {
        if (GameManager.HasReference)
        {
            GameManager gameManager = GameManager.Instance;
            
            if (gameManager.PausePanel != null)
            {
                gameManager.PausePanel.gameObject.SetActive(false);
            }

            if (gameManager.InventoryPanel != null)
            {
                gameManager.InventoryPanel.gameObject.SetActive(false);
            }

            if (gameManager.TabPanel != null)
            {
                gameManager.TabPanel.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                if (isExitModalActive)
                {
                    CloseExitModal();
                }
                else if (panelManager != null && panelManager.currentPanelIndex > 0)
                {
                    panelManager.PanelAnim(0);
                }
                else if (panelManager != null && panelManager.currentPanelIndex == 0)
                {
                    ShowExitModal();
                }
                else
                {
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
            pausedCanvas.SetActive(false);
            pausedCanvas.SetActive(true);
            
            if (panelManager != null)
            {
                panelManager.currentPanelIndex = 0;
                panelManager.OpenFirstTab();
            }
        }

        if (GameManager.HasReference)
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager.PlayerPresence != null && gameManager.PlayerPresence.PlayerIsUnlocked)
            {
                gameManager.PlayerPresence.FreezePlayer(true, true);
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ResumeGame()
    {
        isPaused = false;

        if (pausedCanvas != null)
            pausedCanvas.SetActive(false);

        if (GameManager.HasReference)
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager.PlayerPresence != null && gameManager.PlayerPresence.PlayerIsUnlocked)
            {
                gameManager.PlayerPresence.FreezePlayer(false, false);
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnContinueButtonClicked()
    {
        ResumeGame();
    }

    public void ResumeFromHome()
    {
        if (isExitModalActive)
        {
            CloseExitModal();
        }
        ResumeGame();
    }

    public void ShowExitModal()
    {
        if (exitModal != null)
        {
            exitModal.ModalWindowIn();
            isExitModalActive = true;
        }
    }

    public void CloseExitModal()
    {
        if (exitModal != null)
        {
            exitModal.ModalWindowOut();
            isExitModalActive = false;
        }
    }

    public void OnExitConfirmed()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
