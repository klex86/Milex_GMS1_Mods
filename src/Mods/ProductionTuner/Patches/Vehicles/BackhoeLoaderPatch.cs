using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Vehicles
{
    /// <summary>
    /// Scales Backhoe Loader (KoparkoLadowarka) front loader bucket and rear excavator arm shovel volumes.
    /// </summary>
    [HarmonyPatch]
    public static class BackhoeLoaderPatch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("KoparkoLadowarka");
            return type != null ? AccessTools.Method(type, "Update") : null;
        }

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            if (__instance == null) return;

            Type backhoeType = __instance.GetType();
            FieldInfo diggingFrontField = FieldCache.GetField(backhoeType, "DiggingFront");
            FieldInfo diggingRearField = FieldCache.GetField(backhoeType, "Digging");

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.BackhoeLoaderLoadSpeedMultiplier
                : 1f;

            if (diggingFrontField != null)
            {
                ApplyDiggingController(diggingFrontField.GetValue(__instance), "BackhoeFront", multiplier);
            }

            if (diggingRearField != null)
            {
                ApplyDiggingController(diggingRearField.GetValue(__instance), "BackhoeRear", multiplier);
            }
        }

        private static void ApplyDiggingController(object diggingObj, string prefix, float multiplier)
        {
            if (diggingObj == null) return;

            Type type = diggingObj.GetType();
            FieldInfo maxVolField = FieldCache.GetField(type, "_maxShovelVolume");
            FieldInfo invMaxVolField = FieldCache.GetField(type, "_invmaxShovelVolume");
            if (maxVolField == null) return;

            float curVol = (float)maxVolField.GetValue(diggingObj);
            float baseVol = OriginalValueStore.GetOrRegisterFloat(diggingObj, prefix + "_MaxVol", curVol, (obj, val) =>
            {
                maxVolField.SetValue(obj, val);
                if (invMaxVolField != null && val > 0.0001f)
                {
                    invMaxVolField.SetValue(obj, 1f / val);
                }
            });

            float targetVol = baseVol * multiplier;
            if (Math.Abs(curVol - targetVol) > 0.001f)
            {
                maxVolField.SetValue(diggingObj, targetVol);
                if (invMaxVolField != null && targetVol > 0.0001f)
                {
                    invMaxVolField.SetValue(diggingObj, 1f / targetVol);
                }
            }
        }
    }
}
