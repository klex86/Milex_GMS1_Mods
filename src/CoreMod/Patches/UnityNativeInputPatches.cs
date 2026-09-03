using System;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Core.Patches
{
    /// <summary>
    /// Patches Unity's native Input class directly.
    /// This catches any legacy game scripts that bypass InputManager and Rewired,
    /// directly calling Input.GetAxis("Mouse ScrollWheel") or Input.GetButton.
    /// </summary>
    [HarmonyPatch(typeof(Input))]
    public static class UnityNativeInputPatches
    {
        [HarmonyPatch(nameof(Input.GetAxis))]
        [HarmonyPatch(nameof(Input.GetAxisRaw))]
        [HarmonyPrefix]
        public static bool Prefix_GetAxis(ref float __result)
        {
            if (CorePlugin.IsMenuOpen)
            {
                __result = 0f;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Input.GetButton))]
        [HarmonyPatch(nameof(Input.GetButtonDown))]
        [HarmonyPatch(nameof(Input.GetButtonUp))]
        [HarmonyPrefix]
        public static bool Prefix_GetButton(ref bool __result)
        {
            if (CorePlugin.IsMenuOpen)
            {
                __result = false;
                return false;
            }
            return true;
        }

    }
}
