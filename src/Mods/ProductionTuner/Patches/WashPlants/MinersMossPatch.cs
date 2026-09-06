using System.Collections.Generic;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales dirt and mineral sediment capacity for Miner's Moss mats using pristine vanilla constant (10.0f) from Assembly-CSharp.
    /// Excludes Orange Beast components and prevents multi-instance desynchronization.
    /// </summary>
    public static class MinersMossPatch
    {
        public const float VanillaMaxGroundVolume = 10.0f;

        private static readonly Dictionary<int, GoldDigger.MinersMoss> Tracked =
            new Dictionary<int, GoldDigger.MinersMoss>();
        private static float _lastMultiplier = -1f;

        // -------------------------------------------------------------------------
        // Start() Postfix — apply multiplier on spawn/load before vanilla clamping
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.MinersMoss), "Start")]
        public static class MinersMossStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MinersMoss __instance)
            {
                if (__instance == null || OrangeBeastFilter.IsOrangeBeastPart(__instance)) return;

                int id = __instance.GetInstanceID();
                Tracked[id] = __instance;

                float multiplier = ProductionTunerPlugin.Service?.MinersMossCapacityMultiplier ?? 1f;
                __instance.MaxGroundVolume = VanillaMaxGroundVolume * multiplier;
            }
        }

        // -------------------------------------------------------------------------
        // Update() Postfix — Live multiplier changes in the in-game menu
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.MinersMoss), "Update")]
        public static class MinersMossUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MinersMoss __instance)
            {
                if (__instance == null || OrangeBeastFilter.IsOrangeBeastPart(__instance)) return;

                int id = __instance.GetInstanceID();
                Tracked[id] = __instance;

                float multiplier = ProductionTunerPlugin.Service?.MinersMossCapacityMultiplier ?? 1f;
                if (multiplier != _lastMultiplier)
                {
                    _lastMultiplier = multiplier;
                    foreach (var moss in Tracked.Values)
                    {
                        if (moss != null)
                        {
                            moss.MaxGroundVolume = VanillaMaxGroundVolume * multiplier;
                        }
                    }
                }
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var moss in Tracked.Values)
            {
                if (moss != null)
                {
                    moss.MaxGroundVolume = VanillaMaxGroundVolume;
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
