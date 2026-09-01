# Changelog – Milex GMS1 Production Tuner

All notable changes to this mod are documented in this file.
This format is based on [Keep a Changelog](https://keepachangelog.com/).

---

## [1.3.0] – 2026-09-02

### Added: Mobile Conveyor Belts & Excavator Hydraulics

- **Mobile Conveyor Belts (Frankenstein & Cordylus)**:
  - Integrated directly under the Vehicles group in the in-game menu after the Dump Truck.
  - Separate controls for **Buffer Capacity** (`MaxVolume`, default: 2.0x) and **Transport Speed** (`Speed`, default: 2.0x).
  - Automatic machine type identification via parent hierarchy (`FrankensteinExcavator` vs. `MaximusMachineController`).
  - **Throughput Correction**: Synchronizes discharge chunk volume (`OneLoadVolume`), shortens spawn timer interval (`SpawnInterval`), and dynamically accelerates the secondary drop conveyor section (`MyPathAfterDrop`). The hopper buffer now empties rapidly and synchronously with belt speed.
- **Dedicated Excavator Hydraulic Speed Controls**:
  - 3 new individual sliders for fine-tuned excavator maneuverability (`Koparka`):
    - **Boom / Arm Speed** (lifting and extending cylinders, default: 2.0x)
    - **Turret Rotation Speed** (upper carriage and cabin swing, default: 2.0x)
    - **Bucket Tilt Speed** (bucket curl and dump cylinders, default: 1.0x)
  - Automatically adjusts `Rigidbody.maxAngularVelocity` so physics does not artificially clamp higher rotational speeds.
- **High-Performance Fast-Path**: All new sliders feature the zero-allocation architecture with instant `O(1)` fast exit whenever multipliers remain unchanged.
- **Refined Localization & Concise Descriptions**: All component titles aligned with official in-game terms (*Dump Truck*, *Wheel Loader*, *Backhoe Loader*, *Wave Table*, *Gold Nuggetator*, *Miner's Moss*, etc.). Descriptions streamlined to direct, player-focused summaries without formulas.

---

## [1.2.2] – 2026-09-01

### Performance Overhaul (Eliminated 40% FPS Drop)

- **Zero-Allocation Fast-Path Across All 18 Patches**:
  - Harmony patches check on the very first cycle whether the multiplier is unchanged and exit immediately without allocations (`O(1)` fast exit).
  - Eliminates thousands of boxing heap allocations (`FieldInfo.GetValue` for floats) per second that previously overloaded the Unity garbage collector.
- **Direct Public Field Access & Lock Removal**:
  - Replaced reflection field accesses with direct typed member lookups.
  - Removed all thread locks (`lock (SyncRoot)`) from helper stores because Unity update loops are strictly single-threaded.
- **Wheel Loader Hydraulic Caching**:
  - Gathers `AnimatedJoint` components once upon vehicle spawn rather than calling `GetComponentsInChildren` every frame in `Update()`.
- **Result**: Rock-solid 60 / 144 FPS with zero stutter.

---

## [1.2.1] – 2026-09-01

### Bug Fixes & In-Game Refinements

- **Excavator Precision Digging**: Removed enlarged collision box collider sizing. The excavator digs with millimeter precision at the blade edge while still holding the enlarged bucket payload volume.
- **Hand Shovel Loop**: Switched shovel patching to `Update()` loop with $\sqrt{M}$ blade scaling so existing shovels in inventory and live slider changes update instantly.
- **Dump Truck vs. Wheel Loader Separation**: Cleanly separated `DumpTruck` from `Ladowarka` by checking instance type names and synchronizing reciprocal volume `_invmaxShovelVolume`.
- **Fuel Trailer Live Refresh**: Switched fuel trailer tracking to `Update()` loop so already purchased trailers reflect multiplier changes immediately.
- **UI Slider Procedural Texture Protection**: Protected generated textures with `HideFlags.HideAndDontSave` and automatic style recreation to prevent disappearing orange slider tracks.

---

## [1.2.0] – 2026-09-01

### Phase 2: Complete Game Integration (Harmony Patches)

- **All 22 Initial Components Fully Implemented**:
  - **Tools**: Hand Shovel (`GoldDigger.Shovel`) with $\sqrt{M}$ blade edge scaling, Bucket (`GoldDigger.Bucket`).
  - **Wash Plants**: Hog Pan (`GoldDigger.HogPanDirtBox`), Mobile & Mini Wash Plant (`GoldDigger.MobileWashplant`, `MiniWashplant`), Trommel/Shaker (`GoldDigger.WashplantShakerBase`), Sluice Boxes (`GoldDigger.WashPlantSluiceBoxDirt`), Miner's Moss (`GoldDigger.MinersMoss`).
  - **Vehicles**: All Excavators (`Koparka`), Wheel Loader (`Ladowarka`) with reciprocal volume preservation and hydraulic torque scaling, Backhoe Loader (`KoparkoLadowarka`), Dump Truck (`GoldDigger.DumpTruck`).
  - **Fine Processing**: Gold Nuggetator (`GoldDigger.MatScrubber`), Magnetite Separator (`GoldDigger.MagnetiteSeparator`), Wave Table (`GoldDigger.WaveTable`) with timer-based cycle acceleration.
  - **Logistics & Trailers**: Feeder Hopper (`GoldDigger.ConveyorGround`), Elevator Conveyor Bucket (`GoldDigger.ConveyorElevator`), Magnetite Trailer (`GoldDigger.MagnetiteTrailer`), Fuel Trailer (`GoldDigger.FuelStationController`) with proportional refueling flow rate.
- **Resource Neutrality & Infrastructure Protection**:
  - **Hog Pan Water Protection**: Water drainage rate clamped to vanilla base value in `ProcessPlane`. Mats do not dry out prematurely even with 10x dirt capacity.
  - **Pump & Generator Stability**: Electric wattage and water intake demands remain unmodified to prevent power outages or pressure drops.
  - **Hydraulic Torque**: Automatically increases cylinder lifting torque (`AnimatedJoint.MaxTorque`) for heavier wheel loader bucket payloads.
- **OriginalValueStore & Drift Prevention**:
  - Captures original vanilla values prior to the first multiplication.
  - Accurately restores vanilla values when disabling the mod or resetting sliders.
- **Community Credits & Open Source License**:
  - Prominent credits to community mod authors (stregkoden, DeepCore/Jonathan, FedeRama, GMS Community) added to documentation.
  - Fully open MIT-style license granted for public use.

---

## [1.1.0] – 2026-08-30

### Enhancements & Architecture Rework

- **Specific Default Multipliers Per Component**: Each parameter starts with an optimal default multiplier (e.g. Excavators 3.0x, Dump Truck 3.0x, Shovel 2.0x, Wash Plants 2.0x) instead of a flat 1.0x.
- **Component Cleanup**:
  - Group 1 (Hand Tools): Removed `Pan_Capacity`. Added `MobileWashPlant_Capacity` (2.0x).
  - Group 2 (Vehicles): Unified all excavators under `Excavator_DigSpeed`. Added `DumpTruck_Capacity` (3.0x).
  - Group 3 (Wash Plant Modules): Renamed `Conveyor_Speed` to `ConveyorBucket_Capacity` (2.0x). Replaced redundant switches with `Washplant_Capacity` (2.0x), `Washplant_Speed` (2.0x), and `Sluicebox_Capacity` (2.0x).
  - Group 4 (Fine Processing): Added `MagnetiteSeparator_Capacity` (2.0x).
- **Refined Cascade Protection**: Automatically syncs dependent downstream containers (`HogPan_Capacity`, `MagnetiteSeparator_Capacity`, `WaveTable_Capacity`, `MagnetiteTrailer_Capacity`) to the bucket capacity.
- **Direct Slider Architecture**: Removed group multipliers and simple/advanced modes in favor of clean, direct individual sliders for all parameters.
- **Extended Range for Downstream Containers**: Downstream containers support multipliers up to 20.0x to handle multiple bucket dumps smoothly.

---

## [1.0.1] – 2026-08-30

### Bug Fixes & Improvements

- **Menu Display Names**: Fixed configuration key naming so translated names display properly.
- **0.5 Step Snapping**: All multiplier sliders snap cleanly to 0.5 increments.
- **Separated Trailer Settings**: Magnetite and fuel trailers now configure independently.
- **Live Cascade Protection**: Dynamic UI clamping prevents setting the bucket larger than downstream equipment capacity.

---

## [1.0.0] – 2026-08-30

### Initial Release

- Foundation architecture for Production Tuner with 5 multiplier groups.
- Embedded English and German localization files with automatic template generation.
- Full in-game menu integration via CoreMod.
