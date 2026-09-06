using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Scales capacity (MaxFill) and processing speed (FillSpeed) for MobileWashplant and MiniWashplant
    /// using pristine vanilla constants (15.0f MaxFill, 1.0f FillSpeed) from Assembly-CSharp.
    /// Prevents savegame drift and multi-instance desynchronization.
    /// </summary>
    public static class MobileWashPlantPatch
    {
        public const float VanillaMaxFill = 15.0f;
        public const float VanillaFillSpeed = 1.0f;

        private static readonly Dictionary<int, GoldDigger.MobileWashplant> TrackedMobile =
            new Dictionary<int, GoldDigger.MobileWashplant>();
        private static readonly Dictionary<int, GoldDigger.MiniWashplant> TrackedMini =
            new Dictionary<int, GoldDigger.MiniWashplant>();

        private static float _lastCapMultiplier = -1f;
        private static float _lastSpdMultiplier = -1f;

        // -------------------------------------------------------------------------
        // Start() Postfixes — apply multipliers on spawn/load before vanilla clamping
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.MobileWashplant), "Start")]
        public static class MobileWashplantStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MobileWashplant __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();
                TrackedMobile[id] = __instance;

                float capMult = ProductionTunerPlugin.Service?.MobileWashPlantCapacityMultiplier ?? 1f;
                float spdMult = ProductionTunerPlugin.Service?.MobileWashPlantSpeedMultiplier ?? 1f;

                __instance.MaxFill = VanillaMaxFill * capMult;
                __instance.FillSpeed = VanillaFillSpeed * spdMult;
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
                TrackedMini[id] = __instance;

                float capMult = ProductionTunerPlugin.Service?.MobileWashPlantCapacityMultiplier ?? 1f;
                float spdMult = ProductionTunerPlugin.Service?.MobileWashPlantSpeedMultiplier ?? 1f;

                __instance.MaxFill = VanillaMaxFill * capMult;
                __instance.FillSpeed = VanillaFillSpeed * spdMult;
            }
        }

        // -------------------------------------------------------------------------
        // Update() Postfixes — Live multiplier changes in the in-game menu
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.MobileWashplant), "Update")]
        public static class MobileWashplantUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MobileWashplant __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();
                TrackedMobile[id] = __instance;

                SyncAllIfChanged();
            }
        }

        [HarmonyPatch(typeof(GoldDigger.MiniWashplant), "Update")]
        public static class MiniWashplantUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.MiniWashplant __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();
                TrackedMini[id] = __instance;

                SyncAllIfChanged();
            }
        }

        private static void SyncAllIfChanged()
        {
            float capMultiplier = ProductionTunerPlugin.Service?.MobileWashPlantCapacityMultiplier ?? 1f;
            float spdMultiplier = ProductionTunerPlugin.Service?.MobileWashPlantSpeedMultiplier ?? 1f;

            if (capMultiplier == _lastCapMultiplier && spdMultiplier == _lastSpdMultiplier) return;

            _lastCapMultiplier = capMultiplier;
            _lastSpdMultiplier = spdMultiplier;

            foreach (var m in TrackedMobile.Values)
            {
                if (m != null)
                {
                    m.MaxFill = VanillaMaxFill * capMultiplier;
                    m.FillSpeed = VanillaFillSpeed * spdMultiplier;
                }
            }

            foreach (var mini in TrackedMini.Values)
            {
                if (mini != null)
                {
                    mini.MaxFill = VanillaMaxFill * capMultiplier;
                    mini.FillSpeed = VanillaFillSpeed * spdMultiplier;
                }
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var m in TrackedMobile.Values)
            {
                if (m != null)
                {
                    m.MaxFill = VanillaMaxFill;
                    m.FillSpeed = VanillaFillSpeed;
                }
            }

            foreach (var mini in TrackedMini.Values)
            {
                if (mini != null)
                {
                    mini.MaxFill = VanillaMaxFill;
                    mini.FillSpeed = VanillaFillSpeed;
                }
            }

            TrackedMobile.Clear();
            TrackedMini.Clear();
            _lastCapMultiplier = 1f;
            _lastSpdMultiplier = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
        }
    }
}
