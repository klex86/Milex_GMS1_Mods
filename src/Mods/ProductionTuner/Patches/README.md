# Patches – Production Tuner

This directory contains all Harmony patch classes for `Milex GMS1 Production Tuner`.

## Structure & Functional Domains

Patches are strictly organized by domain:

```
Patches/
├── Tools/
│   ├── ShovelPatch.cs              (GoldDigger.Shovel: MaxCapacity + Sqrt-Blades)
│   └── BucketPatch.cs              (GoldDigger.Bucket: MaxVolume)
├── WashPlants/
│   ├── HogPanDirtBoxPatch.cs       (GoldDigger.HogPanDirtBox: PlaneVolumeMax + Water Protection)
│   ├── MobileWashPlantPatch.cs     (GoldDigger.MobileWashplant + Mini: MaxFill + FillSpeed)
│   ├── WashPlantShakerPatch.cs     (GoldDigger.WashplantShakerBase: MaxFill + FillSpeed)
│   ├── SluiceBoxPatch.cs           (GoldDigger.WashPlantSluiceBoxDirt: MaxFill)
│   └── MinersMossPatch.cs          (GoldDigger.MinersMoss: MaxGroundVolume)
├── Vehicles/
│   ├── ExcavatorPatch.cs           (Koparka: Digging + 3-Axis Hydraulic Speed: Boom, Turret, Bucket)
│   ├── WheelLoaderPatch.cs         (Ladowarka: Digging + _invmax + Hydraulic Torque Scaling)
│   ├── BackhoeLoaderPatch.cs       (KoparkoLadowarka: Front & Rear Digging Controllers)
│   └── DumpTruckPatch.cs           (GoldDigger.DumpTruck: Digging Capacity)
├── Processing/
│   ├── MatScrubberPatch.cs         (GoldDigger.MatScrubber: CleanigDirtSpeed + InBucket Speed)
│   ├── MagnetiteSeparatorPatch.cs  (GoldDigger.MagnetiteSeparator: MaxFill + FillOutSpeed)
│   └── WaveTablePatch.cs           (GoldDigger.WaveTable: MaxGroundVolume + Cycle Acceleration)
└── Logistics/
    ├── ConveyorGroundPatch.cs      (GoldDigger.ConveyorGround: MaxDirt)
    ├── ConveyorElevatorPatch.cs    (GoldDigger.ConveyorElevator: BucketCapacity)
    ├── MobileConveyorPatch.cs      (GoldDigger.FrankensteinBelt: Frankenstein + Cordylus Capacity & Speed)
    ├── MagnetiteTrailerPatch.cs    (GoldDigger.MagnetiteTrailer: MaxMagnetiteTrailerVolume)
    └── FuelTrailerPatch.cs         (GoldDigger.FuelStationController: MaxCapacity + Refueling Rate)
```

## Architectural & Safety Principles
1. **Zero-Allocation Fast-Path**: Harmony patches evaluate multiplier states and exit immediately without allocations (`O(1)` fast-path) when values are unchanged.
2. **OriginalValueStore**: Base values are cached prior to first multiplication. Disabling the mod or resetting sliders cleanly restores the exact vanilla values.
3. **OrangeBeastFilter**: The Tier 5 *Orange Beast* wash plant is protected from sub-shaker patches to safeguard savegame counters.
4. **Water Neutrality**: `HogPanDirtBox.ProcessPlane` clamps water drainage to vanilla base rates, preventing mats from drying out prematurely under enlarged capacities.
