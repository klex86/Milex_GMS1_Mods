using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Processing
{
    /// <summary>
    /// Scales Magnetite Separator capacity (MaxFill) and output processing speed (FillOutSpeed).
    /// Save/load safe: Start() prefix ensures multipliers are applied before vanilla code can clamp
    /// serialized CurrentFill values against the unmodified vanilla MaxFill on game load.
    /// </summary>
    public static class MagnetiteSeparatorPatch
    {
        private struct SeparatorBase
        {
            public GoldDigger.MagnetiteSeparator Instance;
            public float BaseFill;
            public float BaseSpeed;
        }

        private static readonly Dictionary<int, SeparatorBase> BaseValues = new Dictionary<int, SeparatorBase>();
        private static float _lastCapMultiplier = -1f;
        private static float _lastSpdMultiplier = -1f;

        // -------------------------------------------------------------------------
        // Start() Prefix — apply multipliers before vanilla code runs to prevent
        // CurrentFill being clamped against vanilla MaxFill on load.
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.MagnetiteSeparator), "Start")]
        public static class MagnetiteSeparatorStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MagnetiteSeparator __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();
                if (BaseValues.ContainsKey(id)) return;
                float capMult = ProductionTunerPlugin.Service?.MagnetiteSeparatorCapacityMultiplier ?? 1f;
                float spdMult = ProductionTunerPlugin.Service?.MagnetiteSeparatorSpeedMultiplier ?? 1f;
                var baseVal = new SeparatorBase { Instance = __instance, BaseFill = __instance.MaxFill, BaseSpeed = __instance.FillOutSpeed };
                BaseValues[id] = baseVal;
                __instance.MaxFill = baseVal.BaseFill * capMult;
                __instance.FillOutSpeed = baseVal.BaseSpeed * spdMult;
                _lastCapMultiplier = capMult;
                _lastSpdMultiplier = spdMult;
            }
        }

        // -------------------------------------------------------------------------
        // Update() Postfix — fast-path for live multiplier changes in the in-game menu
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.MagnetiteSeparator), "Update")]
        public static class MagnetiteSeparatorUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MagnetiteSeparator __instance)
            {
                if (__instance == null) return;

                float capMultiplier = ProductionTunerPlugin.Service?.MagnetiteSeparatorCapacityMultiplier ?? 1f;
                float spdMultiplier = ProductionTunerPlugin.Service?.MagnetiteSeparatorSpeedMultiplier ?? 1f;

                int id = __instance.GetInstanceID();

                // Zero-allocation fast-path
                if (BaseValues.TryGetValue(id, out var baseVal))
                {
                    if (capMultiplier == _lastCapMultiplier && spdMultiplier == _lastSpdMultiplier) return;
                    __instance.MaxFill = baseVal.BaseFill * capMultiplier;
                    __instance.FillOutSpeed = baseVal.BaseSpeed * spdMultiplier;
                    _lastCapMultiplier = capMultiplier;
                    _lastSpdMultiplier = spdMultiplier;
                    return;
                }

                // Fallback registration
                baseVal = new SeparatorBase
                {
                    Instance = __instance,
                    BaseFill = __instance.MaxFill,
                    BaseSpeed = __instance.FillOutSpeed
                };
                BaseValues[id] = baseVal;
                __instance.MaxFill = baseVal.BaseFill * capMultiplier;
                __instance.FillOutSpeed = baseVal.BaseSpeed * spdMultiplier;
                _lastCapMultiplier = capMultiplier;
                _lastSpdMultiplier = spdMultiplier;
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var data in BaseValues.Values)
            {
                if (data.Instance != null)
                {
                    data.Instance.MaxFill = data.BaseFill;
                    data.Instance.FillOutSpeed = data.BaseSpeed;
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
