# Milex GMS1 HelloMod

- **Version:** `1.1.0`
- **Mod-Name:** HelloMod
- **Autor:** Milex
- **Dateiname:** `Milex_GMS1_HelloMod.dll`
- **Abhängigkeit:** `Milex_GMS1_CoreMod.dll`

Ein Beispiel-Mod für *Gold Mining Simulator*, der demonstriert, wie Erweiterungs-Mods mit dem `Milex GMS1 CoreMod` zusammenarbeiten.

---

## Installation & Voraussetzungen

1. **Voraussetzung**: Installiertes BepInEx 5 und `Milex_GMS1_CoreMod.dll`.
2. **Installation**: Kopiere `Milex_GMS1_HelloMod.dll` in den Ordner `BepInEx/plugins/`.

---

## Funktionen

- **Test-Taste (`F2`)**: Gibt bei Druck eine konfigurierbare Begrüßungsnachricht im Log aus.
- **Vollständig anpassbar**:
  - Hotkey lässt sich im Ingame-Menü frei umbelegen.
  - Der Grußtext kann direkt im Menü geändert werden.
  - An/Aus-Schalter zum Aktivieren und Deaktivieren im laufenden Spiel.
- **Mehrsprachig**: Unterstützt automatisch Deutsch und Englisch.

---

## Konfigurations-Referenz (`Milex_GMS1_HelloMod.cfg`)

Die Einstellungen liegen in der Datei `BepInEx/config/Milex_GMS1_HelloMod.cfg`:

### Sektion `[General]` (Allgemeine Optionen & Mod-Status)

| Schlüssel | Typ | Standardwert | Beschreibung |
|---|---|---|---|
| **`Enabled`** | `Boolean` | `true` | Aktivierungszustand des Mods. Bei `false` werden alle Mod-Funktionen gestoppt. |
| **`ShowPrefix`** | `Boolean` | `true` | Legt fest, ob dem ausgegebenen Log-Text das Kürzel `[Greeting]` vorangestellt wird. |
| **`GreetingMessage`** | `String` | `"Hello from Gold Mining Simulator Modding!"` | Indivdueller Grußtext. |

### Sektion `[Controls]` (Steuerung)

| Schlüssel | Typ | Standardwert | Beschreibung |
|---|---|---|---|
| **`TestHotkey`** | `KeyCode` | `F2` | Taste zum Auslösen des Log-Eintrags. |

---

## Sprachdateien

- **Ordner**: `BepInEx/plugins/Milex GMS1 Mod Localization/`
- **Dateinamen**: `Milex_GMS1_HelloMod_de.json`, `Milex_GMS1_HelloMod_en.json`
- **Funktion**: Übersetzt alle Beschreibungen und Hinweistexte des Mods im Ingame-Menü sowie die Log-Ausgaben im Spiel.
