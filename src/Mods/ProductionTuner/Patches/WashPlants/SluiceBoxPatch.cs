using System.Collections.Generic;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales dirt capacity for washplant sluice boxes.
    /// Employs a zero-allocation fast exit path to guarantee 0 FPS overhead in runtime loops.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.WashPlantSluiceBoxDirt), "Update")]
    public static class SluiceBoxPatch
    {
        private static readonly Dictionary<int, float> BaseFills = new Dictionary<int, float>();
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
            if (BaseFills.TryGetValue(id, out float baseFill))
            {
                if (multiplier == _lastMultiplier) return;
                __instance.MaxFill = baseFill * multiplier;
                return;
            }

            if (OrangeBeastFilter.IsOrangeBeastPart(__instance)) return;

            baseFill = __instance.MaxFill;
            BaseFills[id] = baseFill;
            __instance.MaxFill = baseFill * multiplier;
            _lastMultiplier = multiplier;
        }

        public static void Reset()
        {
            BaseFills.Clear();
            _lastMultiplier = -1f;
        }
    }
}
