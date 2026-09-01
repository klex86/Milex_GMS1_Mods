using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales bucket elevator conveyor bucket capacity.
    /// </summary>
    [HarmonyPatch]
    public static class ConveyorElevatorPatch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("GoldDigger.ConveyorElevator");
            return type != null ? AccessTools.Method(type, "Update") : null;
        }

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            if (__instance == null) return;

            Type type = __instance.GetType();
            FieldInfo bucketCapField = FieldCache.GetField(type, "BucketCapacity");
            if (bucketCapField == null) return;

            float curCap = (float)bucketCapField.GetValue(__instance);
            float baseCap = OriginalValueStore.GetOrRegisterFloat(__instance, "ConveyorElevator_BucketCapacity", curCap, (obj, val) =>
                bucketCapField.SetValue(obj, val));

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.ConveyorBucketCapacityMultiplier
                : 1f;

            float targetCap = baseCap * multiplier;
            if (Math.Abs(curCap - targetCap) > 0.001f)
            {
                bucketCapField.SetValue(__instance, targetCap);
            }
        }
    }
}
