using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales Magnetite Trailer maximum volume capacity with zero frame rate impact.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.MagnetiteTrailer), "Update")]
    public static class MagnetiteTrailerPatch
    {
        private static readonly Dictionary<int, float> BaseVolumes = new Dictionary<int, float>();
        private static float _lastMultiplier = -1f;

        [HarmonyPostfix]
        public static void Postfix(GoldDigger.MagnetiteTrailer __instance)
        {
            if (__instance == null) return;

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.MagnetiteTrailerCapacityMultiplier
                : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (BaseVolumes.TryGetValue(id, out float baseVol))
            {
                if (multiplier == _lastMultiplier) return;
                __instance.MaxMagnetiteTrailerVolume = baseVol * multiplier;
                return;
            }

            baseVol = __instance.MaxMagnetiteTrailerVolume;
            BaseVolumes[id] = baseVol;
            __instance.MaxMagnetiteTrailerVolume = baseVol * multiplier;
            _lastMultiplier = multiplier;
        }

        public static void Reset()
        {
            BaseVolumes.Clear();
            _lastMultiplier = -1f;
        }
    }
}
