using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales bucket elevator conveyor bucket capacity using pristine vanilla constant (0.5f) from Assembly-CSharp.
    /// Prevents savegame drift and multi-instance desynchronization.
    /// </summary>
    public static class ConveyorElevatorPatch
    {
        /// Authentic vanilla baseline from Unity prefab dump: 1.25 m3.
        public const float VanillaBucketCapacity = 1.25f;

        private static readonly Dictionary<int, GoldDigger.ConveyorElevator> Tracked =
            new Dictionary<int, GoldDigger.ConveyorElevator>();
        private static float _lastMultiplier = -1f;

        // -------------------------------------------------------------------------
        // Update() Postfix — Safe initial scaling on first sight & live multiplier changes
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.ConveyorElevator), "Update")]
        public static class ConveyorElevatorUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.ConveyorElevator __instance)
            {
                if (__instance == null) return;

                int id = __instance.GetInstanceID();
                float multiplier = ProductionTunerPlugin.Service?.ConveyorBucketCapacityMultiplier ?? 1f;

                if (!Tracked.ContainsKey(id))
                {
                    Tracked[id] = __instance;
                    __instance.BucketCapacity = VanillaBucketCapacity * multiplier;
                }

                if (multiplier != _lastMultiplier)
                {
                    _lastMultiplier = multiplier;
                    foreach (var elevator in Tracked.Values)
                    {
                        if (elevator != null)
                        {
                            elevator.BucketCapacity = VanillaBucketCapacity * multiplier;
                        }
                    }
                }
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var elevator in Tracked.Values)
            {
                if (elevator != null)
                {
                    elevator.BucketCapacity = VanillaBucketCapacity;
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
