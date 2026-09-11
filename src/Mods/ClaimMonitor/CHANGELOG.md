# Changelog - Milex GMS1 Claim Monitor

All notable changes to the `Milex GMS1 Claim Monitor` mod are documented in this file.
This format is based on [Keep a Changelog](https://keepachangelog.com/).

## [1.0.10] - 2026-09-11

### Added: Equipment Baseline Dumper (JSON) in Diagnostic Inspector (F3)

- **Dedicated JSON Baseline Dumper (`EquipmentBaselineDumper`)**:
  - Added a dedicated button **"Dump Baseline (JSON)"** in the Diagnostic Inspector window (`F3`), positioned alongside "Dump All to File".
  - Queries Unity's live memory (`Resources.FindObjectsOfTypeAll`) to inspect both scene instances and prefabs across all tool, vehicle, washplant, conveyor, and infrastructure classes.
  - Automatically captures all `[BalanceSheet]` attributes, capacity metrics, speeds, flow rates, volumes, and physical joint limits.
  - Generates formatted, structured JSON files (`Equipment_Baseline_YYYYMMdd_HHmmss.json` and `Equipment_Baseline_Latest.json`) saved in `BepInEx/plugins/Milex_ClaimMonitor_Dumps/`.

---

## [1.0.9] - 2026-09-11

### Fixed: Main Menu & Level Loading Suppression

- **Warning HUD Scene Filtering**:
  - `WarningHUD.OnGUI` now verifies the active scene name and suppresses HUD rendering completely in the Main Menu (`MainMenu`, scenes containing `menu` or `buffor`) and while level loading is active (`LevelLoadingManager.IsLoading()`).
- **Claim Scanner Idle on Loading**:
  - `ClaimScanner.PeriodicScan` and `ForceScan` now idle during Main Menu and level loading, preventing needless `FindObjectsOfType<MonoBehaviour>` overhead during scene streaming.

---

## [1.0.8] - 2026-09-11

### Fixed: Conveyor Connection Validation, F3 Toggle Sync, HUD Max Height Clamp & Dumper Expansion

- **Conveyor Connection Tracking & False Alert Elimination**:
  - Fixed false "no power" alerts triggered on freshly started games or claims where conveyors are not yet in use.
  - `ScanConveyor` now verifies whether a stationary wash plant setup (`WashPlantGoldCounter` or `OrangeBeastWashPlantGoldCounter`) actually exists on the claim before issuing missing-power alerts.
  - Checks physical cable connections (`PowerIndicator`, `Producent`, `MyRopes`) and marks disconnected conveyors as inactive (`IsConnected = false`), suppressing power-loss warnings and wear evaluation on disconnected conveyors.
- **Diagnostic Inspector (F3) & Mod Menu Synchronization**:
  - Connected `MonitorConfig.EnableDebugGroup.SettingChanged` to hardware cursor release/request and scanning triggers.
  - Toggling `F3` or flipping the toggle card inside the Modern Canvas Menu now stays in 100% synchronization live.
- **Warning HUD Max Height Clamping (`HudMaxHeight`)**:
  - Constrained dynamic auto-height calculation in both compact and full modes using `Config.HudMaxHeight` as an upper clamp bound.
  - Enabled scrollview inside compact mode when active alerts exceed configured max height, completely preventing window overflow off-screen.
- **ClaimDumper Candidate Type Expansion**:
  - Added `HogPan`, `Bucket`, `GoldPan`, and `Shovel` to `ClaimDumper.IsCandidateComponent`, ensuring complete field and state dumps for hand tools and panning equipment.

---

## [1.0.7] - 2026-09-06

### Multilingual Localization Architecture Overhaul, Native Game Key Resolution & Scrollbar-Free Warning HUD

- **100% Multilingual Alert & Status Pipeline via CoreMod Framework**:
  - Eliminated all hardcoded English equipment issue strings, alert titles, alert descriptions, and setup labels across the entire diagnostic engine (`ClaimScanner`, `ClaimDiagnosticsData`).
  - Powered directly by CoreMod framework helpers (`LocalizationManager.T`, `LocalizationManager.Format`, `ModBase.T`, and `ModBase.Format`), eliminating redundant sub-mod wrapper code.
- **Native Game Localization Key Bridge Integration**:
  - Intercepted and translated all internal game wear part keys (e.g. `SHOP_ITEM_PARTS_GLACIER_CREEK_ENGINE_NAME`, `SHOP_ITEM_PARTS_PLANTER_WATERPUMPFUSE_NAME`, `SHOP_ITEM_PARTS_PLANTER_WATERPUMPFILTER_NAME`) using `LocalizationManager.ResolveGameText`.
  - Seamlessly queries the game's native engine (`global::LocalizationKey.GetLocalized()`) with fallback cleanup, ensuring no raw internal tokens ever leak into HUD warning displays.
  - Added proper `setup.name.infrastructure` category and localized equipment names for standalone water pumps and water towers.
- **Intelligent Cable & Hose Connection Detection for Infrastructure (Pumps & Generators)**:
  - Added deep physical connection tracking (`IsUtilityConnected`) for electric/mobile water pumps, water towers, and power generators.
  - Actively inspects socket plugins, connected cables, water intake and output hoses (`WaterRopeOut`, `_ropeWaterIn`, `_ConnectedRope`, `Holder` components), electric consumers (`_powerConsumer`), and registered network consumers (`_WaterStationConsumerList`, `_PowerStationConsumerList`).
  - Standalone equipment parked or stored on the claim without any connected cables or hoses is automatically identified as unused (`IsConnected = false`). Suppresses all inactive generator warnings, empty water tower alerts, and wear alerts (such as water pump fuses, filters, or generator buttons) in the Warning HUD for disconnected equipment.
- **Big Generator Socket Breaker Button Suppression (`MonitorGeneratorSwitchButtons`)**:
  - Added new configuration option `MonitorGeneratorSwitchButtons` (default `false`).
  - Filters out individual socket circuit breaker buttons on the big power generator (`Power_Generator_Switch_Button`), preventing up to 10 duplicate button wear/breakdown notices from spamming the Warning HUD unless explicitly opted in.
- **Embedded Default Configuration Template (`Milex_GMS1_ClaimMonitor.default.cfg`)**:
  - Pre-seeded full `.default.cfg` template directly inside the DLL assembly resources for pristine first-run deployment with recommended defaults.
- **Warning HUD Usability & Dimension Overhaul**:
  - Removed the unnecessary header compact/full button (`[ - Kompakt ]` / `[ + Voll ]`).
  - Implemented dynamic auto-height in **both** compact and normal modes: HUD height dynamically expands based on the number and line length of active alerts, completely eliminating inner scrollbars and text cutoff. Clamped only to screen bounds (`Screen.height - 40f`).
- **HUD & Diagnostic Inspector Localization**:
  - Localized Warning HUD window titles, status badges (`[OK]`, `[CRITICAL]`, `[WARNINGS]`), nominal system text, and equipment summary counters.
  - Localized Diagnostic Inspector (F3) window title, action buttons (`Force Rescan`, `Dump All to File`), filter labels, status notices, and object counters.
- **Mobile Wash Plant Overhaul & Water Connection Requirement**:
  - Eliminated phantom "Trommel failure (no power)" warnings by excluding internal drum components (`WashPlantMobileTrommel`) from separate machinery evaluation.
  - Enforced water connection requirement for `MobileWashplant` and `MiniWashplant`: corrected connection check to strictly require attached hoses (`ObjectInHolder != null`, `Producent != null`, or active `MyRopes`). Removed faulty static prefab check (`RopeObjectConnectedLogic != null`) which previously registered parked, unhooked mobile wash plants as connected and falsely triggered "turned off" alerts.
  - Parked or unconnected mobile plants generate zero HUD alerts and skip wear scanning completely, while remaining visible as disconnected in the F3 Diagnostic Inspector.
  - Added diesel fuel monitoring for `MiniWashplant` via `FuelStationController` with early warning for empty diesel (`issue.miniwashplant.no_fuel`).
- **Diagnostic Inspector (F3) Polish & Direct Mouse Cursor Unlock**:
  - Toggling **`F3`** now directly unlocks the mouse cursor and halts camera rotation via `CorePlugin.RequestCursorUnlock("ClaimMonitor_DebugOverlay")`, enabling seamless interaction without opening the Mod Menu.
  - Fixed broken category filters (`Setup 1 (Mobile)`, `Setup 2 (Stationary)`, `Setup 3 (Orange Beast)`, `Conveyors & Feeders`, `Vehicles`, `Sluice Mats`, `Utilities`) by utilizing unified language-agnostic category keys and `RawDebugItem.Setup` matching.
  - Split the top toolbar into two distinct rows to comfortably fit all 8 category buttons without window overflow or button clipping.
- **Bilingual Dictionary Parity**:
  - Complete English (`_en.json`) and German (`_de.json`) language files updated with matching keys and natural phrasing.

---

## [1.0.6] - 2026-09-06

### Comprehensive Component Wear Early-Warning System & UI Window Layout Polish

- **Universal Component Wear & Breakdown Early-Warning System**:
  - Integrated full telemetry for all physical wear parts via `CheckAndRepair` across mobile wash plants, stationary setups, Orange Beast, conveyors, and water pumps.
  - Monitors spray nozzles, conveyor motor drive belts, elevator buckets, trommel drive chains and rollers, duplex jig mechanisms, and water filters.
  - **Strict Setup Association**: Only scans parts physically attached to machines (`IsInPlace == true`) that belong to currently enabled setups. Scrapped or loose parts dropped on the ground are completely ignored.
  - **Configurable Alert Threshold**: Added `ComponentWearWarningThreshold` (default 20%, range 5%-50%). Produces yellow warnings prior to breakdown and red critical alerts upon total component failure.
- **Diagnostic Inspector Window Polish (F3)**:
  - Enlarged window width to 1100px and restructured filter buttons so all category tabs fit cleanly on screen without clipping or wrapping.
- **Warning HUD Window Auto-Height**:
  - Compact mode now dynamically scales its vertical dimensions based on active warning text length to eliminate label cutoff.

---

## [1.0.5] - 2026-09-05

### Tier 3–5 Stationary Wash Plant Water Detection Overhaul & Glacier Creek / Derocker Support

- **Tier 3–5 Stationary Wash Plant Water Supply Recognition**:
  - Overhauled water detection on all stationary wash plants to directly query the simulation engine (`WashplantShakerBase.Water.HaveWater`, `CheckHasWater()`, and `_hasWater`), eliminating false alarms caused by stale or unrendered visual HUD indicators.
  - Added first-class type support for `GlacierCreek` (Tier 4) and `DeRocker` (Tier 3/4) so all stationary shaker variants are recognized and monitored seamlessly.
- **Granular Water Failure Diagnostics**:
  - Machinery warnings now provide exact, actionable reasons for missing water (hose disconnected, pump turned off, pump intake dry / tower empty, pump disabled, broken/frozen hose, or damaged nozzle).
- **Strongly-Typed Power & Water Engine Integration**:
  - Replaced reflection fallbacks with direct, strongly-typed checks against the game's simulation classes (`WashplantShakerBase`, `WashplantTrommelBase`, `WashplantDuplexJigBase`, `MobileWashplant`, `MiniWashplant`).

---

## [1.0.4] - 2026-09-05

### Generator/Pump Running State Validation, Jig Water Exemption & Sleek Minimal Compact HUD

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

## [1.0.3] - 2026-09-04

### Visual Indicator Inspection, Orange Beast Deduplication & Front-Aligned Vehicle Fuel Bar

- **Visual Indicator-Driven Power & Water Detection (T2–T5 & T6)**:
  - Overhauled detection to directly inspect the game's `GoldDigger.Indicator` components (the exact green/gray water drop and lightning bolt icons rendered above machines in game).
  - Evaluates `Indicator.LastState` (Green = Active/Nominal, Gray/Red = Disconnected/No Resource) alongside direct `PowerConsumer` and `WaterConsumer` states.
  - Exempted Trommels from water supply requirements (Trommels only consume electric power).
- **Orange Beast Setup Presence & Deduplication**:
  - Prevented ghost warnings on claims without an Orange Beast by requiring active `OrangeBeastWashPlantGoldCounter` presence before evaluating Beast components.
  - Eliminated duplicate entries by strictly ignoring static structural frame objects (`OrangeBeast_Frame`) and deduplicating Shaker instances.
- **Front-Aligned Vehicle Quick-Switcher Fuel Bar**:
  - Re-positioned fuel status indicator directly in front of each vehicle item card (to the left of the yellow selection background).
  - Features a crisp 6px vertical status bar with color grading and clear percentage text, resolving overlap with distance labels (`174 ft`).
- **HUD Position Configuration Cleanup**:
  - Removed redundant `HudPosX` and `HudPosY` sliders from the configuration menu.
  - Position is now saved automatically to `PlayerPrefs` when dragging the window with the mouse.

---

## [1.0.2] - 2026-09-04

### Water Detection Fixes, Orange Beast T6 Support & Vehicle Switcher Fuel Overlay

- **Stationary & Mobile Setup Water Supply Recognition (T2–T5)**:
  - Fixed water supply detection across all stationary wash plants, shakers, sluices, and mobile plants by actively evaluating `WashplantShakerBase.IsWaterReady`, `WaterChangePhysicsMaterial.HasWater`, and attached `WaterConsumer.HaveWater` / `Producent` states.
- **Orange Beast (T6) Power & Water Recognition**:
  - Implemented dedicated GameObject matching and multi-component deduplication for `Washplant_Shaker_Beast(Clone)` and `OrangeBeast_Frame`.
  - Accurately tracks electric power cables and water supply lines attached to the Orange Beast shaker.
- **Vehicle Quick-Switcher Fuel Status Overlay**:
  - Integrated real-time fuel status badges directly inside each card of the vanilla game's vehicle switching list.
  - Features color-coded status circles (Green >= 50%, Yellow 25–49%, Orange 15–24%, Red < 15%) and fuel percentage text, eliminating any ambiguity regarding vehicle assignment.
  - Fully reactive to the `ShowFuelInVehicleSwitcher` configuration toggle.

---

## [1.0.1] - 2026-09-04

### Improvements & Power Detection Fix

- **Robust Power & Water Consumer Integration**:
  - Resolved false "no electric power" warnings on Duplex Jigs and Gravel Pumps by directly querying attached `PowerConsumer.HavePower` properties and referenced power cords.
  - Implemented universal `PowerConsumer` and `WaterConsumer` state evaluation across all stationary, mobile, and feeder machinery.
- **Enhanced Tier 5 Component Support (Gravel Pump & Glacier Creek)**:
  - Added dedicated support and single-bucket handling for Tier 5 Gravel Pumps (`GoldDigger.GravelPump`) alongside Tier 4 Duplex Jigs.
  - Setup-accurate display names reflecting official in-game terminology (DE: "Kiespumpe", "Duplex Jig", "Rüttler", "Trommel"; EN: "Gravel Pump", "Duplex Jig", "Shaker", "Trommel").
- **Configurable Scan Interval**:
  - Added `ScanIntervalSeconds` setting in `[General]` (range: 1.0s to 30.0s, default: 3.0s) allowing players to tune update frequency.
- **Diagnostic Inspector Window Title**:
  - Corrected window header and localization hints to reference **`F3`** toggle hotkey.

---

## [1.0.0] - 2026-09-04

### Initial Release

- **Real-Time Warning HUD Overlay**:
  - Live on-screen tactical telemetry dashboard with customizable positioning and auto-save drag & drop.
  - Three wash plant setups supported with setup-accurate equipment classification:
    - **Setup 1**: Mobile Wash Plants (Mini & Mobile Wash Plants).
    - **Setup 2**: Stationary Setup T3–T5 (Shaker, Trommel, Duplex Jigs, Sluices).
    - **Setup 3**: Setup T6 / Orange Beast (Giant Shaker, Extended Sluices).
  - Optional Feeding Chain integration (Hoppers and Conveyors) linked to wash plant setups.
- **Component Malfunction & Failure Monitoring**:
  - Trommel drive chain breakage detection (`_TrommelChainDestroyed`).
  - Shaker motor stoppage and electric/water disconnection detection.
  - Duplex Jig broken pump mechanism and bucket capacity overfill alerts.
  - Sluice mat fill level tracking with configurable warning thresholds (default: 90%) and critical overflow alerts (100%).
  - Vehicle and heavy machinery fuel tracking with low fuel warnings (< 15%) and empty tank critical alerts.
  - Power generator and water tower level/operation monitoring.
- **Display Modes & In-Game Menu Integration**:
  - Full card view, ultra-compact badge view, and optional "Only Show on Warnings" mode.
  - Full configuration exposure through the CoreMod in-game menu (**`Insert`** key).
- **Diagnostics & Scene Object Dumper**:
  - Interactive Diagnostic Inspector window toggleable via **`F3`**.
  - One-click scene hierarchy object dumper with comprehensive property and field inspection.
- **Multi-Language Support**:
  - Complete English and German localization out of the box with embedded template extraction.
