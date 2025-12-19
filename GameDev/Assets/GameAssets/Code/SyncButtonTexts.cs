using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SyncButtonTexts : MonoBehaviour
{
    [Header("Button Text Objects")]
    [Tooltip("Normal state text")]
    public TextMeshProUGUI normalText;
    
    [Tooltip("Highlighted state text")]
    public TextMeshProUGUI highlightedText;
    
    [Tooltip("Pressed state text")]
    public TextMeshProUGUI pressedText;

    [Header("Settings")]
    [Tooltip("Translation key for this button")]
    public string translationKey;
    
    [Tooltip("Force rebuild layout after text update")]
    public bool rebuildLayout = true;

    void Start()
    {
        UpdateAllTexts();
        SubscribeToLanguageChange();
    }

    void OnEnable()
    {
        SubscribeToLanguageChange();
    }

    void OnDisable()
    {
        UnsubscribeFromLanguageChange();
    }

    void SubscribeToLanguageChange()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            LanguageManager.Instance.OnLanguageChanged += OnLanguageChanged;
        }
    }

    void UnsubscribeFromLanguageChange()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }
    }

    void OnLanguageChanged(LanguageManager.Language newLanguage)
    {
        UpdateAllTexts();
    }

    public void UpdateAllTexts()
    {
        if (string.IsNullOrEmpty(translationKey))
        {
            Debug.LogWarning($"[SyncButtonTexts] Translation key is empty on {gameObject.name}", this);
            return;
        }

        if (LanguageManager.Instance == null)
        {
            Debug.LogWarning($"[SyncButtonTexts] LanguageManager not found", this);
            return;
        }

        string translatedText = LanguageManager.Instance.GetText(translationKey);

        if (normalText != null)
        {
            normalText.text = translatedText;
            Debug.Log($"[SyncButtonTexts] Updated Normal text to: {translatedText}");
        }

        if (highlightedText != null)
        {
            highlightedText.text = translatedText;
            Debug.Log($"[SyncButtonTexts] Updated Highlighted text to: {translatedText}");
        }
        
        if (pressedText != null)
        {
            pressedText.text = translatedText;
            Debug.Log($"[SyncButtonTexts] Updated Pressed text to: {translatedText}");
        }
        
        if (rebuildLayout)
        {
            ForceRebuildLayout();
        }
    }

    void ForceRebuildLayout()
    {
        Canvas.ForceUpdateCanvases();
        
        if (normalText != null)
        {
            ContentSizeFitter fitter = normalText.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(fitter.GetComponent<RectTransform>());
            }
        }
        
        if (highlightedText != null)
        {
            ContentSizeFitter fitter = highlightedText.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(fitter.GetComponent<RectTransform>());
            }
        }
        
        if (pressedText != null)
        {
            ContentSizeFitter fitter = pressedText.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(fitter.GetComponent<RectTransform>());
            }
        }
        
        HorizontalLayoutGroup hLayout = GetComponent<HorizontalLayoutGroup>();
        if (hLayout != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            Debug.Log($"[SyncButtonTexts] Rebuilt layout on {gameObject.name}");
        }
        
        VerticalLayoutGroup vLayout = GetComponent<VerticalLayoutGroup>();
        if (vLayout != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            Debug.Log($"[SyncButtonTexts] Rebuilt layout on {gameObject.name}");
        }
    }

    void OnDestroy()
    {
        UnsubscribeFromLanguageChange();
    }
}
