# Patches – Phase 2

Dieser Ordner ist für die Harmony-Patch-Klassen des Production Tuner Mods reserviert.

## Was hier hingehört

Sobald die Spiel-DLLs (`Assembly-CSharp.dll`) mit einem Dekompiler (z. B. ILSpy oder dnSpy)
analysiert wurden, werden hier für jede der 40 Komponenten Harmony-Patch-Klassen angelegt.

Jede Patch-Klasse greift in die interne Methode oder das Feld der Spielklasse ein und
multipliziert den Originalwert mit dem berechneten Multiplikator aus dem `TuningService`.

## Muster einer Patch-Klasse (Beispiel – nach Dekompilierung ausfüllen)

```csharp
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Services;

namespace Milex.GMS1.Mods.ProductionTuner.Patches
{
    // TODO: Ersetze GameClass und MethodName nach Dekompilierung der Assembly-CSharp.dll
    [HarmonyPatch(typeof(GameClass), "MethodName")]
    internal static class GameClass_MethodName_Patch
    {
        // Beispiel: Postfix-Patch der einen Rückgabewert mit dem Multiplikator skaliert
        static void Postfix(ref float __result)
        {
            __result *= ProductionTunerPlugin.Service.GetShovelFillSpeed();
        }
    }
}
```

## Zuordnungstabelle – TODO: In Phase 2 ausfüllen

| Komponente                | TuningService-Methode              | Spielklasse (TODO)       | Methode/Feld (TODO)      |
|---------------------------|------------------------------------|--------------------------|--------------------------|
| Schaufel                  | GetShovelFillSpeed()               | ?                        | ?                        |
| Eimer                     | GetBucketCapacity()                | ?                        | ?                        |
| Pfanne                    | GetPanCapacity()                   | ?                        | ?                        |
| Hog Pan                   | GetHogPanCapacity()                | ?                        | ?                        |
| Mobile Waschanlage        | GetMobileWashPlantSpeed()          | ?                        | ?                        |
| Minibagger                | GetMiniBaggerDigSpeed()            | ?                        | ?                        |
| Bagger                    | GetBaggerDigSpeed()                | ?                        | ?                        |
| Radlader                  | GetRadladerLoadSpeed()             | ?                        | ?                        |
| Baggerlader               | GetBaggerladerLoadSpeed()          | ?                        | ?                        |
| Mobiles Foerderband       | GetMobileConveyorSpeed()           | ?                        | ?                        |
| Einfuelltrichter          | GetHopperCapacity()                | ?                        | ?                        |
| Foerderband (stationaer)  | GetConveyorSpeed()                 | ?                        | ?                        |
| Ruettler                  | GetVibratingScreenSpeed()          | ?                        | ?                        |
| Derocker                  | GetDerockerSpeed()                 | ?                        | ?                        |
| Waschrinne                | GetSluiceSpeed()                   | ?                        | ?                        |
| Trommelwaschanlage        | GetTrommelSpeed()                  | ?                        | ?                        |
| Jig                       | GetJigSpeed()                      | ?                        | ?                        |
| Miner's Moss              | GetMinersMossCapacity()            | ?                        | ?                        |
| Nuggetator                | GetNuggeterSpeed()                 | ?                        | ?                        |
| Magnetitabscheider        | GetMagnetiteSeparatorSpeed()       | ?                        | ?                        |
| Wave Table (Speed)        | GetWaveTableSpeed()                | ?                        | ?                        |
| Wave Table (Kapazitaet)   | GetWaveTableCapacity()             | ?                        | ?                        |
| Magnetitanhaenger         | GetMagnetiteTrailerCapacity()      | ?                        | ?                        |
| Kraftstoffanhaenger       | GetFuelTrailerCapacity()           | ?                        | ?                        |
