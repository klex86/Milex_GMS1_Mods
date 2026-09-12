using System;
using HarmonyLib;
using Milex.GMS1.Core.Services;

namespace Milex.GMS1.Core.Patches
{
    /// <summary>
    /// Harmony hooks to trigger memory & VRAM cleaning during player-imperceptible game events:
    /// 1. Fast Travel loading screen (MapMenu.FastTravel)
    /// 2. Saving the game (CheckpointManager.QuickSave / Autosave)
    /// 3. Opening the Laptop / Tablet computer (LaptopManager.OnEnable)
    /// </summary>
    public static class MemoryCleanerPatches
    {
        [HarmonyPatch(typeof(GoldDigger.MapMenu), "FastTravel")]
        public static class FastTravelHook
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                try
                {
                    if (CorePlugin.CleanOnFastTravel != null && CorePlugin.CleanOnFastTravel.Value)
                    {
                        MemoryCleanerService.Instance?.Clean(force: false, triggerSource: "FastTravel");
                    }
                }
                catch (Exception ex)
                {
                    CorePlugin.Instance?.LogWarning($"[MemoryCleaner] FastTravel hook error: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(CheckpointManager), "QuickSave")]
        public static class QuickSaveHook
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                try
                {
                    if (CorePlugin.CleanOnSave != null && CorePlugin.CleanOnSave.Value)
                    {
                        MemoryCleanerService.Instance?.Clean(force: false, triggerSource: "QuickSave");
                    }
                }
                catch (Exception ex)
                {
                    CorePlugin.Instance?.LogWarning($"[MemoryCleaner] QuickSave hook error: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(CheckpointManager), "Autosave")]
        public static class AutosaveHook
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                try
                {
                    if (CorePlugin.CleanOnSave != null && CorePlugin.CleanOnSave.Value)
                    {
                        MemoryCleanerService.Instance?.Clean(force: false, triggerSource: "Autosave");
                    }
                }
                catch (Exception ex)
                {
                    CorePlugin.Instance?.LogWarning($"[MemoryCleaner] Autosave hook error: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(LaptopManager), "OnEnable")]
        public static class LaptopOnEnableHook
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                try
                {
                    if (CorePlugin.CleanOnLaptop != null && CorePlugin.CleanOnLaptop.Value)
                    {
                        MemoryCleanerService.Instance?.Clean(force: false, triggerSource: "Laptop");
                    }
                }
                catch (Exception ex)
                {
                    CorePlugin.Instance?.LogWarning($"[MemoryCleaner] Laptop hook error: {ex.Message}");
                }
            }
        }
    }
}
