using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Core.Patches
{
    /// <summary>
    /// Harmony patches on the game's own central InputManager class (Assembly-CSharp::InputManager).
    /// Gold Mining Simulator routes ALL in-game actions (Camera rotation, Mouse Look, Mouse Wheel Tool switch,
    /// Player Movement, Vehicle Controls, Tool usage) through InputManager.GetAxis and InputManager.GetButton.
    /// </summary>
    [HarmonyPatch(typeof(InputManager))]
    public static class GameInputManagerPatches
    {
        // 1. Block Axis reads (Mouse X, Mouse Y, Mouse ScrollWheel, Horizontal, Vertical)
        [HarmonyPatch(nameof(InputManager.GetAxis))]
        [HarmonyPatch(nameof(InputManager.GetAxisRaw))]
        [HarmonyPrefix]
        public static bool Prefix_GetAxis(ref float __result)
        {
            if (CorePlugin.IsMenuOpen)
            {
                __result = 0.0f;
                return false; // Silence all game camera and mouse wheel inputs
            }
            return true;
        }

        // 2. Block Button and Boolean Axis reads
        [HarmonyPatch(nameof(InputManager.GetPositiveAxis))]
        [HarmonyPatch(nameof(InputManager.GetNegativeAxis))]
        [HarmonyPatch(nameof(InputManager.GetDoubleAxis))]
        [HarmonyPatch(nameof(InputManager.GetDoubleAxisDown))]
        [HarmonyPatch(nameof(InputManager.GetAxisNegativePositive))]
        [HarmonyPatch(nameof(InputManager.GetButton))]
        [HarmonyPatch(nameof(InputManager.GetButtonDown))]
        [HarmonyPatch(nameof(InputManager.GetButtonUp))]
        [HarmonyPatch(nameof(InputManager.GetNegativeButton))]
        [HarmonyPatch(nameof(InputManager.GetNegativeButtonDown))]
        [HarmonyPrefix]
        public static bool Prefix_GetButton(ref bool __result)
        {
            if (CorePlugin.IsMenuOpen)
            {
                __result = false;
                return false; // Silence all game button presses
            }
            return true;
        }
    }
}
