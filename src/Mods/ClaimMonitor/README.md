# Milex GMS1 Claim Monitor

- **Version:** `1.0.10` ([View Changelog](CHANGELOG.md))
- **Mod Name:** Milex GMS1 Claim Monitor
- **Author:** Milex
- **Assembly File:** `Milex_GMS1_ClaimMonitor.dll`
- **Dependencies:** `Milex_GMS1_CoreMod.dll`, `BepInEx 5.4.21+`

The **Claim Monitor** is a real-time tactical warning HUD for *Gold Mining Simulator* (*Gold Rush: The Game*). It actively scans your claim and monitors wash plants, sluice box mats, water pumps, power generators, and vehicles. It warns you before equipment breaks down, alerts you when Miner's Moss mats reach capacity, and notifies you when machinery runs low on fuel or water.

---

## Quick Install & Getting Started

1. **Prerequisites**: Ensure you have **BepInEx 5** (x64) and **`Milex_GMS1_CoreMod.dll`** installed in your game directory.
2. **Install**: Copy `Milex_GMS1_ClaimMonitor.dll` into your `BepInEx/plugins/` folder.
3. **Start Game**: Launch the game and load into your claim.
4. **Quick Start**: 
   - The Warning HUD will appear on your screen automatically whenever active machinery is running on your claim.
   - Press **`Insert`** to open the Milex Mod Menu if you want to fine-tune alert thresholds, change window size, or customize HUD behavior.

---

## In-Game Controls & Usage

- **`Insert` (Mod Menu)**: Opens the central Milex configuration menu. From here you can toggle the HUD on or off, adjust warning thresholds (mat fill %, vehicle fuel %), and choose which setups to monitor.
- **Move the HUD**: Click and drag the top title bar of the Warning HUD anywhere on your screen. Its position is saved automatically.
- **Minimize HUD**: Click `[ - Minimize ]` on the HUD header to collapse it into an ultra-compact status badge. Click it again to expand the full alert list.
- **Instant Mod Toggle**: You can disable the mod at any time in the menu sidebar. The HUD and vehicle fuel bars disappear immediately with zero leftover UI elements.

---

## Detailed Features & Functions

### 1. Tactical Warning HUD
The Warning HUD operates with three severity tiers: **Critical** (Red), **Warning** (Yellow), and **Nominal** (Green). It groups your equipment into familiar in-game mining setups:

- **Setup 1: Mobile Wash Plants (Tier 2)**:
  - Monitors the **Mini Wash Plant** (including its internal diesel engine fuel) and the **Mobile Wash Plant**.
  - Tracks Hog Pan hopper dirt level, water flow, and Miner's Moss mats.
- **Setup 2: Stationary Plant (Tier 3 to Tier 5)**:
  - **Large Shaker**: Alerts if power fails, the motor stops, or water pressure drops.
  - **Trommel** (Standard, Reinforced, and Old Arnold): Alerts immediately if the drive chain snaps.
  - **Duplex Jig & Gravel Pump**: Alerts if the jig mechanism breaks or if concentrate collection buckets are full and need replacement.
  - **Sluice Boxes & Miner's Moss**: Warns when mats reach your chosen fill threshold (default: 90%) and triggers an urgent alarm upon overflow (100%).
- **Setup 3: Giant Setup (Tier 6 / Orange Beast)**:
  - Tracks the giant Orange Beast shaker, trommel, nugget traps, and extended sluice rows.
- **Feeding Chain (Optional)**:
  - Can be toggled on to monitor Feeder Hoppers and Conveyor Belts for earth jams, motor stoppages, and power disconnects.
- **Vehicle & Machine Fuel**:
  - Monitors fuel tanks across all your vehicles: Pickup, Small Excavator, Large Excavator, Wheel Loader, Dump Truck, Backhoe Loader, and Bulldozer.
  - Triggers an early warning when fuel dips below 15% and a critical alarm when empty.
- **Power & Water Utilities**:
  - Monitors fuel levels in the large diesel generator and fuel tanks.
  - Monitors water flow from small and large water pumps as well as water towers.

### 2. Smart Standby & Parked Machine Protection
The Claim Monitor understands how you work on your claim:
- Unconnected, parked, or spare equipment (pumps without hoses, generators without cables, or unhooked mobile wash plants) is recognized as **standby** and will never trigger annoying false alarms.
- The 10 individual switch buttons on the large generator can be excluded from wear alerts (`MonitorGeneratorSwitchButtons = false`) to keep your HUD clean.
- During loading screens and in the main menu, background scanning and HUD rendering are fully paused to save CPU cycles.

### 3. Vehicle Switcher Fuel Bar
- Integrates a colored vertical fuel bar and percentage directly into the game's top vehicle quick-switching bar (left of each vehicle icon), allowing you to check the fuel status of your entire fleet at a glance without blocking vehicle names or distance text.

### 4. Flexible HUD Display Modes
- **Always Visible**: Shows current equipment status continuously.
- **Warnings Only Mode (`HudOnlyShowWarnings = true`)**: Keeps the HUD completely invisible during normal operation and only displays it when equipment actually requires your attention (e.g. low fuel, full mats, or broken parts).
- **Compact Mode**: Reduces the HUD to a small single-line status badge to maximize screen real estate.

---

## Configuration Reference (`Milex_GMS1_ClaimMonitor.cfg`)

All settings can be adjusted in the in-game menu (**`Insert`**) or saved in `BepInEx/config/Milex_GMS1_ClaimMonitor.cfg`:

### Section `[General]`

| Key | Type | Default | Range | Description |
|---|---|---|---|---|
| **`ScanIntervalSeconds`** | `Float` | `3.0` | `1.0` to `30.0` | Seconds between background equipment scans. |

### Section `[Setups]`

| Key | Type | Default | Description |
|---|---|---|---|
| **`MonitorSetup1`** | `Boolean` | `true` | Monitor mobile wash plants (Mini Wash Plant & Mobile Wash Plant). |
| **`MonitorSetup2`** | `Boolean` | `true` | Monitor stationary setup (Tier 3–5 Shakers, Trommels, Duplex Jigs, Gravel Pump). |
| **`Setup2IncludeFeedingChain`** | `Boolean` | `false` | Include Feeder Hoppers and Conveyors in Setup 2 health evaluation. |
| **`MonitorSetup3`** | `Boolean` | `true` | Monitor Setup Tier 6 (Orange Beast) wash plants and extended sluices. |
| **`Setup3IncludeFeedingChain`** | `Boolean` | `false` | Include Feeder Hoppers and Conveyors in Orange Beast evaluation. |
| **`MonitorGeneratorSwitchButtons`** | `Boolean` | `false` | Monitor wear on individual generator switch buttons (disabled by default to avoid clutter). |

### Section `[Thresholds]`

| Key | Type | Default | Range | Description |
|---|---|---|---|---|
| **`MatWarningThreshold`** | `Float` | `90.0` | `70.0` to `98.0` | Mat fill percentage threshold to trigger an early attention warning. |
| **`VehicleLowFuelThreshold`** | `Float` | `15.0` | `5.0` to `30.0` | Vehicle fuel percentage threshold to trigger a low fuel warning. |
| **`ComponentWearWarningThreshold`** | `Float` | `20.0` | `5.0` to `50.0` | Durability percentage threshold to trigger a repair warning before total machine failure. |

### Section `[WarningHUD]`

| Key | Type | Default | Range | Description |
|---|---|---|---|---|
| **`HudEnabled`** | `Boolean` | `true` | - | Master toggle for the on-screen Warning HUD. |
| **`HudOnlyShowWarnings`** | `Boolean` | `false` | - | Automatically hides the HUD when all equipment is nominal. |
| **`HudCompactMode`** | `Boolean` | `false` | - | Displays the HUD in an ultra-compact single-line badge format. |
| **`HudMaxWidth`** | `Float` | `340.0` | `200.0` to `800.0` | Maximum width of the HUD window in pixels. |
| **`HudMaxHeight`** | `Float` | `420.0` | `100.0` to `1000.0` | Maximum height of the HUD window in pixels. |

### Section `[Fuel]`

| Key | Type | Default | Description |
|---|---|---|---|
| **`ShowFuelInVehicleSwitcher`** | `Boolean` | `true` | Displays real-time fuel status bars inside the vehicle quick-switch bar. |

---

## Optional Diagnostics for Modders & Troubleshooting (`F3` Overlay)

For mod creators or technical troubleshooting, pressing **`F3`** (or `F8`) opens an optional diagnostic overlay that displays raw machinery data (exact durability values, fluid capacities, and power connections) across your claim. It also includes a button to dump physical claim data to JSON files. Regular players will never need this screen during normal gameplay.

---

## Localization Files

Language files are managed automatically by CoreMod:
- `BepInEx/plugins/Milex GMS1 Mod Localization/Milex_GMS1_ClaimMonitor_en.json` (English)
- `BepInEx/plugins/Milex GMS1 Mod Localization/Milex_GMS1_ClaimMonitor_de.json` (German)

---

## Development & Compilation

To compile this project from source:

```powershell
dotnet build GMSModding.sln
```

---

## License & Free Use (Open Source)

All code in **Milex GMS1 Claim Monitor** is free and open source:
> Anyone is free to use, copy, modify, adapt, or incorporate this code into other mods and projects, in whole or in part, without restriction.
