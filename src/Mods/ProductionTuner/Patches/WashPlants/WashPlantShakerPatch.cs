using System.Collections.Generic;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales capacity (MaxFill) and processing speed (FillSpeed) for all large washplant shakers (Tier 1–Tier 4).
    /// Employs a zero-allocation fast exit path and clean vanilla state restoration.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.WashplantShakerBase), "Update")]
    public static class WashPlantShakerPatch
    {
        private struct ShakerBase
        {
            public GoldDigger.WashplantShakerBase Instance;
            public float BaseFill;
            public float BaseSpeed;
        }

        private static readonly Dictionary<int, ShakerBase> BaseValues = new Dictionary<int, ShakerBase>();
        private static float _lastCapMultiplier = -1f;
        private static float _lastSpdMultiplier = -1f;

        [HarmonyPostfix]
        public static void Postfix(GoldDigger.WashplantShakerBase __instance)
        {
            if (__instance == null) return;

            float capMultiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.WashplantCapacityMultiplier
                : 1f;

            float spdMultiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.WashplantSpeedMultiplier
                : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (BaseValues.TryGetValue(id, out var baseVal))
            {
                if (capMultiplier == _lastCapMultiplier && spdMultiplier == _lastSpdMultiplier) return;
                __instance.MaxFill = baseVal.BaseFill * capMultiplier;
                __instance.FillSpeed = baseVal.BaseSpeed * spdMultiplier;
                return;
            }

            if (OrangeBeastFilter.IsOrangeBeastPart(__instance)) return;

            baseVal = new ShakerBase
            {
                Instance = __instance,
                BaseFill = __instance.MaxFill,
                BaseSpeed = __instance.FillSpeed
            };
            BaseValues[id] = baseVal;
            __instance.MaxFill = baseVal.BaseFill * capMultiplier;
            __instance.FillSpeed = baseVal.BaseSpeed * spdMultiplier;
            _lastCapMultiplier = capMultiplier;
            _lastSpdMultiplier = spdMultiplier;
        }

        public static void RestoreVanilla()
        {
            foreach (var data in BaseValues.Values)
            {
                if (data.Instance != null)
                {
                    data.Instance.MaxFill = data.BaseFill;
                    data.Instance.FillSpeed = data.BaseSpeed;
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
