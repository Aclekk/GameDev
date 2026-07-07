<div align="center">

<img src="https://readme-typing-svg.demolab.com?font=Creepster&size=28&duration=4000&pause=1200&color=8B0000&center=true&vCenter=true&width=650&lines=Echoes+of+The+Old+House;First-Person+Survival+Horror;Built+with+Unity+%26+URP;Some+doors+should+stay+closed..." alt="Typing SVG" />

<br/>

![Unity](https://img.shields.io/badge/Unity-2022.3.62f2-8B0000?style=for-the-badge&logo=unity&logoColor=white)
![URP](https://img.shields.io/badge/Render%20Pipeline-URP-1a1a1a?style=for-the-badge&logo=unity&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-PC-2b0000?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-In%20Development-800000?style=for-the-badge)
![License](https://img.shields.io/badge/License-Educational-1a1a1a?style=for-the-badge)

</div>

---

## 🕯️ Tentang Game

**Echoes of The Old House** adalah game horor **first-person survival** yang membawa pemain menjelajahi sebuah rumah tua yang berhantu. Bersenjatakan hanya sebuah **lentera** yang cahayanya bisa padam kapan saja, pemain harus mengumpulkan item, memecahkan puzzle, dan menghindari sesuatu yang terus mengintai dari kegelapan — sebelum rumah itu menelan mereka juga.

> *"Setiap sudut menyimpan gema masa lalu. Jangan biarkan mereka menemukanmu."*

<div align="center">

### 🩸 Fitur Utama

</div>

| 🎯 Fitur | 📋 Deskripsi |
|:---|:---|
| 🔦 **Sistem Lentera** | Satu-satunya sumber cahaya — bahan bakarnya terbatas dan bisa habis |
| 🎒 **Inventory System** | Kumpulkan kunci, minyak, dan item penting lainnya |
| 👻 **AI Hantu** | Entitas yang memburu pemain dan memicu jumpscare saat tertangkap |
| 🚪 **Puzzle & Interaksi Pintu** | Basement, escape door, dan interaksi objek dinamis |
| 💾 **Save / Pause / Menu** | Sistem menu lengkap dengan main menu & pause manager |
| 🌐 **Multi-Bahasa** | Dukungan penuh Bahasa Indonesia & English |

---

## 🗂️ Struktur Proyek

```
GameDev/
├── Assets/
│   ├── GameAssets/Code/       # Script inti gameplay (player, hantu, inventory, UI, localization)
│   ├── Scripts/               # Script tambahan/bridge (lantern auto-equip, integrasi UHFPS, dsb.)
│   ├── UHFPS/                 # Ultimate Horror FPS System (asset store)
│   ├── AdvancedMobileHorror/  # Advanced Mobile Horror FPS System (asset store)
│   ├── Hantu/                 # Model & animasi karakter hantu
│   ├── modular-dungeon/       # Modular dungeon/mansion tileset
│   ├── Cemetery Kit V1.25/    # Asset kuburan
│   ├── Dark UI/               # UI kit tema gelap
│   ├── MainMenuAsset/         # Aset main menu
│   ├── Scenes/                # Scene: MainMenu, GameKEL9, Coba (testing)
│   └── ...                    # Asset pendukung lain (audio, materials, dsb.)
├── Packages/                  # Unity Package Manager dependencies
└── ProjectSettings/           # Konfigurasi project Unity
```

---

## 🧩 Script Utama

<div align="center">

| Script | Fungsi |
|:---|:---|
| `FirstPersonController.cs` / `HeroController.cs` | Kontrol pergerakan & kamera pemain |
| `HantuMove.cs` / `HantuJumpscare.cs` | AI pergerakan hantu & trigger jumpscare |
| `Inventory.cs` / `InventorySystem.cs` | Sistem penyimpanan item pemain |
| `KeyPickup.cs` / `PetrolOilPickup.cs` / `PickupItem.cs` | Pengambilan item di dunia game |
| `LanternContoller.cs` / `LanternToggle.cs` / `LanternOilWarning.cs` | Sistem lentera (cahaya utama & bahan bakar) |
| `BasementDoor.cs` / `EscapeDoor.cs` / `Door2BlackInteraction.cs` | Interaksi pintu & transisi scene |
| `PauseManager.cs` / `PlayButton.cs` / `ExitButton.cs` | Sistem menu & kontrol game |
| `LanguageManager.cs` / `LocalizedText.cs` / `LanguageDropdown.cs` | Sistem localization EN/ID |
| `TutorialManager.cs` | Panduan/tutorial untuk pemain baru |

</div>

---

## 🛠️ Tech Stack

<div align="center">

![C#](https://img.shields.io/badge/C%23-1a1a1a?style=for-the-badge&logo=csharp&logoColor=8B0000)
![Unity](https://img.shields.io/badge/Unity-000000?style=for-the-badge&logo=unity&logoColor=8B0000)
![Cinemachine](https://img.shields.io/badge/Cinemachine-2b0000?style=for-the-badge)
![ProBuilder](https://img.shields.io/badge/ProBuilder-1a1a1a?style=for-the-badge)
![TextMeshPro](https://img.shields.io/badge/TextMeshPro-800000?style=for-the-badge)

</div>

- **Engine:** Unity 2022.3.62f2 · Universal Render Pipeline (URP) 14.0.12
- **Package pendukung:** Cinemachine · ProBuilder · TextMeshPro · AI Navigation (NavMesh) · Input System · Timeline
- **Asset toolkit horror:** UHFPS (Ultimate Horror FPS System) · Advanced Mobile Horror FPS System

---

## 🚀 Cara Menjalankan

```bash
# 1. Clone repository ini
git clone https://github.com/Aclekk/GameDev.git

# 2. Buka folder GameDev via Unity Hub
#    Gunakan editor versi 2022.3.62f2 (atau 2022.3 LTS terdekat)

# 3. Buka scene entry point
Assets/Scenes/MainMenu.unity

# 4. Tekan ▶️ Play — dan berdoa lampunya nggak mati duluan
```

---

## 📝 Catatan

- 🎬 Scene utama gameplay: `GameKEL9.unity`
- 🧪 Scene `Coba.unity` digunakan untuk keperluan testing/eksperimen
- 🌐 Dokumentasi localization lengkap ada di `LOCALIZATION_FILES.md` (folder `Assets/GameAssets/Code/`)

<div align="center">

---

🕯️ *The house remembers everyone who enters.* 🕯️

Made in the dark by **Kelompok 9**

</div>
