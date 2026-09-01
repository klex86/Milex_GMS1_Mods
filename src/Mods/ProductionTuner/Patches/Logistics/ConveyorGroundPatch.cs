using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales feed hopper (ConveyorGround) maximum dirt storage capacity.
    /// Hooks Update to ensure vanilla game resets do not revert the value.
    /// </summary>
    [HarmonyPatch]
    public static class ConveyorGroundPatch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("GoldDigger.ConveyorGround");
            return type != null ? AccessTools.Method(type, "Update") : null;
        }

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            if (__instance == null) return;

            Type type = __instance.GetType();
            FieldInfo maxDirtField = FieldCache.GetField(type, "MaxDirt");
            if (maxDirtField == null) return;

            float curDirt = (float)maxDirtField.GetValue(__instance);
            float baseDirt = OriginalValueStore.GetOrRegisterFloat(__instance, "ConveyorGround_MaxDirt", curDirt, (obj, val) =>
                maxDirtField.SetValue(obj, val));

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.HopperCapacityMultiplier
                : 1f;

            float targetDirt = baseDirt * multiplier;
            if (Math.Abs(curDirt - targetDirt) > 0.001f)
            {
                maxDirtField.SetValue(__instance, targetDirt);
            }
        }
    }
}
