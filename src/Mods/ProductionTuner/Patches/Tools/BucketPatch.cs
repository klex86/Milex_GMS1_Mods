using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Tools
{
    /// <summary>
    /// Scales hand bucket capacity using the genuine vanilla baseline (15.0f)
    /// from Gold Mining Simulator. Prevents savegame drift and multi-instance desync.
    /// Also protects material and gold from destruction when transferring to GoldPan.
    /// </summary>
    public static class BucketPatch
    {
        /// Authentic vanilla baseline from Unity prefab dump: 0.03 m3 (exactly 3 shovel scoops of 0.01 m3).
        public const float VanillaBucketCapacity = 0.03f;

        private static readonly Dictionary<int, GoldDigger.Bucket> Tracked =
            new Dictionary<int, GoldDigger.Bucket>();
        private static float _lastMultiplier = -1f;

        private static readonly Action<GoldDigger.Bucket> WylejMethod =
            (Action<GoldDigger.Bucket>)Delegate.CreateDelegate(
                typeof(Action<GoldDigger.Bucket>),
                AccessTools.Method(typeof(GoldDigger.Bucket), "Wylej"));

        private static readonly Action<GoldDigger.Bucket> QuestCheckMethod =
            (Action<GoldDigger.Bucket>)Delegate.CreateDelegate(
                typeof(Action<GoldDigger.Bucket>),
                AccessTools.Method(typeof(GoldDigger.Bucket), "PanRestGoldInBucketQuestCheck"));

        private static readonly Action<GoldDigger.GoldPan> PanUpdateFillCountMethod =
            (Action<GoldDigger.GoldPan>)Delegate.CreateDelegate(
                typeof(Action<GoldDigger.GoldPan>),
                AccessTools.Method(typeof(GoldDigger.GoldPan), "UpdateFillCount"));

        // -------------------------------------------------------------------------
        // Update() Postfix — Safe initial scaling on first sight & live multiplier changes
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.Bucket), "Update")]
        public static class BucketUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.Bucket __instance)
            {
                if (__instance == null) return;

                int id = __instance.GetInstanceID();
                float multiplier = ProductionTunerPlugin.Service?.BucketCapacityMultiplier ?? 1f;

                if (!Tracked.ContainsKey(id))
                {
                    Tracked[id] = __instance;
                    __instance.MaxVolume = VanillaBucketCapacity * multiplier;
                }

                if (multiplier != _lastMultiplier)
                {
                    _lastMultiplier = multiplier;
                    foreach (var bucket in Tracked.Values)
                    {
                        if (bucket != null)
                        {
                            bucket.MaxVolume = VanillaBucketCapacity * multiplier;
                        }
                    }
                }
            }
        }

        // -------------------------------------------------------------------------
        // FillOutCorutine() Prefix — Safe GoldPan transfer overflow & gold loss guard
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.Bucket), "FillOutCorutine")]
        public static class BucketFillOutCorutinePatch
        {
            [HarmonyPrefix]
            public static bool Prefix(GoldDigger.Bucket __instance, GameObject objectFillIn, ref IEnumerator __result)
            {
                if (__instance == null || objectFillIn == null) return true;

                var pan = objectFillIn.GetComponent<GoldDigger.GoldPan>();
                if (pan != null)
                {
                    __result = SafeFillOutGoldPanCorutine(__instance, pan);
                    return false;
                }

                return true;
            }
        }

        private static IEnumerator SafeFillOutGoldPanCorutine(GoldDigger.Bucket bucket, GoldDigger.GoldPan pan)
        {
            yield return new WaitForSeconds(0.5f);

            if (bucket == null || pan == null) yield break;

            WylejMethod?.Invoke(bucket);

            // Scale GoldPan capacity to match bucket so pan can hold full bucket transfers
            if (bucket.MaxVolume > pan.PanMaxFill)
            {
                pan.PanMaxFill = bucket.MaxVolume;
            }

            // Recalculate remaining dirt objects inside the pan so cleaned pans are recognized as empty
            PanUpdateFillCountMethod?.Invoke(pan);

            float totalMaterial = bucket.MudVolume + (bucket.Minerals != null ? bucket.Minerals[ResourceType.Magnetite] : 0f);
            if (totalMaterial <= 0.0001f)
            {
                bucket.UpdatePlaneAndMass();
                yield break;
            }

            float panFree = Mathf.Max(0f, pan.PanMaxFill - pan._GroundVolume);
            if (panFree <= 0.001f)
            {
                HUD.Instance?.ShowHint("PLACE_TO_FILL_IS_FULL", 2f, "");
                bucket.UpdatePlaneAndMass();
                yield break;
            }

            float bucketQuarter = bucket.MaxVolume / 4f;
            float volumeToPour = Mathf.Min(totalMaterial, Mathf.Min(bucketQuarter, panFree));
            if (volumeToPour <= 0.0001f)
            {
                bucket.UpdatePlaneAndMass();
                yield break;
            }

            float ratio = volumeToPour / totalMaterial;
            ResourceBag bagToPour = new ResourceBag();
            if (bucket.Minerals != null)
            {
                foreach (ResourceType resType in Enum.GetValues(typeof(ResourceType)))
                {
                    bagToPour[resType] = bucket.Minerals[resType] * ratio;
                }
            }

            float rentGoldToPour = bucket.GoldForRent * ratio;

            pan.FillIn(volumeToPour, bagToPour, rentGoldToPour);

            if (bucket.Minerals != null)
            {
                bucket.Minerals.Sub(bagToPour);
            }

            float mudDeducted = bucket.MudVolume * ratio;
            bucket.MudVolume = Mathf.Max(0f, bucket.MudVolume - mudDeducted);
            bucket.WaterVolume = Mathf.Max(0f, bucket.WaterVolume - (bucket.WaterVolume * ratio));
            bucket.GoldForRent = Mathf.Max(0f, bucket.GoldForRent - rentGoldToPour);

            QuestCheckMethod?.Invoke(bucket);
            bucket.UpdatePlaneAndMass();
        }

        public static void RestoreVanilla()
        {
            foreach (var bucket in Tracked.Values)
            {
                if (bucket != null)
                {
                    bucket.MaxVolume = VanillaBucketCapacity;
                }
            }
            Tracked.Clear();
            _lastMultiplier = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
        }
    }
}

