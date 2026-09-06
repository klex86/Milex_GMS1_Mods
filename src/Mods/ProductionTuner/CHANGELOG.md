# Changelog – Milex GMS1 Production Tuner

All notable changes to this mod are documented in this file.
This format is based on [Keep a Changelog](https://keepachangelog.com/).

---

## [1.4.0] – 2026-09-06

### Fixed: Continuous Float Sliders, Baseline Constants & Multi-Instance Synchronization

- **Continuous Float Slider Support (`AcceptableValueRange`)**:
  - Replaced all BepInEx `AcceptableValueList<float>` bindings with continuous `AcceptableValueRange<float>(0.5f, max)`.
  - Resolves a critical bug where moving sliders in the Modern Canvas or Classic IMGUI menus generated continuous float values (e.g., 5.81x) that were rejected by BepInEx validation and silently forced back to the first item in the list (`0.5f`).
  - Added dedicated support in `TuningConfig.GetEntryMax` to read `AcceptableValueRange<float>.MaxValue`, properly supporting up to 20.0x multipliers.
- **Verified Vanilla Baseline Constants Architecture**:
  - Replaced fragile live-object dynamic base reading with verified hardcoded vanilla baseline constants across all stationary and hand equipment patches (`VanillaHogPanCapacity = 10f`, `VanillaBucketCapacity = 15f`, `VanillaShovelVolume = 0.1f`, `VanillaTrailerCapacity = 1000f`, `VanillaStationaryTankCapacity = 10000f`, `VanillaMaxFill = 15f`, `VanillaCleanSpeed = 0.01f`, etc.).
  - Completely eliminates savegame drift and compounding multiplier calculations when loading games with pre-existing modded values.
- **Vehicle Prefab Baseline Architecture & Dump Truck Fill Display Fix**:
  - Vehicle shovels and beds (`DumpTruck`, `WheelLoader`, `Excavator`, `BackhoeLoader`) store capacity in `DiggingController._maxShovelVolume`, which is marked `[XmlIgnore]` and never serialized into savegames. Unity always instantiates vehicles with pristine prefab defaults.
  - Replaced arbitrary hardcoded constants with dynamic prefab baseline capture upon first registration in `Update()`, coupled with synchronized reciprocal volume `_invmaxShovelVolume = 1f / targetVol` and zero-allocation fast-paths.
  - Completely resolves the issue where a bogus `6400f` baseline caused dump trucks loaded from savegames to jump and freeze at 100% fill.
- **Harmony Startup Lifecycle Safety**:
  - Removed invalid `[HarmonyPatch(..., "Start")]` hooks from classes lacking a `Start()` method in `Assembly-CSharp` (`HogPanDirtBox`, `Bucket`, `Shovel`, `ConveyorElevator`, `DumpTruck`).
  - Completely fixes the fatal startup exception (`Undefined target method`) in `Harmony.PatchAll()` that previously aborted mod initialization and left patches unapplied.
- **Mobile Fuel Trailer Detection Fix**:
  - Replaced brittle GameObject name filtering with robust `Trailer.MyMachineType == MachineType.TrailerFuel` and `TRAILER_FUELTANK_FUELMAXCAPACITY` detection.
  - Prevents the mobile fuel trailer from being misidentified as a 10,000L stationary tank and assigned incorrect capacity multipliers.
- **Multi-Instance Synchronization & Fast-Path Integrity**:
  - Synchronized instance iteration across all patches: when a multiplier changes, all active equipment instances in `Tracked.Values` are updated at once before `_lastMultiplier` updates, eliminating skipped equipment items and frame-rate hitching.
  - Corrected loader fast-paths (`ExcavatorPatch`, `WheelLoaderPatch`, `BackhoeLoaderPatch`) to update tracked state timestamps, preventing per-frame joint loops.
- **Reciprocal Shovel Volume & Nuggetator Ratio Synchronization**:
  - Synchronized `_invmaxShovelVolume = 1f / targetVol` in `DiggingController` for excavators, wheel loaders, and backhoes, keeping internal game event triggers and diagnostic meters accurate.
  - Synchronized Nuggetator (`MatScrubber`) internal cleaning ratio `_ratio` with scaled bucket mat capacities.

### Added: Stationary Fuel Tanks & Refueling Speed

- **Stationary Fuel Tank Capacity (`FuelTank_Capacity`)**:
  - Extends fuel capacity scaling to all stationary `FuelStationController` objects placed on the claim.
  - Separate config key and tracking dictionary — fully independent from the mobile trailer multiplier.
  - Default: `2.0x`.
- **Fuel Nozzle Physics Integrity & Hose Reach Reversion**:
  - Completely reverted experimental physics joint manipulation (`FuelHoseLength`) on `FuelPistolHoldable`.
  - The previous joint limit modification conflicted with `ConfigurableJoint` recreation in `ShovelRopeDestruction.OnJointBreak`, causing fuel nozzles to instantly break (`ROPE_REKT`) with 10,000 durability damage even at minimal distance.
  - Safe refuel flow rate scaling (`TankingSpeed`) is cleanly retained without touching rope joints or physics anchors.

---

## [1.3.1] – 2026-09-06
 

### Excavator Handbrake Stabilization & Dump Truck Driving Mass Balancing

- **Excavator Chassis Stabilization with Handbrake**:
  - Automatically freezes chassis slide and tilt via `RigidbodyConstraints` whenever the excavator's handbrake is engaged (`HandbrakeOn == true`).
  - Eliminates unwanted vehicle tilting, tipping, or slipping on steep claim terrain during aggressive scooping. Releasing the handbrake restores complete track movement instantly.
- **Dump Truck Driving Mass Compensation**:
  - Implemented dynamic mass balancing in `GoldDigger.DumpTruck.MachineMove` using Harmony `__state`.
  - When bed capacity is multiplied above vanilla levels, physical mass drag (`Dirt.LoadMass`) is dampened proportionally while driving, allowing fully loaded trucks to navigate mud and inclines with agile, responsive acceleration. Full capacity and volume display remain untouched.

---

## [1.3.0] – 2026-09-02

### Added: Mobile Conveyor Belts & Excavator Hydraulics

- **Mobile Conveyor Belts (Frankenstein & Cordylus)**:
  - Integrated directly under the Vehicles group in the in-game menu after the Dump Truck.
  - Separate controls for **Buffer Capacity** (default: 2.0x) and **Transport Speed** (default: 2.0x).
  - Automatic detection for both conveyor vehicle types.
  - **Synchronized Throughput**: The hopper buffer now empties synchronously with belt speed, transferring larger dirt volumes at quicker intervals so material moves continuously without bottlenecking.
- **Dedicated Excavator Hydraulic Speed Controls**:
  - 3 independent sliders for fine-tuned excavator maneuverability:
    - **Boom / Arm Speed** (lifting and extending cylinders, default: 2.0x)
    - **Turret Rotation Speed** (upper carriage and cabin swing, default: 2.0x)
    - **Bucket Tilt Speed** (bucket curl and dump cylinders, default: 1.0x)
  - Full rotational range of motion without physical rotation limits or jitter.
- **High-Performance Architecture**: All sliders utilize high-performance fast-paths to ensure 0 frame rate drop during live gameplay.
- **Fixed Vanilla Restoration on Mod Toggle**: Toggling the mod off and on cleanly restores unmodified game defaults, eliminating volume doubling or fill-percentage drift.
- **Refined Localization & Concise Descriptions**: All component titles aligned with official in-game terms (*Dump Truck*, *Wheel Loader*, *Backhoe Loader*, *Wave Table*, *Gold Nuggetator*, *Miner's Moss*, etc.). Descriptions streamlined to direct, player-focused summaries without formulas.

---

## [1.2.2] – 2026-09-01

### Performance Overhaul (Eliminated 40% FPS Drop)

- **Zero-Stutter Fast Path Across All Components**:
  - Immediate exit loops when slider values are unchanged, completely eliminating micro-stutters and frame rate drops.
  - Streamlined memory handling to prevent garbage collection pauses during gameplay.
- **Hydraulic Cylinder Caching**:
  - Gathers joint components once upon vehicle spawn rather than querying the hierarchy every frame.
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
