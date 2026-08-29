# Keybindings Reference & Modding Guidelines

Diese Referenz dokumentiert alle bekannten Tastenbelegungen in **Gold Mining Simulator** (*Gold Rush: The Game*), externe Tool-Hotkeys sowie eine Liste sicherer Tasten für Mods. **Diese Liste ist bei allen Mod-Entwicklungen zwingend zu respektieren**, um Konflikte mit Spielfunktionen oder Dev-Tools zu vermeiden.

---

## 🚫 Reservierte Tasten (NICHT für Mod-Hotkeys verwenden)

### 1. Ingame-Funktionstasten (F-Keys)
- **`F5`**: QuickSave (Schnellspeichern)
- **`F9`**: QuickLoad (Schnellladen)
- **`F10`**: Hide HUD (Interface ein-/ausblenden)
- **`F12`**: Steam Screenshot

### 2. Modding- & Dev-Tools
- **`F7`**: UnityExplorer GUI (Ingame Object-Browser / Inspector)

### 3. Bewegung, Fahrzeuge & Interaktion
- **`W` / `A` / `S` / `D`**: Gehen / Fahren
- **`E`**: Einsteigen in Fahrzeuge / Interagieren
- **`Space`**: Handbremse (Fahrzeuge) / Springen (zu Fuß)
- **`Shift`**: Sprinten / Umschalten des Baggermodus (Fahren ↔ Graben)
- **`C`**: Kamera wechseln (First-Person / Third-Person)
- **`L`**: Taschenlampe / Fahrzeugbeleuchtung an/aus

### 4. Werkzeuge & UI-Navigation
- **`1`, `2`, `3`, `4`**: Direktauswahl von Werkzeugen (z. B. `3` für Schaufel)
- **`Mausrad`**: Schnellauswahl / Durchschalten der Werkzeuge
- **`Tab`** / **`M`**: Tablet / Karte öffnen
- **`Z`**: Steuerungshilfe ein-/ausblenden (Show Controls)
- **`Escape`**: Pause-Menü / Einstellungen

---

## ✅ Sichere & Empfohlene Tasten für Mod-Hotkeys

Folgende Tasten kollidieren standardmäßig weder mit Spielfunktionen noch mit gängigen Modding-Tools:

| Taste | `UnityEngine.KeyCode` | Empfohlene Nutzung |
|---|---|---|
| **`F2`** | `KeyCode.F2` | Allgemeine Mod-Aktionen / Toggles |
| **`F3`** | `KeyCode.F3` | Debug-Overlays / Logging |
| **`F4`** | `KeyCode.F4` | Mod-Menüs |
| **`Insert`** (Einfügen) | `KeyCode.Insert` | Mod-Hauptmenü / Setup |
| **`Home`** (Pos1) | `KeyCode.Home` | Quick-Actions / Reset |
| **`End`** (Ende) | `KeyCode.End` | Quick-Actions |
| **`Numpad *`** | `KeyCode.KeypadMultiply` | Debug-Commands |
| **`Numpad +`** | `KeyCode.KeypadPlus` | Inkrementelle Mod-Werte |
| **`Numpad -`** | `KeyCode.KeypadMinus` | Dekrementelle Mod-Werte |
| **`Numpad /`** | `KeyCode.KeypadDivide` | Mod-Funktionen |
| **`PageUp` / `PageDown`** | `KeyCode.PageUp` / `KeyCode.PageDown` | Pagination in UIs |
