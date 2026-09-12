# Milex GMS1 Production Tuner

- **Version:** `1.4.11` ([View Changelog](CHANGELOG.md))
- **Mod Name:** Milex GMS1 Production Tuner
- **Author:** Milex
- **Assembly File:** `Milex_GMS1_ProductionTuner.dll`
- **Dependencies:** `Milex_GMS1_CoreMod.dll`, `BepInEx 5.4.21+`

The **Production Tuner** gives you total control over work speeds, material capacities, hydraulic movement rates, and logistics in *Gold Mining Simulator* (*Gold Rush: The Game*). 

Whether you want realistic mining or high-speed gameplay, you can adjust all 29 sliders live in-game with zero restarts required. Built-in smart protections ensure that faster speeds and larger capacities never cause lost gold, spilled dirt, or blown generator fuses.

---

## Quick Install & Getting Started

1. **Prerequisites**: Ensure you have **BepInEx 5** (x64) and **`Milex_GMS1_CoreMod.dll`** installed in your game directory.
2. **Install**: Copy `Milex_GMS1_ProductionTuner.dll` into your `BepInEx/plugins/` folder.
3. **Start Game**: Launch the game normally.
4. **Quick Start**:
   - Press **`Insert`** in-game to open the Milex Mod Menu.
   - Select **Production Tuner** in the left sidebar to view all categories (*Hand Tools*, *Vehicles*, *Wash Plants*, *Fine Processing*, *Trailers & Logistics*).
   - Adjust any slider to your liking. Changes take effect immediately in the live game!

---

## In-Game Controls & Usage

- **`Insert` (Mod Menu)**: Opens the in-game dashboard where you can adjust speeds and capacities on the fly.
- **Continuous Fine-Tuning Sliders**: Smooth, continuous sliders let you dial in precise multipliers (from 0.5x up to 10.0x, and up to 20.0x for processing equipment).
- **Default Displays & Reset**: Every setting displays its recommended default value. Click **`[ Reset Group ]`** at the top of any section to revert only that category back to defaults.
- **Instant Mod Toggle**: You can temporarily disable the mod via the `[x] Active` checkbox in the sidebar. All machines instantly revert to 100% original vanilla behavior without restarting the game.

---

## Detailed Features & Functions

### 1. Hand Tools & Early Game
- **Shovel (1-Scoop Filling)**:
  - Fills your shovel to 100% in a single scoop instead of having to dig 5 times.
  - Symmetrically balanced so that with default mod settings, exactly **1 shovel scoop fills 1 bucket to 100%**.
- **Buckets & Handheld Hog Pan**:
  - Lets you carry more dirt per bucket to speed up early-game mining.
  - Hog Pan dirt washes through faster while water drainage stays perfectly balanced so your Miner's Moss mats won't run dry.
  - Handheld buckets and standalone Hog Pans are decoupled from wash plant setups; Duplex Jig buckets and attached tailing Hog Pans scale synchronously with your wash plants instead.
- **Gold Pan Spill & Flake Protection**:
  - When emptying a large bucket into a Gold Pan, pouring stops automatically when the pan is full. Dirt and gold flakes are never destroyed or wasted.

### 2. Heavy Machinery & Vehicles
- **Small & Large Excavators**:
  - Independent hydraulic speed sliders for the **Boom/Arm**, **Turret Rotation**, and **Bucket Tilt/Curl** give you responsive, fluid handling without jerky vehicle tipping.
  - Increased bucket dirt capacity lets you move paydirt much faster.
- **Wheel Loader**:
  - Increased front bucket capacity with an automatic **hydraulic lifting boost**, allowing heavy loads to lift effortlessly without bogging down the hydraulics or rolling the loader forward.
- **Backhoe Loader & Dump Truck**:
  - Faster bucket loading and larger load capacities.
  - Expanded Dump Truck dump bed capacity so you can transport more paydirt per haul.
- **Heavy Conveyor Belts (Frankenstein & Cordylus)**:
  - Independent sliders for both conveyor systems: one for dirt capacity and one for transport speed.

### 3. Wash Plants, Sluices & Mats
- **Feeder Hoppers & Conveyor Elevators**:
  - Larger hopper capacities and faster conveyor bucket throughput for continuous feeding.
- **Shakers & Trommels**:
  - Increased gravel throughput and dirt washing speeds across all stationary plant tiers.
- **Synchronized T3–T5 Wash Plant Setup**:
  - Single synchronized slider that scales all interconnected T3–T5 wash plant components in 100% lockstep: 12 Sluice Miner's Moss mats, Sluice Nugget Trap grates, Duplex Jig buckets, and attached tailing Hog Pan (box & mats). All stations fill concurrently and reach 100% at the exact same moment without material overflow.
- **Synchronized T6 Orange Beast Setup**:
  - Synchronously scales the 20 Orange Beast Miner's Moss mats and the internal gold counter of the Tier 6 wash plant setup.
- **Resource Neutrality (Safe Electricity & Water)**:
  - Higher throughput does not increase power or water demand. Water pumps and diesel generators run at normal baseline load without blown fuses or pressure drops.

### 4. Fine Processing Equipment
- **Gold Nuggetator**: Faster washing speed to quickly separate nuggets from paydirt.
- **Magnetite Separator**: Accelerated separation speed and enlarged magnetite hopper capacity.
- **Wave Table**: Faster shaking speed and increased concentrate capacity.

### 5. Fuel Infrastructure & Logistics
- **Mobile Fuel Trailer**:
  - Significantly enlarged trailer fuel tank capacity and accelerated pump flow rate so field refueling is quick and easy.
- **Large Claim Fuel Tank**:
  - Expanded fuel storage for your main stationary claim tank so you don't run dry during large operations.
- **Extended Fuel Hose Reach**:
  - Drastically extends the reach of fuel hoses on both the trailer and stationary tank. You no longer need to park machinery millimeters away to refuel, and nozzles lock securely into place without snapping off.
- **Small Equipment Protection**:
  - Only your fuel trailer and main claim tank are enlarged. Small equipment (portable generators, small water pumps, light towers, Jerry cans) keeps standard sizes, preventing exorbitant fuel bills in town.

### 6. Fast Travel & Trailer Fixes
- **Trailer Fast Travel Auto-Reconnection**:
  - Fixes the annoying vanilla bug where fast travel unhitches trailers and drops them 4 meters behind your pickup. Trailers now stay hitched after fast travel, and you can still manually detach them at the hitch lever whenever you want.
- **Fast Travel Tire Protection**:
  - Fixes the vanilla bug where pickup or trailer tires blow out or break when landing after fast travel. Landing impacts are safely absorbed while normal driving wear remains completely untouched.

### 7. Smart Container Sizing (`AutoScaleDependentInputs`)
- When enabled, increasing your bucket size automatically keeps downstream processing equipment (Hog Pan, Wave Table, Magnetite Separator, Magnetite Trailer) at least as large as the bucket.
- The bucket slider is dynamically capped at the maximum allowed size of dependent equipment, making dirt overflow or material loss impossible.

---

## Configuration Reference (`Milex_GMS1_ProductionTuner.cfg`)

All settings are configured via the in-game menu (**`Insert`**) or saved in `BepInEx/config/Milex_GMS1_ProductionTuner.cfg`:

### Section `[General]`

| Key | Type | Default | Description |
|---|---|---|---|
| **`AutoScaleDependentInputs`** | `Boolean` | `true` | Smart Container Sizing: Automatically keeps downstream equipment at least as large as the bucket to prevent overflow. |

### Multiplier Defaults & Ranges

All multipliers use smooth continuous sliders. Standard machinery ranges from **0.5x to 10.0x**, while processing equipment offers an extended range up to **20.0x**.

| Section | Target Equipment & Multipliers | Default |
|---|---|---|
| **`[Group1_HandTools]`** | Shovel Scoop Capacity | `6.0x` |
| | Handheld Bucket Capacity | `2.0x` |
| | Standalone Hog Pan Dirt Capacity | `6.6x` |
| | Mobile Wash Plant Speed | `3.0x` |
| | Mobile Wash Plant Capacity | `3.0x` |
| **`[Group2_Vehicles]`** | Small Excavator Bucket Capacity | `3.0x` |
| | Excavator Arm / Boom Speed | `2.0x` |
| | Excavator Turret Rotation Speed | `2.0x` |
| | Excavator Bucket Tilt / Curl Speed | `1.0x` |
| | Wheel Loader Loading Capacity | `3.0x` |
| | Backhoe Loader Loading Capacity | `3.0x` |
| | Dump Truck Bed Capacity | `3.0x` |
| | Frankenstein Heavy Conveyor Capacity | `3.0x` |
| | Frankenstein Heavy Conveyor Speed | `3.0x` |
| | Cordylus Heavy Conveyor Capacity | `3.0x` |
| | Cordylus Heavy Conveyor Speed | `3.0x` |
| **`[Group3_WashPlantModules]`** | Feeder Hopper Capacity | `3.0x` |
| | Conveyor Elevator Bucket Capacity | `3.0x` |
| | Wash Plant Buffer Capacity | `3.0x` |
| | Wash Plant Processing Speed | `3.0x` |
| | T3–T5 Setup Capacity (Mats, Grates, Jigs, Hog Pan) | `5.0x` |
| | T6 Orange Beast Setup Capacity | `2.0x` |
| **`[Group4_FineProcessing]`** | Gold Nuggetator Washing Speed | `3.0x` |
| | Magnetite Separator Processing Speed | `3.0x` |
| | Magnetite Separator Earth Capacity | `4.9x` |
| | Wave Table Shaking Speed | `3.0x` |
| | Wave Table Concentrate Capacity | `4.9x` |
| **`[Group5_Trailers]`** | Magnetite Trailer Earth Capacity | `3.0x` |
| | Mobile Fuel Trailer Capacity | `3.0x` |
| | Stationary Claim Fuel Tank Capacity | `3.0x` |
| | Fuel Hose Reach & Joint Strength | `5.0x` |

---

## Localization Files

Translations are managed automatically by CoreMod:
- `BepInEx/plugins/Milex GMS1 Mod Localization/Milex_GMS1_ProductionTuner_en.json` (English)
- `BepInEx/plugins/Milex GMS1 Mod Localization/Milex_GMS1_ProductionTuner_de.json` (German)

---

## Development & Compilation

To compile this project from source:

```powershell
dotnet build GMSModding.sln
```

---

## Acknowledgements & Credits

Sincere thanks to the pioneers of the Gold Rush modding community whose reverse-engineering insights helped inspire this project:
- **stregkoden**: *Better_Conveyor*, *Better_FEL*, *Better_FuelTrailer*, *Better_Nuggetator*, and *HogPan_Pack*.
- **DeepCore / Jonathan**: *Bigger Shovel* and *Smart Buckets*.
- **FedeRama**: *GMS.WaveTableCapacity*.
- **DSS**: *DSS PickupSnap* (trailer hitching and trigger coupling insights).
- **GMS Community Modders**: Foundations for `MagnetiteSeparator`, `DumpTruck`, and volume conservation.

---

## License & Free Use (Open Source)

All code in **Milex GMS1 Production Tuner** is free and open source:
> Anyone is free to use, copy, modify, adapt, or incorporate this code into other mods and projects, in whole or in part, without restriction.
