using UnityEngine;
using Michsky.UI.Dark;
using UHFPS.Runtime;
using UnityEngine.AI;

public class PauseManager : MonoBehaviour
{
    [Header("References")]
    public GameObject pausedCanvas;
    public MainPanelManager panelManager;
    public ModalWindowManager exitModal;

    private bool isPaused = false;
    private bool isExitModalActive = false;
    private AudioSource[] allAudioSources;
    private bool[] audioSourceStates;
    private HantuMove[] allHantuScripts;
    private NavMeshAgent[] allNavMeshAgents;
    private bool[] hantuScriptsStates;
    private bool[] navMeshAgentsStates;

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

        FreezeAllGhosts();
        MuteAllAudio();

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

        UnfreezeAllGhosts();
        UnmuteAllAudio();

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

    private void MuteAllAudio()
    {
        allAudioSources = FindObjectsOfType<AudioSource>();
        audioSourceStates = new bool[allAudioSources.Length];

        for (int i = 0; i < allAudioSources.Length; i++)
        {
            if (IsAudioSourceInPausedCanvas(allAudioSources[i]))
            {
                audioSourceStates[i] = false;
                continue;
            }

            audioSourceStates[i] = allAudioSources[i].isPlaying;
            if (allAudioSources[i].isPlaying)
            {
                allAudioSources[i].Pause();
            }
        }
    }

    private void UnmuteAllAudio()
    {
        if (allAudioSources != null && audioSourceStates != null)
        {
            for (int i = 0; i < allAudioSources.Length; i++)
            {
                if (allAudioSources[i] != null && audioSourceStates[i])
                {
                    allAudioSources[i].UnPause();
                }
            }
        }
    }

    private bool IsAudioSourceInPausedCanvas(AudioSource audioSource)
    {
        if (pausedCanvas == null || audioSource == null)
            return false;

        Transform current = audioSource.transform;
        while (current != null)
        {
            if (current.gameObject == pausedCanvas)
                return true;
            current = current.parent;
        }
        return false;
    }

    private void FreezeAllGhosts()
    {
        allHantuScripts = FindObjectsOfType<HantuMove>();
        hantuScriptsStates = new bool[allHantuScripts.Length];

        for (int i = 0; i < allHantuScripts.Length; i++)
        {
            hantuScriptsStates[i] = allHantuScripts[i].enabled;
            allHantuScripts[i].enabled = false;
        }

        allNavMeshAgents = FindObjectsOfType<NavMeshAgent>();
        navMeshAgentsStates = new bool[allNavMeshAgents.Length];

        for (int i = 0; i < allNavMeshAgents.Length; i++)
        {
            navMeshAgentsStates[i] = allNavMeshAgents[i].enabled;
            if (allNavMeshAgents[i].enabled)
            {
                allNavMeshAgents[i].isStopped = true;
                allNavMeshAgents[i].velocity = Vector3.zero;
            }
        }
    }

    private void UnfreezeAllGhosts()
    {
        if (allHantuScripts != null && hantuScriptsStates != null)
        {
            for (int i = 0; i < allHantuScripts.Length; i++)
            {
                if (allHantuScripts[i] != null)
                {
                    allHantuScripts[i].enabled = hantuScriptsStates[i];
                }
            }
        }

        if (allNavMeshAgents != null && navMeshAgentsStates != null)
        {
            for (int i = 0; i < allNavMeshAgents.Length; i++)
            {
                if (allNavMeshAgents[i] != null && navMeshAgentsStates[i])
                {
                    allNavMeshAgents[i].isStopped = false;
                }
            }
        }
    }
}
