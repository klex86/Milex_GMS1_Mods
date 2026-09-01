using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales feed hopper (ConveyorGround) maximum dirt storage capacity with zero frame rate impact.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.ConveyorGround), "Update")]
    public static class ConveyorGroundPatch
    {
        private static readonly Dictionary<int, float> BaseDirs = new Dictionary<int, float>();
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
            if (BaseDirs.TryGetValue(id, out float baseDirt))
            {
                if (multiplier == _lastMultiplier) return;
                __instance.MaxDirt = baseDirt * multiplier;
                return;
            }

            baseDirt = __instance.MaxDirt;
            BaseDirs[id] = baseDirt;
            __instance.MaxDirt = baseDirt * multiplier;
            _lastMultiplier = multiplier;
        }

        public static void Reset()
        {
            BaseDirs.Clear();
            _lastMultiplier = -1f;
        }
    }
}
