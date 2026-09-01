using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales feed hopper (ConveyorGround) maximum dirt storage capacity with zero frame rate impact
    /// and clean vanilla state restoration.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.ConveyorGround), "Update")]
    public static class ConveyorGroundPatch
    {
        private static readonly Dictionary<int, (GoldDigger.ConveyorGround instance, float baseDirt)> Tracked =
            new Dictionary<int, (GoldDigger.ConveyorGround, float)>();
        private static float _lastMultiplier = -1f;

        [HarmonyPostfix]
        public static void Postfix(GoldDigger.ConveyorGround __instance)
        {
            if (__instance == null) return;

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.HopperCapacityMultiplier
                : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (Tracked.TryGetValue(id, out var data))
            {
                if (multiplier == _lastMultiplier) return;
                __instance.MaxDirt = data.baseDirt * multiplier;
                return;
            }

            float baseDirt = __instance.MaxDirt;
            Tracked[id] = (__instance, baseDirt);
            __instance.MaxDirt = baseDirt * multiplier;
            _lastMultiplier = multiplier;
        }

        public static void RestoreVanilla()
        {
            foreach (var kvp in Tracked.Values)
            {
                if (kvp.instance != null)
                {
                    kvp.instance.MaxDirt = kvp.baseDirt;
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
