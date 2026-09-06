using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales Hog Pan dirt capacity while protecting water consumption rate from accelerating.
    /// Uses the fixed vanilla capacity (10.0f) from Assembly-CSharp to guard against savegame drift.
    /// </summary>
    public static class HogPanDirtBoxPatch
    {
        public const float VanillaHogPanCapacity = 10.0f;

        private static readonly Dictionary<int, GoldDigger.HogPanDirtBox> Tracked =
            new Dictionary<int, GoldDigger.HogPanDirtBox>();
        private static float _lastMultiplier = -1f;

        // -------------------------------------------------------------------------
        // Update() Postfix — Safe initial scaling on first sight & live multiplier changes
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.HogPanDirtBox), "Update")]
        public static class HogPanUpdatePatch
        {
            [HarmonyPostfix]
            public static void UpdatePostfix(GoldDigger.HogPanDirtBox __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();

                float multiplier = ProductionTunerPlugin.Service?.HogPanCapacityMultiplier ?? 1f;

                if (!Tracked.ContainsKey(id))
                {
                    Tracked[id] = __instance;
                    __instance.PlaneVolumeMax = VanillaHogPanCapacity * multiplier;
                }

                if (multiplier != _lastMultiplier)
                {
                    _lastMultiplier = multiplier;
                    foreach (var hogPan in Tracked.Values)
                    {
                        if (hogPan != null)
                        {
                            hogPan.PlaneVolumeMax = VanillaHogPanCapacity * multiplier;
                        }
                    }
                }
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var hogPan in Tracked.Values)
            {
                if (hogPan != null)
                {
                    hogPan.PlaneVolumeMax = VanillaHogPanCapacity;
                }
            }
            Tracked.Clear();
            _lastMultiplier = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
        }

        /// <summary>
        /// Sub-patch on ProcessPlane to refund excess water drainage caused by the enlarged PlaneVolumeMax.
        /// </summary>
        [HarmonyPatch(typeof(GoldDigger.HogPanDirtBox), "ProcessPlane")]
        public static class ProcessPlaneWaterGuardPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.HogPanDirtBox __instance)
            {
                if (__instance == null) return;

                float currentCap = __instance.PlaneVolumeMax;
                if (currentCap > VanillaHogPanCapacity)
                {
                    float excessDrain = Time.deltaTime * ((currentCap - VanillaHogPanCapacity) / 7.5f);
                    __instance.WaterVolume += excessDrain;
                }
            }
        }
    }
}