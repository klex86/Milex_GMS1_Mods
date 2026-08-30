using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Milex.GMS1.Core.Localization;

namespace Milex.GMS1.Core
{
    /// <summary>
    /// Abstract base class for all Milex GMS1 mods.
    /// Provides lifecycle management, auto-Harmony patching, unified logging,
    /// automatic localization support, and registration with ModRegistry &amp; Ingame Config Menu.
    ///
    /// Enable/Disable Lifecycle:
    ///   - IsEnabled: Persisted in [General] Enabled in the mod own .cfg file.
    ///   - SetEnabled(true):  Applies all Harmony patches and calls OnModEnabled().
    ///   - SetEnabled(false): Removes all Harmony patches and calls OnModDisabled().
    ///   - Sub-mods may override OnModEnabled/OnModDisabled for custom logic.
    ///   - CorePlugin overrides CanBeDisabled = false to protect the core.
    /// </summary>
    public abstract class ModBase : BaseUnityPlugin
    {
        public abstract string ModGuid { get; }
        public abstract string ModName { get; }
        public abstract string ModVersion { get; }

        /// <summary>
        /// Whether this mod can be disabled at runtime.
        /// Override and return false in CorePlugin to protect the core mod.
        /// </summary>
        public virtual bool CanBeDisabled => true;

        // ---- Config ----

        private ConfigFile _customConfig;

        /// <summary>
        /// Gets the config file named exactly after the mod assembly/DLL (e.g. Milex_GMS1_HelloMod.cfg).
        /// </summary>
        public new ConfigFile Config
        {
            get
            {
                if (_customConfig == null)
                {
                    string assemblyName = GetType().Assembly.GetName().Name;
                    string configPath = System.IO.Path.Combine(Paths.ConfigPath, $"{assemblyName}.cfg");
                    _customConfig = new ConfigFile(configPath, true, Info?.Metadata);
                }
                return _customConfig;
            }
        }

        // ---- Enable/Disable State ----

        private ConfigEntry<bool> _enabledEntry;

        /// <summary>
        /// Gets the current enabled state of this mod.
        /// Persisted in [General] Enabled in the mod own .cfg.
        /// </summary>
        public bool IsEnabled { get; private set; } = true;

        // ---- Harmony ----

        protected Harmony HarmonyInstance { get; private set; }
        protected ManualLogSource ModLogger => Logger;

        // ---- Unity Lifecycle ----

        protected virtual void Awake()
        {
            LogInfo($"Initializing {ModName} (v{ModVersion})...");

            // Bind enabled state to config (default: true) if mod can be disabled
            if (CanBeDisabled)
            {
                _enabledEntry = Config.Bind("General", "Enabled", true,
                    $"Determines whether {ModName} is active. Can be toggled in the Mod Menu at runtime.");
                IsEnabled = _enabledEntry.Value;
            }
            else
            {
                IsEnabled = true;
            }

            // Register with Core Mod Registry & Localization
            ModRegistry.Register(this);

            // Create Harmony instance (always; needed even when disabled for later enable)
            HarmonyInstance = new Harmony(ModGuid);

            if (IsEnabled)
            {
                try
                {
                    HarmonyInstance.PatchAll(GetType().Assembly);
                    LogInfo("Harmony patches applied successfully.");
                }
                catch (Exception ex)
                {
                    LogError($"Failed to apply Harmony patches: {ex}");
                }
            }
            else
            {
                // Disable the MonoBehaviour so Update()/LateUpdate() do not run
                this.enabled = false;
                LogInfo("Mod is disabled - Harmony patches and Update() skipped.");
            }
        }

        protected virtual void OnDestroy()
        {
            LogInfo($"Destroying {ModName}...");

            // Unregister from Core Mod Registry
            ModRegistry.Unregister(this);

            // Unpatch Harmony
            try
            {
                HarmonyInstance?.UnpatchSelf();
                LogInfo("Harmony patches removed.");
            }
            catch (Exception ex)
            {
                LogError($"Failed to unpatch Harmony instances: {ex}");
            }
        }

        // ---- Enable/Disable API ----

        /// <summary>
        /// Enables or disables this mod at runtime.
        /// Applies or removes Harmony patches and persists the new state.
        /// </summary>
        public void SetEnabled(bool enable)
        {
            if (!CanBeDisabled && !enable)
            {
                LogWarning($"{ModName} cannot be disabled - it is a core dependency.");
                return;
            }

            if (IsEnabled == enable) return;

            IsEnabled = enable;

            // Persist the new state
            if (_enabledEntry != null)
            {
                _enabledEntry.Value = enable;
                _enabledEntry.ConfigFile?.Save();
            }

            if (enable)
            {
                // Re-enable the MonoBehaviour so Update()/LateUpdate() run again
                this.enabled = true;

                try
                {
                    HarmonyInstance?.PatchAll(GetType().Assembly);
                    LogInfo("Mod enabled - Harmony patches applied.");
                }
                catch (Exception ex)
                {
                    LogError($"Failed to apply Harmony patches on enable: {ex}");
                }

                try { OnModEnabled(); } catch (Exception ex) { LogError($"OnModEnabled threw: {ex}"); }
            }
            else
            {
                try
                {
                    HarmonyInstance?.UnpatchSelf();
                    LogInfo("Mod disabled - Harmony patches removed.");
                }
                catch (Exception ex)
                {
                    LogError($"Failed to remove Harmony patches on disable: {ex}");
                }

                try { OnModDisabled(); } catch (Exception ex) { LogError($"OnModDisabled threw: {ex}"); }

                // Disable the MonoBehaviour so Update()/LateUpdate() stop running
                this.enabled = false;
            }
        }

        /// <summary>
        /// Called after this mod has been re-enabled and patches re-applied.
        /// Override in sub-mods for custom logic (e.g. reinitialise state).
        /// </summary>
        protected virtual void OnModEnabled() { }

        /// <summary>
        /// Called after this mod has been disabled and patches removed.
        /// Override in sub-mods for custom cleanup (e.g. reset game state).
        /// </summary>
        protected virtual void OnModDisabled() { }

        // ---- Localization ----

        /// <summary>
        /// Translates a key using this mod own localization tables.
        /// </summary>
        public string Translate(string key, string defaultValue = null)
        {
            string assemblyName = GetType().Assembly.GetName().Name;
            return LocalizationManager.Translate(assemblyName, key, defaultValue);
        }

        #region Logging Wrappers

        public void LogInfo(object message)
        {
            ModLogger.LogInfo($"[{ModName}] {message}");
        }

        public void LogWarning(object message)
        {
            ModLogger.LogWarning($"[{ModName}] {message}");
        }

        public void LogError(object message)
        {
            ModLogger.LogError($"[{ModName}] {message}");
        }

        #endregion
    }
}
