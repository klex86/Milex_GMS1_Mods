using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales feed hopper (ConveyorGround) dirt capacity using pristine vanilla constant (40.0f) from Assembly-CSharp.
    /// Prevents savegame drift and multi-instance desynchronization.
    /// </summary>
    public static class ConveyorGroundPatch
    {
        public const float VanillaHopperMaxDirt = 40.0f;

        private static readonly Dictionary<int, GoldDigger.ConveyorGround> Tracked =
            new Dictionary<int, GoldDigger.ConveyorGround>();
        private static float _lastMultiplier = -1f;

        // -------------------------------------------------------------------------
        // Start() Postfix — apply multiplier on spawn/load before vanilla clamping
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.ConveyorGround), "Start")]
        public static class ConveyorGroundStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.ConveyorGround __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();
                Tracked[id] = __instance;

                float multiplier = ProductionTunerPlugin.Service?.HopperCapacityMultiplier ?? 1f;
                __instance.MaxDirt = VanillaHopperMaxDirt * multiplier;
            }
        }

        // -------------------------------------------------------------------------
        // Update() Postfix — Live multiplier changes in the in-game menu
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.ConveyorGround), "Update")]
        public static class ConveyorGroundUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.ConveyorGround __instance)
            {
                if (__instance == null) return;

                int id = __instance.GetInstanceID();
                Tracked[id] = __instance;

                float multiplier = ProductionTunerPlugin.Service?.HopperCapacityMultiplier ?? 1f;
                if (multiplier != _lastMultiplier)
                {
                    _lastMultiplier = multiplier;
                    foreach (var conveyor in Tracked.Values)
                    {
                        if (conveyor != null)
                        {
                            conveyor.MaxDirt = VanillaHopperMaxDirt * multiplier;
                        }
                    }
                }
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var conveyor in Tracked.Values)
            {
                if (conveyor != null)
                {
                    conveyor.MaxDirt = VanillaHopperMaxDirt;
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
