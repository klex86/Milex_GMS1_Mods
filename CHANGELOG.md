# Changelog - Milex GMS1 Mods

All notable changes to the `Milex GMS1 Mods` solution will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.2.0] - 2026-08-30

### Added
- **Sub-Mod Enable/Disable Lifecycle**:
  - Live Enable/Disable toggle button in the Mod Menu for all registered sub-mods.
  - Automatically applies (`PatchAll`) or removes (`UnpatchSelf`) Harmony patches upon state change.
  - Toggles `MonoBehaviour.enabled` to completely halt/resume `Update()` loop execution on disabled mods.
  - State persisted to each mod's individual `.cfg` under `[General] Enabled`.
- **Developer Option (`IgnoreExternalTranslations`)**:
  - Config entry in CoreMod (`General / IgnoreExternalTranslations`) allowing developers to bypass external disk JSON files and force loading embedded DLL resources directly.
  - Live reloading of all localization tables upon toggle.
- **Improved UI Scaling & Anchoring**:
  - Matrix-based scaling anchors the window's top-left screen-space position, expanding towards right/bottom without window drift.
  - Fixed 200px screen-space sidebar width.
- **TimeScale & Pause Management**:
  - Reliable state tracking (`_isGamePausedByMenu`) ensuring time scale is always correctly restored when closing the menu or toggling options while open.
- **Input Blocker & Camera Freeze**:
  - Native game input blocker (`InputManager.SetPauseMenuBlocked`), native Unity input patches, and `Cursor.lockState` getter deception ensuring camera and mouse wheel remain completely frozen during menu navigation.
- **Clean UI Text Formatting**:
  - Stripped unnecessary unicode icons and emojis from menu labels and localization files for clean rendering across all fonts.

---

## [1.1.0] - 2026-08-29

### Added
- Modular Multi-Assembly architecture with `GMSCore` and `HelloMod`.
- In-Game IMGUI Mod Configuration Menu (`Insert` key) with real-time BepInEx `.cfg` persistence and KeyCode rebinding.

---

## [1.0.0] - 2026-08-29

### Added
- Initial project setup, `ModBase`, `HelloMod`, and keybinding guidelines.
