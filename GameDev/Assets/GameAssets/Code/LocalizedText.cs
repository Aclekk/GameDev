using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [Header("Translation Key")]
    [Tooltip("Key untuk mencari translation di LanguageManager")]
    public string translationKey;

    [Header("Optional")]
    public bool updateOnEnable = true;
    
    [Header("Layout")]
    [Tooltip("Force rebuild layout after text update")]
    public bool rebuildLayout = true;

    private TextMeshProUGUI textComponent;
    private bool isSubscribed = false;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        SubscribeToLanguageChange();
        
        if (updateOnEnable)
        {
            UpdateText();
        }
    }

    void OnDisable()
    {
        UnsubscribeFromLanguageChange();
    }

    void Start()
    {
        SubscribeToLanguageChange();
        UpdateText();
    }

    void SubscribeToLanguageChange()
    {
        if (!isSubscribed && LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            LanguageManager.Instance.OnLanguageChanged += OnLanguageChanged;
            isSubscribed = true;
        }
    }

    void UnsubscribeFromLanguageChange()
    {
        if (isSubscribed && LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            isSubscribed = false;
        }
    }

    void OnLanguageChanged(LanguageManager.Language newLanguage)
    {
        UpdateText();
    }

    public void UpdateText()
    {
        if (string.IsNullOrEmpty(translationKey))
        {
            Debug.LogWarning($"[LocalizedText] Translation key is empty on {gameObject.name}", this);
            return;
        }

        if (LanguageManager.Instance == null)
        {
            Debug.LogWarning($"[LocalizedText] LanguageManager not found for {gameObject.name}", this);
            return;
        }

        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();

        string translatedText = LanguageManager.Instance.GetText(translationKey);
        
        Debug.Log($"[LocalizedText] Updating '{gameObject.name}' | Key: '{translationKey}' | Text: '{translatedText}' | Old: '{textComponent.text}'");
        
        textComponent.text = translatedText;
        
        if (rebuildLayout)
        {
            ForceRebuildLayout();
        }
        
        Debug.Log($"[LocalizedText] After update: '{textComponent.text}'");
    }

    void ForceRebuildLayout()
    {
        Canvas.ForceUpdateCanvases();
        
        ContentSizeFitter fitter = GetComponent<ContentSizeFitter>();
        if (fitter != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(fitter.GetComponent<RectTransform>());
        }
        
        Transform parent = transform.parent;
        while (parent != null)
        {
            HorizontalLayoutGroup hLayout = parent.GetComponent<HorizontalLayoutGroup>();
            if (hLayout != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());
                Debug.Log($"[LocalizedText] Rebuilt HorizontalLayoutGroup on {parent.name}");
            }
            
            VerticalLayoutGroup vLayout = parent.GetComponent<VerticalLayoutGroup>();
            if (vLayout != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());
                Debug.Log($"[LocalizedText] Rebuilt VerticalLayoutGroup on {parent.name}");
            }
            
            parent = parent.parent;
        }
    }

    public void SetKey(string newKey)
    {
        translationKey = newKey;
        UpdateText();
    }

    void OnDestroy()
    {
        UnsubscribeFromLanguageChange();
    }
}
