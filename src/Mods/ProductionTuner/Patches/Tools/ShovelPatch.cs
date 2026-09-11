using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Tools
{
    /// <summary>
    /// Scales hand shovel volume and digging yield aligned with the 15.0f container bucket standard.
    /// Baseline: 3 full shovel scoops (5.0f each) fill one standard bucket (15.0f).
    /// With default 6.0x multiplier, each shovel yields 30.0f, filling a 2.0x bucket (30.0f) in exactly 1 scoop.
    /// </summary>
    public static class ShovelPatch
    {
        /// Authentic vanilla baseline from Unity prefab dump: 0.01 m3 (exactly 1/3 of the 0.03 m3 bucket).
        /// 3 full shovel scoops (0.01 each) fill one standard bucket (0.03).
        public const float VanillaShovelVolume = 0.01f;
        public const float VanillaBladeSize = 0.4f;
        public const float VanillaDigDepth = 0.2f;

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

        // -------------------------------------------------------------------------
        // FixedUpdate() Prefix/Postfix — Fill shovel volume & minerals proportionally in 1 stroke
        // -------------------------------------------------------------------------
        public struct DigState
        {
            public float VolumeBefore;
            public float GoldBefore;
            public float MagBefore;
            public float DiaBefore;
        }

        [HarmonyPatch(typeof(GoldDigger.Shovel), "FixedUpdate")]
        public static class ShovelFixedUpdatePatch
        {
            [HarmonyPrefix]
            public static void Prefix(GoldDigger.Shovel __instance, out DigState __state)
            {
                __state = default;
                if (__instance == null || __instance.Minerals == null) return;

                __state = new DigState
                {
                    VolumeBefore = __instance.CurrentVolume,
                    GoldBefore = __instance.Minerals[ResourceType.Gold],
                    MagBefore = __instance.Minerals[ResourceType.Magnetite],
                    DiaBefore = __instance.Minerals[ResourceType.Diamond]
                };
            }

            [HarmonyPostfix]
            public static void Postfix(GoldDigger.Shovel __instance, DigState __state)
            {
                if (__instance == null || __instance.Minerals == null) return;

                float addedDirt = __instance.CurrentVolume - __state.VolumeBefore;
                if (addedDirt <= 0.00001f) return;

                // Smoothly fill shovel volume across physics frames during a single dig stroke
                float step = __instance.MaxVolume / 3f;
                float targetGain = Mathf.Min(step, Mathf.Max(0f, __instance.MaxVolume - __state.VolumeBefore));
                if (targetGain <= 0.00001f) return;

                float multiplierFactor = targetGain / addedDirt;
                __instance.CurrentVolume = __state.VolumeBefore + targetGain;

                float addedGold = (__instance.Minerals[ResourceType.Gold] - __state.GoldBefore) * multiplierFactor;
                float addedMag = (__instance.Minerals[ResourceType.Magnetite] - __state.MagBefore) * multiplierFactor;
                float addedDia = (__instance.Minerals[ResourceType.Diamond] - __state.DiaBefore) * multiplierFactor;

                __instance.Minerals[ResourceType.Gold] = __state.GoldBefore + addedGold;
                __instance.Minerals[ResourceType.Magnetite] = __state.MagBefore + addedMag;
                __instance.Minerals[ResourceType.Diamond] = __state.DiaBefore + addedDia;

                if (__instance.DirtPlane != null && __instance.MaxVolume > 0.0001f)
                {
                    __instance.DirtPlane.localPosition = Vector3.forward * 0.25f * (__instance.CurrentVolume / __instance.MaxVolume);
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

            // Safe dig depth scaling: slightly deepen voxel bite but clamp to prevent terrain deformation explosions
            shovel.DigDepth = Mathf.Clamp(VanillaDigDepth * Mathf.Sqrt(multiplier), VanillaDigDepth, 0.5f);

            float bladeScale = Mathf.Sqrt(Mathf.Max(1f, multiplier));
            float baseBladeX = shovel.BladesBoxCollider != null ? shovel.BladesBoxCollider.size.x * shovel.DigScaleX : VanillaBladeSize;
            float baseBladeZ = shovel.BladesBoxCollider != null ? shovel.BladesBoxCollider.size.z * shovel.DigScaleZ : -VanillaBladeSize;

            if (BladeXRef != null)
            {
                BladeXRef(shovel) = baseBladeX * bladeScale;
            }
            if (BladeZRef != null)
            {
                BladeZRef(shovel) = baseBladeZ * bladeScale;
            }
        }

        public static void RestoreVanilla()
        {
            foreach (var shovel in Tracked.Values)
            {
                if (shovel != null)
                {
                    shovel.MaxVolume = VanillaShovelVolume;
                    shovel.DigDepth = VanillaDigDepth;
                    if (BladeXRef != null) BladeXRef(shovel) = VanillaBladeSize;
                    if (BladeZRef != null) BladeZRef(shovel) = -VanillaBladeSize;
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
