# Changelog - Milex GMS1 Claim Monitor

All notable changes to the `Milex GMS1 Claim Monitor` mod are documented in this file.
This format is based on [Keep a Changelog](https://keepachangelog.com/).

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
  - Interactive Diagnostic Inspector window toggleable via **`F3`** or **`F8`**.
  - One-click scene hierarchy object dumper with comprehensive property and field inspection.
- **Multi-Language Support**:
  - Complete English and German localization out of the box with embedded template extraction.
