using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales Hog Pan dirt capacity while protecting water consumption rate from accelerating.
    /// Anchors water loss to the vanilla baseline so water lasts normally regardless of capacity multipliers.
    /// </summary>
    [HarmonyPatch]
    public static class HogPanDirtBoxPatch
    {
        private static FieldInfo _planeVolumeMaxField;
        private static FieldInfo _waterVolumeField;

        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("GoldDigger.HogPanDirtBox");
            return type != null ? AccessTools.Method(type, "Update") : null;
        }

        [HarmonyPostfix]
        public static void UpdatePostfix(object __instance)
        {
            if (__instance == null) return;

            Type type = __instance.GetType();
            if (_planeVolumeMaxField == null) _planeVolumeMaxField = FieldCache.GetField(type, "PlaneVolumeMax");
            if (_planeVolumeMaxField == null) return;

            float currentCap = (float)_planeVolumeMaxField.GetValue(__instance);
            float baseCap = OriginalValueStore.GetOrRegisterFloat(__instance, "HogPan_PlaneVolumeMax", currentCap, (obj, val) =>
                _planeVolumeMaxField.SetValue(obj, val));

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.HogPanCapacityMultiplier
                : 1f;

            float targetCap = baseCap * multiplier;
            if (Math.Abs(currentCap - targetCap) > 0.001f)
            {
                _planeVolumeMaxField.SetValue(__instance, targetCap);
            }
        }

        /// <summary>
        /// Sub-patch on ProcessPlane to refund excess water drainage caused by the enlarged PlaneVolumeMax.
        /// </summary>
        [HarmonyPatch]
        public static class ProcessPlaneWaterGuardPatch
        {
            [HarmonyTargetMethod]
            private static MethodBase TargetMethod()
            {
                Type type = AccessTools.TypeByName("GoldDigger.HogPanDirtBox");
                return type != null ? AccessTools.Method(type, "ProcessPlane") : null;
            }

            [HarmonyPostfix]
            public static void Postfix(object __instance)
            {
                if (__instance == null) return;

                Type type = __instance.GetType();
                if (_planeVolumeMaxField == null) _planeVolumeMaxField = FieldCache.GetField(type, "PlaneVolumeMax");
                if (_waterVolumeField == null) _waterVolumeField = FieldCache.GetField(type, "WaterVolume");
                if (_planeVolumeMaxField == null || _waterVolumeField == null) return;

                float currentCap = (float)_planeVolumeMaxField.GetValue(__instance);
                float baseCap = OriginalValueStore.GetOrRegisterFloat(__instance, "HogPan_PlaneVolumeMax", currentCap, null);

                // If capacity is enlarged, compensate excess water drain:
                // Vanilla drain = Time.deltaTime * (PlaneVolumeMax / 7.5f)
                // Desired drain = Time.deltaTime * (BasePlaneVolumeMax / 7.5f)
                if (currentCap > baseCap)
                {
                    float excessDrain = Time.deltaTime * ((currentCap - baseCap) / 7.5f);
                    float currentWater = (float)_waterVolumeField.GetValue(__instance);
                    _waterVolumeField.SetValue(__instance, currentWater + excessDrain);
                }
            }
        }
    }
}
