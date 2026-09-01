using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales dirt and mineral sediment capacity for Miner's Moss mats.
    /// </summary>
    [HarmonyPatch]
    public static class MinersMossPatch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("GoldDigger.MinersMoss");
            return type != null ? AccessTools.Method(type, "Update") : null;
        }

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            if (__instance == null || OrangeBeastFilter.IsOrangeBeastPart(__instance)) return;

            Type type = __instance.GetType();
            FieldInfo maxVolField = FieldCache.GetField(type, "MaxGroundVolume");
            if (maxVolField == null) return;

            float curVol = (float)maxVolField.GetValue(__instance);
            float baseVol = OriginalValueStore.GetOrRegisterFloat(__instance, "MinersMoss_MaxGroundVolume", curVol, (obj, val) =>
                maxVolField.SetValue(obj, val));

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.MinersMossCapacityMultiplier
                : 1f;

            float targetVol = baseVol * multiplier;
            if (Math.Abs(curVol - targetVol) > 0.001f)
            {
                maxVolField.SetValue(__instance, targetVol);
            }
        }
    }
}
