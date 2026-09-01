using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Vehicles
{
    /// <summary>
    /// Scales Backhoe Loader (KoparkoLadowarka) front loader bucket and rear excavator arm shovel volumes.
    /// Employs a zero-allocation fast exit path to guarantee 0 FPS overhead in runtime loops.
    /// </summary>
    [HarmonyPatch(typeof(KoparkoLadowarka), "Update")]
    public static class BackhoeLoaderPatch
    {
        private struct BackhoeBase
        {
            public float FrontVol;
            public float RearVol;
        }

        private static readonly Dictionary<int, BackhoeBase> BaseVolumes = new Dictionary<int, BackhoeBase>();
        private static float _lastMultiplier = -1f;

        [HarmonyPostfix]
        public static void Postfix(KoparkoLadowarka __instance)
        {
            if (__instance == null) return;

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.BackhoeLoaderLoadSpeedMultiplier
                : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (BaseVolumes.TryGetValue(id, out var baseVol))
            {
                if (multiplier == _lastMultiplier) return;
                if (__instance.DiggingFront != null) __instance.DiggingFront._maxShovelVolume = baseVol.FrontVol * multiplier;
                if (__instance.Digging != null) __instance.Digging._maxShovelVolume = baseVol.RearVol * multiplier;
                return;
            }

            baseVol = new BackhoeBase
            {
                FrontVol = __instance.DiggingFront != null ? __instance.DiggingFront._maxShovelVolume : 1f,
                RearVol = __instance.Digging != null ? __instance.Digging._maxShovelVolume : 1f
            };
            BaseVolumes[id] = baseVol;

            if (__instance.DiggingFront != null) __instance.DiggingFront._maxShovelVolume = baseVol.FrontVol * multiplier;
            if (__instance.Digging != null) __instance.Digging._maxShovelVolume = baseVol.RearVol * multiplier;
            _lastMultiplier = multiplier;
        }

        public static void Reset()
        {
            BaseVolumes.Clear();
            _lastMultiplier = -1f;
        }
    }
}
