using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales mobile fuel trailer capacity (1000.0f), stationary fuel tank capacity (10000.0f),
    /// refuel pump speed, and physical fuel hose reach — using pristine vanilla constants
    /// from Assembly-CSharp. Prevents savegame drift and multi-instance desynchronization.
    /// </summary>
    public static class FuelTrailerPatch
    {
        /// <summary>Genuine vanilla capacity of the mobile fuel trailer in liters.</summary>
        public const float VanillaTrailerCapacity = 1000f;

        /// <summary>Genuine vanilla capacity of stationary fuel stations/tanks on the claim in liters.</summary>
        public const float VanillaStationaryTankCapacity = 10000f;

        // Mobile fuel trailer (End_Bottom, child of a Trailer component)
        private static readonly Dictionary<int, GoldDigger.FuelStationController> TrackedTrailers =
            new Dictionary<int, GoldDigger.FuelStationController>();

        // Stationary fuel tanks (all other FuelStationController instances)
        private static readonly Dictionary<int, GoldDigger.FuelStationController> TrackedStationary =
            new Dictionary<int, GoldDigger.FuelStationController>();

        // Fuel pistol tanking speed
        private static readonly Dictionary<int, (GoldDigger.FuelPistolHoldable instance, float baseSpeed)> TrackedPistols =
            new Dictionary<int, (GoldDigger.FuelPistolHoldable, float)>();

        private static float _lastTrailerMult = -1f;
        private static float _lastTankMult = -1f;

        /// <summary>
        /// Detects mobile fuel trailer by machine type and balance sheet key.
        /// </summary>
        private static bool IsMobileTrailer(GoldDigger.FuelStationController fsc)
        {
            if (fsc == null) return false;
            var trailer = fsc.GetComponentInParent<GoldDigger.Trailer>();
            if (trailer != null && trailer.MyMachineType == MachineType.TrailerFuel)
                return true;
            return fsc.MaxCapacityPropertyDrawerKey == "TRAILER_FUELTANK_FUELMAXCAPACITY";
        }

        // -------------------------------------------------------------------------
        // Start() Postfix — Safe initial scaling on spawn/load
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.FuelStationController), "Start")]
        public static class FuelStationStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.FuelStationController __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();

                if (IsMobileTrailer(__instance))
                {
                    TrackedTrailers[id] = __instance;
                    float multiplier = ProductionTunerPlugin.Service?.FuelTrailerCapacityMultiplier ?? 1f;
                    __instance.MaxCapacity = VanillaTrailerCapacity * multiplier;
                }
                else
                {
                    TrackedStationary[id] = __instance;
                    float multiplier = ProductionTunerPlugin.Service?.FuelTankCapacityMultiplier ?? 1f;
                    __instance.MaxCapacity = VanillaStationaryTankCapacity * multiplier;
                }
            }
        }

        // -------------------------------------------------------------------------
        // Update() Postfix — Live multiplier changes in the in-game menu
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.FuelStationController), "Update")]
        public static class FuelStationUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.FuelStationController __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();

                float trailerMult = ProductionTunerPlugin.Service?.FuelTrailerCapacityMultiplier ?? 1f;
                float tankMult = ProductionTunerPlugin.Service?.FuelTankCapacityMultiplier ?? 1f;

                if (IsMobileTrailer(__instance))
                {
                    if (!TrackedTrailers.ContainsKey(id))
                    {
                        TrackedTrailers[id] = __instance;
                        __instance.MaxCapacity = VanillaTrailerCapacity * trailerMult;
                    }
                }
                else
                {
                    if (!TrackedStationary.ContainsKey(id))
                    {
                        TrackedStationary[id] = __instance;
                        __instance.MaxCapacity = VanillaStationaryTankCapacity * tankMult;
                    }
                }

                if (trailerMult != _lastTrailerMult || tankMult != _lastTankMult)
                {
                    _lastTrailerMult = trailerMult;
                    _lastTankMult = tankMult;

                    foreach (var trailer in TrackedTrailers.Values)
                    {
                        if (trailer != null)
                        {
                            trailer.MaxCapacity = VanillaTrailerCapacity * trailerMult;
                        }
                    }

                    foreach (var tank in TrackedStationary.Values)
                    {
                        if (tank != null)
                        {
                            tank.MaxCapacity = VanillaStationaryTankCapacity * tankMult;
                        }
                    }
                }
            }
        }

        // -------------------------------------------------------------------------
        // FuelPistol Attach Postfix — scales tanking speed safely
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

                // --- Tanking speed ---
                if (!TrackedPistols.TryGetValue(id, out var speedData))
                {
                    speedData = (__instance, __instance.TankingSpeed);
                    TrackedPistols[id] = speedData;
                }
                __instance.TankingSpeed = speedData.baseSpeed * Mathf.Max(1f, speedMult);
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var trailer in TrackedTrailers.Values)
            {
                if (trailer != null)
                {
                    trailer.MaxCapacity = VanillaTrailerCapacity;
                }
            }

            foreach (var tank in TrackedStationary.Values)
            {
                if (tank != null)
                {
                    tank.MaxCapacity = VanillaStationaryTankCapacity;
                }
            }

            foreach (var kvp in TrackedPistols.Values)
            {
                if (kvp.instance != null)
                {
                    kvp.instance.TankingSpeed = kvp.baseSpeed;
                }
            }

            _lastTrailerMult = 1f;
            _lastTankMult = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
            TrackedTrailers.Clear();
            TrackedStationary.Clear();
            TrackedPistols.Clear();
            _lastTrailerMult = -1f;
            _lastTankMult = -1f;
        }
    }
}
