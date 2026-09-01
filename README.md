# Milex GMS1 Mods - Mod Framework & Collection

A modular modding system for **Gold Mining Simulator** (*Gold Rush: The Game*), built on BepInEx 5 and Harmony. It features a centralized in-game configuration menu, automatic configuration lifecycle management, and a complete multi-language localization framework.

---

## Documentation & Navigation

Direct links to documentation, guides, and changelogs for all repository components:

| Topic / Mod | Description | Documentation | Changelog |
|---|---|---|---|
| **Entire Monorepo** | Overview, architecture, and general setup | [Main Documentation](README.md) | [Root Changelog](CHANGELOG.md) |
| **Milex GMS1 CoreMod** | Central manager, in-game menu & localization engine | [CoreMod Guide](src/CoreMod/README.md) | [CoreMod Changelog](src/CoreMod/CHANGELOG.md) |
| **Milex GMS1 HelloMod** | Example mod demonstrating framework features | [HelloMod Guide](src/Mods/HelloMod/README.md) | [HelloMod Changelog](src/Mods/HelloMod/CHANGELOG.md) |
| **Milex GMS1 Production Tuner** | High-performance tuning for speeds, capacities, and hydraulics across 29 components | [Production Tuner Guide](src/Mods/ProductionTuner/README.md) | [Production Tuner Changelog](src/Mods/ProductionTuner/CHANGELOG.md) |
| **Community Knowledge Compendium** | Comprehensive reverse-engineering findings from 10 community mods & vanilla codebase | [Knowledge Compendium](Community%20Knowledge/COMMUNITY_KNOWLEDGE_COMPENDIUM.md) | - |
| **Keybindings** | Overview of hotkeys and in-game rebinding | [Keybindings Guide](KEYBINDINGS.md) | - |

---

## Installation & Getting Started

1. **Prerequisite**: Ensure **BepInEx 5** (x64) is installed in your main game directory.
2. **Install Mods**: Copy the compiled mod files (at least `Milex_GMS1_CoreMod.dll`, plus any sub-mods) into your `BepInEx/plugins/` folder.
3. **Start Game**: Launch the game normally.
4. **Open Mod Menu**: Press **`Insert`** in-game to open the Milex Mod Menu.

---

## In-Game Controls & Features

- **Toggle Mod Menu**: Press the **`Insert`** key.
- **Enable/Disable Mods Live**: In the *Loaded Mods* sidebar, toggle any mod on or off during live gameplay without restarting.
- **Adjust Configuration**: Keybindings, sliders, and options can be configured directly inside the menu.
- **Switch Languages**: In the *General* tab, select any installed language or keep automatic game-language synchronization enabled.

---

## Configuration & Localization Files

All mods in this framework handle configuration and text through standardized paths. No manual file creation is required — files are initialized automatically on first startup.

### Configuration Files (`.cfg`)

- **Location**: `BepInEx/config/`
- **Automatic Initialization**: On startup, each mod generates its own configuration file matching its assembly name (e.g. `Milex_GMS1_CoreMod.cfg` or `Milex_GMS1_ProductionTuner.cfg`).
- **Editing**: Settings can be modified live in the in-game menu or edited offline with any text editor.

### Localization Files (`.json`)

- **Location**: `BepInEx/plugins/Milex GMS1 Mod Localization/`
- **Embedded Defaults**: English (`_en.json`) and German (`_de.json`) translation templates are extracted automatically on first launch.

---

## Guide: Generating Templates for New Languages

If you play the game in a language that does not yet have a translation file (e.g. French, Polish, Spanish), you can generate translation templates directly from the game:

1. Press **`Insert`** to open the mod menu.
2. Navigate to **General** and disable **`Use Game Language`**.
3. In the **`Select Language`** dropdown, pick your desired target language.
4. The system detects missing translation files and displays a prompt dialog.
5. Click **`Create Templates`**.
6. Editable JSON template files (e.g. `Milex_GMS1_ProductionTuner_fr.json`) are immediately created in `BepInEx/plugins/Milex GMS1 Mod Localization/`.
7. Translate these files using any text editor. Community contributions are welcome on NexusMods!

---

## Development & Compilation

To build the entire solution:

```powershell
dotnet build GMSModding.sln
```

Build targets automatically deploy compiled assemblies to the `BepInEx/plugins/` directory.
