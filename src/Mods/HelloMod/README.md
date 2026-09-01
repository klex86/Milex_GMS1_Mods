# Milex GMS1 HelloMod

- **Version:** `1.1.0` ([View Changelog](CHANGELOG.md))
- **Mod Name:** HelloMod
- **Author:** Milex
- **Assembly File:** `Milex_GMS1_HelloMod.dll`
- **Dependency:** `Milex_GMS1_CoreMod.dll`

An example demonstration mod for *Gold Mining Simulator*, illustrating how sub-mods interact with `Milex GMS1 CoreMod`.

---

## Installation & Prerequisites

1. **Prerequisite**: BepInEx 5 and `Milex_GMS1_CoreMod.dll` installed.
2. **Installation**: Copy `Milex_GMS1_HelloMod.dll` into your `BepInEx/plugins/` folder.

---

## Features

- **Test Hotkey (`F2`)**: Emits a configurable greeting message to the game log.
- **Fully Configurable**:
  - Rebindable hotkey in the in-game menu.
  - Custom greeting message editable live.
  - Live on/off toggle via menu checkbox.
- **Multi-Language Support**: English and German out of the box.

---

## Configuration Reference (`Milex_GMS1_HelloMod.cfg`)

Configuration is stored in `BepInEx/config/Milex_GMS1_HelloMod.cfg`:

### Section `[General]` (General Options & Mod State)

| Key | Type | Default | Description |
|---|---|---|---|
| **`Enabled`** | `Boolean` | `true` | Mod activation toggle. When `false`, all mod updates are halted. |
| **`ShowPrefix`** | `Boolean` | `true` | Determines whether log output is prefixed with `[Greeting]`. |
| **`GreetingMessage`** | `String` | `"Hello from Gold Mining Simulator Modding!"` | Custom greeting text. |

### Section `[Controls]` (Controls)

| Key | Type | Default | Description |
|---|---|---|---|
| **`TestHotkey`** | `KeyCode` | `F2` | Hotkey used to trigger the greeting log entry. |

---

## Localization Files

- **Directory**: `BepInEx/plugins/Milex GMS1 Mod Localization/`
- **Files**: `Milex_GMS1_HelloMod_de.json`, `Milex_GMS1_HelloMod_en.json`
