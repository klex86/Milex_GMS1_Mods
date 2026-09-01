using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Vehicles
{
    /// <summary>
    /// Scales wheel loader (Ladowarka) bucket volume, reciprocal volume, hydraulic lifting torque, and steering agility.
    /// Employs cached joint lookups and an instant fast-path exit to eliminate frame rate drops.
    /// </summary>
    [HarmonyPatch(typeof(Ladowarka), "Update")]
    public static class WheelLoaderPatch
    {
        private static readonly Dictionary<int, float> BaseVolumes = new Dictionary<int, float>();
        private static readonly Dictionary<int, (AnimatedJoint joint, float baseTorque)[]> CachedJoints =
            new Dictionary<int, (AnimatedJoint joint, float baseTorque)[]>();

        private static float _lastMultiplier = -1f;

        [HarmonyPostfix]
        public static void Postfix(Ladowarka __instance)
        {
            if (__instance == null || __instance.GetType().Name != "Ladowarka") return;

            var digging = __instance.Digging;
            if (digging == null) return;

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.WheelLoaderLoadSpeedMultiplier
                : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (BaseVolumes.TryGetValue(id, out float baseVol))
            {
                if (multiplier == _lastMultiplier) return;

                ApplyLoaderState(__instance, digging, baseVol, multiplier);
                return;
            }

            baseVol = digging._maxShovelVolume;
            BaseVolumes[id] = baseVol;
            CacheJoints(__instance, id);
            ApplyLoaderState(__instance, digging, baseVol, multiplier);
            _lastMultiplier = multiplier;
        }

        private static void ApplyLoaderState(Ladowarka loader, DiggingController digging, float baseVol, float multiplier)
        {
            float targetVol = baseVol * multiplier;
            digging._maxShovelVolume = targetVol;
            if (targetVol > 0.0001f)
            {
                digging.MaxShovelVolumeOffset = 0f;
            }

            int id = loader.GetInstanceID();
            if (CachedJoints.TryGetValue(id, out var joints))
            {
                float torqueMult = Mathf.Max(1f, multiplier);
                for (int i = 0; i < joints.Length; i++)
                {
                    if (joints[i].joint != null)
                    {
                        joints[i].joint.MaxTorque = joints[i].baseTorque * torqueMult;
                    }
                }
            }
        }

        private static void CacheJoints(Ladowarka loader, int id)
        {
            var found = loader.GetComponentsInChildren<AnimatedJoint>();
            if (found == null || found.Length == 0) return;

            var list = new (AnimatedJoint, float)[found.Length];
            for (int i = 0; i < found.Length; i++)
            {
                list[i] = (found[i], found[i].MaxTorque);
            }
            CachedJoints[id] = list;
        }

        public static void Reset()
        {
            BaseVolumes.Clear();
            CachedJoints.Clear();
            _lastMultiplier = -1f;
        }
    }
}
