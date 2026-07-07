<div align="center">

<img src="https://readme-typing-svg.demolab.com?font=Fira+Code&size=28&duration=3000&pause=1000&color=F5A623&center=true&vCenter=true&width=600&lines=Mansion+Horror+%E2%80%94+KEL+9;First-Person+Survival+Horror;Built+with+Unity+%26+URP;Explore.+Survive.+Escape." alt="Typing SVG" />

<br/>

![Unity](https://img.shields.io/badge/Unity-2022.3.62f2-000000?style=for-the-badge&logo=unity&logoColor=white)
![URP](https://img.shields.io/badge/Render%20Pipeline-URP-333333?style=for-the-badge&logo=unity&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-PC-informational?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-In%20Development-yellow?style=for-the-badge)
![License](https://img.shields.io/badge/License-Educational-blue?style=for-the-badge)

</div>

---

## 🏚️ Tentang Game

**Echoes of The Old House** adalah game horor **first-person survival** yang membawa pemain menjelajahi sebuah rumah tua (mansion) yang berhantu. Bersenjatakan hanya sebuah **lentera** sebagai sumber cahaya, pemain harus mengumpulkan item, memecahkan puzzle, dan menghindari kejaran hantu untuk menemukan jalan keluar.

<div align="center">

### ✨ Fitur Utama

</div>

| 🎯 Fitur | 📋 Deskripsi |
|:---|:---|
| 🔦 **Sistem Lentera** | Sumber cahaya utama dengan mekanik bahan bakar (oil) yang bisa habis |
| 🎒 **Inventory System** | Kumpulkan kunci, minyak, dan item penting lainnya |
| 👻 **AI Hantu** | Musuh yang mengejar pemain dan memicu jumpscare |
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

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Unity](https://img.shields.io/badge/Unity-100000?style=for-the-badge&logo=unity&logoColor=white)
![Cinemachine](https://img.shields.io/badge/Cinemachine-1E90FF?style=for-the-badge)
![ProBuilder](https://img.shields.io/badge/ProBuilder-8A2BE2?style=for-the-badge)
![TextMeshPro](https://img.shields.io/badge/TextMeshPro-FF6347?style=for-the-badge)

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

# 4. Tekan ▶️ Play di Unity Editor
```

---

## 📝 Catatan

- 🎬 Scene utama gameplay: `GameKEL9.unity`
- 🧪 Scene `Coba.unity` digunakan untuk keperluan testing/eksperimen
- 🌐 Dokumentasi localization lengkap ada di `LOCALIZATION_FILES.md` (folder `Assets/GameAssets/Code/`)

<div align="center">

---

Made with 🖤 & jumpscares by **Kelompok 9**

</div>
