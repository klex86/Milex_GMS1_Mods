using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales bucket elevator conveyor bucket capacity with zero frame rate impact.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.ConveyorElevator), "Update")]
    public static class ConveyorElevatorPatch
    {
        private static readonly Dictionary<int, float> BaseCaps = new Dictionary<int, float>();
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
            if (BaseCaps.TryGetValue(id, out float baseCap))
            {
                if (multiplier == _lastMultiplier) return;
                __instance.BucketCapacity = baseCap * multiplier;
                return;
            }

            baseCap = __instance.BucketCapacity;
            BaseCaps[id] = baseCap;
            __instance.BucketCapacity = baseCap * multiplier;
            _lastMultiplier = multiplier;
        }

        public static void Reset()
        {
            BaseCaps.Clear();
            _lastMultiplier = -1f;
        }
    }
}
