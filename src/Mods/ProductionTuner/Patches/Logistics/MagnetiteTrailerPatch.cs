using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales Magnetite Trailer maximum volume capacity with zero frame rate impact
    /// and clean vanilla state restoration.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.MagnetiteTrailer), "Update")]
    public static class MagnetiteTrailerPatch
    {
        private static readonly Dictionary<int, (GoldDigger.MagnetiteTrailer instance, float baseVol)> Tracked =
            new Dictionary<int, (GoldDigger.MagnetiteTrailer, float)>();
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
            if (Tracked.TryGetValue(id, out var data))
            {
                if (multiplier == _lastMultiplier) return;
                __instance.MaxMagnetiteTrailerVolume = data.baseVol * multiplier;
                return;
            }

            float baseVol = __instance.MaxMagnetiteTrailerVolume;
            Tracked[id] = (__instance, baseVol);
            __instance.MaxMagnetiteTrailerVolume = baseVol * multiplier;
            _lastMultiplier = multiplier;
        }

        public static void RestoreVanilla()
        {
            foreach (var kvp in Tracked.Values)
            {
                if (kvp.instance != null)
                {
                    kvp.instance.MaxMagnetiteTrailerVolume = kvp.baseVol;
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
