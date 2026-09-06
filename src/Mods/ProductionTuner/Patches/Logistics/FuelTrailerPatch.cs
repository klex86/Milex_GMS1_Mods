using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales mobile fuel trailer capacity, stationary fuel tank capacity, refuel pump speed,
    /// and physical fuel hose reach — with save/load data safety and clean vanilla state restoration.
    ///
    /// Save/Load Safety Strategy:
    ///   All Start() patches use [HarmonyPostfix] so that vanilla Start() runs first and
    ///   establishes true initialized field values before we capture the base. This prevents
    ///   accidentally reading a previously-serialized modded value as the base.
    ///   For the mobile trailer, a hardcoded vanilla capacity (1000 L) is always used as the
    ///   base to guard against serialized modded values regardless of Start() ordering.
    /// </summary>
    public static class FuelTrailerPatch
    {
        /// <summary>Known vanilla capacity of the mobile fuel trailer.</summary>
        private const float VanillaTrailerCapacity = 1000f;

        // Mobile fuel trailer (End_Bottom, child of a Trailer component)
        private static readonly Dictionary<int, (GoldDigger.FuelStationController instance, float baseCap)> TrackedTrailers =
            new Dictionary<int, (GoldDigger.FuelStationController, float)>();

        // Stationary fuel tanks (all other FuelStationController instances)
        private static readonly Dictionary<int, (GoldDigger.FuelStationController instance, float baseCap)> TrackedStationary =
            new Dictionary<int, (GoldDigger.FuelStationController, float)>();

        // Fuel pistol tanking speed
        private static readonly Dictionary<int, (GoldDigger.FuelPistolHoldable instance, float baseSpeed)> TrackedPistols =
            new Dictionary<int, (GoldDigger.FuelPistolHoldable, float)>();

        // Fuel pistol joint limits (hose length)
        private static readonly Dictionary<int, (GoldDigger.FuelPistolHoldable instance, float baseLimit)> TrackedPistolJoints =
            new Dictionary<int, (GoldDigger.FuelPistolHoldable, float)>();

        private static float _lastTrailerMult = -1f;
        private static float _lastTankMult = -1f;

        /// <summary>
        /// Detects mobile fuel trailer by object name and parent hierarchy only.
        /// Does NOT use MaxCapacity value — this avoids misclassification when
        /// MaxCapacity was serialized as a previously-modded value.
        /// </summary>
        private static bool IsMobileTrailerByHierarchy(GoldDigger.FuelStationController fsc)
        {
            return fsc.gameObject.name.Contains("End_Bottom")
                   && fsc.GetComponentInParent<GoldDigger.Trailer>() != null;
        }

        // -------------------------------------------------------------------------
        // Start() Postfix — runs AFTER vanilla Start() so fields are in their true
        // initialized state. Captures the vanilla base AFTER the object is ready.
        // For the mobile trailer we always use the hardcoded vanilla 1000 L.
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.FuelStationController), "Start")]
        public static class FuelStationStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.FuelStationController __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();

                if (IsMobileTrailerByHierarchy(__instance))
                {
                    // Always use the hardcoded vanilla capacity as base — never read the
                    // possibly-serialized modded MaxCapacity.
                    float multiplier = ProductionTunerPlugin.Service?.FuelTrailerCapacityMultiplier ?? 1f;
                    if (!TrackedTrailers.ContainsKey(id))
                        TrackedTrailers[id] = (__instance, VanillaTrailerCapacity);
                    __instance.MaxCapacity = VanillaTrailerCapacity * multiplier;
                    _lastTrailerMult = multiplier;
                }
                else
                {
                    // For stationary tanks, read vanilla MaxCapacity AFTER vanilla Start().
                    // Vanilla Start() should have initialized it to the true default.
                    if (!TrackedStationary.ContainsKey(id))
                    {
                        float baseCap = __instance.MaxCapacity;
                        float multiplier = ProductionTunerPlugin.Service?.FuelTankCapacityMultiplier ?? 1f;
                        TrackedStationary[id] = (__instance, baseCap);
                        __instance.MaxCapacity = baseCap * multiplier;
                        _lastTankMult = multiplier;
                    }
                }
            }
        }

        // -------------------------------------------------------------------------
        // Update() Postfix — fast-path for live multiplier changes in the in-game menu.
        // Fallback registration for objects that were not yet initialized during Start.
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.FuelStationController), "Update")]
        public static class FuelStationUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.FuelStationController __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();

                // Mobile trailer fast-path
                if (TrackedTrailers.TryGetValue(id, out var trailerData))
                {
                    float multiplier = ProductionTunerPlugin.Service?.FuelTrailerCapacityMultiplier ?? 1f;
                    if (multiplier == _lastTrailerMult) return;
                    __instance.MaxCapacity = trailerData.baseCap * multiplier;
                    _lastTrailerMult = multiplier;
                    return;
                }

                // Stationary tank fast-path
                if (TrackedStationary.TryGetValue(id, out var tankData))
                {
                    float multiplier = ProductionTunerPlugin.Service?.FuelTankCapacityMultiplier ?? 1f;
                    if (multiplier == _lastTankMult) return;
                    __instance.MaxCapacity = tankData.baseCap * multiplier;
                    _lastTankMult = multiplier;
                    return;
                }

                // Fallback registration (objects active after initial load, no Start() was caught)
                if (IsMobileTrailerByHierarchy(__instance))
                {
                    float multiplier = ProductionTunerPlugin.Service?.FuelTrailerCapacityMultiplier ?? 1f;
                    TrackedTrailers[id] = (__instance, VanillaTrailerCapacity);
                    __instance.MaxCapacity = VanillaTrailerCapacity * multiplier;
                    _lastTrailerMult = multiplier;
                }
                else
                {
                    // For stationary: read current MaxCapacity only as last resort.
                    // Ideally Start() already ran; this is just a safety net.
                    float curCap = __instance.MaxCapacity;
                    float multiplier = ProductionTunerPlugin.Service?.FuelTankCapacityMultiplier ?? 1f;
                    TrackedStationary[id] = (__instance, curCap);
                    __instance.MaxCapacity = curCap * multiplier;
                    _lastTankMult = multiplier;
                }
            }
        }

        // -------------------------------------------------------------------------
        // FuelPistol Attach Postfix — scales tanking speed and hose joint limit
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.FuelPistolHoldable), "Attach")]
        public static class FuelPistolSubPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.FuelPistolHoldable __instance)
            {
                if (__instance == null) return;

                int id = __instance.GetInstanceID();
                float speedMult = ProductionTunerPlugin.Service?.FuelTrailerCapacityMultiplier ?? 1f;
                float hoseMult  = ProductionTunerPlugin.Service?.FuelHoseLengthMultiplier ?? 1f;

                // --- Tanking speed ---
                if (!TrackedPistols.TryGetValue(id, out var speedData))
                {
                    speedData = (__instance, __instance.TankingSpeed);
                    TrackedPistols[id] = speedData;
                }
                __instance.TankingSpeed = speedData.baseSpeed * Mathf.Max(1f, speedMult);

                // --- Hose reach (ConfigurableJoint on the pistol GameObject) ---
                ConfigurableJoint goJoint = __instance.GetComponent<ConfigurableJoint>();
                if (goJoint != null)
                {
                    if (!TrackedPistolJoints.TryGetValue(id, out var jointData))
                    {
                        jointData = (__instance, goJoint.linearLimit.limit);
                        TrackedPistolJoints[id] = jointData;
                    }
                    SoftJointLimit lim = goJoint.linearLimit;
                    lim.limit = jointData.baseLimit * Mathf.Max(1f, hoseMult);
                    goJoint.linearLimit = lim;
                }

                // --- Hose reach (MyConfigurableJ field on the pistol) ---
                if (__instance.MyConfigurableJ != null)
                {
                    float baseLimit = TrackedPistolJoints.TryGetValue(id, out var jd)
                        ? jd.baseLimit
                        : __instance.MyConfigurableJ.linearLimit.limit;

                    SoftJointLimit lim2 = __instance.MyConfigurableJ.linearLimit;
                    lim2.limit = baseLimit * Mathf.Max(1f, hoseMult);
                    __instance.MyConfigurableJ.linearLimit = lim2;
                }
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var kvp in TrackedTrailers.Values)
                if (kvp.instance != null)
                    kvp.instance.MaxCapacity = kvp.baseCap;

            foreach (var kvp in TrackedStationary.Values)
                if (kvp.instance != null)
                    kvp.instance.MaxCapacity = kvp.baseCap;

            foreach (var kvp in TrackedPistols.Values)
                if (kvp.instance != null)
                    kvp.instance.TankingSpeed = kvp.baseSpeed;

            _lastTrailerMult = 1f;
            _lastTankMult = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
            TrackedTrailers.Clear();
            TrackedStationary.Clear();
            TrackedPistols.Clear();
            TrackedPistolJoints.Clear();
            _lastTrailerMult = -1f;
            _lastTankMult = -1f;
        }
    }
}
