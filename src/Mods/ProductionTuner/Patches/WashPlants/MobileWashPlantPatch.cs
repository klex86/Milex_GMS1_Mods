using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales capacity (MaxFill) and processing speed (FillSpeed) for MobileWashplant and MiniWashplant.
    /// Employs a zero-allocation fast exit path to guarantee 0 FPS overhead in runtime loops.
    /// </summary>
    public static class MobileWashPlantPatch
    {
        private struct PlantBase
        {
            public float BaseFill;
            public float BaseSpeed;
        }

        private static readonly Dictionary<int, PlantBase> BaseValues = new Dictionary<int, PlantBase>();
        private static float _lastCapMultiplier = -1f;
        private static float _lastSpdMultiplier = -1f;

        [HarmonyPatch(typeof(GoldDigger.MobileWashplant), "Update")]
        public static class MobileWashplantSubPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MobileWashplant __instance)
            {
                if (__instance == null) return;
                ApplyValues(__instance.GetInstanceID(), ref __instance.MaxFill, ref __instance.FillSpeed);
            }
        }

        [HarmonyPatch(typeof(GoldDigger.MiniWashplant), "Update")]
        public static class MiniWashplantSubPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MiniWashplant __instance)
            {
                if (__instance == null) return;
                ApplyValues(__instance.GetInstanceID(), ref __instance.MaxFill, ref __instance.FillSpeed);
            }
        }

        private static void ApplyValues(int id, ref float maxFill, ref float fillSpeed)
        {
            float capMultiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.MobileWashPlantCapacityMultiplier
                : 1f;

            float spdMultiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.MobileWashPlantSpeedMultiplier
                : 1f;

            // Zero-allocation fast-path
            if (BaseValues.TryGetValue(id, out var baseVal))
            {
                if (capMultiplier == _lastCapMultiplier && spdMultiplier == _lastSpdMultiplier) return;
                maxFill = baseVal.BaseFill * capMultiplier;
                fillSpeed = baseVal.BaseSpeed * spdMultiplier;
                return;
            }

            baseVal = new PlantBase
            {
                BaseFill = maxFill,
                BaseSpeed = fillSpeed
            };
            BaseValues[id] = baseVal;
            maxFill = baseVal.BaseFill * capMultiplier;
            fillSpeed = baseVal.BaseSpeed * spdMultiplier;
            _lastCapMultiplier = capMultiplier;
            _lastSpdMultiplier = spdMultiplier;
        }

        public static void Reset()
        {
            BaseValues.Clear();
            _lastCapMultiplier = -1f;
            _lastSpdMultiplier = -1f;
        }
    }
}
