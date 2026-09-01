using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales Hog Pan dirt capacity while protecting water consumption rate from accelerating.
    /// Employs a zero-allocation fast exit path to guarantee 0 FPS overhead in runtime loops.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.HogPanDirtBox), "Update")]
    public static class HogPanDirtBoxPatch
    {
        private static readonly Dictionary<int, float> BaseCaps = new Dictionary<int, float>();
        private static float _lastMultiplier = -1f;

        [HarmonyPostfix]
        public static void UpdatePostfix(GoldDigger.HogPanDirtBox __instance)
        {
            if (__instance == null) return;

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.HogPanCapacityMultiplier
                : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (BaseCaps.TryGetValue(id, out float baseCap))
            {
                if (multiplier == _lastMultiplier) return;
                __instance.PlaneVolumeMax = baseCap * multiplier;
                return;
            }

            baseCap = __instance.PlaneVolumeMax;
            BaseCaps[id] = baseCap;
            __instance.PlaneVolumeMax = baseCap * multiplier;
            _lastMultiplier = multiplier;
        }

        public static float GetBaseCap(int id, float fallback)
        {
            return BaseCaps.TryGetValue(id, out float baseCap) ? baseCap : fallback;
        }

        public static void Reset()
        {
            BaseCaps.Clear();
            _lastMultiplier = -1f;
        }

        /// <summary>
        /// Sub-patch on ProcessPlane to refund excess water drainage caused by the enlarged PlaneVolumeMax.
        /// Direct typed access with 0 reflection overhead.
        /// </summary>
        [HarmonyPatch(typeof(GoldDigger.HogPanDirtBox), "ProcessPlane")]
        public static class ProcessPlaneWaterGuardPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.HogPanDirtBox __instance)
            {
                if (__instance == null) return;

                float currentCap = __instance.PlaneVolumeMax;
                float baseCap = GetBaseCap(__instance.GetInstanceID(), 10f);

                if (currentCap > baseCap)
                {
                    float excessDrain = Time.deltaTime * ((currentCap - baseCap) / 7.5f);
                    __instance.WaterVolume += excessDrain;
                }
            }
        }
    }
}
