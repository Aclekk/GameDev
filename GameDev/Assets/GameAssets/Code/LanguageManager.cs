using UnityEngine;
using System.Collections.Generic;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    public enum Language
    {
        English,
        Indonesian
    }

    [Header("Settings")]
    public Language currentLanguage = Language.English;

    public delegate void LanguageChangedDelegate(Language newLanguage);
    public event LanguageChangedDelegate OnLanguageChanged;

    private Dictionary<string, Dictionary<Language, string>> translations;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeTranslations();
            currentLanguage = Language.English;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeTranslations()
    {
        translations = new Dictionary<string, Dictionary<Language, string>>();

        AddTranslation("play", "PLAY", "BERMAIN");
        AddTranslation("help", "HELP & GUIDE", "BANTUAN");
        AddTranslation("settings", "SETTINGS", "PENGATURAN");
        AddTranslation("exit_game", "EXIT", "KELUAR");
        AddTranslation("back", "BACK", "KEMBALI");
        AddTranslation("no", "NO", "TIDAK");
        AddTranslation("yes", "YES", "YA");

        AddTranslation("title_help", "HOW TO PLAY", "CARA BERMAIN");
        AddTranslation("title_settings", "SETTINGS", "PENGATURAN");
        
        AddTranslation("tab_video", "VIDEO", "VIDEO");
        AddTranslation("tab_audio", "AUDIO", "AUDIO");
        AddTranslation("tab_gameplay", "GAMEPLAY", "GAMEPLAY");
        AddTranslation("general", "GENERAL", "UMUM");
        AddTranslation("ui", "UI SCALE: ", "SKALA UI: ");
        
        AddTranslation("resolution", "RESOLUTION", "RESOLUSI");
        AddTranslation("quality", "QUALITY", "KUALITAS");
        AddTranslation("fullscreen", "FULLSCREEN", "LAYAR PENUH");
        AddTranslation("vsync", "VSYNC", "VSYNC");
        AddTranslation("language", "LANGUAGE:", "BAHASA:  ");
        AddTranslation("usedkey", "USED KEYS", "TOMBOL PINTAS");
        AddTranslation("previous", "PREVIOUS", "SEBELUMNYA");
        AddTranslation("next", "NEXT", "BERIKUTNYA");
        
        AddTranslation("master_volume", "MASTER VOLUME:", "VOLUME UTAMA:");
        AddTranslation("asset", "ASSET WE USE:", "ASSET YANG KAMI GUNAKAN:");
        AddTranslation("music_volume", "MUSIC VOLUME:", "VOLUME MUSIK:");
        AddTranslation("sfx_volume", "SFX VOLUME: ", "VOLUME EFEK: ");
        AddTranslation("yakin?", "ARE YOU SURE YOU WANT TO EXIT?", "APAKAH ANDA YAKIN INGIN KELUAR?");
        
        AddTranslation("sensitivity", "SENSITIVITY: ", "SENSITIVITAS: ");
        AddTranslation("invert_y", "INVERT Y AXIS", "BALIK SUMBU Y");
        AddTranslation("fov", "FIELD OF VIEW", "BIDANG PANDANG");
        
        AddTranslation("key_esc", "PAUSE MENU", "MENU JEDA");
        AddTranslation("key_f", "TOGGLE LANTERN", "NYALAKAN LENTERA");
        AddTranslation("key_wasd", "MOVEMENT", "BERGERAK");
        AddTranslation("key_shift", "SPRINT", "LARI");
        AddTranslation("key_mouse", "LOOK AROUND", "LIHAT SEKELILING");
        AddTranslation("key_e", "INTERACT", "INTERAKSI");
        
        AddTranslation("oil_warning", "OIL IS RUNNING LOW!", "MINYAK HAMPIR HABIS!");
        AddTranslation("door_locked", "DOOR IS LOCKED", "PINTU TERKUNCI");
        AddTranslation("key_found", "KEY FOUND!", "KUNCI DITEMUKAN!");
        AddTranslation("collect_item", "PRESS E TO COLLECT", "TEKAN E UNTUK AMBIL");
        
        AddTranslation("you_died", "YOU DIED", "KAMU MATI");
        AddTranslation("you_escaped", "YOU ESCAPED!", "KAMU BERHASIL KABUR!");
        AddTranslation("retry", "RETRY", "COBA LAGI");
        AddTranslation("main_menu", "MAIN MENU", "MENU UTAMA");
        
        AddTranslation("loading", "LOADING...", "MEMUAT...");
        AddTranslation("press_any_key", "PRESS ANY KEY TO CONTINUE", "TEKAN TOMBOL APAPUN UNTUK LANJUT");
        AddTranslation("continue", "CONTINUE", "LANJUTKAN");
        
        AddTranslation("lang_english", "English", "English");
        AddTranslation("lang_indonesian", "Indonesian", "Bahasa Indonesia");
    }

    void AddTranslation(string key, string english, string indonesian)
    {
        if (!translations.ContainsKey(key))
        {
            translations[key] = new Dictionary<Language, string>();
        }

        translations[key][Language.English] = english;
        translations[key][Language.Indonesian] = indonesian;
    }

    public string GetText(string key)
    {
        if (translations.ContainsKey(key))
        {
            if (translations[key].ContainsKey(currentLanguage))
            {
                return translations[key][currentLanguage];
            }
            else
            {
                Debug.LogWarning($"Translation for key '{key}' not found in language '{currentLanguage}'");
                return key;
            }
        }
        else
        {
            Debug.LogWarning($"Translation key '{key}' not found");
            return key;
        }
    }

    public void SetLanguage(Language language)
    {
        if (currentLanguage == language)
        {
            return;
        }

        currentLanguage = language;
        SaveLanguagePreference();
        
        Debug.Log($"Language changed to: {currentLanguage}");
        
        if (OnLanguageChanged != null)
        {
            OnLanguageChanged.Invoke(currentLanguage);
            Debug.Log($"Language change event invoked! Listeners: {OnLanguageChanged.GetInvocationList().Length}");
        }
        else
        {
            Debug.LogWarning("OnLanguageChanged has no listeners!");
        }
        
        ForceRefreshAllLocalizedTexts();
    }

    void ForceRefreshAllLocalizedTexts()
    {
        LocalizedText[] allLocalizedTexts = FindObjectsOfType<LocalizedText>(true);
        foreach (LocalizedText localizedText in allLocalizedTexts)
        {
            localizedText.UpdateText();
        }
        Debug.Log($"Force refreshed {allLocalizedTexts.Length} localized texts");
    }

    public void SetLanguage(int languageIndex)
    {
        if (languageIndex >= 0 && languageIndex < System.Enum.GetValues(typeof(Language)).Length)
        {
            SetLanguage((Language)languageIndex);
        }
    }

    void SaveLanguagePreference()
    {
        PlayerPrefs.SetInt("GameLanguage", (int)currentLanguage);
        PlayerPrefs.Save();
    }

    void LoadLanguagePreference()
    {
        if (PlayerPrefs.HasKey("GameLanguage"))
        {
            int savedLanguage = PlayerPrefs.GetInt("GameLanguage");
            currentLanguage = (Language)savedLanguage;
        }
        else
        {
            currentLanguage = Language.English;
        }
    }

    public Language GetCurrentLanguage()
    {
        return currentLanguage;
    }

    public string[] GetLanguageNames()
    {
        return new string[] 
        { 
            GetText("lang_english"), 
            GetText("lang_indonesian") 
        };
    }
}
