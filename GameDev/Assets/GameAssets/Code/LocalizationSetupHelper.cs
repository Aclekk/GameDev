using UnityEngine;
using TMPro;

public class LocalizationSetupHelper : MonoBehaviour
{
    [Header("Auto Setup Keys")]
    [Tooltip("Mapping GameObject path ke translation key")]
    public LocalizationMapping[] mappings;

    [System.Serializable]
    public class LocalizationMapping
    {
        public string gameObjectPath;
        public string translationKey;
    }

    [ContextMenu("Setup All Localized Texts")]
    void SetupAllLocalizedTexts()
    {
        if (mappings == null || mappings.Length == 0)
        {
            Debug.LogWarning("No mappings defined!");
            return;
        }

        int setupCount = 0;

        foreach (var mapping in mappings)
        {
            GameObject targetObj = GameObject.Find(mapping.gameObjectPath);
            
            if (targetObj != null)
            {
                TextMeshProUGUI textComponent = targetObj.GetComponent<TextMeshProUGUI>();
                
                if (textComponent != null)
                {
                    LocalizedText localizedText = targetObj.GetComponent<LocalizedText>();
                    
                    if (localizedText == null)
                    {
                        localizedText = targetObj.AddComponent<LocalizedText>();
                    }

                    localizedText.translationKey = mapping.translationKey;
                    setupCount++;
                    Debug.Log($"Setup localization for: {mapping.gameObjectPath} with key: {mapping.translationKey}");
                }
                else
                {
                    Debug.LogWarning($"No TextMeshProUGUI found on: {mapping.gameObjectPath}");
                }
            }
            else
            {
                Debug.LogWarning($"GameObject not found: {mapping.gameObjectPath}");
            }
        }

        Debug.Log($"Setup complete! {setupCount} texts configured.");
    }
}
