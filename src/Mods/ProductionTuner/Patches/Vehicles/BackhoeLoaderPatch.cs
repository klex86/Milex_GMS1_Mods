using System.Collections.Generic;
using System.Reflection;
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
        private static readonly FieldInfo InvMaxShovelVolumeField = AccessTools.Field(typeof(DiggingController), "_invmaxShovelVolume");

        private struct BackhoeBase
        {
            public KoparkoLadowarka Instance;
            public float FrontVol;
            public float RearVol;
        }

        private static readonly Dictionary<int, BackhoeBase> Tracked = new Dictionary<int, BackhoeBase>();
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
            if (Tracked.TryGetValue(id, out var baseVol))
            {
                if (multiplier == _lastMultiplier) return;

                _lastMultiplier = multiplier;
                foreach (var entry in Tracked.Values)
                {
                    if (entry.Instance != null)
                    {
                        ApplyBackhoeState(entry.Instance, entry, multiplier);
                    }
                }
                return;
            }

            baseVol = new BackhoeBase
            {
                Instance = __instance,
                FrontVol = __instance.DiggingFront != null ? __instance.DiggingFront._maxShovelVolume : 1f,
                RearVol = __instance.Digging != null ? __instance.Digging._maxShovelVolume : 1f
            };
            Tracked[id] = baseVol;

            ApplyBackhoeState(__instance, baseVol, multiplier);
            _lastMultiplier = multiplier;
        }

        private static void ApplyBackhoeState(KoparkoLadowarka loader, BackhoeBase baseVol, float multiplier)
        {
            if (loader == null) return;

            if (loader.DiggingFront != null)
            {
                float targetFront = baseVol.FrontVol * multiplier;
                loader.DiggingFront._maxShovelVolume = targetFront;
                if (targetFront > 0.0001f && InvMaxShovelVolumeField != null)
                {
                    InvMaxShovelVolumeField.SetValue(loader.DiggingFront, 1f / targetFront);
                }
            }

            if (loader.Digging != null)
            {
                float targetRear = baseVol.RearVol * multiplier;
                loader.Digging._maxShovelVolume = targetRear;
                if (targetRear > 0.0001f && InvMaxShovelVolumeField != null)
                {
                    InvMaxShovelVolumeField.SetValue(loader.Digging, 1f / targetRear);
                }
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var data in Tracked.Values)
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
            Tracked.Clear();
            _lastMultiplier = -1f;
        }
    }
}
