using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Processing
{
    /// <summary>
    /// Scales Magnetite Separator capacity (MaxFill) and output processing speed (FillOutSpeed).
    /// </summary>
    [HarmonyPatch]
    public static class MagnetiteSeparatorPatch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("GoldDigger.MagnetiteSeparator");
            return type != null ? AccessTools.Method(type, "Update") : null;
        }

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            if (__instance == null) return;

            Type type = __instance.GetType();
            FieldInfo maxFillField = FieldCache.GetField(type, "MaxFill");
            FieldInfo fillOutSpeedField = FieldCache.GetField(type, "FillOutSpeed");

            if (maxFillField != null)
            {
                float curFill = (float)maxFillField.GetValue(__instance);
                float baseFill = OriginalValueStore.GetOrRegisterFloat(__instance, "MagSep_MaxFill", curFill, (obj, val) =>
                    maxFillField.SetValue(obj, val));

                float capMultiplier = ProductionTunerPlugin.Service != null
                    ? ProductionTunerPlugin.Service.MagnetiteSeparatorCapacityMultiplier
                    : 1f;

                float targetFill = baseFill * capMultiplier;
                if (Math.Abs(curFill - targetFill) > 0.001f)
                {
                    maxFillField.SetValue(__instance, targetFill);
                }
            }

            if (fillOutSpeedField != null)
            {
                float curSpeed = (float)fillOutSpeedField.GetValue(__instance);
                float baseSpeed = OriginalValueStore.GetOrRegisterFloat(__instance, "MagSep_FillOutSpeed", curSpeed, (obj, val) =>
                    fillOutSpeedField.SetValue(obj, val));

                float spdMultiplier = ProductionTunerPlugin.Service != null
                    ? ProductionTunerPlugin.Service.MagnetiteSeparatorSpeedMultiplier
                    : 1f;

                float targetSpeed = baseSpeed * spdMultiplier;
                if (Math.Abs(curSpeed - targetSpeed) > 0.001f)
                {
                    fillOutSpeedField.SetValue(__instance, targetSpeed);
                }
            }
        }
    }
}
