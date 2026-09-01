using BepInEx;
using Milex.GMS1.Core;
using Milex.GMS1.Mods.ProductionTuner.Config;
using Milex.GMS1.Mods.ProductionTuner.Helpers;
using Milex.GMS1.Mods.ProductionTuner.Services;

namespace Milex.GMS1.Mods.ProductionTuner
{
    /// <summary>
    /// Production Tuner – Provides sliders for processing speeds, capacities,
    /// and hydraulic rates for all components, vehicles, and tools in Gold Rush: The Game.
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CorePlugin.PluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public class ProductionTunerPlugin : ModBase
    {
        public const string PluginGuid    = "com.milex.gms1.productiontuner";
        public const string PluginName    = "Milex GMS1 Production Tuner";
        public const string PluginVersion = "1.0.0";

        public override string ModGuid    => PluginGuid;
        public override string ModName    => PluginName;
        public override string ModVersion => PluginVersion;

        /// <summary>
        /// Global instance – used by Harmony patch classes in Phase 2
        /// to query multipliers from TuningService.
        /// </summary>
        public static ProductionTunerPlugin Instance { get; private set; }

        /// <summary>Access to all configuration settings of the mod.</summary>
        public TuningConfig TuningConfig { get; private set; }

        /// <summary>Access to domain service (multiplier calculation).</summary>
        public static TuningService Service { get; private set; }

        protected override void Awake()
        {
            Instance = this;

            // Layer 1: Load / initialize configuration
            TuningConfig = new TuningConfig(Config);

            // Layer 2: Initialize domain service
            Service = new TuningService(TuningConfig);

            // Base initialization (Harmony, ModRegistry, localization)
            base.Awake();

            LogInfo(Translate("log.ready", "Production Tuner loaded. Open the Mod Menu to adjust multipliers."));
        }

        /// <summary>
        /// Called when the mod is re-enabled via the Mod Menu during runtime.
        /// </summary>
        protected override void OnModEnabled()
        {
            LogInfo(Translate("log.enabled", "Production Tuner enabled."));
        }

        /// <summary>
        /// Called when the mod is disabled via the Mod Menu during runtime.
        /// All Harmony patches are removed automatically and all tracked instances
        /// are restored back to their exact original vanilla values.
        /// </summary>
        protected override void OnModDisabled()
        {
            OriginalValueStore.RestoreAll();
            LogInfo(Translate("log.disabled", "Production Tuner disabled. Original game values restored."));
        }
    }
}
