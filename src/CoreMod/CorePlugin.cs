using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using Milex.GMS1.Core.Localization;
using Milex.GMS1.Core.UI;
using Milex.GMS1.Core.UI.Modern;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        public const string PluginVersion = "1.4.0";

        public override string ModGuid => PluginGuid;
        public override string ModName => PluginName;
        public override string ModVersion => PluginVersion;

        /// <summary>The core mod can never be disabled at runtime.</summary>
        public override bool CanBeDisabled => false;

        public static CorePlugin Instance { get; private set; }

        // Core Configuration Entries
        public static ConfigEntry<KeyCode> MenuToggleKey { get; private set; }
        public static ConfigEntry<bool> PauseGameOnMenu { get; private set; }
        public static ConfigEntry<bool> IgnoreExternalTranslations { get; private set; }
        public static ConfigEntry<bool> UseGameLanguage { get; private set; }
        public static ConfigEntry<string> SelectedLanguage { get; private set; }
        public static ConfigEntry<float> UIScale { get; private set; }
        public static ConfigEntry<MenuEngineType> MenuEngine { get; private set; }

        // Performance & Memory Cleaner Entries
        public static ConfigEntry<bool> EnableMemoryCleaner { get; private set; }
        public static ConfigEntry<float> MemoryCleanCooldownSeconds { get; private set; }
        public static ConfigEntry<bool> CleanOnFastTravel { get; private set; }
        public static ConfigEntry<bool> CleanOnSave { get; private set; }
        public static ConfigEntry<bool> CleanOnLaptop { get; private set; }
        public static ConfigEntry<bool> CleanOnMenuOpen { get; private set; }

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

            // Bind Performance Settings
            EnableMemoryCleaner = Config.Bind("Performance", "EnableMemoryCleaner", true, "Periodically purges unused assets (Resources.UnloadUnusedAssets) and triggers GC during player-imperceptible moments to fight FPS loss.");
            MemoryCleanCooldownSeconds = Config.Bind("Performance", "MemoryCleanCooldownSeconds", 60.0f, "Minimum cooldown in seconds between automatic background memory cleans to prevent stutter.");
            CleanOnFastTravel = Config.Bind("Performance", "CleanOnFastTravel", true, "Automatically purge unused assets and collect memory during Fast Travel loading screens.");
            CleanOnSave = Config.Bind("Performance", "CleanOnSave", true, "Automatically purge unused assets and collect memory when saving (Quicksave / Autosave).");
            CleanOnLaptop = Config.Bind("Performance", "CleanOnLaptop", true, "Automatically purge unused assets and collect memory when opening the laptop / tablet computer.");
            CleanOnMenuOpen = Config.Bind("Performance", "CleanOnMenuOpen", true, "Automatically purge unused assets and collect memory when opening the Mod Menu.");

            IgnoreExternalTranslations.SettingChanged += (s, e) => LocalizationManager.ReloadAll();

            PauseGameOnMenu.SettingChanged += (s, e) =>
            {
                if (IsMenuOpen)
                {
                    if (PauseGameOnMenu.Value && !_isGamePausedByMenu)
                    {
                        ApplyGamePause(true);
                    }
                    else if (!PauseGameOnMenu.Value && _isGamePausedByMenu)
                    {
                        ApplyGamePause(false);
                    }
                }
            };

            SceneManager.sceneLoaded += OnSceneLoaded;

            base.Awake();

            // Initialize optimization service
            Services.MemoryCleanerService.Initialize();

            // Attach Menu Components to persistent UI Host
            _uiHost = new GameObject("Milex_GMS1_Core_UIHost", typeof(RectTransform));
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

        private static readonly HashSet<string> _cursorRequesters = new HashSet<string>();

        public static bool IsCursorUnlocked => IsMenuOpen || _cursorRequesters.Count > 0;

        public static void RequestCursorUnlock(string requesterId)
        {
            if (string.IsNullOrEmpty(requesterId)) return;
            bool wasActive = IsCursorUnlocked;
            if (!wasActive)
            {
                OnCursorFreed();
            }
            _cursorRequesters.Add(requesterId);
        }

        public static void ReleaseCursorUnlock(string requesterId)
        {
            if (string.IsNullOrEmpty(requesterId)) return;
            bool wasActive = IsCursorUnlocked;
            _cursorRequesters.Remove(requesterId);
            if (wasActive && !IsCursorUnlocked)
            {
                OnCursorRestored();
            }
        }

        private static void OnCursorFreed()
        {
            Patches.CursorControlPatches.SuppressGetterPatch = true;
            try
            {
                _previousLockMode = Cursor.lockState;
                _previousCursorVisible = Cursor.visible;
                Patches.CursorControlPatches.GameLockState = _previousLockMode;
                Patches.CursorControlPatches.GameCursorVisible = _previousCursorVisible;
            }
            finally
            {
                Patches.CursorControlPatches.SuppressGetterPatch = false;
            }

            SetNativeInputBlocked(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private static void OnCursorRestored()
        {
            SetNativeInputBlocked(false);

            CursorLockMode targetLock = Patches.CursorControlPatches.GameLockState != CursorLockMode.None
                ? Patches.CursorControlPatches.GameLockState
                : _previousLockMode;
            bool targetVisible = Patches.CursorControlPatches.GameLockState != CursorLockMode.None 
                ? Patches.CursorControlPatches.GameCursorVisible 
                : _previousCursorVisible;

            Cursor.lockState = targetLock;
            Cursor.visible = targetVisible;

            try
            {
                if (CursorManager.Instance != null)
                {
                    CursorManager.Instance.Refresh();
                }
            }
            catch { }

            Patches.CursorControlPatches.GameLockState = CursorLockMode.None;
            Patches.CursorControlPatches.GameCursorVisible = true;
        }

        private static void SetNativeInputBlocked(bool blocked)
        {
            try
            {
                InputManager.SetPauseMenuBlocked(blocked, "MilexModMenu");
            }
            catch
            {
                try
                {
                    var inputManagerType = Type.GetType("InputManager, Assembly-CSharp");
                    if (inputManagerType != null)
                    {
                        var setPauseMethod = inputManagerType.GetMethod("SetPauseMenuBlocked", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                        if (setPauseMethod != null)
                        {
                            var parameters = setPauseMethod.GetParameters();
                            if (parameters.Length == 2)
                            {
                                setPauseMethod.Invoke(null, new object[] { blocked, "MilexModMenu" });
                            }
                            else if (parameters.Length == 1)
                            {
                                setPauseMethod.Invoke(null, new object[] { blocked });
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private static float _sceneLoadTimestamp = 0f;
        private static float _nextPauseDiagTimestamp = 0f;

        private void Update()
        {
            if (Input.GetKeyDown(MenuToggleKey.Value))
            {
                ToggleMenu();
            }

            // Continuously ensure cursor is freed while menu or external overlay is active
            if (IsCursorUnlocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            UpdatePauseDiagnosticsAndRecovery();
        }

        private void LateUpdate()
        {
            // Some games force cursor lock in LateUpdate; enforce unlock
            if (IsCursorUnlocked)
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

        private void UpdatePauseDiagnosticsAndRecovery()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName) || sceneName.ToLower().Contains("menu"))
            {
                return;
            }

            if (Time.realtimeSinceStartup < _nextPauseDiagTimestamp)
            {
                return;
            }
            _nextPauseDiagTimestamp = Time.realtimeSinceStartup + 3.0f;

            try
            {
                if (PauseManager.Instance != null)
                {
                    var field = typeof(PauseManager).GetField("PauseReasons", BindingFlags.NonPublic | BindingFlags.Instance);
                    var reasons = field?.GetValue(PauseManager.Instance) as List<string>;

                    bool isTimePaused = Time.timeScale < 0.001f;
                    int count = reasons != null ? reasons.Count : 0;

                    if (isTimePaused || count > 0)
                    {
                        string reasonList = reasons != null ? string.Join(", ", reasons.ToArray()) : "none";
                        bool allInputBlocked = Singleton<InputManager>.IsInstanced() && Singleton<InputManager>.Instance.AllInputBlocked;
                        LogWarning($"[PauseDiagnostics] Scene: '{sceneName}', TimeScale: {Time.timeScale:F2}, Active PauseReasons ({count}): [{reasonList}], AllInputBlocked: {allInputBlocked}");

                        // Automatic recovery: Check if loading has finished and the player is stranded in a leaked pause
                        // FastTravel ForceLoad waits up to 7.5 seconds in realtime with scene name pause reason.
                        // We must wait at least 15.0 seconds after scene loading and verify MenuLoading is not active to avoid interrupting AreaStreamer and FastTravel.
                        float elapsedSinceLoad = Time.realtimeSinceStartup - _sceneLoadTimestamp;
                        bool isLoading = Singleton<GoldDigger.LevelLoadingManager>.IsInstanced() && Singleton<GoldDigger.LevelLoadingManager>.Instance.IsLoading();
                        bool isVanillaMenuOpen = Singleton<GoldDigger.MenuManager>.IsInstanced() && Singleton<GoldDigger.MenuManager>.Instance.InGameMenu;
                        bool isMenuLoading = Singleton<GoldDigger.MenuManager>.IsInstanced() &&
                                             Singleton<GoldDigger.MenuManager>.Instance.MyMenuLoading != null &&
                                             (Singleton<GoldDigger.MenuManager>.Instance.MyMenuLoading.gameObject.activeInHierarchy ||
                                              Singleton<GoldDigger.MenuManager>.Instance.MyMenuLoading.isFastTravel);

                        if (elapsedSinceLoad > 15.0f && !isLoading && !isVanillaMenuOpen && !isMenuLoading && !IsMenuOpen)
                        {
                            // Check if any pause reason is an intentional interactive player UI
                            bool isLegitimateUIPause = false;
                            if (reasons != null && reasons.Count > 0)
                            {
                                foreach (var r in reasons)
                                {
                                    if (r == "ShopGUI" || r == "LaptopUse" || r == "MenuManager" || r == "FastTravel" || r == "FreeCam")
                                    {
                                        isLegitimateUIPause = true;
                                        break;
                                    }
                                }
                            }

                            if (!isLegitimateUIPause)
                            {
                                LogWarning($"[PauseDiagnostics] Leaked pause detected ({elapsedSinceLoad:F1}s after load). Auto-recovering game state...");
                                ForceResumeGame();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Pause recovery check error: {ex.Message}");
            }
        }

        public static void ForceResumeGame()
        {
            try
            {
                if (PauseManager.Instance != null)
                {
                    var field = typeof(PauseManager).GetField("PauseReasons", BindingFlags.NonPublic | BindingFlags.Instance);
                    var reasons = field?.GetValue(PauseManager.Instance) as List<string>;
                    if (reasons != null && reasons.Count > 0)
                    {
                        Instance?.LogInfo($"Clearing {reasons.Count} stuck PauseReasons: [{string.Join(", ", reasons.ToArray())}]");
                        reasons.Clear();
                    }
                    PauseManager.Instance.SetGamePaused(false, "MilexModMenu", -1f);
                }

                Time.timeScale = 1.0f;
                _isGamePausedByMenu = false;

                if (Singleton<InputManager>.IsInstanced())
                {
                    InputManager.SetAllInputBlocked(false);
                    InputManager.SetPauseMenuBlocked(false, "MilexModMenu");
                }

                if (Singleton<CursorManager>.IsInstanced())
                {
                    Singleton<CursorManager>.Instance.SetCursorMenu(false);
                    Singleton<CursorManager>.Instance.ShowCursor(false);
                    Singleton<CursorManager>.Instance.Refresh();
                }

                Instance?.LogInfo("ForceResumeGame completed: TimeScale = 1.0, AllInputBlocked = false, Cursor refreshed.");
            }
            catch (Exception ex)
            {
                Instance?.LogWarning($"ForceResumeGame failed: {ex.Message}");
            }
        }

        private static void ApplyGamePause(bool pause)
        {
            if (pause)
            {
                // Never pause the Main Menu
                string sceneName = SceneManager.GetActiveScene().name;
                if (!string.IsNullOrEmpty(sceneName) && sceneName.ToLower().Contains("menu"))
                {
                    return;
                }

                _previousTimeScale = Time.timeScale > 0.001f ? Time.timeScale : 1.0f;
                try
                {
                    if (PauseManager.Instance != null)
                    {
                        PauseManager.Instance.SetGamePaused(true, "MilexModMenu", -1f);
                    }
                    else
                    {
                        Time.timeScale = 0.0f;
                    }
                }
                catch
                {
                    Time.timeScale = 0.0f;
                }
                _isGamePausedByMenu = true;
            }
            else
            {
                try
                {
                    if (PauseManager.Instance != null)
                    {
                        PauseManager.Instance.SetGamePaused(false, "MilexModMenu", -1f);
                    }
                }
                catch { }

                if (_isGamePausedByMenu)
                {
                    Time.timeScale = _previousTimeScale > 0.001f ? _previousTimeScale : 1.0f;
                    _isGamePausedByMenu = false;
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _sceneLoadTimestamp = Time.realtimeSinceStartup;
            LogInfo($"Scene loaded: '{scene.name}' (mode: {mode}). Resetting menu state.");

            bool hadCursorUnlocked = IsCursorUnlocked;

            // Force close menu on scene transition
            if (IsMenuOpen)
            {
                IsMenuOpen = false;
                Instance?._classicMenu?.Hide();
                Instance?._modernMenu?.Hide();
            }

            // Only release game pause if the mod menu itself had paused it
            if (_isGamePausedByMenu)
            {
                ApplyGamePause(false);
            }

            _cursorRequesters.Clear();

            // Only restore cursor and unblock input if our mod had actually unlocked or blocked it
            if (hadCursorUnlocked)
            {
                Patches.CursorControlPatches.GameLockState = CursorLockMode.None;
                Patches.CursorControlPatches.GameCursorVisible = true;
                OnCursorRestored();
                SetNativeInputBlocked(false);
            }
        }

        public static void ToggleMenu()
        {
            bool wasUnlocked = IsCursorUnlocked;
            bool willOpen = !IsMenuOpen;

            if (willOpen)
            {
                if (!wasUnlocked)
                {
                    OnCursorFreed();
                }

                IsMenuOpen = true;

                if (PauseGameOnMenu != null && PauseGameOnMenu.Value)
                {
                    ApplyGamePause(true);
                }

                if (CleanOnMenuOpen != null && CleanOnMenuOpen.Value)
                {
                    Services.MemoryCleanerService.Instance?.Clean(force: false, triggerSource: "MenuOpen");
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
                IsMenuOpen = false;

                if (_isGamePausedByMenu)
                {
                    ApplyGamePause(false);
                }
                else if (PauseGameOnMenu != null && !PauseGameOnMenu.Value && Time.timeScale < 0.001f)
                {
                    // If the user configured PauseGameOnMenu = false, and the game is paused,
                    // closing the menu provides a recovery opportunity if loading has finished.
                    string sceneName = SceneManager.GetActiveScene().name;
                    if (!string.IsNullOrEmpty(sceneName) && !sceneName.ToLower().Contains("menu"))
                    {
                        bool isLoading = Singleton<GoldDigger.LevelLoadingManager>.IsInstanced() && Singleton<GoldDigger.LevelLoadingManager>.Instance.IsLoading();
                        bool isVanillaMenuOpen = Singleton<GoldDigger.MenuManager>.IsInstanced() && Singleton<GoldDigger.MenuManager>.Instance.InGameMenu;
                        if (!isLoading && !isVanillaMenuOpen)
                        {
                            Instance?.LogInfo("Menu closed with PauseGameOnMenu disabled while game was paused. Force-resuming gameplay...");
                            ForceResumeGame();
                        }
                    }
                }

                // Hide UI renderers
                Instance?._classicMenu?.Hide();
                Instance?._modernMenu?.Hide();

                if (wasUnlocked && !IsCursorUnlocked)
                {
                    OnCursorRestored();
                }
            }
        }

        protected override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

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
