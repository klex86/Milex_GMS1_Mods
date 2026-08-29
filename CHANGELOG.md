# Changelog - Milex GMS1 Mods

All notable changes to the `Milex GMS1 Mods` solution will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.2.0] - 2026-08-29

### Added
- **Developer Branding**: Renamed project to **Milex GMS1 Mods** (`Milex_GMS1_CoreMod.dll`, `Milex_GMS1_HelloMod.dll`).
- **Cursor Management & Lock/Unlock**:
  - Automatically frees (`CursorLockMode.None`) and displays (`Cursor.visible = true`) cursor when opening the Mod Menu.
  - Automatically restores previous game cursor lock state when closing the menu.
- **Input Blocker Harmony Patches**:
  - Blocks game actions (digging, tools, firing) during menu clicks via `UnityEngine.Input` Harmony prefixes.
  - Blocks mouse movement axis input (`Mouse X/Y`) to stop camera rotation while navigating the menu.
  - Consumes UI mouse events over the menu rect via `Event.current.Use()`.
- **Exact DLL-Named Configuration & Localization Files**: Mods write directly to `BepInEx/config/%AssemblyName%.cfg` and use `%AssemblyName%_%lang%.json`.
- **UI Scaling for 1440p / 4K**: Matrix-based UI scaling slider in Core Options (80% - 180%) for high-resolution displays.
- **Native Endonym Language Dropdown**: Scrollable dropdown menu showing languages in their native names (e.g. *Deutsch*, *Français*, *Polski*, *Русский*, *日本語*).
- **Missing Translation Dialog & Template Generator**:
  - Automatically detects when a newly selected language has no translation file for loaded mods.
  - Interactive popup modal asking the user if template JSON files should be created.
  - Generated templates contain instruction comments on how to translate and a request to post translations to the mod's NexusMods page.
- **Stylized Single-Toggle Checkboxes**: Clean `[✓] Aktiviert` / `[  ] Deaktiviert` toggle controls.
- **Sidebar Cleanup**: CoreMod is now excluded from the feature mods list to prevent duplication with the "⚙ Core-Optionen" tab.
- **Comprehensive Multi-Language Localization System**:
  - Automatic creation of `BepInEx\plugins\Milex GMS1 Mod Localization\`.
  - Automatic extraction of `%Modname%_en.json` and `%Modname%_de.json` templates from embedded DLL resources to disk.
  - Strict fallback cascade with English (`en`) as the guaranteed fallback language.
  - Support for 21 languages with native names.

---

## [1.1.0] - 2026-08-29

### Added
- Modular Multi-Assembly architecture with `GMSCore` and `HelloMod`.
- In-Game IMGUI Mod Configuration Menu (`Insert` key) with real-time BepInEx `.cfg` persistence and KeyCode rebinding.

---

## [1.0.0] - 2026-08-29

### Added
- Initial project setup, `ModBase`, `HelloMod`, and keybinding guidelines.
