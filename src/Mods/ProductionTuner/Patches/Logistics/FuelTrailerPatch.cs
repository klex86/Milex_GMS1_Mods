using System.Collections.Generic;
using System.Reflection;
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
        public const float VanillaTrailerCapacity = 2500f;

        /// <summary>Genuine vanilla capacity of stationary fuel stations/tanks on the claim in liters.</summary>
        public const float VanillaStationaryTankCapacity = 10000f;

        // Reflection accessors for internal fields in ShovelRopeDestruction
        private static readonly FieldInfo FieldMyCJoint =
            AccessTools.Field(typeof(GoldDigger.ShovelRopeDestruction), "MyCJoint");
        private static readonly FieldInfo FieldJlimit =
            AccessTools.Field(typeof(GoldDigger.ShovelRopeDestruction), "jlimit");
        private static readonly FieldInfo FieldBreakForce =
            AccessTools.Field(typeof(GoldDigger.ShovelRopeDestruction), "_breakForce");
        private static readonly FieldInfo FieldBreakTorque =
            AccessTools.Field(typeof(GoldDigger.ShovelRopeDestruction), "_breakTorque");

        // Mobile fuel trailer (End_Bottom, child of a Trailer component)
        private static readonly Dictionary<int, GoldDigger.FuelStationController> TrackedTrailers =
            new Dictionary<int, GoldDigger.FuelStationController>();

        // Stationary fuel tanks (all other FuelStationController instances)
        private static readonly Dictionary<int, GoldDigger.FuelStationController> TrackedStationary =
            new Dictionary<int, GoldDigger.FuelStationController>();

        // Fuel pistol tanking speed
        private static readonly Dictionary<int, (GoldDigger.FuelPistolHoldable instance, float baseSpeed)> TrackedPistols =
            new Dictionary<int, (GoldDigger.FuelPistolHoldable, float)>();

        // Fuel hose joints (ShovelRopeDestruction on fuel trailers and tanks)
        private class TrackedHoseData
        {
            public GoldDigger.ShovelRopeDestruction Destruction;
            public ConfigurableJoint Joint;
            public float BaseLimit;
            public float BaseBreakForce;
            public float BaseBreakTorque;
        }

        private static readonly Dictionary<int, TrackedHoseData> TrackedHoses =
            new Dictionary<int, TrackedHoseData>();

        private static float _lastTrailerMult = -1f;
        private static float _lastTankMult = -1f;
        private static float _lastHoseMult = -1f;

        /// <summary>
        /// Detects mobile fuel trailer by machine type, balance sheet key, or trailer hierarchy.
        /// </summary>
        public static bool IsMobileTrailer(GoldDigger.FuelStationController fsc)
        {
            if (fsc == null) return false;
            var trailer = fsc.GetComponentInParent<GoldDigger.Trailer>();
            if (trailer != null && trailer.MyMachineType == MachineType.TrailerFuel)
                return true;
            if (fsc.MaxCapacityPropertyDrawerKey == "TRAILER_FUELTANK_FUELMAXCAPACITY")
                return true;
            if (trailer != null && fsc.gameObject.name == "End_Bottom")
                return true;

            // Robust fallback via Fuel_Rope link when nozzle is unholstered/detached
            var pistol = fsc.GetComponent<GoldDigger.FuelPistolHoldable>() ?? fsc.GetComponentInChildren<GoldDigger.FuelPistolHoldable>();
            if (pistol != null && pistol.MyFuelRope != null)
            {
                var ropeTrailer = pistol.MyFuelRope.GetComponentInParent<GoldDigger.Trailer>();
                if (ropeTrailer != null && ropeTrailer.MyMachineType == MachineType.TrailerFuel)
                    return true;
            }

            // Characteristic fallback: mobile trailer has 3.0 L/s tanking speed vs stationary tank's 20.0 L/s
            if (fsc.gameObject.name == "End_Bottom" && fsc.MaxCapacityPropertyDrawerKey != "FUELTANK_STATIONARY_FUELMAXCAPACITY")
            {
                if (pistol != null && pistol.TankingSpeed <= 10f)
                    return true;
                if (fsc.MaxCapacity <= 3000f && fsc.MaxCapacity > 50f)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Detects the large stationary claim fuel tank (10,000L base).
        /// Explicitly excludes generators, water pumps, light trailers, Jerry cans, and vehicle tanks.
        /// </summary>
        public static bool IsStationaryClaimTank(GoldDigger.FuelStationController fsc)
        {
            if (fsc == null) return false;
            if (fsc.MaxCapacityPropertyDrawerKey == "FUELTANK_STATIONARY_FUELMAXCAPACITY")
                return true;

            // Exclusion guards: do NOT treat generators, pumps, trailers or jerry cans as stationary fuel tank
            if (IsMobileTrailer(fsc)) return false;
            if (fsc.GetComponent<GoldDigger.PowerStationController>() != null || fsc.GetComponent<GoldDigger.WaterStationController>() != null)
                return false;
            if (fsc.GetComponentInParent<GoldDigger.Trailer>() != null)
                return false;

            // Fallback for stationary claim tank in scene (named End_Bottom with fuel hose)
            if (fsc.gameObject.name == "End_Bottom" && fsc.MaxCapacity >= 5000f)
            {
                var srd = fsc.GetComponentInChildren<GoldDigger.ShovelRopeDestruction>(true);
                if (srd != null) return true;
            }

            return false;
        }

        /// <summary>
        /// Validates that a ShovelRopeDestruction component belongs to a fuel dispenser rope.
        /// </summary>
        private static bool IsFuelHose(GoldDigger.ShovelRopeDestruction srd)
        {
            if (srd == null) return false;
            if (srd.MyFuelRope != null) return true;
            if (srd.EndBottom != null && srd.EndBottom.GetComponent<GoldDigger.FuelPistolHoldable>() != null) return true;
            if (srd.GetComponentInParent<GoldDigger.Fuel_Rope>() != null) return true;
            return false;
        }

        /// <summary>
        /// Registers a fuel hose's ShovelRopeDestruction and applies the configured reach multiplier.
        /// </summary>
        public static void RegisterAndScaleHose(GoldDigger.ShovelRopeDestruction srd, float multiplier)
        {
            if (srd == null || !IsFuelHose(srd)) return;
            int id = srd.GetInstanceID();

            ConfigurableJoint cjoint = (ConfigurableJoint)FieldMyCJoint?.GetValue(srd)
                ?? srd.GetComponent<ConfigurableJoint>();
            if (cjoint == null) return;

            if (!TrackedHoses.TryGetValue(id, out var data))
            {
                data = new TrackedHoseData
                {
                    Destruction = srd,
                    Joint = cjoint,
                    BaseLimit = cjoint.linearLimit.limit,
                    BaseBreakForce = cjoint.breakForce,
                    BaseBreakTorque = cjoint.breakTorque
                };
                TrackedHoses[id] = data;
            }

            ApplyHoseScale(data, multiplier);
        }

        private static void ApplyHoseScale(TrackedHoseData data, float multiplier)
        {
            if (data == null || data.Destruction == null) return;

            ConfigurableJoint cjoint = (ConfigurableJoint)FieldMyCJoint?.GetValue(data.Destruction)
                ?? data.Joint
                ?? data.Destruction.GetComponent<ConfigurableJoint>();

            float targetLimit = data.BaseLimit * Mathf.Max(1f, multiplier);
            // Buffer breakForce and breakTorque to prevent false breakages while running or during vehicle suspension bounce
            float targetBreakForce = Mathf.Max(data.BaseBreakForce * multiplier, 50000f);
            float targetBreakTorque = Mathf.Max(data.BaseBreakTorque * multiplier, 50000f);

            if (cjoint != null)
            {
                data.Joint = cjoint;
                SoftJointLimit lim = cjoint.linearLimit;
                lim.limit = targetLimit;
                cjoint.linearLimit = lim;

                cjoint.breakForce = targetBreakForce;
                cjoint.breakTorque = targetBreakTorque;
            }

            // Update internal fields on ShovelRopeDestruction so CreateJoint() uses them
            SoftJointLimit savedLim = new SoftJointLimit { limit = targetLimit };
            FieldJlimit?.SetValue(data.Destruction, savedLim);
            FieldBreakForce?.SetValue(data.Destruction, targetBreakForce);
            FieldBreakTorque?.SetValue(data.Destruction, targetBreakTorque);

            // Wire up FuelPistolHoldable.MyConfigurableJ so Attach() checks distance and uses 5,000,000f breakForce
            if (data.Destruction.EndBottom != null)
            {
                var pistol = data.Destruction.EndBottom.GetComponent<GoldDigger.FuelPistolHoldable>();
                if (pistol != null && cjoint != null)
                {
                    pistol.MyConfigurableJ = cjoint;
                }
            }
        }

        private static void RestoreHose(TrackedHoseData data)
        {
            if (data == null || data.Destruction == null) return;

            ConfigurableJoint cjoint = (ConfigurableJoint)FieldMyCJoint?.GetValue(data.Destruction)
                ?? data.Joint
                ?? data.Destruction.GetComponent<ConfigurableJoint>();

            if (cjoint != null)
            {
                SoftJointLimit lim = cjoint.linearLimit;
                lim.limit = data.BaseLimit;
                cjoint.linearLimit = lim;
                cjoint.breakForce = data.BaseBreakForce;
                cjoint.breakTorque = data.BaseBreakTorque;
            }

            SoftJointLimit savedLim = new SoftJointLimit { limit = data.BaseLimit };
            FieldJlimit?.SetValue(data.Destruction, savedLim);
            FieldBreakForce?.SetValue(data.Destruction, data.BaseBreakForce);
            FieldBreakTorque?.SetValue(data.Destruction, data.BaseBreakTorque);
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
                else if (IsStationaryClaimTank(__instance))
                {
                    TrackedStationary[id] = __instance;
                    float multiplier = ProductionTunerPlugin.Service?.FuelTankCapacityMultiplier ?? 1f;
                    __instance.MaxCapacity = VanillaStationaryTankCapacity * multiplier;
                }
                else
                {
                    // Strictly do NOT modify capacities of portable generators, water pumps, light trailers, Jerry cans, etc.!
                    return;
                }

                // Discover fuel hose on the fuel station or trailer
                var srd = __instance.GetComponentInChildren<GoldDigger.ShovelRopeDestruction>(true);
                if (srd != null)
                {
                    float hoseMult = ProductionTunerPlugin.Service?.FuelHoseLengthMultiplier ?? 1f;
                    RegisterAndScaleHose(srd, hoseMult);
                }
            }
        }

        // -------------------------------------------------------------------------
        // ShovelRopeDestruction Awake & CreateJoint Patches — Scale physical hose reach
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.ShovelRopeDestruction), "Awake")]
        public static class ShovelRopeDestructionAwakePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.ShovelRopeDestruction __instance)
            {
                if (__instance == null) return;
                float mult = ProductionTunerPlugin.Service?.FuelHoseLengthMultiplier ?? 1f;
                RegisterAndScaleHose(__instance, mult);
            }
        }

        [HarmonyPatch(typeof(GoldDigger.ShovelRopeDestruction), "CreateJoint")]
        public static class ShovelRopeDestructionCreateJointPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.ShovelRopeDestruction __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();
                if (TrackedHoses.TryGetValue(id, out var data))
                {
                    float mult = ProductionTunerPlugin.Service?.FuelHoseLengthMultiplier ?? 1f;
                    ApplyHoseScale(data, mult);
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
                float hoseMult = ProductionTunerPlugin.Service?.FuelHoseLengthMultiplier ?? 1f;

                if (IsMobileTrailer(__instance))
                {
                    if (!TrackedTrailers.ContainsKey(id))
                    {
                        TrackedTrailers[id] = __instance;
                        __instance.MaxCapacity = VanillaTrailerCapacity * trailerMult;
                    }
                }
                else if (IsStationaryClaimTank(__instance))
                {
                    if (!TrackedStationary.ContainsKey(id))
                    {
                        TrackedStationary[id] = __instance;
                        __instance.MaxCapacity = VanillaStationaryTankCapacity * tankMult;
                    }
                }
                else
                {
                    // Not a fuel trailer or stationary claim tank - ignore completely!
                    return;
                }

                // Fallback check for fuel hose discovery
                var srd = __instance.GetComponentInChildren<GoldDigger.ShovelRopeDestruction>(true);
                if (srd != null && !TrackedHoses.ContainsKey(srd.GetInstanceID()))
                {
                    RegisterAndScaleHose(srd, hoseMult);
                }

                // Live updates for capacities
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

                // Live updates for fuel hose length
                if (hoseMult != _lastHoseMult)
                {
                    _lastHoseMult = hoseMult;
                    foreach (var hose in TrackedHoses.Values)
                    {
                        ApplyHoseScale(hose, hoseMult);
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

            foreach (var hose in TrackedHoses.Values)
            {
                RestoreHose(hose);
            }

            _lastTrailerMult = 1f;
            _lastTankMult = 1f;
            _lastHoseMult = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
            TrackedTrailers.Clear();
            TrackedStationary.Clear();
            TrackedPistols.Clear();
            TrackedHoses.Clear();
            _lastTrailerMult = -1f;
            _lastTankMult = -1f;
            _lastHoseMult = -1f;
        }
    }
}
