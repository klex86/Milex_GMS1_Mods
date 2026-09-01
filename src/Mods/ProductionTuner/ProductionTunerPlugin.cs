using BepInEx;
using Milex.GMS1.Core;
using Milex.GMS1.Mods.ProductionTuner.Config;
using Milex.GMS1.Mods.ProductionTuner.Helpers;
using Milex.GMS1.Mods.ProductionTuner.Patches.Logistics;
using Milex.GMS1.Mods.ProductionTuner.Patches.Processing;
using Milex.GMS1.Mods.ProductionTuner.Patches.Tools;
using Milex.GMS1.Mods.ProductionTuner.Patches.Vehicles;
using Milex.GMS1.Mods.ProductionTuner.Patches.WashPlants;
using Milex.GMS1.Mods.ProductionTuner.Services;

namespace Milex.GMS1.Mods.ProductionTuner
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CorePlugin.PluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public class ProductionTunerPlugin : ModBase
    {
        public const string PluginGuid = "com.milex.gms1.productiontuner";
        public const string PluginName = "Milex GMS1 Production Tuner";
        public const string PluginVersion = "1.3.0";

        public override string ModGuid => PluginGuid;
        public override string ModName => PluginName;
        public override string ModVersion => PluginVersion;

        public static ProductionTunerPlugin Instance { get; private set; }
        public static TuningConfig TuningConfig { get; private set; }
        public static TuningService Service { get; private set; }

        protected override void Awake()
        {
            Instance = this;

            // 1. Initialize configuration definitions and bindings
            TuningConfig = new TuningConfig(Config);

            // 2. Initialize domain logic service
            Service = new TuningService(TuningConfig);

            // Base initialization (Harmony, ModRegistry, localization)
            base.Awake();

            // Clear baseline tracking caches on scene transitions (e.g. loading save games)
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

            LogInfo(Translate("log.ready", "Production Tuner loaded. Open the Mod Menu to adjust multipliers."));
        }

        private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            ResetAllPatchCaches();
        }

        /// <summary>
        /// Called when the mod is re-enabled via the Mod Menu during runtime.
        /// Patches will re-evaluate and apply multipliers to the cached pristine vanilla baselines.
        /// </summary>
        protected override void OnModEnabled()
        {
            LogInfo(Translate("log.enabled", "Production Tuner enabled."));
        }

        /// <summary>
        /// Called when the mod is disabled via the Mod Menu during runtime.
        /// All Harmony patches are removed automatically and all tracked instances
        /// are restored back to their exact original vanilla values without losing their baseline memory.
        /// </summary>
        protected override void OnModDisabled()
        {
            RestoreAllVanillaValues();
            LogInfo(Translate("log.disabled", "Production Tuner disabled. Original game values restored."));
        }

        public static void RestoreAllVanillaValues()
        {
            MinersMossPatch.RestoreVanilla();
            SluiceBoxPatch.RestoreVanilla();
            WashPlantShakerPatch.RestoreVanilla();
            HogPanDirtBoxPatch.RestoreVanilla();
            MobileWashPlantPatch.RestoreVanilla();
            BucketPatch.RestoreVanilla();
            ShovelPatch.RestoreVanilla();
            WheelLoaderPatch.RestoreVanilla();
            DumpTruckPatch.RestoreVanilla();
            ExcavatorPatch.RestoreVanilla();
            BackhoeLoaderPatch.RestoreVanilla();
            MatScrubberPatch.RestoreVanilla();
            MagnetiteSeparatorPatch.RestoreVanilla();
            WaveTablePatch.RestoreVanilla();
            ConveyorGroundPatch.RestoreVanilla();
            ConveyorElevatorPatch.RestoreVanilla();
            MobileConveyorPatch.RestoreVanilla();
            MagnetiteTrailerPatch.RestoreVanilla();
            FuelTrailerPatch.RestoreVanilla();
        }

        public static void ResetAllPatchCaches()
        {
            RestoreAllVanillaValues();
            MinersMossPatch.Reset();
            SluiceBoxPatch.Reset();
            WashPlantShakerPatch.Reset();
            HogPanDirtBoxPatch.Reset();
            MobileWashPlantPatch.Reset();
            BucketPatch.Reset();
            ShovelPatch.Reset();
            WheelLoaderPatch.Reset();
            DumpTruckPatch.Reset();
            ExcavatorPatch.Reset();
            BackhoeLoaderPatch.Reset();
            MatScrubberPatch.Reset();
            MagnetiteSeparatorPatch.Reset();
            WaveTablePatch.Reset();
            ConveyorGroundPatch.Reset();
            ConveyorElevatorPatch.Reset();
            MobileConveyorPatch.Reset();
            MagnetiteTrailerPatch.Reset();
            FuelTrailerPatch.Reset();
            OrangeBeastFilter.Clear();
        }

        protected override void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            base.OnDestroy();
        }
    }
}
