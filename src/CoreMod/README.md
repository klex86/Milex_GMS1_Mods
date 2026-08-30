# Milex GMS1 CoreMod

- **Version:** `1.2.0`
- **Mod-Name:** Milex GMS1 CoreMod
- **Autor:** Milex
- **Dateiname:** `Milex_GMS1_CoreMod.dll`

Der **CoreMod** ist das zentrale Verwaltungssystem für alle Milex-Mods in *Gold Mining Simulator*. Er stellt das Ingame-Menü, die Sprachverwaltung und die grundlegenden Steuerungsfunktionen bereit.

---

## Blick unter die Haube: Die Konfigurationsdatei `Milex_GMS1_CoreMod.cfg`

Beim ersten Start des Spiels erstellt der CoreMod automatisch seine Konfigurationsdatei im Ordner:
`BepInEx/config/Milex_GMS1_CoreMod.cfg`

Diese Datei speichert alle Einstellungen des Mod-Managers. Du kannst die Einstellungen entweder im Ingame-Menü ändern oder die `.cfg`-Datei mit einem Texteditor bearbeiten.

### Detaillierter Aufbau der Einstellungen

#### Sektion `[General]` (Allgemeines Verhalten & Steuerung)

- **`MenuToggleKey`**
  - **Typ**: Tastencode (`KeyCode`)
  - **Standardwert**: `Insert` (Taste *Einfügen*)
  - **Beschreibung**: Legt fest, welche Taste das Ingame-Mod-Menü öffnet und schließt. Kann im Menü durch einfaches Drücken einer neuen Taste umbelegt werden.
- **`PauseGameOnMenu`**
  - **Typ**: Ja/Nein (`Boolean`)
  - **Standardwert**: `false`
  - **Beschreibung**: Wenn auf `true` gesetzt, hält das Spiel die Zeit im Hintergrund an (TimeScale = 0), sobald das Mod-Menü geöffnet wird.
- **`IgnoreExternalTranslations`**
  - **Typ**: Ja/Nein (`Boolean`)
  - **Standardwert**: `false`
  - **Beschreibung**: Entwickler-Option. Bei `true` werden alle externen Sprachdateien im Ordner `Milex GMS1 Mod Localization` ignoriert und Texte direkt aus den internen Ressourcen der Mod-Dateien geladen.

#### Sektion `[Localization]` (Spracheinstellungen)

- **`UseGameLanguage`**
  - **Typ**: Ja/Nein (`Boolean`)
  - **Standardwert**: `true`
  - **Beschreibung**: Bei `true` erkennt der Mod-Manager automatisch die im Spiel oder System gewählte Sprache.
- **`SelectedLanguage`**
  - **Typ**: Text / Sprachcode (`String`)
  - **Standardwert**: `en`
  - **Mögliche Werte**: `de`, `en`, `fr`, `es`, `pl`, `ru`, `it`, `pt`, `tr`, `nl`, `sv`, `da`, `no`, `ro`, `cs`, `bg`, `el`, `ja`, `ko`, `zh-CN`, `zh-TD`
  - **Beschreibung**: Die manuell gewählte Sprache, falls `UseGameLanguage` auf `false` steht.

#### Sektion `[UI]` (Menü-Darstellung)

- **`UIScale`**
  - **Typ**: Dezimalzahl (`Float`)
  - **Standardwert**: `1.0`
  - **Wertebereich**: `0.70` bis `1.60` (entspricht 70% bis 160%)
  - **Beschreibung**: Skaliert die Größe des Mod-Menüs und der Schriften. Ideal für hochauflösende 1440p- oder 4K-Monitore.

---

## Das Lokalisierungssystem & Vorlagen-Erstellung

Der CoreMod verwaltet die Übersetzungen für sich selbst und alle angeschlossenen Sub-Mods.

- **Speicherort der Sprachdateien**: `BepInEx/plugins/Milex GMS1 Mod Localization/`
- **Dateinamen**: `Milex_GMS1_CoreMod_de.json` (Deutsch), `Milex_GMS1_CoreMod_en.json` (Englisch) usw.

### Erstellen neuer Sprachvorlagen
Wenn du eine Sprache wählst, für die noch keine Datei existiert, fragt dich das Ingame-Menü automatisch, ob Vorlagen erstellt werden sollen. Bei Bestätigung schreibt das System fertige, strukturierte JSON-Dateien mit Übersetzungshinweisen in den Lokalisierungsordner.
