# Milex GMS1 Production Tuner

- **Version:** `1.1.0` ([Changelog ansehen](CHANGELOG.md))
- **Mod-Name:** Milex GMS1 Production Tuner
- **Autor:** Milex
- **Dateiname:** `Milex_GMS1_ProductionTuner.dll`
- **Abhaengigkeit:** `Milex_GMS1_CoreMod.dll`

Der **Production Tuner** gibt dir vollstaendige Kontrolle ueber die Verarbeitungsgeschwindigkeiten, Kapazitaeten und das Ladetempo aller Maschinen, Fahrzeuge und Werkzeuge in *Gold Rush: The Game*. 

Alle 22 einstellbaren Parameter werden als direkte Multiplikatoren auf die Original-Basiswerte des Spiels angewendet. Jeder Wert besitzt seinen eigenen, optimal abgestimmten Standard-Multiplikator und laesst sich in 0.5-Schritten (von 0.5 bis 10.0 bzw. bis 20.0) feinstufig einstellen.

---

## Dokumentation & Navigation

| Dokument | Link |
|---|---|
| **Haupt-Dokumentation (Repository)** | [README.md](../../../README.md) |
| **Haupt-Changelog** | [CHANGELOG.md](../../../CHANGELOG.md) |
| **CoreMod Dokumentation** | [CoreMod README.md](../../CoreMod/README.md) |
| **Production Tuner Changelog** | [ProductionTuner CHANGELOG.md](CHANGELOG.md) |
| **Community Knowledge Compendium** | [COMMUNITY_KNOWLEDGE_COMPENDIUM.md](../../../Community%20Knowledge/COMMUNITY_KNOWLEDGE_COMPENDIUM.md) |

---

## Installation & Erste Schritte

1. **Voraussetzung**: BepInEx 5 (x64) und `Milex_GMS1_CoreMod.dll` muessen im Spielverzeichnis installiert sein.
2. **Installation**: Kopiere `Milex_GMS1_ProductionTuner.dll` in den Ordner `BepInEx/plugins/`.
3. **Start**: Starte das Spiel. Beim ersten Start legt der Mod automatisch seine Konfigurations- und Sprachdateien an.

---

## Bedienung im Spiel

1. Druecke **`Einfuegen`** (`Insert`), um das Mod-Menue zu oeffnen.
2. Waehle links in der Leiste den Eintrag **Production Tuner** aus.
3. Alle 22 Komponenten lassen sich direkt und unabhaengig ueber ihre jeweiligen Schieberegler anpassen.
4. Hinter jedem Eingabefeld wird der werkseitige Standardwert des Mods angezeigt (z. B. `(Standard: 2.0)`).
5. Klicke auf **`[ Gruppe zuruecksetzen ]`**, um alle Werte einer Gruppe auf ihre mod-seitigen Standard-Multiplikatoren zurueckzusetzen.
6. **Live-Umschaltung**: Wird der Mod ueber die Checkbox `[x] Aktiv` im Menue deaktiviert, werden alle Maschinen sofort auf ihre originalen Vanilla-Werte zurueckgesetzt, ohne dass ein Neustart erforderlich ist.

---

## Konfiguration

Die Einstellungsdatei wird beim ersten Start automatisch angelegt:  
`BepInEx/config/Milex_GMS1_ProductionTuner.cfg`

### Sektion `[General]`

| Schalter | Standardwert | Bedeutung |
|---|---|---|
| `AutoScaleDependentInputs` | `true` | Kaskadenschutz: Skaliert Hog Pan, Magnetitabscheider, Wave Table und Magnetitanhaenger automatisch auf mindestens den Eimer-Wert, um Materialverlust zu verhindern. |

### Multiplikator-Gruppen

Alle Regler arbeiten in festen **0.5-Schritten**. Standard-Komponenten reichen von **0.5 bis 10.0**. Abhaengige Behaelterkapazitaeten (Hog Pan, Magnetitabscheider, Wave Table, Magnetitanhaenger) bieten einen erweiterten Bereich von **0.5 bis 20.0**, um auch bei grossem Eimer ausreichend Puffer fuer mehrfaches Entleeren zu gewaehrleisten.

| Sektion | Enthaltene Komponenten & Default-Multiplikatoren |
|---|---|
| `[Group1_HandTools]` | Schaufel (2.0x), Eimer (2.0x), Hog Pan (2.0x), Mobile Waschanlage Speed (3.0x), Mobile Waschanlage Kapazitaet (2.0x) |
| `[Group2_Vehicles]` | Bagger alle (3.0x), Radlader (3.0x), Baggerlader (3.0x), Muldenkipper Dump Truck (3.0x) |
| `[Group3_WashPlantModules]` | Einfuelltrichter (2.0x), Foerderband-Eimer (2.0x), Waschanlagen Kapazitaet (2.0x), Waschanlagen Speed (2.0x), Waschrinnen (2.0x), Miner's Moss (2.0x) |
| `[Group4_FineProcessing]` | Nuggetator Speed (2.0x), Magnetitabscheider Speed (2.0x), Magnetitabscheider Kapazitaet (2.0x), Wave Table Speed (3.0x), Wave Table Kapazitaet (3.0x) |
| `[Group5_Trailers]` | Magnetitanhaenger (2.0x), Kraftstoffanhaenger (3.0x) |

### Kaskadenschutz & Eimer-Begrenzung (`AutoScaleDependentInputs = true`)

- **Automatisches Anpassen**: Vergroesserst du den Eimer, werden die Folgebehaelter (Hog Pan, Magnetitabscheider, Wave Table und Magnetitanhaenger) in Echtzeit auf mindestens denselben Wert synchronisiert.
- **Eimer-Obergrenze**: Der Eimer-Multiplikator wird dynamisch auf das zulaessige Maximum der abhaengigen Eingaenge begrenzt. Der Eimer kann somit niemals groesser eingestellt werden als das, was die Folgestationen maximal fassen koennen.

### Ressourcen-Neutralitaet (Wasser, Sprit, Strom)

- **Hog Pan Wasserschutz**: Die Abflussrate des Wassers in der Hog Pan ist an den originalen Basiswert gekoppelt. Selbst bei 4-facher oder 10-facher Dreckkapazitaet leert sich das Wasser nicht schneller als im normalen Spiel.
- **Pumpen- & Generatorschutz**: Waschanlagen und elektrische Verbraucher fordern keine unzulaessig hoehere Nennleistung an. Bestehende Wasserpumpen und Generatoren laufen ohne Druckabfall oder Sicherungs-Blackouts weiter.
- **Hydraulik-Kraft**: Bei vergroessertem Radlader-Schaufelvolumen wird das Drehmoment der Hubzylinder (`AnimatedJoint.MaxTorque`) automatisch verstaerkt, damit die Maschine die volle Ladung muehelos heben kann.

---

## Sprachdateien

Der Mod unterstuetzt vollstaendige Lokalisierung ueber CoreMod:

- **Speicherort:** `BepInEx/plugins/Milex GMS1 Mod Localization/`
- **Dateien:** `Milex_GMS1_ProductionTuner_en.json` und `Milex_GMS1_ProductionTuner_de.json`
- Neue Sprachvorlagen koennen jederzeit ueber das CoreMod-Menue generiert werden.

---

## Danksagung & Credits

Ein grosser Teil der Reverse-Engineering-Erkenntnisse und Klassen-Zuordnungen basiert auf der wertvollen Pionierarbeit der Gold-Rush-Modding-Community. Besonderer Dank und Credits gebuehren folgenden Moddern und ihren Projekten:

- **stregkoden**:
  - *Better_Conveyor*: Wegweisende Zuordnungen fuer `ConveyorGround.MaxDirt` und `ConveyorElevator.BucketCapacity`.
  - *Better_FEL*: Entdeckung des hydraulischen Drehmoment-Bedarfs (`AnimatedJoint.MaxTorque`) und Lenkungs-Boosts beim Radlader (`Ladowarka`).
  - *Better_FuelTrailer*: Identifikation des Kraftstoffanhaengers ueber `FuelStationController` und proportionale Betankungsraten.
  - *Better_Nuggetator*: Praezise Analyse der `CleanigDirtSpeed`-Durchsatzrate.
  - *HogPan_Pack*: Aufdeckung der `MobileWashplant`- und `MiniWashplant`-Klassen sowie des Wasserhaushalts der Hog Pan.
- **DeepCore / Jonathan**:
  - *Bigger Shovel*: Mathematisch brillante $\sqrt{M}$-Skalierung der Schaufelraender (`_bladeSizex`, `_bladeSizez`) und saubere Speicherung von Originalwerten.
  - *Smart Buckets*: Tiefenanalyse der `Bucket`-Interna (`UpdatePlaneAndMass`, `MudVolume`, Material-Tints).
- **FedeRama**:
  - *GMS.WaveTableCapacity*: Zuordnung von `WaveTable.MaxGroundVolume`.
- **GMS Community Modders**:
  - *Increased Capacity And Speed* & *IncreasedCapacity*: Umfangreiche Vorarbeiten fuer `MagnetiteSeparator`, `DumpTruck`, `OrangeBeastFilter` und Kehrwert-Erhaltung (`_invmaxShovelVolume`).

---

## Lizenz & Freie Nutzung (Open Source / Public Domain)

Der gesamte Code des **Milex GMS1 Production Tuner** steht unter einer extrem liberalen Open-Source-Lizenz (nach Vorbild der MIT- / Unlicense-Bedingungen):

> **Recht auf freie Nutzung:**  
> Jeder hat das uneingeschraenkte Recht, diesen Code ganz oder in Teilen kostenlos zu verwenden, zu kopieren, zu modifizieren, zusammenzufuehren, zu veroeffentlichen, zu verbreiten oder in eigene Mods und Projekte einzubinden. Es gibt keinerlei Beschraenkungen oder Verpflichtungen. Wir freuen uns lediglich ueber eine kurze Nennung in den Credits deines Projekts.
