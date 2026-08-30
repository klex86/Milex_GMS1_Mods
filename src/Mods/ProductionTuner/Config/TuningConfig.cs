using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Config
{
    /// <summary>
    /// Layer 1: Manages all configuration settings for the Production Tuner mod.
    ///
    /// Each setting is stored as a multiplier against base game values in BepInEx/config/Milex_GMS1_ProductionTuner.cfg.
    /// Each value has its own specific default multiplier and is configured individually.
    ///
    /// All config keys and descriptions are in English. Display labels come from localization files.
    /// Reactive cascade protection synchronizes dependent stations live in the GUI.
    /// When bucket capacity is increased, dependent equipment scales automatically,
    /// and bucket capacity is clamped to the maximum allowed input capacity of dependent equipment.
    /// </summary>
    public class TuningConfig
    {
        // Allowed multiplier steps in increments of 0.5
        private static readonly float[] StandardSteps = GenerateSteps(0.5f, 10.0f, 0.5f);
        private static readonly float[] DependentCapacitySteps = GenerateSteps(0.5f, 20.0f, 0.5f);

        private static float[] GenerateSteps(float min, float max, float step)
        {
            var list = new List<float>();
            for (float v = min; v <= max + 0.01f; v += step)
                list.Add((float)System.Math.Round(v, 1));
            return list.ToArray();
        }

        private readonly ConfigFile _cfg;
        private bool _isSyncing = false;

        // ===========================================================
        // GENERAL SETTINGS
        // ===========================================================

        /// <summary>
        /// When active, dependent equipment (hog pan, magnetite separator, wave table, trailers)
        /// automatically scales to at least the bucket multiplier to prevent material overflow.
        /// </summary>
        public ConfigEntry<bool> AutoScaleDependentInputs { get; private set; }

        // ===========================================================
        // GROUP 1 – Hand Tools & Mobile Wash Plants
        // Shovel, bucket, hog pan, mobile wash plant
        // ===========================================================

        public ConfigEntry<float> Shovel_FillSpeed { get; private set; }
        public ConfigEntry<float> Bucket_Capacity { get; private set; }
        public ConfigEntry<float> HogPan_Capacity { get; private set; }
        public ConfigEntry<float> MobileWashPlant_Speed { get; private set; }
        public ConfigEntry<float> MobileWashPlant_Capacity { get; private set; }

        // ===========================================================
        // GROUP 2 – Vehicles
        // Excavators (all), wheel loader, backhoe loader, dump truck
        // ===========================================================

        public ConfigEntry<float> Excavator_DigSpeed { get; private set; }
        public ConfigEntry<float> WheelLoader_LoadSpeed { get; private set; }
        public ConfigEntry<float> BackhoeLoader_LoadSpeed { get; private set; }
        public ConfigEntry<float> DumpTruck_Capacity { get; private set; }

        // ===========================================================
        // GROUP 3 – Wash Plant Modules
        // Hopper, conveyor bucket, wash plants (capacity & speed), sluice boxes, miner's moss
        // ===========================================================

        public ConfigEntry<float> Hopper_Capacity { get; private set; }
        public ConfigEntry<float> ConveyorBucket_Capacity { get; private set; }
        public ConfigEntry<float> Washplant_Capacity { get; private set; }
        public ConfigEntry<float> Washplant_Speed { get; private set; }
        public ConfigEntry<float> Sluicebox_Capacity { get; private set; }
        public ConfigEntry<float> MinersMoss_Capacity { get; private set; }

        // ===========================================================
        // GROUP 4 – Fine Processing
        // Nuggetator, magnetite separator (speed & capacity), wave table (speed & capacity)
        // ===========================================================

        public ConfigEntry<float> Nuggetator_Speed { get; private set; }
        public ConfigEntry<float> MagnetiteSeparator_Speed { get; private set; }
        public ConfigEntry<float> MagnetiteSeparator_Capacity { get; private set; }
        public ConfigEntry<float> WaveTable_Speed { get; private set; }
        public ConfigEntry<float> WaveTable_Capacity { get; private set; }

        // ===========================================================
        // GROUP 5 – Trailers
        // Magnetite trailer, fuel trailer
        // ===========================================================

        public ConfigEntry<float> MagnetiteTrailer_Capacity { get; private set; }
        public ConfigEntry<float> FuelTrailer_Capacity { get; private set; }

        // ===========================================================
        // CONSTRUCTOR
        // ===========================================================

        public TuningConfig(ConfigFile cfg)
        {
            _cfg = cfg;
            BindAll();
            ApplyCascadeProtection();
        }

        private void BindAll()
        {
            // General
            AutoScaleDependentInputs = _cfg.Bind("General", "AutoScaleDependentInputs", true,
                "Automatically scales dependent devices (hog pan, wave table, magnetite separator, trailers) " +
                "to at least the bucket multiplier to prevent material loss.");

            // Group 1
            Shovel_FillSpeed = BindStep("Group1_HandTools", "Shovel_FillSpeed", 2.0f,
                "How fast the shovel picks up material.");
            Bucket_Capacity = BindStep("Group1_HandTools", "Bucket_Capacity", 2.0f,
                "Maximum fill volume of the bucket.");
            HogPan_Capacity = BindStep("Group1_HandTools", "HogPan_Capacity", 2.0f,
                "Maximum fill volume of the hog pan.", DependentCapacitySteps);
            MobileWashPlant_Speed = BindStep("Group1_HandTools", "MobileWashPlant_Speed", 3.0f,
                "Processing speed of the mobile wash plant.");
            MobileWashPlant_Capacity = BindStep("Group1_HandTools", "MobileWashPlant_Capacity", 2.0f,
                "Maximum material capacity of the mobile wash plant.");

            // Group 2
            Excavator_DigSpeed = BindStep("Group2_Vehicles", "Excavator_DigSpeed", 3.0f,
                "How fast the excavator digs.");
            WheelLoader_LoadSpeed = BindStep("Group2_Vehicles", "WheelLoader_LoadSpeed", 3.0f,
                "How fast the wheel loader picks up material.");
            BackhoeLoader_LoadSpeed = BindStep("Group2_Vehicles", "BackhoeLoader_LoadSpeed", 3.0f,
                "How fast the backhoe loader picks up material.");
            DumpTruck_Capacity = BindStep("Group2_Vehicles", "DumpTruck_Capacity", 3.0f,
                "Maximum load capacity of the dump truck.");

            // Group 3
            Hopper_Capacity = BindStep("Group3_WashPlantModules", "Hopper_Capacity", 2.0f,
                "Fill capacity of the feed hopper.");
            ConveyorBucket_Capacity = BindStep("Group3_WashPlantModules", "ConveyorBucket_Capacity", 2.0f,
                "Material capacity of the conveyor buckets.");
            Washplant_Capacity = BindStep("Group3_WashPlantModules", "Washplant_Capacity", 2.0f,
                "Maximum material capacity of all wash plants.");
            Washplant_Speed = BindStep("Group3_WashPlantModules", "Washplant_Speed", 2.0f,
                "Processing speed of all wash plants.");
            Sluicebox_Capacity = BindStep("Group3_WashPlantModules", "Sluicebox_Capacity", 2.0f,
                "Maximum material capacity of all sluice boxes.");
            MinersMoss_Capacity = BindStep("Group3_WashPlantModules", "MinersMoss_Capacity", 2.0f,
                "Gold retention capacity of the miner's moss mats.");

            // Group 4
            Nuggetator_Speed = BindStep("Group4_FineProcessing", "Nuggetator_Speed", 2.0f,
                "Processing speed of the nuggetator.");
            MagnetiteSeparator_Speed = BindStep("Group4_FineProcessing", "MagnetiteSeparator_Speed", 2.0f,
                "Separation speed of the magnetite separator.");
            MagnetiteSeparator_Capacity = BindStep("Group4_FineProcessing", "MagnetiteSeparator_Capacity", 2.0f,
                "Maximum input capacity of the magnetite separator.", DependentCapacitySteps);
            WaveTable_Speed = BindStep("Group4_FineProcessing", "WaveTable_Speed", 3.0f,
                "Vibration speed of the wave table.");
            WaveTable_Capacity = BindStep("Group4_FineProcessing", "WaveTable_Capacity", 3.0f,
                "Maximum material volume on the wave table.", DependentCapacitySteps);

            // Group 5
            MagnetiteTrailer_Capacity = BindStep("Group5_Trailers", "MagnetiteTrailer_Capacity", 2.0f,
                "Load capacity of the magnetite trailer.", DependentCapacitySteps);
            FuelTrailer_Capacity = BindStep("Group5_Trailers", "FuelTrailer_Capacity", 3.0f,
                "Load capacity of the fuel trailer.");

            // Event listeners: reactive cascade protection
            AutoScaleDependentInputs.SettingChanged += (s, e) => ApplyCascadeProtection();
            Bucket_Capacity.SettingChanged += (s, e) => ApplyCascadeProtection();

            // Prevent manual reduction of dependent inputs below bucket multiplier
            HogPan_Capacity.SettingChanged += (s, e) => EnforceMinimum(HogPan_Capacity);
            MagnetiteSeparator_Capacity.SettingChanged += (s, e) => EnforceMinimum(MagnetiteSeparator_Capacity);
            WaveTable_Capacity.SettingChanged += (s, e) => EnforceMinimum(WaveTable_Capacity);
            MagnetiteTrailer_Capacity.SettingChanged += (s, e) => EnforceMinimum(MagnetiteTrailer_Capacity);
        }

        private ConfigEntry<float> BindStep(string section, string key, float defaultValue, string description, float[] steps = null)
        {
            return _cfg.Bind(section, key, defaultValue,
                new ConfigDescription(description,
                    new AcceptableValueList<float>(steps ?? StandardSteps)));
        }

        /// <summary>
        /// Determines the maximum allowed capacity for the bucket,
        /// based on the maximum acceptable capacity of all dependent equipment.
        /// </summary>
        public float GetMaxAllowedBucketCapacity()
        {
            float maxHogPan = GetEntryMax(HogPan_Capacity);
            float maxSeparator = GetEntryMax(MagnetiteSeparator_Capacity);
            float maxWaveTable = GetEntryMax(WaveTable_Capacity);
            float maxTrailer = GetEntryMax(MagnetiteTrailer_Capacity);

            float maxAllowed = Mathf.Min(maxHogPan, Mathf.Min(maxSeparator, Mathf.Min(maxWaveTable, maxTrailer)));
            return maxAllowed;
        }

        private static float GetEntryMax(ConfigEntry<float> entry)
        {
            if (entry?.Description?.AcceptableValues is AcceptableValueList<float> list && list.AcceptableValues.Length > 0)
            {
                return list.AcceptableValues[list.AcceptableValues.Length - 1];
            }
            return 10.0f;
        }

        /// <summary>
        /// Clamps bucket capacity to the maximum capacity supported by dependent inputs.
        /// When AutoScaleDependentInputs is enabled, ensures all dependent inputs
        /// (hog pan, magnetite separator, wave table, trailers) match at least the bucket capacity.
        /// </summary>
        public void ApplyCascadeProtection()
        {
            if (_isSyncing) return;
            _isSyncing = true;

            try
            {
                // 1. Clamp bucket capacity against maximum allowed dependent capacity
                float maxBucket = GetMaxAllowedBucketCapacity();
                if (Bucket_Capacity.Value > maxBucket)
                {
                    Bucket_Capacity.Value = maxBucket;
                }

                // 2. Cascade protection: dependent inputs must at least match bucket
                if (AutoScaleDependentInputs.Value)
                {
                    float bucketVal = Bucket_Capacity.Value;

                    if (HogPan_Capacity.Value < bucketVal) HogPan_Capacity.Value = bucketVal;
                    if (MagnetiteSeparator_Capacity.Value < bucketVal) MagnetiteSeparator_Capacity.Value = bucketVal;
                    if (WaveTable_Capacity.Value < bucketVal) WaveTable_Capacity.Value = bucketVal;
                    if (MagnetiteTrailer_Capacity.Value < bucketVal) MagnetiteTrailer_Capacity.Value = bucketVal;
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void EnforceMinimum(ConfigEntry<float> entry)
        {
            if (_isSyncing || !AutoScaleDependentInputs.Value) return;
            float bucketVal = Bucket_Capacity.Value;
            if (entry.Value < bucketVal)
            {
                _isSyncing = true;
                try
                {
                    entry.Value = bucketVal;
                }
                finally
                {
                    _isSyncing = false;
                }
            }
        }

        // ===========================================================
        // GROUP RESET API
        // ===========================================================

        public void ResetGroup(int groupIndex)
        {
            _isSyncing = true;
            try
            {
                switch (groupIndex)
                {
                    case 1:
                        Shovel_FillSpeed.Value = (float)Shovel_FillSpeed.DefaultValue;
                        Bucket_Capacity.Value = (float)Bucket_Capacity.DefaultValue;
                        HogPan_Capacity.Value = (float)HogPan_Capacity.DefaultValue;
                        MobileWashPlant_Speed.Value = (float)MobileWashPlant_Speed.DefaultValue;
                        MobileWashPlant_Capacity.Value = (float)MobileWashPlant_Capacity.DefaultValue;
                        break;
                    case 2:
                        Excavator_DigSpeed.Value = (float)Excavator_DigSpeed.DefaultValue;
                        WheelLoader_LoadSpeed.Value = (float)WheelLoader_LoadSpeed.DefaultValue;
                        BackhoeLoader_LoadSpeed.Value = (float)BackhoeLoader_LoadSpeed.DefaultValue;
                        DumpTruck_Capacity.Value = (float)DumpTruck_Capacity.DefaultValue;
                        break;
                    case 3:
                        Hopper_Capacity.Value = (float)Hopper_Capacity.DefaultValue;
                        ConveyorBucket_Capacity.Value = (float)ConveyorBucket_Capacity.DefaultValue;
                        Washplant_Capacity.Value = (float)Washplant_Capacity.DefaultValue;
                        Washplant_Speed.Value = (float)Washplant_Speed.DefaultValue;
                        Sluicebox_Capacity.Value = (float)Sluicebox_Capacity.DefaultValue;
                        MinersMoss_Capacity.Value = (float)MinersMoss_Capacity.DefaultValue;
                        break;
                    case 4:
                        Nuggetator_Speed.Value = (float)Nuggetator_Speed.DefaultValue;
                        MagnetiteSeparator_Speed.Value = (float)MagnetiteSeparator_Speed.DefaultValue;
                        MagnetiteSeparator_Capacity.Value = (float)MagnetiteSeparator_Capacity.DefaultValue;
                        WaveTable_Speed.Value = (float)WaveTable_Speed.DefaultValue;
                        WaveTable_Capacity.Value = (float)WaveTable_Capacity.DefaultValue;
                        break;
                    case 5:
                        MagnetiteTrailer_Capacity.Value = (float)MagnetiteTrailer_Capacity.DefaultValue;
                        FuelTrailer_Capacity.Value = (float)FuelTrailer_Capacity.DefaultValue;
                        break;
                }
            }
            finally
            {
                _isSyncing = false;
            }

            ApplyCascadeProtection();
            _cfg.Save();
        }

        public void ResetAll()
        {
            for (int i = 1; i <= 5; i++) ResetGroup(i);
        }
    }
}
