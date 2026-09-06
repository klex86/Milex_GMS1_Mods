using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Vehicles
{
    /// <summary>
    /// Scales dump truck bed dirt capacity with zero frame rate impact.
    /// Preserves pristine vanilla baseline values to enable clean runtime disable/enable toggling without drift.
    /// Save/Load Safety: Start() Postfix captures the vanilla baseline after vanilla Start() runs,
    /// preventing a previously-serialized modded _maxShovelVolume from being stored as the base.
    /// </summary>
    public static class DumpTruckPatch
    {
        private static readonly Dictionary<int, (GoldDigger.DumpTruck instance, float baseVolume)> Tracked =
            new Dictionary<int, (GoldDigger.DumpTruck, float)>();
        private static float _lastMultiplier = -1f;

        // -------------------------------------------------------------------------
        // Start() Postfix — captures vanilla baseline AFTER vanilla Start() runs.
        // Prevents a serialized modded _maxShovelVolume from being read as base.
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.DumpTruck), "Start")]
        public static class DumpTruckStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.DumpTruck __instance)
            {
                if (__instance == null || __instance.GetType().Name != "DumpTruck") return;
                var digging = __instance.Digging;
                if (digging == null) return;
                int id = __instance.GetInstanceID();
                if (Tracked.ContainsKey(id)) return;
                float multiplier = ProductionTunerPlugin.Service?.DumpTruckCapacityMultiplier ?? 1f;
                float baseVol = digging._maxShovelVolume;
                Tracked[id] = (__instance, baseVol);
                digging._maxShovelVolume = baseVol * multiplier;
                _lastMultiplier = multiplier;
            }
        }

        // -------------------------------------------------------------------------
        // Update() Postfix — fast-path for live multiplier changes in the in-game menu
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.DumpTruck), "Update")]
        public static class DumpTruckUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.DumpTruck __instance)
            {
                if (__instance == null || __instance.GetType().Name != "DumpTruck") return;

                var digging = __instance.Digging;
                if (digging == null) return;

                float multiplier = ProductionTunerPlugin.Service?.DumpTruckCapacityMultiplier ?? 1f;
                int id = __instance.GetInstanceID();

                // Zero-allocation fast-path
                if (Tracked.TryGetValue(id, out var data))
                {
                    if (multiplier == _lastMultiplier) return;
                    digging._maxShovelVolume = data.baseVolume * multiplier;
                    _lastMultiplier = multiplier;
                    return;
                }

                // Fallback registration (Start() not caught)
                float baseVol = digging._maxShovelVolume;
                Tracked[id] = (__instance, baseVol);
                digging._maxShovelVolume = baseVol * multiplier;
                _lastMultiplier = multiplier;
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var kvp in Tracked.Values)
            {
                if (kvp.instance != null && kvp.instance.Digging != null)
                    kvp.instance.Digging._maxShovelVolume = kvp.baseVolume;
            }
            _lastMultiplier = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
            Tracked.Clear();
            _lastMultiplier = -1f;
        }

        /// <summary>
        /// Compensates physical payload drag while driving so high-capacity dump trucks remain agile and responsive.
        /// </summary>
        [HarmonyPatch(typeof(GoldDigger.DumpTruck), "MachineMove")]
        public static class DumpTruckMachineMovePatch
        {
            [HarmonyPrefix]
            public static void Prefix(GoldDigger.DumpTruck __instance, out int __state)
            {
                __state = -1;
                if (__instance == null || __instance.Dirt == null) return;

                float multiplier = ProductionTunerPlugin.Service?.DumpTruckCapacityMultiplier ?? 1f;

                // Only compensate if capacity is scaled beyond vanilla
                if (multiplier > 1.05f)
                {
                    __state = __instance.Dirt.LoadMass;
                    // Cap physical simulation mass to baseline vanilla equivalent
                    __instance.Dirt.LoadMass = (int)(__state / multiplier);
                }
            }

            [HarmonyPostfix]
            public static void Postfix(GoldDigger.DumpTruck __instance, int __state)
            {
                if (__instance == null || __instance.Dirt == null || __state < 0) return;
                __instance.Dirt.LoadMass = __state;
            }
        }
    }
}
