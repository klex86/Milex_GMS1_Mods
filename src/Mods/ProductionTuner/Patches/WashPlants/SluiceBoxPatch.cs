using System.Collections.Generic;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales dirt capacity for washplant sluice box nugget trap grates using verified vanilla baseline (0.007f).
    /// Synchronized with the T3-T5 wash plant setup multiplier.
    /// Excludes Orange Beast components and prevents multi-instance desynchronization.
    /// </summary>
    public static class SluiceBoxPatch
    {
        public const float VanillaMaxFill = 0.007f;

        private static readonly Dictionary<int, GoldDigger.WashPlantSluiceBoxDirt> Tracked =
            new Dictionary<int, GoldDigger.WashPlantSluiceBoxDirt>();
        private static float _lastMultiplier = -1f;

        // -------------------------------------------------------------------------
        // Start() Postfix — apply multiplier on spawn/load before vanilla clamping
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.WashPlantSluiceBoxDirt), "Start")]
        public static class SluiceBoxStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.WashPlantSluiceBoxDirt __instance)
            {
                if (__instance == null || OrangeBeastFilter.IsOrangeBeastPart(__instance)) return;

                int id = __instance.GetInstanceID();
                Tracked[id] = __instance;

                float multiplier = ProductionTunerPlugin.Service?.WashplantT3T5SetupCapacityMultiplier ?? 1f;
                __instance.MaxFill = VanillaMaxFill * multiplier;
            }
        }

        // -------------------------------------------------------------------------
        // Update() Postfix — Live multiplier changes in the in-game menu
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.WashPlantSluiceBoxDirt), "Update")]
        public static class SluiceBoxUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.WashPlantSluiceBoxDirt __instance)
            {
                if (__instance == null || OrangeBeastFilter.IsOrangeBeastPart(__instance)) return;

                int id = __instance.GetInstanceID();
                Tracked[id] = __instance;

                float multiplier = ProductionTunerPlugin.Service?.WashplantT3T5SetupCapacityMultiplier ?? 1f;
                if (multiplier != _lastMultiplier)
                {
                    _lastMultiplier = multiplier;
                    foreach (var sluice in Tracked.Values)
                    {
                        if (sluice != null)
                        {
                            sluice.MaxFill = VanillaMaxFill * multiplier;
                        }
                    }
                }
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var sluice in Tracked.Values)
            {
                if (sluice != null)
                {
                    sluice.MaxFill = VanillaMaxFill;
                }
            }
            Tracked.Clear();
            _lastMultiplier = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
        }
    }
}
