# Milex GMS1 CoreMod - Sub-Mod Developer Guide

Welcome to the **Milex GMS1 CoreMod** developer handbook!
This guide is written for modders, software engineers, and community developers who want to create mods for **Gold Mining Simulator** (*Gold Rush: The Game*) that integrate smoothly into the Milex CoreMod framework.

---

## 1. Why Develop on the Milex Framework?

Historically, modding Unity games with BepInEx required every mod author to write custom IMGUI code, manage configuration files manually, handle keybindings, and deal with game crashes when re-patching methods.

The **Milex GMS1 CoreMod** provides a unified foundation:

- **Zero-Code In-Game UI**: Declare your settings via standard BepInEx `Config.Bind(...)`, and CoreMod automatically generates modern UI cards, sliders, toggles, category tabs, and search indexing in both the **Modern Canvas Dashboard** (uGUI) and the **Classic IMGUI** fallback menu.
- **Runtime Enable/Disable**: Players can turn your mod on and off in-game without restarting the game. Harmony patches and `Update()` loops are attached and detached cleanly.
- **Multi-Language Localization**: Full localization engine built-in. Ship embedded JSON files; CoreMod extracts them, resolves player languages, and lets users add custom community translations.
- **Input & Pause Management**: Opening the mod menu seamlessly unlocks the mouse cursor, blocks player/vehicle movement, and freezes or restores game time without disrupting the game state.
- **Shared Architecture**: Built on clean, tested abstractions with zero dependencies on third-party binary assets.

---

## 2. Prerequisites & Environment Setup

Before starting, ensure you have:
1. **.NET SDK 6.0 or newer** (supporting `netstandard2.0`).
2. **Gold Mining Simulator** installed via Steam.
3. **BepInEx 5.4.21+ (x64)** installed in your game directory (`<GameRoot>/BepInEx/`).
4. An IDE of your choice:
   - **VS Code** with the *C# Dev Kit* extension.
   - **Visual Studio 2022** (.NET desktop development workload).
   - **JetBrains Rider**.

---

## 3. Getting Started: Setting Up Your Project

You can develop your mod either **inside this monorepo** or as a **standalone external project**.

### Option A: Inside the Monorepo (Recommended)

1. Create a new directory under `src/Mods/<YourModName>/`.
2. Add your `.csproj` file (see template below).
3. Register the project in the central solution:
   ```powershell
   dotnet sln GMSModding.sln add src/Mods/<YourModName>/Milex_GMS1_<YourModName>.csproj
   ```
4. Build using:
   ```powershell
   dotnet build GMSModding.sln
   ```
   All assemblies will automatically be copied to your game's `BepInEx/plugins/` directory via `Directory.Build.props`.

### Option B: Standalone External Project

Create a new C# Class Library targeting `netstandard2.0`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <RootNamespace>Milex.GMS1.Mods.<YourModName></RootNamespace>
    <AssemblyName>Milex_GMS1_<YourModName></AssemblyName>
    <Version>1.0.0</Version>
    <GameRootPath>D:\SteamLibrary\steamapps\common\Gold Rush The Game</GameRootPath>
    <ManagedDataPath>$(GameRootPath)\GoldMiningSimulator_Data\Managed</ManagedDataPath>
    <BepInExCorePath>$(GameRootPath)\BepInEx\core</BepInExCorePath>
    <BepInExPluginsPath>$(GameRootPath)\BepInEx\plugins</BepInExPluginsPath>
  </PropertyGroup>

  <ItemGroup>
    <!-- CoreMod Reference -->
    <Reference Include="Milex_GMS1_CoreMod">
      <HintPath>$(BepInExPluginsPath)\Milex_GMS1_CoreMod.dll</HintPath>
      <Private>false</Private>
    </Reference>

    <!-- BepInEx & Harmony -->
    <Reference Include="BepInEx"><HintPath>$(BepInExCorePath)\BepInEx.dll</HintPath><Private>false</Private></Reference>
    <Reference Include="0Harmony"><HintPath>$(BepInExCorePath)\0Harmony.dll</HintPath><Private>false</Private></Reference>

    <!-- Unity & Game Assemblies -->
    <Reference Include="UnityEngine"><HintPath>$(ManagedDataPath)\UnityEngine.dll</HintPath><Private>false</Private></Reference>
    <Reference Include="UnityEngine.CoreModule"><HintPath>$(ManagedDataPath)\UnityEngine.CoreModule.dll</HintPath><Private>false</Private></Reference>
    <Reference Include="UnityEngine.UI"><HintPath>$(ManagedDataPath)\UnityEngine.UI.dll</HintPath><Private>false</Private></Reference>
    <Reference Include="Assembly-CSharp"><HintPath>$(ManagedDataPath)\Assembly-CSharp.dll</HintPath><Private>false</Private></Reference>

    <!-- Embedded Localization Resources -->
    <EmbeddedResource Include="Localization\*.json" />
  </ItemGroup>

  <!-- Auto-deploy on build -->
  <Target Name="PostBuildDeploy" AfterTargets="PostBuildEvent">
    <Copy SourceFiles="$(TargetPath)" DestinationFolder="$(BepInExPluginsPath)\" ContinueOnError="true" />
  </Target>
</Project>
```

---

## 4. The Plugin Class (`ModBase`)

Every sub-mod inherits from `Milex.GMS1.Core.ModBase` (which itself extends BepInEx's `BaseUnityPlugin`).

```csharp
using BepInEx;
using Milex.GMS1.Core;
using UnityEngine;

namespace Milex.GMS1.Mods.QuickFuel
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CorePlugin.PluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public class QuickFuelPlugin : ModBase
    {
        public const string PluginGuid = "com.milex.gms1.quickfuel";
        public const string PluginName = "Milex GMS1 Quick Fuel";
        public const string PluginVersion = "1.0.0";

        public override string ModGuid => PluginGuid;
        public override string ModName => PluginName;
        public override string ModVersion => PluginVersion;

        public static QuickFuelPlugin Instance { get; private set; }

        protected override void Awake()
        {
            Instance = this;

            // 1. Bind your settings (they will automatically appear in the In-Game Menu!)
            BindSettings();

            // 2. Base initialization - handles ModRegistry, Harmony patching, and localization
            base.Awake();

            LogInfo(Translate("log.ready", "Quick Fuel mod loaded and ready."));
        }

        private void BindSettings()
        {
            // Settings go here...
        }

        protected override void OnModEnabled()
        {
            LogInfo("Quick Fuel enabled by player.");
        }

        protected override void OnModDisabled()
        {
            LogInfo("Quick Fuel disabled by player.");
            // Revert modified game state here
        }
    }
}
```

### What `base.Awake()` Does Automatically:
1. **Creates `Config`**: Named after your DLL (`BepInEx/config/Milex_GMS1_<YourModName>.cfg`).
2. **Registers with `ModRegistry`**: Appears in the In-Game Menu sidebar.
3. **Registers Localization**: Loads embedded language files and maps them to your mod.
4. **Applies Harmony Patches**: Calls `Harmony.PatchAll(Assembly)` if enabled.
5. **Registers Lifecycle Handlers**: Hooks into live enable/disable toggles.

---

## 5. Automatic In-Game UI Generation

When the player presses **`Insert`** in-game, CoreMod builds a clean configuration interface for your mod dynamically. You do **not** write a single line of UI rendering code!

### 5.1 Continuous Numeric Sliders (`float`)

Provide an `AcceptableValueRange<float>(min, max)`:

```csharp
public static ConfigEntry<float> FuelTransferSpeed;

FuelTransferSpeed = Config.Bind(
    "Logistics",                                    // Section Name (becomes Category Tab & Group Header)
    "TransferSpeed",                               // Config Key Name
    2.0f,                                          // Default Value
    new ConfigDescription(
        "Multiplier for fuel transfer rate.",      // Tooltip / Description
        new AcceptableValueRange<float>(1.0f, 10.0f) // Slider range: 1.0x to 10.0x
    )
);
```

In the menu:
- A smooth slider with visual track and gold progress bar.
- Interactive text showing `2.00x`.
- A dedicated **Reset to Default** (`[R]`) button that resets only this value.

### 5.2 Stepped Integer Sliders (`int`)

```csharp
public static ConfigEntry<int> MaxJerryCans;

MaxJerryCans = Config.Bind(
    "Logistics",
    "MaxCans",
    4,
    new ConfigDescription(
        "Maximum simultaneous jerry cans allowed in the bed.",
        new AcceptableValueRange<int>(1, 16)
    )
);
```

### 5.3 Toggle Switches (`bool`)

```csharp
public static ConfigEntry<bool> InfiniteGeneratorFuel;

InfiniteGeneratorFuel = Config.Bind(
    "Generators",
    "InfiniteFuel",
    false,
    new ConfigDescription("Prevents generators from consuming fuel.")
);
```

In the menu:
- Rendered as a sleek animated toggle pill switch.

### 5.4 Customizable Hotkeys (`KeyCode`)

```csharp
public static ConfigEntry<KeyCode> RefuelHotkey;

RefuelHotkey = Config.Bind(
    "Controls",
    "RefuelKey",
    KeyCode.R,
    new ConfigDescription("Press this key while near a machine to trigger quick refuel.")
);
```

In the menu:
- Rendered with an interactive keybinding card showing the active key.
- Clicking the button enters rebinding mode: *"Press any key..."*.

### 5.5 Section Tabs & Group Resets
- Every unique `Section` string in `Config.Bind(...)` automatically creates a category tab in the Modern Dashboard and a card group header.
- Each header includes an automatic **"Reset Group"** button that resets all settings in that section at once.

---

## 6. Safe Harmony Patching & The Golden Rule

> [!CAUTION]
> **Avoid the Multiplicative Drift Trap!**
> Never do this in an `Update()` loop or Harmony patch:
> ```csharp
> // WRONG! This will multiply every frame or every time a slider moves!
> __instance.fuelRate *= ConfigMultiplier.Value; 
> ```
> In Unity, object state persists. If a player slides a slider back and forth, or if a method is called repeatedly, the value will exponentially explode (`1.5 * 1.5 * 1.5...`).

### The Safe Pattern: Baseline Memory (`OriginalValueStore`)

Always store the vanilla value on first encounter and calculate from the baseline:

```csharp
using HarmonyLib;
using Milex.GMS1.Mods.QuickFuel.Helpers;

[HarmonyPatch(typeof(FuelStation), "UpdateTransfer")]
public static class FuelTransferPatch
{
    [HarmonyPrefix]
    public static void Prefix(FuelStation __instance)
    {
        if (!QuickFuelPlugin.Instance.IsEnabled) return;

        // 1. Get or register pristine vanilla baseline
        float vanillaRate = OriginalValueStore.GetOrRegisterFloat(
            __instance,
            "TransferRate",
            __instance.transferSpeed,
            (inst, original) => ((FuelStation)inst).transferSpeed = original
        );

        // 2. Compute current rate strictly from vanilla baseline
        __instance.transferSpeed = vanillaRate * QuickFuelPlugin.FuelTransferSpeed.Value;
    }
}
```

### Reverting on Mod Disable
When the player turns your mod off via the Mod Menu, `ModBase` removes all Harmony patches and calls `OnModDisabled()`. Revert your tracked instances:

```csharp
protected override void OnModDisabled()
{
    OriginalValueStore.RestoreAll();
    LogInfo("Quick Fuel disabled. Original game values restored.");
}
```

---

## 7. Multi-Language Localization

CoreMod provides a complete, automatic localization system.

### 7.1 Key Naming Standard
All config labels and descriptions can be localized using this naming pattern:

- **Section Header**: `config.<section_lowercase>.section`
- **Setting Name**: `config.<section_lowercase>.<key_lowercase>.name`
- **Setting Description**: `config.<section_lowercase>.<key_lowercase>.desc`

### 7.2 Creating Embedded Localization Files

Create a `Localization` folder in your mod project and add:
- `Milex_GMS1_<YourModName>_en.json` (English template)
- `Milex_GMS1_<YourModName>_de.json` (German template)

#### Example `Milex_GMS1_QuickFuel_en.json`:
```json
{
  "config.logistics.section": "Logistics & Fuel",
  "config.logistics.transferspeed.name": "Fuel Transfer Speed",
  "config.logistics.transferspeed.desc": "Multiplier for fuel pumping speed.",
  "config.generators.section": "Power Generators",
  "config.generators.infinitefuel.name": "Infinite Generator Fuel",
  "config.generators.infinitefuel.desc": "Prevents generators from running out of fuel.",
  "log.ready": "Quick Fuel mod loaded and ready.",
  "log.refuel_done": "Equipment fully refueled!"
}
```

#### Example `Milex_GMS1_QuickFuel_de.json`:
```json
{
  "config.logistics.section": "Logistik & Treibstoff",
  "config.logistics.transferspeed.name": "Betankungs-Geschwindigkeit",
  "config.logistics.transferspeed.desc": "Multiplikator für die Pump-Geschwindigkeit beim Betanken.",
  "config.generators.section": "Generatoren",
  "config.generators.infinitefuel.name": "Unendlicher Generator-Treibstoff",
  "config.generators.infinitefuel.desc": "Verhindert, dass Generatoren Treibstoff verbrauchen.",
  "log.ready": "Quick Fuel Mod geladen und einsatzbereit.",
  "log.refuel_done": "Gerät erfolgreich betankt!"
}
```

In your code, access translations anytime using:
```csharp
string msg = Translate("log.refuel_done", "Equipment fully refueled!");
```

---

## 8. Helper Methods & Core API Reference

Your mod inherits several helpful methods from `ModBase`:

| Method / Property | Description |
|---|---|
| `IsEnabled` | Returns `true` if the mod is currently active. |
| `Config` | Gets your mod's `ConfigFile` (`BepInEx/config/<AssemblyName>.cfg`). |
| `Translate(key, fallback)` | Translates a key using your mod's localization tables. |
| `LogInfo(msg)` | Logs an informational message prefixed with `[<ModName>]`. |
| `LogWarning(msg)` | Logs a warning message prefixed with `[<ModName>]`. |
| `LogError(msg)` | Logs an error message prefixed with `[<ModName>]`. |
| `SetEnabled(bool)` | Programmatically toggles the mod's active state. |
| `OnModEnabled()` | Virtual callback invoked after the mod is re-enabled. |
| `OnModDisabled()` | Virtual callback invoked after the mod is disabled. |

---

## 9. Best Practices & Modding Etiquette

1. **Strict English in Code & Docs**: Write all C# identifiers, comments, BepInEx descriptions, and documentation in English.
2. **Resource Neutrality**: If your mod increases machine speeds, ensure electrical generators and water pumps are not overloaded unintentionally.
3. **No UI Emojis**: IMGUI and Unity dynamic fonts frequently corrupt Unicode emoji symbols. Use clean ASCII or alphanumeric text instead.
4. **Clean Readme & Changelog**: Every mod should provide an informative `README.md` and `CHANGELOG.md`.

---

## 10. Compiling & Publishing

### Build
Run `dotnet build` from the repository root:
```powershell
dotnet build GMSModding.sln
```

### Files to Distribute
To share your mod with players on NexusMods or GitHub:
1. `BepInEx/plugins/Milex_GMS1_<YourModName>.dll`
2. Remind users that `Milex_GMS1_CoreMod.dll` is required!
