# Changelog - Milex GMS1 CoreMod

All notable changes to the `Milex GMS1 CoreMod` management framework.
This format is based on [Keep a Changelog](https://keepachangelog.com/).

---

## [1.3.0] - 2026-09-03

### Added & Architectural Improvements

- **Next-Gen Modern Dashboard (uGUI Canvas)**:
  - Added a state-of-the-art runtime Canvas interface with zero external assets/bundles.
  - Interactive window with draggable header, subtle bordered cards (`CardBoxSprite`), and sleek gold accent theme.
  - **Fixed Header Hierarchy with Mod Subtitle**: Top-left header displays fixed `Milex GMS1 CoreMod (v1.3.0)` with a dynamic gold subtitle for the active mod (`> Production Tuner`).
  - **Dynamic Content-Proportional Filter Tabs**: Tabs now allocate space proportionally based on text length (`flexibleWidth = label.Length`), preventing text clipping on long names while removing unnecessary dead space on short labels.
  - **High-Contrast Tactical Badges**: Inactive tabs render in distinct slate with bright silver labels, while the selected tab pops in radiant gold with dark charcoal text.
  - **Instant Button Hover & Tinting Fix**: Fixed uGUI `targetGraphic.color` ColorBlock multiplication issue, enabling vivid, responsive slate-blue hover states across all buttons and cards.
  - **Non-Flashing In-Place Sidebar Selection**: Switching mods updates existing UI component states directly without destroying and rebuilding GameObjects.
  - **Zero-Jump Scroll Preservation**: Resets and slider adjustments maintain the player's exact scroll position without jumping to the top.
  - **High-Contrast Section Banners**: Enhanced category headers with distinct dark slate container styling, left gold accent bars, and prominent reset buttons.
  - Category tabs for quick navigation with clean, shortened tab titles.
  - Live real-time search filter bar to instantly filter settings across all categories.
  - Modern toggle switches and wide responsive sliders with direct reset-to-default buttons and hover glow.
  - **Compact Layout**: Reduced card heights so significantly more settings fit on screen simultaneously.
  - **Group Reset Button**: Added `[ Reset Group ]` button on section headers to reset an entire section's settings back to defaults.
  - **Manual Language Selector**: Added language selector buttons when automatic game language detection is disabled.
  - **Permanent Visible Scrollbars**: Sleek 8px scrollbar with smooth thumb hover states.
  - **Accurate Localization**: Integrated all sub-mod section titles and configuration keys with language files.
- **Zero-Flicker In-Place Filtering (`FilterCards`)**: Replaced destructive GameObject teardown on tab switches with instant in-place visibility toggling (`card.SetActive(...)`), completely eliminating visual flashes and keeping navigation at a silky-smooth 60 FPS.
- **Strict Tab Bar Containment & Concise Badges**: Added `RectMask2D` on the category tabs bar, shortened labels to clear badges (`Alle`, `Allgemein`, `Werkzeuge`, `Fahrzeuge`, `Waschanlagen`, `Veredelung`, `Logistik`), and implemented proportional layout compression to prevent any tabs from overflowing past the right window edge.
- **Sidebar Selection State Fix (White Card Bug Resolved)**: Configured `colors.selectedColor = normalColor` and deselected focus on click via `EventSystem.current.SetSelectedGameObject(null)`, preventing selected mod cards from flashing or getting stuck in solid white.
- **Full Native Language Dropdown Selector**: Replaced horizontal buttons with an expandable, scrollable dropdown listing all 21 supported languages in their respective native endonyms (`Deutsch`, `English`, `Français`, `Español`, `Polski`, `Русский`, etc.) with active `[v]` badges.
- **Mutual Exclusivity & Permanent Visibility**: Both "Use Game Language" and "Select Language" remain permanently visible in CoreMod settings; the manual selector dynamically disables and dims when automatic detection is enabled.
- **Interactive Missing Translation Template Generator Modal**: Selecting any language missing translations now triggers a dedicated modal dialog prompting the player to generate JSON templates on-demand directly into the localization directory.
- **Fixed Header Hierarchy**: Guaranteed that the top-left main title permanently displays `Milex GMS1 CoreMod (v1.3.0)` while sub-mod titles and versions cleanly route to the secondary gold subtitle.
- **Compact Missing Translation Modal & Localization Folder Opener**:
  - Redesigned the missing translation prompt into a sleek, compact 460x252 modal card with centered layout and high-contrast styling.
  - Displays the target destination directory path (`BepInEx/plugins/Milex GMS1 Mod Localization/`) right inside the dialog.
  - Added a direct **`[ Open Folder ]`** button that instantly opens the localization directory in Windows Explorer using `Process.Start`.
- **Sub-Mod Developer Guide & AI Agent Blueprint Documentation**:
  - Created [`DEVELOPER_GUIDE.md`](../../DEVELOPER_GUIDE.md): A comprehensive handbook for human modders explaining `ModBase` inheritance, zero-code UI generation, baseline memory (`OriginalValueStore`), and multi-language localization.
  - Created [`AGENT_MOD_GUIDE.md`](../../AGENT_MOD_GUIDE.md): A complete technical specification and system prompt designed for AI coding agents to create 100% framework-compliant sub-mods from game code excerpts.
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
