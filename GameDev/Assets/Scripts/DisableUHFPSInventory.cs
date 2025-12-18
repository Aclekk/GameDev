using UnityEngine;
using UnityEngine.InputSystem;
using UHFPS.Runtime;

public class DisableUHFPSInventory : MonoBehaviour
{
    private void Start()
    {
        DisableInventoryInput();
        DisablePauseMenuUHFPS();
    }

    private void DisableInventoryInput()
    {
        if (GameManager.HasReference)
        {
            GameManager gameManager = GameManager.Instance;
            
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

    private void DisablePauseMenuUHFPS()
    {
        if (GameManager.HasReference)
        {
            GameManager gameManager = GameManager.Instance;
            
            if (gameManager.PausePanel != null)
            {
                gameManager.PausePanel.gameObject.SetActive(false);
            }
        }
    }
}
