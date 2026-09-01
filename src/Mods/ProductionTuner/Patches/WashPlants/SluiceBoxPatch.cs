using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales dirt capacity for washplant sluice boxes.
    /// </summary>
    [HarmonyPatch]
    public static class SluiceBoxPatch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("GoldDigger.WashPlantSluiceBoxDirt");
            return type != null ? AccessTools.Method(type, "Update") : null;
        }

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            if (__instance == null || OrangeBeastFilter.IsOrangeBeastPart(__instance)) return;

            Type type = __instance.GetType();
            FieldInfo maxFillField = FieldCache.GetField(type, "MaxFill");
            if (maxFillField == null) return;

            float curFill = (float)maxFillField.GetValue(__instance);
            float baseFill = OriginalValueStore.GetOrRegisterFloat(__instance, "SluiceBox_MaxFill", curFill, (obj, val) =>
                maxFillField.SetValue(obj, val));

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.SluiceboxCapacityMultiplier
                : 1f;

            float targetFill = baseFill * multiplier;
            if (Math.Abs(curFill - targetFill) > 0.001f)
            {
                maxFillField.SetValue(__instance, targetFill);
            }
        }
    }
}
