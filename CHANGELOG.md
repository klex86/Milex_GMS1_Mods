# Changelog - Milex GMS1 Mods

All notable changes and releases for this mod collection are documented in this file.
This format is based on [Keep a Changelog](https://keepachangelog.com/).

---

## [1.6.0] - 2026-09-02

### Added: Production Tuner Vehicle & Logistics Expansion

- **Mobile Conveyors (Frankenstein & Cordylus)**:
  - Added directly under the Vehicles group in the in-game menu with independent controls for buffer capacity (default: 2.0x) and transport speed (default: 2.0x).
  - Fully automatic vehicle identification via component hierarchy.
  - **Throughput Synchronization**: Discharge lump volume (`OneLoadVolume`) and spawn interval (`SpawnInterval`) now scale with belt speed, and the secondary conveyor section (`MyPathAfterDrop`) is dynamically accelerated.
- **Dedicated 3-Axis Excavator Hydraulic Controls**:
  - 3 new individual sliders for excavators (`Koparka`): Boom/Arm speed (2.0x), Turret rotation speed (2.0x), and Bucket tilt speed (1.0x).
  - Automatically adjusts `Rigidbody.maxAngularVelocity` for smooth physical rotation.
- **In-Game Localization Alignment**:
  - All components aligned with official in-game terms (*Dump Truck*, *Wheel Loader*, *Backhoe Loader*, *Wave Table*, *Gold Nuggetator*, *Miner's Moss*, etc.).
  - Streamlined descriptions into concise, clear player-facing summaries.
- **Fixed Vanilla Restoration & Cumulative Drift on Mod Toggles**:
  - Resolved an issue where toggling the mod off and on caused capacities (such as the Dump Truck bed volume) to repeatedly multiply and halve the fill gauge.
  - All 19 patches now retain immutable baseline values in memory and cleanly restore exact vanilla values when the mod is disabled.

---

## [1.5.2] - 2026-09-01

### Performance Optimization (Eliminated FPS Drop)

- **Zero-Allocation Fast-Path**: All 18 Harmony patches check on the very first cycle whether multiplier values are unchanged and exit immediately without allocations (`O(1)` fast exit).
- **Direct Field Access Without Boxing**: Replaced reflection lookups with direct typed member lookups, preventing thousands of heap allocations per second for the Unity garbage collector.
- **Removed Thread Locks**: Update loops run single-threaded; eliminated thread locks (`lock (SyncRoot)`) from helper stores.
- **Wheel Loader Joint Caching**: Caches hydraulic cylinders once upon vehicle spawn rather than querying the hierarchy every frame.

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
