# Milex GMS1 CoreMod

- **Version:** `1.4.0` ([View Changelog](CHANGELOG.md))
- **Mod Name:** Milex GMS1 CoreMod
- **Author:** Milex
- **Assembly File:** `Milex_GMS1_CoreMod.dll`
- **Dependencies:** `BepInEx 5.4.21+`

The **CoreMod** is the central foundation and management engine for all Milex mods in *Gold Mining Simulator* (*Gold Rush: The Game*). It introduces a dual-engine in-game menu (Modern Canvas Dashboard & Classic IMGUI), automated background memory management to prevent FPS loss, an intelligent game-pause recovery system, centralized multi-language localization, and live mod enable/disable toggling without restarting the game.

---

## Quick Install & Getting Started

1. **Prerequisites**: Ensure you have **BepInEx 5** (x64) installed in your game root directory.
2. **Install**: Copy `Milex_GMS1_CoreMod.dll` into your `BepInEx/plugins/` folder.
3. **Start Game**: Launch *Gold Mining Simulator* normally.
4. **Quick Start**:
   - Press **`Insert`** in-game at any time to open the Milex Mod Menu.
   - Enjoy the modern uGUI dashboard, browse your active mods in the sidebar, or customize global settings.
   - Everything is configured and ready to go out of the box with zero setup required.

---

## In-Game Controls & Usage

- **`Insert` (Toggle Menu)**: Opens and closes the mod menu. Camera movement and tool actions are automatically locked while the menu is open, and restored cleanly upon closing.
- **Modern vs. Classic UI Switch**: Switch between the modern card dashboard and the lightweight classic IMGUI menu under `General` -> `UI Settings` without restarting.
- **UI Scaling**: Adjust the `UI Scale` slider (from 0.8x up to 1.8x) to achieve perfect readability on 1080p, 1440p, or 4K ultrawide monitors.
- **Emergency Unpause**: If the game ever remains stuck in pause after a loading screen or fast travel, click the **"Resume Game (Emergency Unpause)"** button under `General` settings to immediately resume gameplay.
- **Manual Memory Purge**: Click **"Clean Memory & VRAM Now"** under `Performance` to instantly reclaim memory and see how many megabytes were freed.

---

## Detailed Features & Functions

### 1. Dual-Engine In-Game Menu
Choose the visual presentation that best matches your setup:
- **Modern Dashboard (uGUI Canvas)**:
  - Procedural slate-and-gold card design with smooth toggles, sliders, and category tabs.
  - **Live Search Bar**: Quickly filter settings, features, and descriptions in real time.
  - **Draggable Window Header**: Click and drag the header to position the menu anywhere on your display.
  - **High-Refresh Performance (165+ FPS)**: Built with optimized non-blocking raycasting and zero vertex snapping overhead, keeping game framerates completely silky while the menu is open.
- **Classic IMGUI Fallback**:
  - A lightweight, rock-solid alternative interface rendered via Unity's immediate GUI system. Ideal for troubleshooting or low-spec systems.

### 2. Central Memory & VRAM Cleaner (Anti-FPS-Drop Engine)
In vanilla *Gold Mining Simulator*, game performance gradually degrades during long sessions, frequently dropping by 60 to 80 FPS over an hour of gameplay due to accumulating unreferenced terrain meshes, textures, and fragmented RAM.

CoreMod reduces this progressive stutter by automatically sweeping stale memory and unused textures during natural transition moments:
- **Fast Travel**: Cleans memory behind the loading screen so you arrive at your destination with clean RAM and peak FPS.
- **Saving**: Cleans memory whenever you quicksave (`F5`) or when the game triggers an autosave.
- **Laptop & Tablet**: Frees memory whenever you open the in-game computer.
- **Mod Menu Open**: Cleans memory while you are browsing mod settings.
- **Smart Cooldown Protection**: Automatic sweeps are throttled by a 60-second cooldown to guarantee zero gameplay hiccups during active mining.
- **Live Status Reporting**: Displays real-time status in the menu showing exactly when the last sweep occurred and how many MB of RAM were reclaimed.

### 3. Engine Pause Recovery & Safety Guards
- **Leaked Pause Detection**: Vanilla loading screens or terrain streamer glitches can sometimes leave the game world frozen in an unpausable state. CoreMod actively monitors the game engine and safely resumes gameplay if an orphaned pause is detected after 15 seconds.
- **Emergency Unpause Button**: A dedicated button in the menu allows you to force an immediate engine resume if the game ever gets stuck.
- **Safe Scene Transitions**: Mod menus close automatically during scene loads to prevent UI desynchronization.

### 4. Live Sub-Mod Lifecycle Management
- All sub-mods (such as *Production Tuner* and *Claim Monitor*) can be toggled on or off live in the mod menu sidebar.
- Disabling a mod instantly stops its background loops and restores original vanilla behavior without requiring a game restart.

### 5. Multi-Language Localization Framework
- **Automatic Language Sync**: Automatically detects and matches the language chosen in the game options (English, German, French, Spanish, etc.).
- **Manual Language Override**: Option to manually lock mods to a specific language of your choice.
- **Built-in Template Generator**: Missing language translations can be generated as JSON files directly from the in-game menu.

---

## Configuration Reference (`Milex_GMS1_CoreMod.cfg`)

All CoreMod settings can be adjusted in the in-game menu or directly in `BepInEx/config/Milex_GMS1_CoreMod.cfg`:

### Section `[General]` (General Settings & Controls)

| Key | Type | Default | Description |
|---|---|---|---|
| **`MenuToggleKey`** | `KeyCode` | `Insert` | Hotkey used to open and close the mod menu. |
| **`PauseGameOnMenu`** | `Boolean` | `false` | When enabled, pauses the game world while the mod menu is open. |
| **`IgnoreExternalTranslations`** | `Boolean` | `false` | Developer option: Bypasses external JSON files and loads language strings directly from embedded DLL resources. |

### Section `[Performance]` (Memory & Asset Cleaner)

| Key | Type | Default | Description |
|---|---|---|---|
| **`EnableMemoryCleaner`** | `Boolean` | `true` | Master switch for automatic background memory and VRAM cleaning. |
| **`MemoryCleanCooldownSeconds`** | `Float` | `60.0` | Minimum cooldown in seconds between automatic background cleaning cycles. |
| **`CleanOnFastTravel`** | `Boolean` | `true` | Purges unused assets during Fast Travel loading screens. |
| **`CleanOnSave`** | `Boolean` | `true` | Purges unused assets when quicksaving (`F5`) or during autosaves. |
| **`CleanOnLaptop`** | `Boolean` | `true` | Purges unused assets when opening the in-game laptop or tablet. |
| **`CleanOnMenuOpen`** | `Boolean` | `true` | Purges unused assets whenever the Milex Mod Menu is opened. |

### Section `[Localization]` (Language Settings)

| Key | Type | Default | Description |
|---|---|---|---|
| **`UseGameLanguage`** | `Boolean` | `true` | Automatically synchronizes mod text with the language set in the game options. |
| **`SelectedLanguage`** | `String` | `en` | Manual language code (e.g. `de`, `en`, `fr`, `es`, `pl`) used when `UseGameLanguage` is disabled. |

### Section `[UI]` (Appearance & Scaling)

| Key | Type | Default | Description |
|---|---|---|---|
| **`MenuEngine`** | `MenuEngineType` | `Modern` | User interface renderer: `Modern` (uGUI Canvas Dashboard) or `Classic` (IMGUI). |
| **`UIScale`** | `Float` | `1.0` | Global interface scale multiplier (0.80 to 1.80) for comfortable reading on high-resolution monitors. |

---

## Guide: Creating Custom Translations & Templates

CoreMod features an automated template generator that makes it easy to translate all Milex mods into your native language:

1. Press **`Insert`** in-game to open the mod menu.
2. In the left sidebar, click on **General**.
3. Under **Localization**, turn off **`Use Game Language`**.
4. In the **`Select Language`** dropdown, choose your target language (e.g. French, Polish, Spanish).
5. If the translation files do not exist yet, an in-game prompt will appear informing you that templates can be generated.
6. Click **`Create Templates`**.
7. CoreMod will automatically generate pre-structured JSON template files for every active mod inside:
   `BepInEx/plugins/Milex GMS1 Mod Localization/` (e.g. `Milex_GMS1_CoreMod_fr.json`, `Milex_GMS1_ProductionTuner_fr.json`).
8. Open the `.json` files in any text editor (like Notepad++ or VS Code), translate the values, and save.
9. Restart the game or re-select the language in the menu to enjoy your translation immediately!

---

## License & Free Use

All code in **Milex GMS1 CoreMod** is free and open source:
> Anyone is free to use, modify, adapt, or incorporate this code into their own mods. Attribution or a brief credit in your project is appreciated.
