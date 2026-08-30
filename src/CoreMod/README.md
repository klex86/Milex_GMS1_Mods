# Milex GMS1 CoreMod

- **Version:** `1.2.0`
- **Mod-Name:** Milex GMS1 CoreMod
- **Autor:** Milex
- **Dateiname:** `Milex_GMS1_CoreMod.dll`

Der **CoreMod** ist das zentrale Verwaltungssystem für alle Milex-Mods in *Gold Mining Simulator*. Er stellt das Ingame-Menü, die Sprachverwaltung und die grundlegenden Steuerungsfunktionen bereit.

---

## Installation & Voraussetzungen

1. **Voraussetzung**: BepInEx 5 (x64) im Hauptverzeichnis des Spiels.
2. **Installation**: Kopiere `Milex_GMS1_CoreMod.dll` in den Ordner `BepInEx/plugins/`.
3. **Start**: Das Mod-Menü wird im Spiel mit der Taste **`Einfügen`** (`Insert`) geöffnet.

---

## Funktionen im Spiel

1. **Ingame-Menü (Taste `Einfügen` / `Insert`)**:
   - Ermöglicht das Verwalten und Konfigurieren aller installierten Mods direkt im laufenden Spiel.

2. **Live-Verwaltung von Mods**:
   - Schalte einzelne Erweiterungs-Mods beliebig an oder aus. Änderungen greifen sofort ohne Spielneustart.

3. **Sprachsystem**:
   - Automatische Erkennung der Spielsprache.
   - Verwaltung aller Sprachdateien im Ordner `BepInEx/plugins/Milex GMS1 Mod Localization/`.
   - Entwickler-Option *"Externe Sprachdateien ignorieren"* zum direkten Testen interner Texte.

4. **Sichere Eingabesperre**:
   - Hält Kamera, Mausrad und Spielfigur an, während das Menü bedient wird.

---

## Konfigurations-Referenz (`Milex_GMS1_CoreMod.cfg`)

Die Einstellungen des Core-Mods liegen in der Datei `BepInEx/config/Milex_GMS1_CoreMod.cfg`. Die Parameter untergliedern sich wie folgt:

### Sektion `[General]` (Allgemeines Verhalten & Steuerung)

| Schlüssel | Typ | Standardwert | Beschreibung |
|---|---|---|---|
| **`MenuToggleKey`** | `KeyCode` | `Insert` | Taste zum Öffnen und Schließen des Mod-Menüs. |
| **`PauseGameOnMenu`** | `Boolean` | `false` | Pausiert die Spielwelt (TimeScale = 0), solange das Mod-Menü geöffnet ist. |
| **`IgnoreExternalTranslations`** | `Boolean` | `false` | Entwickler-Option: Ignoriert externe JSON-Sprachdateien und lädt Texte direkt aus den DLL-Ressourcen. |

### Sektion `[Localization]` (Spracheinstellungen)

| Schlüssel | Typ | Standardwert | Beschreibung |
|---|---|---|---|
| **`UseGameLanguage`** | `Boolean` | `true` | Erkennt automatisch die im Spiel/System gewählte Sprache. |
| **`SelectedLanguage`** | `String` | `en` | Manuell gewählter Sprachcode (z. B. `de`, `en`, `fr`), falls `UseGameLanguage` deaktiviert ist. |

### Sektion `[UI]` (Menü-Darstellung)

| Schlüssel | Typ | Standardwert | Beschreibung |
|---|---|---|---|
| **`UIScale`** | `Float` | `1.0` | Skalierungsfaktor des Mod-Menüs (0.70 bis 1.60) für hochauflösende Bildschirme. |

---

## Sprachdateien & Vorlagen-Verwaltung

- **Ordner**: `BepInEx/plugins/Milex GMS1 Mod Localization/`
- **Dateinamen**: `Milex_GMS1_CoreMod_de.json` (Deutsch), `Milex_GMS1_CoreMod_en.json` (Englisch) usw.

Wenn im Ingame-Menü eine Sprache ausgewählt wird, für die noch keine Datei existiert, kann per Klick auf *"Vorlagen erstellen"* automatisch eine neue JSON-Vorlage im Lokalisierungsordner erzeugt werden.
