# 🚀 Quick Setup - Localization System

## 📋 5 Menit Setup!

### **Step 1: Buat LanguageManager (2 menit)**

1. Di **MainMenu scene**
2. **Klik kanan** di Hierarchy > Create Empty
3. Rename jadi **"LanguageManager"**
4. **Add Component** > LanguageManager
5. Set **Current Language** = English
6. ✅ Done!

---

### **Step 2: Setup Language Dropdown (1 menit)**

1. Di Hierarchy, cari: **Canvas > Main Panels > Settings > Content > Gameplay > Content > List > Language > Dropdown**
2. **Select Dropdown** GameObject
3. **Add Component** > LanguageDropdown
4. ✅ Done! (Script akan otomatis detect CustomDropdown component)

---

### **Step 3: Setup Text di Main Menu (2 menit)**

**Cara cepat pakai search:**

#### **Play Button:**
1. Hierarchy search: "Play" → pilih yang ada Text component
2. Select **salah satu Text** (Normal atau Highlighted)
3. **Add Component** > LocalizedText
4. Set **Translation Key** = `play`
5. **Copy component** (klik gear icon > Copy Component)
6. Select **Text yang lain** (Highlighted/Normal)
7. Paste (klik gear icon > Paste Component Values)

#### **Help Button:**
Ulangi step di atas dengan **Translation Key** = `help`

#### **Settings Button:**
Translation Key = `settings`

#### **Exit Game Button:**
Translation Key = `exit_game`

---

### **Step 4: Test! (30 detik)**

1. **Play** MainMenu scene
2. Buka **Settings** > **Gameplay**
3. Klik **Language dropdown**
4. Pilih **"Bahasa Indonesia"**
5. **Semua text berubah!** ✅

---

## 🎯 Yang Sudah Berfungsi Setelah Setup:

| UI Element | English | Indonesian |
|-----------|---------|-----------|
| Play button | Play | Bermain |
| Help button | Help | Bantuan |
| Settings button | Settings | Pengaturan |
| Exit button | Exit Game | Keluar |
| Language dropdown | English / Indonesian | English / Bahasa Indonesia |

---

## 📝 Common Translation Keys

Copy-paste ini untuk setup cepat:

**Main Menu:**
- `play` → Play / Bermain
- `help` → Help / Bantuan
- `settings` → Settings / Pengaturan
- `exit_game` → Exit Game / Keluar
- `back` → Back / Kembali

**Settings Labels:**
- `resolution` → Resolution / Resolusi
- `quality` → Quality / Kualitas
- `fullscreen` → Fullscreen / Layar Penuh
- `language` → Language / Bahasa
- `master_volume` → Master Volume / Volume Utama
- `music_volume` → Music Volume / Volume Musik
- `sfx_volume` → SFX Volume / Volume Efek

**In-Game:**
- `oil_warning` → Oil is running low! / Minyak hampir habis!
- `door_locked` → Door is locked / Pintu terkunci
- `collect_item` → Press E to collect / Tekan E untuk ambil
- `you_died` → You Died / Kamu Mati
- `you_escaped` → You Escaped! / Kamu Berhasil Kabur!
- `retry` → Retry / Coba Lagi
- `main_menu` → Main Menu / Menu Utama

---

## 💡 Pro Tip: Bulk Setup

Kalau mau setup banyak text sekaligus:

1. Create Empty GameObject "SetupHelper"
2. Add Component > LocalizationSetupHelper
3. Set mappings untuk semua text sekaligus
4. Klik kanan script > "Setup All Localized Texts"
5. Delete SetupHelper GameObject

---

## ✅ Checklist

Minimal setup untuk language switching:

- [ ] ✅ LanguageManager GameObject created
- [ ] ✅ LanguageDropdown added to dropdown
- [ ] ✅ LocalizedText added to Play button text
- [ ] ✅ LocalizedText added to Help button text  
- [ ] ✅ LocalizedText added to Settings button text
- [ ] ✅ LocalizedText added to Exit button text
- [ ] ✅ Test switching language

**Setelah ini, tombol Language sudah berfungsi!** 🎉

---

**Next steps (optional):**
- Add LocalizedText ke semua text di Settings menu
- Add LocalizedText ke Help/Controls text
- Add in-game messages localization di GameKEL9 scene
