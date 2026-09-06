using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales Magnetite Trailer maximum volume capacity using pristine vanilla constant (2.0f) from Assembly-CSharp.
    /// Prevents savegame drift and multi-instance desynchronization.
    /// </summary>
    public static class MagnetiteTrailerPatch
    {
        public const float VanillaMaxMagnetiteTrailerVolume = 2.0f;

        private static readonly Dictionary<int, GoldDigger.MagnetiteTrailer> Tracked =
            new Dictionary<int, GoldDigger.MagnetiteTrailer>();
        private static float _lastMultiplier = -1f;

        // -------------------------------------------------------------------------
        // Start() Postfix — apply multiplier on spawn/load before vanilla clamping
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.MagnetiteTrailer), "Start")]
        public static class MagnetiteTrailerStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MagnetiteTrailer __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();
                Tracked[id] = __instance;

                float multiplier = ProductionTunerPlugin.Service?.MagnetiteTrailerCapacityMultiplier ?? 1f;
                __instance.MaxMagnetiteTrailerVolume = VanillaMaxMagnetiteTrailerVolume * multiplier;
            }
        }

        // -------------------------------------------------------------------------
        // Update() Postfix — Live multiplier changes in the in-game menu
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.MagnetiteTrailer), "Update")]
        public static class MagnetiteTrailerUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MagnetiteTrailer __instance)
            {
                if (__instance == null) return;

                int id = __instance.GetInstanceID();
                Tracked[id] = __instance;

                float multiplier = ProductionTunerPlugin.Service?.MagnetiteTrailerCapacityMultiplier ?? 1f;
                if (multiplier != _lastMultiplier)
                {
                    _lastMultiplier = multiplier;
                    foreach (var trailer in Tracked.Values)
                    {
                        if (trailer != null)
                        {
                            trailer.MaxMagnetiteTrailerVolume = VanillaMaxMagnetiteTrailerVolume * multiplier;
                        }
                    }
                }
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var trailer in Tracked.Values)
            {
                if (trailer != null)
                {
                    trailer.MaxMagnetiteTrailerVolume = VanillaMaxMagnetiteTrailerVolume;
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
