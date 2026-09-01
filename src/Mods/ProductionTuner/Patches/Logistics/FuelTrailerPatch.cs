using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales mobile fuel trailer capacity and refuel pump speed proportionally with zero frame rate impact.
    /// </summary>
    public static class FuelTrailerPatch
    {
        private static readonly Dictionary<int, float> BaseCapacities = new Dictionary<int, float>();
        private static float _lastMultiplier = -1f;

        [HarmonyPatch(typeof(GoldDigger.FuelStationController), "Update")]
        public static class FuelStationSubPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.FuelStationController __instance)
            {
                if (__instance == null) return;

                float curCap = __instance.MaxCapacity;
                int id = __instance.GetInstanceID();

                float multiplier = ProductionTunerPlugin.Service != null
                    ? ProductionTunerPlugin.Service.FuelTrailerCapacityMultiplier
                    : 1f;

                // Zero-allocation fast-path
                if (BaseCapacities.TryGetValue(id, out float baseCap))
                {
                    if (multiplier == _lastMultiplier) return;
                    __instance.MaxCapacity = baseCap * multiplier;
                    return;
                }

                // Detect mobile fuel trailer via GameObject hierarchy and nominal 1000L baseline
                bool isTrailer = __instance.GetComponentInParent<GoldDigger.Trailer>() != null;
                if (__instance.gameObject.name.Contains("End_Bottom") || (Mathf.Approximately(curCap, 1000f) && isTrailer))
                {
                    BaseCapacities[id] = curCap;
                    __instance.MaxCapacity = curCap * multiplier;
                    _lastMultiplier = multiplier;
                }
            }
        }

        [HarmonyPatch(typeof(GoldDigger.FuelPistolHoldable), "Attach")]
        public static class FuelPistolSubPatch
        {
            private static readonly Dictionary<int, float> BaseSpeeds = new Dictionary<int, float>();

            [HarmonyPostfix]
            public static void Postfix(GoldDigger.FuelPistolHoldable __instance)
            {
                if (__instance == null) return;

                int id = __instance.GetInstanceID();
                if (!BaseSpeeds.TryGetValue(id, out float baseSpeed))
                {
                    baseSpeed = __instance.TankingSpeed;
                    BaseSpeeds[id] = baseSpeed;
                }

                float multiplier = ProductionTunerPlugin.Service != null
                    ? ProductionTunerPlugin.Service.FuelTrailerCapacityMultiplier
                    : 1f;

                __instance.TankingSpeed = baseSpeed * Mathf.Max(1f, multiplier);
            }
        }

        public static void Reset()
        {
            BaseCapacities.Clear();
            _lastMultiplier = -1f;
        }
    }
}
