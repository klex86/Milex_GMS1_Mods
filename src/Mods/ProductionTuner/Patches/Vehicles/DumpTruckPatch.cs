using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Vehicles
{
    /// <summary>
    /// Scales dump truck bed dirt capacity with zero frame rate impact.
    /// Excludes base wheel loaders and synchronizes reciprocal volume for proper fill physics.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.DumpTruck), "Update")]
    public static class DumpTruckPatch
    {
        private static readonly Dictionary<int, float> BaseVolumes = new Dictionary<int, float>();
        private static float _lastMultiplier = -1f;

        [HarmonyPostfix]
        public static void Postfix(GoldDigger.DumpTruck __instance)
        {
            if (__instance == null || __instance.GetType().Name != "DumpTruck") return;

            var digging = __instance.Digging;
            if (digging == null) return;

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.DumpTruckCapacityMultiplier
                : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (BaseVolumes.TryGetValue(id, out float baseVol))
            {
                if (multiplier == _lastMultiplier) return;
                digging._maxShovelVolume = baseVol * multiplier;
                return;
            }

            baseVol = digging._maxShovelVolume;
            BaseVolumes[id] = baseVol;
            digging._maxShovelVolume = baseVol * multiplier;
            _lastMultiplier = multiplier;
        }

        public static void Reset()
        {
            BaseVolumes.Clear();
            _lastMultiplier = -1f;
        }
    }
}
