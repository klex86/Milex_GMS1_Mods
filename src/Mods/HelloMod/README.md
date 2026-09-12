# Milex GMS1 HelloMod

- **Version:** `1.1.0` ([View Changelog](CHANGELOG.md))
- **Mod Name:** Milex GMS1 HelloMod
- **Author:** Milex
- **Assembly File:** `Milex_GMS1_HelloMod.dll`
- **Dependencies:** `Milex_GMS1_CoreMod.dll`, `BepInEx 5.4.21+`

**HelloMod** is a lightweight demonstration and template mod for *Gold Mining Simulator* (*Gold Rush: The Game*). It serves as an interactive reference for how sub-mods integrate into the `Milex GMS1 CoreMod` framework, featuring live configuration binding, in-game menu integration, and multi-language localization.

---

## Quick Install & Getting Started

1. **Prerequisites**: Ensure **BepInEx 5** (x64) and **`Milex_GMS1_CoreMod.dll`** are installed.
2. **Install**: Copy `Milex_GMS1_HelloMod.dll` into your `BepInEx/plugins/` folder.
3. **Start Game**: Launch the game normally.
4. **Quick Start**:
   - Press **`F2`** in-game to trigger a test greeting message in the game log.
   - Press **`Insert`** to open the Milex Mod Menu, select **HelloMod**, and customize the greeting text or rebind the hotkey.

---

## In-Game Controls & Usage

- **`F2` (Test Hotkey)**: Triggers the test greeting output. Can be rebound to any key in the in-game menu.
- **`Insert` (Mod Menu)**: Opens the central configuration dashboard where you can edit greeting settings live or toggle the mod on and off.

---

## Detailed Features & Functions

- **Custom Greeting Message**: Fully customizable in-game greeting string with an optional prefix.
- **Live Lifecycle Toggling**: Sub-mod can be enabled or disabled live in the mod menu sidebar without restarting the game.
- **Multi-Language Support**: Out-of-the-box English and German language files, synchronized automatically with the game.

---

## Configuration Reference (`Milex_GMS1_HelloMod.cfg`)

Configuration is stored in `BepInEx/config/Milex_GMS1_HelloMod.cfg`:

### Section `[General]` (General Options & Mod State)

| Key | Type | Default | Description |
|---|---|---|---|
| **`Enabled`** | `Boolean` | `true` | Mod activation toggle. When `false`, all hotkey listeners and update loops are halted. |
| **`ShowPrefix`** | `Boolean` | `true` | Determines whether log messages are prefixed with `[Greeting]`. |
| **`GreetingMessage`** | `String` | `"Hello from Gold Mining Simulator Modding!"` | Custom greeting text printed to the game log. |

### Section `[Controls]` (Controls)

| Key | Type | Default | Description |
|---|---|---|---|
| **`TestHotkey`** | `KeyCode` | `F2` | Hotkey used to trigger the test greeting output. |

---

## Localization Files

- **Directory**: `BepInEx/plugins/Milex GMS1 Mod Localization/`
- **Files**: `Milex_GMS1_HelloMod_en.json` (English), `Milex_GMS1_HelloMod_de.json` (German).

---

## License & Free Use

All code in **Milex GMS1 HelloMod** is free and open source:
> Anyone is free to use, modify, adapt, or incorporate this code into their own mods. Attribution or a brief credit in your project is appreciated.
