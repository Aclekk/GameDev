using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PromptUI : MonoBehaviour
{
    [Header("Assign salah satu")]
    public Text uiText;       // UnityEngine.UI.Text
    public TMP_Text tmpText;  // TextMeshProUGUI

    [Header("Fade (opsional)")]
    public CanvasGroup canvasGroup;
    public float fadeSpeed = 12f;

    string _targetText = "";
    float _targetAlpha = 0f;

    void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        SetText("");
        if (canvasGroup) canvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (canvasGroup)
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, _targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
    }

    public void Show(string msg)
    {
        _targetText = msg;
        SetText(_targetText);
        _targetAlpha = 1f;
    }

    public void Hide()
    {
        _targetText = "";
        _targetAlpha = 0f;
        SetText("");
    }

    void SetText(string msg)
    {
        if (uiText)  uiText.text  = msg;
        if (tmpText) tmpText.text = msg;
    }
}
