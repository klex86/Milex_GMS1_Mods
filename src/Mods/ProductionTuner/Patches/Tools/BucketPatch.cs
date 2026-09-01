using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Tools
{
    /// <summary>
    /// Scales hand bucket capacity with a zero-allocation fast exit path and clean vanilla state restoration.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.Bucket), "Update")]
    public static class BucketPatch
    {
        private static readonly Dictionary<int, (GoldDigger.Bucket instance, float baseVolume)> Tracked =
            new Dictionary<int, (GoldDigger.Bucket, float)>();
        private static float _lastMultiplier = -1f;

        [HarmonyPostfix]
        public static void Postfix(GoldDigger.Bucket __instance)
        {
            if (__instance == null) return;

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.BucketCapacityMultiplier
                : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (Tracked.TryGetValue(id, out var data))
            {
                if (multiplier == _lastMultiplier) return;
                __instance.MaxVolume = data.baseVolume * multiplier;
                return;
            }

            float baseVol = __instance.MaxVolume;
            Tracked[id] = (__instance, baseVol);
            __instance.MaxVolume = baseVol * multiplier;
            _lastMultiplier = multiplier;
        }

        public static void RestoreVanilla()
        {
            foreach (var kvp in Tracked.Values)
            {
                if (kvp.instance != null)
                {
                    kvp.instance.MaxVolume = kvp.baseVolume;
                }
            }
            _lastMultiplier = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
            Tracked.Clear();
            _lastMultiplier = -1f;
        }
    }
}
