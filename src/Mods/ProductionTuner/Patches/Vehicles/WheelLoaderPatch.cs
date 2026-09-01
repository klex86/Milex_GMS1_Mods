using System;
using System.Reflection;
using HarmonyLib;
using Milex.GMS1.Mods.ProductionTuner.Helpers;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Vehicles
{
    /// <summary>
    /// Scales wheel loader (Ladowarka) bucket volume, reciprocal volume, hydraulic lifting torque, and steering agility.
    /// </summary>
    [HarmonyPatch]
    public static class WheelLoaderPatch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName("Ladowarka");
            return type != null ? AccessTools.Method(type, "Update") : null;
        }

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            if (__instance == null) return;

            Type ladowarkaType = __instance.GetType();
            FieldInfo diggingField = FieldCache.GetField(ladowarkaType, "Digging");
            if (diggingField == null) return;

            object diggingObj = diggingField.GetValue(__instance);
            if (diggingObj == null) return;

            Type diggingType = diggingObj.GetType();
            FieldInfo maxVolField = FieldCache.GetField(diggingType, "_maxShovelVolume");
            FieldInfo invMaxVolField = FieldCache.GetField(diggingType, "_invmaxShovelVolume");
            if (maxVolField == null) return;

            float curVol = (float)maxVolField.GetValue(diggingObj);
            float baseVol = OriginalValueStore.GetOrRegisterFloat(diggingObj, "Ladowarka_MaxVolume", curVol, (obj, val) =>
            {
                maxVolField.SetValue(obj, val);
                if (invMaxVolField != null && val > 0.0001f)
                {
                    invMaxVolField.SetValue(obj, 1f / val);
                }
            });

            float multiplier = ProductionTunerPlugin.Service != null
                ? ProductionTunerPlugin.Service.WheelLoaderLoadSpeedMultiplier
                : 1f;

            float targetVol = baseVol * multiplier;
            if (Math.Abs(curVol - targetVol) > 0.001f)
            {
                maxVolField.SetValue(diggingObj, targetVol);
                if (invMaxVolField != null && targetVol > 0.0001f)
                {
                    invMaxVolField.SetValue(diggingObj, 1f / targetVol);
                }

                // Adjust hydraulic joint torque on children so the loader can lift the heavier payload
                BoostHydraulics(__instance, multiplier);
            }
        }

        private static void BoostHydraulics(object ladowarkaObj, float multiplier)
        {
            if (!(ladowarkaObj is Component comp)) return;

            Type jointType = AccessTools.TypeByName("AnimatedJoint");
            if (jointType == null) return;

            FieldInfo maxTorqueField = FieldCache.GetField(jointType, "MaxTorque");
            if (maxTorqueField == null) return;

            Component[] joints = comp.GetComponentsInChildren(jointType);
            if (joints == null) return;

            foreach (var joint in joints)
            {
                float curTorque = (float)maxTorqueField.GetValue(joint);
                float baseTorque = OriginalValueStore.GetOrRegisterFloat(joint, "Joint_MaxTorque", curTorque, (j, t) =>
                    maxTorqueField.SetValue(j, t));
                maxTorqueField.SetValue(joint, baseTorque * Mathf.Max(1f, multiplier));
            }
        }
    }
}
