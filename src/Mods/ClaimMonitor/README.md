# Milex GMS1 Claim Monitor

- **Version:** `1.0.0` ([View Changelog](CHANGELOG.md))
- **Mod Name:** Milex Claim Monitor
- **Author:** Milex
- **Assembly File:** `Milex_GMS1_ClaimMonitor.dll`
- **Dependencies:** `Milex_GMS1_CoreMod.dll`, `BepInEx 5.4.21+`

Live telemetry, real-time warning HUD, and component malfunction monitoring for wash plants, sluice mats, feeding chains, and vehicles in *Gold Mining Simulator* (*Gold Rush: The Game*).

---

## 1. Features & Overview

- **Real-Time Warning HUD Overlay**:
  - Live on-screen tactical dashboard monitoring all active equipment across the claim.
  - Automatically identifies and groups machinery into three distinct setups:
    - **Setup 1**: Mobile Wash Plants (Mini Wash Plant, Mobile Wash Plant).
    - **Setup 2**: Stationary Setup T3–T5 (Large Shaker, Trommel, Duplex Jigs, Sluice Boxes).
    - **Setup 3**: Setup T6 / Orange Beast (Giant Shaker, Extended Sluices).
  - Optional monitoring of the **Feeding Chain** (Hoppers and Conveyors) linked to stationary setups.
- **Critical Malfunction Warnings**:
  - **Trommel**: Broken or destroyed drive chain (`_TrommelChainDestroyed`).
  - **Shaker**: Motor stoppage, power outage, or broken water supply hose.
  - **Duplex Jig**: Broken pump mechanism or full buckets requiring replacement.
  - **Sluice Mats**: Warning when mats reach high fill capacity (default: 90%), and critical alerts when mats overflow (100%).
  - **Vehicles & Machines**: Low fuel warnings (< 15%) and critical alerts when fuel tanks run dry.
  - **Utilities**: Power generator stoppages, empty water towers, and disabled pumps.
- **Interactive In-Game Customization**:
  - Configurable position, width, height, and display modes (Full or Ultra-Compact) via the CoreMod Menu (**`Insert`** key).
  - Drag and drop window repositioning directly on screen.
  - Optional mode to only show the HUD when active warnings exist.
- **Built-in Diagnostic Inspector & Claim Dumper**:
  - Press **`F3`** or **`F8`** to open the Diagnostic Inspector for deep inspection of live Unity scene objects and properties.
  - One-click deep memory dump to file (`BepInEx/plugins/Milex_ClaimMonitor_Dumps/`) for troubleshooting.

---

## 2. Documentation & Navigation

| Topic | Description | Link |
|---|---|---|
| **Root Monorepo** | Overview of the full Milex mod collection | [Main README](../../../README.md) |
| **Changelog** | Version history and release notes | [ClaimMonitor Changelog](CHANGELOG.md) |
| **Developer Guide** | Guide for building Sub-Mods on CoreMod | [Developer Guide](../../../DEVELOPER_GUIDE.md) |
| **AI Agent Blueprint** | Architectural blueprint for AI coding agents | [Agent Guide](../../../AGENT_MOD_GUIDE.md) |

---

## 3. Installation & Getting Started

1. Ensure **BepInEx 5** (x64) and **`Milex_GMS1_CoreMod.dll`** are installed in your game directory.
2. Place `Milex_GMS1_ClaimMonitor.dll` into your `BepInEx/plugins/` folder.
3. Start the game. The Warning HUD will appear automatically on your screen.
4. Press **`Insert`** to open the Milex Mod Menu and configure alert thresholds or reposition the HUD.

---

## 4. In-Game Usage & Hotkeys

- **`Insert`**: Opens the CoreMod in-game configuration menu to adjust settings and thresholds.
- **`F3` / `F8`**: Toggles the Diagnostic Inspector window for raw object and component diagnostics.
- **HUD Drag & Drop**: Click and drag the top title bar of the Warning HUD to reposition it on your screen. Positions are saved automatically.
- **Minimize / Expand Button**: Click `[ - Minimize ]` on the HUD to toggle between compact status and full alert lists.

---

## 5. Configuration Reference (`Milex_GMS1_ClaimMonitor.cfg`)

Configuration is saved in `BepInEx/config/Milex_GMS1_ClaimMonitor.cfg`:

### Section `[Setups]` (Wash Plant Setups)

| Key | Type | Default | Description |
|---|---|---|---|
| **`MonitorSetup1`** | `Boolean` | `true` | Monitor mobile wash plants in the Warning HUD. |
| **`MonitorSetup2`** | `Boolean` | `true` | Monitor stationary setup (T3–T5) in the Warning HUD. |
| **`Setup2IncludeFeedingChain`** | `Boolean` | `false` | Include hoppers and conveyors in Setup T3–T5 evaluation. |
| **`MonitorSetup3`** | `Boolean` | `true` | Monitor Setup T6 (Orange Beast) in the Warning HUD. |
| **`Setup3IncludeFeedingChain`** | `Boolean` | `false` | Include hoppers and conveyors in Orange Beast evaluation. |

### Section `[Thresholds]` (Alert Thresholds)

| Key | Type | Default | Description |
|---|---|---|---|
| **`MatWarningThreshold`** | `Single` | `90.0` | Mat fill percentage threshold to trigger an attention warning (70%–98%). |
| **`VehicleLowFuelThreshold`** | `Single` | `15.0` | Vehicle fuel percentage threshold to trigger low fuel warning (5%–30%). |

### Section `[WarningHUD]` (Warning HUD Settings)

| Key | Type | Default | Description |
|---|---|---|---|
| **`HudEnabled`** | `Boolean` | `true` | Master switch for the on-screen Warning HUD. |
| **`HudOnlyShowWarnings`** | `Boolean` | `false` | Automatically hides the HUD when all equipment is nominal. |
| **`HudCompactMode`** | `Boolean` | `false` | Displays the HUD in an ultra-compact single line badge. |
| **`HudPosX`** | `Single` | `20.0` | Horizontal screen position in pixels. |
| **`HudPosY`** | `Single` | `100.0` | Vertical screen position in pixels. |
| **`HudMaxWidth`** | `Single` | `340.0` | Maximum width of the HUD container. |
| **`HudMaxHeight`** | `Single` | `420.0` | Maximum height of the HUD container. |

---

## 6. Localization Files

Translation templates are extracted automatically to:
- `BepInEx/plugins/Milex GMS1 Mod Localization/Milex_GMS1_ClaimMonitor_en.json`
- `BepInEx/plugins/Milex GMS1 Mod Localization/Milex_GMS1_ClaimMonitor_de.json`

---

## 7. Development & Compilation

Compile with the .NET SDK:
```powershell
dotnet build src/Mods/ClaimMonitor/Milex_GMS1_ClaimMonitor.csproj
```
Assemblies are deployed automatically to `BepInEx/plugins/`.
