using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales capacity (MaxFill) and processing speed (FillSpeed) for all large washplant shakers (Tier 1–Tier 4).
    /// Excludes the Tier 5 Orange Beast to protect its custom gold counters from corruption.
    /// </summary>
    [HarmonyPatch]
    public static class WashPlantShakerPatch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("GoldDigger.WashplantShakerBase");
            return type != null ? AccessTools.Method(type, "Update") : null;
        }

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            if (__instance == null || OrangeBeastFilter.IsOrangeBeastPart(__instance)) return;

            Type type = __instance.GetType();
            FieldInfo maxFillField = FieldCache.GetField(type, "MaxFill");
            FieldInfo fillSpeedField = FieldCache.GetField(type, "FillSpeed");

            if (maxFillField != null)
            {
                float curFill = (float)maxFillField.GetValue(__instance);
                float baseFill = OriginalValueStore.GetOrRegisterFloat(__instance, "Shaker_MaxFill", curFill, (obj, val) =>
                    maxFillField.SetValue(obj, val));

                float capMultiplier = ProductionTunerPlugin.Service != null
                    ? ProductionTunerPlugin.Service.WashplantCapacityMultiplier
                    : 1f;

                float targetFill = baseFill * capMultiplier;
                if (Math.Abs(curFill - targetFill) > 0.001f)
                {
                    maxFillField.SetValue(__instance, targetFill);
                }
            }

            if (fillSpeedField != null)
            {
                float curSpeed = (float)fillSpeedField.GetValue(__instance);
                float baseSpeed = OriginalValueStore.GetOrRegisterFloat(__instance, "Shaker_FillSpeed", curSpeed, (obj, val) =>
                    fillSpeedField.SetValue(obj, val));

                float spdMultiplier = ProductionTunerPlugin.Service != null
                    ? ProductionTunerPlugin.Service.WashplantSpeedMultiplier
                    : 1f;

                float targetSpeed = baseSpeed * spdMultiplier;
                if (Math.Abs(curSpeed - targetSpeed) > 0.001f)
                {
                    fillSpeedField.SetValue(__instance, targetSpeed);
                }
            }
        }
    }
}
