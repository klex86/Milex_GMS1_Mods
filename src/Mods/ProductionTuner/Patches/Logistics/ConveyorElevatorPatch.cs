using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales bucket elevator conveyor bucket capacity with zero frame rate impact
    /// and clean vanilla state restoration.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.ConveyorElevator), "Update")]
    public static class ConveyorElevatorPatch
    {
        private static readonly Dictionary<int, (GoldDigger.ConveyorElevator instance, float baseCap)> Tracked =
            new Dictionary<int, (GoldDigger.ConveyorElevator, float)>();
        private static float _lastMultiplier = -1f;

        [HarmonyPostfix]
        public static void Postfix(GoldDigger.ConveyorElevator __instance)
        {
            if (__instance == null) return;

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.ConveyorBucketCapacityMultiplier
                : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (Tracked.TryGetValue(id, out var data))
            {
                if (multiplier == _lastMultiplier) return;
                __instance.BucketCapacity = data.baseCap * multiplier;
                return;
            }

            float baseCap = __instance.BucketCapacity;
            Tracked[id] = (__instance, baseCap);
            __instance.BucketCapacity = baseCap * multiplier;
            _lastMultiplier = multiplier;
        }

        public static void RestoreVanilla()
        {
            foreach (var kvp in Tracked.Values)
            {
                if (kvp.instance != null)
                {
                    kvp.instance.BucketCapacity = kvp.baseCap;
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
