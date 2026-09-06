using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Processing
{
    /// <summary>
    /// Scales Nuggetator (MatScrubber) cleaning throughput speed and bucket mat capacities
    /// based on verified vanilla baseline values.
    /// Employs a zero-allocation fast exit path and multi-instance synchronization.
    /// </summary>
    [HarmonyPatch]
    public static class MatScrubberPatch
    {
        public const float VanillaCleanSpeed = 0.01f;
        public const int VanillaBig = 12;
        public const int VanillaSmall = 8;
        public const int VanillaXL = 60;

        private static readonly FieldInfo RatioField = AccessTools.Field(typeof(GoldDigger.MatScrubber), "_ratio");
        private static readonly FieldInfo MyBucketField = AccessTools.Field(typeof(GoldDigger.MatScrubber), "_myBucket");

        private static readonly Dictionary<int, GoldDigger.MatScrubber> Tracked = new Dictionary<int, GoldDigger.MatScrubber>();
        private static float _lastSpeedMult = -1f;
        private static float _lastBucketMult = -1f;

        [HarmonyPatch(typeof(GoldDigger.MatScrubber), "Start")]
        [HarmonyPrefix]
        public static void StartPrefix(GoldDigger.MatScrubber __instance)
        {
            if (__instance == null) return;

            float spdMultiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.NuggetatorSpeedMultiplier
                : 1f;
            float bucketMult = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.BucketCapacityMultiplier
                : 1f;

            ApplyToInstance(__instance, spdMultiplier, bucketMult);
            Tracked[__instance.GetInstanceID()] = __instance;
        }

        [HarmonyPatch(typeof(GoldDigger.MatScrubber), "Update")]
        [HarmonyPostfix]
        public static void UpdatePostfix(GoldDigger.MatScrubber __instance)
        {
            if (__instance == null) return;

            float spdMultiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.NuggetatorSpeedMultiplier
                : 1f;
            float bucketMult = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.BucketCapacityMultiplier
                : 1f;

            int id = __instance.GetInstanceID();
            if (!Tracked.ContainsKey(id))
            {
                Tracked[id] = __instance;
                ApplyToInstance(__instance, spdMultiplier, bucketMult);
            }

            if (spdMultiplier == _lastSpeedMult && bucketMult == _lastBucketMult)
                return;

            _lastSpeedMult = spdMultiplier;
            _lastBucketMult = bucketMult;

            foreach (var scrubber in Tracked.Values)
            {
                if (scrubber != null)
                {
                    ApplyToInstance(scrubber, spdMultiplier, bucketMult);
                }
            }
        }

        private static void ApplyToInstance(GoldDigger.MatScrubber scrubber, float spdMultiplier, float bucketMult)
        {
            scrubber.CleanigDirtSpeed = VanillaCleanSpeed * spdMultiplier;
            scrubber.SmallInBucket = (int)Math.Round(VanillaSmall * bucketMult);
            scrubber.BigInBucket = (int)Math.Round(VanillaBig * bucketMult);
            scrubber.XLInBucket = (int)Math.Round(VanillaXL * bucketMult);

            if (RatioField != null && scrubber.MinerMosses != null && scrubber.MinerMosses.Length > 6 &&
                scrubber.MinerMosses[0] != null && scrubber.MinerMosses[6] != null)
            {
                float num = scrubber.MinerMosses[0].MaxGroundVolume * scrubber.BigInBucket +
                            scrubber.MinerMosses[6].MaxGroundVolume * scrubber.SmallInBucket;
                if (num > 0.0001f)
                {
                    var myBucket = MyBucketField?.GetValue(scrubber) as GoldDigger.Bucket;
                    float bucketVol = myBucket != null ? myBucket.MaxVolume : 0.03f;
                    RatioField.SetValue(scrubber, bucketVol / num);
                }
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var scrubber in Tracked.Values)
            {
                if (scrubber != null)
                {
                    ApplyToInstance(scrubber, 1f, 1f);
                }
            }
            _lastSpeedMult = 1f;
            _lastBucketMult = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
            Tracked.Clear();
            _lastSpeedMult = -1f;
            _lastBucketMult = -1f;
        }
    }
}
