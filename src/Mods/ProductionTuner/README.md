# Milex GMS1 Production Tuner

- **Version:** `1.3.0` ([View Changelog](CHANGELOG.md))
- **Mod Name:** Milex GMS1 Production Tuner
- **Author:** Milex
- **Assembly File:** `Milex_GMS1_ProductionTuner.dll`
- **Dependency:** `Milex_GMS1_CoreMod.dll`

The **Production Tuner** gives you complete control over processing speeds, throughput, hydraulic movement rates, and capacities for all equipment, tools, and vehicles in *Gold Rush: The Game*.

All 29 individual parameters are applied as direct multipliers to the game's original base values. Each setting has its own carefully tuned default multiplier and can be adjusted in precise 0.5 steps (ranging from 0.5x up to 10.0x or 20.0x).

---

## Documentation & Navigation

| Document | Link |
|---|---|
| **Repository Root Documentation** | [README.md](../../../README.md) |
| **Repository Root Changelog** | [CHANGELOG.md](../../../CHANGELOG.md) |
| **CoreMod Documentation** | [CoreMod README.md](../../CoreMod/README.md) |
| **Production Tuner Changelog** | [ProductionTuner CHANGELOG.md](CHANGELOG.md) |
| **Community Knowledge Compendium** | [COMMUNITY_KNOWLEDGE_COMPENDIUM.md](../../../Community%20Knowledge/COMMUNITY_KNOWLEDGE_COMPENDIUM.md) |

---

## Installation & First Steps

1. **Requirements**: BepInEx 5 (x64) and `Milex_GMS1_CoreMod.dll` must be installed in the game directory.
2. **Installation**: Copy `Milex_GMS1_ProductionTuner.dll` into your `BepInEx/plugins/` folder.
3. **Launch**: Start the game. On the first launch, the mod will automatically create its configuration and localization files.

---

## In-Game Usage

1. Press **`Insert`** to open the mod menu.
2. Select **Production Tuner** in the left sidebar.
3. Adjust any of the 29 individual sliders independently.
4. Each entry displays its default value (e.g. `(Default: 2.0)`).
5. Click **`[ Reset Group ]`** to restore all settings in a section to their default multipliers.
6. **Live Toggle**: If you disable the mod via the `[x] Active` checkbox in the menu, all machines instantly revert to their original vanilla values without requiring a game restart.

---

## Configuration

The configuration file is automatically generated on first startup:  
`BepInEx/config/Milex_GMS1_ProductionTuner.cfg`

### Section `[General]`

| Key | Default | Description |
|---|---|---|
| `AutoScaleDependentInputs` | `true` | Cascade Protection: Automatically keeps dependent downstream containers (hog pan, wave table, magnetite separator, trailers) at least as large as the bucket multiplier to prevent material overflow. |

### Multiplier Groups

All sliders operate in **0.5 increments**. Standard components range from **0.5x to 10.0x**. Dependent container capacities (hog pan, magnetite separator, wave table, magnetite trailer) offer an extended range of **0.5x to 20.0x** to provide ample buffer capacity when emptying large buckets repeatedly.

| Section | Included Components & Default Multipliers |
|---|---|
| `[Group1_HandTools]` | Shovel Scoop Capacity (2.0x), Bucket Capacity (2.0x), Hog Pan Capacity (2.0x), Mobile Wash Plant Speed (3.0x), Mobile Wash Plant Capacity (2.0x) |
| `[Group2_Vehicles]` | Excavator Bucket Capacity (3.0x), Excavator Arm Speed (2.0x), Excavator Turret Rotation Speed (2.0x), Excavator Bucket Tilt Speed (1.0x), Wheel Loader Loading Speed (3.0x), Backhoe Loader Loading Speed (3.0x), Dump Truck Load Capacity (3.0x), Frankenstein Conveyor Capacity (2.0x), Frankenstein Conveyor Speed (2.0x), Cordylus Conveyor Capacity (2.0x), Cordylus Conveyor Speed (2.0x) |
| `[Group3_WashPlantModules]` | Feeder Hopper Capacity (2.0x), Conveyor Bucket Capacity (2.0x), Wash Plant Buffer Capacity (2.0x), Wash Plant Processing Speed (2.0x), Sluice Box Capacity (2.0x), Miner's Moss Mat Capacity (2.0x) |
| `[Group4_FineProcessing]` | Gold Nuggetator Speed (2.0x), Magnetite Separator Speed (2.0x), Magnetite Separator Capacity (2.0x), Wave Table Speed (3.0x), Wave Table Capacity (3.0x) |
| `[Group5_Trailers]` | Magnetite Trailer Capacity (2.0x), Fuel Trailer Capacity (3.0x) |

### Cascade Protection & Bucket Cap (`AutoScaleDependentInputs = true`)

- **Automatic Synchronization**: If you increase the bucket capacity, downstream processing containers (hog pan, magnetite separator, wave table, magnetite trailer) are dynamically synchronized to at least the same value in real time.
- **Dynamic Bucket Ceiling**: The bucket multiplier is capped at the maximum allowed value of dependent inputs, ensuring the bucket can never exceed what downstream stations are capable of receiving.

### Resource Neutrality (Water, Fuel, Electricity)

- **Hog Pan Water Protection**: Water drainage in the hog pan is clamped to the vanilla base rate. Even with 4x or 10x dirt capacity, water drains at normal vanilla speed without drying out mats.
- **Pump & Generator Stability**: Wash plants and electric equipment do not request inflated wattage or water flow. Existing generators and pumps run stably without circuit breaker trips or pressure loss.
- **Hydraulic Torque Scaling**: When wheel loader bucket capacity is enlarged, lifting cylinder torque (`AnimatedJoint.MaxTorque`) is automatically boosted to effortlessly lift full loads.

---

## Localization Files

The mod features full localization support handled by CoreMod:

- **Directory:** `BepInEx/plugins/Milex GMS1 Mod Localization/`
- **Files:** `Milex_GMS1_ProductionTuner_en.json` and `Milex_GMS1_ProductionTuner_de.json`
- Missing language templates can be generated directly via the in-game CoreMod menu.

---

## Development & Compilation

To compile all projects in the solution, use the standard .NET CLI:

```bash
dotnet build GMSModding.sln
```

---

## Acknowledgements & Credits

A major portion of reverse engineering insights, component mappings, and architectural techniques in this mod builds upon pioneering work from the Gold Rush modding community. Sincere thanks and credits go to the following authors and their projects:

- **stregkoden**:
  - *Better_Conveyor*: Component mappings for `ConveyorGround.MaxDirt` and `ConveyorElevator.BucketCapacity`.
  - *Better_FEL*: Discovery of hydraulic torque demands (`AnimatedJoint.MaxTorque`) and steering assist for wheel loaders (`Ladowarka`).
  - *Better_FuelTrailer*: Identification of fuel trailers via `FuelStationController` and proportional refueling flow rates.
  - *Better_Nuggetator*: Analysis of `CleanigDirtSpeed` throughput rates.
  - *HogPan_Pack*: Discovery of `MobileWashplant` and `MiniWashplant` classes, as well as hog pan water consumption mechanics.
- **DeepCore / Jonathan**:
  - *Bigger Shovel*: Precise mathematical $\sqrt{M}$ scaling of shovel digging edges (`_bladeSizex`, `_bladeSizez`) and base value caching patterns.
  - *Smart Buckets*: In-depth analysis of `Bucket` internals (`UpdatePlaneAndMass`, `MudVolume`, material tints).
- **FedeRama**:
  - *GMS.WaveTableCapacity*: Identification of `WaveTable.MaxGroundVolume`.
- **GMS Community Modders**:
  - *Increased Capacity And Speed* & *IncreasedCapacity*: Foundational work on `MagnetiteSeparator`, `DumpTruck`, `OrangeBeastFilter`, and reciprocal volume conservation (`_invmaxShovelVolume`).

---

## License & Free Use (Open Source / Public Domain)

All code in **Milex GMS1 Production Tuner** is released under an open-source license inspired by MIT and The Unlicense:

> **Free Use Notice:**  
> Anyone is free to use, copy, modify, merge, publish, distribute, or incorporate this code into other mods and projects, in whole or in part, without restriction or obligation. A brief attribution or mention in your project's credits is appreciated.
