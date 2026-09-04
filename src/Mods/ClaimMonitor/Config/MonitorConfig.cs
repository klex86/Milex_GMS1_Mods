using BepInEx.Configuration;

namespace Milex.GMS1.Mods.ClaimMonitor.Config
{
    public class MonitorConfig
    {
        // Setups & Monitoring
        public ConfigEntry<bool> MonitorSetup1 { get; private set; }

        public ConfigEntry<bool> MonitorSetup2 { get; private set; }
        public ConfigEntry<bool> Setup2IncludeFeedingChain { get; private set; }

        public ConfigEntry<bool> MonitorSetup3 { get; private set; }
        public ConfigEntry<bool> Setup3IncludeFeedingChain { get; private set; }

        // Thresholds
        public ConfigEntry<float> MatWarningThreshold { get; private set; }
        public ConfigEntry<float> VehicleLowFuelThreshold { get; private set; }

        // Warning HUD
        public ConfigEntry<bool> HudEnabled { get; private set; }
        public ConfigEntry<bool> HudOnlyShowWarnings { get; private set; }
        public ConfigEntry<bool> HudCompactMode { get; private set; }
        public ConfigEntry<float> HudPosX { get; private set; }
        public ConfigEntry<float> HudPosY { get; private set; }
        public ConfigEntry<float> HudMaxWidth { get; private set; }
        public ConfigEntry<float> HudMaxHeight { get; private set; }

        // Fuel Monitor
        public ConfigEntry<bool> ShowFuelInVehicleSwitcher { get; private set; }

        // Debug & Raw Inspection
        public ConfigEntry<bool> EnableDebugGroup { get; private set; }

        public MonitorConfig(ConfigFile config)
        {
            // Setups
            MonitorSetup1 = config.Bind(
                "Setups",
                "MonitorSetup1",
                true,
                "Monitor mobile wash plants in the Warning HUD."
            );

            MonitorSetup2 = config.Bind(
                "Setups",
                "MonitorSetup2",
                true,
                "Monitor stationary setup (T3-T5) in the Warning HUD."
            );

            Setup2IncludeFeedingChain = config.Bind(
                "Setups",
                "Setup2IncludeFeedingChain",
                false,
                "Setup T3-T5: Include feeding chain (hoppers and conveyors) in health evaluation."
            );

            MonitorSetup3 = config.Bind(
                "Setups",
                "MonitorSetup3",
                true,
                "Monitor Setup T6 (Orange Beast) in the Warning HUD."
            );

            Setup3IncludeFeedingChain = config.Bind(
                "Setups",
                "Setup3IncludeFeedingChain",
                false,
                "Setup T6: Include feeding chain (hoppers and conveyors) in health evaluation."
            );

            // Thresholds
            MatWarningThreshold = config.Bind(
                "Thresholds",
                "MatWarningThreshold",
                90.0f,
                new ConfigDescription("Mat fill percentage threshold to trigger a warning.", new AcceptableValueRange<float>(70.0f, 98.0f))
            );

            VehicleLowFuelThreshold = config.Bind(
                "Thresholds",
                "VehicleLowFuelThreshold",
                15.0f,
                new ConfigDescription("Vehicle fuel percentage threshold to trigger low fuel warning.", new AcceptableValueRange<float>(5.0f, 30.0f))
            );

            // Warning HUD
            HudEnabled = config.Bind(
                "WarningHUD",
                "HudEnabled",
                true,
                "Enable Warning HUD overlay on screen."
            );

            HudOnlyShowWarnings = config.Bind(
                "WarningHUD",
                "HudOnlyShowWarnings",
                false,
                "Only display the Warning HUD when there are active warnings or critical alerts."
            );

            HudCompactMode = config.Bind(
                "WarningHUD",
                "HudCompactMode",
                false,
                "Render HUD in ultra-compact mode with single-line status badge."
            );

            HudPosX = config.Bind(
                "WarningHUD",
                "HudPosX",
                20f,
                new ConfigDescription("Horizontal screen position of the Warning HUD.", new AcceptableValueRange<float>(0f, 3840f))
            );

            HudPosY = config.Bind(
                "WarningHUD",
                "HudPosY",
                100f,
                new ConfigDescription("Vertical screen position of the Warning HUD.", new AcceptableValueRange<float>(0f, 2160f))
            );

            HudMaxWidth = config.Bind(
                "WarningHUD",
                "HudMaxWidth",
                340f,
                new ConfigDescription("Maximum width of the Warning HUD container.", new AcceptableValueRange<float>(200f, 800f))
            );

            HudMaxHeight = config.Bind(
                "WarningHUD",
                "HudMaxHeight",
                420f,
                new ConfigDescription("Maximum height of the Warning HUD container.", new AcceptableValueRange<float>(100f, 1000f))
            );

            // Fuel
            ShowFuelInVehicleSwitcher = config.Bind(
                "Fuel",
                "ShowFuelInVehicleSwitcher",
                true,
                "Display fuel fill percentage directly inside the vehicle quick-switching bar."
            );

            // Debug
            EnableDebugGroup = config.Bind(
                "Debug",
                "EnableDebugGroup",
                false,
                "Show detailed component debug inspection and raw object diagnostics in the menu."
            );
        }
    }
}