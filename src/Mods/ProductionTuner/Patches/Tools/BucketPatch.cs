using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Tools
{
    /// <summary>
    /// Scales hand bucket capacity with a zero-allocation fast exit path.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.Bucket), "Update")]
    public static class BucketPatch
    {
        private static readonly Dictionary<int, float> BaseVolumes = new Dictionary<int, float>();
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
            if (BaseVolumes.TryGetValue(id, out float baseVol))
            {
                if (multiplier == _lastMultiplier) return;
                __instance.MaxVolume = baseVol * multiplier;
                return;
            }

            baseVol = __instance.MaxVolume;
            BaseVolumes[id] = baseVol;
            __instance.MaxVolume = baseVol * multiplier;
            _lastMultiplier = multiplier;
        }

        public static void Reset()
        {
            BaseVolumes.Clear();
            _lastMultiplier = -1f;
        }
    }
}
