using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Vehicles
{
    /// <summary>
    /// Scales dump truck (Moxy 6x6) bed dirt capacity with zero frame rate impact.
    /// Uses the genuine vehicle prefab baseline and synchronizes reciprocal volume for proper fill physics.
    /// Also compensates payload mass during driving so high-capacity trucks remain agile.
    /// </summary>
    public static class DumpTruckPatch
    {
        private static readonly FieldInfo InvMaxShovelVolumeField = AccessTools.Field(typeof(DiggingController), "_invmaxShovelVolume");

        private static readonly Dictionary<int, (GoldDigger.DumpTruck instance, float baseVolume)> Tracked =
            new Dictionary<int, (GoldDigger.DumpTruck, float)>();
        private static float _lastMultiplier = -1f;

        // -------------------------------------------------------------------------
        // DumpTruck Update() Prefix — Scales capacity before DirtPlane.SetPerc runs
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.DumpTruck), "Update")]
        public static class DumpTruckUpdatePatch
        {
            [HarmonyPrefix]
            public static void Prefix(GoldDigger.DumpTruck __instance)
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

                    _lastMultiplier = multiplier;
                    foreach (var entry in Tracked.Values)
                    {
                        if (entry.instance != null && entry.instance.Digging != null)
                        {
                            ApplyTruckState(entry.instance, entry.baseVolume, multiplier);
                        }
                    }
                    return;
                }

                // First-time registration: record true vanilla prefab volume
                float baseVol = digging._maxShovelVolume;
                Tracked[id] = (__instance, baseVol);
                ApplyTruckState(__instance, baseVol, multiplier);
                _lastMultiplier = multiplier;
            }
        }

        private static void ApplyTruckState(GoldDigger.DumpTruck truck, float baseVol, float multiplier)
        {
            if (truck == null || truck.Digging == null) return;

            float targetVol = baseVol * multiplier;
            truck.Digging._maxShovelVolume = targetVol;
            if (targetVol > 0.0001f && InvMaxShovelVolumeField != null)
            {
                InvMaxShovelVolumeField.SetValue(truck.Digging, 1f / targetVol);
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var kvp in Tracked.Values)
            {
                if (kvp.instance != null && kvp.instance.Digging != null)
                {
                    ApplyTruckState(kvp.instance, kvp.baseVolume, 1f);
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

        // -------------------------------------------------------------------------
        // MachineMove mass compensation — prevents excessive physical drag
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.DumpTruck), "MachineMove")]
        public static class DumpTruckMachineMovePatch
        {
            [HarmonyPrefix]
            public static void Prefix(GoldDigger.DumpTruck __instance, out int __state)
            {
                __state = -1;
                if (__instance == null || __instance.Dirt == null) return;

                float multiplier = ProductionTunerPlugin.Service?.DumpTruckCapacityMultiplier ?? 1f;
                if (multiplier > 1.05f)
                {
                    __state = __instance.Dirt.LoadMass;
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