# Milex GMS1 Production Tuner
### The Ultimate Speed, Capacity & Physics Fix Mod for Gold Mining Simulator

[![Gold Mining Simulator Compatible](https://img.shields.io/badge/Game-Gold%20Mining%20Simulator-gold.svg)](#)
[![Version 1.4.14](https://img.shields.io/badge/Version-1.4.14-blue.svg)](#)
[![Requires Milex CoreMod](https://img.shields.io/badge/Requires-Milex%20CoreMod%201.4%2B-orange.svg)](#)
[![BepInEx 5 Required](https://img.shields.io/badge/Requires-BepInEx%205.4%2B-green.svg)](#)

---

## What Does This Mod Do?

Let’s be honest: vanilla *Gold Mining Simulator* has plenty of annoying moments. 
- Having to dig 5 times with a shovel just to fill a single bucket.
- Parking your excavator on flat ground, driving your dump truck away to unload, and coming back to find your excavator mysteriously lying on its side.
- Wash plant mats filling completely unevenly—one mat is at 100% while your buckets are stuck at 20%.
- Fast travel leaving your trailer behind or blowing out your truck tires.
- Fuel hoses that are too short to reach your machinery.

**Milex GMS1 Production Tuner** fixes all of this. It gives you full control over machine speeds, bucket sizes, and wash plant capacities via an easy in-game menu (**`Insert`**), while fixing the most frustrating vehicle physics bugs in the game.

---

## Highlights

### 1. Digging & Hand Tools
- **1-Scoop Shovel Filling**: Your shovel fills to 100% in a single scoop. No more repetitive digging just to move dirt.
- **1 Shovel Scoop = 1 Full Bucket**: Perfectly balanced for early game so you can work fast.
- **Carry More in Buckets**: Crank up bucket sizes so you spend less time running back and forth.
- **No Spilled Gold**: When pouring big buckets into your gold pan, pouring stops automatically when the pan is full—zero wasted dirt or lost gold flakes.

### 2. Heavy Machinery & Physics Fixes
- **No More Toppling Excavators**: Parked your excavator on solid ground? With this mod, it stays upright! When parked or when the handbrake is on, the undercarriage locks firmly to the ground so it won't roll over while you're away hauling paydirt.
- **DLC Mini Excavator Glitch Fixed**: Buying or transporting the DLC mini excavator no longer makes it fly through the air and tumble uncontrollably.
- **Instant Flip Recovery**: Accidental rollover on a steep pit slope? Just step into the cabin—your machine immediately sets itself upright right where you are! No more getting teleported all the way across the map to the claim entrance depot.
- **Hydraulic Speed Boost**: Make sluggish excavator arms, boom lifts, and turret rotations as fast and responsive as you want.
- **Wheel Loader Power**: Front loaders can carry more dirt and get an automatic lift boost so heavy loads won't bog down the front wheels.
- **Bigger Haul Trucks & Faster Belts**: Haul more paydirt per trip in your dump truck and speed up conveyor belt feeding.

### 3. Wash Plants That Actually Fill Evenly
- **Synchronized T3–T5 Setup**: No more lopsided plants! A single setup slider scales your 12 Sluice mats, nugget grates, Jig buckets, and tailing Hog Pan in **100% lockstep**. Everything fills together and reaches 100% at the exact same moment so you can wash everything in one clean trip.
- **Orange Beast Synchronized Slider**: Scales the 20 giant Orange Beast mats and gold collection together.
- **Safe Generators & Pumps**: Faster washing speeds do NOT draw more electricity or water. Your generators won't trip fuses and water pumps won't lose pressure.

### 4. Logistics & Fast Travel Fixes
- **Trailers Stay Hitched**: Fast travel with a pickup and trailer no longer leaves the trailer behind in the dirt. It stays hitched and ready to drive, and you can still detach it manually at the hitch lever anytime.
- **Tire Blowout Protection**: Tires will no longer blow out or break when landing after fast travel. Normal driving wear stays active, but teleport shock damage is gone.
- **Massive Fuel Trailers & Long Hoses**:
  - Fuel trailer holds up to **7,500 L** and claim tank holds up to **75,000 L**.
  - Long flexible hoses easily reach your machines without parking inches away.
  - Small gear (generators, small water pumps) keeps normal tank sizes so you won't get huge fuel bills at the town gas station.

---

## Quick Install Guide

1. Make sure you have **BepInEx 5 (x64)** and **[Milex GMS1 CoreMod](https://www.nexusmods.com/goldrush/mods/xxx)** installed.
2. Put `Milex_GMS1_ProductionTuner.dll` into your `Gold Mining Simulator/BepInEx/plugins/` folder.
3. Start the game, press **`Insert`**, click **Production Tuner** in the sidebar, and tweak your sliders!

---

## In-Game Controls

| Key / Control | What It Does |
|---|---|
| **`Insert`** | Opens the mod menu. Select **Production Tuner** to view all sliders. |
| **Category Tabs** | Quickly jump between *Hand Tools*, *Vehicles*, *Wash Plants*, *Fine Processing*, and *Trailers*. |
| **Sliders** | Smooth sliders let you set exact multipliers (e.g. 2.0x, 5.0x, 10.0x). |
| **Reset Category Button** | Resets only the current group back to its recommended default values. |
| **Sidebar Active Toggle** | Turn the mod off with one click to test vanilla behavior without restarting. |

---

## Main Settings Overview

All multipliers can be set in-game (**`Insert`**) or in `BepInEx/config/Milex_GMS1_ProductionTuner.cfg`:

| Category | Setting | Default | What It Does |
|---|---|---|---|
| **Hand Tools** | Shovel Scoop Size | `6.0x` | Fills shovel in 1 scoop; 1 scoop = 1 full bucket. |
| | Bucket Capacity | `2.0x` | Dirt capacity of handheld buckets. |
| | Handheld Hog Pan | `6.6x` | Capacity of the portable early-game Hog Pan. |
| **Vehicles** | Excavator Bucket Size | `3.0x` | Move 3x more dirt per excavator scoop. |
| | Excavator Arm / Boom Speed | `2.0x` | Makes excavator arm lifting twice as fast. |
| | Excavator Swing Speed | `2.0x` | Makes cabin rotation twice as fast and responsive. |
| | Wheel Loader Capacity | `3.0x` | Loader bucket carries more dirt with extra lift power. |
| | Dump Truck Bed | `3.0x` | Haul 3x more paydirt per trip. |
| | Conveyor Belts | `3.0x` | Moves dirt faster on heavy conveyor systems. |
| **Wash Plants** | T3–T5 Synchronized Setup | `5.0x` | Scales all mats, grates, and jig buckets in lockstep! |
| | Orange Beast Setup | `2.0x` | Scales T6 giant mats and gold recovery together. |
| | Feeder Hoppers | `3.0x` | Hoppers hold more dirt so feeding is smoother. |
| | Trommels & Shakers | `3.0x` | Speeds up gravel washing across all plant tiers. |
| **Logistics** | Mobile Fuel Trailer | `3.0x` | Expands fuel trailer to 7,500 Liters. |
| | Claim Fuel Tank | `3.0x` | Expands stationary tank to 75,000 Liters. |
| | Fuel Hose Length | `5.0x` | Long flexible hoses that reach your equipment easily. |

---

## Requirements & Step-by-Step Install

### Requirements
- *Gold Mining Simulator* (*Gold Rush: The Game*) on Steam.
- [BepInEx 5 (version 5.4.21 or newer, x64)](https://github.com/BepInEx/BepInEx/releases).
- **[Milex GMS1 CoreMod (v1.4.0 or newer)](https://www.nexusmods.com/goldrush/mods/xxx)**.

### How to Install
1. Make sure BepInEx 5 and Milex CoreMod are already installed.
2. Download this mod.
3. Place `Milex_GMS1_ProductionTuner.dll` into your `Gold Mining Simulator/BepInEx/plugins/` directory.
4. Launch the game!

### How to Uninstall
Delete `Milex_GMS1_ProductionTuner.dll` from `BepInEx/plugins/`. All vehicle physics, speeds, and capacities immediately revert back to 100% normal vanilla.

---

## Changelog

### Version 1.4.14
- Fixed newly delivered or teleported excavators hovering in mid-air so they land cleanly on the ground before stabilization engages.

### Version 1.4.13
- Added excavator anti-topple stability: parked excavators will never roll over onto their side while you haul dirt.
- Fixed the DLC mini excavator glitch where it would spin and fly through the air after purchase or transport.
- Added instant upright recovery: entering an overturned vehicle sets it upright on its tracks right where you were working.

### Version 1.4.12
- Added synchronized wash plant scaling: single slider scales Sluice mats, grates, Jig buckets, and Hog Pan in 100% lockstep.
- Added synchronized scaling for the Tier 6 Orange Beast plant.

### Version 1.4.0 – 1.4.11
- Added trailer fast-travel auto-reconnect (trailers stay attached with hitch levers fully working).
- Added fast travel tire shock protection (tires won't pop on teleport landing).
- Added 1-scoop shovel filling (1 scoop = 1 full bucket).
- Added ultra-long fuel hoses and enlarged fuel trailer/tanks.
- Added smooth continuous sliders for all 29 settings.
