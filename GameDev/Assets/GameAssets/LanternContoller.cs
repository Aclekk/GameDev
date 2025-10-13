using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LanternController : MonoBehaviour
{
    [Header("Referensi")]
    public Light lanternLight;        // drag komponen Light (Spot / Point)
    public AudioSource audioSource;   // opsional: SFX nyala / mati
    public AudioClip sfxIgnite;
    public AudioClip sfxExtinguish;

    [Header("Startup")]
    public bool startOn = true;       // ← mulai ON

    [Header("Stat Minyak (Oil)")]
    public float maxOil = 100f;
    public float currentOil = 100f;
    public float consumptionPerSecond = 1.0f;

    [Header("Lampu")]
    public float maxIntensity = 2.2f;
    public float minIntensity = 0.0f;
    public float maxRange = 100f;     // ← range awal 100
    public float minRange = 0f;

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.F;

    [Header("Events (Opsional untuk UI)")]
    public UnityEvent<float> OnOilPercentChanged; // 0..1
    public UnityEvent<bool> OnLanternToggled;     // true=ON

    public bool IsOn { get; private set; } = false;

    void Start()
    {
        if (lanternLight == null)
            lanternLight = GetComponentInChildren<Light>(true);

        // pastikan ada minyak (biar t=1 → range=maxRange)
        currentOil = Mathf.Clamp(currentOil, 0f, maxOil);

        ApplyLightByOil();            // set intensity & range berdasar oil
        SetLantern(startOn, true);    // ← langsung ON di awal kalau startOn = true
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            SetLantern(!IsOn);

        if (IsOn && currentOil > 0f)
        {
            currentOil -= consumptionPerSecond * Time.deltaTime;
            currentOil = Mathf.Max(0f, currentOil);
            if (currentOil <= 0f) SetLantern(false); // mati otomatis saat habis
        }

        ApplyLightByOil();
    }

    public void AddOil(float amount)
    {
        currentOil = Mathf.Clamp(currentOil + amount, 0f, maxOil);
        ApplyLightByOil();
        OnOilPercentChanged?.Invoke(currentOil / maxOil);
    }

    void SetLantern(bool on, bool instant = false)
    {
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

        // t=1 saat oil penuh → intensity=maxIntensity & range=maxRange (100)
        float t = (maxOil <= 0f) ? 0f : Mathf.Clamp01(currentOil / maxOil);
        lanternLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        lanternLight.range = Mathf.Lerp(minRange, maxRange, t);

        OnOilPercentChanged?.Invoke(t);
    }

    public float GetVisibilityStrength()
    {
        if (!IsOn || currentOil <= 0f) return 0f;
        return currentOil / maxOil;
    }
}
