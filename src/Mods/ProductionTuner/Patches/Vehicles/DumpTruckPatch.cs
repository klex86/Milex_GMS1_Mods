using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Vehicles
{
    /// <summary>
    /// Scales dump truck bed dirt capacity with zero frame rate impact.
    /// Preserves pristine vanilla baseline values to enable clean runtime disable/enable toggling without drift.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.DumpTruck), "Update")]
    public static class DumpTruckPatch
    {
        private static readonly Dictionary<int, (GoldDigger.DumpTruck instance, float baseVolume)> Tracked =
            new Dictionary<int, (GoldDigger.DumpTruck, float)>();
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
            if (Tracked.TryGetValue(id, out var data))
            {
                if (multiplier == _lastMultiplier) return;
                digging._maxShovelVolume = data.baseVolume * multiplier;
                return;
            }

            // First-time registration: record true vanilla base volume
            float baseVol = digging._maxShovelVolume;
            Tracked[id] = (__instance, baseVol);
            digging._maxShovelVolume = baseVol * multiplier;
            _lastMultiplier = multiplier;
        }

        public static void RestoreVanilla()
        {
            foreach (var kvp in Tracked.Values)
            {
                if (kvp.instance != null && kvp.instance.Digging != null)
                {
                    kvp.instance.Digging._maxShovelVolume = kvp.baseVolume;
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
