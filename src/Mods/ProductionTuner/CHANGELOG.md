# Changelog – Milex GMS1 Production Tuner

All notable changes to this mod are documented in this file.
This format is based on [Keep a Changelog](https://keepachangelog.com/).

## [1.4.10] – 2026-09-12

### Fixed: Fuel Station Save/Load Clamping Protection & Mobile Trailer Baseline Calibration

- **Save/Load Fuel Clamping Protection (`FuelTrailerPatch`)**:
  - Fixed a critical bug where reloading a saved game with a refueled fuel trailer or stationary tank truncated its fuel volume down to the raw prefab capacity.
  - *Root cause*: `FuelStationController.Serialize()` stores fuel as an absolute liter value (`CurrentCapacity`). When loading a save, Unity instantiates the trailer prefab with its raw vanilla capacity ($1,000\text{L}$). In the very first frame of native `FuelStationController.Update()`, the game executes `CurrentCapacity = Mathf.Clamp(CurrentCapacity, 0f, MaxCapacity)`. Because the patch was previously a `[HarmonyPostfix]`, line 145 executed *before* the patch could update `MaxCapacity`, immediately clamping any fuel above $1,000\text{L}$ down to $1,000\text{L}$ (e.g. 77% of $7,500\text{L} = 5,775\text{L}$ clamped to $1,000\text{L} = 13.3\%$).
  - *Fix*:
    - Switched `FuelStationUpdatePatch` to `[HarmonyPrefix]` so `MaxCapacity` is guaranteed to be scaled before `Mathf.Clamp` is evaluated.
    - Added `FuelStationDeserializePatch` (`[HarmonyPostfix]` on `FuelStationController.Deserialize`) to immediately restore scaled capacity the instant save data is read.
- **Mobile Trailer Authentic Prefab Baseline Calibration (`VanillaTrailerCapacity = 1000f`)**:
  - Calibrated mobile fuel trailer baseline capacity strictly to authentic Unity prefab runtime dump value ($1,000\text{L}$, `TRAILER_FUELTANK_FUELMAXCAPACITY`). Runtime object with $2,500\text{L}$ was confirmed as the commercial town gas station dispenser (`IsInfinitySource = true`), not the trailer. Default 3.0x multiplier yields $3,000\text{L}$.

---

## [1.4.9] – 2026-09-11

### Fixed: Fuel Infrastructure Isolation & 6.6 HogPan Default Ratio

- **Fuel Infrastructure Isolation & Portable Equipment Protection (`FuelTrailerPatch`)**:
  - Fixed a critical issue where `FuelStationController` instances on portable equipment (portable generator 4L, small water pump 6L, big water pump 216L, Jerry can 20L, light trailer 50L, conveyor belt engines 300L) were improperly matched by the unconstrained fallback in `FuelTrailerPatch` and scaled to stationary fuel tank capacity ($10,000\text{L} \times \text{multiplier} = 30,000\text{L}$).
  - This caused portable generators to hold thousands of gallons at the town gas station and charge exorbitant prices in the hundreds of thousands of dollars to refuel.
  - Introduced strict instance filtering:
    - **`IsMobileTrailer`**: Only matches mobile fuel trailers (`MachineType.TrailerFuel`, `TRAILER_FUELTANK_FUELMAXCAPACITY`). Corrected baseline capacity to verified runtime value of `VanillaTrailerCapacity = 2500f`.
    - **`IsStationaryClaimTank`**: Only matches the large claim fuel tank (`FUELTANK_STATIONARY_FUELMAXCAPACITY`, $10,000\text{L}$).
    - **All other machines & appliances**: Remain completely untouched at their authentic vanilla capacities.
- **HogPan & Fine Processing Default Ratio Adjustments (`TuningConfig`, `Milex_GMS1_ProductionTuner.default.cfg`)**:
  - Updated default `HogPan_Capacity` to `6.6f` (using standard invariant dot notation) so that a modified 2.0x bucket ($0.06\text{ m}^3$) fills exactly 10% of the HogPan dirt box ($0.60\text{ m}^3$).
  - Updated default `MagnetiteSeparator_Capacity` to `4.9f` and `WaveTable_Capacity` to `4.9f` for harmonious fine processing pacing with enlarged buckets.
- **Clean Configuration Range Formatting (`Milex_GMS1_ProductionTuner.default.cfg`)**:
  - Replaced legacy verbose comma-separated lists (`# Acceptable values: 0,5, 1, 1,5, ...`) with clean BepInEx range directives (`# Acceptable value range: From 0.5 to 10` and `From 0.5 to 20`), reflecting continuous float slider support.

---

## [1.4.8] – 2026-09-11

### Fixed: Full Alignment with Authentic Unity Prefab Dump Baselines (m³)

- **Universal Unity Prefab Baseline Calibration**:
  - Replaced all legacy C# uninitialized placeholder values with authentic numbers extracted from the live Unity runtime memory dump:
    - **Hand Tools**: Shovel ($0.01\text{ m}^3$), Bucket ($0.03\text{ m}^3$), GoldPan ($0.01\text{ m}^3$), HogPan dirt box ($0.09\text{ m}^3$).
    - **Processing Equipment**: WaveTable ($0.06\text{ m}^3$), Magnetite Separator ($0.12\text{ m}^3$, $0.002\text{ speed}$), Magnetite Trailer ($0.30\text{ m}^3$).
    - **Conveyors**: ConveyorGround ($80.0\text{ m}^3$, $0.2\text{ speed}$), ConveyorElevator ($1.25\text{ m}^3$, $3.0\text{ speed}$).
    - **Wash Plants**: MobileWashplant ($10.0\text{ m}^3$, $0.225\text{ speed}$), MiniWashplant ($3.0\text{ m}^3$, $0.05\text{ speed}$), WashPlantShaker ($40.0\text{ m}^3$, $0.55\text{ speed}$).
    - **Miner's Moss**: HogPan mats ($0.2488\text{ m}^3$), Stationary & Orange Beast mats ($0.90\text{ m}^3$).
- **Multi-Ratio Cascade Protection**:
  - Rewrote cascade limits and automatic scaling in `TuningConfig` to strictly follow genuine equipment bucket capacities: HogPan ($3:1$), WaveTable ($2:1$), Magnetite Separator ($4:1$), and Magnetite Trailer ($10:1$).

---

## [1.4.7] – 2026-09-11

### Fixed: Authentic Vanilla 3-Bucket HogPan Capacity Baseline (45.0f)

- **HogPan Hopper Baseline Harmonization (`VanillaHogPanCapacity = 45.0f`)**:
  - Corrected `VanillaHogPanCapacity` from the uncalibrated class field fallback ($10.0\text{f}$) to the authentic gameplay vanilla baseline ($45.0\text{f}$), reflecting the verified vanilla game reality that exactly 3 standard buckets ($15.0\text{f}$ each) fill 1 HogPan hopper ($45.0\text{f}$).
  - A 2.0x modified bucket ($30.0\text{f}$) poured into a 1.0x vanilla HogPan ($45.0\text{f}$) now fills exactly $66.67\%$ (two thirds).
  - With both bucket and HogPan scaled by the same multiplier (e.g., both 2.0x), 1 bucket fills exactly $33.33\%$, preserving authentic vanilla pacing while doubling throughput.
- **Config Cascade Threshold Correction (`TuningConfig`)**:
  - Adjusted `AutoScaleDependentInputs` and `EnforceMinimum` for HogPan to respect the $3:1$ vanilla volume ratio ($45.0\text{f} / 15.0\text{f}$), preventing HogPan's multiplier from being falsely forced to match bucket's multiplier when bucket capacity is already smaller than the HogPan.

---

## [1.4.6] – 2026-09-11

### Fixed: HogPan Infinite Water Loop & GoldPan Capacity Synchronization

- **HogPan Clean Zeroing Water Drain (`ProcessPlaneWaterGuardPatch`)**:
  - Fixed a mathematical clamp loop where `WaterVolume` near zero was restored by excess drain compensation every frame, preventing it from ever reaching 0 and causing infinite water flow.
  - The patch now directly applies the authentic vanilla water drain rate from the captured pre-drain volume (`WaterVolume = Mathf.Max(0f, __state - (Time.deltaTime * (VanillaHogPanCapacity / 7.5f)))`), ensuring water drains at authentic vanilla speed and cleanly shuts off at 0.
- **GoldPan Capacity Synchronization & Recount (`BucketFillOutCorutinePatch`)**:
  - Dynamically scales `GoldPan.PanMaxFill` to match enlarged `Bucket.MaxVolume`, preventing premature `PLACE_TO_FILL_IS_FULL` aborts.
  - Automatically invokes `GoldPan.UpdateFillCount()` via reflection prior to capacity checks so that pans washed clean in the river are immediately recognized as empty.

---

## [1.4.5] – 2026-09-11

### Fixed: Shovel-to-Bucket Capacity Harmonization & 1-Stroke Fill Ratio

- **Container Scale Harmonization (`VanillaShovelVolume = 5.0f`)**:
  - Identified the root cause of the broken shovel-to-bucket filling ratio: vanilla `Shovel.MaxVolume = 0.1f` was calibrated in Unity voxel mesh cubic meters ($0.1\text{ m}^3 = 100\text{ liters}$), while `Bucket.MaxVolume = 15.0f` and `HogPanDirtBox.PlaneVolumeMax = 10.0f` were calibrated in liters/container units without any conversion factor on transfer. This caused vanilla to require 150 shovels per bucket, and 2.0x bucket / 6.0x shovel to require 50 shovels.
  - Harmonized baseline shovel capacity to `VanillaShovelVolume = 5.0f` (3 baseline shovels = 1 standard 15.0f bucket, aligning with player-expected vanilla balance).
  - With default multipliers (`6.0x` Shovel = 30.0f yield, `2.0x` Bucket = 30.0f capacity), exactly **1 shovel** fills a 2.0x bucket to 100%, and fills a standard 1.0x bucket to full.
- **Proportional 1-Stroke Digging Fill (`ShovelFixedUpdatePatch`)**:
  - Added `ShovelFixedUpdatePatch` to monitor terrain voxel excavation during `TryTakeGround`.
  - Smoothly scales the harvested dirt volume and claim mineral densities (gold, magnetite, diamonds) across the digging animation so that a single shovel stroke fills `CurrentVolume` to 100% of `MaxVolume` without requiring dozens of repeated digs in the same hole.

---

## [1.4.4] – 2026-09-11

### Fixed: Bucket Dump Reflection Delegate Signature

- **`Bucket.Wylej` Delegate Signature Fix**:
  - Corrected the reflection delegate for the private `Bucket.Wylej` method from `Action<Bucket, float>` to parameterless `Action<Bucket>`, matching the vanilla `Assembly-CSharp` signature `private void Wylej()`.
  - Prevents an `ArgumentException` / `TypeInitializationException` from occurring during static initialization of `BucketFillOutCorutinePatch`.

---

## [1.4.3] – 2026-09-11

### Fixed: Pickup Bed Physics Integrity & Robust Shovel Tuning

- **Pickup Bed Physics Integrity**:
  - Removed faulty `PickupSafetyPatch.cs`. The vanilla game already manages item freezing safely via `FreezeObjectsCorutine()` and `WaitForFixedUpdate()`.
  - Calling `FreezeNow()` prematurely in a postfix cleared `ObjectsOnBedD` without freezing objects, leaving loose tools as active colliders that caused physics explosions. Removing the patch completely restored stable truck physics.
- **Robust Shovel Blade & Dig Depth Scaling**:
  - Dynamically computes blade dimensions directly from `shovel.BladesBoxCollider.size` and `shovel.DigScale`, preserving genuine coordinates and signs across all game versions without hardcoded constant drift.
  - Clamped `DigDepth` to a stable maximum (0.03m), ensuring single-stroke bucket filling while preventing severe terrain gouging around vehicles.

---

## [1.4.2] – 2026-09-11

### Fixed: Config Defaults, HogPan Infinite Water, Shovel Digging Speed & Bucket-to-GoldPan Transfer

- **Group Reset Defaults Alignment**:
  - Aligned all `BindStep` parameter defaults in `TuningConfig.cs` with `Milex_GMS1_ProductionTuner.default.cfg`.
  - Clicking the in-game "Reset Group" button in Modern Canvas or Classic IMGUI menus now cleanly restores the curated default values (e.g. 6.0x shovel, 3.0x capacities, 5.0x fuel hose) instead of falling back to 1.0x.
- **Manual HogPan Infinite Water Flow Bug Fix**:
  - Fixed an issue where the water guard patch added excess drain refund every frame even when the hog pan had zero water, causing dry hog pans to wash endlessly with infinite water and looping audio/particles.
  - `ProcessPlaneWaterGuardPatch` now captures initial `WaterVolume` in a `[HarmonyPrefix]` and only refunds water in `[HarmonyPostfix]` if water was genuinely present before drain (`__state > 0.0001f`), clamped to never exceed pre-drain volume.
- **Shovel Digging Volume & Speed Fix**:
  - Fixed `_bladeSizez` negative sign inversion (`-VanillaBladeSize * bladeScale`), matching vanilla `Shovel.Awake` (`BladesBoxCollider.size.z * DigScaleZ` where `DigScaleZ = -1f`).
  - Dynamically scales voxel cut depth (`DigDepth = VanillaDigDepth * multiplier`) so each dig scoop extracts dirt volume proportionally, allowing the enlarged shovel to fill in a single scoop instead of requiring dozens of scoops to fill a bucket.
- **Bucket to GoldPan Transfer & Gold Loss Protection**:
  - Fixed a critical vanilla design flaw where dumping an enlarged bucket into a gold pan unconditionally deducted the bucket's contents even if the pan had less free space or was already full, permanently destroying material and gold.
  - Added `BucketFillOutCorutinePatch` to intercept bucket dumps into `GoldPan`.
  - Queries available pan space (`PanMaxFill - _GroundVolume`) and strictly caps the transferred volume to the pan's capacity.
  - If the pan is full, displays the native `PLACE_TO_FILL_IS_FULL` notification and retains 100% of material and gold in the bucket.

---

## [1.4.1] – 2026-09-06

### Added & Fixed: Robust Fuel Hose Reach Scaling (`FuelHose_Length`)

- **Root Cause Resolution for Breaking Fuel Nozzle**:
  - Identified and fixed the root cause of the broken fuel nozzle bug (`ROPE_REKT` and 10,000 durability damage to `CheckAndRepair`).
  - Previously, joint limits were applied only on `FuelPistolHoldable.Attach` and targeted the temporary vehicle lock joint instead of the actual physical rope joint (`MyCJoint` in `ShovelRopeDestruction`) on the fuel trailer/tank.
  - Additionally, `FuelPistolHoldable.MyConfigurableJ` was unassigned in vanilla game code, causing `Attach` to spawn a weak 1,000 N joint that immediately tore itself apart against the rope tension.
- **Dedicated `ShovelRopeDestruction` Physical Joint Scaling**:
  - Patched `ShovelRopeDestruction.Awake` and `CreateJoint` to directly scale `MyCJoint.linearLimit` by the configured multiplier.
  - Automatically updates the internal `jlimit` field on `ShovelRopeDestruction` to ensure any recreated joint retains the extended reach.
  - Explicitly assigns `FuelPistolHoldable.MyConfigurableJ = MyCJoint`, allowing `Attach()` to validate reach against the extended distance and dock with full 5,000,000 N locking force.
- **Break Force & Impulse Buffering**:
  - Buffered `cjoint.breakForce` and `_breakForce` (and `breakTorque`) to a minimum of 50,000 N. Completely prevents false joint snaps and nozzle destruction when running, jumping, or moving across bumpy terrain while carrying the nozzle.
  - Preserves legitimate drive-away penalties if a heavy vehicle drives off while the nozzle is still plugged in.
- **Continuous In-Game Slider (`[Group5_Trailers] FuelHose_Length`)**:
  - Added `FuelHose_Length` with range 1.0x to 5.0x (default 2.0x, reaching 10–12 meters).
  - Fully integrated with real-time menu updates, reset buttons, and English/German localizations.
- **Stationary Fuel Tanks & Generators Clarification (`FuelTank_Capacity`)**:
  - Clarified UI descriptions and labels: `FuelTank_Capacity` deliberately scales both stationary claim depot fuel tanks and stationary claim infrastructure (large diesel generators, pump stations) sharing the internal `FuelStationController` component, providing consistent long-running fuel supplies across the entire claim.

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
