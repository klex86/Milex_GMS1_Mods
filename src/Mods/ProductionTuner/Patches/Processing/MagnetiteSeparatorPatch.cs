using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Processing
{
    /// <summary>
    /// Scales Magnetite Separator capacity (MaxFill) and output processing speed (FillOutSpeed)
    /// using pristine vanilla constants (100.0f MaxFill, 0.002f FillOutSpeed) from Assembly-CSharp.
    /// Prevents savegame drift and multi-instance desynchronization.
    /// </summary>
    public static class MagnetiteSeparatorPatch
    {
        public const float VanillaMaxFill = 100.0f;
        public const float VanillaFillOutSpeed = 0.002f;

        private static readonly Dictionary<int, GoldDigger.MagnetiteSeparator> Tracked =
            new Dictionary<int, GoldDigger.MagnetiteSeparator>();
        private static float _lastCapMultiplier = -1f;
        private static float _lastSpdMultiplier = -1f;

        // -------------------------------------------------------------------------
        // Start() Postfix — apply multipliers on spawn/load before vanilla clamping
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.MagnetiteSeparator), "Start")]
        public static class MagnetiteSeparatorStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MagnetiteSeparator __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();
                Tracked[id] = __instance;

                float capMult = ProductionTunerPlugin.Service?.MagnetiteSeparatorCapacityMultiplier ?? 1f;
                float spdMult = ProductionTunerPlugin.Service?.MagnetiteSeparatorSpeedMultiplier ?? 1f;

                __instance.MaxFill = VanillaMaxFill * capMult;
                __instance.FillOutSpeed = VanillaFillOutSpeed * spdMult;
            }
        }

        // -------------------------------------------------------------------------
        // Update() Postfix — Live multiplier changes in the in-game menu
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.MagnetiteSeparator), "Update")]
        public static class MagnetiteSeparatorUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MagnetiteSeparator __instance)
            {
                if (__instance == null) return;

                int id = __instance.GetInstanceID();
                Tracked[id] = __instance;

                float capMultiplier = ProductionTunerPlugin.Service?.MagnetiteSeparatorCapacityMultiplier ?? 1f;
                float spdMultiplier = ProductionTunerPlugin.Service?.MagnetiteSeparatorSpeedMultiplier ?? 1f;

                if (capMultiplier != _lastCapMultiplier || spdMultiplier != _lastSpdMultiplier)
                {
                    _lastCapMultiplier = capMultiplier;
                    _lastSpdMultiplier = spdMultiplier;

                    foreach (var separator in Tracked.Values)
                    {
                        if (separator != null)
                        {
                            separator.MaxFill = VanillaMaxFill * capMultiplier;
                            separator.FillOutSpeed = VanillaFillOutSpeed * spdMultiplier;
                        }
                    }
                }
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var separator in Tracked.Values)
            {
                if (separator != null)
                {
                    separator.MaxFill = VanillaMaxFill;
                    separator.FillOutSpeed = VanillaFillOutSpeed;
                }
            }
            Tracked.Clear();
            _lastCapMultiplier = 1f;
            _lastSpdMultiplier = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
        }
    }
}
