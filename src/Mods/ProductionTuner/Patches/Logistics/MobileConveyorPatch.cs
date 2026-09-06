using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales buffer capacity (MaxVolume) and transport throughput speed for large mobile conveyors
    /// (Frankenstein excavator belt and Cordylus robot carrier belt) using pristine vanilla constants (20.0f MaxVolume, 1.0f Speed, 0.05f OneLoadVolume).
    /// Dynamically scales chunk discharge size, spawn timer, and compensates for the secondary conveyor belt section.
    /// </summary>
    public static class MobileConveyorPatch
    {
        public const float VanillaMaxVolume = 20.0f;
        public const float VanillaSpeed = 1.0f;
        public const float VanillaOneLoadVolume = 0.05f;
        public const float VanillaTextureOffsetSpeed = 0.05f;
        public static readonly Vector2 VanillaSpawnInterval = new Vector2(0.2f, 0.4f);

        private struct ConveyorInfo
        {
            public GoldDigger.FrankensteinBelt Instance;
            public bool IsCordylus;
        }

        private static readonly Dictionary<int, ConveyorInfo> Tracked = new Dictionary<int, ConveyorInfo>();
        private static readonly AccessTools.FieldRef<GoldDigger.FrankensteinBelt, float> LastSpawnRef =
            AccessTools.FieldRefAccess<GoldDigger.FrankensteinBelt, float>("lastSpawn");

        private static float _lastFrankCapMult = -1f;
        private static float _lastFrankSpdMult = -1f;
        private static float _lastCordCapMult = -1f;
        private static float _lastCordSpdMult = -1f;

        // -------------------------------------------------------------------------
        // Start() Postfix — Safe initial scaling on spawn/load
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.FrankensteinBelt), "Start")]
        public static class MobileConveyorStartSafetyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.FrankensteinBelt __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();

                bool isCord = __instance.GetComponentInParent<GoldDigger.MaximusMachineController>() != null;
                Tracked[id] = new ConveyorInfo { Instance = __instance, IsCordylus = isCord };

                var service = ProductionTunerPlugin.Service;
                float capMult = isCord
                    ? (service?.CordylusCapacityMultiplier ?? 1f)
                    : (service?.FrankensteinCapacityMultiplier ?? 1f);
                float spdMult = isCord
                    ? (service?.CordylusSpeedMultiplier ?? 1f)
                    : (service?.FrankensteinSpeedMultiplier ?? 1f);

                ApplyParameters(__instance, capMult, spdMult);
            }
        }

        // -------------------------------------------------------------------------
        // Update() Postfix — Live multiplier changes and runtime belt speed compensation
        // -------------------------------------------------------------------------
        [HarmonyPatch(typeof(GoldDigger.FrankensteinBelt), "Update")]
        public static class MobileConveyorUpdatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(GoldDigger.FrankensteinBelt __instance)
            {
                if (__instance == null) return;
                int id = __instance.GetInstanceID();

                if (!Tracked.TryGetValue(id, out var info))
                {
                    bool isCord = __instance.GetComponentInParent<GoldDigger.MaximusMachineController>() != null;
                    info = new ConveyorInfo { Instance = __instance, IsCordylus = isCord };
                    Tracked[id] = info;
                }

                var service = ProductionTunerPlugin.Service;
                float frankCapMult = service?.FrankensteinCapacityMultiplier ?? 1f;
                float frankSpdMult = service?.FrankensteinSpeedMultiplier ?? 1f;
                float cordCapMult  = service?.CordylusCapacityMultiplier ?? 1f;
                float cordSpdMult  = service?.CordylusSpeedMultiplier ?? 1f;

                if (frankCapMult != _lastFrankCapMult || frankSpdMult != _lastFrankSpdMult ||
                    cordCapMult != _lastCordCapMult || cordSpdMult != _lastCordSpdMult)
                {
                    _lastFrankCapMult = frankCapMult;
                    _lastFrankSpdMult = frankSpdMult;
                    _lastCordCapMult  = cordCapMult;
                    _lastCordSpdMult  = cordSpdMult;

                    foreach (var entry in Tracked.Values)
                    {
                        if (entry.Instance != null)
                        {
                            float cMult = entry.IsCordylus ? cordCapMult : frankCapMult;
                            float sMult = entry.IsCordylus ? cordSpdMult : frankSpdMult;
                            ApplyParameters(entry.Instance, cMult, sMult);
                        }
                    }
                }

                // Runtime compensation: accelerate discharge spawn timer and secondary drop belt proportionally
                float activeSpdMult = info.IsCordylus ? cordSpdMult : frankSpdMult;
                if (activeSpdMult > 1f && __instance.IsEnabled)
                {
                    if (LastSpawnRef != null)
                    {
                        ref float lastSpawn = ref LastSpawnRef(__instance);
                        lastSpawn -= Time.deltaTime * __instance.SpeedMultiplier * (activeSpdMult - 1f);
                    }

                    if (__instance.CurrentObjects != null)
                    {
                        float extraProgress = Time.deltaTime * __instance.SpeedMultiplier * (activeSpdMult - 1f);
                        var objects = __instance.CurrentObjects;
                        for (int i = 0; i < objects.Count; i++)
                        {
                            var obj = objects[i];
                            if (obj != null && obj.MyPathAfterDrop != null && obj.MyPathAfterDrop.Count > 0)
                            {
                                obj.CurrentProgress += extraProgress;
                            }
                        }
                    }
                }
            }
        }

        private static void ApplyParameters(GoldDigger.FrankensteinBelt belt, float capMult, float spdMult)
        {
            if (belt == null) return;
            belt.MaxVolume = VanillaMaxVolume * capMult;
            belt.Speed = VanillaSpeed * spdMult;
            belt.OneLoadVolume = VanillaOneLoadVolume * spdMult;
            if (spdMult > 0.01f)
            {
                belt.SpawnInterval = VanillaSpawnInterval / spdMult;
            }
            belt.TextureOffsetSpeed = VanillaTextureOffsetSpeed * spdMult;
        }

        public static void RestoreVanilla()
        {
            foreach (var entry in Tracked.Values)
            {
                if (entry.Instance != null)
                {
                    ApplyParameters(entry.Instance, 1f, 1f);
                }
            }
            Tracked.Clear();
            _lastFrankCapMult = 1f;
            _lastFrankSpdMult = 1f;
            _lastCordCapMult = 1f;
            _lastCordSpdMult = 1f;
        }

        public static void Reset()
        {
            RestoreVanilla();
        }
    }
}
