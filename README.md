# Milex GMS1 Mods - Mod-Framework & Mod-Sammlung

Modding-Framework und modulare Mod-Sammlung für **Gold Mining Simulator** (*Gold Rush: The Game*), basierend auf **BepInEx 5**, **Harmony**, zentralem **Ingame-Konfigurationsmenü** und vollständigem **Mehrsprachigkeits-System (Localization)**.

---

## 📌 Architektur & Multi-Assembly Aufbau

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
│   ├── CoreMod\                           # 📦 Milex_GMS1_CoreMod.dll (v1.2.0)
│   │   ├── Milex_GMS1_CoreMod.csproj
│   │   ├── CorePlugin.cs                  # BepInEx-Einstiegspunkt, Menü-Toggle (Insert)
│   │   ├── ModBase.cs                     # Basisklasse mit Auto-Harmony & ModRegistry
│   │   ├── ModRegistry.cs                 # Zentrale Mod-Registrierung
│   │   ├── Localization\                  # LocalizationManager & Embedded JSONs
│   │   │   ├── LocalizationManager.cs
│   │   │   ├── CoreMod_en.json
│   │   │   └── CoreMod_de.json
│   │   └── UI\
│   │       └── ModMenuUI.cs               # Ingame IMGUI-Menü mit Core Settings Tab
│   │
│   └── Mods\
│       └── HelloMod\                      # 📦 Milex_GMS1_HelloMod.dll (v1.1.0)
│           ├── Milex_GMS1_HelloMod.csproj
│           ├── Localization\
│           │   ├── HelloMod_en.json
│           │   └── HelloMod_de.json
│           └── HelloModPlugin.cs
```

---

## 🌐 Lokalisierung (Multi-Language)

- **Ordner auf der Festplatte**: Beim Mod-Start wird automatisch der Ordner  
  `BepInEx\plugins\Milex GMS1 Mod Localization\` erstellt.
- **Dateibenennung**: `%Modname%_%language%.json` (z. B. `CoreMod_en.json`, `CoreMod_de.json`, `HelloMod_en.json`, `HelloMod_de.json`).
- **Auto-Template-Generierung**: Fehlt eine EN- oder DE-Sprachdatei auf der Festplatte, schreibt `LocalizationManager` sie automatisch aus den eingebetteten DLL-Ressourcen als editierbare Vorlage heraus.
- **Unterstützte Sprachcodes**:
  `fr`, `en`, `de`, `es`, `ru`, `pl`, `it`, `pt`, `tr`, `nl`, `sv`, `da`, `no`, `ro`, `cs`, `bg`, `el`, `ja`, `ko`, `zh-CN`, `zh-TD`.
- **Fallback**: Bei fehlenden Übersetzungen wird immer garantiert auf **Englisch (`en`)** zurückgegriffen.

---

## 🛠 Voraussetzungen & Setup

1. **Spiel**: Installiertes *Gold Rush: The Game* / *Gold Mining Simulator*.
2. **BepInEx 5**: BepInEx 5.x x64 im Hauptverzeichnis des Spiels.
3. **.NET SDK**: .NET Core / .NET 6+ SDK zur Kompilierung (`netstandard2.0`).

---

## 🚀 Build & Deployment

```powershell
dotnet build GMSModding.sln
```
Alle DLLs (`Milex_GMS1_CoreMod.dll`, `Milex_GMS1_HelloMod.dll` usw.) werden automatisch nach `BepInEx\plugins\` kopiert.

---

## 🧪 Ingame-Bedienung

1. Drücke **`Insert`** (Einfügen) $\rightarrow$ Das **MILEX GMS1 MODS**-Menü öffnet sich.
2. Im Tab **`⚙ Core-Optionen`**:
   - `Spiel-Sprache verwenden` an-/abschalten.
   - Falls abgeschaltet: Sprache manuell aus der Button-Matrix wählen (z. B. `de`, `en`, `fr`...).
   - Menü-Taste per Rebind ändern.
3. Im Tab **`📦 Geladene Mods`**:
   - Mod auswählen (z. B. `HelloMod`).
   - Alle Sektionen, Einstellungen, Beschreibungen und Tastenbelegungen werden in der gewählten Sprache gerendert und bei Änderung sofort auf die Festplatte gespeichert.
