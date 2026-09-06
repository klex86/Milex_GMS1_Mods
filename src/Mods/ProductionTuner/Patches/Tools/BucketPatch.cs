using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Tools
{
    /// <summary>
    /// Scales hand bucket capacity using the genuine vanilla baseline (15.0f)
    /// from Gold Mining Simulator. Prevents savegame drift and multi-instance desync.
    /// </summary>
    public static class BucketPatch
    {
        public const float VanillaBucketCapacity = 15.0f;

        private static readonly Dictionary<int, GoldDigger.Bucket> Tracked =
            new Dictionary<int, GoldDigger.Bucket>();
        private static float _lastMultiplier = -1f;

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
