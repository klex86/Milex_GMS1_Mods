# Changelog - Milex GMS1 Mods

Alle relevanten Änderungen und Neuerungen der Mod-Sammlung werden in dieser Datei verständlich dokumentiert.

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
