using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Vehicles
{
    /// <summary>
    /// Scales dump truck bed dirt capacity.
    /// </summary>
    [HarmonyPatch]
    public static class DumpTruckPatch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("GoldDigger.DumpTruck");
            return type != null ? AccessTools.Method(type, "Update") : null;
        }

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            if (__instance == null) return;

            Type truckType = __instance.GetType();
            FieldInfo diggingField = FieldCache.GetField(truckType, "Digging");
            if (diggingField == null) return;

            object diggingObj = diggingField.GetValue(__instance);
            if (diggingObj == null) return;

            Type diggingType = diggingObj.GetType();
            FieldInfo maxVolField = FieldCache.GetField(diggingType, "_maxShovelVolume");
            if (maxVolField == null) return;

            float curVol = (float)maxVolField.GetValue(diggingObj);
            float baseVol = OriginalValueStore.GetOrRegisterFloat(diggingObj, "DumpTruck_MaxVolume", curVol, (obj, val) =>
                maxVolField.SetValue(obj, val));

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.DumpTruckCapacityMultiplier
                : 1f;

            float targetVol = baseVol * multiplier;
            if (Math.Abs(curVol - targetVol) > 0.001f)
            {
                maxVolField.SetValue(diggingObj, targetVol);
            }
        }
    }
}
