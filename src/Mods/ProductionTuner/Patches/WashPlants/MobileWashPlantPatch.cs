using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales capacity (MaxFill) and processing speed (FillSpeed) for both MobileWashplant and MiniWashplant.
    /// </summary>
    public static class MobileWashPlantPatch
    {
        [HarmonyPatch]
        public static class MobileWashplantSubPatch
        {
            [HarmonyTargetMethod]
            private static MethodBase TargetMethod()
            {
                Type type = AccessTools.TypeByName("GoldDigger.MobileWashplant");
                return type != null ? AccessTools.Method(type, "Update") : null;
            }

            [HarmonyPostfix]
            public static void Postfix(object __instance)
            {
                ApplyValues(__instance, "MobileWashplant");
            }
        }

        [HarmonyPatch]
        public static class MiniWashplantSubPatch
        {
            [HarmonyTargetMethod]
            private static MethodBase TargetMethod()
            {
                Type type = AccessTools.TypeByName("GoldDigger.MiniWashplant");
                return type != null ? AccessTools.Method(type, "Update") : null;
            }

            [HarmonyPostfix]
            public static void Postfix(object __instance)
            {
                ApplyValues(__instance, "MiniWashplant");
            }
        }

        private static void ApplyValues(object instance, string prefix)
        {
            if (instance == null) return;

            Type type = instance.GetType();
            FieldInfo maxFillField = FieldCache.GetField(type, "MaxFill");
            FieldInfo fillSpeedField = FieldCache.GetField(type, "FillSpeed");

            if (maxFillField != null)
            {
                float curFill = (float)maxFillField.GetValue(instance);
                float baseFill = OriginalValueStore.GetOrRegisterFloat(instance, prefix + "_MaxFill", curFill, (obj, val) =>
                    maxFillField.SetValue(obj, val));

                float capMultiplier = ProductionTunerPlugin.Service != null
                    ? ProductionTunerPlugin.Service.MobileWashPlantCapacityMultiplier
                    : 1f;

                float targetFill = baseFill * capMultiplier;
                if (Math.Abs(curFill - targetFill) > 0.001f)
                {
                    maxFillField.SetValue(instance, targetFill);
                }
            }

            if (fillSpeedField != null)
            {
                float curSpeed = (float)fillSpeedField.GetValue(instance);
                float baseSpeed = OriginalValueStore.GetOrRegisterFloat(instance, prefix + "_FillSpeed", curSpeed, (obj, val) =>
                    fillSpeedField.SetValue(obj, val));

                float spdMultiplier = ProductionTunerPlugin.Service != null
                    ? ProductionTunerPlugin.Service.MobileWashPlantSpeedMultiplier
                    : 1f;

                float targetSpeed = baseSpeed * spdMultiplier;
                if (Math.Abs(curSpeed - targetSpeed) > 0.001f)
                {
                    fillSpeedField.SetValue(instance, targetSpeed);
                }
            }
        }
    }
}
