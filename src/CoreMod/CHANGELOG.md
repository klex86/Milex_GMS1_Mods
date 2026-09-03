# Changelog - Milex GMS1 CoreMod

All notable changes to the `Milex GMS1 CoreMod` management framework.
This format is based on [Keep a Changelog](https://keepachangelog.com/).

---

## [1.3.0] - 2026-09-03

### Added & Architectural Improvements

- **Next-Gen Modern Dashboard (uGUI Canvas)**:
  - Added a state-of-the-art runtime Canvas interface with zero external assets/bundles.
  - Interactive window with draggable header, smooth rounded cards, and gold accent styling.
  - Category tabs for quick navigation across settings sections (`All`, `Logistics`, `Processing`, `Vehicles`, `Wash Plants`, `Tools`).
  - Live real-time search filter bar to instantly filter settings across all categories.
  - Modern toggle switches and wide responsive sliders with direct reset-to-default buttons.
- **Dual-Engine Menu Architecture (`IMenuRenderer`)**:
  - Cleanly decoupled rendering layer from core plugin logic.
  - Seamless in-game switching between **Modern (uGUI Canvas)** and **Classic (IMGUI)** via configuration and header buttons.

---

## [1.2.2] - 2026-09-02

### Bug Fixes & Stability

- **Mouse Cursor State Restoration**:
  - Restores exact previous cursor visibility and lock state upon menu close. If the menu is opened while inside the pause menu or inventory, the cursor remains visible and unlocked.
- **Procedural UI Texture Protection**:
  - Protected generated background and slider textures with `HideFlags.HideAndDontSave` against garbage collection during scene transitions.

---

## [1.2.1] - 2026-08-30

### Improvements & Bug Fixes

- **High-Contrast Slider Design**:
  - Sliders feature clearly visible dark tracks and distinct gold thumb handles.
- **Group Reset Button**:
  - Added `[ Reset Group ]` button to restore section defaults instantly.
- **English Configuration Keys**:
  - Standardized all internal `.cfg` descriptions to English.
- **Live Status Bar Updating**:
  - Real-time active mod counter updates immediately on toggles (`Active Mods: X / Y`).

---

## [1.2.0] - 2026-08-30

### Added

- **Live Sub-Mod Toggle**:
  - Enable or disable extension mods live without restarting the game.
- **Developer Option for Translations**:
  - Added *"Ignore External Localization Files"* to test embedded strings directly.

### Fixed & Improved

- **Reliable Game Pause**:
  - Fixed pause state desynchronization when toggling pause while menu was open.
- **Anchored UI Scaling**:
  - Fixed origin drift during UI scale changes.
- **Native Input Lock**:
  - Fixed background player and camera motion while menu is open.

---

## [1.1.0] - 2026-08-29

### Added

- Decoupled core framework from sub-mods.
- In-game mod menu opened via `Insert` key.

---

## [1.0.0] - 2026-08-29

### Initial Release

- Core framework architecture and base lifecycle support.
