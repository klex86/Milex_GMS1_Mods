using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using Milex.GMS1.Core.Localization;

namespace Milex.GMS1.Core
{
    /// <summary>
    /// Metadata descriptor for a registered mod.
    /// </summary>
    public class ModInfo
    {
        public string Guid { get; }
        public string Name { get; }
        public string Version { get; }
        public string AssemblyName { get; }
        public ModBase Instance { get; }
        public Assembly Assembly => Instance?.GetType().Assembly;
        public ConfigFile Config => Instance?.Config;

        /// <summary>Whether the mod is currently enabled (live state from ModBase).</summary>
        public bool IsEnabled => Instance?.IsEnabled ?? false;

        /// <summary>Whether the mod supports being disabled at runtime.</summary>
        public bool CanBeDisabled => Instance?.CanBeDisabled ?? true;

        public ModInfo(ModBase instance)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            Guid = instance.ModGuid;
            Name = instance.ModName;
            Version = instance.ModVersion;
            AssemblyName = instance.GetType().Assembly.GetName().Name;
        }

        public string Translate(string key, string defaultValue = null)
        {
            return LocalizationManager.Translate(AssemblyName, key, defaultValue);
        }
    }

    /// <summary>
    /// Central registry managing all active ModBase instances and their configurations.
    /// </summary>
    public static class ModRegistry
    {
        private static readonly List<ModInfo> _mods = new List<ModInfo>();
        private static readonly object _lock = new object();

        public static event Action<ModInfo> OnModRegistered;
        public static event Action<ModInfo> OnModUnregistered;

        public static IReadOnlyList<ModInfo> RegisteredMods
        {
            get
            {
                lock (_lock)
                {
                    return _mods.ToArray();
                }
            }
        }

        public static void Register(ModBase mod)
        {
            if (mod == null) return;

            ModInfo info;
            lock (_lock)
            {
                if (_mods.Exists(m => m.Guid.Equals(mod.ModGuid, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                info = new ModInfo(mod);
                _mods.Add(info);
            }

            // Register mod with LocalizationManager using exact AssemblyName
            LocalizationManager.RegisterMod(info.AssemblyName, mod.GetType().Assembly);

            OnModRegistered?.Invoke(info);
        }

        public static void Unregister(ModBase mod)
        {
            if (mod == null) return;

            ModInfo found = null;
            lock (_lock)
            {
                found = _mods.Find(m => m.Instance == mod || m.Guid.Equals(mod.ModGuid, StringComparison.OrdinalIgnoreCase));
                if (found != null)
                {
                    _mods.Remove(found);
                }
            }

            if (found != null)
            {
                OnModUnregistered?.Invoke(found);
            }
        }
    }
}
