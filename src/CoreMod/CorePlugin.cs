using System;
using BepInEx;
using BepInEx.Configuration;
using Milex.GMS1.Core.Localization;
using Milex.GMS1.Core.UI;
using UnityEngine;

namespace Milex.GMS1.Core
{
    /// <summary>
    /// Core Plugin providing the central mod registry, localization engine,
    /// and in-game configuration menu for Milex GMS1 Mods.
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class CorePlugin : ModBase
    {
        public const string PluginGuid = "com.milex.gms1.core";
        public const string PluginName = "Milex GMS1 CoreMod";
        public const string PluginVersion = "1.2.0";

        public override string ModGuid => PluginGuid;
        public override string ModName => PluginName;
        public override string ModVersion => PluginVersion;

        public static CorePlugin Instance { get; private set; }

        public static ConfigEntry<KeyCode> MenuToggleKey { get; private set; }
        public static ConfigEntry<bool> PauseGameOnMenu { get; private set; }
        public static ConfigEntry<bool> UseGameLanguage { get; private set; }
        public static ConfigEntry<string> SelectedLanguage { get; private set; }
        public static ConfigEntry<float> UIScale { get; private set; }

        public static bool IsMenuOpen { get; private set; } = false;

        private GameObject _uiHost;
        private ModMenuUI _modMenuUI;

        protected override void Awake()
        {
            Instance = this;

            // Bind Core Settings (writes to Milex_GMS1_CoreMod.cfg)
            MenuToggleKey = Config.Bind("General", "MenuToggleKey", KeyCode.Insert, "Taste zum Öffnen und Schließen des Mod-Menüs");
            PauseGameOnMenu = Config.Bind("General", "PauseGameOnMenu", false, "Pausiert die Spielwelt (TimeScale = 0), wenn das Mod-Menü offen ist");
            UseGameLanguage = Config.Bind("Localization", "UseGameLanguage", true, "Gibt an, ob die Spiel-Sprache automatisch verwendet wird");
            SelectedLanguage = Config.Bind("Localization", "SelectedLanguage", "en", "Manuell gewählte Sprache (nur aktiv, wenn UseGameLanguage false ist)");
            UIScale = Config.Bind("UI", "UIScale", 1.0f, "Skalierungsfaktor des Mod-Menüs (0.75 bis 1.5 für High-DPI/4K)");

            base.Awake();

            // Attach ModMenuUI component
            _uiHost = new GameObject("Milex_GMS1_Core_UIHost");
            DontDestroyOnLoad(_uiHost);
            _modMenuUI = _uiHost.AddComponent<ModMenuUI>();

            LogInfo($"Ready. Press {MenuToggleKey.Value} to open Mod Menu.");
        }

        private static CursorLockMode _previousLockMode = CursorLockMode.None;
        private static bool _previousCursorVisible = true;
        private static float _previousTimeScale = 1.0f;

        private void Update()
        {
            if (Input.GetKeyDown(MenuToggleKey.Value))
            {
                ToggleMenu();
            }

            // Continuously ensure cursor is freed while menu is open
            if (IsMenuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void LateUpdate()
        {
            // Some games force cursor lock in LateUpdate; enforce unlock
            if (IsMenuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public static void ToggleMenu()
        {
            IsMenuOpen = !IsMenuOpen;

            try
            {
                // Call the game's native input blocker!
                var inputManagerType = Type.GetType("InputManager, Assembly-CSharp");
                if (inputManagerType != null)
                {
                    var setPauseMethod = inputManagerType.GetMethod("SetPauseMenuBlocked", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    if (setPauseMethod != null)
                    {
                        setPauseMethod.Invoke(null, new object[] { IsMenuOpen });
                    }
                }
            }
            catch { }

            if (IsMenuOpen)
            {
                // Save current game cursor state
                _previousLockMode = Cursor.lockState;
                _previousCursorVisible = Cursor.visible;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (PauseGameOnMenu != null && PauseGameOnMenu.Value)
                {
                    _previousTimeScale = Time.timeScale;
                    Time.timeScale = 0.0f;
                }
            }
            else
            {
                if (PauseGameOnMenu != null && PauseGameOnMenu.Value)
                {
                    Time.timeScale = _previousTimeScale;
                }

                // Restore game cursor state
                Cursor.lockState = _previousLockMode != CursorLockMode.None ? _previousLockMode : CursorLockMode.Locked;
                Cursor.visible = _previousCursorVisible;
            }
        }

        protected override void OnDestroy()
        {
            if (_uiHost != null)
            {
                Destroy(_uiHost);
            }

            base.OnDestroy();
        }
    }
}
