using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Config
{
    /// <summary>
    /// Schicht 1: Verwaltet alle Einstellungen des Production Tuner Mods.
    ///
    /// Jede Einstellung wird als Multiplikator auf den jeweiligen Basiswert des Spiels
    /// in der Datei BepInEx/config/Milex_GMS1_ProductionTuner.cfg gespeichert.
    /// Jeder Wert besitzt seinen eigenen Default-Multiplikator.
    ///
    /// Alle Config-Keys und Beschreibungen sind auf Englisch. Anzeigenamen kommen aus den Lokalisierungsdateien.
    /// Jede Gruppe (1-4) hat einen eigenen Simple/Advanced-Schalter.
    /// Gruppe 5 (Anhaenger) hat keinen Gruppen-Multiplikator, da die Komponenten unabhaengig sind.
    /// Kaskadenschutz synchronisiert abhaengige Stationen live und sichtbar in der GUI.
    /// </summary>
    public class TuningConfig
    {
        // Erlaubte Multiplikatorwerte in 0.5-Schritten von 0.5 bis 10.0
        private static readonly float[] Steps = GenerateSteps();

        private static float[] GenerateSteps()
        {
            var list = new List<float>();
            for (float v = 0.5f; v <= 10.0f + 0.01f; v += 0.5f)
                list.Add((float)System.Math.Round(v, 1));
            return list.ToArray();
        }

        private readonly ConfigFile _cfg;
        private bool _isSyncing = false;

        // ===========================================================
        // ALLGEMEINE EINSTELLUNGEN
        // ===========================================================

        /// <summary>
        /// Wenn aktiv, werden Folgegeraete (Hog Pan, Wave Table, Magnetitabscheider, Anhaenger)
        /// automatisch mindestens auf den Eimer-Multiplikator skaliert.
        /// </summary>
        public ConfigEntry<bool> AutoScaleDependentInputs { get; private set; }

        // ===========================================================
        // GRUPPE 1 – Hand Tools & Mobile Wash Plants
        // Schaufel, Eimer, Hog Pan, Mobile Waschanlage
        // ===========================================================

        public ConfigEntry<bool> Group1SimpleMode { get; private set; }
        public ConfigEntry<float> Group1Multiplier { get; private set; }

        // Advanced: individual controls
        public ConfigEntry<float> Shovel_FillSpeed { get; private set; }
        public ConfigEntry<float> Bucket_Capacity { get; private set; }
        public ConfigEntry<float> HogPan_Capacity { get; private set; }
        public ConfigEntry<float> MobileWashPlant_Speed { get; private set; }
        public ConfigEntry<float> MobileWashPlant_Capacity { get; private set; }

        // ===========================================================
        // GRUPPE 2 – Vehicles
        // Bagger (alle), Radlader, Baggerlader, Muldenkipper (Dump Truck)
        // ===========================================================

        public ConfigEntry<bool> Group2SimpleMode { get; private set; }
        public ConfigEntry<float> Group2Multiplier { get; private set; }

        // Advanced
        public ConfigEntry<float> Excavator_DigSpeed { get; private set; }
        public ConfigEntry<float> WheelLoader_LoadSpeed { get; private set; }
        public ConfigEntry<float> BackhoeLoader_LoadSpeed { get; private set; }
        public ConfigEntry<float> DumpTruck_Capacity { get; private set; }

        // ===========================================================
        // GRUPPE 3 – Wash Plant Modules
        // Einfuelltrichter, Foerderband-Eimer, Waschanlagen (Kapazitaet & Speed), Waschrinnen, Miner's Moss
        // ===========================================================

        public ConfigEntry<bool> Group3SimpleMode { get; private set; }
        public ConfigEntry<float> Group3Multiplier { get; private set; }

        // Advanced
        public ConfigEntry<float> Hopper_Capacity { get; private set; }
        public ConfigEntry<float> ConveyorBucket_Capacity { get; private set; }
        public ConfigEntry<float> Washplant_Capacity { get; private set; }
        public ConfigEntry<float> Washplant_Speed { get; private set; }
        public ConfigEntry<float> Sluicebox_Capacity { get; private set; }
        public ConfigEntry<float> MinersMoss_Capacity { get; private set; }

        // ===========================================================
        // GRUPPE 4 – Fine Processing
        // Nuggetator, Magnetitabscheider (Speed & Kapazitaet), Wave Table (Speed & Kapazitaet)
        // ===========================================================

        public ConfigEntry<bool> Group4SimpleMode { get; private set; }
        public ConfigEntry<float> Group4Multiplier { get; private set; }

        // Advanced
        public ConfigEntry<float> Nuggetator_Speed { get; private set; }
        public ConfigEntry<float> MagnetiteSeparator_Speed { get; private set; }
        public ConfigEntry<float> MagnetiteSeparator_Capacity { get; private set; }
        public ConfigEntry<float> WaveTable_Speed { get; private set; }
        public ConfigEntry<float> WaveTable_Capacity { get; private set; }

        // ===========================================================
        // GRUPPE 5 – Trailers (KEIN Gruppen-Multiplikator – Komponenten sind unabhaengig)
        // Magnetitanhaenger, Kraftstoffanhaenger
        // ===========================================================

        public ConfigEntry<float> MagnetiteTrailer_Capacity { get; private set; }
        public ConfigEntry<float> FuelTrailer_Capacity { get; private set; }

        // ===========================================================
        // KONSTRUKTOR
        // ===========================================================

        public TuningConfig(ConfigFile cfg)
        {
            _cfg = cfg;
            BindAll();
            ApplyCascadeProtection();
        }

        private void BindAll()
        {
            // Allgemein
            AutoScaleDependentInputs = _cfg.Bind("General", "AutoScaleDependentInputs", true,
                "Automatically scales dependent devices (pan, wave table, magnetite separator, trailers) " +
                "to at least the bucket multiplier to prevent material loss.");

            // Gruppe 1
            Group1SimpleMode = _cfg.Bind("Group1_HandTools", "SimpleMode", true,
                "Simple mode: only the group multiplier slider is active. Advanced mode: individual sliders per component.");
            Group1Multiplier = BindStep("Group1_HandTools", "Group_Multiplier", 1.0f,
                "Shared multiplier for all hand tools and mobile wash plants.");
            Shovel_FillSpeed = BindStep("Group1_HandTools", "Shovel_FillSpeed", 2.0f,
                "How fast the shovel picks up material.");
            Bucket_Capacity = BindStep("Group1_HandTools", "Bucket_Capacity", 2.0f,
                "Maximum fill volume of the bucket.");
            HogPan_Capacity = BindStep("Group1_HandTools", "HogPan_Capacity", 2.0f,
                "Maximum fill volume of the hog pan.");
            MobileWashPlant_Speed = BindStep("Group1_HandTools", "MobileWashPlant_Speed", 3.0f,
                "Processing speed of the mobile wash plant.");
            MobileWashPlant_Capacity = BindStep("Group1_HandTools", "MobileWashPlant_Capacity", 2.0f,
                "Maximum material capacity of the mobile wash plant.");

            // Gruppe 2
            Group2SimpleMode = _cfg.Bind("Group2_Vehicles", "SimpleMode", true,
                "Simple mode: only the group multiplier slider is active. Advanced mode: individual sliders per component.");
            Group2Multiplier = BindStep("Group2_Vehicles", "Group_Multiplier", 1.0f,
                "Shared multiplier for all construction vehicles.");
            Excavator_DigSpeed = BindStep("Group2_Vehicles", "Excavator_DigSpeed", 3.0f,
                "How fast the excavator digs.");
            WheelLoader_LoadSpeed = BindStep("Group2_Vehicles", "WheelLoader_LoadSpeed", 3.0f,
                "How fast the wheel loader picks up material.");
            BackhoeLoader_LoadSpeed = BindStep("Group2_Vehicles", "BackhoeLoader_LoadSpeed", 3.0f,
                "How fast the backhoe loader picks up material.");
            DumpTruck_Capacity = BindStep("Group2_Vehicles", "DumpTruck_Capacity", 3.0f,
                "Maximum load capacity of the dump truck.");

            // Gruppe 3
            Group3SimpleMode = _cfg.Bind("Group3_WashPlantModules", "SimpleMode", true,
                "Simple mode: only the group multiplier slider is active. Advanced mode: individual sliders per component.");
            Group3Multiplier = BindStep("Group3_WashPlantModules", "Group_Multiplier", 1.0f,
                "Shared multiplier for all wash plant modules.");
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

            // Gruppe 4
            Group4SimpleMode = _cfg.Bind("Group4_FineProcessing", "SimpleMode", true,
                "Simple mode: only the group multiplier slider is active. Advanced mode: individual sliders per component.");
            Group4Multiplier = BindStep("Group4_FineProcessing", "Group_Multiplier", 1.0f,
                "Shared multiplier for all fine processing equipment.");
            Nuggetator_Speed = BindStep("Group4_FineProcessing", "Nuggetator_Speed", 2.0f,
                "Processing speed of the nuggetator.");
            MagnetiteSeparator_Speed = BindStep("Group4_FineProcessing", "MagnetiteSeparator_Speed", 2.0f,
                "Separation speed of the magnetite separator.");
            MagnetiteSeparator_Capacity = BindStep("Group4_FineProcessing", "MagnetiteSeparator_Capacity", 2.0f,
                "Maximum input capacity of the magnetite separator.");
            WaveTable_Speed = BindStep("Group4_FineProcessing", "WaveTable_Speed", 3.0f,
                "Vibration speed of the wave table.");
            WaveTable_Capacity = BindStep("Group4_FineProcessing", "WaveTable_Capacity", 3.0f,
                "Maximum material volume on the wave table.");

            // Gruppe 5 – kein SimpleMode/Gruppen-Multiplikator
            MagnetiteTrailer_Capacity = BindStep("Group5_Trailers", "MagnetiteTrailer_Capacity", 2.0f,
                "Load capacity of the magnetite trailer.");
            FuelTrailer_Capacity = BindStep("Group5_Trailers", "FuelTrailer_Capacity", 3.0f,
                "Load capacity of the fuel trailer.");

            // Event-Listener: Reaktiver Kaskadenschutz und SimpleMode-Synchronisation
            AutoScaleDependentInputs.SettingChanged += (s, e) => ApplyCascadeProtection();

            Group1SimpleMode.SettingChanged += (s, e) => ApplyCascadeProtection();
            Group1Multiplier.SettingChanged += (s, e) => ApplyCascadeProtection();
            Bucket_Capacity.SettingChanged += (s, e) => ApplyCascadeProtection();

            Group2SimpleMode.SettingChanged += (s, e) => ApplyCascadeProtection();
            Group2Multiplier.SettingChanged += (s, e) => ApplyCascadeProtection();

            Group3SimpleMode.SettingChanged += (s, e) => ApplyCascadeProtection();
            Group3Multiplier.SettingChanged += (s, e) => ApplyCascadeProtection();

            Group4SimpleMode.SettingChanged += (s, e) => ApplyCascadeProtection();
            Group4Multiplier.SettingChanged += (s, e) => ApplyCascadeProtection();

            // Manuelle Eingabe auf abhängigen Reglern gegen Bucket-Minimum absichern
            HogPan_Capacity.SettingChanged += (s, e) => EnforceMinimum(HogPan_Capacity);
            MagnetiteSeparator_Capacity.SettingChanged += (s, e) => EnforceMinimum(MagnetiteSeparator_Capacity);
            WaveTable_Capacity.SettingChanged += (s, e) => EnforceMinimum(WaveTable_Capacity);
            MagnetiteTrailer_Capacity.SettingChanged += (s, e) => EnforceMinimum(MagnetiteTrailer_Capacity);
        }

        private ConfigEntry<float> BindStep(string section, string key, float defaultValue, string description)
        {
            return _cfg.Bind(section, key, defaultValue,
                new ConfigDescription(description,
                    new AcceptableValueList<float>(Steps)));
        }

        public float GetEffectiveBucketMultiplier()
        {
            return Group1SimpleMode.Value ? (Group1Multiplier.Value * Bucket_Capacity.Value) : Bucket_Capacity.Value;
        }

        /// <summary>
        /// Erzwingt bei aktivem AutoScaleDependentInputs, dass alle Folgegeraete
        /// (Hog Pan, Magnetitabscheider, Wave Table, Anhaenger) mindestens das effektive Eimervolumen erreichen.
        /// </summary>
        public void ApplyCascadeProtection()
        {
            if (_isSyncing) return;
            _isSyncing = true;

            try
            {
                // Kaskadenschutz fuer Eimer-Folgestationen
                if (AutoScaleDependentInputs.Value)
                {
                    float bucketMult = GetEffectiveBucketMultiplier();

                    if (HogPan_Capacity.Value < bucketMult) HogPan_Capacity.Value = bucketMult;
                    if (MagnetiteSeparator_Capacity.Value < bucketMult) MagnetiteSeparator_Capacity.Value = bucketMult;
                    if (WaveTable_Capacity.Value < bucketMult) WaveTable_Capacity.Value = bucketMult;
                    if (MagnetiteTrailer_Capacity.Value < bucketMult) MagnetiteTrailer_Capacity.Value = bucketMult;
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
            float bucketMult = GetEffectiveBucketMultiplier();
            if (entry.Value < bucketMult)
            {
                _isSyncing = true;
                try
                {
                    entry.Value = bucketMult;
                }
                finally
                {
                    _isSyncing = false;
                }
            }
        }

        // ===========================================================
        // GRUPPEN-RESET-API
        // ===========================================================

        public void ResetGroup(int groupIndex)
        {
            _isSyncing = true;
            try
            {
                switch (groupIndex)
                {
                    case 1:
                        Group1SimpleMode.Value = true;
                        Group1Multiplier.Value = (float)Group1Multiplier.DefaultValue;
                        Shovel_FillSpeed.Value = (float)Shovel_FillSpeed.DefaultValue;
                        Bucket_Capacity.Value = (float)Bucket_Capacity.DefaultValue;
                        HogPan_Capacity.Value = (float)HogPan_Capacity.DefaultValue;
                        MobileWashPlant_Speed.Value = (float)MobileWashPlant_Speed.DefaultValue;
                        MobileWashPlant_Capacity.Value = (float)MobileWashPlant_Capacity.DefaultValue;
                        break;
                    case 2:
                        Group2SimpleMode.Value = true;
                        Group2Multiplier.Value = (float)Group2Multiplier.DefaultValue;
                        Excavator_DigSpeed.Value = (float)Excavator_DigSpeed.DefaultValue;
                        WheelLoader_LoadSpeed.Value = (float)WheelLoader_LoadSpeed.DefaultValue;
                        BackhoeLoader_LoadSpeed.Value = (float)BackhoeLoader_LoadSpeed.DefaultValue;
                        DumpTruck_Capacity.Value = (float)DumpTruck_Capacity.DefaultValue;
                        break;
                    case 3:
                        Group3SimpleMode.Value = true;
                        Group3Multiplier.Value = (float)Group3Multiplier.DefaultValue;
                        Hopper_Capacity.Value = (float)Hopper_Capacity.DefaultValue;
                        ConveyorBucket_Capacity.Value = (float)ConveyorBucket_Capacity.DefaultValue;
                        Washplant_Capacity.Value = (float)Washplant_Capacity.DefaultValue;
                        Washplant_Speed.Value = (float)Washplant_Speed.DefaultValue;
                        Sluicebox_Capacity.Value = (float)Sluicebox_Capacity.DefaultValue;
                        MinersMoss_Capacity.Value = (float)MinersMoss_Capacity.DefaultValue;
                        break;
                    case 4:
                        Group4SimpleMode.Value = true;
                        Group4Multiplier.Value = (float)Group4Multiplier.DefaultValue;
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
