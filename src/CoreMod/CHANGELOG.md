# Changelog - Milex GMS1 CoreMod

All notable changes to the `Milex GMS1 CoreMod` management framework.
This format is based on [Keep a Changelog](https://keepachangelog.com/).

## [1.4.0] - 2026-09-12

### Added: Central Memory & VRAM Cleaner (Anti-Leak & FPS Drop Mitigation)

- **Central Memory & VRAM Cleaner (`MemoryCleanerService`)**:
  - Implemented an asynchronous memory and asset optimization service to combat the game's progressive FPS degradation (~80 FPS drop over 60 minutes).
  - Triggers asynchronous `Resources.UnloadUnusedAssets()` combined with a 2-stage `GC.Collect()` sweep to purge stale textures, meshes, audio clips, and Mono heap fragmentation.
- **Smart Imperceptible Triggers**:
  - **Fast Travel (`MapMenu.FastTravel`)**: Automatically triggers cleanup behind the black loading screen (`MenuLoading`).
  - **Save Game (`CheckpointManager.QuickSave` & `Autosave`)**: Purges unreferenced memory during saving.
  - **Computer / Laptop Access (`LaptopManager.OnEnable`)**: Cleans memory when the player opens the in-game laptop/tablet.
  - **Mod Menu Open (`CorePlugin.ToggleMenu`)**: Cleans memory while gameplay is paused or menu is displayed.
- **Manual Cleanup Button & Live Telemetry**:
  - Added "Clean Memory & VRAM Now" action button in both Modern Canvas Dashboard and Classic IMGUI Menu under `[Performance]`.
  - Displays real-time status: time elapsed since last clean, trigger source, and megabytes of RAM freed.
  - Bypasses cooldown when clicked manually.
- **Cooldown Guard**:
  - Configurable cooldown timer (`MemoryCleanCooldownSeconds`, default 60s) prevents redundant GC or asset unload sweeps during rapid quicksaving or laptop toggling.
- **New Configuration Options**:
  - `[Performance] EnableMemoryCleaner` (default: `true`)
  - `[Performance] MemoryCleanCooldownSeconds` (default: `60.0`)
  - `[Performance] CleanOnFastTravel` (default: `true`)
  - `[Performance] CleanOnSave` (default: `true`)
  - `[Performance] CleanOnLaptop` (default: `true`)
  - `[Performance] CleanOnMenuOpen` (default: `true`)

---

## [1.3.5] - 2026-09-11

### Added & Fixed: Real-Time Pause Diagnostics, Auto-Recovery & Emergency Unpause

- **Real-Time Pause Diagnostics (`UpdatePauseDiagnosticsAndRecovery`)**:
  - Automatically monitors `Time.timeScale`, `PauseManager.PauseReasons`, and `InputManager.AllInputBlocked` every 3 seconds outside the main menu.
  - Emits detailed warning logs identifying any lingering pause tags and input blocking states.
- **Intelligent Engine Auto-Recovery (`ForceResumeGame`)**:
  - If the game remains paused after scene loading finishes (`elapsedSinceLoad > 4.0s`, safely beyond `AreaStreamerBase`'s 30-frame collider initialization) and no interactive player UI (`ShopGUI`, `LaptopUse`, `MenuManager`, `FastTravel`, `FreeCam`) is active, automatically clears leaked streaming pause reasons, restores `Time.timeScale = 1.0f`, clears input blocks, and locks the hardware cursor.
- **Manual "Resume Game (Emergency Unpause)" Action**:
  - Added an emergency unpause action card / button in both the Modern Canvas Dashboard and Classic IMGUI Menu under General Settings.
  - Allows players to instantly unfreeze their game and regain full mouse and movement control at any time.
- **Automatic Menu Close Recovery**:
  - Closing the Mod Menu when `PauseGameOnMenu == false` while the game is stuck paused now triggers `ForceResumeGame()`.

---

## [1.3.4] - 2026-09-11

### Fixed: Non-Intrusive Scene Loading & Engine Streaming Synchronization

- **Streaming Synchronization & Void-Fall Immunity**:
  - Removed harmful unconditional `Time.timeScale = 1.0f` overrides in `OnSceneLoaded` during intermediate level loading stages (`SceneBuffor`, `Build_Stream_Main`, `Scenario_1`, and additive chunk scenes).
  - Preserved `AreaStreamerBase`'s native 30-frame initial freeze, allowing terrain collision meshes to fully generate before physics simulation begins. Resolves the issue where starting a fresh game caused the player to fall through the ground into the void.
- **Conditional Menu Unpause (`ApplyGamePause`)**:
  - `ApplyGamePause(false)` is now strictly conditional on `_isGamePausedByMenu == true`, ensuring the mod menu never tampers with native game loading pauses (`LevelLoadingManager`, `load`, `sceneName`, `THERE IS NO DIFFICULTY CHOSEN`).
- **Default Config Alignment**:
  - Aligned `Milex_GMS1_CoreMod.default.cfg` default for `PauseGameOnMenu` to `false` (matching code and documentation).

---

## [1.3.2] - 2026-09-06

### Framework-Level Localization Extension, Caller Resolution & Native Game Key Bridge

- **Caller-Aware Framework Localization in `LocalizationManager`**:
  - Added caller-aware overloads `Translate(key, defaultValue)` and `TranslateFormat(key, defaultFormat, args)` using `Assembly.GetCallingAssembly()` and `[MethodImpl(MethodImplOptions.NoInlining)]`.
  - Added concise static aliases `LocalizationManager.T` and `LocalizationManager.Format`.
  - Sub-mods can now resolve localized strings directly through the CoreMod framework without writing custom boilerplate wrappers or maintaining local translation helpers.
- **Native Game Localization Key Bridge (`ResolveGameText`)**:
  - Added `LocalizationManager.ResolveGameText(string textOrKey)` to resolve internal Gold Mining Simulator strings (such as `SHOP_ITEM_PARTS_...`).
  - Seamlessly prioritizes mod/core dictionaries, queries the game's native engine (`global::LocalizationKey.GetLocalized()`), and gracefully formats unknown keys into readable Title Case if unmapped.
- **`ModBase` Formatting & Alias Extensions**:
  - Added `TranslateFormat(key, defaultFormat, args)`, `T(key, defaultValue)`, and `Format(key, defaultFormat, args)` directly to `ModBase` for strongly-typed sub-mod instance access.
- **Embedded Default Config Template Engine (`ExtractDefaultConfigFile`)**:
  - `ModBase.Config` now checks if the target `%AssemblyName%.cfg` file exists on disk prior to initializing `ConfigFile`.
  - If no config file exists yet (first launch or after manual reset), `ModBase` automatically extracts the embedded `%AssemblyName%.default.cfg` template from the DLL resources into `BepInEx/config/%AssemblyName%.cfg`.
  - Allows full authorial control over default settings, section ordering, and comments directly from source repository template files without requiring runtime creation from scratch.
- **Centralized Cursor Requester Architecture (`CorePlugin.RequestCursorUnlock` / `ReleaseCursorUnlock`)**:
  - Provides a centralized cursor registry enabling external overlays and sub-mods (e.g. Diagnostic Inspectors) to unlock the mouse cursor and block game input without forcing the main Mod Menu to open.
  - Ensures clean 1:1 restoration of first-person mouse locking and camera control only after all requesters have released their hold.
- **Mouse Cursor Capture & Hardware Lock Fix**:
  - Resolved cursor state leakage where the mouse cursor remained visible and unlocked after closing the in-game menu.
  - Added `SuppressGetterPatch` during state capture and ensured the game's actual lock state is recorded prior to flipping `IsMenuOpen = true`.
  - Fixed `InputManager.SetPauseMenuBlocked` parameter count exception by passing both `pauseMenuBlocked` and the reason string (`"MilexModMenu"`).
  - Integrated `CursorManager.Instance.Refresh()` on menu close to re-engage the game's native hardware window clipping and hide the cursor cleanly.
- **Modern Canvas Menu Backdrop Decoupling**:
  - Removed click-to-close behavior from the semi-transparent canvas backdrop. Clicks outside the dashboard container or into other active mod overlays will no longer unintentionally dismiss the Mod Menu.

---

## [1.3.1] - 2026-09-04

### Bug Fixes & Stability

- **Mouse Cursor State Restoration Fix**:
  - Resolved cursor state leakage where the mouse cursor remained visible and unlocked after closing the in-game menu during gameplay.
  - Intercepted cursor state before setting `IsMenuOpen = true` and prevented internal UI unlock calls from overwriting the remembered game lock state.
  - Guarantees 1:1 restoration of first-person gameplay mouse lock and visibility upon menu close.

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
