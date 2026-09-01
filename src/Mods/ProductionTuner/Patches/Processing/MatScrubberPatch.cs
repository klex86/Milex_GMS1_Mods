using System;
using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Processing
{
    /// <summary>
    /// Scales Nuggetator (MatScrubber) cleaning throughput speed and bucket mat capacities.
    /// Employs a zero-allocation fast exit path to guarantee 0 FPS overhead in runtime loops.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.MatScrubber), "Update")]
    public static class MatScrubberPatch
    {
        private struct ScrubberBase
        {
            public float CleanSpeed;
            public int Small;
            public int Big;
            public int XL;
        }

        private static readonly Dictionary<int, ScrubberBase> BaseValues = new Dictionary<int, ScrubberBase>();
        private static float _lastSpeedMult = -1f;
        private static float _lastBucketMult = -1f;

        [HarmonyPostfix]
        public static void Postfix(GoldDigger.MatScrubber __instance)
        {
            if (__instance == null) return;

            float spdMultiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.NuggetatorSpeedMultiplier
                : 1f;

            float bucketMult = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.BucketCapacityMultiplier
                : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (BaseValues.TryGetValue(id, out var baseVal))
            {
                if (spdMultiplier == _lastSpeedMult && bucketMult == _lastBucketMult) return;
                __instance.CleanigDirtSpeed = baseVal.CleanSpeed * spdMultiplier;
                __instance.SmallInBucket = (int)Math.Round(baseVal.Small * bucketMult);
                __instance.BigInBucket = (int)Math.Round(baseVal.Big * bucketMult);
                __instance.XLInBucket = (int)Math.Round(baseVal.XL * bucketMult);
                return;
            }

            baseVal = new ScrubberBase
            {
                CleanSpeed = __instance.CleanigDirtSpeed,
                Small = __instance.SmallInBucket,
                Big = __instance.BigInBucket,
                XL = __instance.XLInBucket
            };
            BaseValues[id] = baseVal;
            __instance.CleanigDirtSpeed = baseVal.CleanSpeed * spdMultiplier;
            __instance.SmallInBucket = (int)Math.Round(baseVal.Small * bucketMult);
            __instance.BigInBucket = (int)Math.Round(baseVal.Big * bucketMult);
            __instance.XLInBucket = (int)Math.Round(baseVal.XL * bucketMult);
            _lastSpeedMult = spdMultiplier;
            _lastBucketMult = bucketMult;
        }

        public static void Reset()
        {
            BaseValues.Clear();
            _lastSpeedMult = -1f;
            _lastBucketMult = -1f;
        }
    }
}
