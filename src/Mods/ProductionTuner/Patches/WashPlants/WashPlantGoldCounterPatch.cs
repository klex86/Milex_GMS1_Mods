using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Synchronizes the entire T3-T5 stationary wash plant setup:
    /// - 12 Sluice Miner's Moss mats (baseline 0.225 m3)
    /// - Sluice Nugget Trap Grates (baseline 0.007 m3)
    /// - Duplex Jig Buckets (baseline 0.030 m3)
    /// - Attached Wash Plant Hog Pans (baseline 0.249 m3 mats, 0.090 m3 dirt box)
    ///
    /// Scales WashPlantGoldCounter fields and recalculates dirt distribution ratios
    /// so that all 4 stations fill to 100% in perfect synchronization.
    /// </summary>
    public static class WashPlantGoldCounterPatch
    {
        public const float VanillaMatsCapacity = 0.225f;
        public const float VanillaSluiceCrateCapacity = 0.007f;
        public const float VanillaBucketCapacity = 0.030f;
        public const float VanillaHogPanMatsCapacity = 0.2488f;
        public const float VanillaHogPanDirtBoxCapacity = 0.090f;

        private static readonly HashSet<int> TrackedCounterIds = new HashSet<int>();
        private static readonly List<GoldDigger.WashPlantGoldCounter> TrackedCounters =
            new List<GoldDigger.WashPlantGoldCounter>();

        private static readonly HashSet<int> WashPlantBucketIds = new HashSet<int>();
        private static readonly HashSet<int> WashPlantHogPanIds = new HashSet<int>();
        private static readonly HashSet<int> WashPlantHogPanDirtBoxIds = new HashSet<int>();
        private static readonly HashSet<int> WashPlantHogPanMossIds = new HashSet<int>();

        public static bool IsWashPlantBucket(GoldDigger.Bucket bucket)
        {
            if (bucket == null) return false;
            return WashPlantBucketIds.Contains(bucket.GetInstanceID());
        }

        public static bool IsWashPlantHogPan(GoldDigger.HogPanDirtBox dirtBox)
        {
            if (dirtBox == null) return false;
            if (WashPlantHogPanDirtBoxIds.Contains(dirtBox.GetInstanceID())) return true;

            for (int i = 0; i < TrackedCounters.Count; i++)
            {
                var counter = TrackedCounters[i];
                if (counter != null)
                {
                    if (counter.MyHogPan != null && counter.MyHogPan.DirtBox == dirtBox)
                    {
                        WashPlantHogPanDirtBoxIds.Add(dirtBox.GetInstanceID());
                        return true;
                    }
                    if (counter.MyHogPan2 != null && counter.MyHogPan2.DirtBox == dirtBox)
                    {
                        WashPlantHogPanDirtBoxIds.Add(dirtBox.GetInstanceID());
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsWashPlantHogPan(GoldDigger.HogPan hogPan)
        {
            if (hogPan == null) return false;
            if (WashPlantHogPanIds.Contains(hogPan.GetInstanceID())) return true;

            for (int i = 0; i < TrackedCounters.Count; i++)
            {
                var counter = TrackedCounters[i];
                if (counter != null && (counter.MyHogPan == hogPan || counter.MyHogPan2 == hogPan))
                {
                    WashPlantHogPanIds.Add(hogPan.GetInstanceID());
                    return true;
                }
            }
            return false;
        }

        public static bool IsWashPlantHogPanMoss(GoldDigger.MinersMoss moss)
        {
            if (moss == null) return false;
            if (WashPlantHogPanMossIds.Contains(moss.GetInstanceID())) return true;

            var hp = moss.GetComponentInParent<GoldDigger.HogPan>();
            if (hp != null && IsWashPlantHogPan(hp))
            {
                WashPlantHogPanMossIds.Add(moss.GetInstanceID());
                return true;
            }
            return false;
        }

        // -------------------------------------------------------------------------
        // CalculateRatios Prefix/Postfix — scale internal capacities & harmonize ratios
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.WashPlantGoldCounter), "CalculateRatios")]
        public static class CalculateRatiosPatch
        {
            [HarmonyPrefix]
            public static void Prefix(GoldDigger.WashPlantGoldCounter __instance)
            {
                if (__instance == null) return;

                int id = __instance.GetInstanceID();
                if (!TrackedCounterIds.Contains(id))
                {
                    TrackedCounterIds.Add(id);
                    TrackedCounters.Add(__instance);
                }

                float mult = ProductionTunerPlugin.Service?.WashplantT3T5SetupCapacityMultiplier ?? 1f;

                __instance.MinerMossMaxGroundVolume = VanillaMatsCapacity * mult;
                __instance.SLUICECRATE_MAX_DIRT_VOLUME = VanillaSluiceCrateCapacity * mult;
                __instance.BUCKET_MAX_CAPACITY = VanillaBucketCapacity * mult;
                __instance.HOGPAN_MATS_MAX_CAPACITY = VanillaHogPanMatsCapacity * mult;

                SyncJigBuckets(__instance.WashPlantDuplex, mult);
                SyncJigBuckets(__instance.WashPlantDuplex2, mult);

                SyncHogPan(__instance.MyHogPan, mult);
                SyncHogPan(__instance.MyHogPan2, mult);
            }

            [HarmonyPostfix]
            public static void Postfix(GoldDigger.WashPlantGoldCounter __instance)
            {
                if (__instance == null) return;

                // In vanilla, CalculateRatios computes:
                // v0 = MinerMossMaxGroundVolume / (DirtToMossRatio / 12f)
                // WashplantToSluiceCrateDirtRatio = SLUICECRATE_MAX_DIRT_VOLUME / v0
                // DirtToBucketRatio = (BUCKET_MAX_CAPACITY / v0) * 2f
                // DirtToHogPanRatio = HOGPAN_MATS_MAX_CAPACITY / 103f  <-- Vanilla bug: divides by fixed 103f instead of v0!
                //
                // In HogPanDirtBox.ProcessPlane, dirt is divided across 4 mats (v7 = v0 / 4f).
                // To achieve identical percentage fill rate (loc1 / v0) on Hog Pan mats as on the 12 main Sluice mats:
                // (loc1 * DirtToHogPanRatio) / (4 * HOGPAN_MATS_MAX_CAPACITY) = loc1 / v0
                // => DirtToHogPanRatio = (4f * HOGPAN_MATS_MAX_CAPACITY) / v0
                float dirtToMossPerMat = __instance.DirtToMossRatio / 12f;
                if (dirtToMossPerMat > 0f)
                {
                    float v0 = __instance.MinerMossMaxGroundVolume / dirtToMossPerMat;
                    if (v0 > 0f)
                    {
                        __instance.DirtToHogPanRatio = (4f * __instance.HOGPAN_MATS_MAX_CAPACITY) / v0;
                    }
                }
            }
        }

        // -------------------------------------------------------------------------
        // Start Postfix — initial sync on scene load / spawn
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.WashPlantGoldCounter), "Start")]
        public static class StartPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.WashPlantGoldCounter __instance)
            {
                if (__instance == null) return;
                __instance.CalculateRatios();
            }
        }

        private static void SyncJigBuckets(GoldDigger.WashplantDuplexJigBase jig, float mult)
        {
            if (jig == null) return;
            float targetVol = VanillaBucketCapacity * mult;

            if (jig.Bucket1 != null)
            {
                jig.Bucket1.MaxVolume = targetVol;
                WashPlantBucketIds.Add(jig.Bucket1.GetInstanceID());
            }
            if (jig.Bucket2 != null)
            {
                jig.Bucket2.MaxVolume = targetVol;
                WashPlantBucketIds.Add(jig.Bucket2.GetInstanceID());
            }

            if (jig.SwapBuckets != null)
            {
                for (int i = 0; i < jig.SwapBuckets.Count; i++)
                {
                    var holder = jig.SwapBuckets[i];
                    if (holder != null && holder.ObjectInHolder != null)
                    {
                        var b = holder.ObjectInHolder.GetComponent<GoldDigger.Bucket>();
                        if (b != null)
                        {
                            b.MaxVolume = targetVol;
                            WashPlantBucketIds.Add(b.GetInstanceID());
                        }
                    }
                }
            }
        }

        private static void SyncHogPan(GoldDigger.HogPan hogPan, float mult)
        {
            if (hogPan == null) return;

            WashPlantHogPanIds.Add(hogPan.GetInstanceID());

            if (hogPan.DirtBox != null)
            {
                hogPan.DirtBox.PlaneVolumeMax = VanillaHogPanDirtBoxCapacity * mult;
                WashPlantHogPanDirtBoxIds.Add(hogPan.DirtBox.GetInstanceID());
            }

            if (hogPan.MinerMoss != null)
            {
                for (int i = 0; i < hogPan.MinerMoss.Count; i++)
                {
                    var m = hogPan.MinerMoss[i];
                    if (m != null)
                    {
                        m.MaxGroundVolume = VanillaHogPanMatsCapacity * mult;
                        WashPlantHogPanMossIds.Add(m.GetInstanceID());
                    }
                }
            }

            var mats = hogPan.GetComponentsInChildren<GoldDigger.MinersMoss>(true);
            if (mats != null)
            {
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] != null)
                    {
                        mats[i].MaxGroundVolume = VanillaHogPanMatsCapacity * mult;
                        WashPlantHogPanMossIds.Add(mats[i].GetInstanceID());
                    }
                }
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var counter in TrackedCounters)
            {
                if (counter != null)
                {
                    counter.MinerMossMaxGroundVolume = VanillaMatsCapacity;
                    counter.SLUICECRATE_MAX_DIRT_VOLUME = VanillaSluiceCrateCapacity;
                    counter.BUCKET_MAX_CAPACITY = VanillaBucketCapacity;
                    counter.HOGPAN_MATS_MAX_CAPACITY = VanillaHogPanMatsCapacity;

                    SyncJigBuckets(counter.WashPlantDuplex, 1f);
                    SyncJigBuckets(counter.WashPlantDuplex2, 1f);

                    SyncHogPan(counter.MyHogPan, 1f);
                    SyncHogPan(counter.MyHogPan2, 1f);

                    counter.CalculateRatios();
                }
            }

            TrackedCounterIds.Clear();
            TrackedCounters.Clear();
            WashPlantBucketIds.Clear();
            WashPlantHogPanIds.Clear();
            WashPlantHogPanDirtBoxIds.Clear();
            WashPlantHogPanMossIds.Clear();
        }

        public static void Reset()
        {
            RestoreVanilla();
        }
    }
}
