# Changelog - Milex GMS1 Claim Monitor

All notable changes to the `Milex GMS1 Claim Monitor` mod are documented in this file.
This format is based on [Keep a Changelog](https://keepachangelog.com/).

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
