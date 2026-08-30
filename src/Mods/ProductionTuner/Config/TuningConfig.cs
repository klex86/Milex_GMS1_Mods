using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Config
{
    /// <summary>
    /// Schicht 1: Verwaltet alle Einstellungen des Production Tuner Mods.
    ///
    /// Jede Einstellung wird als Multiplikator (1.0 = Standardwert des Spiels, 2.0 = doppelte Leistung)
    /// in der Datei BepInEx/config/Milex_GMS1_ProductionTuner.cfg gespeichert.
    ///
    /// Alle Config-Keys sind auf Englisch. Anzeigenamen kommen aus den Lokalisierungsdateien.
    /// Jede Gruppe hat einen eigenen Simple/Advanced-Schalter.
    /// Gruppe 5 (Anhaenger) hat keinen Gruppen-Multiplikator, da die Komponenten nichts miteinander zu tun haben.
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

        private const float DefaultMultiplier = 1.0f;
        private readonly ConfigFile _cfg;

        // ===========================================================
        // ALLGEMEINE EINSTELLUNGEN
        // ===========================================================

        /// <summary>
        /// Wenn aktiv, werden Folgegeraete (Pfanne, Wave Table, Magnetitabscheider, Anhaenger)
        /// automatisch mindestens auf den Eimer-Multiplikator skaliert.
        /// </summary>
        public ConfigEntry<bool> AutoScaleDependentInputs { get; private set; }

        // ===========================================================
        // GRUPPE 1 – Hand Tools & Mobile Wash Plants
        // Schaufel, Eimer, Pfanne, Hog Pan, Mobile Waschanlage
        // ===========================================================

        public ConfigEntry<bool> Group1SimpleMode { get; private set; }
        public ConfigEntry<float> Group1Multiplier { get; private set; }

        // Advanced: individual controls
        public ConfigEntry<float> Shovel_FillSpeed    { get; private set; }
        public ConfigEntry<float> Bucket_Capacity     { get; private set; }
        public ConfigEntry<float> Pan_Capacity        { get; private set; }
        public ConfigEntry<float> HogPan_Capacity     { get; private set; }
        public ConfigEntry<float> MobileWashPlant_Speed { get; private set; }

        // ===========================================================
        // GRUPPE 2 – Vehicles & Mobile Conveyor
        // Minibagger, Bagger, Radlader, Baggerlader, Mobiles Foerderband
        // ===========================================================

        public ConfigEntry<bool> Group2SimpleMode { get; private set; }
        public ConfigEntry<float> Group2Multiplier { get; private set; }

        // Advanced
        public ConfigEntry<float> MiniExcavator_DigSpeed  { get; private set; }
        public ConfigEntry<float> Excavator_DigSpeed      { get; private set; }
        public ConfigEntry<float> WheelLoader_LoadSpeed   { get; private set; }
        public ConfigEntry<float> BackhoeLoader_LoadSpeed { get; private set; }
        public ConfigEntry<float> MobileConveyor_Speed    { get; private set; }

        // ===========================================================
        // GRUPPE 3 – Wash Plant Modules (Tier 3-6)
        // Einfuelltrichter, Foerderband, Ruettler, Derocker, Waschrinne, Trommel, Jig, Miner's Moss
        // ===========================================================

        public ConfigEntry<bool> Group3SimpleMode { get; private set; }
        public ConfigEntry<float> Group3Multiplier { get; private set; }

        // Advanced
        public ConfigEntry<float> Hopper_Capacity       { get; private set; }
        public ConfigEntry<float> Conveyor_Speed        { get; private set; }
        public ConfigEntry<float> VibratingScreen_Speed { get; private set; }
        public ConfigEntry<float> Derocker_Speed        { get; private set; }
        public ConfigEntry<float> Sluice_Speed          { get; private set; }
        public ConfigEntry<float> Trommel_Speed         { get; private set; }
        public ConfigEntry<float> Jig_Speed             { get; private set; }
        public ConfigEntry<float> MinersMoss_Capacity   { get; private set; }

        // ===========================================================
        // GRUPPE 4 – Fine Processing
        // Nuggetator, Magnetitabscheider, Wave Table
        // ===========================================================

        public ConfigEntry<bool> Group4SimpleMode { get; private set; }
        public ConfigEntry<float> Group4Multiplier { get; private set; }

        // Advanced
        public ConfigEntry<float> Nuggetator_Speed          { get; private set; }
        public ConfigEntry<float> MagnetiteSeparator_Speed  { get; private set; }
        public ConfigEntry<float> WaveTable_Speed           { get; private set; }
        public ConfigEntry<float> WaveTable_Capacity        { get; private set; }

        // ===========================================================
        // GRUPPE 5 – Trailers (KEIN Gruppen-Multiplikator – Komponenten sind unabhaengig)
        // Magnetitanhaenger, Kraftstoffanhaenger
        // ===========================================================

        public ConfigEntry<float> MagnetiteTrailer_Capacity { get; private set; }
        public ConfigEntry<float> FuelTrailer_Capacity      { get; private set; }

        // ===========================================================
        // KONSTRUKTOR
        // ===========================================================

        public TuningConfig(ConfigFile cfg)
        {
            _cfg = cfg;
            BindAll();
        }

        private void BindAll()
        {
            // Allgemein
            AutoScaleDependentInputs = _cfg.Bind("General", "AutoScaleDependentInputs", true,
                "Automatically scales dependent devices (pan, wave table, magnetite separator, trailers) " +
                "to at least the bucket multiplier to prevent material loss.");

            // Gruppe 1
            Group1SimpleMode    = _cfg.Bind("Group1_HandTools", "SimpleMode", true,
                "Simple mode: only the group multiplier slider is active. Advanced mode: individual sliders per component.");
            Group1Multiplier    = BindStep("Group1_HandTools", "Group_Multiplier",
                "Shared multiplier for all hand tools and mobile wash plants.");
            Shovel_FillSpeed    = BindStep("Group1_HandTools", "Shovel_FillSpeed",
                "How fast the shovel picks up material.");
            Bucket_Capacity     = BindStep("Group1_HandTools", "Bucket_Capacity",
                "Maximum fill volume of the bucket.");
            Pan_Capacity        = BindStep("Group1_HandTools", "Pan_Capacity",
                "Maximum fill volume of the gold panning pan.");
            HogPan_Capacity     = BindStep("Group1_HandTools", "HogPan_Capacity",
                "Maximum fill volume of the hog pan.");
            MobileWashPlant_Speed = BindStep("Group1_HandTools", "MobileWashPlant_Speed",
                "Processing speed of the mobile wash plant.");

            // Gruppe 2
            Group2SimpleMode    = _cfg.Bind("Group2_Vehicles", "SimpleMode", true,
                "Simple mode: only the group multiplier slider is active. Advanced mode: individual sliders per component.");
            Group2Multiplier    = BindStep("Group2_Vehicles", "Group_Multiplier",
                "Shared multiplier for all construction vehicles and the mobile conveyor belt.");
            MiniExcavator_DigSpeed  = BindStep("Group2_Vehicles", "MiniExcavator_DigSpeed",  "How fast the mini excavator digs.");
            Excavator_DigSpeed      = BindStep("Group2_Vehicles", "Excavator_DigSpeed",      "How fast the excavator digs.");
            WheelLoader_LoadSpeed   = BindStep("Group2_Vehicles", "WheelLoader_LoadSpeed",   "How fast the wheel loader picks up material.");
            BackhoeLoader_LoadSpeed = BindStep("Group2_Vehicles", "BackhoeLoader_LoadSpeed", "How fast the backhoe loader picks up material.");
            MobileConveyor_Speed    = BindStep("Group2_Vehicles", "MobileConveyor_Speed",    "Transport speed of the mobile conveyor belt.");

            // Gruppe 3
            Group3SimpleMode    = _cfg.Bind("Group3_WashPlantModules", "SimpleMode", true,
                "Simple mode: only the group multiplier slider is active. Advanced mode: individual sliders per component.");
            Group3Multiplier    = BindStep("Group3_WashPlantModules", "Group_Multiplier",
                "Shared multiplier for all wash plant modules.");
            Hopper_Capacity       = BindStep("Group3_WashPlantModules", "Hopper_Capacity",       "Fill capacity of the feed hopper.");
            Conveyor_Speed        = BindStep("Group3_WashPlantModules", "Conveyor_Speed",        "Transport speed of stationary conveyor belts.");
            VibratingScreen_Speed = BindStep("Group3_WashPlantModules", "VibratingScreen_Speed", "Screening throughput of the vibrating screen.");
            Derocker_Speed        = BindStep("Group3_WashPlantModules", "Derocker_Speed",        "Processing speed of the derocker.");
            Sluice_Speed          = BindStep("Group3_WashPlantModules", "Sluice_Speed",          "Material flow rate through the sluice box.");
            Trommel_Speed         = BindStep("Group3_WashPlantModules", "Trommel_Speed",         "Rotation speed of the trommel wash plant.");
            Jig_Speed             = BindStep("Group3_WashPlantModules", "Jig_Speed",             "Processing speed of the jig.");
            MinersMoss_Capacity   = BindStep("Group3_WashPlantModules", "MinersMoss_Capacity",   "Gold retention capacity of the miner's moss mats.");

            // Gruppe 4
            Group4SimpleMode    = _cfg.Bind("Group4_FineProcessing", "SimpleMode", true,
                "Simple mode: only the group multiplier slider is active. Advanced mode: individual sliders per component.");
            Group4Multiplier    = BindStep("Group4_FineProcessing", "Group_Multiplier",
                "Shared multiplier for all fine processing equipment.");
            Nuggetator_Speed         = BindStep("Group4_FineProcessing", "Nuggetator_Speed",        "Processing speed of the nuggetator.");
            MagnetiteSeparator_Speed = BindStep("Group4_FineProcessing", "MagnetiteSeparator_Speed","Separation speed of the magnetite separator.");
            WaveTable_Speed          = BindStep("Group4_FineProcessing", "WaveTable_Speed",         "Vibration speed of the wave table.");
            WaveTable_Capacity       = BindStep("Group4_FineProcessing", "WaveTable_Capacity",      "Maximum material volume on the wave table.");

            // Gruppe 5 – kein SimpleMode/Gruppen-Multiplikator
            MagnetiteTrailer_Capacity = BindStep("Group5_Trailers", "MagnetiteTrailer_Capacity", "Load capacity of the magnetite trailer.");
            FuelTrailer_Capacity      = BindStep("Group5_Trailers", "FuelTrailer_Capacity",      "Load capacity of the fuel trailer.");
        }

        private ConfigEntry<float> BindStep(string section, string key, string description)
        {
            return _cfg.Bind(section, key, DefaultMultiplier,
                new ConfigDescription(description,
                    new AcceptableValueList<float>(Steps)));
        }

        // ===========================================================
        // GRUPPEN-RESET-API
        // ===========================================================

        public void ResetGroup(int groupIndex)
        {
            switch (groupIndex)
            {
                case 1:
                    Group1Multiplier.Value      = DefaultMultiplier;
                    Shovel_FillSpeed.Value       = DefaultMultiplier;
                    Bucket_Capacity.Value        = DefaultMultiplier;
                    Pan_Capacity.Value           = DefaultMultiplier;
                    HogPan_Capacity.Value        = DefaultMultiplier;
                    MobileWashPlant_Speed.Value  = DefaultMultiplier;
                    break;
                case 2:
                    Group2Multiplier.Value          = DefaultMultiplier;
                    MiniExcavator_DigSpeed.Value    = DefaultMultiplier;
                    Excavator_DigSpeed.Value        = DefaultMultiplier;
                    WheelLoader_LoadSpeed.Value     = DefaultMultiplier;
                    BackhoeLoader_LoadSpeed.Value   = DefaultMultiplier;
                    MobileConveyor_Speed.Value      = DefaultMultiplier;
                    break;
                case 3:
                    Group3Multiplier.Value       = DefaultMultiplier;
                    Hopper_Capacity.Value        = DefaultMultiplier;
                    Conveyor_Speed.Value         = DefaultMultiplier;
                    VibratingScreen_Speed.Value  = DefaultMultiplier;
                    Derocker_Speed.Value         = DefaultMultiplier;
                    Sluice_Speed.Value           = DefaultMultiplier;
                    Trommel_Speed.Value          = DefaultMultiplier;
                    Jig_Speed.Value              = DefaultMultiplier;
                    MinersMoss_Capacity.Value    = DefaultMultiplier;
                    break;
                case 4:
                    Group4Multiplier.Value           = DefaultMultiplier;
                    Nuggetator_Speed.Value           = DefaultMultiplier;
                    MagnetiteSeparator_Speed.Value   = DefaultMultiplier;
                    WaveTable_Speed.Value            = DefaultMultiplier;
                    WaveTable_Capacity.Value         = DefaultMultiplier;
                    break;
                case 5:
                    MagnetiteTrailer_Capacity.Value = DefaultMultiplier;
                    FuelTrailer_Capacity.Value      = DefaultMultiplier;
                    break;
            }
            _cfg.Save();
        }

        public void ResetAll()
        {
            for (int i = 1; i <= 5; i++) ResetGroup(i);
        }
    }
}
