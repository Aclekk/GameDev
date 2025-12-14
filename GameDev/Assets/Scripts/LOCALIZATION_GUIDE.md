# 🌐 Localization System Guide

## 📋 System Overview

Sistem localization untuk mendukung **multi-bahasa** di game kamu:
- ✅ **English** (default)
- ✅ **Bahasa Indonesia**

## 🎯 Components

### **1. LanguageManager.cs**
Manager utama untuk handle bahasa dan translations.

**Features:**
- Singleton pattern (persist across scenes)
- Save/load language preference
- Event system untuk update semua text
- Dictionary-based translation storage

### **2. LocalizedText.cs**
Component yang ditambahkan ke setiap TextMeshProUGUI yang perlu di-translate.

**Features:**
- Auto-update saat bahasa berubah
- Translation key system
- Easy to setup di Inspector

### **3. LanguageDropdown.cs**
Handle dropdown untuk switch bahasa di Settings menu.

**Compatible with:**
- ✅ Michsky Dark UI CustomDropdown
- ✅ Standard TMP_Dropdown (with modifications)

**Features:**
- Auto-populate dengan list bahasa
- Save preference saat berubah
- Sync dengan LanguageManager
- Works with existing CustomDropdown component

---

## 🚀 Quick Setup

### **Step 1: Setup LanguageManager**

1. **Buat Empty GameObject** "LanguageManager" di scene **MainMenu**
2. **Add Component** > LanguageManager
3. **Set Current Language** = English (default)
4. Done! LanguageManager akan persist ke scene lain (DontDestroyOnLoad)

### **Step 2: Setup Language Dropdown**

1. **Select** GameObject: `/Canvas/Main Panels/Settings/Content/Gameplay/Content/List/Language/Dropdown`
2. **Add Component** > LanguageDropdown
3. Done! Dropdown sekarang berfungsi untuk switch bahasa

### **Step 3: Setup Text Components**

Ada 2 cara:

#### **Cara Manual (Per Text):**
```
1. Select text GameObject (contoh: Play button text)
2. Add Component > LocalizedText
3. Set Translation Key = "play"
4. Done!
```

#### **Cara Otomatis (Semua Text Sekaligus):**
```
1. Buat Empty GameObject "LocalizationHelper"
2. Add Component > LocalizationSetupHelper
3. Klik arrow di "Mappings" untuk expand
4. Set Size = jumlah text yang mau di-setup
5. Isi mappings (contoh di bawah)
6. Klik kanan script > Context Menu > "Setup All Localized Texts"
```

---

## 📝 Translation Keys Reference

### **Main Menu Buttons:**
| GameObject Path | Translation Key |
|----------------|----------------|
| Play button text | `play` |
| Help button text | `help` |
| Settings button text | `settings` |
| Exit Game button text | `exit_game` |
| Back button text | `back` |

### **Settings Menu:**
| UI Element | Translation Key |
|-----------|----------------|
| Resolution label | `resolution` |
| Quality label | `quality` |
| Fullscreen label | `fullscreen` |
| VSync label | `vsync` |
| Language label | `language` |
| Master Volume | `master_volume` |
| Music Volume | `music_volume` |
| SFX Volume | `sfx_volume` |
| Sensitivity | `sensitivity` |
| Invert Y Axis | `invert_y` |
| Field of View | `fov` |

### **Help/Controls:**
| Control | Translation Key |
|---------|----------------|
| Pause Menu | `key_esc` |
| Toggle Lantern | `key_f` |
| Movement | `key_wasd` |
| Sprint | `key_shift` |
| Look Around | `key_mouse` |
| Interact | `key_e` |

### **In-Game Messages:**
| Message | Translation Key |
|---------|----------------|
| Oil warning | `oil_warning` |
| Door locked | `door_locked` |
| Key found | `key_found` |
| Collect item | `collect_item` |

### **Game Over/Win:**
| Text | Translation Key |
|------|----------------|
| You Died | `you_died` |
| You Escaped | `you_escaped` |
| Retry | `retry` |
| Main Menu | `main_menu` |

---

## 🔧 Adding New Translations

### **Method 1: Edit LanguageManager.cs**

Tambahkan di fungsi `InitializeTranslations()`:

```csharp
AddTranslation("new_key", "English Text", "Teks Indonesia");
```

**Example:**
```csharp
AddTranslation("pause", "Pause", "Jeda");
AddTranslation("resume", "Resume", "Lanjutkan");
AddTranslation("objective", "Find the key and escape", "Cari kunci dan kabur");
```

### **Method 2: Runtime (Advanced)**

Bisa juga add translation via script:

```csharp
LanguageManager.Instance.AddTranslation("dynamic_key", "English", "Indonesian");
```

---

## 💻 Usage in Scripts

### **Get Translated Text:**

```csharp
using UnityEngine;

public class MyScript : MonoBehaviour
{
    void Start()
    {
        string text = LanguageManager.Instance.GetText("play");
        Debug.Log(text); // "Play" atau "Bermain"
    }
}
```

### **Change Language Programmatically:**

```csharp
// Method 1: By enum
LanguageManager.Instance.SetLanguage(LanguageManager.Language.Indonesian);

// Method 2: By index (0=English, 1=Indonesian)
LanguageManager.Instance.SetLanguage(1);
```

### **Listen to Language Change Events:**

```csharp
using UnityEngine;

public class MyScript : MonoBehaviour
{
    void OnEnable()
    {
        LanguageManager.Instance.OnLanguageChanged += OnLanguageChanged;
    }

    void OnDisable()
    {
        LanguageManager.Instance.OnLanguageChanged -= OnLanguageChanged;
    }

    void OnLanguageChanged(LanguageManager.Language newLanguage)
    {
        Debug.Log($"Language changed to: {newLanguage}");
        // Update custom UI elements here
    }
}
```

### **Update LocalizedText Dynamically:**

```csharp
LocalizedText localizedText = GetComponent<LocalizedText>();
localizedText.SetKey("new_translation_key");
```

---

## 🎨 Example Mappings for LocalizationSetupHelper

```
Size: 8

Element 0:
  Game Object Path: Text (Play button)
  Translation Key: play

Element 1:
  Game Object Path: Text (Help button)
  Translation Key: help

Element 2:
  Game Object Path: Text (Settings button)
  Translation Key: settings

Element 3:
  Game Object Path: Text (Exit button)
  Translation Key: exit_game

Element 4:
  Game Object Path: Title (Help panel)
  Translation Key: title_help

Element 5:
  Game Object Path: Title (Settings panel)
  Translation Key: title_settings

... dan seterusnya
```

---

## 🧪 Testing

### **Test Language Switch:**
1. Play game di MainMenu
2. Buka Settings > Gameplay
3. Klik Language dropdown
4. Pilih "Bahasa Indonesia"
5. **Semua text seharusnya langsung berubah!**
6. Close Settings, buka lagi → bahasa tetap Indonesia (saved via PlayerPrefs)

### **Test Persistence:**
1. Set language ke Indonesian
2. Exit play mode
3. Play again
4. Language masih Indonesian ✅

---

## ❌ Troubleshooting

### **"LanguageManager instance not found!"**
**Problem**: LanguageManager belum ada di scene  
**Fix**: Buat GameObject dengan LanguageManager script di MainMenu scene

### **Text tidak berubah saat ganti bahasa**
**Problem**: LocalizedText component belum ditambahkan  
**Fix**: Add LocalizedText component ke text GameObject dan set translation key

### **Dropdown kosong**
**Problem**: LanguageDropdown script belum ada atau LanguageManager belum initialized  
**Fix**: 
1. Pastikan LanguageManager ada di scene dan Awake sudah dipanggil
2. Add LanguageDropdown script ke Dropdown GameObject

### **Translation key tidak ditemukan**
**Problem**: Key typo atau belum ditambahkan di LanguageManager  
**Fix**: Check spelling atau tambahkan translation di InitializeTranslations()

---

## 🌍 Adding More Languages

Untuk tambah bahasa baru (contoh: Japanese):

### **Step 1: Update Enum**
```csharp
public enum Language
{
    English,
    Indonesian,
    Japanese  // NEW
}
```

### **Step 2: Update AddTranslation**
```csharp
void AddTranslation(string key, string english, string indonesian, string japanese)
{
    if (!translations.ContainsKey(key))
    {
        translations[key] = new Dictionary<Language, string>();
    }

    translations[key][Language.English] = english;
    translations[key][Language.Indonesian] = indonesian;
    translations[key][Language.Japanese] = japanese;
}
```

### **Step 3: Update GetLanguageNames**
```csharp
public string[] GetLanguageNames()
{
    return new string[] 
    { 
        GetText("lang_english"), 
        GetText("lang_indonesian"),
        GetText("lang_japanese")  // NEW
    };
}
```

### **Step 4: Add lang_japanese translation**
```csharp
AddTranslation("lang_japanese", "Japanese", "Bahasa Jepang", "日本語");
```

---

## 📊 Current Translations

Total keys: **40+**

**Supported languages:**
- 🇬🇧 English
- 🇮🇩 Bahasa Indonesia

**Categories:**
- Main Menu (5 keys)
- Settings (11 keys)
- Controls (6 keys)
- In-Game Messages (4 keys)
- Game Over/Win (4 keys)
- Misc (10+ keys)

---

## ✅ Checklist

Setup localization untuk game:

- [ ] Create LanguageManager GameObject di MainMenu
- [ ] Add LanguageManager script
- [ ] Add LanguageDropdown script ke Language dropdown
- [ ] Add LocalizedText ke semua UI text yang perlu di-translate
- [ ] Set translation keys untuk setiap LocalizedText
- [ ] Test language switching
- [ ] Test persistence (bahasa tetap setelah restart)
- [ ] Add translations untuk in-game messages di GameKEL9 scene

---

**Updated**: 2024  
**Scripts**: LanguageManager.cs, LocalizedText.cs, LanguageDropdown.cs  
**Feature**: Multi-language support (English & Indonesian)
