using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Processing
{
    /// <summary>
    /// Scales Wave Table concentrate capacity (MaxGroundVolume) and wash cycle speed.
    /// Uses high-speed field reference and instant fast-path exit.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.WaveTable), "Update")]
    public static class WaveTablePatch
    {
        private static readonly Dictionary<int, float> BaseVolumes = new Dictionary<int, float>();
        private static readonly AccessTools.FieldRef<GoldDigger.WaveTable, float> ElapsedTimeRef =
            AccessTools.FieldRefAccess<GoldDigger.WaveTable, float>("_ElapsedTimeThrow");

        private static float _lastMultiplier = -1f;

        [HarmonyPrefix]
        public static void Prefix(GoldDigger.WaveTable __instance)
        {
            if (__instance == null || ElapsedTimeRef == null) return;

            float spdMultiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.WaveTableSpeedMultiplier
                : 1f;

            if (spdMultiplier > 1f)
            {
                ref float elapsed = ref ElapsedTimeRef(__instance);
                if (elapsed > 0f)
                {
                    elapsed -= Time.deltaTime * (spdMultiplier - 1f);
                }
            }
        }

        [HarmonyPostfix]
        public static void Postfix(GoldDigger.WaveTable __instance)
        {
            if (__instance == null) return;

            float capMultiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.WaveTableCapacityMultiplier
                : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (BaseVolumes.TryGetValue(id, out float baseVol))
            {
                if (capMultiplier == _lastMultiplier) return;
                __instance.MaxGroundVolume = baseVol * capMultiplier;
                return;
            }

            baseVol = __instance.MaxGroundVolume;
            BaseVolumes[id] = baseVol;
            __instance.MaxGroundVolume = baseVol * capMultiplier;
            _lastMultiplier = capMultiplier;
        }

        public static void Reset()
        {
            BaseVolumes.Clear();
            _lastMultiplier = -1f;
        }
    }
}
