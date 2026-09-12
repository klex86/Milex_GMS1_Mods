# Milex GMS1 Mods - Framework & Mod Collection

A comprehensive, modular modding suite for **Gold Mining Simulator** (*Gold Rush: The Game*), built on BepInEx 5 and Harmony. It features a modern dual-engine in-game configuration dashboard, an automated Memory & VRAM cleaner that eliminates long-session FPS drops, real-time tactical equipment monitoring, and deep production & hydraulics tuning across all mining tiers.

---

## Documentation & Navigation

Direct links to user guides, technical documentation, and changelogs for all components:

| Component | Description | Guide / Readme | Changelog |
|---|---|---|---|
| **Entire Monorepo** | Global overview, architecture, and installation | [Main Documentation](README.md) | [Root Changelog](CHANGELOG.md) |
| **Milex GMS1 CoreMod** | Central framework, dual-engine menu, memory cleaner & localization | [CoreMod Guide](src/CoreMod/README.md) | [CoreMod Changelog](src/CoreMod/CHANGELOG.md) |
| **Milex GMS1 Production Tuner** | Multipliers for capacities, processing speeds, hydraulics & logistics across 29 components | [Production Tuner Guide](src/Mods/ProductionTuner/README.md) | [Production Tuner Changelog](src/Mods/ProductionTuner/CHANGELOG.md) |
| **Milex GMS1 Claim Monitor** | Real-time tactical warning HUD, equipment health, sluice mat alerts, and vehicle fuel | [Claim Monitor Guide](src/Mods/ClaimMonitor/README.md) | [Claim Monitor Changelog](src/Mods/ClaimMonitor/CHANGELOG.md) |
| **Milex GMS1 HelloMod** | Example demonstration sub-mod for developers | [HelloMod Guide](src/Mods/HelloMod/README.md) | [HelloMod Changelog](src/Mods/HelloMod/CHANGELOG.md) |
| **Sub-Mod Developer Guide** | Handbook for human developers creating CoreMod-compatible mods | [Developer Guide](DEVELOPER_GUIDE.md) | - |
| **AI Agent Blueprint** | Technical prompt and architectural guidelines for AI coding agents | [Agent Guide](AGENT_MOD_GUIDE.md) | - |
| **Community Knowledge Compendium** | Reverse-engineering findings from 10 community mods and vanilla codebase | [Knowledge Compendium](Community%20Knowledge/COMMUNITY_KNOWLEDGE_COMPENDIUM.md) | - |
| **Keybindings Reference** | Comprehensive list of hotkeys and in-game rebindings | [Keybindings Guide](KEYBINDINGS.md) | - |

---

## Quick Install & Getting Started

1. **Prerequisite**: Install **BepInEx 5** (x64) into your main game directory (`GoldMiningSimulator/` or `GoldRushTheGame/`).
2. **Install Mods**: Copy the compiled mod `.dll` files (at minimum `Milex_GMS1_CoreMod.dll`, plus any desired sub-mods) into your `BepInEx/plugins/` directory.
3. **Start Game**: Launch the game normally.
4. **Quick Start**:
   - Press **`Insert`** at any time in-game to open the Milex Mod Menu.
   - Adjust sliders, switch languages, or customize HUD warnings on the fly. All changes take effect immediately!

---

## In-Game Controls & Framework Usage

- **`Insert` (Mod Menu)**: Opens and closes the mod dashboard. Camera movement and tool switching are paused while navigating settings.
- **Live Sub-Mod Toggling**: Enable or disable any sub-mod in real time from the menu sidebar without restarting the game. All machines revert to 100% vanilla when a mod is turned off.
- **Central Memory & VRAM Cleaner**: Automatically purges unreferenced textures and memory during Fast Travel, quicksaving, autosaving, laptop access, and menu pauses to eliminate the game's progressive FPS degradation over long play sessions. Includes a manual one-click clean button with real-time status reporting.
- **Dual-Engine UI**: Seamlessly toggle between the modern card dashboard (uGUI Canvas) and the lightweight classic IMGUI fallback in `General` -> `UI Settings`.

---

## Included Mods Overview

### 1. [Milex GMS1 CoreMod](src/CoreMod/README.md)
The central management framework:
- **Modern Canvas Dashboard**: Fast, sleek card interface with live search filtering, category tabs, draggable header, and smooth controls. Built for silky performance on high-refresh monitors (165+ FPS).
- **Anti-FPS-Drop Memory Engine**: Purges unreferenced assets in the background during natural transition moments (Fast Travel, saving, laptop access) so your framerate stays smooth.
- **Engine Recovery**: Safely protects game loading states and features an emergency unpause button to recover from loading glitches.
- **Localization Engine**: Multi-language support with automatic game-language synchronization and an in-game template generator.

### 2. [Milex GMS1 Production Tuner](src/Mods/ProductionTuner/README.md)
Fine-grained speed, capacity, and logistics tuning across 29 components:
- **Hand Tools**: 1-scoop shovel filling (1 shovel scoop fills 1 bucket to 100%), water-balanced Hog Pan, and Gold Pan spill/flake protection.
- **Heavy Machinery**: Independent hydraulic sliders for excavator boom, turret rotation, and bucket tilt; torque-boosted Wheel Loader buckets; Backhoe Loader and Dump Truck capacity scaling.
- **Wash Plants & Sluices**: Synchronized setup multipliers for T3–T5 plants and T6 Orange Beast ensuring all connected mats, grates, jig buckets, and hog pans fill in 100% lockstep without overflow; safe water/electrical baseline load.
- **Fine Processing**: Faster processing speeds and expanded capacities for Gold Nuggetator, Magnetite Separator, and Wave Table.
- **Fuel Logistics**: Enlarged Mobile Fuel Trailer and stationary Claim Tank, faster pump speed, and extended fuel hose reach so you don't need to park millimeter-close.
- **Fast Travel & Trailer Fixes**: Keeps trailers hitched after fast travel, restores the physical hitch lever, and shields tires from landing damage.

### 3. [Milex GMS1 Claim Monitor](src/Mods/ClaimMonitor/README.md)
Real-time tactical warning HUD for your claim:
- **Tiered Warning HUD**: Groups equipment into familiar setups (Mobile, Stationary Tier 3-5, Orange Beast, and Feeding Chain). Alerts you before machines break or run dry on fuel or water.
- **Sluice Mat Alerts**: Warns when Miner's Moss reaches threshold capacity (default: 90%) and triggers an urgent alarm on overflow (100%).
- **Vehicle Fuel Switcher Bar**: Adds clean vertical fuel gauges and percentages beside vehicle icons in the game's top quick-switch bar.
- **Smart Standby Protection**: Ignores disconnected or parked machines so you never get false alarms.

### 4. [Milex GMS1 HelloMod](src/Mods/HelloMod/README.md)
- Reference sub-mod demonstrating hotkey binding (`F2`), configuration synchronization, and multi-language support.

---

## Configuration & Localization Architecture

### Configuration Files (`.cfg`)
- Located in `BepInEx/config/`.
- Each mod creates and manages its own configuration file matching its assembly name (e.g. `Milex_GMS1_CoreMod.cfg`, `Milex_GMS1_ProductionTuner.cfg`, `Milex_GMS1_ClaimMonitor.cfg`).
- Settings can be adjusted live in-game or edited with any text editor.

### Localization Files (`.json`)
- Located in `BepInEx/plugins/Milex GMS1 Mod Localization/`.
- Embedded English (`_en.json`) and German (`_de.json`) translation templates are automatically extracted on first launch.

---

## Guide: Generating Templates for New Languages

If you play in a language without an existing translation file (e.g. French, Polish, Spanish, Portuguese):

1. Press **`Insert`** in-game to open the mod menu.
2. Navigate to **General** and disable **`Use Game Language`**.
3. Under **`Select Language`**, choose your desired language.
4. When prompted by the menu, click **`Create Templates`**.
5. CoreMod immediately creates editable JSON template files for all active mods in `BepInEx/plugins/Milex GMS1 Mod Localization/`.
6. Edit the `.json` files in any text editor and restart the game to enjoy your translations!

---

## Building from Source

To compile the entire solution:

```powershell
dotnet build GMSModding.sln
```

Build scripts automatically compile all projects and deploy the finished DLLs directly into your `BepInEx/plugins/` directory.

---

## Acknowledgements & Community Credits

Sincere thanks and credits to the pioneers of the Gold Rush modding community whose early reverse-engineering insights helped inspire this project:
- **stregkoden**: *Better_Conveyor*, *Better_FEL*, *Better_FuelTrailer*, *Better_Nuggetator*, *HogPan_Pack*.
- **DeepCore / Jonathan**: *Bigger Shovel*, *Smart Buckets*, *Gold Bar Totals*.
- **FedeRama**: *GMS.WaveTableCapacity*.
- **Mishuuw**: *Allow Debt Smelting*.
- **DSS**: *DSS PickupSnap* (Trailer snapping and hitch trigger insights).
- **GMS Community Modders**: *Increased Capacity And Speed*, *IncreasedCapacity*, *AE Groundworks*, *Crazy Mods*, *Longer Range Pumps*.

---

## License & Free Use

All code in **Milex GMS1 Mods** is released under an open-source license:
> Anyone is free to use, copy, modify, adapt, or incorporate this code into their own mods and projects. Attribution or a brief mention in your project's credits is appreciated.
