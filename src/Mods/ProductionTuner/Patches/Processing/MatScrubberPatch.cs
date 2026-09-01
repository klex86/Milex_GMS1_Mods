using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Processing
{
    /// <summary>
    /// Scales Nuggetator (MatScrubber) cleaning throughput speed and bucket mat capacities.
    /// </summary>
    [HarmonyPatch]
    public static class MatScrubberPatch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("GoldDigger.MatScrubber");
            return type != null ? AccessTools.Method(type, "Update") : null;
        }

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            if (__instance == null || OrangeBeastFilter.IsOrangeBeastPart(__instance)) return;

            Type type = __instance.GetType();
            FieldInfo speedField = FieldCache.GetField(type, "CleanigDirtSpeed");
            FieldInfo smallField = FieldCache.GetField(type, "SmallInBucket");
            FieldInfo bigField = FieldCache.GetField(type, "BigInBucket");
            FieldInfo xlField = FieldCache.GetField(type, "XLInBucket");

            if (speedField != null)
            {
                float curSpeed = (float)speedField.GetValue(__instance);
                float baseSpeed = OriginalValueStore.GetOrRegisterFloat(__instance, "Nuggetator_Speed", curSpeed, (obj, val) =>
                    speedField.SetValue(obj, val));

                float spdMultiplier = ProductionTunerPlugin.Service != null
                    ? ProductionTunerPlugin.Service.NuggetatorSpeedMultiplier
                    : 1f;

                float targetSpeed = baseSpeed * spdMultiplier;
                if (Math.Abs(curSpeed - targetSpeed) > 0.0001f)
                {
                    speedField.SetValue(__instance, targetSpeed);
                }
            }

            float bucketMult = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.BucketCapacityMultiplier
                : 1f;

            if (smallField != null)
            {
                int curSmall = (int)smallField.GetValue(__instance);
                int baseSmall = OriginalValueStore.GetOrRegisterInt(__instance, "Nuggetator_Small", curSmall, (obj, val) =>
                    smallField.SetValue(obj, val));
                int targetSmall = (int)Math.Round(baseSmall * bucketMult);
                if (curSmall != targetSmall) smallField.SetValue(__instance, targetSmall);
            }

            if (bigField != null)
            {
                int curBig = (int)bigField.GetValue(__instance);
                int baseBig = OriginalValueStore.GetOrRegisterInt(__instance, "Nuggetator_Big", curBig, (obj, val) =>
                    bigField.SetValue(obj, val));
                int targetBig = (int)Math.Round(baseBig * bucketMult);
                if (curBig != targetBig) bigField.SetValue(__instance, targetBig);
            }

            if (xlField != null)
            {
                int curXl = (int)xlField.GetValue(__instance);
                int baseXl = OriginalValueStore.GetOrRegisterInt(__instance, "Nuggetator_XL", curXl, (obj, val) =>
                    xlField.SetValue(obj, val));
                int targetXl = (int)Math.Round(baseXl * bucketMult);
                if (curXl != targetXl) xlField.SetValue(__instance, targetXl);
            }
        }
    }
}
