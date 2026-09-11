using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Processing
{
    /// <summary>
    /// Scales Wave Table concentrate capacity (MaxGroundVolume) and wash cycle speed
    /// using pristine vanilla constant (0.6f) from Assembly-CSharp.
    /// Prevents savegame drift and multi-instance desynchronization.
    /// </summary>
    public static class WaveTablePatch
    {
        /// Authentic vanilla baseline from Unity prefab dump: 0.06 m3 (exactly 2 standard 0.03 m3 buckets).
        public const float VanillaMaxGroundVolume = 0.06f;

        private static readonly Dictionary<int, GoldDigger.WaveTable> Tracked =
            new Dictionary<int, GoldDigger.WaveTable>();
        private static readonly AccessTools.FieldRef<GoldDigger.WaveTable, float> ElapsedTimeRef =
            AccessTools.FieldRefAccess<GoldDigger.WaveTable, float>("_ElapsedTimeThrow");

        private static float _lastMultiplier = -1f;

        // -------------------------------------------------------------------------
        // Start() Postfix — apply capacity multiplier on spawn/load before vanilla clamping
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.WaveTable), "Start")]
        public static class WaveTableStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.WaveTable __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();
                Tracked[id] = __instance;

                float capMult = ProductionTunerPlugin.Service?.WaveTableCapacityMultiplier ?? 1f;
                __instance.MaxGroundVolume = VanillaMaxGroundVolume * capMult;
            }
        }

        // -------------------------------------------------------------------------
        // Update() Prefix — advance elapsed timer for speed multiplier
        // Update() Postfix — Live capacity multiplier changes in the in-game menu
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

                int id = __instance.GetInstanceID();
                Tracked[id] = __instance;

                float capMultiplier = ProductionTunerPlugin.Service?.WaveTableCapacityMultiplier ?? 1f;
                if (capMultiplier != _lastMultiplier)
                {
                    _lastMultiplier = capMultiplier;
                    foreach (var table in Tracked.Values)
                    {
                        if (table != null)
                        {
                            table.MaxGroundVolume = VanillaMaxGroundVolume * capMultiplier;
                        }
                    }
                }
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var table in Tracked.Values)
            {
                if (table != null)
                {
                    table.MaxGroundVolume = VanillaMaxGroundVolume;
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
