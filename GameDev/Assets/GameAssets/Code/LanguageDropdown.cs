using UnityEngine;

public class LanguageDropdown : MonoBehaviour
{
    private Michsky.UI.Dark.CustomDropdown customDropdown;

    void Awake()
    {
        customDropdown = GetComponent<Michsky.UI.Dark.CustomDropdown>();
        
        if (customDropdown == null)
        {
            Debug.LogError("CustomDropdown component not found on " + gameObject.name);
        }
    }

    public void SetEnglish()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.SetLanguage(LanguageManager.Language.English);
            Debug.Log("Language set to English");
        }
        else
        {
            Debug.LogWarning("LanguageManager not found!");
        }
    }

    public void SetIndonesian()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.SetLanguage(LanguageManager.Language.Indonesian);
            Debug.Log("Language set to Indonesian");
        }
        else
        {
            Debug.LogWarning("LanguageManager not found!");
        }
    }
}
