using UnityEngine;
using UHFPS.Runtime;

public class LanternOilWarning : MonoBehaviour
{
    [Header("References")]
    public LanternItem lanternItem;
    public Canvas warningCanvas;
    
    [Header("Warning Settings")]
    [Range(0f, 1f)]
    public float lowOilThreshold = 0.2f;
    
    [Range(0f, 1f)]
    public float criticalOilThreshold = 0.1f;
    
    [Header("Fade Settings")]
    public float fadeSpeed = 2f;
    public float normalAlpha = 0.5f;
    public float criticalAlpha = 1f;
    
    [Header("Blink Settings")]
    public bool enableBlinkOnCritical = true;
    public float blinkSpeed = 2f;
    
    private CanvasGroup canvasGroup;
    private float targetAlpha;
    private float currentAlpha;
    private bool isWarningActive;
    private bool isBlinking;
    
    void Awake()
    {
        if (lanternItem == null)
            lanternItem = FindObjectOfType<LanternItem>();
        
        if (warningCanvas == null)
        {
            Debug.LogError("[LanternOilWarning] Warning Canvas tidak di-assign!");
            enabled = false;
            return;
        }
        
        canvasGroup = warningCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = warningCanvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0f;
        currentAlpha = 0f;
        targetAlpha = 0f;
        warningCanvas.gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (lanternItem == null)
            return;
        
        float oilLevel = lanternItem.lanternFuel;
        
        if (oilLevel <= criticalOilThreshold)
        {
            ShowWarning(true);
            targetAlpha = criticalAlpha;
            isBlinking = enableBlinkOnCritical;
        }
        else if (oilLevel <= lowOilThreshold)
        {
            ShowWarning(false);
            targetAlpha = normalAlpha;
            isBlinking = false;
        }
        else
        {
            HideWarning();
            isBlinking = false;
        }
        
        UpdateWarningVisual();
    }
    
    void ShowWarning(bool isCritical)
    {
        if (!isWarningActive)
        {
            isWarningActive = true;
            warningCanvas.gameObject.SetActive(true);
        }
    }
    
    void HideWarning()
    {
        if (isWarningActive)
        {
            isWarningActive = false;
            targetAlpha = 0f;
        }
    }
    
    void UpdateWarningVisual()
    {
        float finalTargetAlpha = targetAlpha;
        
        if (isBlinking && isWarningActive)
        {
            float blinkValue = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            finalTargetAlpha = Mathf.Lerp(0.3f, targetAlpha, blinkValue);
        }
        
        currentAlpha = Mathf.Lerp(currentAlpha, finalTargetAlpha, Time.deltaTime * fadeSpeed);
        canvasGroup.alpha = currentAlpha;
        
        if (!isWarningActive && currentAlpha <= 0.01f)
        {
            canvasGroup.alpha = 0f;
            warningCanvas.gameObject.SetActive(false);
        }
    }
}
