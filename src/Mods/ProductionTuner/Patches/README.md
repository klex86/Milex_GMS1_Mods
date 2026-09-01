# Patches – Production Tuner (Phase 2)

Dieser Ordner enthält alle aktiven Harmony-Patch-Klassen des `Milex GMS1 Production Tuner`.

## Struktur & Domänen

Die Patches sind strikt nach funktionalen Domänen organisiert:

```
Patches/
├── Tools/
│   ├── ShovelPatch.cs              (GoldDigger.Shovel: MaxVolume + Sqrt-Blades)
│   └── BucketPatch.cs              (GoldDigger.Bucket: MaxVolume)
├── WashPlants/
│   ├── HogPanDirtBoxPatch.cs       (GoldDigger.HogPanDirtBox: PlaneVolumeMax + Wasserschutz)
│   ├── MobileWashPlantPatch.cs     (GoldDigger.MobileWashplant + Mini: MaxFill + FillSpeed)
│   ├── WashPlantShakerPatch.cs     (GoldDigger.WashplantShakerBase: MaxFill + FillSpeed)
│   ├── SluiceBoxPatch.cs           (GoldDigger.WashPlantSluiceBoxDirt: MaxFill)
│   └── MinersMossPatch.cs          (GoldDigger.MinersMoss: MaxGroundVolume)
├── Vehicles/
│   ├── ExcavatorPatch.cs           (Koparka: Digging + BladesBoxCollider)
│   ├── WheelLoaderPatch.cs         (Ladowarka: Digging + _invmax + Hydraulik-Torque)
│   ├── BackhoeLoaderPatch.cs       (KoparkoLadowarka: Front- & Heck-DiggingController)
│   └── DumpTruckPatch.cs           (GoldDigger.DumpTruck: Digging)
├── Processing/
│   ├── MatScrubberPatch.cs         (GoldDigger.MatScrubber: CleanigDirtSpeed + *InBucket)
│   ├── MagnetiteSeparatorPatch.cs  (GoldDigger.MagnetiteSeparator: MaxFill + FillOutSpeed)
│   └── WaveTablePatch.cs           (GoldDigger.WaveTable: MaxGroundVolume + Zyklus-Timer)
└── Logistics/
    ├── ConveyorGroundPatch.cs      (GoldDigger.ConveyorGround: MaxDirt)
    ├── ConveyorElevatorPatch.cs    (GoldDigger.ConveyorElevator: BucketCapacity)
    ├── MagnetiteTrailerPatch.cs    (GoldDigger.MagnetiteTrailer: MaxMagnetiteTrailerVolume)
    └── FuelTrailerPatch.cs         (GoldDigger.FuelStationController: MaxCapacity + Betankungstempo)
```

## Sicherheits- & Entkopplungsprinzipien
1. **OriginalValueStore**: Vor jedem Multiplizieren wird der originale Vanilla-Basiswert gesichert. Bei Mod-Deaktivierung oder Slider-Reset wird exakt dieser Basiswert wiederhergestellt.
2. **FieldCache**: Alle Reflection-Lookups werden gecacht. Keine Reflection-Suchen in Update-Schleifen.
3. **OrangeBeastFilter**: Die Großwaschanlage Tier 5 *Orange Beast* wird von Standard-Rüttler-Patches ausgenommen, um Savegame-Zähler zu schützen.
4. **Wasserschutz**: In `HogPanDirtBox.ProcessPlane` wird der Wasserverbrauch auf dem Vanilla-Basiswert verankert, damit die Hog Pan bei höherer Kapazität nicht trockenläuft.
