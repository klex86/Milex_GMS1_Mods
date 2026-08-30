# Milex GMS1 Mods - Mod-Framework & Mod-Sammlung

Modding-Framework und modulare Mod-Sammlung für **Gold Mining Simulator** (*Gold Rush: The Game*), basierend auf **BepInEx 5**, **Harmony**, zentralem **Ingame-Konfigurationsmenü** und vollständigem **Mehrsprachigkeits-System (Localization)**.

---

## Architektur & Multi-Assembly Aufbau

Das Repository ist als modulare Multi-Project Solution aufgesetzt:

```text
d:\Modding\GMSModding\
├── GMSModding.sln                         # Solution für alle Projekte
├── Directory.Build.props                  # Zentrale Spielpfade, References & Auto-Deploy
├── KEYBINDINGS.md                         # Hotkey-Referenz & Modding-Leitfaden
├── README.md                              # Gesamt-Dokumentation
├── CHANGELOG.md                           # Zentrales Changelog
│
├── src\
│   ├── CoreMod\                           # Milex_GMS1_CoreMod.dll (v1.2.0)
│   │   ├── Milex_GMS1_CoreMod.csproj
│   │   ├── CorePlugin.cs                  # BepInEx-Einstiegspunkt, Menü-Toggle (Insert)
│   │   ├── ModBase.cs                     # Basisklasse mit Enable/Disable-Lifecycle & ModRegistry
│   │   ├── ModRegistry.cs                 # Zentrale Mod-Registrierung
│   │   ├── Localization\                  # LocalizationManager & Embedded JSONs
│   │   │   ├── LocalizationManager.cs
│   │   │   ├── Milex_GMS1_CoreMod_en.json
│   │   │   └── Milex_GMS1_CoreMod_de.json
│   │   ├── Patches\                       # Input-Blocker & Cursor-Patches
│   │   └── UI\
│   │       └── ModMenuUI.cs               # Ingame IMGUI-Menü mit Skalierung & Mod-Verwaltung
│   │
│   └── Mods\
│       └── HelloMod\                      # Milex_GMS1_HelloMod.dll (v1.1.0)
│           ├── Milex_GMS1_HelloMod.csproj
│           ├── Localization\
│           │   ├── Milex_GMS1_HelloMod_en.json
│           │   └── Milex_GMS1_HelloMod_de.json
│           └── HelloModPlugin.cs
```

---

## Hauptfunktionen des CoreMod Frameworks

1. **Ingame Mod-Menü (`Insert`)**:
   - Tab **Allgemein**: Sprachauswahl, UI-Skalierung, Spiel-Pause bei offenem Menü, Option zum Ignorieren externer Sprachdateien, Menü-Taste Rebinding.
   - Tab **Geladene Mods**: Liste aller aktiven Feature-Mods mit Live Enable/Disable-Schalter und dynamischer Konfiguration.

2. **Live Enable / Disable Lifecycle**:
   - Jeder Sub-Mod kann direkt im Ingame-Menü per Klick aktiviert oder deaktiviert werden.
   - Bei Deaktivierung werden alle Harmony-Patches entfernt (`UnpatchSelf`) und die MonoBehaviour-Ausführung gestoppt (`enabled = false`).

3. **Mehrsprachigkeit (Localization System)**:
   - Eingebettete JSON-Übersetzungen in den DLL-Ressourcen.
   - Automatische Template-Generierung auf der Festplatte (`BepInEx\plugins\Milex GMS1 Mod Localization\`).
   - Unterbindung externer JSON-Dateien für Entwickler per Schalter `IgnoreExternalTranslations`.

4. **Input & Kamera-Freeze**:
   - Vollständiges Einfrieren der Kamera, Spielerbewegung und Werkzeuge (inkl. Mausrad) bei geöffnetem Mod-Menü.

---

## Voraussetzungen & Setup

1. **Spiel**: Installiertes *Gold Rush: The Game* / *Gold Mining Simulator*.
2. **BepInEx 5**: BepInEx 5.x x64 im Hauptverzeichnis des Spiels.
3. **.NET SDK**: .NET Core / .NET Framework / .NET SDK zur Kompilierung (`netstandard2.0`).

---

## Build & Deployment

```powershell
dotnet build GMSModding.sln
```
Alle fertigen DLLs (`Milex_GMS1_CoreMod.dll`, `Milex_GMS1_HelloMod.dll` usw.) werden automatisch nach `BepInEx\plugins\` kopiert.
