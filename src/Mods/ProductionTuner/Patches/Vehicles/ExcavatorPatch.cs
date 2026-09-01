using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Vehicles
{
    /// <summary>
    /// Scales excavator digging capacity without enlarging the physical excavation blade collider.
    /// Employs a zero-allocation fast exit path to guarantee 0 FPS overhead in runtime loops.
    /// </summary>
    [HarmonyPatch(typeof(Koparka), "Update")]
    public static class ExcavatorPatch
    {
        private static readonly Dictionary<int, float> BaseVolumes = new Dictionary<int, float>();
        private static float _lastMultiplier = -1f;

        [HarmonyPostfix]
        public static void Postfix(Koparka __instance)
        {
            if (__instance == null) return;

            var digging = __instance.Digging;
            if (digging == null) return;

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.ExcavatorDigSpeedMultiplier
                : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (BaseVolumes.TryGetValue(id, out float baseVol))
            {
                if (multiplier == _lastMultiplier) return;
                digging._maxShovelVolume = baseVol * multiplier;
                return;
            }

            baseVol = digging._maxShovelVolume;
            BaseVolumes[id] = baseVol;
            digging._maxShovelVolume = baseVol * multiplier;
            _lastMultiplier = multiplier;
        }

        public static void Reset()
        {
            BaseVolumes.Clear();
            _lastMultiplier = -1f;
        }
    }
}
