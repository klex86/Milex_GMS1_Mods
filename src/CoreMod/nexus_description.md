# Milex GMS1 CoreMod
### In-Game Menu, FPS-Drop Fix & Mod Manager

[![Gold Mining Simulator Compatible](https://img.shields.io/badge/Game-Gold%20Mining%20Simulator-gold.svg)](#)
[![Version 1.4.0](https://img.shields.io/badge/Version-1.4.0-blue.svg)](#)
[![BepInEx 5 Required](https://img.shields.io/badge/Requires-BepInEx%205.4%2B-green.svg)](#)

---

## What Does This Mod Do?

Ever get annoyed having to quit your game, open config text files in Notepad, and restart the game just to change a simple mod setting? Or noticed that *Gold Mining Simulator* starts lagging and losing FPS the longer you play?

**Milex GMS1 CoreMod** fixes that. It gives you a clean in-game menu accessed anytime by pressing **`Insert`**, stops the game from slowing down over long sessions, fixes annoying loading screen freezes, and manages your mods right inside the game.

> **Important:** CoreMod is the required base mod for all other Milex mods (like *Production Tuner* and *Claim Monitor*). Install this first!

---

## Highlights

- **Change Mod Settings Live In-Game (`Insert`)**:
  - Press `Insert` at any time to open the menu.
  - Sliders, checkboxes, and search bar: Tweak your speeds, capacities, and HUDs live while playing.
  - Zero restarts needed: Changes apply immediately.
  - Move the menu anywhere on your screen and scale the UI size so it looks great on any monitor (1080p, 1440p, or 4K).
- **Stops Long-Session FPS Drops**:
  - You've probably noticed that after playing for an hour or two, your frame rate gets worse and worse.
  - This mod automatically cleans up accumulated memory leaks in the background when you fast travel, save your game, or open your laptop.
  - Keeps your game running smooth and stutter-free even during 5-hour mining marathons.
  - Got a sudden lag spike? Click **"Clean Memory Now"** in the menu to instantly clear clutter.
- **Turn Mods On or Off with One Click**:
  - Don't want a specific mod active right now? Just uncheck it in the sidebar. It turns off instantly and returns your game to 100% normal vanilla without quitting.
- **Play in Your Own Language**:
  - Works out of the box in English and German.
  - Want the mod in Spanish, French, Polish, or another language? You can generate translation files with one click in-game!

---

## Quick Install Guide

1. Make sure you have **BepInEx 5 (x64)** installed in your game folder.
2. Drop `Milex_GMS1_CoreMod.dll` into your `Gold Rush The Game\BepInEx\plugins` folder.
3. Start the game, load your claim, and press **`Insert`**!

---

## Controls

| Key / Button | What It Does |
|---|---|
| **`Insert`** | **Open / Close Menu** (Locks camera while open so you can click freely). |
| **Click & Drag Header** | Move the menu window anywhere on your screen. |
| **UI Scale Slider** | Make the menu bigger or smaller to fit your screen resolution. |
| **Clean Memory Now** | Instantly clears RAM clutter and shows how many MB were freed. |
| **Resume Game Button** | Unfreezes the game if a vanilla fast-travel bug left you stuck. |

*(Prefer a different key? You can rebind `Insert` to any key in the menu or config).*

---

## Settings & Configuration

You can change everything directly in the in-game menu (**`Insert`**). If you prefer editing files, your config is saved at:
`BepInEx/config/Milex_GMS1_CoreMod.cfg`

- **`MenuToggleKey`**: Change the hotkey from `Insert` to whatever key you like.
- **`PauseGameOnMenu`**: Choose whether opening the mod menu freezes the game world or lets it keep running in the background.
- **`UIScale`**: Adjust how large the menu looks on your screen.
- **`EnableMemoryCleaner`**: Turn automatic background memory cleanups on or off.

---

## Requirements & Step-by-Step Install

### What You Need
- *Gold Mining Simulator* (*Gold Rush: The Game*) on Steam.
- [BepInEx 5 (version 5.4.21 or newer, x64)](https://github.com/BepInEx/BepInEx/releases) – the standard mod loader for the game.

### How to Install
1. If you don't have BepInEx yet: Download **BepInEx_x64_5.4.x.zip**, open it, and drag the contents into your main game folder (where `GoldRushTheGame.exe` is).
2. Download this mod.
3. Put `Milex_GMS1_CoreMod.dll` into your `Gold Rush The Game\BepInEx\plugins` folder.
4. Launch the game!

### How to Uninstall
Delete `Milex_GMS1_CoreMod.dll` from your `BepInEx\plugins\` folder. Done. Your game and saves remain 100% untouched.

---

## Want to Help Translate the Mod?

CoreMod makes it super easy to translate all Milex mods into your language:

1. Press **`Insert`** in-game and click **General** in the sidebar.
2. Turn off **"Use Game Language"** and select your language in the dropdown (French, Polish, Spanish, etc.).
3. Click **"Create Templates"** when prompted.
4. The mod will instantly create editable translation files in:
   `BepInEx\plugins\Milex GMS1 Mod Localization\`
5. Open the files with Notepad, translate the text, and save!

> **Did you translate it?** 
> Post your translated file in the **Comments / Posts** section here on NexusMods! We'll happily include it in the next update so other players speaking your language can use it too!

---

## Changelog

### Version 1.4.0
- Completely overhauled menu performance: super smooth, fast, and responsive with zero stutter or lag.
- Improved window dragging and resizing on high-resolution displays.

### Version 1.3.0
- Added automatic background memory cleaner to stop FPS drops during long play sessions.
- Added one-click "Clean Memory Now" button with megabyte readout.
- Added emergency unpause button to rescue games stuck after fast travel.

### Version 1.2.0
- Added live search bar to quickly find any mod setting.
- Added organized category tabs for faster navigation.

### Version 1.1.0
- Added UI Scale slider for 1440p, 4K, and ultrawide screens.
- Added in-game translation template generator.

### Version 1.0.0
- Initial release with in-game menu, mod toggle sidebar, and multi-language support.
