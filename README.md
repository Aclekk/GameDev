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

## 🕯️ About the Game

**Echoes of The Old House** is a **first-person survival horror** game that drops the player into a haunted old house. Armed with nothing but a **lantern** whose light can die out at any moment, the player must gather items, solve puzzles, and evade something that keeps watching from the dark — before the house swallows them too.

> *"Every corner holds an echo of the past. Don't let them find you."*

<div align="center">

### 🩸 Key Features

</div>

| 🎯 Feature | 📋 Description |
|:---|:---|
| 🔦 **Lantern System** | The only source of light — its fuel is limited and can run out |
| 🎒 **Inventory System** | Collect keys, oil, and other essential items |
| 👻 **Ghost AI** | An entity that hunts the player and triggers jumpscares when it catches them |
| 🚪 **Puzzles & Door Interactions** | Basement, escape doors, and dynamic object interactions |
| 💾 **Save / Pause / Menu** | Full menu system with main menu and pause manager |
| 🌐 **Multi-Language** | Full support for Indonesian & English |

---

## 🗂️ Project Structure

```
GameDev/
├── Assets/
│   ├── GameAssets/Code/       # Core gameplay scripts (player, ghost, inventory, UI, localization)
│   ├── Scripts/               # Additional/bridge scripts (lantern auto-equip, UHFPS integration, etc.)
│   ├── UHFPS/                 # Ultimate Horror FPS System (asset store)
│   ├── AdvancedMobileHorror/  # Advanced Mobile Horror FPS System (asset store)
│   ├── Hantu/                 # Ghost character model & animations
│   ├── modular-dungeon/       # Modular dungeon/mansion tileset
│   ├── Cemetery Kit V1.25/    # Cemetery asset pack
│   ├── Dark UI/               # Dark-themed UI kit
│   ├── MainMenuAsset/         # Main menu assets
│   ├── Scenes/                # Scenes: MainMenu, GameKEL9, Coba (testing)
│   └── ...                    # Other supporting assets (audio, materials, etc.)
├── Packages/                  # Unity Package Manager dependencies
└── ProjectSettings/           # Unity project configuration
```

---

## 🧩 Core Scripts

<div align="center">

| Script | Function |
|:---|:---|
| `FirstPersonController.cs` / `HeroController.cs` | Player movement & camera control |
| `HantuMove.cs` / `HantuJumpscare.cs` | Ghost movement AI & jumpscare trigger |
| `Inventory.cs` / `InventorySystem.cs` | Player item storage system |
| `KeyPickup.cs` / `PetrolOilPickup.cs` / `PickupItem.cs` | Item pickup in the game world |
| `LanternContoller.cs` / `LanternToggle.cs` / `LanternOilWarning.cs` | Lantern system (main light source & fuel) |
| `BasementDoor.cs` / `EscapeDoor.cs` / `Door2BlackInteraction.cs` | Door interactions & scene transitions |
| `PauseManager.cs` / `PlayButton.cs` / `ExitButton.cs` | Menu system & game controls |
| `LanguageManager.cs` / `LocalizedText.cs` / `LanguageDropdown.cs` | EN/ID localization system |
| `TutorialManager.cs` | Guide/tutorial for new players |

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
- **Supporting packages:** Cinemachine · ProBuilder · TextMeshPro · AI Navigation (NavMesh) · Input System · Timeline
- **Horror asset toolkit:** UHFPS (Ultimate Horror FPS System) · Advanced Mobile Horror FPS System

---

## 🚀 Getting Started

```bash
# 1. Clone this repository
git clone https://github.com/Aclekk/GameDev.git

# 2. Open the GameDev folder via Unity Hub
#    Use editor version 2022.3.62f2 (or the closest 2022.3 LTS release)

# 3. Open the entry-point scene
Assets/Scenes/MainMenu.unity

# 4. Hit ▶️ Play — and pray your light doesn't die first
```

---

## 📝 Notes

- 🎬 Main gameplay scene: `GameKEL9.unity`
- 🧪 The `Coba.unity` scene is used for testing/experimentation purposes
- 🌐 Full localization documentation is available in `LOCALIZATION_FILES.md` (inside `Assets/GameAssets/Code/`)

<div align="center">

---

🕯️ *The house remembers everyone who enters.* 🕯️

Made in the dark by **Kelompok 9**

</div>
