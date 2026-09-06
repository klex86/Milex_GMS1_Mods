using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Tools
{
    /// <summary>
    /// Scales hand shovel volume and blade surface area using pristine vanilla constants (0.1f volume, 0.2f blade bounds).
    /// Digging fills the enlarged volume proportionally without savegame drift.
    /// </summary>
    public static class ShovelPatch
    {
        public const float VanillaShovelVolume = 0.1f;
        public const float VanillaBladeSize = 0.2f;

        private static readonly Dictionary<int, GoldDigger.Shovel> Tracked =
            new Dictionary<int, GoldDigger.Shovel>();
        private static readonly AccessTools.FieldRef<GoldDigger.Shovel, float> BladeXRef =
            AccessTools.FieldRefAccess<GoldDigger.Shovel, float>("_bladeSizex");
        private static readonly AccessTools.FieldRef<GoldDigger.Shovel, float> BladeZRef =
            AccessTools.FieldRefAccess<GoldDigger.Shovel, float>("_bladeSizez");

        private static float _lastMultiplier = -1f;

        // -------------------------------------------------------------------------
        // Update() Postfix — Safe initial scaling on first sight & live multiplier changes
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.Shovel), "Update")]
        public static class ShovelUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.Shovel __instance)
            {
                if (__instance == null) return;

                int id = __instance.GetInstanceID();
                float multiplier = ProductionTunerPlugin.Service?.ShovelFillSpeedMultiplier ?? 1f;

                if (!Tracked.ContainsKey(id))
                {
                    Tracked[id] = __instance;
                    ApplyShovel(__instance, multiplier);
                }

                if (multiplier != _lastMultiplier)
                {
                    _lastMultiplier = multiplier;
                    foreach (var shovel in Tracked.Values)
                    {
                        if (shovel != null)
                        {
                            ApplyShovel(shovel, multiplier);
                        }
                    }
                }
            }
        }

        private static void ApplyShovel(GoldDigger.Shovel shovel, float? overrideMultiplier = null)
        {
            if (shovel == null) return;

            int id = shovel.GetInstanceID();
            Tracked[id] = shovel;

            float multiplier = overrideMultiplier ?? ProductionTunerPlugin.Service?.ShovelFillSpeedMultiplier ?? 1f;
            shovel.MaxVolume = VanillaShovelVolume * multiplier;

            float bladeScale = Mathf.Sqrt(Mathf.Max(1f, multiplier));
            if (BladeXRef != null)
            {
                BladeXRef(shovel) = VanillaBladeSize * bladeScale;
            }
            if (BladeZRef != null)
            {
                BladeZRef(shovel) = VanillaBladeSize * bladeScale;
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var shovel in Tracked.Values)
            {
                if (shovel != null)
                {
                    ApplyShovel(shovel, 1f);
                }
            }
            Tracked.Clear();
            _lastMultiplier = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
        }
    }
}
