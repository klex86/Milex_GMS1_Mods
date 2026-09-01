using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Vehicles
{
    /// <summary>
    /// Scales excavator digging capacity as well as hydraulic maneuvering speeds
    /// (boom/stick extension, turret/cabin rotation, and bucket curl).
    /// Employs a zero-allocation fast exit path and clean vanilla state restoration.
    /// </summary>
    [HarmonyPatch(typeof(Koparka), "Update")]
    public static class ExcavatorPatch
    {
        private struct ExcavatorBase
        {
            public Koparka Instance;
            public float BaseVolume;
            public (Backhoe arm, float baseSpeed)[] CachedArms;
        }

        private static readonly Dictionary<int, ExcavatorBase> BaseData = new Dictionary<int, ExcavatorBase>();
        private static float _lastDigMult = -1f;
        private static float _lastArmMult = -1f;
        private static float _lastTurretMult = -1f;
        private static float _lastBucketMult = -1f;

        [HarmonyPostfix]
        public static void Postfix(Koparka __instance)
        {
            if (__instance == null) return;

            var service = ProductionTunerPlugin.Service;
            float digMult = service != null ? service.ExcavatorDigSpeedMultiplier : 1f;
            float armMult = service != null ? service.ExcavatorArmSpeedMultiplier : 1f;
            float turretMult = service != null ? service.ExcavatorTurretSpeedMultiplier : 1f;
            float bucketMult = service != null ? service.ExcavatorBucketSpeedMultiplier : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (BaseData.TryGetValue(id, out var data))
            {
                if (digMult == _lastDigMult && armMult == _lastArmMult &&
                    turretMult == _lastTurretMult && bucketMult == _lastBucketMult)
                {
                    return;
                }

                ApplyExcavatorState(__instance, data, digMult, armMult, turretMult, bucketMult);
                return;
            }

            // First-time registration: capture true vanilla values
            float baseVol = __instance.Digging != null ? __instance.Digging._maxShovelVolume : 1f;
            var cachedArms = CacheArms(__instance);

            data = new ExcavatorBase
            {
                Instance = __instance,
                BaseVolume = baseVol,
                CachedArms = cachedArms
            };
            BaseData[id] = data;

            ApplyExcavatorState(__instance, data, digMult, armMult, turretMult, bucketMult);
            _lastDigMult = digMult;
            _lastArmMult = armMult;
            _lastTurretMult = turretMult;
            _lastBucketMult = bucketMult;
        }

        private static (Backhoe arm, float baseSpeed)[] CacheArms(Koparka excavator)
        {
            if (excavator.ControledJoints == null || excavator.ControledJoints.Arms == null)
            {
                return new (Backhoe, float)[0];
            }

            var arms = excavator.ControledJoints.Arms;
            var result = new (Backhoe, float)[arms.Length];
            for (int i = 0; i < arms.Length; i++)
            {
                float baseSpd = arms[i] != null ? arms[i].AngularSpeed : 2f;
                result[i] = (arms[i], baseSpd);
            }
            return result;
        }

        private static void ApplyExcavatorState(
            Koparka excavator,
            ExcavatorBase data,
            float digMult,
            float armMult,
            float turretMult,
            float bucketMult)
        {
            if (excavator == null) return;

            if (excavator.Digging != null)
            {
                excavator.Digging._maxShovelVolume = data.BaseVolume * digMult;
            }

            if (data.CachedArms == null) return;

            for (int i = 0; i < data.CachedArms.Length; i++)
            {
                var arm = data.CachedArms[i].arm;
                if (arm == null) continue;

                float baseSpeed = data.CachedArms[i].baseSpeed;
                float targetMult = DetermineJointMultiplier(i, arm, armMult, turretMult, bucketMult);

                arm.AngularSpeed = baseSpeed * targetMult;

                if (arm.Arm != null)
                {
                    var rb = arm.Arm.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.maxAngularVelocity = (float)Math.PI / 180f * arm.AngularSpeed * arm.AngularReverseSpeedMultiply;
                    }
                }
            }
        }

        private static float DetermineJointMultiplier(
            int index,
            Backhoe arm,
            float armMult,
            float turretMult,
            float bucketMult)
        {
            string name = arm.Arm != null ? arm.Arm.name.ToLowerInvariant() : "";

            if (index == 2 || name.Contains("body") || name.Contains("rotate") || name.Contains("turret") || name.Contains("cabin"))
            {
                return turretMult;
            }

            if (index == 3 || name.Contains("bucket") || name.Contains("shovel") || name.Contains("lyzk"))
            {
                return bucketMult;
            }

            return armMult;
        }

        public static void RestoreVanilla()
        {
            foreach (var data in BaseData.Values)
            {
                if (data.Instance != null)
                {
                    ApplyExcavatorState(data.Instance, data, 1f, 1f, 1f, 1f);
                }
            }
            _lastDigMult = 1f;
            _lastArmMult = 1f;
            _lastTurretMult = 1f;
            _lastBucketMult = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
            BaseData.Clear();
            _lastDigMult = -1f;
            _lastArmMult = -1f;
            _lastTurretMult = -1f;
            _lastBucketMult = -1f;
        }
    }
}
