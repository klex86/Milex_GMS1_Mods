using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Vehicles
{
    /// <summary>
    /// Scales excavator digging capacity and bucket collider size, calling Digging.Start() when modified.
    /// </summary>
    [HarmonyPatch]
    public static class ExcavatorPatch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("Koparka");
            return type != null ? AccessTools.Method(type, "Update") : null;
        }

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            if (__instance == null) return;

            Type koparkaType = __instance.GetType();
            FieldInfo controlledField = FieldCache.GetField(koparkaType, "Controlled");
            if (controlledField != null && !(bool)controlledField.GetValue(__instance)) return;

            FieldInfo diggingField = FieldCache.GetField(koparkaType, "Digging");
            if (diggingField == null) return;

            object diggingObj = diggingField.GetValue(__instance);
            if (diggingObj == null) return;

            Type diggingType = diggingObj.GetType();
            FieldInfo maxVolField = FieldCache.GetField(diggingType, "_maxShovelVolume");
            FieldInfo bladesBoxField = FieldCache.GetField(diggingType, "BladesBoxCollider");
            MethodInfo startMethod = FieldCache.GetMethod(diggingType, "Start");

            if (maxVolField == null) return;

            float curVol = (float)maxVolField.GetValue(diggingObj);
            float baseVol = OriginalValueStore.GetOrRegisterFloat(diggingObj, "Koparka_MaxVolume", curVol, (obj, val) =>
                maxVolField.SetValue(obj, val));

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.ExcavatorDigSpeedMultiplier
                : 1f;

            float targetVol = baseVol * multiplier;
            if (Math.Abs(curVol - targetVol) > 0.001f)
            {
                maxVolField.SetValue(diggingObj, targetVol);

                if (bladesBoxField != null)
                {
                    BoxCollider box = bladesBoxField.GetValue(diggingObj) as BoxCollider;
                    if (box != null)
                    {
                        Vector3 curSize = box.size;
                        Vector3 baseSize = OriginalValueStore.GetOrRegisterVector3(box, "Koparka_BladeBox", curSize, (b, s) =>
                            ((BoxCollider)b).size = s);
                        box.size = new Vector3(baseSize.x * multiplier, baseSize.y, baseSize.z);
                    }
                }

                if (startMethod != null)
                {
                    startMethod.Invoke(diggingObj, null);
                }
            }
        }
    }
}
