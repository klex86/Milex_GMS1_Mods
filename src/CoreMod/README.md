# Milex GMS1 CoreMod

- **Version:** `1.2.0`
- **Mod-Name:** Milex GMS1 CoreMod
- **Autor:** Milex

Der **CoreMod** ist das zentrale Verwaltungssystem für alle Milex-Mods in *Gold Mining Simulator*. Er stellt das Ingame-Menü, die Sprachverwaltung und die grundlegenden Steuerungsfunktionen für alle anderen Mods bereit.

---

## Funktionen

1. **Ingame-Menü (Taste `Einfügen` / `Insert`)**:
   - Ermöglicht das Verwalten und Konfigurieren aller installierten Mods direkt im laufenden Spiel.

2. **Live-Verwaltung von Mods**:
   - Schalte einzelne Mods nach Belieben an oder aus. Die Änderungen greifen sofort ohne Spielneustart.

3. **Sprachsystem**:
   - Automatische Erkennung der Spielsprache.
   - Ermöglicht das Bearbeiten von Sprachdateien im Ordner `BepInEx/plugins/Milex GMS1 Mod Localization/`.
   - Entwickler-Funktion *"Externe Sprachdateien ignorieren"* zum direkten Testen interner Texte.

4. **Sichere Eingabesperre**:
   - Hält Kamera, Mausrad und Spielfigur an, während das Menü bedient wird.

---

## Konfigurations-Optionen im Menü

| Bereich | Einstellung | Beschreibung |
|---|---|---|
| **Allgemein** | `Spiel bei offenem Menü pausieren` | Hält die Zeit im Spiel an, solange das Mod-Menü geöffnet ist |
| **Allgemein** | `Externe Sprachdateien ignorieren` | Lädt Texte direkt aus den Mod-Dateien für Entwicklertests |
| **Sprache** | `Spiel-Sprache verwenden` | Erkennt automatisch die eingestellte Sprache des Spiels |
| **Sprache** | `Sprache wählen` | Manuelle Auswahl der Menüsprache |
| **Darstellung** | `UI-Skalierung` | Vergrößert oder verkleinert das Menü für hochauflösende Bildschirme |
| **Tastenbelegung** | `Menü-Taste` | Anpassbare Taste zum Öffnen und Schließen des Menüs |
