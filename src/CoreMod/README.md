# Milex GMS1 CoreMod

- **Version:** `1.2.0`
- **GUID:** `com.milex.gms1.core`
- **Autor:** Milex
- **Assembly:** `Milex_GMS1_CoreMod.dll`

Das zentrale Framework und Management-Modul für alle **Milex GMS1 Mods** in **Gold Mining Simulator** (*Gold Rush: The Game*).

---

## Features & Aufgaben

1. **`ModBase` Basisklasse**:
   - Standardisierte Basis für alle Unity-Mods (`BaseUnityPlugin`).
   - Automatisches Harmony-Patching und Unpatching.
   - Live Enable/Disable Lifecycle (`SetEnabled(bool)`, `OnModEnabled()`, `OnModDisabled()`).
   - Mod-Prefix-Logging (`LogInfo`, `LogWarning`, `LogError`).
   - Automatische Registrierung im zentralen Menü und bei der Lokalisierungs-Engine.
   - `Translate(key, fallback)`-Methode für einfachen Zugriff auf Mod-Sprachdateien.

2. **Mehrsprachigkeits- & Lokalisierungssystem (`LocalizationManager`)**:
   - Legt automatisch den Ordner `BepInEx\plugins\Milex GMS1 Mod Localization\` an.
   - Schreibt fehlende Sprachdateien (`%AssemblyName%_en.json`, `%AssemblyName%_de.json`) beim Mod-Start automatisch als editierbare Vorlagen aus den eingebetteten DLL-Ressourcen auf die Festplatte.
   - Unterstützt 21 Sprachen.
   - Entwickler-Option `IgnoreExternalTranslations` zum Ignorieren lokaler JSON-Dateien.

3. **Ingame Mod-Menü (`ModMenuUI`)**:
   - Aufrufbar mit Taste `Insert` (Einfügen).
   - **Tab Allgemein**:
     - Spiel-Sprache verwenden (Standard: `true`).
     - Manuelle Sprachauswahl.
     - UI-Skalierung mit fester verankerter Bildschirm-Position.
     - Spiel-Pause bei geöffnetem Menü (`PauseGameOnMenu`).
     - Externe Sprachdateien ignorieren (`IgnoreExternalTranslations`).
     - Hotkey-Rebinding für die Menü-Taste.
   - **Tab Geladene Mods / Sidebar**:
     - Übersicht aller installierten Sub-Mods.
     - Status-Indikator und Live Enable/Disable Schalter pro Mod.
     - Echtzeit-Persistenz in die `.cfg`-Dateien unter `BepInEx/config/`.

---

## Konfiguration

Die Einstellungen des Core-Mods befinden sich in `BepInEx/config/Milex_GMS1_CoreMod.cfg`:

| Sektion | Schlüssel | Standardwert | Beschreibung |
|---|---|---|---|
| `General` | `MenuToggleKey` | `Insert` | Taste zum Öffnen/Schließen des Mod-Menüs |
| `General` | `PauseGameOnMenu` | `false` | Pausiert die Spielwelt bei geöffnetem Menü |
| `General` | `IgnoreExternalTranslations` | `false` | Ignoriert externe JSON-Dateien und lädt aus DLL-Ressourcen |
| `Localization` | `UseGameLanguage` | `true` | Automatische Erkennung der Spiel-/System-Sprache |
| `Localization` | `SelectedLanguage` | `en` | Manuell gewählter Sprachcode |
| `UI` | `UIScale` | `1.0` | Skalierungsfaktor des Mod-Menüs |
