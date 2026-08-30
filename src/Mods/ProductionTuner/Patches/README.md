# Patches – Phase 2

Dieser Ordner ist für die Harmony-Patch-Klassen des Production Tuner Mods reserviert.

## Was hier hingehört

Sobald die Spiel-DLLs (`Assembly-CSharp.dll`) mit einem Dekompiler (z. B. ILSpy oder dnSpy)
analysiert wurden, werden hier für jede der Komponenten Harmony-Patch-Klassen angelegt.

Jede Patch-Klasse greift in die interne Methode oder das Feld der Spielklasse ein und
multipliziert den Originalwert mit dem berechneten Multiplikator aus dem `TuningService`.

## Muster einer Patch-Klasse (Beispiel – nach Dekompilierung ausfüllen)

```csharp
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Services;

namespace Milex.GMS1.Mods.ProductionTuner.Patches
{
    // TODO: Replace GameClass and MethodName after decompiling Assembly-CSharp.dll
    [HarmonyPatch(typeof(GameClass), "MethodName")]
    internal static class GameClass_MethodName_Patch
    {
        // Example: Postfix patch scaling return value with the multiplier
        static void Postfix(ref float __result)
        {
            __result *= ProductionTunerPlugin.Service.GetShovelFillSpeed();
        }
    }
}
```

## Zuordnungstabelle – TODO: In Phase 2 ausfüllen

| Komponente                         | TuningService-Methode               | Spielklasse (TODO)       | Methode/Feld (TODO)      |
|------------------------------------|-------------------------------------|--------------------------|--------------------------|
| Schaufel (Fill Speed)              | GetShovelFillSpeed()                | ?                        | ?                        |
| Eimer (Kapazitaet)                 | GetBucketCapacity()                 | ?                        | ?                        |
| Hog Pan (Kapazitaet)               | GetHogPanCapacity()                 | ?                        | ?                        |
| Mobile Waschanlage (Speed)         | GetMobileWashPlantSpeed()           | ?                        | ?                        |
| Mobile Waschanlage (Kapazitaet)    | GetMobileWashPlantCapacity()        | ?                        | ?                        |
| Bagger (alle: Mini & Gross)        | GetExcavatorDigSpeed()              | ?                        | ?                        |
| Radlader                           | GetWheelLoaderLoadSpeed()           | ?                        | ?                        |
| Baggerlader                        | GetBackhoeLoaderLoadSpeed()         | ?                        | ?                        |
| Muldenkipper (Dump Truck)          | GetDumpTruckCapacity()              | ?                        | ?                        |
| Einfuelltrichter                   | GetHopperCapacity()                 | ?                        | ?                        |
| Foerderband-Eimer                  | GetConveyorBucketCapacity()         | ?                        | ?                        |
| Waschanlagen (Kapazitaet)          | GetWashplantCapacity()              | ?                        | ?                        |
| Waschanlagen (Speed)               | GetWashplantSpeed()                 | ?                        | ?                        |
| Waschrinnen (Sluice Boxes)         | GetSluiceboxCapacity()              | ?                        | ?                        |
| Miner's Moss                       | GetMinersMossCapacity()             | ?                        | ?                        |
| Nuggetator                         | GetNuggetatorSpeed()                | ?                        | ?                        |
| Magnetitabscheider (Speed)         | GetMagnetiteSeparatorSpeed()        | ?                        | ?                        |
| Magnetitabscheider (Kapazitaet)    | GetMagnetiteSeparatorCapacity()     | ?                        | ?                        |
| Wave Table (Speed)                 | GetWaveTableSpeed()                 | ?                        | ?                        |
| Wave Table (Kapazitaet)            | GetWaveTableCapacity()              | ?                        | ?                        |
| Magnetitanhaenger                  | GetMagnetiteTrailerCapacity()       | ?                        | ?                        |
| Kraftstoffanhaenger                | GetFuelTrailerCapacity()            | ?                        | ?                        |
