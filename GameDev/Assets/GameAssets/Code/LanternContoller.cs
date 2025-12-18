using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class LanternController : MonoBehaviour
{
    [Header("Referensi")]
    public Light lanternLight;
    public AudioSource audioSource;
    public AudioClip sfxIgnite;
    public AudioClip sfxExtinguish;

    [Header("Startup")]
    public bool startOn = true;

    [Header("Stat Minyak (Oil)")]
    public float maxOil = 100f;
    public float currentOil = 100f;
    public float consumptionPerSecond = 1.5f;

    [Header("Lampu")]
    public float maxIntensity = 2.2f;
    public float minIntensity = 0.3f;
    public float maxRange = 8f;
    public float minRange = 2f;

    [Header("Events (Opsional untuk UI)")]
    public UnityEvent<float> OnOilPercentChanged;
    public UnityEvent<bool> OnLanternToggled;

    [Header("UI Warning Settings")]
    public GameObject lowOilWarningUI;   // UI Warning Text / TMP
    public float warningThreshold = 0.40f; // 40%
    public float blinkSpeed = 3f; // semakin besar semakin cepat berkedip

    Coroutine blinkCo;

    public bool IsOn { get; private set; } = false;

    public bool IsLit => IsOn && currentOil > 0f && lanternLight != null && lanternLight.enabled;

    public float CurrentLightRange => (lanternLight != null && lanternLight.enabled) ? lanternLight.range : 0f;

    void Start()
    {
        if (lanternLight == null)
            lanternLight = GetComponentInChildren<Light>(true);

        currentOil = Mathf.Clamp(currentOil, 0f, maxOil);

        ApplyLightByOil();
        SetLantern(startOn, true);

        // Hide warning UI at start
        if (lowOilWarningUI)
            lowOilWarningUI.SetActive(false);
    }

    void Update()
    {
        if (IsOn && currentOil <= 0f)
            SetLantern(false);

        if (IsOn && currentOil > 0f)
        {
            currentOil -= consumptionPerSecond * Time.deltaTime;
            currentOil = Mathf.Max(0f, currentOil);

            if (currentOil <= 0f)
                SetLantern(false);
        }

        ApplyLightByOil();
        UpdateLowOilWarning();
    }

    public void AddOil(float amount)
    {
        currentOil = Mathf.Clamp(currentOil + amount, 0f, maxOil);
        ApplyLightByOil();
        OnOilPercentChanged?.Invoke(currentOil / maxOil);
    }

    public void ToggleLantern()
    {
        SetLantern(!IsOn);
    }

    void SetLantern(bool on, bool instant = false)
    {
        if (on && currentOil <= 0f)
            on = false;

        if (IsOn == on && !instant) return;

        IsOn = on;

        if (audioSource != null)
        {
            AudioClip clip = on ? sfxIgnite : sfxExtinguish;
            if (clip != null) audioSource.PlayOneShot(clip);
        }

        if (lanternLight != null)
            lanternLight.enabled = IsOn && currentOil > 0f;

        OnLanternToggled?.Invoke(IsOn);
        ApplyLightByOil();
    }

    void ApplyLightByOil()
    {
        if (lanternLight == null) return;

        if (!IsOn || currentOil <= 0f)
        {
            lanternLight.enabled = false;
            return;
        }

        lanternLight.enabled = true;

        float oilPercent = Mathf.Clamp01(currentOil / maxOil);
        float intensityCurve = Mathf.Pow(oilPercent, 0.7f);
        float rangeCurve = Mathf.Pow(oilPercent, 0.5f);

        lanternLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, intensityCurve);
        lanternLight.range = Mathf.Lerp(minRange, maxRange, rangeCurve);

        OnOilPercentChanged?.Invoke(oilPercent);
    }

    // ==============================
    //      LOW OIL WARNING SYSTEM
    // ==============================
    void UpdateLowOilWarning()
    {
        if (lowOilWarningUI == null) return;

        float oilPercent = currentOil / maxOil;

        bool shouldShow =
            IsOn &&
            oilPercent <= warningThreshold &&
            currentOil > 0f;

        if (shouldShow)
        {
            lowOilWarningUI.SetActive(true);

            if (blinkCo == null)
                blinkCo = StartCoroutine(BlinkWarning());
        }
        else
        {
            lowOilWarningUI.SetActive(false);

            if (blinkCo != null)
            {
                StopCoroutine(blinkCo);
                blinkCo = null;
            }
        }
    }

    IEnumerator BlinkWarning()
    {
        Graphic uiGraphic = lowOilWarningUI.GetComponent<Graphic>();
        TMP_Text tmp = lowOilWarningUI.GetComponent<TMP_Text>();

        while (true)
        {
            float alpha = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;

            Color c = Color.red;
            c.a = alpha;

            if (uiGraphic != null)
                uiGraphic.color = c;

            if (tmp != null)
                tmp.color = c;

            yield return null;
        }
    }

    public float GetVisibilityStrength()
    {
        if (!IsOn || currentOil <= 0f) return 0f;
        return currentOil / maxOil;
    }
}
