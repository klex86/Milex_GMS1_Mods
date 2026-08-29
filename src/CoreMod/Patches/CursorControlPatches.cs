using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Core.Patches
{
    /// <summary>
    /// Harmony patches on UnityEngine.Cursor identical to UniverseLib / UnityExplorer's CursorUnlocker.
    /// Intercepts any script trying to lock or hide the cursor while the menu is open,
    /// and remembers the game's requested state to restore seamlessly upon closing.
    /// </summary>
    [HarmonyPatch(typeof(Cursor))]
    public static class CursorControlPatches
    {
        public static CursorLockMode GameLockState { get; set; } = CursorLockMode.None;
        public static bool GameCursorVisible { get; set; } = true;

        [HarmonyPatch(nameof(Cursor.lockState), MethodType.Setter)]
        [HarmonyPrefix]
        public static bool Prefix_set_lockState(ref CursorLockMode value)
        {
            if (CorePlugin.IsMenuOpen)
            {
                // Remember what the game wanted to set
                GameLockState = value;
                // Force None so mouse is free
                value = CursorLockMode.None;
            }
            return true;
        }

        [HarmonyPatch(nameof(Cursor.visible), MethodType.Setter)]
        [HarmonyPrefix]
        public static bool Prefix_set_visible(ref bool value)
        {
            if (CorePlugin.IsMenuOpen)
            {
                GameCursorVisible = value;
                // Force visible
                value = true;
            }
            return true;
        }
        [HarmonyPatch(nameof(Cursor.lockState), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool Prefix_get_lockState(ref CursorLockMode __result)
        {
            if (CorePlugin.IsMenuOpen)
            {
                __result = CursorLockMode.None; // Tell the game the cursor is unlocked
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Cursor.visible), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool Prefix_get_visible(ref bool __result)
        {
            if (CorePlugin.IsMenuOpen)
            {
                __result = true; // Tell the game the cursor is visible
                return false;
            }
            return true;
        }
    }
}
