# Milex GMS1 Claim Monitor
### Real-Time Warning HUD, Mat Tracking & Fleet Fuel Gauges

[![Gold Mining Simulator Compatible](https://img.shields.io/badge/Game-Gold%20Mining%20Simulator-gold.svg)](#)
[![Version 1.0.10](https://img.shields.io/badge/Version-1.0.10-blue.svg)](#)
[![Requires Milex CoreMod](https://img.shields.io/badge/Requires-Milex%20CoreMod%201.4%2B-orange.svg)](#)
[![BepInEx 5 Required](https://img.shields.io/badge/Requires-BepInEx%205.4%2B-green.svg)](#)

---

## What Does This Mod Do?

Have you ever spent an hour happily digging paydirt, only to walk over to your wash plant and realize your Miner's Moss mats reached 100% full 20 minutes ago—meaning tons of precious gold was washed right down the drain? 

Or had your wash plant grind to a halt because your generator silently ran out of diesel or a trommel drive chain snapped without you noticing?

**Milex GMS1 Claim Monitor** keeps you in full control of your claim. It puts a customizable, live warning HUD on your screen that tracks your wash plant mats, equipment durability, water pumps, generator fuel, and vehicle fleet in real time.

---

## Highlights

- **Live Mat Fill Tracking (Never Lose Gold Again)**:
  - Tracks the exact fill percentage of Miner's Moss mats, nugget traps, and Jig buckets across all wash plant setups (Tier 2 Mobile, Tier 3–5 Stationary, and Tier 6 Orange Beast).
  - Gives you a yellow warning when mats reach 90% and a pulsing red alarm when they hit 100% so you can clean them out in time.
- **Breakdown & Wear Warnings**:
  - Warns you before belts, chains, motors, or gravel pumps break down so you can fix them before your entire mining operation stalls.
- **Generator & Water Pump Alerts**:
  - Keeps an eye on the fuel in your main claim diesel generator and warns you when fuel is getting low.
  - Alerts you if water pumps run out of fuel or lose water pressure.
- **Fleet Fuel Gauges**:
  - Adds clean vertical fuel bars right into the top vehicle switcher bar! You can see the exact fuel level of every excavator, loader, and dump truck on your claim without having to get into the cabin.
- **Smart Standby (No False Alarms)**:
  - Spare water pumps without hoses or extra generators parked in your yard are recognized as on standby. They won't trigger annoying warnings or clutter your screen.
- **"Warnings Only" Mode (Clean Screen)**:
  - Prefer a clean, immersive screen? Turn on "Warnings Only" mode. The HUD stays completely invisible while everything is running smoothly, and only pops up when something actually needs your attention (like low fuel or full mats).
- **Movable & Compact**:
  - Drag the warning window anywhere on your screen, or click the minimize button to collapse it into a tiny single-line status badge.

---

## Quick Install Guide

1. Make sure you have **BepInEx 5 (x64)** and **[Milex GMS1 CoreMod](https://www.nexusmods.com/goldrush/mods/xxx)** installed.
2. Put `Milex_GMS1_ClaimMonitor.dll` into your `Gold Mining Simulator/BepInEx/plugins/` folder.
3. Start the game, load your claim, and your warning HUD will appear automatically when machines are running!

---

## Controls

| Key / Control | What It Does |
|---|---|
| **`Insert`** | Opens the Milex menu. Click **Claim Monitor** to adjust thresholds and settings. |
| **Click & Drag Header** | Move the Warning HUD window anywhere on your screen. |
| **`[ - Minimize ]`** | Collapses the HUD into a compact status badge (or expands it back). |
| **`F3`** *(Optional)* | Advanced technical screen showing raw machinery stats across your claim. |

---

## Settings & Customization

You can change everything in the in-game menu (**`Insert`**) or in:
`BepInEx/config/Milex_GMS1_ClaimMonitor.cfg`

- **`HudEnabled`** (Default: `true`): Master switch for the on-screen Warning HUD.
- **`HudOnlyShowWarnings`** (Default: `false`): Keeps the HUD hidden until an alert or warning is triggered.
- **`HudCompactMode`** (Default: `false`): Turns the HUD into a compact single-line badge.
- **`MatWarningThreshold`** (Default: `90%`): Choose at what mat percentage the early warning triggers.
- **`VehicleLowFuelThreshold`** (Default: `15%`): Choose at what fuel percentage the vehicle low-fuel alert triggers.
- **`ComponentWearWarningThreshold`** (Default: `20%`): Durability percentage that triggers a repair alert before parts break.
- **`ShowFuelInVehicleSwitcher`** (Default: `true`): Displays fuel gauges in the top vehicle bar.

---

## Requirements & Step-by-Step Install

### Requirements
- *Gold Mining Simulator* (*Gold Rush: The Game*) on Steam.
- [BepInEx 5 (version 5.4.21 or newer, x64)](https://github.com/BepInEx/BepInEx/releases).
- **[Milex GMS1 CoreMod (v1.4.0 or newer)](https://www.nexusmods.com/goldrush/mods/xxx)**.

### How to Install
1. Make sure BepInEx 5 and Milex CoreMod are installed.
2. Download this mod.
3. Place `Milex_GMS1_ClaimMonitor.dll` into your `Gold Mining Simulator/BepInEx/plugins/` folder.
4. Launch the game and start mining!

### How to Uninstall
Delete `Milex_GMS1_ClaimMonitor.dll` from `BepInEx/plugins/`. Your save games remain completely untouched.

---

## Changelog

### Version 1.0.10
- Added equipment baseline exporter in the optional F3 diagnostic screen for modders.

### Version 1.0.9
- HUD and scanner now sleep during loading screens and main menus to save system performance.

### Version 1.0.8
- Disconnected or unused conveyors no longer trigger false "missing power" warnings on new claims.
- Fixed window height clamping on lower screen resolutions.

### Version 1.0.0 – 1.0.7
- Initial release featuring the live Warning HUD, vehicle switcher fuel bars, mat tracking across all wash plant tiers, and smart standby filtering.
