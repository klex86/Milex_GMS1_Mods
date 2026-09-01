using System.Collections.Generic;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales dirt capacity for washplant sluice boxes.
    /// Employs a zero-allocation fast exit path and clean vanilla state restoration.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.WashPlantSluiceBoxDirt), "Update")]
    public static class SluiceBoxPatch
    {
        private static readonly Dictionary<int, (GoldDigger.WashPlantSluiceBoxDirt instance, float baseFill)> Tracked =
            new Dictionary<int, (GoldDigger.WashPlantSluiceBoxDirt, float)>();
        private static float _lastMultiplier = -1f;

        [HarmonyPostfix]
        public static void Postfix(GoldDigger.WashPlantSluiceBoxDirt __instance)
        {
            if (__instance == null) return;

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.SluiceboxCapacityMultiplier
                : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (Tracked.TryGetValue(id, out var data))
            {
                if (multiplier == _lastMultiplier) return;
                __instance.MaxFill = data.baseFill * multiplier;
                return;
            }

            if (OrangeBeastFilter.IsOrangeBeastPart(__instance)) return;

            float baseFill = __instance.MaxFill;
            Tracked[id] = (__instance, baseFill);
            __instance.MaxFill = baseFill * multiplier;
            _lastMultiplier = multiplier;
        }

        public static void RestoreVanilla()
        {
            foreach (var kvp in Tracked.Values)
            {
                if (kvp.instance != null)
                {
                    kvp.instance.MaxFill = kvp.baseFill;
                }
            }
            _lastMultiplier = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
            Tracked.Clear();
            _lastMultiplier = -1f;
        }
    }
}
