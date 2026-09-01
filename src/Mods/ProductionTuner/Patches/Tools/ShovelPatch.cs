using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Tools
{
    /// <summary>
    /// Scales hand shovel volume and blade surface area so that digging fills the enlarged volume proportionally.
    /// Employs a zero-allocation fast exit path and clean vanilla state restoration.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.Shovel), "Update")]
    public static class ShovelPatch
    {
        private struct ShovelBase
        {
            public GoldDigger.Shovel Instance;
            public float BaseVolume;
            public float BaseBladeX;
            public float BaseBladeZ;
        }

        private static readonly Dictionary<int, ShovelBase> BaseValues = new Dictionary<int, ShovelBase>();
        private static readonly AccessTools.FieldRef<GoldDigger.Shovel, float> BladeXRef =
            AccessTools.FieldRefAccess<GoldDigger.Shovel, float>("_bladeSizex");
        private static readonly AccessTools.FieldRef<GoldDigger.Shovel, float> BladeZRef =
            AccessTools.FieldRefAccess<GoldDigger.Shovel, float>("_bladeSizez");

        private static float _lastMultiplier = -1f;

        [HarmonyPostfix]
        public static void Postfix(GoldDigger.Shovel __instance)
        {
            if (__instance == null) return;

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.ShovelFillSpeedMultiplier
                : 1f;

            int id = __instance.GetInstanceID();

            // Zero-allocation fast-path
            if (BaseValues.TryGetValue(id, out var baseVal))
            {
                if (multiplier == _lastMultiplier) return;
                ApplyShovel(__instance, baseVal, multiplier);
                return;
            }

            float curBladeX = BladeXRef != null ? BladeXRef(__instance) : 0.2f;
            float curBladeZ = BladeZRef != null ? BladeZRef(__instance) : 0.2f;

            baseVal = new ShovelBase
            {
                Instance = __instance,
                BaseVolume = __instance.MaxVolume,
                BaseBladeX = curBladeX,
                BaseBladeZ = curBladeZ
            };
            BaseValues[id] = baseVal;
            ApplyShovel(__instance, baseVal, multiplier);
            _lastMultiplier = multiplier;
        }

        private static void ApplyShovel(GoldDigger.Shovel shovel, ShovelBase baseVal, float multiplier)
        {
            if (shovel == null) return;

            shovel.MaxVolume = baseVal.BaseVolume * multiplier;

            float bladeScale = Mathf.Sqrt(Mathf.Max(1f, multiplier));
            if (BladeXRef != null)
            {
                BladeXRef(shovel) = baseVal.BaseBladeX * bladeScale;
            }
            if (BladeZRef != null)
            {
                BladeZRef(shovel) = baseVal.BaseBladeZ * bladeScale;
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var data in BaseValues.Values)
            {
                if (data.Instance != null)
                {
                    ApplyShovel(data.Instance, data, 1f);
                }
            }
            _lastMultiplier = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
            BaseValues.Clear();
            _lastMultiplier = -1f;
        }
    }
}
