# Changelog - Milex GMS1 Mods

All notable changes and releases for this mod collection are documented in this file.
This format is based on [Keep a Changelog](https://keepachangelog.com/).

## [1.8.18] - 2026-09-12

### Production Tuner: Fast Travel Wheel Landing Shock Protection & Trailer Auto-Reconnection

- **Production Tuner v1.4.11 — Fast Travel Wheel Landing Shock Protection (`WheelLandingProtectionPatch`)**:
  - Eliminated the notorious vanilla bug where trailer or vehicle wheels are severely damaged or broken upon fast travel arrival.
  - Teleporting vehicles causes instantaneous drops and joint snap that spike `suspensionCompression > 1.3f`, which the game erroneously treats as hitting severe potholes (`DrivingThroughHoles`), subtracting durability per frame until destroyed.
  - Implemented a 4-second settling grace window (`FastTravelProtectionUntil`) in `CheckAndRepair.UpdateDurability` that suppresses false landing shock damage while preserving authentic road pothole wear during normal driving.
- **Production Tuner v1.4.11 — Fast Travel Trailer Auto-Reconnection & Manual Unhitch Fix (`TrailerFastTravelPatch`)**:
  - Solved the persistent vanilla annoyance where fast traveling in a pickup forcibly disconnects any hitched trailer (`MapMenu.FastTravel` calls `DisconnectInstantly()` and leaves the trailer 4 meters behind the vehicle).
  - Automatically captures the connected trailer before fast travel and re-hitches it to the pickup hitch after destination streaming and physics settle.
  - Restores the hitch lever GameObject (`OnCanConnect.SetActive(true)`) and colliders, which vanilla `Pickup.Teleport` had disabled, allowing the player to walk up to the hitch and manually uncouple the trailer at any time. Added `TrailerConnectToPickupGuard` to ensure `PickupCanConnect` remains safely resolved.

---

## [1.8.17] - 2026-09-12

### Production Tuner: Fuel Station Save/Load Clamping Protection & Prefab Baseline Calibration

- **Production Tuner v1.4.10 — Fuel Station Save/Load Clamping Immunity (`FuelTrailerPatch`)**:
  - Solved fuel truncation on save/load where refueled trailers dropped from 77% to 13%.
  - Fuel volume is serialized as an absolute float in liters (`CurrentCapacity`). In vanilla Unity, newly spawned trailer prefabs initialize with `MaxCapacity = 1000L`. Native `FuelStationController.Update()` evaluates `CurrentCapacity = Mathf.Clamp(CurrentCapacity, 0f, MaxCapacity)` on frame 1.
  - Converted `FuelStationUpdatePatch` to `[HarmonyPrefix]` so that `MaxCapacity` is scaled before native clamp evaluation, and added a `[HarmonyPostfix]` on `FuelStationController.Deserialize` to guarantee instant capacity scaling during load deserialization.
- **CoreMod v1.8.17 — Fast Travel & Streaming Engine Immunity (`CorePlugin`)**:
  - Fixed an issue where the leaked pause auto-recovery triggered prematurely after 4.0 seconds during Fast Travel (`MapMenu.ForceLoad`), which intentionally holds the engine paused for 6.5–7.5 seconds while terrain mesh colliders initialize.
  - Increased recovery grace period to 15.0 seconds and added active checks for `MenuLoading.gameObject.activeInHierarchy` / `isFastTravel`, preventing trailers from falling through the uncollided terrain and respawning at neutral points.

---

## [1.8.16] - 2026-09-11

### Production Tuner: Fuel Infrastructure Isolation & 6.6 HogPan Default Ratio

- **Production Tuner v1.4.9 — Fuel Infrastructure Isolation (`FuelTrailerPatch`)**:
  - Resolved a severe issue where `FuelStationController` instances across all portable machinery (portable generator 4L, water pump 6L, Jerry can 20L, light trailer 50L, conveyor engines 300L) were incorrectly classified as stationary claim fuel tanks, setting their `MaxCapacity` to $10,000\text{L} \times \text{multiplier}$ (up to $30,000\text{L}$).
  - This caused the town gas station GUI to display exorbitant capacities (~7,925 gallons) for small generators, asking hundreds of thousands of dollars to refuel.
  - Implemented strict equipment targeting:
    - `IsMobileTrailer` scales mobile fuel trailers with verified runtime baseline `VanillaTrailerCapacity = 2500f`.
    - `IsStationaryClaimTank` scales only the large claim fuel tank (`FUELTANK_STATIONARY_FUELMAXCAPACITY`, $10,000\text{L}$).
    - Portable generators, pumps, Jerry cans, and vehicle tanks are completely excluded and preserve authentic vanilla values.
- **Production Tuner v1.4.9 — HogPan & Fine Processing Default Ratios (`TuningConfig`, default cfg)**:
  - Updated default `HogPan_Capacity` to `6.6f` with dot notation so that 1 modified 2.0x bucket ($0.06\text{ m}^3$) fills exactly 10% of the HogPan ($0.60\text{ m}^3$).
  - Updated default `MagnetiteSeparator_Capacity` to `4.9f` and `WaveTable_Capacity` to `4.9f`.
- **Production Tuner v1.4.9 — Clean Configuration Range Formatting (`Milex_GMS1_ProductionTuner.default.cfg`)**:
  - Replaced verbose comma lists with clean `# Acceptable value range: From 0.5 to 10` and `From 0.5 to 20` directives across all settings.

---

## [1.8.15] - 2026-09-11

### Production Tuner: Full Alignment with Authentic Unity Prefab Baselines (m³)

- **Production Tuner v1.4.8 — Universal Unity Prefab Baseline Calibration**:
  - Replaced legacy C# uninitialized placeholders across all stationary and hand equipment patches with authentic values from the live Unity runtime memory dump:
    - **Hand Tools**: Shovel ($0.01\text{ m}^3$), Bucket ($0.03\text{ m}^3$), GoldPan ($0.01\text{ m}^3$), HogPan hopper ($0.09\text{ m}^3$).
    - **Processing Equipment**: WaveTable ($0.06\text{ m}^3$), Magnetite Separator ($0.12\text{ m}^3$), Magnetite Trailer ($0.30\text{ m}^3$).
    - **Conveyors**: ConveyorGround ($80.0\text{ m}^3$, $0.2\text{ speed}$), ConveyorElevator ($1.25\text{ m}^3$, $3.0\text{ speed}$).
    - **Wash Plants**: MobileWashplant ($10.0\text{ m}^3$, $0.225\text{ speed}$), MiniWashplant ($3.0\text{ m}^3$, $0.05\text{ speed}$), WashPlantShaker ($40.0\text{ m}^3$, $0.55\text{ speed}$).
    - **Miner's Moss**: HogPan mats ($0.2488\text{ m}^3$), Stationary & Orange Beast mats ($0.90\text{ m}^3$).
- **Production Tuner v1.4.8 — Multi-Ratio Cascade Protection**:
  - Rewrote cascade limits and automatic scaling in `TuningConfig` to strictly follow genuine equipment bucket capacities: HogPan ($3:1$), WaveTable ($2:1$), Magnetite Separator ($4:1$), and Magnetite Trailer ($10:1$).

---

## [1.8.14] - 2026-09-11

### Claim Monitor: Equipment Baseline Dumper (JSON)

- **Claim Monitor v1.0.10 — Equipment Baseline Dumper (`EquipmentBaselineDumper`)**:
  - Added a dedicated button **"Dump Baseline (JSON)"** in the Diagnostic Inspector window (`F3`), positioned alongside "Dump All to File".
  - Queries Unity's live memory (`Resources.FindObjectsOfTypeAll`) to inspect both scene instances and loaded prefabs across all tool, vehicle, washplant, conveyor, and infrastructure classes.
  - Automatically captures all `[BalanceSheet]` attributes, capacity metrics, speeds, flow rates, volumes, and physical joint limits.
  - Generates structured JSON files (`Equipment_Baseline_YYYYMMdd_HHmmss.json` and `Equipment_Baseline_Latest.json`) saved directly in `BepInEx/plugins/Milex_ClaimMonitor_Dumps/`.

---

## [1.8.13] - 2026-09-11

### Production Tuner: Authentic Vanilla 3-Bucket HogPan Capacity Baseline (45.0f)

- **Production Tuner v1.4.7 — HogPan Hopper Baseline Harmonization (`VanillaHogPanCapacity = 45.0f`)**:
  - Harmonized `VanillaHogPanCapacity` from the raw C# field default ($10.0\text{f}$) to the authentic gameplay vanilla baseline ($45.0\text{f}$), perfectly matching the verified vanilla ratio where 3 standard buckets ($15.0\text{f}$) fill 1 HogPan hopper ($45.0\text{f}$).
  - A 2.0x modified bucket ($30.0\text{f}$) poured into a 1.0x vanilla HogPan ($45.0\text{f}$) now fills exactly $66.67\%$ (two thirds).
  - Preserves authentic vanilla pacing when scaling both bucket and HogPan equally (e.g., both 2.0x = 3 buckets to fill).
- **Production Tuner v1.4.7 — Cascade Threshold Adjustment (`TuningConfig`)**:
  - Adjusted cascade clamping so HogPan accounts for its genuine $3:1$ vanilla size advantage over the bucket, allowing independent down-tuning without false clamp locks.

---

## [1.8.12] - 2026-09-11

### Production Tuner: HogPan Infinite Water Loop Fix & GoldPan Capacity Sync

- **Production Tuner v1.4.6 — HogPan Clean Zeroing Water Drain (`ProcessPlaneWaterGuardPatch`)**:
  - Eliminated the mathematical clamp loop that prevented water volume in the HogPan from reaching zero, causing infinite water flow.
  - Directly calculates authentic vanilla water drain rate from the pre-drain snapshot (`WaterVolume = Mathf.Max(0f, __state - (Time.deltaTime * (VanillaHogPanCapacity / 7.5f)))`), ensuring water reliably runs out at authentic vanilla speed.
- **Production Tuner v1.4.6 — GoldPan Capacity Synchronization & Recount (`BucketFillOutCorutinePatch`)**:
  - Dynamically synchronizes `GoldPan.PanMaxFill` with `Bucket.MaxVolume` so that the gold pan scales proportionally with enlarged buckets.
  - Automatically invokes `GoldPan.UpdateFillCount()` via reflection prior to capacity checks, recognizing washed pans as empty immediately.

---

## [1.8.11] - 2026-09-11

### Production Tuner: Shovel-to-Bucket Proportion Alignment & 1-Stroke Fill Ratio

- **Production Tuner v1.4.5 — Container Scale Harmonization (`VanillaShovelVolume = 5.0f`)**:
  - Solved the severe ratio discrepancy between shovel and bucket: in vanilla, `Shovel.MaxVolume` was defined in Unity voxel cubic meters ($0.1\text{ m}^3 = 100\text{ l}$), but transferred directly into `Bucket.MaxVolume = 15.0f` (liters) without unit conversion, leading to 150 shovels per bucket in vanilla, and 50 shovels with 2.0x bucket / 6.0x shovel.
  - Aligned baseline shovel capacity to `VanillaShovelVolume = 5.0f` so 3 baseline shovels fill 1 standard 15.0f bucket (aligning with player-expected vanilla balance).
  - With default multipliers (`6.0x` Shovel = 30.0f, `2.0x` Bucket = 30.0f), exactly **1 shovel** fills a 2.0x bucket to 100%, and fills a 1.0x bucket to full.
- **Proportional 1-Stroke Digging Fill (`ShovelFixedUpdatePatch`)**:
  - Smoothly scales the harvested dirt volume and claim mineral densities (gold, magnetite, diamonds) across the digging animation so that a single shovel stroke fills `CurrentVolume` to 100% of `MaxVolume`.

---

## [1.8.10] - 2026-09-11

### CoreMod & Production Tuner: Leaked Pause Auto-Recovery & In-Game Emergency Unpause

- **CoreMod v1.3.5 — Leaked Pause Diagnostics & Auto-Recovery (`ForceResumeGame`)**:
  - Added real-time diagnostics monitoring `PauseManager.Instance.PauseReasons` and `InputManager.AllInputBlocked` every 3 seconds during pause states outside the main menu.
  - Implemented automatic pause recovery: 4.0 seconds after scene loading completes (giving `AreaStreamerBase` ample time to finish its 30-frame collision generation), if level loading is done and no in-game menu is open, orphaned transient pause tags are safely cleared, restoring `Time.timeScale = 1.0f`, unlocking player movement (`InputManager.SetAllInputBlocked(false)`), and refreshing the cursor.
  - Added a dedicated "Resume Game (Emergency Unpause)" action card in both Modern Canvas and Classic IMGUI dashboards under General Settings.
  - Closing the Mod Menu with `PauseGameOnMenu = false` now triggers `ForceResumeGame()` if the engine was stranded in a paused state.
- **Production Tuner v1.4.4 — Bucket Wylej Reflection Delegate Fix**:
  - Corrected `Bucket.Wylej` delegate from `Action<Bucket, float>` to parameterless `Action<Bucket>`, preventing type initializer crashes during Harmony patching.

---

## [1.8.9] - 2026-09-11

### CoreMod, Claim Monitor & Production Tuner: Scene Loading Synchronization, Pause & Physics Fix

- **CoreMod v1.3.4 — Non-Intrusive Scene Loading & Streaming Synchronization**:
  - Removed premature `Time.timeScale = 1.0f` overwrites in `OnSceneLoaded` during intermediate scene loads (`SceneBuffor`, `Build_Stream_Main`, `Scenario_1`, and additive chunk scenes).
  - Preserved `AreaStreamerBase`'s native 30-frame initial freeze, allowing terrain collision meshes to fully generate before physics simulation begins. This completely resolves the bug where starting a fresh game caused the player to spawn out-of-bounds and fall endlessly through the map into the void.
  - Made `ApplyGamePause(false)` strictly conditional on `_isGamePausedByMenu == true`, ensuring the mod menu never tampers with native game loading pauses (`LevelLoadingManager`, `load`, `sceneName`, `THERE IS NO DIFFICULTY CHOSEN`).
  - Aligned `Milex_GMS1_CoreMod.default.cfg` default for `PauseGameOnMenu` to `false` (matching code and documentation).
- **Claim Monitor v1.0.9 — Main Menu & Level Loading Suppression**:
  - Added strict scene and loading state guards to `WarningHUD.OnGUI`: suppressed completely in `MainMenu`, buffer scenes, and whenever `LevelLoadingManager.IsLoading()` is true.
  - Suppressed background equipment scans in `ClaimScanner.PeriodicScan` and `ForceScan` during the Main Menu and level loading.
- **Production Tuner v1.4.3 — Pickup Truck Physics Fix & Shovel Scaling**:
  - Removed faulty `PickupSafetyPatch.cs` which called `FreezeNow()` prematurely before `WaitForFixedUpdate`, preventing PhysX collisions and flying pickup trucks.
  - Dynamically calculates shovel blade extents from `shovel.BladesBoxCollider.size` and `shovel.DigScale` without hardcoded coordinate drift, and clamped voxel cut depth (`DigDepth <= 0.03m`).

---

## [1.8.8] - 2026-09-11

### Production Tuner & Claim Monitor: Test Run 1 Fixes & Enhancements

- **Production Tuner v1.4.2 — Group Reset Defaults Alignment**:
  - Aligned all `BindStep` defaults in `TuningConfig.cs` with `Milex_GMS1_ProductionTuner.default.cfg`, allowing in-game "Reset Group" buttons to restore curated presets.
- **Production Tuner v1.4.2 — Manual HogPan Infinite Water Flow Bug**:
  - Captured initial water state in Prefix and clamped excess drain refunds in Postfix only when water was present before drain, eliminating infinite water loops and phantom particles on dry hog pans.
- **Production Tuner v1.4.2 — Shovel Digging Volume & Speed Fix**:
  - Corrected `_bladeSizez` negative sign and scaled penetration depth (`DigDepth = VanillaDigDepth * multiplier`), enabling single-scoop fills for tuned shovel volumes.
- **Production Tuner v1.4.2 — Bucket to GoldPan Transfer & Gold Loss Protection**:
  - Added safe transfer coroutine prefix (`BucketFillOutCorutinePatch`) to prevent dumping more volume than target gold pans can accept (`PanMaxFill - _GroundVolume`), preserving 100% of material and gold from destruction.
- **Claim Monitor v1.0.8 — Conveyor False Alerts**:
  - Standalone scene conveyors now verify stationary wash plant presence and cable hookups before alerting, eliminating unpowered warnings on fresh games.
- **Claim Monitor v1.0.8 — F3 Hotkey & Mod Menu Synchronization**:
  - Linked `EnableDebugGroup.SettingChanged` so the F3 hotkey and in-game menu toggle remain 100% in sync with cursor locking and scanner refreshes.
- **Claim Monitor v1.0.8 — Warning HUD Max Height Clamping (`HudMaxHeight`)**:
  - Clamped dynamic window height to `HudMaxHeight` with scrollviews enabled in both full and compact views.
- **Claim Monitor v1.0.8 — ClaimDumper Expansion**:
  - Added `HogPan`, `Bucket`, `GoldPan`, and `Shovel` to diagnostic dump component candidates.

---

## [1.8.7] - 2026-09-06

### Production Tuner: Robust Fuel Hose Reach Scaling (`FuelHose_Length`) & Physics Integrity

- **Production Tuner v1.4.1 — Fixed & Re-introduced Fuel Hose Reach (`FuelHose_Length`)**:
  - Fixed the fatal issue where extending fuel hose reach broke the dispenser nozzle (`ROPE_REKT`, 10,000 durability damage).
  - Properly scales the true rope joint `ConfigurableJoint.linearLimit` on `ShovelRopeDestruction` and updates internal `jlimit` fields.
  - Automatically wires `FuelPistolHoldable.MyConfigurableJ`, allowing vanilla `Attach` distance validation and strong 5,000,000 N vehicle locking.
  - Buffers `breakForce` and `breakTorque` (min 50,000 N) to prevent false snaps during player running and vehicle suspension movement.
  - Configurable in `[Group5_Trailers]` via continuous slider (1.0x–5.0x, default 2.0x).
- **Production Tuner v1.4.1 — Stationary Tanks & Diesel Generator Scope (`FuelTank_Capacity`)**:
  - Clarified UI descriptions: `FuelTank_Capacity` deliberately covers both stationary claim depot fuel tanks and stationary claim infrastructure (large diesel generators, pump stations) sharing the internal `FuelStationController` component, ensuring reliable fuel reserves across claim equipment.

---

## [1.8.6] - 2026-09-06

### Production Tuner: Stationary Fuel Tanks, Fuel Hose Length, Vanilla Baseline Constants & Continuous Slider Support

- **Production Tuner v1.4.0 — Continuous Float Slider Support (`AcceptableValueRange`)**:
  - Replaced all BepInEx `AcceptableValueList<float>` bindings with continuous `AcceptableValueRange<float>(0.5f, max)`.
  - Fixes the bug where in-game slider adjustments produced floating-point values (e.g. 5.81x) that were rejected by BepInEx and reset back to 0.5x upon closing the menu.
- **Production Tuner v1.4.0 — Verified Vanilla Baseline Constants Architecture**:
  - Replaced fragile live-object dynamic base reading with verified hardcoded vanilla baseline constants across all stationary and hand equipment patches (`VanillaHogPanCapacity = 10f`, `VanillaBucketCapacity = 15f`, `VanillaShovelVolume = 0.1f`, `VanillaTrailerCapacity = 1000f`, `VanillaStationaryTankCapacity = 10000f`, `VanillaMaxFill = 15f`, `VanillaCleanSpeed = 0.01f`, etc.).
  - Completely eliminates savegame drift and compounding multiplier calculations when loading games with pre-existing modded values.
- **Production Tuner v1.4.0 — Vehicle Prefab Baseline & Dump Truck Display Fix**:
  - Vehicle capacities (`DumpTruck`, `WheelLoader`, `Excavator`, `BackhoeLoader`) are stored in `DiggingController._maxShovelVolume` (`[XmlIgnore]`), which is never serialized into savegames and cleanly instantiated from vanilla prefabs.
  - Dynamically captures pristine prefab baseline upon initial registration in `Update()` Prefix, scales capacity, and synchronizes reciprocal volume `_invmaxShovelVolume = 1f / targetVol` via reflection.
  - Completely fixes the bug where a bogus hardcoded `6400f` constant caused dump trucks loaded from savegames to jump and lock at 100% capacity.
- **Production Tuner v1.4.0 — Harmony Startup Lifecycle Safety**:
  - Removed invalid `[HarmonyPatch(..., "Start")]` hooks from classes lacking a `Start()` method in `Assembly-CSharp` (`HogPanDirtBox`, `Bucket`, `Shovel`, `ConveyorElevator`, `DumpTruck`).
  - Completely eliminates the fatal `Undefined target method` exception in `Harmony.PatchAll()` that previously caused mod initialization to abort.
- **Production Tuner v1.4.0 — Mobile Fuel Trailer Detection Fix**:
  - Replaced brittle GameObject name matching with machine type verification (`Trailer.MyMachineType == MachineType.TrailerFuel` / `TRAILER_FUELTANK_FUELMAXCAPACITY`), preventing mobile fuel trailers from being misclassified as 10,000L stationary tanks.
- **Production Tuner v1.4.0 — Multi-Instance Synchronization & Loader Fast-Paths**:
  - Ensured all active instances in `Tracked.Values` update simultaneously before `_lastMultiplier` updates.
  - Synchronized `_invmaxShovelVolume` via reflection for excavators, wheel loaders, and backhoe loaders.
  - Synchronized Nuggetator (`MatScrubber`) internal cleaning ratio `_ratio` with scaled bucket capacities.
- **Production Tuner v1.4.0 — Stationary Fuel Tank Capacity (`FuelTank_Capacity`)**:
  - Extends fuel capacity scaling to all stationary `FuelStationController` objects on the claim (separate from the mobile fuel trailer). Default: `2.0x`.
- **Production Tuner v1.4.0 — Fuel Nozzle Physics Integrity & Hose Reach Reversion**:
  - Completely reverted experimental physics joint manipulation (`FuelHoseLength`) on `FuelPistolHoldable` which conflicted with `ConfigurableJoint` recreation in `ShovelRopeDestruction.OnJointBreak`, causing fuel nozzles to instantly break (`ROPE_REKT`) and suffer 10,000 durability damage upon attachment.
  - Safe refuel pump flow speed scaling (`TankingSpeed`) is cleanly retained without touching rope joints or physics anchors.
- **CoreMod v1.3.2 — Mouse Cursor State Capture & Hardware Lock Fix**:
  - Fixed cursor state leakage where the mouse cursor remained visible and unlocked after closing the Mod Menu during gameplay.
  - Added getter suppression (`SuppressGetterPatch`) and captured the game's actual lock state prior to setting `IsMenuOpen = true`.
  - Fixed `InputManager.SetPauseMenuBlocked` reflection/direct parameter mismatch (`blocked, "MilexModMenu"`).
  - Integrated `CursorManager.Instance.Refresh()` on menu close to re-engage the game's native hardware window clipping and hide the cursor cleanly.

---

## [1.8.5] - 2026-09-06

### Claim Monitor & CoreMod: Multilingual Localization Architecture, Game Key Resolution & Scrollbar-Free HUD

- **CoreMod Framework Localization Extension**:
  - Added caller-aware `LocalizationManager.Translate`, `LocalizationManager.TranslateFormat`, and concise aliases `LocalizationManager.T` / `LocalizationManager.Format`.
  - Added `T` and `Format` directly to `ModBase` for clean, boilerplate-free sub-mod localization.
  - Implemented `LocalizationManager.ResolveGameText(string textOrKey)`: bridges mod translations with Gold Mining Simulator's native `LocalizationKey.GetLocalized()` and provides clean Title Case fallback formatting for internal shop/part tokens.
- **Claim Monitor: 100% Multilingual Alert & Status Pipeline**:
  - Replaced all hardcoded English strings throughout `ClaimScanner` and `ClaimDiagnosticsData` with dynamic calls to the CoreMod localization framework.
  - Wear parts (e.g. `SHOP_ITEM_PARTS_GLACIER_CREEK_ENGINE_NAME`, `SHOP_ITEM_PARTS_PLANTER_WATERPUMPFUSE_NAME`, `SHOP_ITEM_PARTS_PLANTER_WATERPUMPFILTER_NAME`) are now resolved directly to player language text.
- **CoreMod: Embedded Default Config Template Engine (`ExtractDefaultConfigFile`)**:
  - `ModBase.Config` now intercepts missing `%AssemblyName%.cfg` files on disk prior to `ConfigFile` initialization and automatically extracts the embedded `%AssemblyName%.default.cfg` template from DLL resources.
  - Gives complete authorial control over default settings, section ordering, and descriptions directly from version-controlled repository templates.
- **Claim Monitor: Intelligent Cable & Hose Connection Detection**:
  - Implemented deep physical attachment tracking (`IsUtilityConnected`) for electric/mobile water pumps, water towers, and generators.
  - Detects socket plugins, power cables, water intake/output hoses (`WaterRopeOut`, `_ropeWaterIn`, `_ConnectedRope`, `Holder` components), electric consumers (`_powerConsumer`), and network consumer lists (`_WaterStationConsumerList`, `_PowerStationConsumerList`).
  - Standalone equipment standing idle or stored on the claim without connected cables or hoses is tagged as unused (`IsConnected = false`). Suppresses all inactive generator warnings, empty reservoir warnings, and component wear alerts (such as pump fuses or filters) in the Warning HUD for disconnected equipment.
- **Claim Monitor: Big Generator Socket Breaker Button Suppression (`MonitorGeneratorSwitchButtons`)**:
  - Added new configuration option `MonitorGeneratorSwitchButtons` (default `false`).
  - Filters out individual socket circuit breaker buttons on the big power generator (`Power_Generator_Switch_Button`), preventing up to 10 duplicate button wear/breakdown notices from spamming the Warning HUD unless explicitly opted in.
- **Warning HUD Usability & Dimension Overhaul**:
  - Removed the redundant compact/full toggle button from the window header (`[ - Kompakt ]` / `[ + Voll ]`). HUD mode is controlled directly via mod configuration.
  - Dynamic vertical auto-sizing in **both** compact and normal modes: the window dynamically calculates needed height based on alert count and text length, completely eliminating inner scrollbars. Clamped only to screen resolution (`Screen.height - 40f`).
- **HUD and Diagnostic Inspector Localization**:
  - All labels, badges (`[OK]`, `[CRITICAL]`, `[WARNING]`), buttons (`Force Rescan`, `Dump All to File`), nominal status texts, and summaries in the Warning HUD and Diagnostic Inspector (F3) adapt automatically to the user's selected language.
- **CoreMod: Centralized Cursor Requester Architecture & Backdrop Decoupling**:
  - Implemented `CorePlugin.RequestCursorUnlock(id)` and `ReleaseCursorUnlock(id)`: external diagnostic overlays (such as ClaimMonitor's F3 inspector) can unlock the mouse cursor and block game camera input directly without requiring the main Mod Menu to be open.
  - Removed full-screen backdrop click-to-close behavior in `ModernCanvasMenu`: clicking outside the dashboard panel will no longer accidentally close the Mod Menu.
- **Claim Monitor: Mobile Wash Plant Overhaul & Water Connection Enforcement**:
  - Excluded internal drum components (`WashPlantMobileTrommel`) from independent machinery detection, eliminating phantom "Trommel failure (no power)" warnings.
  - `MobileWashplant` and `MiniWashplant` are only evaluated when their water intake hose (`_WaterConsumer`) is physically connected. Corrected socket detection to check `ObjectInHolder != null` instead of the static prefab GameObject (`RopeObjectConnectedLogic != null`), completely fixing false "turned off" warnings for unhooked parked machines.
  - Disconnected or parked mobile plants generate zero HUD alerts and skip wear checking completely.
  - Added diesel fuel monitoring for `MiniWashplant` via `FuelStationController` with early alerting on diesel depletion (`issue.miniwashplant.no_fuel`).
- **Claim Monitor: Diagnostic Inspector (F3) Realignment & Direct Cursor Access**:
  - Pressing **`F3`** now directly unlocks the mouse cursor and halts camera rotation via the new CoreMod cursor requester API, enabling effortless inspection.
  - Fixed category filter comparisons across all setups and split the inspector toolbar into two distinct rows to comfortably fit all 8 category buttons.
- **Bilingual Dictionary Expansion**:
  - Added comprehensive translation dictionaries in `Milex_GMS1_ClaimMonitor_en.json` and `Milex_GMS1_ClaimMonitor_de.json` with full key parity and natural German translations.

---

## [1.8.4] - 2026-09-06

### Dynamic Physics Enhancements, Equipment Wear Telemetry & Modern UI Streamlining

- **Production Tuner: Vehicle Physics & Chassis Dynamics**:
  - **Excavator Handbrake Chassis Stabilization**: Automatically locks horizontal position and tilt rotations via `RigidbodyConstraints` whenever the excavator's handbrake is engaged, preventing tipping, sliding, or track lifting during heavy bucket scooping. Releasing the handbrake restores normal driving immediately.
  - **Dump Truck Driving Mass Compensation**: When dump truck payload capacity is multiplied above vanilla levels, physical mass drag (`Dirt.LoadMass`) is dynamically balanced during movement (`MachineMove`) using Harmony `__state`. Heavy dump trucks accelerate and climb inclines with agile handling while retaining their full enlarged volume.
- **Claim Monitor: Comprehensive Component Wear & Breakdown Early-Warning System**:
  - **Direct `CheckAndRepair` Integration**: Monitors all installed wear components across active wash plants, conveyors, and water pumps (spray nozzles, drive belts, trommel chains/rollers, jig mechanisms, shaker springs, and water filters).
  - **Strict Setup Association & Stray-Part Isolation**: Only components physically installed on active equipment (`IsInPlace == true`) belonging to enabled setups are scanned. Detached, loose, or scrapped parts lying on the ground are completely ignored.
  - **Configurable Wear Warning Threshold**: Added `ComponentWearWarningThreshold` (default 20%, range 5%-50%). Emits early yellow warnings before total breakdown and critical red alerts upon component destruction.
- **UI & HUD Improvements**:
  - **CoreMod Modern Canvas**: Hidden the legacy "Classic UI" engine switch button to deliver a clean, modern top bar where the search input smoothly stretches up to the close button.
  - **Diagnostic Inspector (F3)**: Widened window to 1100px and streamlined toolbar layout so all category filter buttons fit side-by-side without clipping.
  - **Warning HUD**: Auto-adjusts height in compact mode based on text length to guarantee all warnings are readable without cutoff.

---

## [1.8.3] - 2026-09-05

### Claim Monitor: Stationary Wash Plant Water Detection Overhaul & Glacier Creek / Derocker Support

- **Tier 3–5 Stationary Wash Plant Water Supply Recognition**:
  - Overhauled water detection across all stationary wash plants to directly query the simulation engine (`WashplantShakerBase.Water.HaveWater`, `CheckHasWater()`, and `_hasWater`), eliminating false alarms caused by stale or unrendered visual HUD indicators.
  - Added full first-class support for `GlacierCreek` (Tier 4) and `DeRocker` (Tier 3/4) so all stationary shaker variants are recognized and monitored seamlessly.
- **Granular Water Failure Diagnostics**:
  - Machinery warnings now provide exact, actionable reasons for missing water (hose disconnected, pump turned off, pump intake dry / tower empty, pump disabled, broken/frozen hose, or damaged nozzle).
- **Strongly-Typed Engine Integration**:
  - Replaced reflection fallbacks with direct, strongly-typed checks against the game's simulation classes (`WashplantShakerBase`, `WashplantTrommelBase`, `WashplantDuplexJigBase`, `MobileWashplant`, `MiniWashplant`).

---

## [1.8.2] - 2026-09-05

### Claim Monitor: Generator/Pump Running State Validation & Sleek Minimal Compact HUD

- **Generator & Water Pump Running State Validation**:
  - Fixed power state checks where machines connected to a stopped/empty generator were falsely reported as powered.
  - Actively validates generator/pump controller states (`PowerStationController.IsWorking`, `isEnabled`, `!IsOverLoaded`, and `WaterStationController.IsWorking`) alongside `Indicator.LastState` (State 0 = White/Off, State 1 = Gray/Disconnected, State 3 = Red/Overload).
- **Duplex Jig, Gravel Pump & Mini Wash Plant Requirements**:
  - Corrected requirement profiles: Duplex Jigs and Gravel Pumps only consume electric power and do not require water connections (eliminating false water warnings on Tier 5 Glacier Creek / Gravel Pump setups).
  - Configured Mini Wash Plant to run on internal fuel engine without external electric power requirements.
- **Sleek Minimal Compact HUD Overlay**:
  - Redesigned Compact Mode into a sleek, minimal text HUD overlay that directly lists active warnings as clean, compact bullet points with color-coded severity tags (`• [CRITICAL]`, `• [WARN]`).
  - Automatically sizes to fit active warnings tightly and supports full window drag & drop across the entire compact banner.

---

## [1.8.1] - 2026-09-04

### Claim Monitor: Indicator-Driven Detection, Orange Beast Deduplication & Fuel Bar Overlay

- **In-Game Visual Indicator-Driven State Detection**:
  - Overhauled power and water tracking by reading the state of `GoldDigger.Indicator` instances (the actual in-game green/gray water drop and lightning bolt icons).
  - Exempted Trommels from water supply requirements (Trommels only require electric power).
- **Orange Beast Setup Presence & Deduplication**:
  - Requires active `OrangeBeastWashPlantGoldCounter` on the claim to prevent false alarms on uninstalled setups.
  - Ignores structural frame GameObjects and deduplicates Orange Beast Shaker items.
- **Front-Aligned Vehicle Quick-Switcher Fuel Status Bar**:
  - Re-positioned fuel status indicator directly in front of each vehicle card (to the left of the selection area).
  - Displays a clean 6px vertical status bar with fuel percentage text, completely eliminating overlap with distance labels (`174 ft`).
- **HUD Position Cleanup**:
  - Removed non-functioning X/Y position sliders from the config menu; dragged window position is persisted automatically via `PlayerPrefs`.

---

## [1.8.0] - 2026-09-04

### Added: Milex Claim Monitor & CoreMod Cursor Lock Bugfix

- **New Sub-Mod: Milex GMS1 Claim Monitor (v1.0.1)**:
  - Real-time on-screen Warning HUD telemetry dashboard with draggable, auto-saving UI.
  - Setup-accurate wash plant classification across three tiers:
    - **Setup 1**: Mobile Wash Plants (Mini & Mobile Wash Plants).
    - **Setup 2**: Stationary Setup T3–T5 (Shaker/Glacier Creek, Trommel/Reinforced Trommel, Duplex Jigs/Gravel Pumps, Sluices).
    - **Setup 3**: Setup T6 / Orange Beast (Giant Shaker, Extended Sluices).
  - Optional Feeding Chain monitoring (Hoppers and Conveyors) linked to wash plant setups.
  - Malfunction detection for Trommel drive chain breakage, Shaker motor/water/power failures, Duplex Jig pump failures, and full buckets (with dedicated single-bucket logic for Tier 5 Gravel Pumps).
  - Robust power and water status verification via active `PowerConsumer` and `WaterConsumer` game properties.
  - Sluice mat fill level tracking with configurable warning thresholds (default: 90%) and critical overflow alerts (100%).
  - Vehicle and heavy machinery fuel tracking with low fuel warnings (< 15%) and empty tank critical alerts.
  - Power generator and water tower level/operation monitoring.
  - Built-in Diagnostic Inspector (`F3`) and deep memory object dumper for claim diagnostics.
  - Configurable update scan interval (1.0s to 30.0s).
  - Complete English and German localization out of the box.
- **CoreMod Cursor Lock/Visibility Bugfix**:
  - Resolved cursor state leakage where the mouse cursor remained visible and unlocked after closing the in-game menu during gameplay.
  - Intercepted cursor state before setting `IsMenuOpen = true` and prevented internal UI unlock calls from overwriting the remembered game lock state.
  - Guarantees 1:1 restoration of first-person gameplay mouse lock and visibility upon menu close.

---

## [1.7.0] - 2026-09-03

### Added: Next-Gen Modern Dashboard & Dual-Engine Menu Architecture

- **Next-Gen Modern Dashboard (uGUI Canvas)**:
  - Built a state-of-the-art runtime Canvas interface created purely in C# with zero external asset dependencies.
  - Interactive window with draggable header, subtle bordered cards (`CardBoxSprite`), and sleek gold accent theme.
  - **Fixed Header Hierarchy with Mod Subtitle**: Top-left header displays fixed `Milex GMS1 CoreMod (v1.3.0)` with a dynamic gold subtitle indicating the currently active mod (`> Production Tuner`).
  - **Dynamic Content-Proportional Filter Tabs**: Tabs now allocate space proportionally based on text length (`flexibleWidth = label.Length`), preventing text clipping on long names while removing unnecessary dead space on short labels.
  - **High-Contrast Tactical Badges**: Inactive tabs render in distinct slate with bright silver labels, while the selected tab pops in radiant gold with dark charcoal text.
  - **Instant Button Hover & Tinting Fix**: Fixed uGUI `targetGraphic.color` ColorBlock multiplication issue, enabling vivid, responsive slate-blue hover states across all buttons and cards.
  - **Non-Flashing In-Place Sidebar Selection**: Switching mods updates existing UI component states directly without destroying and rebuilding GameObjects.
  - **Zero-Jump Scroll Preservation**: Resets and slider adjustments maintain the player's exact scroll position without snapping to the top.
  - **High-Contrast Section Banners**: Enhanced category headers with distinct dark slate container styling, left gold accent bars, and prominent reset buttons.
  - Real-time search filter bar to instantly locate any setting or multiplier across all categories.
  - Modern toggle switches and wide responsive sliders with direct reset-to-default buttons and hover glow effects.
  - **Compact High-Density Layout**: Optimized card heights allowing almost twice as many settings on screen at once.
  - **Category Group Reset**: Added direct reset buttons on section headers to quickly reset entire groups of settings to default values.
  - **Interactive Language Selector**: Dedicated manual language switching cards for German, English, and other supported languages.
  - **Visible Stylized Scrollbars**: Permanent, sleek scrollbars with slate tracks and gold hover highlights.
  - **Full Localization Alignment**: Accurately mapped all sub-mod section titles and configuration keys to language files.
- **Zero-Flicker In-Place Filtering (`FilterCards`)**: Replaced destructive GameObject teardown on tab switches with instant in-place visibility toggling (`card.SetActive(...)`), completely eliminating visual flashes and keeping navigation at a silky-smooth 60 FPS.
- **Strict Tab Bar Containment & Concise Badges**: Added `RectMask2D` on the category tabs bar, shortened labels to clear badges (`Alle`, `Allgemein`, `Werkzeuge`, `Fahrzeuge`, `Waschanlagen`, `Veredelung`, `Logistik`), and implemented proportional layout compression to prevent any tabs from overflowing past the right window edge.
- **Sidebar Selection State Fix (White Card Bug Resolved)**: Configured `colors.selectedColor = normalColor` and deselected focus on click via `EventSystem.current.SetSelectedGameObject(null)`, preventing selected mod cards from flashing or getting stuck in solid white.
- **Full Native Language Dropdown Selector**: Replaced horizontal buttons with an expandable, scrollable dropdown listing all 21 supported languages in their respective native endonyms (`Deutsch`, `English`, `Français`, `Español`, `Polski`, `Русский`, etc.) with active `[v]` badges.
- **Mutual Exclusivity & Permanent Visibility**: Both "Use Game Language" and "Select Language" remain permanently visible in CoreMod settings; the manual selector dynamically disables and dims when automatic detection is enabled.
- **Interactive Missing Translation Template Generator Modal**: Selecting any language missing translations now triggers a dedicated modal dialog prompting the player to generate JSON templates on-demand directly into the localization directory.
- **Fixed Header Hierarchy**: Guaranteed that the top-left main title permanently displays `Milex GMS1 CoreMod (v1.3.0)` while sub-mod titles and versions cleanly route to the secondary gold subtitle.
- **Compact Missing Translation Modal & Localization Folder Opener**:
  - Redesigned the missing translation prompt into a sleek, compact 460x252 modal card with centered layout and high-contrast styling.
  - Displays the target destination directory path (`BepInEx/plugins/Milex GMS1 Mod Localization/`) right inside the dialog.
  - Added a direct **`[ Open Folder ]`** button that instantly opens the localization directory in Windows Explorer using `Process.Start`.
- **Sub-Mod Developer Guide & AI Agent Blueprint Documentation**:
  - Created [`DEVELOPER_GUIDE.md`](DEVELOPER_GUIDE.md): A comprehensive handbook for human modders explaining `ModBase` inheritance, zero-code UI generation, baseline memory (`OriginalValueStore`), and multi-language localization.
  - Created [`AGENT_MOD_GUIDE.md`](AGENT_MOD_GUIDE.md): A complete technical specification and system prompt designed for AI coding agents to create 100% framework-compliant sub-mods from game code excerpts.
- **Dual-Engine Menu Architecture (`IMenuRenderer`)**:
  - Fully decoupled rendering layer from core plugin logic.
  - Seamless in-game switching between **Modern (uGUI Canvas)** and **Classic (IMGUI)** via configuration setting and header buttons.

---

## [1.6.0] - 2026-09-02

### Added: Production Tuner Vehicle & Logistics Expansion

- **Mobile Conveyors (Frankenstein & Cordylus)**:
  - Added directly under the Vehicles group in the in-game menu with independent controls for buffer capacity (default: 2.0x) and transport speed (default: 2.0x).
  - Fully automatic detection for both conveyor types.
  - **Synchronized Throughput**: The hopper buffer empties proportionally with the selected belt speed, moving larger dirt portions at faster intervals so material moves continuously without bottlenecking.
- **Dedicated 3-Axis Excavator Hydraulic Controls**:
  - 3 independent sliders for fine-tuned excavator operation:
    - **Boom / Arm Speed** (lifting and extending, default: 2.0x)
    - **Turret Rotation Speed** (cabin and upper carriage swing, default: 2.0x)
    - **Bucket Tilt Speed** (curling and dumping, default: 1.0x)
  - Full range of motion without physical rotation limits or jitter.
- **In-Game Localization Alignment**:
  - All components aligned with official in-game terms (*Dump Truck*, *Wheel Loader*, *Backhoe Loader*, *Wave Table*, *Gold Nuggetator*, *Miner's Moss*, etc.).
  - Streamlined descriptions into concise, clear player-facing summaries.
- **Fixed Vanilla Restoration on Mod Toggles**:
  - Toggling the mod off and on now cleanly restores unmodified game defaults without doubling capacities or halving fill levels.
  - All equipment, vehicles, and tools reliably remember their original game values.

---

## [1.5.2] - 2026-09-01

### Performance Optimization (Eliminated FPS Drop)

- **Zero-Stutter Fast Path**: All calculations exit immediately when slider values are unchanged, completely eliminating micro-stutters and frame rate drops.
- **Optimized Memory Management**: Streamlined internal updates to prevent garbage collection pauses during gameplay.
- **Hydraulic Cylinder Caching**: Vehicle joints are registered once when spawning, saving CPU power during vehicle operation.

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
