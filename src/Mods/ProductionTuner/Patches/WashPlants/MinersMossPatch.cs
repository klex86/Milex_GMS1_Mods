using System.Collections.Generic;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales dirt and mineral sediment capacity for Miner's Moss mats.
    /// Employs a zero-allocation fast exit path and clean vanilla state restoration.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.MinersMoss), "Update")]
    public static class MinersMossPatch
    {
        private static readonly Dictionary<int, (GoldDigger.MinersMoss instance, float baseVol)> Tracked =
            new Dictionary<int, (GoldDigger.MinersMoss, float)>();
        private static float _lastMultiplier = -1f;

        [HarmonyPostfix]
        public static void Postfix(GoldDigger.MinersMoss __instance)
        {
            if (__instance == null) return;

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.MinersMossCapacityMultiplier
                : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (Tracked.TryGetValue(id, out var data))
            {
                if (multiplier == _lastMultiplier) return;
                __instance.MaxGroundVolume = data.baseVol * multiplier;
                return;
            }

            if (OrangeBeastFilter.IsOrangeBeastPart(__instance)) return;

            float baseVol = __instance.MaxGroundVolume;
            Tracked[id] = (__instance, baseVol);
            __instance.MaxGroundVolume = baseVol * multiplier;
            _lastMultiplier = multiplier;
        }

        public static void RestoreVanilla()
        {
            foreach (var kvp in Tracked.Values)
            {
                if (kvp.instance != null)
                {
                    kvp.instance.MaxGroundVolume = kvp.baseVol;
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
