# Milex GMS1 Production Tuner

- **Version:** `1.0.0` ([Changelog ansehen](CHANGELOG.md))
- **Mod-Name:** Milex GMS1 Production Tuner
- **Autor:** Milex
- **Dateiname:** `Milex_GMS1_ProductionTuner.dll`
- **Abhaengigkeit:** `Milex_GMS1_CoreMod.dll`

Der **Production Tuner** gibt dir vollstaendige Kontrolle ueber die Verarbeitungsgeschwindigkeiten,
Kapazitaeten und Hydraulikleistung aller Maschinen, Fahrzeuge und Werkzeuge in *Gold Rush: The Game*.
Alle Werte werden als Multiplikatoren auf die Original-Spielwerte angewendet – 1.0 entspricht
immer dem unveraeinderten Spiel, 2.0 bedeutet doppelte Leistung.

---

## Installation

1. **Voraussetzung**: BepInEx 5 (x64) und `Milex_GMS1_CoreMod.dll` muessen im Spielverzeichnis installiert sein.
2. **Installation**: Kopiere `Milex_GMS1_ProductionTuner.dll` in den Ordner `BepInEx/plugins/`.
3. **Start**: Starte das Spiel. Beim ersten Start legt der Mod automatisch seine Einstellungsdatei an.

---

## Bedienung im Spiel

1. Druecke **`Einfuegen`** (`Insert`), um das Mod-Menue zu oeffnen.
2. Wähle links in der Leiste den Eintrag **Production Tuner** aus.
3. Im Hauptbereich erscheinen die fuenf Gruppen-Regler.
4. Klicke auf **`[ Gruppe zuruecksetzen ]`**, um alle Werte einer Gruppe auf Standard (1.0) zu setzen.
5. Klicke auf **`[ Alle zuruecksetzen ]`**, um saemtliche Werte auf Standard zu setzen.
6. Schalte im Einstellungsbereich den **Erweiterten Modus** ein, um Einzelregler pro Komponente und Parameter zu sehen.

---

## Konfiguration

Die Einstellungsdatei wird beim ersten Start automatisch angelegt:
`BepInEx/config/Milex_GMS1_ProductionTuner.cfg`

### Sektion `[General]`

| Schalter | Standardwert | Bedeutung |
|---|---|---|
| `AdvancedMode` | `false` | Einfacher Modus: ein Regler pro Gruppe. Erweiterter Modus: Einzelregler pro Komponente. |
| `AutoScaleDependentInputs` | `true` | Skaliert Pfanne, Wave Table, Magnetitabscheider und Anhaenger automatisch auf mindestens den Eimer-Wert, um Materialverlust zu verhindern. |

### Multiplikator-Gruppen

Alle Multiplikatoren haben einen Bereich von **0.1** (10% Leistung) bis **10.0** (zehnfache Leistung).
Der Standardwert **1.0** entspricht dem unveraenderten Spiel.

| Sektion | Enthaltene Komponenten |
|---|---|
| `[Group1_HandTools]` | Schaufel, Eimer, Pfanne, Hog Pan, Mobile Waschanlage |
| `[Group2_Vehicles]` | Minibagger, Bagger, Radlader, Baggerlader, Mobiles Foerderband |
| `[Group3_WashPlantModules]` | Einfuelltrichter, Foerderband, Ruettler, Derocker, Waschrinne, Trommel, Jig, Miner's Moss |
| `[Group4_FineProcessing]` | Nuggetator, Magnetitabscheider, Wave Table |
| `[Group5_Trailers]` | Magnetitanhaenger, Kraftstoffanhaenger |

Im einfachen Modus (Standard) gibt es in jeder Sektion nur den Schalter `GroupMultiplier`.
Im erweiterten Modus werden alle Einzelregler sichtbar und koennen unabhaengig angepasst werden.

### Kaskadenschutz (`AutoScaleDependentInputs = true`)

Vergrossert du den Eimer auf zum Beispiel 3.0x, kann er mehr Material aufnehmen als Pfanne,
Wave Table oder Magnetitabscheider fassen koennen – Material wuerde verloren gehen.
Mit aktivem Kaskadenschutz werden diese Geraete automatisch auf mindestens 3.0x gesetzt,
egal was du dort manuell eingestellt hast.

---

## Sprachdateien

Der Mod unterstuetzt Mehrsprachigkeit ueber externe JSON-Dateien:

- **Speicherort:** `BepInEx/plugins/Milex GMS1 Mod Localization/`
- **Vorlagen:** `Milex_GMS1_ProductionTuner_en.json` und `Milex_GMS1_ProductionTuner_de.json`
  werden beim ersten Start automatisch entpackt.
- **Eigene Uebersetzung:** Eine Vorlage fuer eine fehlende Sprache kann im Ingame-Menue unter
  *Allgemein -> Sprache* generiert werden. Die Datei dann mit einem Texteditor uebersetzen und speichern.
