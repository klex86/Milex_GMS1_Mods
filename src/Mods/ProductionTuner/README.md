# Milex GMS1 Production Tuner

- **Version:** `1.1.0` ([Changelog ansehen](CHANGELOG.md))
- **Mod-Name:** Milex GMS1 Production Tuner
- **Autor:** Milex
- **Dateiname:** `Milex_GMS1_ProductionTuner.dll`
- **Abhaengigkeit:** `Milex_GMS1_CoreMod.dll`

Der **Production Tuner** gibt dir vollstaendige Kontrolle ueber die Verarbeitungsgeschwindigkeiten,
Kapazitaeten und das Ladetempo aller Maschinen, Fahrzeuge und Werkzeuge in *Gold Rush: The Game*.
Alle Werte werden als Multiplikatoren auf die Original-Basiswerte des Spiels angewendet. Jeder Wert besitzt
seinen eigenen, optimal abgestimmten Standard-Multiplikator und laesst sich in 0.5-Schritten (von 0.5 bis 10.0) feinstufig einstellen.

---

## Installation

1. **Voraussetzung**: BepInEx 5 (x64) und `Milex_GMS1_CoreMod.dll` muessen im Spielverzeichnis installiert sein.
2. **Installation**: Kopiere `Milex_GMS1_ProductionTuner.dll` in den Ordner `BepInEx/plugins/`.
3. **Start**: Starte das Spiel. Beim ersten Start legt der Mod automatisch seine Einstellungsdatei an.

---

## Bedienung im Spiel

1. Druecke **`Einfuegen`** (`Insert`), um das Mod-Menue zu oeffnen.
2. Waehle links in der Leiste den Eintrag **Production Tuner** aus.
3. Alle Komponenten lassen sich direkt und unabhaengig ueber ihre jeweiligen Schieberegler anpassen.
4. Hinter jedem Eingabefeld wird der werkseitige Standardwert des Mods angezeigt (z. B. `(Standard: 2.0)`).
5. Klicke auf **`[ Gruppe zuruecksetzen ]`**, um alle Werte einer Gruppe auf ihre mod-seitigen Standard-Multiplikatoren zurueckzusetzen.

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

- **Automatisches Hochskalieren**: Vergroesserst du den Eimer, werden die Folgebehaelter (Hog Pan, Magnetitabscheider, Wave Table und Magnetitanhaenger) in Echtzeit auf mindestens denselben Wert gesetzt.
- **Eimer-Obergrenze**: Der Eimer-Multiplikator wird dynamisch auf das zulaessige Maximum der abhaengigen Eingaenge begrenzt. Der Eimer kann somit niemals groesser eingestellt werden als das, was die Folgestationen maximal fassen koennen.

---

## Sprachdateien

Der Mod unterstuetzt vollstaendige Lokalisierung:

- **Speicherort:** `BepInEx/plugins/Milex GMS1 Mod Localization/`
- **Dateien:** `Milex_GMS1_ProductionTuner_en.json` und `Milex_GMS1_ProductionTuner_de.json`
- Neue Sprachvorlagen koennen jederzeit ueber das CoreMod-Menue generiert werden.
