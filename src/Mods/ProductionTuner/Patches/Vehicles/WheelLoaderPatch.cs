using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Vehicles
{
    /// <summary>
    /// Scales wheel loader (Ladowarka) bucket volume, reciprocal volume, hydraulic lifting torque, and steering agility.
    /// Employs cached joint lookups, fast-path exit, and clean vanilla state restoration.
    /// </summary>
    [HarmonyPatch(typeof(Ladowarka), "Update")]
    public static class WheelLoaderPatch
    {
        private static readonly Dictionary<int, (Ladowarka instance, float baseVolume)> Tracked =
            new Dictionary<int, (Ladowarka, float)>();
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
            if (Tracked.TryGetValue(id, out var data))
            {
                if (multiplier == _lastMultiplier) return;

                ApplyLoaderState(__instance, digging, data.baseVolume, multiplier);
                return;
            }

            // First-time registration
            float baseVol = digging._maxShovelVolume;
            Tracked[id] = (__instance, baseVol);
            CacheJoints(__instance, id);
            ApplyLoaderState(__instance, digging, baseVol, multiplier);
            _lastMultiplier = multiplier;
        }

        private static void ApplyLoaderState(Ladowarka loader, DiggingController digging, float baseVol, float multiplier)
        {
            if (loader == null || digging == null) return;

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
            var jointList = loader.GetComponentsInChildren<AnimatedJoint>(true);
            if (jointList != null && jointList.Length > 0)
            {
                var cached = new (AnimatedJoint, float)[jointList.Length];
                for (int i = 0; i < jointList.Length; i++)
                {
                    cached[i] = (jointList[i], jointList[i].MaxTorque);
                }
                CachedJoints[id] = cached;
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var kvp in Tracked.Values)
            {
                if (kvp.instance != null && kvp.instance.Digging != null)
                {
                    ApplyLoaderState(kvp.instance, kvp.instance.Digging, kvp.baseVolume, 1f);
                }
            }
            _lastMultiplier = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
            Tracked.Clear();
            CachedJoints.Clear();
            _lastMultiplier = -1f;
        }
    }
}
