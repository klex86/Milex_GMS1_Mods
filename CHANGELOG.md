# Changelog - Milex GMS1 Mods

All notable changes and releases for this mod collection are documented in this file.
This format is based on [Keep a Changelog](https://keepachangelog.com/).

---

## [1.8.2] - 2026-09-05

### Claim Monitor: Generator/Pump Running State Validation & Sleek Minimal Compact HUD

- **Generator & Water Pump Running State Validation**:
  - Fixed power state checks where machines connected to a stopped/empty generator were falsely reported as powered.
  - Actively validates generator/pump controller states (`PowerStationController.IsWorking`, `isEnabled`, `!IsOverLoaded`, and `WaterStationController.IsWorking`) alongside `Indicator.LastState` (State 0 = White/Off, State 1 = Gray/Disconnected, State 3 = Red/Overload).
- **Duplex Jig, Gravel Pump & Mini Wash Plant Requirements**:
  - Corrected requirement profiles: Duplex Jigs and Gravel Pumps only consume electric power and do not require water connections (eliminating false water warnings on Tier 5 Glacier Creek / Gravel Pump setups).
  - Configured Mini Wash Plant to run on internal fuel engine without external electric power requirements.
- **Sleek Minimal Compact HUD Overlay**:
  - Redesigned Compact Mode into a sleek, minimal text HUD overlay that directly lists active warnings as clean, compact bullet points with color-coded severity tags (`• [CRITICAL]`, `• [WARN]`).
  - Automatically sizes to fit active warnings tightly and supports full window drag & drop across the entire compact banner.

---

## [1.8.1] - 2026-09-04

### Claim Monitor: Indicator-Driven Detection, Orange Beast Deduplication & Fuel Bar Overlay

- **In-Game Visual Indicator-Driven State Detection**:
  - Overhauled power and water tracking by reading the state of `GoldDigger.Indicator` instances (the actual in-game green/gray water drop and lightning bolt icons).
  - Exempted Trommels from water supply requirements (Trommels only require electric power).
- **Orange Beast Setup Presence & Deduplication**:
  - Requires active `OrangeBeastWashPlantGoldCounter` on the claim to prevent false alarms on uninstalled setups.
  - Ignores structural frame GameObjects and deduplicates Orange Beast Shaker items.
- **Front-Aligned Vehicle Quick-Switcher Fuel Status Bar**:
  - Re-positioned fuel status indicator directly in front of each vehicle card (to the left of the selection area).
  - Displays a clean 6px vertical status bar with fuel percentage text, completely eliminating overlap with distance labels (`174 ft`).
- **HUD Position Cleanup**:
  - Removed non-functioning X/Y position sliders from the config menu; dragged window position is persisted automatically via `PlayerPrefs`.

---

## [1.8.0] - 2026-09-04

### Added: Milex Claim Monitor & CoreMod Cursor Lock Bugfix

- **New Sub-Mod: Milex GMS1 Claim Monitor (v1.0.1)**:
  - Real-time on-screen Warning HUD telemetry dashboard with draggable, auto-saving UI.
  - Setup-accurate wash plant classification across three tiers:
    - **Setup 1**: Mobile Wash Plants (Mini & Mobile Wash Plants).
    - **Setup 2**: Stationary Setup T3–T5 (Shaker/Glacier Creek, Trommel/Reinforced Trommel, Duplex Jigs/Gravel Pumps, Sluices).
    - **Setup 3**: Setup T6 / Orange Beast (Giant Shaker, Extended Sluices).
  - Optional Feeding Chain monitoring (Hoppers and Conveyors) linked to wash plant setups.
  - Malfunction detection for Trommel drive chain breakage, Shaker motor/water/power failures, Duplex Jig pump failures, and full buckets (with dedicated single-bucket logic for Tier 5 Gravel Pumps).
  - Robust power and water status verification via active `PowerConsumer` and `WaterConsumer` game properties.
  - Sluice mat fill level tracking with configurable warning thresholds (default: 90%) and critical overflow alerts (100%).
  - Vehicle and heavy machinery fuel tracking with low fuel warnings (< 15%) and empty tank critical alerts.
  - Power generator and water tower level/operation monitoring.
  - Built-in Diagnostic Inspector (`F3`) and deep memory object dumper for claim diagnostics.
  - Configurable update scan interval (1.0s to 30.0s).
  - Complete English and German localization out of the box.
- **CoreMod Cursor Lock/Visibility Bugfix**:
  - Resolved cursor state leakage where the mouse cursor remained visible and unlocked after closing the in-game menu during gameplay.
  - Intercepted cursor state before setting `IsMenuOpen = true` and prevented internal UI unlock calls from overwriting the remembered game lock state.
  - Guarantees 1:1 restoration of first-person gameplay mouse lock and visibility upon menu close.

---

## [1.7.0] - 2026-09-03

### Added: Next-Gen Modern Dashboard & Dual-Engine Menu Architecture

- **Next-Gen Modern Dashboard (uGUI Canvas)**:
  - Built a state-of-the-art runtime Canvas interface created purely in C# with zero external asset dependencies.
  - Interactive window with draggable header, subtle bordered cards (`CardBoxSprite`), and sleek gold accent theme.
  - **Fixed Header Hierarchy with Mod Subtitle**: Top-left header displays fixed `Milex GMS1 CoreMod (v1.3.0)` with a dynamic gold subtitle indicating the currently active mod (`> Production Tuner`).
  - **Dynamic Content-Proportional Filter Tabs**: Tabs now allocate space proportionally based on text length (`flexibleWidth = label.Length`), preventing text clipping on long names while removing unnecessary dead space on short labels.
  - **High-Contrast Tactical Badges**: Inactive tabs render in distinct slate with bright silver labels, while the selected tab pops in radiant gold with dark charcoal text.
  - **Instant Button Hover & Tinting Fix**: Fixed uGUI `targetGraphic.color` ColorBlock multiplication issue, enabling vivid, responsive slate-blue hover states across all buttons and cards.
  - **Non-Flashing In-Place Sidebar Selection**: Switching mods updates existing UI component states directly without destroying and rebuilding GameObjects.
  - **Zero-Jump Scroll Preservation**: Resets and slider adjustments maintain the player's exact scroll position without snapping to the top.
  - **High-Contrast Section Banners**: Enhanced category headers with distinct dark slate container styling, left gold accent bars, and prominent reset buttons.
  - Real-time search filter bar to instantly locate any setting or multiplier across all categories.
  - Modern toggle switches and wide responsive sliders with direct reset-to-default buttons and hover glow effects.
  - **Compact High-Density Layout**: Optimized card heights allowing almost twice as many settings on screen at once.
  - **Category Group Reset**: Added direct reset buttons on section headers to quickly reset entire groups of settings to default values.
  - **Interactive Language Selector**: Dedicated manual language switching cards for German, English, and other supported languages.
  - **Visible Stylized Scrollbars**: Permanent, sleek scrollbars with slate tracks and gold hover highlights.
  - **Full Localization Alignment**: Accurately mapped all sub-mod section titles and configuration keys to language files.
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
  - Created [`DEVELOPER_GUIDE.md`](DEVELOPER_GUIDE.md): A comprehensive handbook for human modders explaining `ModBase` inheritance, zero-code UI generation, baseline memory (`OriginalValueStore`), and multi-language localization.
  - Created [`AGENT_MOD_GUIDE.md`](AGENT_MOD_GUIDE.md): A complete technical specification and system prompt designed for AI coding agents to create 100% framework-compliant sub-mods from game code excerpts.
- **Dual-Engine Menu Architecture (`IMenuRenderer`)**:
  - Fully decoupled rendering layer from core plugin logic.
  - Seamless in-game switching between **Modern (uGUI Canvas)** and **Classic (IMGUI)** via configuration setting and header buttons.

---

## [1.6.0] - 2026-09-02

### Added: Production Tuner Vehicle & Logistics Expansion

- **Mobile Conveyors (Frankenstein & Cordylus)**:
  - Added directly under the Vehicles group in the in-game menu with independent controls for buffer capacity (default: 2.0x) and transport speed (default: 2.0x).
  - Fully automatic detection for both conveyor types.
  - **Synchronized Throughput**: The hopper buffer empties proportionally with the selected belt speed, moving larger dirt portions at faster intervals so material moves continuously without bottlenecking.
- **Dedicated 3-Axis Excavator Hydraulic Controls**:
  - 3 independent sliders for fine-tuned excavator operation:
    - **Boom / Arm Speed** (lifting and extending, default: 2.0x)
    - **Turret Rotation Speed** (cabin and upper carriage swing, default: 2.0x)
    - **Bucket Tilt Speed** (curling and dumping, default: 1.0x)
  - Full range of motion without physical rotation limits or jitter.
- **In-Game Localization Alignment**:
  - All components aligned with official in-game terms (*Dump Truck*, *Wheel Loader*, *Backhoe Loader*, *Wave Table*, *Gold Nuggetator*, *Miner's Moss*, etc.).
  - Streamlined descriptions into concise, clear player-facing summaries.
- **Fixed Vanilla Restoration on Mod Toggles**:
  - Toggling the mod off and on now cleanly restores unmodified game defaults without doubling capacities or halving fill levels.
  - All equipment, vehicles, and tools reliably remember their original game values.

---

## [1.5.2] - 2026-09-01

### Performance Optimization (Eliminated FPS Drop)

- **Zero-Stutter Fast Path**: All calculations exit immediately when slider values are unchanged, completely eliminating micro-stutters and frame rate drops.
- **Optimized Memory Management**: Streamlined internal updates to prevent garbage collection pauses during gameplay.
- **Hydraulic Cylinder Caching**: Vehicle joints are registered once when spawning, saving CPU power during vehicle operation.

---

## [1.5.1] - 2026-09-01

### Bug Fixes & In-Game Refinements

- **Excavator Digging Precision**: Removed enlarged collision box collider sizing. Excavator digs with pinpoint accuracy at the shovel blade while holding the full enlarged volume.
- **Hand Shovel Live Scaling**: Switched shovel patching to `Update()` loop with $\sqrt{M}$ blade scaling so existing shovels in inventory and live slider changes update instantly.
- **Dump Truck & Wheel Loader Decoupling**: Fixed mutual volume overwriting between dump trucks and wheel loaders.
- **Fuel Trailer Live Refresh**: Switched fuel trailer tracking to `Update()` loop so already purchased trailers receive the new capacity immediately.
- **UI Slider Texture Protection**: Protected procedural slider textures against garbage collection on scene changes.

---

## [1.5.0] - 2026-09-01

### Milestone: Production Tuner Phase 2 (Complete Game Integration)

- **Full Implementation of All 22 Harmony Patches**:
  - Shovels, buckets, excavators, wheel loaders, backhoe loaders, dump trucks, conveyors, wash plants, shakers, sluice boxes, miner's moss, nuggetator, magnetite separator, wave table, and trailers fully hooked.
- **Resource Neutrality & Infrastructure Protection**:
  - Hog pan water drainage clamped to vanilla base rate in `ProcessPlane`.
  - Electric wattage and water intake demands remain unmodified to prevent power outages or pressure drops.
  - Wheel loader lifting torque automatically boosted for heavier bucket payloads.
- **OriginalValueStore & Drift Prevention**:
  - Original base values cached prior to first multiplication; cleanly restored when disabling mods or resetting sliders.
- **Community Credits & Open Source License**:
  - Comprehensive credits to community mod authors added to documentation.
  - Fully open MIT-style license granted for public use.

---

## [1.4.0] - 2026-08-30

### Production Tuner Enhancements & Rework

- **Specific Default Multipliers**: Each slider starts with an optimal default multiplier (e.g. Excavators 3.0x, Dump Truck 3.0x, Shovel 2.0x, Wash Plants 2.0x).
- **Component Cleanup**:
  - Hand Tools: Removed gold pan; added mobile wash plant capacity slider.
  - Vehicles: Unified all excavators under a single slider; added dump truck; removed obsolete mobile conveyor switch.
  - Wash Plant Modules: Centralized wash plant capacity, speed, and sluice box controls.
  - Fine Processing: Added magnetite separator capacity.
- **Direct Slider Controls**: Removed group multipliers and simple/advanced modes in favor of clean individual sliders.
- **Extended Range & Dynamic Bucket Ceiling**: Downstream containers support multipliers up to 20.0x, while bucket capacity is dynamically capped at the maximum allowed downstream capacity.

---

## [1.3.1] - 2026-08-30

### UI Improvements & Bug Fixes

- **Centered Slider Controls**: Thumb handles aligned symmetrically along slider tracks.
- **Text Wrap Protection**: Group headers and labels protected against unwanted word wrapping.
- **Dynamic Sidebar Width**: Menu sidebar width dynamically calculates based on mod name lengths.
- **Default Value Display**: Each entry displays its default value (e.g. `(Default: 2.0)`).
- **Live Mod Counter**: Status bar updates active mod counts immediately on toggles (`Active Mods: X / Y`).

---

## [1.3.0] - 2026-08-30

### New Mod: Production Tuner

- Introduced Production Tuner mod for configuring processing speeds, throughput, and capacities across all equipment, vehicles, and tools.
- Cascade protection for downstream equipment.
- Full English and German localization.

---

## [1.2.0] - 2026-08-30

### Features & Core Enhancements

- **Live Mod Toggle**: Enable or disable any installed mod during live gameplay without restarting.
- **Developer Option for Translations**: Added *"Ignore External Localization Files"* to test embedded DLL resources directly.
- **Reliable Game Pause**: Game safely freezes and resumes when opening and closing the mod menu.
- **Input & Camera Lock**: Camera and native mouse inputs locked while menu is open.
- **Stable UI Scaling**: Matrix-based window scaling anchored to the top-left screen origin.

---

## [1.1.0] - 2026-08-29

### Initial In-Game Menu & Localization

- In-game mod menu opened via `Insert` key.
- Real-time keybinding remapping.
- Localization system for English and German.

---

## [1.0.0] - 2026-08-29

### Initial Release

- Initial release of Milex GMS1 Mod Framework and `HelloMod` demonstration plugin.
