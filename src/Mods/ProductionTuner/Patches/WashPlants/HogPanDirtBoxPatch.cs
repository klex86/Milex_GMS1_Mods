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
        /// Authentic vanilla baseline from Unity prefab dump: 0.09 m3 (exactly 3 standard 0.03 m3 buckets).
        public const float VanillaHogPanCapacity = 0.09f;

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
                if (__instance == null || WashPlantGoldCounterPatch.IsWashPlantHogPan(__instance)) return;
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
                        if (hogPan != null && !WashPlantGoldCounterPatch.IsWashPlantHogPan(hogPan))
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
        /// Protects against infinite water flow by only refunding water when water was actually present.
        /// </summary>
        [HarmonyPatch(typeof(GoldDigger.HogPanDirtBox), "ProcessPlane")]
        public static class ProcessPlaneWaterGuardPatch
        {
            [HarmonyPrefix]
            public static void Prefix(GoldDigger.HogPanDirtBox __instance, out float __state)
            {
                __state = __instance != null ? __instance.WaterVolume : 0f;
            }

            [HarmonyPostfix]
            public static void Postfix(GoldDigger.HogPanDirtBox __instance, float __state)
            {
                if (__instance == null) return;

                if (__state > 0.0001f)
                {
                    // Compute authentic vanilla water drain rate (10.0f / 7.5f = 1.333 l/s)
                    // and apply directly from pre-drain volume so it cleanly reaches 0 without clamp loops.
                    float vanillaDrain = Time.deltaTime * (VanillaHogPanCapacity / 7.5f);
                    __instance.WaterVolume = Mathf.Max(0f, __state - vanillaDrain);
                }
            }
        }
    }
}