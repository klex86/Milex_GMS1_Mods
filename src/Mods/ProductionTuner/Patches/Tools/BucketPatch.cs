using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Tools
{
    /// <summary>
    /// Scales hand bucket capacity. Serves as the master baseline for downstream sluice and table cascade protection.
    /// </summary>
    [HarmonyPatch]
    public static class BucketPatch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("GoldDigger.Bucket");
            return type != null ? AccessTools.Method(type, "Update") : null;
        }

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            if (__instance == null) return;

            Type type = __instance.GetType();
            FieldInfo maxVolField = FieldCache.GetField(type, "MaxVolume");
            if (maxVolField == null) return;

            float currentVol = (float)maxVolField.GetValue(__instance);
            float baseVol = OriginalValueStore.GetOrRegisterFloat(__instance, "Bucket_MaxVolume", currentVol, (obj, val) =>
                maxVolField.SetValue(obj, val));

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.BucketCapacityMultiplier
                : 1f;

            float targetVol = baseVol * multiplier;
            if (Math.Abs(currentVol - targetVol) > 0.001f)
            {
                maxVolField.SetValue(__instance, targetVol);
            }
        }
    }
}
