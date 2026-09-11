using System.Collections.Generic;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales capacity (MaxFill) and processing speed (FillSpeed) for all large washplant shakers (Tier 1-Tier 4)
    /// using pristine vanilla constants (15.0f MaxFill, 1.0f FillSpeed) from Assembly-CSharp.
    /// Excludes Orange Beast components and prevents savegame drift.
    /// </summary>
    public static class WashPlantShakerPatch
    {
        /// Authentic vanilla baselines from Unity prefab dump: MaxFill = 40.0 m3, FillSpeed = 0.55 m3/s
        public const float VanillaMaxFill = 40.0f;
        public const float VanillaFillSpeed = 0.55f;

        private static readonly Dictionary<int, GoldDigger.WashplantShakerBase> Tracked =
            new Dictionary<int, GoldDigger.WashplantShakerBase>();
        private static float _lastCapMultiplier = -1f;
        private static float _lastSpdMultiplier = -1f;

        // -------------------------------------------------------------------------
        // Start() Postfix — apply multipliers on spawn/load before vanilla clamping
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.WashplantShakerBase), "Start")]
        public static class WashPlantShakerStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.WashplantShakerBase __instance)
            {
                if (__instance == null || OrangeBeastFilter.IsOrangeBeastPart(__instance)) return;

                int id = __instance.GetInstanceID();
                Tracked[id] = __instance;

                float capMult = ProductionTunerPlugin.Service?.WashplantCapacityMultiplier ?? 1f;
                float spdMult = ProductionTunerPlugin.Service?.WashplantSpeedMultiplier ?? 1f;

                __instance.MaxFill = VanillaMaxFill * capMult;
                __instance.FillSpeed = VanillaFillSpeed * spdMult;
            }
        }

        // -------------------------------------------------------------------------
        // Update() Postfix — Live multiplier changes in the in-game menu
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.WashplantShakerBase), "Update")]
        public static class WashPlantShakerUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.WashplantShakerBase __instance)
            {
                if (__instance == null || OrangeBeastFilter.IsOrangeBeastPart(__instance)) return;

                int id = __instance.GetInstanceID();
                Tracked[id] = __instance;

                float capMultiplier = ProductionTunerPlugin.Service?.WashplantCapacityMultiplier ?? 1f;
                float spdMultiplier = ProductionTunerPlugin.Service?.WashplantSpeedMultiplier ?? 1f;

                if (capMultiplier != _lastCapMultiplier || spdMultiplier != _lastSpdMultiplier)
                {
                    _lastCapMultiplier = capMultiplier;
                    _lastSpdMultiplier = spdMultiplier;

                    foreach (var shaker in Tracked.Values)
                    {
                        if (shaker != null)
                        {
                            shaker.MaxFill = VanillaMaxFill * capMultiplier;
                            shaker.FillSpeed = VanillaFillSpeed * spdMultiplier;
                        }
                    }
                }
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var shaker in Tracked.Values)
            {
                if (shaker != null)
                {
                    shaker.MaxFill = VanillaMaxFill;
                    shaker.FillSpeed = VanillaFillSpeed;
                }
            }
            Tracked.Clear();
            _lastCapMultiplier = 1f;
            _lastSpdMultiplier = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
        }
    }
}
