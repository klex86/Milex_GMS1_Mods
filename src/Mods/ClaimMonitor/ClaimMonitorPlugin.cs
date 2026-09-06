using BepInEx;
using Milex.GMS1.Core;
using Milex.GMS1.Mods.ClaimMonitor.Config;
using Milex.GMS1.Mods.ClaimMonitor.Diagnostics;
using Milex.GMS1.Mods.ClaimMonitor.UI;
using UnityEngine;

namespace Milex.GMS1.Mods.ClaimMonitor
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CorePlugin.PluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public class ClaimMonitorPlugin : ModBase
    {
        public const string PluginGuid = "com.milex.gms1.claimmonitor";
        public const string PluginName = "Milex Claim Monitor";
        public const string PluginVersion = "1.0.0";

        public override string ModGuid => PluginGuid;
        public override string ModName => PluginName;
        public override string ModVersion => PluginVersion;

        public static ClaimMonitorPlugin Instance { get; private set; }
        public MonitorConfig MonitorConfig { get; private set; }

        public ClaimScanner Scanner { get; private set; }
        public WarningHUD Hud { get; private set; }
        public DebugOverlay DebugOverlay { get; private set; }

        protected override void Awake()
        {
            Instance = this;

            // 1. Initialize BepInEx configuration
            MonitorConfig = new MonitorConfig(Config);

            // 2. Base initialization (ModRegistry, Harmony, localization)
            base.Awake();

            LogInfo("ClaimMonitorPlugin initializing components...");

            // 3. Attach Scanner, Warning HUD, and Debug Inspector
            Scanner = gameObject.AddComponent<ClaimScanner>();
            Scanner.Config = MonitorConfig;

            Hud = gameObject.AddComponent<WarningHUD>();
            Hud.Config = MonitorConfig;

            DebugOverlay = gameObject.AddComponent<DebugOverlay>();
            DebugOverlay.Config = MonitorConfig;

            LogInfo(Translate("log.ready", "Claim Monitor initialized and active."));
        }

        private void Start()
        {
            if (IsEnabled)
            {
                Scanner?.StartScanning();
            }
        }

        private void Update()
        {
            // F3 or F8 toggles the diagnostic debug inspector
            if (Input.GetKeyDown(KeyCode.F3) || Input.GetKeyDown(KeyCode.F8))
            {
                if (MonitorConfig != null)
                {
                    MonitorConfig.EnableDebugGroup.Value = !MonitorConfig.EnableDebugGroup.Value;
                    LogInfo($"Diagnostic Inspector toggle pressed: Debug is now {MonitorConfig.EnableDebugGroup.Value}");
                    if (MonitorConfig.EnableDebugGroup.Value)
                    {
                        Scanner?.ForceScan();
                    }
                }
            }
        }

        protected override void OnModEnabled()
        {
            Scanner?.StartScanning();
            if (Hud != null) Hud.enabled = true;
            LogInfo(Translate("log.enabled", "Claim Monitor enabled."));
        }

        protected override void OnModDisabled()
        {
            Scanner?.StopScanning();
            if (Hud != null) Hud.enabled = false;
            LogInfo(Translate("log.disabled", "Claim Monitor disabled."));
        }

        protected override void OnDestroy()
        {
            Scanner?.StopScanning();
            base.OnDestroy();
        }
    }
}