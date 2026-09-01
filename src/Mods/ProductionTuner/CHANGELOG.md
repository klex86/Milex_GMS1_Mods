# Changelog – Milex GMS1 Production Tuner

Alle wichtigen Aenderungen an diesem Mod sind hier aufgelistet.
Das Format folgt den Grundsaetzen von [Keep a Changelog](https://keepachangelog.com/).

---

## [1.3.0] – 2026-09-02

### Neue Funktionen: Mobile Förderbänder & Bagger-Hydraulik

- **Mobile Förderbänder Frankenstein & Cordylus**:
  - Im Menü direkt hinter dem Muldenkipper als eigene Maschinen integriert.
  - Jeweils getrennte Regler für **Pufferkapazität** (`MaxVolume`, Default: 2.0x) und **Transportgeschwindigkeit** (`Speed`, Default: 2.0x).
  - Automatische Unterscheidung der beiden Förderbänder über die Fahrzeug-Hierarchie (`FrankensteinExcavator` vs. `MaximusMachineController`).
- **Fein abgestufte Bagger-Hydraulik-Geschwindigkeit**:
  - 3 neue Schieberegler zur gezielten Steuerung der Bagger-Manövrierfähigkeit (`Koparka`):
    - **Ausleger-Geschwindigkeit** (Ausleger- und Löffelstielzylinder, Default: 2.0x)
    - **Turmdreh-Geschwindigkeit** (Oberwagen- und Kabinendrehung, Default: 2.0x)
    - **Schaufel-Kippgeschwindigkeit** (Kipp- und Schöpfzylinder der Baggerschaufel, Default: 1.0x)
  - Automatische Anpassung von `Rigidbody.maxAngularVelocity`, damit die Drehgeschwindigkeit physikalisch nicht künstlich abgeriegelt wird.
- **High-Performance Fast-Path**: Alle neuen Regler nutzen die Zero-Allocation-Architektur mit sofortigem `O(1)`-Ausstieg bei unveränderten Werten.

---

## [1.2.2] – 2026-09-01

### Performance-Overhaul (Beseitigung des 40 % FPS-Drops)

- **Zero-Allocation Fast-Path in allen 18 Patches**:
  - Sämtliche Harmony-Patches prüfen nun im ersten Takt, ob der Reglerwert unverändert ist. Wenn ja, wird die Methode sofort ohne jegliche Berechnung beendet (`O(1)` Fast Exit).
  - Vermeidet tausende Boxing-Allokationen (`FieldInfo.GetValue` für Floats) pro Sekunde, die zuvor die Unity-Garbage-Collection überlastet und zu Rucklern geführt haben.
- **Entfernung von Frame-weiser Reflection & Thread-Locks**:
  - Alle Patches greifen nun direkt typisiert auf die öffentlichen Felder der Spielklassen zu (`Direct Public Field Access`), anstatt über Reflection-Lookups zu gehen.
  - Sämtliche Thread-Locks (`lock (SyncRoot)`) in `OriginalValueStore` und `OrangeBeastFilter` wurden entfernt, da Unity-Update-Schleifen strikt single-threaded laufen.
- **Radlader-Hydraulik-Optimierung**:
  - `GetComponentsInChildren<AnimatedJoint>()` wird nun nur noch ein einziges Mal beim Auftauchen des Radladers gecacht und läuft nicht mehr in jedem Frame der `Update()`-Schleife.
- **Ergebnis**: 100 % butterweiche 60 / 144 FPS ohne Frame-Einbrüche.

---

## [1.2.1] – 2026-09-01

### Fehlerbehebungen & Feinschliff aus In-Game-Tests

- **Bagger (Excavator)**: Die überdimensionierte Kollisionsbox wurde vollständig entfernt. Der Bagger gräbt nun wieder exakt an der Position der Schaufel, fasst aber das volle, vergrößerte Schaufelvolumen.
- **Handschaufel (Shovel)**: Die Schaufel-Logik wurde auf die `Update`-Schleife umgestellt. Bereits im Inventar vorhandene Schaufeln sowie Live-Änderungen an den Reglern werden nun sofort aktiv. Dank $\sqrt{M}$-Flächenskalierung füllt sich die Schaufel in einem einzigen Einstich.
- **Muldenkipper & Radlader**: Saubere Typ-Trennung zwischen Muldenkipper (`DumpTruck`) und Radlader (`Ladowarka`). Beide Fahrzeuge überschreiben sich nun nicht mehr gegenseitig, und das Muldenkipper-Volumen skaliert wie gewünscht.
- **Kraftstoffanhänger (Fuel Trailer)**: Von `Start()` auf die `Update()`-Schleife umgestellt, damit auch bereits gekaufte Anhänger auf bestehenden Spielständen und Regleränderungen sofort aktiv werden.
- **UI-Slider (Verschwindende Texturen behoben)**: Prozedurale Texturen werden nun vor der Garbage Collection bei Szenenwechseln geschützt (`HideFlags.HideAndDontSave`) und bei Bedarf automatisch regeneriert.

---

## [1.2.0] – 2026-09-01

### Phase 2: Vollstaendige Spiel-Integration (Harmony-Patches)

- **Alle 22 Komponenten vollstaendig implementiert**:
  - **Tools**: Handschaufel (`GoldDigger.Shovel`) mit $\sqrt{M}$-Skalierung der Schaufelraender, Eimer (`GoldDigger.Bucket`).
  - **Waschanlagen**: Hog Pan (`GoldDigger.HogPanDirtBox`), Mobile & Mini Waschanlage (`GoldDigger.MobileWashplant`, `MiniWashplant`), Grosswaschanlagen-Rüttler (`GoldDigger.WashplantShakerBase`), Waschrinnen (`GoldDigger.WashPlantSluiceBoxDirt`), Miner's Moss (`GoldDigger.MinersMoss`).
  - **Fahrzeuge**: Bagger alle (`Koparka`), Radlader (`Ladowarka`) mit Kehrwert-Erhaltung und Drehmoment-Anpassung, Baggerlader (`KoparkoLadowarka`) fuer Front- und Heckarm, Muldenkipper (`GoldDigger.DumpTruck`).
  - **Feinverarbeitung**: Nuggetator (`GoldDigger.MatScrubber`), Magnetitabscheider (`GoldDigger.MagnetiteSeparator`), Wave Table (`GoldDigger.WaveTable`) mit timer-basierter Zyklusbeschleunigung.
  - **Logistik & Anhaenger**: Einfuelltrichter (`GoldDigger.ConveyorGround`), Schraegfoerderer-Eimer (`GoldDigger.ConveyorElevator`), Magnetitanhaenger (`GoldDigger.MagnetiteTrailer`), Kraftstoffanhaenger (`GoldDigger.FuelStationController`) mit proportionaler Betankungsrate.
- **Ressourcen-Neutralitaet & Infrastruktur-Schutz**:
  - **Wasserschutz der Hog Pan**: In `ProcessPlane` wird der Wasserabfluss an den Vanilla-Basiswert gekoppelt. Kein vorzeitiges Trockenlaufen mehr bei 2x–10x Dreckkapazitaet.
  - **Stromnetz & Wasserpumpen**: Nennleistungsaufnahmen bleiben unveraendert. Kein Ausloesen der Generatorsicherung oder Druckabfall an den Wasserpumpen.
  - **Hydraulik-Kraft**: Bei schwereren Schaufelladungen wird das Drehmoment der Hebezylinder (`AnimatedJoint.MaxTorque`) automatisch angehoben.
- **OriginalValueStore & Drift-Schutz**:
  - Alle originalen Vanilla-Werte werden vor dem ersten Multiplizieren instanzgenau erfasst.
  - Beim Deaktivieren des Mods oder beim Zuruecksetzen von Slidern werden die exakten Originalwerte im laufenden Spiel restauriert.
- **Performance & Stabilitaet (`FieldCache` & `OrangeBeastFilter`)**:
  - Gecachte Reflection-Lookups verhindern Framerate-Einbrueche in Update-Schleifen.
  - Die Grosswaschanlage Tier 5 *Orange Beast* wird gezielt vor unpassenden Sub-Shaker-Patches geschuetzt.
- **Community-Credits & Open-Source-Lizenz**:
  - Danksagungen an alle 10 Community-Modder (stregkoden, DeepCore/Jonathan, FedeRama, GMS Community) in der README hinterlegt.
  - Vollstaendig freie Lizenz (Public Domain / MIT-Stil) fuer jedermann erteilt.

---

## [1.1.0] – 2026-08-30

### Neue Funktionen & Überarbeitungen

- **Spezifische Default-Multiplikatoren pro Komponente**: Jeder Wert startet nun mit seinem eigenen, optimal abgestimmten Standard-Multiplikator (z. B. Bagger 3.0x, Muldenkipper 3.0x, Schaufel 2.0x, Waschanlagen 2.0x) anstelle eines pauschalen 1.0-Werts.
- **Bereinigung und Ergänzung der Komponenten**:
  - **Gruppe 1 (Handwerkzeuge)**: `Pan_Capacity` entfernt. `MobileWashPlant_Capacity` (2.0x) als neuer Regler für das Fassungsvermögen der mobilen Waschanlage hinzugefügt.
  - **Gruppe 2 (Fahrzeuge)**: `MiniExcavator_DigSpeed` entfernt (alle Bagger werden nun einheitlich über `Excavator_DigSpeed` geregelt). `MobileConveyor_Speed` entfernt. `DumpTruck_Capacity` (3.0x) neu hinzugefügt.
  - **Gruppe 3 (Waschanlagen-Module)**: `Conveyor_Speed` in `ConveyorBucket_Capacity` (2.0x) umbenannt. Nicht benötigte Einzelschalter (`VibratingScreen_Speed`, `Derocker_Speed`, `Sluice_Speed`, `Trommel_Speed`, `Jig_Speed`) entfernt. Neue zentrale Regler: `Washplant_Capacity` (2.0x), `Washplant_Speed` (2.0x) und `Sluicebox_Capacity` (2.0x) hinzugefügt.
  - **Gruppe 4 (Feinverarbeitung)**: `MagnetiteSeparator_Capacity` (2.0x) als neuer Regler für die Eingangskapazität des Magnetitabscheiders hinzugefügt.
- **Präzisierter Kaskadenschutz**: Der Kaskadenschutz sichert nun exakt die relevanten Behälter ab (`HogPan_Capacity`, `MagnetiteSeparator_Capacity`, `WaveTable_Capacity`, `MagnetiteTrailer_Capacity`) und verhindert Materialverlust bei vergrößertem Eimer.
- **Entfall der Gruppen-Multiplikatoren & Modi**: Sämtliche Gruppen-Multiplikatoren sowie der einfache/erweiterte Modus wurden entfernt. Alle Komponenten lassen sich nun direkt und übersichtlich als Einzelregler anpassen.
- **Dynamische Eimer-Obergrenze & Erweiterter Wertebereich für Folgegeräte**:
  - Folgebehälter (Hog Pan, Magnetitabscheider, Wave Table, Magnetitanhänger) unterstützen nun Multiplikatoren bis zu 20.0x, um bei vergrößertem Eimer ausreichend Puffer für mehrfaches Entleeren zu bieten.
  - Der Eimer-Multiplikator wird automatisch auf das maximal zulässige Fassungsvermögen der Folgebehälter begrenzt – ein Überschreiten des Maximums der Folgestationen ist ausgeschlossen.

---

## [1.0.1] – 2026-08-30

### Fehlerbehebungen & Verbesserungen

- **Anzeigenamen im Menue funktionieren jetzt korrekt**: Alle Einstellungen zeigen nun ihren uebersetzten Namen statt des internen Dateinamens an. Ursache war eine falsche Benennung der Config-Keys (die Keys waren auf Deutsch statt auf Englisch).
- **Einfacher / Erweiterter Modus jetzt pro Gruppe**: Jede der fuenf Gruppen hat einen eigenen Schalter. So kann man z. B. Gruppe 1 auf einfach lassen und Gruppe 3 auf erweitert schalten, ohne dass alle anderen Gruppen beeinflusst werden. Der globale Modus-Schalter wurde entfernt.
- **Slider in 0.5-Schritten**: Alle Multiplikator-Regler rasten jetzt in Schritten von 0.5 ein (0.5, 1.0, 1.5, … 10.0). Stufenlose Werte zwischen den Schritten sind nicht mehr moeglich.
- **Gruppe 5 (Anhaenger) hat keinen Gruppen-Multiplikator mehr**: Magnetitanhaenger und Kraftstoffanhaenger haben nichts miteinander zu tun und werden jetzt getrennt und unabhaengig voneinander eingestellt.
- **Kaskadenschutz live in der Benutzeroberfläche**: Wird das Eimervolumen (oder der Gruppenregler von Gruppe 1) erhöht, springen alle abhängigen Geräte (Goldwaschpfanne, Rütteltisch, Magnetitabscheider, Anhänger) in Echtzeit auf mindestens denselben Wert. Ein manuelles Unterschreiten des Eimerwerts wird bei aktivem Kaskadenschutz automatisch verhindert.
- **Einzelregler im einfachen Modus gesperrt**: Wenn der einfache Modus einer Gruppe aktiv ist, sind die Einzelregler dieser Gruppe ausgegraut und koennen nicht veraendert werden. Gleichzeitig spiegeln sie den Gruppenwert wider.

---

## [1.0.0] – 2026-08-30

### Neu

- **Fuenf Regler-Gruppen**: Handwerkzeuge & Mobile Waschanlagen, Baufahrzeuge & Mobiles Foerderband,
  Waschanlagen-Module (Tier 3–6), Feinverarbeitung sowie Anhaenger.
- **Einfacher Modus** (Standard): Ein gemeinsamer Multiplikator steuert alle Komponenten einer Gruppe gleichzeitig.
- **Erweiterter Modus**: Einzelregler pro Komponente und Parameter (Kapazitaet, Geschwindigkeit, Hydraulik)
  koennen unabhaengig voneinander angepasst werden.
- **Gruppen-Reset-Button**: Setzt alle Werte einer Gruppe auf den Standardwert (1.0) zurueck.
- **Gesamt-Reset-Button**: Setzt alle Werte aller Gruppen zurueck.
- **Kaskadenschutz** (`AutoScaleDependentInputs`, Standard: an): Verhindert Materialverlust,
  wenn ein grosser Eimer in ein kleineres Folgegeraet geleert wird. Pfanne, Wave Table,
  Magnetitabscheider und Anhaenger werden automatisch mindestens so gross wie der Eimer skaliert.
- **Volles Mehrsprachigkeits-System**: Deutsch und Englisch sind eingebaut. Weitere Sprachen
  koennen als Vorlage im Ingame-Menue erstellt und uebersetzt werden.
- **Ingame-Anzeige**: Der Mod erscheint mit eigenem Eintrag in der Sidebar des Milex Mod-Menues.
  Kann im laufenden Spiel ohne Neustart aktiviert und deaktiviert werden.
- **Phase 1 Architektur**: Vollstaendige Konfiguration, Domainlogik und Lokalisierung sind implementiert.
  Harmony-Patches fuer die direkten Spielklassen folgen in Phase 2 (nach Dekompilierung der Spiel-DLLs).
