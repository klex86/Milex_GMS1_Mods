# Changelog - Milex GMS1 Mods

Alle relevanten Änderungen und Neuerungen der Mod-Sammlung werden in dieser Datei verständlich dokumentiert.

---

## [1.6.0] - 2026-09-02

### Neue Funktionen & Regler-Erweiterungen (Production Tuner)
- **Mobile Förderbänder Frankenstein & Cordylus**:
  - Im Ingame-Menü direkt nach dem Muldenkipper mit separaten Reglern für Pufferkapazität (Default: 2.0x) und Transportgeschwindigkeit (Default: 2.0x) ausgestattet.
  - Vollautomatische Identifikation des Fahrzeugs über die Komponenten-Hierarchie.
- **Getrennte Bagger-Hydraulik-Regelung**:
  - 3 neue Einzelregler für Bagger (`Koparka`): Ausleger-Geschwindigkeit (2.0x), Turmdrehung (2.0x) und Schaufel-Kippgeschwindigkeit (1.0x).
  - Volle Bewegungsfreiheit durch dynamische Anpassung von `Rigidbody.maxAngularVelocity`.

---

## [1.5.2] - 2026-09-01

### Performance-Optimierung (Beseitigung des FPS-Drops)
- **Zero-Allocation Fast-Path**: Alle 18 Harmony-Patches prüfen nun im ersten Takt, ob der Reglerwert unverändert ist, und beenden sich sofort ohne Allokationen (`O(1)` Fast Exit).
- **Direkter Feldzugriff ohne Boxing**: Ersetzt Reflection-Lookups durch typisierten Direktzugriff. Keine tausenden Heap-Allokationen pro Sekunde mehr für die Unity-Garbage-Collection.
- **Entfernung unnötiger Thread-Locks**: Unitys Update-Schleife läuft strikt single-threaded; sperrende Locks in Hilfsklassen wurden eliminiert.
- **Radlader-Gelenk-Caching**: Hydraulikzylinder werden nur noch einmalig gecacht statt in jedem Frame neu per Hierarchie-Suche durchlaufen.

---

## [1.5.1] - 2026-09-01

### Fehlerbehebungen & Feinschliff aus In-Game-Tests
- **Bagger-Kollisionsbox bereinigt**: Die vergrößerte Grabbox wurde entfernt. Der Bagger gräbt nun millimetergenau an der Schaufel, fasst aber das volle, vergrößerte Schaufelvolumen.
- **Handschaufel-Sofortwirkung**: Reagiert nun direkt im laufenden Spiel auf Slider-Änderungen und füllt sich mit einem einzigen Einstich proportional zur neuen Kapazität.
- **Muldenkipper & Radlader entkoppelt**: Gegenseitiges Überschreiben der Schaufel-/Muldenvolumina behoben.
- **Kraftstoffanhänger sofort aktiv**: Von `Start()` auf `Update()` umgestellt, damit auch bereits vorhandene Anhänger die neue Kapazität sofort erhalten.
- **UI-Slider-Darstellung dauerhaft fixiert**: Texturen von Schiebereglern gegen Entladen bei Szenenwechseln gesichert.

---

## [1.5.0] - 2026-09-01

### Meilenstein: Production Tuner Phase 2 (Vollstaendige Spielintegration)
- **Vollstaendige Umsetzung aller 22 Komponenten-Patches**:
  - Sämtliche Schieberegler des Production Tuners (Schaufeln, Eimer, Bagger, Radlader, Baggerlader, Kipper, Förderbänder, Waschanlagen, Rüttler, Rinnen, Miner's Moss, Nuggetator, Magnetitabscheider, Wave Table, Anhänger) sind nun über dedizierte Harmony-Patches fest im Spiel verankert.
- **Ressourcen-Neutralitaet & Infrastruktur-Schutz**:
  - Der Wasserabfluss der Hog Pan wurde an den Vanilla-Basiswert gekoppelt, sodass sie selbst bei maximal vergrößerter Kapazität niemals vorzeitig trockenläuft.
  - Generatoren und Wasserpumpen werden nicht durch erhöhten Durchsatz überlastet; alle Strom- und Wassernetze bleiben vollkommen stabil.
  - Das Hubdrehmoment von Radladern wird proportional zur Schaufellast verstärkt, um schwere Lasten problemlos zu heben.
- **OriginalValueStore & Drift-Schutz**:
  - Alle Original-Basiswerte werden instanzgenau gepuffert. Beim Deaktivieren des Mods oder beim Bewegen von Slidern werden die Werte im laufenden Spiel sauber wiederhergestellt.
- **Community-Credits & Freie Lizenz**:
  - Ausführliche Danksagungen an die Modding-Community und freie Open-Source-Lizenz in die Dokumentation aufgenommen.

---

## [1.4.0] - 2026-08-30

### Neuerungen & Optimierungen in Production Tuner
- **Spezifische Default-Multiplikatoren**: Jeder Regler startet nun mit seinem eigenen, sinnvollen Standardmultiplikator (z. B. Bagger 3.0x, Muldenkipper 3.0x, Schaufel 2.0x, Waschanlagen 2.0x).
- **Überarbeitung der Komponenten & Schalter**:
  - Handwerkzeuge: Goldwaschpfanne entfernt, Kapazitätsregler für mobile Waschanlage ergänzt.
  - Fahrzeuge: Minibagger und Bagger zu einem gemeinsamen Bagger-Regler zusammengefasst, Muldenkipper (Dump Truck) neu aufgenommen, mobiles Förderband entfernt.
  - Waschanlagen-Module: Zentralisierte Regler für Waschanlagen-Kapazität, Verarbeitungsgeschwindigkeit und Waschrinnen; überflüssige Einzelschalter bereinigt.
  - Feinverarbeitung: Eingangskapazität für den Magnetitabscheider neu implementiert und in den Kaskadenschutz integriert.
- **Direkte Einzelregler ohne Gruppen-Multiplikatoren**: Die Gruppen-Multiplikatoren und Modi wurden entfernt; alle Einstellungen lassen sich direkt und übersichtlich pro Komponente anpassen.
- **Dynamische Eimer-Deckelung & bis zu 20x Kapazität**: Der Eimer-Multiplikator wird dynamisch auf das Maximum der Folgebehälter gedeckelt. Folgebehälter unterstützen nun bis zu 20.0x Puffer.

---

## [1.3.1] - 2026-08-30

### Verbesserungen & Fehlerbehebungen
- **Perfekt zentrierte Schieberegler**:
  - Der goldene Schiebegriff ist nun vertikal exakt symmetrisch auf der Führungsrinne des Reglers ausgerichtet.
- **Schutz vor Zeilenumbrüchen**:
  - Gruppen-Überschriften und Beschriftungen brechen nicht mehr ungewollt in mehrere Zeilen um.
- **Dynamische Sidebar-Breite**:
  - Die Leiste für geladene Mods berechnet ihre Breite nun automatisch dynamisch anhand der Mod-Namen, damit lange Namen nie abgeschnitten werden.
- **Standardwert-Anzeige**:
  - Hinter jedem Eingabefeld wird der werkseitige Standardwert des Mods angezeigt (z. B. `(Standard: 1.0)`).
- **Einfacher Modus als Standard**:
  - Beim Zurücksetzen einer Einstellungs-Gruppe wird immer der einfache Modus als Standard wiederhergestellt.
- **Statusleisten-Zähler für aktive Mods**:
  - Der Zähler aktiver Mods in der Fußzeile aktualisiert sich jetzt sofort live beim Aktivieren oder Deaktivieren eines Mods (Anzeige: `Aktive Mods: X / Y`).

---

## [1.3.0] - 2026-08-30

### Neue Mods

- **Production Tuner** – Neuer Mod zum Anpassen von Verarbeitungsgeschwindigkeiten, Kapazitaeten
  und Hydraulikleistung fuer alle 40 Maschinen, Fahrzeuge und Werkzeuge im Spiel.
  - Einfacher Modus: ein Regler pro Gruppe (5 Gruppen gesamt).
  - Erweiterter Modus: Einzelregler pro Komponente und Parameter.
  - Kaskadenschutz: Folgegeraete (Pfanne, Wave Table, Magnetitabscheider, Anhaenger) werden
    automatisch skaliert, um Materialverlust bei grossen Eimern zu verhindern.
  - Gruppen- und Gesamt-Reset-Buttons fuer schnelles Zuruecksetzen auf Standardwerte.
  - Vollstaendige Deutsch- und Englisch-Lokalisierung inklusive.

---

## [1.2.0] - 2026-08-30

### Neue Funktionen & Erweiterungen
- **Mods im laufenden Spiel an- und ausschalten**:
  - Jeder installierte Mod kann jetzt direkt im Ingame-Menü per Klick aktiviert oder deaktiviert werden.
  - Wenn ein Mod deaktiviert wird, werden alle seine Anpassungen und Spiellogiken sofort im Hintergrund gestoppt – ein Neustart des Spiels ist nicht erforderlich.
  - Der Zustand (aktiv/inaktiv) wird automatisch gespeichert und beim nächsten Spielstart wiederhergestellt.
  - Das Core-Framework selbst bleibt stets aktiv, um die Stabilität des Spiels zu gewährleisten.
- **Entwickler-Option für Übersetzungen**:
  - Neue Option *"Externe Sprachdateien ignorieren"* in den allgemeinen Einstellungen.
  - Nützlich für Mod-Entwickler: Übersetzungen werden direkt aus den Mod-Dateien gelesen, sodass lokale Sprachdateien für Tests nicht manuell gelöscht werden müssen.

### Verbesserungen & Fehlerbehebungen
- **Saubere Menü-Darstellung**:
  - Alle Symbole und Textdarstellungen wurden überarbeitet, sodass Menüs und Beschreibungen auf jedem Monitor sauber und gut lesbar angezeigt werden.
- **Verlässliche Spiel-Pause im Menü**:
  - Das Spiel hält beim Öffnen des Mod-Menüs verlässlich an und läuft beim Schließen wieder normal weiter – selbst wenn die Pause-Einstellung bei geöffnetem Menü umgestellt wird.
- **Kamera- & Steuerungssperre**:
  - Während das Mod-Menü geöffnet ist, bleibt die Spielkamera fixiert und Werkzeuge oder das Mausrad lösen keine ungewollten Aktionen im Hintergrund aus.
- **Stabile Menü-Skalierung**:
  - Das Vergrößern und Verkleinern des Menüs (UI-Skalierung) behält die linke obere Ecke an fester Position, sodass das Menü nicht über den Bildschirm wandert.

---

## [1.1.0] - 2026-08-29

### Neue Funktionen
- **Ingame-Menü**:
  - Neues Einstellungen-Menü im Spiel, aufrufbar über die Taste `Einfügen` (`Insert`).
  - Tastenbelegungen können direkt im Spiel angepasst werden.
- **Mehrsprachigkeit**:
  - Automatische Übersetzung von Menüs und Hinweisen auf Deutsch und Englisch.

---

## [1.0.0] - 2026-08-29

### Erstveröffentlichung
- Erster Release des Mod-Frameworks und des `HelloMod`-Demonstrations-Mods.
