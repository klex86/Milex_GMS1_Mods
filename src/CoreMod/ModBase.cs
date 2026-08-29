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
    /// automatic localization support, and registration with ModRegistry & Ingame Config Menu.
    /// </summary>
    public abstract class ModBase : BaseUnityPlugin
    {
        public abstract string ModGuid { get; }
        public abstract string ModName { get; }
        public abstract string ModVersion { get; }

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

        protected Harmony HarmonyInstance { get; private set; }
        protected ManualLogSource ModLogger => Logger;

        protected virtual void Awake()
        {
            LogInfo($"Initializing {ModName} (v{ModVersion})...");

            // Register with Core Mod Registry & Localization
            ModRegistry.Register(this);

            // Apply Harmony patches
            try
            {
                HarmonyInstance = new Harmony(ModGuid);
                HarmonyInstance.PatchAll(GetType().Assembly);
                LogInfo("Harmony patches applied successfully.");
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply Harmony patches: {ex}");
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

        /// <summary>
        /// Translates a key using this mod's localization tables.
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
