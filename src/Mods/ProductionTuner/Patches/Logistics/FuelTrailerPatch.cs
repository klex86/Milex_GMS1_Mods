using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales mobile fuel trailer capacity and refuel pump speed proportionally.
    /// Identifies the trailer through its specific fuel station controller naming and baseline capacity.
    /// </summary>
    public static class FuelTrailerPatch
    {
        [HarmonyPatch]
        public static class FuelStationSubPatch
        {
            [HarmonyTargetMethod]
            private static MethodBase TargetMethod()
            {
                Type type = AccessTools.TypeByName("GoldDigger.FuelStationController");
                return type != null ? AccessTools.Method(type, "Start") : null;
            }

            [HarmonyPostfix]
            public static void Postfix(object __instance)
            {
                if (__instance == null || !(__instance is Component comp)) return;

                Type type = __instance.GetType();
                FieldInfo maxCapField = FieldCache.GetField(type, "MaxCapacity");
                if (maxCapField == null) return;

                float curCap = (float)maxCapField.GetValue(__instance);

                // Detect mobile fuel trailer via GameObject hierarchy and nominal 1000L baseline
                Type trailerType = AccessTools.TypeByName("GoldDigger.Trailer");
                bool isTrailer = trailerType != null && comp.GetComponentInParent(trailerType) != null;
                if (comp.gameObject.name.Contains("End_Bottom") || (Mathf.Approximately(curCap, 1000f) && isTrailer))
                {
                    float baseCap = OriginalValueStore.GetOrRegisterFloat(__instance, "FuelTrailer_MaxCapacity", curCap, (obj, val) =>
                        maxCapField.SetValue(obj, val));

                    float multiplier = ProductionTunerPlugin.Service != null
                        ? ProductionTunerPlugin.Service.FuelTrailerCapacityMultiplier
                        : 1f;

                    maxCapField.SetValue(__instance, baseCap * multiplier);
                }
            }
        }

        [HarmonyPatch]
        public static class FuelPistolSubPatch
        {
            [HarmonyTargetMethod]
            private static MethodBase TargetMethod()
            {
                Type type = AccessTools.TypeByName("GoldDigger.FuelPistolHoldable");
                return type != null ? AccessTools.Method(type, "Attach") : null;
            }

            [HarmonyPostfix]
            public static void Postfix(object __instance)
            {
                if (__instance == null) return;

                Type type = __instance.GetType();
                FieldInfo speedField = FieldCache.GetField(type, "TankingSpeed");
                if (speedField == null) return;

                float curSpeed = (float)speedField.GetValue(__instance);
                float baseSpeed = OriginalValueStore.GetOrRegisterFloat(__instance, "FuelPistol_TankingSpeed", curSpeed, (obj, val) =>
                    speedField.SetValue(obj, val));

                float multiplier = ProductionTunerPlugin.Service != null
                    ? ProductionTunerPlugin.Service.FuelTrailerCapacityMultiplier
                    : 1f;

                // Scale pump speed proportionally with capacity multiplier so fueling is not excessively tedious
                speedField.SetValue(__instance, baseSpeed * Mathf.Max(1f, multiplier));
            }
        }
    }
}
