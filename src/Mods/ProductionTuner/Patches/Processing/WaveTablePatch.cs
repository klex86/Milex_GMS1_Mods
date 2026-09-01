using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Processing
{
    /// <summary>
    /// Scales Wave Table concentrate capacity (MaxGroundVolume) and wash cycle speed.
    /// Accelerates cycle timer proportionally without invasive bytecode transpilers.
    /// </summary>
    [HarmonyPatch]
    public static class WaveTablePatch
    {
        private static FieldInfo _maxGroundVolumeField;
        private static FieldInfo _elapsedTimeThrowField;

        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("GoldDigger.WaveTable");
            return type != null ? AccessTools.Method(type, "Update") : null;
        }

        [HarmonyPrefix]
        public static void Prefix(object __instance)
        {
            if (__instance == null) return;

            Type type = __instance.GetType();
            if (_elapsedTimeThrowField == null) _elapsedTimeThrowField = FieldCache.GetField(type, "_ElapsedTimeThrow");
            if (_elapsedTimeThrowField == null) return;

            float spdMultiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.WaveTableSpeedMultiplier
                : 1f;

            // Accelerate the throw interval timer so cycles occur proportionally faster
            if (spdMultiplier > 1f)
            {
                float currentElapsed = (float)_elapsedTimeThrowField.GetValue(__instance);
                if (currentElapsed > 0f)
                {
                    float extraDecrement = Time.deltaTime * (spdMultiplier - 1f);
                    _elapsedTimeThrowField.SetValue(__instance, currentElapsed - extraDecrement);
                }
            }
        }

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            if (__instance == null) return;

            Type type = __instance.GetType();
            if (_maxGroundVolumeField == null) _maxGroundVolumeField = FieldCache.GetField(type, "MaxGroundVolume");
            if (_maxGroundVolumeField == null) return;

            float curCap = (float)_maxGroundVolumeField.GetValue(__instance);
            float baseCap = OriginalValueStore.GetOrRegisterFloat(__instance, "WaveTable_MaxGroundVolume", curCap, (obj, val) =>
                _maxGroundVolumeField.SetValue(obj, val));

            float capMultiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.WaveTableCapacityMultiplier
                : 1f;

            float targetCap = baseCap * capMultiplier;
            if (Math.Abs(curCap - targetCap) > 0.001f)
            {
                _maxGroundVolumeField.SetValue(__instance, targetCap);
            }
        }
    }
}
