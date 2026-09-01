# Milex GMS1 CoreMod

- **Version:** `1.2.2` ([View Changelog](CHANGELOG.md))
- **Mod Name:** Milex GMS1 CoreMod
- **Author:** Milex
- **Assembly File:** `Milex_GMS1_CoreMod.dll`

The **CoreMod** is the central management framework for all Milex mods in *Gold Mining Simulator*. It provides the in-game menu, localization lifecycle management, input locking, and base mod state persistence.

---

## Installation & Prerequisites

1. **Prerequisite**: BepInEx 5 (x64) installed in the main game directory.
2. **Installation**: Copy `Milex_GMS1_CoreMod.dll` into your `BepInEx/plugins/` folder.
3. **Launch**: Open the in-game menu using the **`Insert`** key.

---

## In-Game Features

1. **In-Game Menu (`Insert` Key)**:
   - Configure and manage all installed Milex mods directly during live gameplay.

2. **Live Mod Lifecycle Management**:
   - Toggle individual extension mods on or off at any time. Changes take effect immediately without requiring a game restart.

3. **Localization Engine**:
   - Automatic game-language synchronization.
   - Central management of all JSON language files in `BepInEx/plugins/Milex GMS1 Mod Localization/`.
   - Developer option *"Ignore External Localization Files"* to test embedded DLL resources directly.

4. **Reliable Input & Camera Locking**:
   - Freezes player rotation, camera movements, and tool switching while the menu is open.

---

## Configuration Reference (`Milex_GMS1_CoreMod.cfg`)

CoreMod settings are stored in `BepInEx/config/Milex_GMS1_CoreMod.cfg`:

### Section `[General]` (General Behavior & Controls)

| Key | Type | Default | Description |
|---|---|---|---|
| **`MenuToggleKey`** | `KeyCode` | `Insert` | Hotkey used to open and close the mod menu. |
| **`PauseGameOnMenu`** | `Boolean` | `false` | Freezes the game world (TimeScale = 0) while the mod menu is open. |
| **`IgnoreExternalTranslations`** | `Boolean` | `false` | Developer option: Ignores external JSON files and loads strings directly from embedded DLL resources. |

### Section `[Localization]` (Language Settings)

| Key | Type | Default | Description |
|---|---|---|---|
| **`UseGameLanguage`** | `Boolean` | `true` | Automatically synchronizes with the language selected in the game settings. |
| **`SelectedLanguage`** | `String` | `en` | Manually selected language code (e.g. `de`, `en`, `fr`) when `UseGameLanguage` is disabled. |

### Section `[UI]` (Appearance & Scaling)

| Key | Type | Default | Description |
|---|---|---|---|
| **`UIScale`** | `Float` | `1.0` | Scaling multiplier for the mod menu (0.70 to 1.60) for high-DPI displays. |

---

## Localization Files & Template Generation

- **Directory**: `BepInEx/plugins/Milex GMS1 Mod Localization/`
- **File Format**: `Milex_GMS1_CoreMod_de.json` (German), `Milex_GMS1_CoreMod_en.json` (English), etc.

When selecting an uninstalled language in the menu, clicking *"Create Templates"* automatically extracts new JSON translation templates directly into the localization directory.
