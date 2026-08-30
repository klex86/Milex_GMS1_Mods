# Changelog - Milex GMS1 CoreMod

All notable changes to `Milex_GMS1_CoreMod` will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.2.0] - 2026-08-30

### Added
- **Live Sub-Mod Lifecycle Management**:
  - `ModBase.SetEnabled(bool)` with automatic Harmony `PatchAll` / `UnpatchSelf` and `MonoBehaviour.enabled` toggle.
  - Virtual `OnModEnabled()` and `OnModDisabled()` hooks.
  - `CorePlugin` overrides `CanBeDisabled => false` to protect core functionality.
- **Developer Localization Option**:
  - `IgnoreExternalTranslations` setting in `Milex_GMS1_CoreMod.cfg`.
  - Bypasses disk JSON files in `Milex GMS1 Mod Localization` folder to load directly from embedded DLL resources.
- **TimeScale Pause Logic**:
  - Independent pause state tracking (`_isGamePausedByMenu`) ensuring time scale is safely restored under all conditions.
- **Input & Camera Lock Matrix**:
  - Native `InputManager.SetPauseMenuBlocked(true)` reflection call, native Unity input patches, mouse wheel delta blocking, and `Cursor.lockState` getter interception.
- **UI Anchoring & Formatting**:
  - Fixed top-left corner anchoring during matrix UI scaling.
  - Removed unicode emojis from menu titles, buttons, and localization files.

---

## [1.1.0] - 2026-08-29

### Added
- Multi-Assembly separation and `ModRegistry`.
- IMGUI In-Game Mod Configuration Menu on `Insert`.

---

## [1.0.0] - 2026-08-29

### Added
- Initial `ModBase` class with automated Harmony patch/unpatch lifecycle.
