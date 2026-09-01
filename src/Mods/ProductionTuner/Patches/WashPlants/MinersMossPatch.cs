using System.Collections.Generic;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales dirt and mineral sediment capacity for Miner's Moss mats.
    /// Employs a zero-allocation fast exit path to guarantee 0 FPS overhead in runtime loops.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.MinersMoss), "Update")]
    public static class MinersMossPatch
    {
        private static readonly Dictionary<int, float> BaseVolumes = new Dictionary<int, float>();
        private static float _lastMultiplier = -1f;

        [HarmonyPostfix]
        public static void Postfix(GoldDigger.MinersMoss __instance)
        {
            if (__instance == null) return;

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.MinersMossCapacityMultiplier
                : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path: if already tracked and multiplier has not changed, exit immediately
            if (BaseVolumes.TryGetValue(id, out float baseVol))
            {
                if (multiplier == _lastMultiplier) return;
                __instance.MaxGroundVolume = baseVol * multiplier;
                return;
            }

            if (OrangeBeastFilter.IsOrangeBeastPart(__instance)) return;

            baseVol = __instance.MaxGroundVolume;
            BaseVolumes[id] = baseVol;
            __instance.MaxGroundVolume = baseVol * multiplier;
            _lastMultiplier = multiplier;
        }

        public static void Reset()
        {
            BaseVolumes.Clear();
            _lastMultiplier = -1f;
        }
    }
}
