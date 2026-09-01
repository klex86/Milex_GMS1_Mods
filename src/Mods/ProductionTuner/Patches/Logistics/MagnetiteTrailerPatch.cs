using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales Magnetite Trailer maximum volume capacity.
    /// </summary>
    [HarmonyPatch]
    public static class MagnetiteTrailerPatch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("GoldDigger.MagnetiteTrailer");
            return type != null ? AccessTools.Method(type, "Update") : null;
        }

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            if (__instance == null) return;

            Type type = __instance.GetType();
            FieldInfo maxVolField = FieldCache.GetField(type, "MaxMagnetiteTrailerVolume");
            if (maxVolField == null) return;

            float curVol = (float)maxVolField.GetValue(__instance);
            float baseVol = OriginalValueStore.GetOrRegisterFloat(__instance, "MagTrailer_MaxVolume", curVol, (obj, val) =>
                maxVolField.SetValue(obj, val));

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.MagnetiteTrailerCapacityMultiplier
                : 1f;

            float targetVol = baseVol * multiplier;
            if (Math.Abs(curVol - targetVol) > 0.001f)
            {
                maxVolField.SetValue(__instance, targetVol);
            }
        }
    }
}
