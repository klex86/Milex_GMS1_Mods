using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Vehicles
{
    /// <summary>
    /// Scales Backhoe Loader (KoparkoLadowarka) front loader bucket and rear excavator arm shovel volumes.
    /// Employs a zero-allocation fast exit path and clean vanilla state restoration.
    /// </summary>
    [HarmonyPatch(typeof(KoparkoLadowarka), "Update")]
    public static class BackhoeLoaderPatch
    {
        private struct BackhoeBase
        {
            public KoparkoLadowarka Instance;
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
                ApplyBackhoeState(__instance, baseVol, multiplier);
                return;
            }

            baseVol = new BackhoeBase
            {
                Instance = __instance,
                FrontVol = __instance.DiggingFront != null ? __instance.DiggingFront._maxShovelVolume : 1f,
                RearVol = __instance.Digging != null ? __instance.Digging._maxShovelVolume : 1f
            };
            BaseVolumes[id] = baseVol;

            ApplyBackhoeState(__instance, baseVol, multiplier);
            _lastMultiplier = multiplier;
        }

        private static void ApplyBackhoeState(KoparkoLadowarka loader, BackhoeBase baseVol, float multiplier)
        {
            if (loader == null) return;
            if (loader.DiggingFront != null) loader.DiggingFront._maxShovelVolume = baseVol.FrontVol * multiplier;
            if (loader.Digging != null) loader.Digging._maxShovelVolume = baseVol.RearVol * multiplier;
        }

        public static void RestoreVanilla()
        {
            foreach (var data in BaseVolumes.Values)
            {
                if (data.Instance != null)
                {
                    ApplyBackhoeState(data.Instance, data, 1f);
                }
            }
            _lastMultiplier = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
            BaseVolumes.Clear();
            _lastMultiplier = -1f;
        }
    }
}
