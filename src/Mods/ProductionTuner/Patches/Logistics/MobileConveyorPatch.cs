using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Scales buffer capacity (MaxVolume) and transport throughput speed for the large mobile conveyors
    /// (Frankenstein excavator belt and Cordylus robot carrier belt).
    /// Dynamically scales chunk discharge size (OneLoadVolume), spawn timer, and compensates
    /// for the secondary conveyor belt section (MyPathAfterDrop), seamlessly multiplying the vanilla 3-stage speeds.
    /// </summary>
    [HarmonyPatch(typeof(GoldDigger.FrankensteinBelt), "Update")]
    public static class MobileConveyorPatch
    {
        private struct ConveyorBase
        {
            public float BaseVolume;
            public float BaseSpeed;
            public float BaseOneLoad;
            public float BaseTextureOffsetSpeed;
            public Vector2 BaseSpawnInterval;
            public bool IsCordylus;
        }

        private static readonly Dictionary<int, ConveyorBase> BaseValues = new Dictionary<int, ConveyorBase>();
        private static readonly AccessTools.FieldRef<GoldDigger.FrankensteinBelt, float> LastSpawnRef =
            AccessTools.FieldRefAccess<GoldDigger.FrankensteinBelt, float>("lastSpawn");

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

            if (BaseValues.TryGetValue(id, out var data))
            {
                float curSpdMult = data.IsCordylus ? cordSpdMult : frankSpdMult;
                float curCapMult = data.IsCordylus ? cordCapMult : frankCapMult;
                float lastSpdMult = data.IsCordylus ? _lastCordSpdMult : _lastFrankSpdMult;
                float lastCapMult = data.IsCordylus ? _lastCordCapMult : _lastFrankCapMult;

                if (curSpdMult != lastSpdMult || curCapMult != lastCapMult)
                {
                    ApplyStaticParameters(__instance, data, curCapMult, curSpdMult);
                }

                // Runtime compensation: accelerate discharge spawn timer and secondary drop belt proportionally
                if (curSpdMult > 1f && __instance.IsEnabled)
                {
                    // Accelerate dirt chunk spawning from hopper onto belt
                    if (LastSpawnRef != null)
                    {
                        ref float lastSpawn = ref LastSpawnRef(__instance);
                        lastSpawn -= Time.deltaTime * __instance.SpeedMultiplier * (curSpdMult - 1f);
                    }

                    // Accelerate secondary drop belt section
                    if (__instance.CurrentObjects != null)
                    {
                        float extraProgress = Time.deltaTime * __instance.SpeedMultiplier * (curSpdMult - 1f);
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
                return;
            }

            // First-time registration
            bool isCord = __instance.GetComponentInParent<GoldDigger.MaximusMachineController>() != null;

            data = new ConveyorBase
            {
                BaseVolume = __instance.MaxVolume,
                BaseSpeed = __instance.Speed,
                BaseOneLoad = __instance.OneLoadVolume > 0f ? __instance.OneLoadVolume : 0.05f,
                BaseTextureOffsetSpeed = __instance.TextureOffsetSpeed > 0f ? __instance.TextureOffsetSpeed : 0.05f,
                BaseSpawnInterval = __instance.SpawnInterval != Vector2.zero ? __instance.SpawnInterval : new Vector2(0.2f, 0.4f),
                IsCordylus = isCord
            };
            BaseValues[id] = data;

            float activeCapMult = isCord ? cordCapMult : frankCapMult;
            float activeSpdMult = isCord ? cordSpdMult : frankSpdMult;

            ApplyStaticParameters(__instance, data, activeCapMult, activeSpdMult);

            _lastFrankCapMult = frankCapMult;
            _lastFrankSpdMult = frankSpdMult;
            _lastCordCapMult = cordCapMult;
            _lastCordSpdMult = cordSpdMult;
        }

        private static void ApplyStaticParameters(
            GoldDigger.FrankensteinBelt belt,
            ConveyorBase data,
            float capMult,
            float spdMult)
        {
            belt.MaxVolume = data.BaseVolume * capMult;
            belt.Speed = data.BaseSpeed * spdMult;

            // Scale discharge lump size and interval so the buffer empties proportionally
            belt.OneLoadVolume = data.BaseOneLoad * spdMult;
            if (spdMult > 0.01f)
            {
                belt.SpawnInterval = data.BaseSpawnInterval / spdMult;
            }
            belt.TextureOffsetSpeed = data.BaseTextureOffsetSpeed * spdMult;
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
