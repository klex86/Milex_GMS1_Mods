# Milex GMS1 HelloMod

- **Version:** `1.1.0`
- **Mod-Name:** HelloMod
- **Autor:** Milex
- **Dateiname:** `Milex_GMS1_HelloMod.dll`
- **Abhängigkeit:** `Milex_GMS1_CoreMod.dll`

Ein Beispiel-Mod für *Gold Mining Simulator*, der demonstriert, wie Erweiterungs-Mods mit dem `Milex GMS1 CoreMod` zusammenarbeiten.

---

## Blick unter die Haube: Die Konfigurationsdatei `Milex_GMS1_HelloMod.cfg`

Beim ersten Start des Spiels erstellt der HelloMod automatisch seine Konfigurationsdatei im Ordner:
`BepInEx/config/Milex_GMS1_HelloMod.cfg`

### Detaillierter Aufbau der Einstellungen

#### Sektion `[General]` (Allgemeine Optionen & Mod-Status)

- **`Enabled`**
  - **Typ**: Ja/Nein (`Boolean`)
  - **Standardwert**: `true`
  - **Beschreibung**: Steuert den Aktivierungszustand des Mods. Wenn auf `false` gestellt (z. B. über den Mod-Status Schalter im Ingame-Menü), werden alle Mod-Funktionen im laufenden Spiel gestoppt.
- **`ShowPrefix`**
  - **Typ**: Ja/Nein (`Boolean`)
  - **Standardwert**: `true`
  - **Beschreibung**: Legt fest, ob dem ausgegebenen Log-Text das Kürzel `[Greeting]` vorangestellt wird.
- **`GreetingMessage`**
  - **Typ**: Text (`String`)
  - **Standardwert**: `"Hello from Gold Mining Simulator Modding!"`
  - **Beschreibung**: Der individuelle Grußtext, der beim Drücken der Test-Taste ausgegeben wird.

#### Sektion `[Controls]` (Steuerung)

- **`TestHotkey`**
  - **Typ**: Tastencode (`KeyCode`)
  - **Standardwert**: `F2`
  - **Beschreibung**: Die Taste, die gedrückt werden muss, um den Grußtext in die Log-Konsole zu schreiben. Kann im Ingame-Menü beliebig umbelegt werden.

---

## Sprachdateien (`Milex_GMS1_HelloMod_<sprache>.json`)

- **Speicherort**: `BepInEx/plugins/Milex GMS1 Mod Localization/`
- **Enthaltene Sprachen**: `Milex_GMS1_HelloMod_de.json` (Deutsch), `Milex_GMS1_HelloMod_en.json` (Englisch)
- **Funktion**: Übersetzt alle Beschreibungen, Gruppentitel und Hinweise des HelloMods im Ingame-Menü sowie die Log-Ausgaben im Spiel.
