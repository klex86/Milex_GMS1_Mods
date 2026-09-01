using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Tools
{
    /// <summary>
    /// Scales hand shovel volume and blade surface area so that digging fills the enlarged volume proportionally.
    /// </summary>
    [HarmonyPatch]
    public static class ShovelPatch
    {
        private static FieldInfo _bladeSizeXField;
        private static FieldInfo _bladeSizeZField;

        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("GoldDigger.Shovel");
            return type != null ? AccessTools.Method(type, "Awake") : null;
        }

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            Apply(__instance);
        }

        public static void Apply(object shovelObj)
        {
            if (shovelObj == null) return;

            Type type = shovelObj.GetType();
            FieldInfo maxVolField = FieldCache.GetField(type, "MaxVolume");
            if (maxVolField == null) return;

            if (_bladeSizeXField == null) _bladeSizeXField = FieldCache.GetField(type, "_bladeSizex");
            if (_bladeSizeZField == null) _bladeSizeZField = FieldCache.GetField(type, "_bladeSizez");

            float currentVol = (float)maxVolField.GetValue(shovelObj);
            float baseVol = 0f;
            baseVol = OriginalValueStore.GetOrRegisterFloat(shovelObj, "Shovel_Volume", currentVol, (obj, newVol) =>
            {
                maxVolField.SetValue(obj, newVol);
                if (baseVol > 0.0001f)
                {
                    UpdateBladeSizes(obj, newVol / baseVol);
                }
            });

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.ShovelFillSpeedMultiplier
                : 1f;

            float targetVol = baseVol * multiplier;
            maxVolField.SetValue(shovelObj, targetVol);
            UpdateBladeSizes(shovelObj, multiplier);
        }

        private static void UpdateBladeSizes(object shovelObj, float multiplier)
        {
            if (_bladeSizeXField == null || _bladeSizeZField == null) return;

            float sqrtMult = Mathf.Sqrt(Mathf.Max(0.01f, multiplier));

            float curX = (float)_bladeSizeXField.GetValue(shovelObj);
            float baseX = OriginalValueStore.GetOrRegisterFloat(shovelObj, "Shovel_BladeX", curX, (obj, val) =>
                _bladeSizeXField.SetValue(obj, val));
            _bladeSizeXField.SetValue(shovelObj, baseX * sqrtMult);

            float curZ = (float)_bladeSizeZField.GetValue(shovelObj);
            float baseZ = OriginalValueStore.GetOrRegisterFloat(shovelObj, "Shovel_BladeZ", curZ, (obj, val) =>
                _bladeSizeZField.SetValue(obj, val));
            _bladeSizeZField.SetValue(shovelObj, baseZ * sqrtMult);
        }
    }
}
