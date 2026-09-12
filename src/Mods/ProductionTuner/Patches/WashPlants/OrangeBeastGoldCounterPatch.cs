using System.Collections.Generic;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants
{
    /// <summary>
    /// Synchronizes the T6 Orange Beast wash plant setup:
    /// - 20 Orange Beast Miner's Moss mats (baseline 0.900 m3)
    /// - OrangeBeastWashPlantGoldCounter.MinerMossMaxGroundVolume
    /// </summary>
    public static class OrangeBeastGoldCounterPatch
    {
        public const float VanillaOrangeBeastMatsCapacity = 0.900f;

        private static readonly HashSet<int> TrackedIds = new HashSet<int>();
        private static readonly List<GoldDigger.OrangeBeastWashPlantGoldCounter> TrackedCounters =
            new List<GoldDigger.OrangeBeastWashPlantGoldCounter>();

        [HarmonyPatch(typeof(GoldDigger.OrangeBeastWashPlantGoldCounter), "Update")]
        public static class OrangeBeastUpdatePatch
        {
            [HarmonyPrefix]
            public static void Prefix(GoldDigger.OrangeBeastWashPlantGoldCounter __instance)
            {
                if (__instance == null) return;

                int id = __instance.GetInstanceID();
                if (!TrackedIds.Contains(id))
                {
                    TrackedIds.Add(id);
                    TrackedCounters.Add(__instance);
                }

                float mult = ProductionTunerPlugin.Service?.WashplantT6OrangeBeastCapacityMultiplier ?? 1f;
                __instance.MinerMossMaxGroundVolume = VanillaOrangeBeastMatsCapacity * mult;
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var counter in TrackedCounters)
            {
                if (counter != null)
                {
                    counter.MinerMossMaxGroundVolume = VanillaOrangeBeastMatsCapacity;
                }
            }

            TrackedIds.Clear();
            TrackedCounters.Clear();
        }

        public static void Reset()
        {
            RestoreVanilla();
        }
    }
}
