using System.Collections.Generic;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales dirt and mineral sediment capacity for Miner's Moss mats based on verified Unity baselines:
    /// - Hog Pan mats: 0.2488 m3 (scaled by HogPan_Capacity)
    /// - T3-T5 Wash Plant mats: 0.225 m3 (scaled by Washplant_T3T5_SetupCapacity)
    /// - T6 Orange Beast mats: 0.900 m3 (scaled by Washplant_T6_OrangeBeastCapacity)
    /// </summary>
    public static class MinersMossPatch
    {
        public const float VanillaHogPanMossVolume = 0.2488f;
        public const float VanillaWashPlantMossVolume = 0.225f;
        public const float VanillaOrangeBeastMossVolume = 0.900f;

        public static float GetBaseMossVolume(GoldDigger.MinersMoss moss)
        {
            if (moss == null) return VanillaWashPlantMossVolume;
            if (moss.MaxGroundVolumePropertyDrawerKey == "HOGPAN_MATS_MAX_CAPACITY")
            {
                return VanillaHogPanMossVolume;
            }
            if (moss.MaxGroundVolumePropertyDrawerKey == "WASHPLANT_OBMATS_MAX_CAPACITY" || OrangeBeastFilter.IsOrangeBeastPart(moss))
            {
                return VanillaOrangeBeastMossVolume;
            }
            return VanillaWashPlantMossVolume;
        }

        public static float GetEffectiveMultiplier(GoldDigger.MinersMoss moss)
        {
            var service = ProductionTunerPlugin.Service;
            if (service == null) return 1f;

            if (moss != null && (moss.MaxGroundVolumePropertyDrawerKey == "WASHPLANT_OBMATS_MAX_CAPACITY" || OrangeBeastFilter.IsOrangeBeastPart(moss)))
            {
                return service.WashplantT6OrangeBeastCapacityMultiplier;
            }
            if (moss != null && moss.MaxGroundVolumePropertyDrawerKey == "HOGPAN_MATS_MAX_CAPACITY")
            {
                if (WashPlantGoldCounterPatch.IsWashPlantHogPanMoss(moss))
                {
                    return service.WashplantT3T5SetupCapacityMultiplier;
                }
                return service.HogPanCapacityMultiplier;
            }
            return service.WashplantT3T5SetupCapacityMultiplier;
        }

        private static readonly Dictionary<int, GoldDigger.MinersMoss> Tracked =
            new Dictionary<int, GoldDigger.MinersMoss>();
        private static float _lastT3T5Multiplier = -1f;
        private static float _lastT6Multiplier = -1f;
        private static float _lastHogPanMultiplier = -1f;

        // -------------------------------------------------------------------------
        // Start() Postfix — apply multiplier on spawn/load before vanilla clamping
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.MinersMoss), "Start")]
        public static class MinersMossStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MinersMoss __instance)
            {
                if (__instance == null) return;

                int id = __instance.GetInstanceID();
                Tracked[id] = __instance;

                __instance.MaxGroundVolume = GetBaseMossVolume(__instance) * GetEffectiveMultiplier(__instance);
            }
        }

        // -------------------------------------------------------------------------
        // Update() Postfix — Live multiplier changes in the in-game menu
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.MinersMoss), "Update")]
        public static class MinersMossUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MinersMoss __instance)
            {
                if (__instance == null) return;

                int id = __instance.GetInstanceID();
                Tracked[id] = __instance;

                var service = ProductionTunerPlugin.Service;
                float currentT3T5 = service?.WashplantT3T5SetupCapacityMultiplier ?? 1f;
                float currentT6 = service?.WashplantT6OrangeBeastCapacityMultiplier ?? 1f;
                float currentHogPan = service?.HogPanCapacityMultiplier ?? 1f;

                if (currentT3T5 != _lastT3T5Multiplier || currentT6 != _lastT6Multiplier || currentHogPan != _lastHogPanMultiplier)
                {
                    _lastT3T5Multiplier = currentT3T5;
                    _lastT6Multiplier = currentT6;
                    _lastHogPanMultiplier = currentHogPan;

                    foreach (var moss in Tracked.Values)
                    {
                        if (moss != null)
                        {
                            moss.MaxGroundVolume = GetBaseMossVolume(moss) * GetEffectiveMultiplier(moss);
                        }
                    }
                }
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var moss in Tracked.Values)
            {
                if (moss != null)
                {
                    moss.MaxGroundVolume = GetBaseMossVolume(moss);
                }
            }
            Tracked.Clear();
            _lastT3T5Multiplier = 1f;
            _lastT6Multiplier = 1f;
            _lastHogPanMultiplier = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
        }
    }
}
