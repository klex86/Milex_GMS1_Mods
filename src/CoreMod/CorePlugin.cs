using System;
using BepInEx;
using BepInEx.Configuration;
using Milex.GMS1.Core.Localization;
using Milex.GMS1.Core.UI;
using Milex.GMS1.Core.UI.Modern;
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
        public const string PluginVersion = "1.3.0";

        public override string ModGuid => PluginGuid;
        public override string ModName => PluginName;
        public override string ModVersion => PluginVersion;

        /// <summary>The core mod can never be disabled at runtime.</summary>
        public override bool CanBeDisabled => false;

        public static CorePlugin Instance { get; private set; }

        public static ConfigEntry<KeyCode> MenuToggleKey { get; private set; }
        public static ConfigEntry<bool> PauseGameOnMenu { get; private set; }
        public static ConfigEntry<bool> IgnoreExternalTranslations { get; private set; }
        public static ConfigEntry<bool> UseGameLanguage { get; private set; }
        public static ConfigEntry<string> SelectedLanguage { get; private set; }
        public static ConfigEntry<float> UIScale { get; private set; }
        public static ConfigEntry<MenuEngineType> MenuEngine { get; private set; }

        public static bool IsMenuOpen { get; private set; } = false;

        private GameObject _uiHost;
        private ModMenuUI _classicMenu;
        private ModernCanvasMenu _modernMenu;

        protected override void Awake()
        {
            Instance = this;

            // Bind Core Settings (writes to Milex_GMS1_CoreMod.cfg)
            MenuToggleKey = Config.Bind("General", "MenuToggleKey", KeyCode.Insert, "Key to open and close the mod menu.");
            PauseGameOnMenu = Config.Bind("General", "PauseGameOnMenu", false, "Pauses the game world (TimeScale = 0) while the mod menu is open.");
            IgnoreExternalTranslations = Config.Bind("General", "IgnoreExternalTranslations", false, "Ignores external localization files on disk and loads directly from embedded DLL resources.");
            UseGameLanguage = Config.Bind("Localization", "UseGameLanguage", true, "Determines whether the game language is detected and used automatically.");
            SelectedLanguage = Config.Bind("Localization", "SelectedLanguage", "en", "Manually selected language code (only active when UseGameLanguage is false).");
            UIScale = Config.Bind("UI", "UIScale", 1.0f, "Scale factor of the mod menu interface (0.75 to 1.5 for High-DPI / 4K displays).");
            MenuEngine = Config.Bind("UI", "MenuEngine", MenuEngineType.Modern, "Interface engine style: Modern (uGUI Canvas Dashboard) or Classic (IMGUI).");

            IgnoreExternalTranslations.SettingChanged += (s, e) => LocalizationManager.ReloadAll();

            PauseGameOnMenu.SettingChanged += (s, e) =>
            {
                if (IsMenuOpen)
                {
                    if (PauseGameOnMenu.Value && !_isGamePausedByMenu)
                    {
                        _previousTimeScale = Time.timeScale > 0.001f ? Time.timeScale : 1.0f;
                        Time.timeScale = 0.0f;
                        _isGamePausedByMenu = true;
                    }
                    else if (!PauseGameOnMenu.Value && _isGamePausedByMenu)
                    {
                        Time.timeScale = _previousTimeScale > 0.001f ? _previousTimeScale : 1.0f;
                        _isGamePausedByMenu = false;
                    }
                }
            };

            base.Awake();

            // Attach Menu Components to persistent UI Host
            _uiHost = new GameObject("Milex_GMS1_Core_UIHost");
            DontDestroyOnLoad(_uiHost);
            _classicMenu = _uiHost.AddComponent<ModMenuUI>();
            _modernMenu = _uiHost.AddComponent<ModernCanvasMenu>();

            _classicMenu.Initialize();
            _modernMenu.Initialize();

            LogInfo($"Ready. Press {MenuToggleKey.Value} to open Mod Menu (Engine: {MenuEngine.Value}).");
        }

        private static CursorLockMode _previousLockMode = CursorLockMode.None;
        private static bool _previousCursorVisible = true;
        private static float _previousTimeScale = 1.0f;
        private static bool _isGamePausedByMenu = false;

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

        public static void SwitchMenuEngine(MenuEngineType newEngine)
        {
            if (MenuEngine == null) return;
            MenuEngine.Value = newEngine;
            MenuEngine.ConfigFile?.Save();

            if (IsMenuOpen)
            {
                if (newEngine == MenuEngineType.Modern)
                {
                    Instance?._classicMenu?.Hide();
                    Instance?._modernMenu?.Show();
                }
                else
                {
                    Instance?._modernMenu?.Hide();
                    Instance?._classicMenu?.Show();
                }
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
                    _previousTimeScale = Time.timeScale > 0.001f ? Time.timeScale : 1.0f;
                    Time.timeScale = 0.0f;
                    _isGamePausedByMenu = true;
                }

                // Show active UI renderer
                if (MenuEngine != null && MenuEngine.Value == MenuEngineType.Modern)
                {
                    Instance?._modernMenu?.Show();
                    Instance?._classicMenu?.Hide();
                }
                else
                {
                    Instance?._classicMenu?.Show();
                    Instance?._modernMenu?.Hide();
                }
            }
            else
            {
                if (_isGamePausedByMenu)
                {
                    Time.timeScale = _previousTimeScale > 0.001f ? _previousTimeScale : 1.0f;
                    _isGamePausedByMenu = false;
                }

                // Hide UI renderers
                Instance?._classicMenu?.Hide();
                Instance?._modernMenu?.Hide();

                // Restore game cursor state
                Cursor.lockState = _previousLockMode;
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

    public enum MenuEngineType
    {
        Modern,
        Classic
    }
}
