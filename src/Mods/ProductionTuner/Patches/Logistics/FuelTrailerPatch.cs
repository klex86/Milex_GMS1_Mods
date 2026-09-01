using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales mobile fuel trailer capacity and refuel pump speed proportionally with zero frame rate impact
    /// and clean vanilla state restoration.
    /// </summary>
    public static class FuelTrailerPatch
    {
        private static readonly Dictionary<int, (GoldDigger.FuelStationController instance, float baseCap)> TrackedStations =
            new Dictionary<int, (GoldDigger.FuelStationController, float)>();

        private static readonly Dictionary<int, (GoldDigger.FuelPistolHoldable instance, float baseSpeed)> TrackedPistols =
            new Dictionary<int, (GoldDigger.FuelPistolHoldable, float)>();

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
                if (TrackedStations.TryGetValue(id, out var data))
                {
                    if (multiplier == _lastMultiplier) return;
                    __instance.MaxCapacity = data.baseCap * multiplier;
                    return;
                }

                // Detect mobile fuel trailer via GameObject hierarchy and nominal 1000L baseline
                bool isTrailer = __instance.GetComponentInParent<GoldDigger.Trailer>() != null;
                if (__instance.gameObject.name.Contains("End_Bottom") || (Mathf.Approximately(curCap, 1000f) && isTrailer))
                {
                    TrackedStations[id] = (__instance, curCap);
                    __instance.MaxCapacity = curCap * multiplier;
                    _lastMultiplier = multiplier;
                }
            }
        }

        [HarmonyPatch(typeof(GoldDigger.FuelPistolHoldable), "Attach")]
        public static class FuelPistolSubPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.FuelPistolHoldable __instance)
            {
                if (__instance == null) return;

                int id = __instance.GetInstanceID();
                if (!TrackedPistols.TryGetValue(id, out var data))
                {
                    data = (__instance, __instance.TankingSpeed);
                    TrackedPistols[id] = data;
                }

                float multiplier = ProductionTunerPlugin.Service != null
                    ? ProductionTunerPlugin.Service.FuelTrailerCapacityMultiplier
                    : 1f;

                __instance.TankingSpeed = data.baseSpeed * Mathf.Max(1f, multiplier);
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var kvp in TrackedStations.Values)
            {
                if (kvp.instance != null)
                {
                    kvp.instance.MaxCapacity = kvp.baseCap;
                }
            }
            foreach (var kvp in TrackedPistols.Values)
            {
                if (kvp.instance != null)
                {
                    kvp.instance.TankingSpeed = kvp.baseSpeed;
                }
            }
            _lastMultiplier = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
            TrackedStations.Clear();
            TrackedPistols.Clear();
            _lastMultiplier = -1f;
        }
    }
}
