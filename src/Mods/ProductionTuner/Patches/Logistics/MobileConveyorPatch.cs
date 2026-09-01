using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales buffer capacity (MaxVolume) and transport speed (Speed) for the large mobile conveyors
    /// (Frankenstein excavator belt and Cordylus robot carrier belt).
    /// Distinguishes between Frankenstein and Cordylus via parent machine hierarchy.
    /// Employs a zero-allocation fast exit path to guarantee 0 FPS overhead in runtime loops.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.FrankensteinBelt), "Update")]
    public static class MobileConveyorPatch
    {
        private struct ConveyorBase
        {
            public float BaseVolume;
            public float BaseSpeed;
            public bool IsCordylus;
        }

        private static readonly Dictionary<int, ConveyorBase> BaseValues = new Dictionary<int, ConveyorBase>();
        private static float _lastFrankCapMult = -1f;
        private static float _lastFrankSpdMult = -1f;
        private static float _lastCordCapMult = -1f;
        private static float _lastCordSpdMult = -1f;

        [HarmonyPostfix]
        public static void Postfix(GoldDigger.FrankensteinBelt __instance)
        {
            if (__instance == null) return;

            var service = ProductionTunerPlugin.Service;
            float frankCapMult = service != null ? service.FrankensteinCapacityMultiplier : 1f;
            float frankSpdMult = service != null ? service.FrankensteinSpeedMultiplier : 1f;
            float cordCapMult = service != null ? service.CordylusCapacityMultiplier : 1f;
            float cordSpdMult = service != null ? service.CordylusSpeedMultiplier : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (BaseValues.TryGetValue(id, out var data))
            {
                if (data.IsCordylus)
                {
                    if (cordCapMult == _lastCordCapMult && cordSpdMult == _lastCordSpdMult) return;
                    __instance.MaxVolume = data.BaseVolume * cordCapMult;
                    __instance.Speed = data.BaseSpeed * cordSpdMult;
                }
                else
                {
                    if (frankCapMult == _lastFrankCapMult && frankSpdMult == _lastFrankSpdMult) return;
                    __instance.MaxVolume = data.BaseVolume * frankCapMult;
                    __instance.Speed = data.BaseSpeed * frankSpdMult;
                }
                return;
            }

            // First-time registration
            bool isCord = __instance.GetComponentInParent<GoldDigger.MaximusMachineController>() != null;

            data = new ConveyorBase
            {
                BaseVolume = __instance.MaxVolume,
                BaseSpeed = __instance.Speed,
                IsCordylus = isCord
            };
            BaseValues[id] = data;

            float capMult = isCord ? cordCapMult : frankCapMult;
            float spdMult = isCord ? cordSpdMult : frankSpdMult;

            __instance.MaxVolume = data.BaseVolume * capMult;
            __instance.Speed = data.BaseSpeed * spdMult;

            _lastFrankCapMult = frankCapMult;
            _lastFrankSpdMult = frankSpdMult;
            _lastCordCapMult = cordCapMult;
            _lastCordSpdMult = cordSpdMult;
        }

        public static void Reset()
        {
            BaseValues.Clear();
            _lastFrankCapMult = -1f;
            _lastFrankSpdMult = -1f;
            _lastCordCapMult = -1f;
            _lastCordSpdMult = -1f;
        }
    }
}
