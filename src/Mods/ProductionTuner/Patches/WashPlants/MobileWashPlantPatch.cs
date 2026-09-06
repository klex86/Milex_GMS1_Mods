using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales capacity (MaxFill) and processing speed (FillSpeed) for MobileWashplant and MiniWashplant.
    /// Save/load safe: Start() prefix ensures multipliers are applied before vanilla code can clamp
    /// serialized CurrentFill values against the unmodified vanilla MaxFill on game load.
    /// </summary>
    public static class MobileWashPlantPatch
    {
        private struct PlantBase
        {
            public object Instance;
            public float BaseFill;
            public float BaseSpeed;
        }

        private static readonly Dictionary<int, PlantBase> BaseValues = new Dictionary<int, PlantBase>();
        private static float _lastCapMultiplier = -1f;
        private static float _lastSpdMultiplier = -1f;

        // -------------------------------------------------------------------------
        // Start() Prefixes — apply multipliers before vanilla code runs
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.MobileWashplant), "Start")]
        public static class MobileWashplantStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MobileWashplant __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();
                if (BaseValues.ContainsKey(id)) return;
                float capMult = ProductionTunerPlugin.Service?.MobileWashPlantCapacityMultiplier ?? 1f;
                float spdMult = ProductionTunerPlugin.Service?.MobileWashPlantSpeedMultiplier ?? 1f;
                var baseVal = new PlantBase { Instance = __instance, BaseFill = __instance.MaxFill, BaseSpeed = __instance.FillSpeed };
                BaseValues[id] = baseVal;
                __instance.MaxFill = baseVal.BaseFill * capMult;
                __instance.FillSpeed = baseVal.BaseSpeed * spdMult;
                _lastCapMultiplier = capMult;
                _lastSpdMultiplier = spdMult;
            }
        }

        [HarmonyPatch(typeof(GoldDigger.MiniWashplant), "Start")]
        public static class MiniWashplantStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MiniWashplant __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();
                if (BaseValues.ContainsKey(id)) return;
                float capMult = ProductionTunerPlugin.Service?.MobileWashPlantCapacityMultiplier ?? 1f;
                float spdMult = ProductionTunerPlugin.Service?.MobileWashPlantSpeedMultiplier ?? 1f;
                var baseVal = new PlantBase { Instance = __instance, BaseFill = __instance.MaxFill, BaseSpeed = __instance.FillSpeed };
                BaseValues[id] = baseVal;
                __instance.MaxFill = baseVal.BaseFill * capMult;
                __instance.FillSpeed = baseVal.BaseSpeed * spdMult;
                _lastCapMultiplier = capMult;
                _lastSpdMultiplier = spdMult;
            }
        }

        // -------------------------------------------------------------------------
        // Update() Postfixes — fast-path for live multiplier changes in the in-game menu
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.MobileWashplant), "Update")]
        public static class MobileWashplantUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MobileWashplant __instance)
            {
                if (__instance == null) return;
                ApplyValues(__instance, __instance.GetInstanceID(), ref __instance.MaxFill, ref __instance.FillSpeed);
            }
        }

        [HarmonyPatch(typeof(GoldDigger.MiniWashplant), "Update")]
        public static class MiniWashplantUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MiniWashplant __instance)
            {
                if (__instance == null) return;
                ApplyValues(__instance, __instance.GetInstanceID(), ref __instance.MaxFill, ref __instance.FillSpeed);
            }
        }

        private static void ApplyValues(object instance, int id, ref float maxFill, ref float fillSpeed)
        {
            float capMultiplier = ProductionTunerPlugin.Service?.MobileWashPlantCapacityMultiplier ?? 1f;
            float spdMultiplier = ProductionTunerPlugin.Service?.MobileWashPlantSpeedMultiplier ?? 1f;

            // Zero-allocation fast-path
            if (BaseValues.TryGetValue(id, out var baseVal))
            {
                if (capMultiplier == _lastCapMultiplier && spdMultiplier == _lastSpdMultiplier) return;
                maxFill = baseVal.BaseFill * capMultiplier;
                fillSpeed = baseVal.BaseSpeed * spdMultiplier;
                _lastCapMultiplier = capMultiplier;
                _lastSpdMultiplier = spdMultiplier;
                return;
            }

            // Fallback registration
            baseVal = new PlantBase { Instance = instance, BaseFill = maxFill, BaseSpeed = fillSpeed };
            BaseValues[id] = baseVal;
            maxFill = baseVal.BaseFill * capMultiplier;
            fillSpeed = baseVal.BaseSpeed * spdMultiplier;
            _lastCapMultiplier = capMultiplier;
            _lastSpdMultiplier = spdMultiplier;
        }

        public static void RestoreVanilla()
        {
            foreach (var data in BaseValues.Values)
            {
                if (data.Instance is GoldDigger.MobileWashplant m && m != null)
                {
                    m.MaxFill = data.BaseFill;
                    m.FillSpeed = data.BaseSpeed;
                }
                else if (data.Instance is GoldDigger.MiniWashplant mini && mini != null)
                {
                    mini.MaxFill = data.BaseFill;
                    mini.FillSpeed = data.BaseSpeed;
                }
            }
            _lastCapMultiplier = 1f;
            _lastSpdMultiplier = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
            BaseValues.Clear();
            _lastCapMultiplier = -1f;
            _lastSpdMultiplier = -1f;
        }
    }
}
