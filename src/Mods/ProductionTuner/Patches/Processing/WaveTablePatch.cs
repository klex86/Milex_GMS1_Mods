using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Processing
{
    /// <summary>
    /// Scales Wave Table concentrate capacity (MaxGroundVolume) and wash cycle speed.
    /// Save/load safe: Start() prefix ensures the capacity multiplier is applied before vanilla
    /// code can clamp the serialized CurrentGroundVolume against vanilla MaxGroundVolume on load.
    /// </summary>
    public static class WaveTablePatch
    {
        private static readonly Dictionary<int, (GoldDigger.WaveTable instance, float baseVol)> Tracked =
            new Dictionary<int, (GoldDigger.WaveTable, float)>();
        private static readonly AccessTools.FieldRef<GoldDigger.WaveTable, float> ElapsedTimeRef =
            AccessTools.FieldRefAccess<GoldDigger.WaveTable, float>("_ElapsedTimeThrow");

        private static float _lastMultiplier = -1f;

        // -------------------------------------------------------------------------
        // Start() Prefix — apply capacity multiplier before vanilla code runs to prevent
        // CurrentGroundVolume being clamped against vanilla MaxGroundVolume on load.
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.WaveTable), "Start")]
        public static class WaveTableStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.WaveTable __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();
                if (Tracked.ContainsKey(id)) return;
                float baseVol = __instance.MaxGroundVolume;
                float capMult = ProductionTunerPlugin.Service?.WaveTableCapacityMultiplier ?? 1f;
                Tracked[id] = (__instance, baseVol);
                __instance.MaxGroundVolume = baseVol * capMult;
                _lastMultiplier = capMult;
            }
        }

        // -------------------------------------------------------------------------
        // Update() Prefix — advance elapsed timer for speed multiplier
        // Update() Postfix — fast-path for live capacity multiplier changes
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.WaveTable), "Update")]
        public static class WaveTableUpdatePatch
        {
            [HarmonyPrefix]
            public static void Prefix(GoldDigger.WaveTable __instance)
            {
                if (__instance == null || ElapsedTimeRef == null) return;

                float spdMultiplier = ProductionTunerPlugin.Service?.WaveTableSpeedMultiplier ?? 1f;

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

                float capMultiplier = ProductionTunerPlugin.Service?.WaveTableCapacityMultiplier ?? 1f;
                int id = __instance.GetInstanceID();

                // Zero-allocation fast-path
                if (Tracked.TryGetValue(id, out var data))
                {
                    if (capMultiplier == _lastMultiplier) return;
                    __instance.MaxGroundVolume = data.baseVol * capMultiplier;
                    _lastMultiplier = capMultiplier;
                    return;
                }

                // Fallback registration
                float baseVol = __instance.MaxGroundVolume;
                Tracked[id] = (__instance, baseVol);
                __instance.MaxGroundVolume = baseVol * capMultiplier;
                _lastMultiplier = capMultiplier;
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var kvp in Tracked.Values)
            {
                if (kvp.instance != null)
                    kvp.instance.MaxGroundVolume = kvp.baseVol;
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
