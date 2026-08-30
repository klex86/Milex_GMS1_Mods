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
    /// Im Simple Mode (Standard) gibt es einen Regler pro Gruppe.
    /// Im Advanced Mode gibt es einen Regler pro Komponente/Parameter.
    /// </summary>
    public class TuningConfig
    {
        // ---- Allgemeine Einstellungen ----

        /// <summary>
        /// Schaltet den erweiterten Modus um. Im einfachen Modus steuert ein
        /// Regler alle Komponenten einer Gruppe gemeinsam.
        /// Im erweiterten Modus sind alle Einzelregler verfügbar.
        /// </summary>
        public ConfigEntry<bool> AdvancedMode { get; private set; }

        /// <summary>
        /// Wenn aktiv, werden die maximalen Füllmengen von Folgegeräten (Pfanne, Wave Table,
        /// Magnetitabscheider, Anhänger) automatisch mindestens so gross wie der Eimer-Multiplikator
        /// gesetzt. Verhindert Materialverlust bei grossen Eimern.
        /// </summary>
        public ConfigEntry<bool> AutoScaleDependentInputs { get; private set; }

        // ---- Gruppe 1: Handwerkzeuge & Mobile Waschanlagen ----
        // Schaufel, Eimer, Pfanne (pan), Hog Pan, Mobile Waschanlagen

        public ConfigEntry<float> Group1Multiplier { get; private set; }

        // Advanced: Einzelregler
        public ConfigEntry<float> ShovelFillSpeed { get; private set; }
        public ConfigEntry<float> BucketCapacity { get; private set; }
        public ConfigEntry<float> PanCapacity { get; private set; }
        public ConfigEntry<float> HogPanCapacity { get; private set; }
        public ConfigEntry<float> MobileWashPlantSpeed { get; private set; }

        // ---- Gruppe 2: Baufahrzeuge & Mobiles Förderband ----
        // Minibagger, Bagger, Radlader, Baggerlader, Mobiles Förderband

        public ConfigEntry<float> Group2Multiplier { get; private set; }

        // Advanced: Einzelregler
        public ConfigEntry<float> MiniBaggerDigSpeed { get; private set; }
        public ConfigEntry<float> BaggerDigSpeed { get; private set; }
        public ConfigEntry<float> RadladerLoadSpeed { get; private set; }
        public ConfigEntry<float> BaggerladerLoadSpeed { get; private set; }
        public ConfigEntry<float> MobileConveyorSpeed { get; private set; }

        // ---- Gruppe 3: Waschanlagen-Module (Tier 3-6) ----
        // Einfülltrichter, Förderbänder, Rüttler, Derocker, Waschrinnen, Trommeln, Jigs, Miner's Moss

        public ConfigEntry<float> Group3Multiplier { get; private set; }

        // Advanced: Einzelregler
        public ConfigEntry<float> HopperCapacity { get; private set; }
        public ConfigEntry<float> ConveyorSpeed { get; private set; }
        public ConfigEntry<float> VibratingScreenSpeed { get; private set; }
        public ConfigEntry<float> DerockerSpeed { get; private set; }
        public ConfigEntry<float> SluiceSpeed { get; private set; }
        public ConfigEntry<float> TrommelSpeed { get; private set; }
        public ConfigEntry<float> JigSpeed { get; private set; }
        public ConfigEntry<float> MinersMossCapacity { get; private set; }

        // ---- Gruppe 4: Feinverarbeitung ----
        // Nuggetator, Magnetitabscheider, Wave Table Rütteltisch

        public ConfigEntry<float> Group4Multiplier { get; private set; }

        // Advanced: Einzelregler
        public ConfigEntry<float> NuggeterSpeed { get; private set; }
        public ConfigEntry<float> MagnetiteSeparatorSpeed { get; private set; }
        public ConfigEntry<float> WaveTableSpeed { get; private set; }
        public ConfigEntry<float> WaveTableCapacity { get; private set; }

        // ---- Gruppe 5: Anhänger ----
        // Magnetitanhänger, Kraftstoffanhänger

        public ConfigEntry<float> Group5Multiplier { get; private set; }

        // Advanced: Einzelregler
        public ConfigEntry<float> MagnetiteTrailerCapacity { get; private set; }
        public ConfigEntry<float> FuelTrailerCapacity { get; private set; }

        // ---- Standardwerte aller Multiplikatoren ----

        private const float DefaultMultiplier = 1.0f;
        private const float MinMultiplier = 0.1f;
        private const float MaxMultiplier = 10.0f;

        private readonly ConfigFile _config;

        public TuningConfig(ConfigFile config)
        {
            _config = config;
            BindAll();
        }

        private void BindAll()
        {
            // Allgemeine Schalter
            AdvancedMode = _config.Bind("General", "AdvancedMode", false,
                "Einfacher Modus (false): Ein Regler pro Gruppe. Erweiterter Modus (true): Einzelregler pro Komponente und Parameter.");

            AutoScaleDependentInputs = _config.Bind("General", "AutoScaleDependentInputs", true,
                "Skaliert Folgegeraete (Pfanne, Wave Table, Magnetitabscheider, Anhaenger) automatisch mit dem Eimer-Multiplikator, um Materialverlust zu vermeiden.");

            // Gruppe 1 – Simple
            Group1Multiplier = Bind("Group1_HandTools", "GroupMultiplier", DefaultMultiplier,
                "Gemeinsamer Multiplikator fuer alle Handwerkzeuge und Mobile Waschanlagen (Schaufel, Eimer, Pfanne, Hog Pan, Mobile Waschanlage).");

            // Gruppe 1 – Advanced
            ShovelFillSpeed    = Bind("Group1_HandTools", "Schaufel_Fuellgeschwindigkeit", DefaultMultiplier, "Wie schnell die Schaufel Material aufnimmt.");
            BucketCapacity     = Bind("Group1_HandTools", "Eimer_Kapazitaet", DefaultMultiplier, "Fassungsvermoegen des Eimers.");
            PanCapacity        = Bind("Group1_HandTools", "Pfanne_Kapazitaet", DefaultMultiplier, "Fassungsvermoegen der Goldwaesch-Pfanne.");
            HogPanCapacity     = Bind("Group1_HandTools", "HogPan_Kapazitaet", DefaultMultiplier, "Fassungsvermoegen der Hog Pan.");
            MobileWashPlantSpeed = Bind("Group1_HandTools", "MobileWaschanlage_Geschwindigkeit", DefaultMultiplier, "Verarbeitungsgeschwindigkeit der mobilen Waschanlage.");

            // Gruppe 2 – Simple
            Group2Multiplier = Bind("Group2_Vehicles", "GroupMultiplier", DefaultMultiplier,
                "Gemeinsamer Multiplikator fuer alle Baufahrzeuge und das mobile Foerderband (Minibagger, Bagger, Radlader, Baggerlader, Foerderband).");

            // Gruppe 2 – Advanced
            MiniBaggerDigSpeed    = Bind("Group2_Vehicles", "Minibagger_Aushubgeschwindigkeit", DefaultMultiplier, "Wie schnell der Minibagger Material ausgräbt.");
            BaggerDigSpeed        = Bind("Group2_Vehicles", "Bagger_Aushubgeschwindigkeit", DefaultMultiplier, "Wie schnell der Bagger Material ausgraebt.");
            RadladerLoadSpeed     = Bind("Group2_Vehicles", "Radlader_Ladegeschwindigkeit", DefaultMultiplier, "Wie schnell der Radlader Material laedt.");
            BaggerladerLoadSpeed  = Bind("Group2_Vehicles", "Baggerlader_Ladegeschwindigkeit", DefaultMultiplier, "Wie schnell der Baggerlader Material laedt.");
            MobileConveyorSpeed   = Bind("Group2_Vehicles", "MobildeFoerderband_Geschwindigkeit", DefaultMultiplier, "Transportgeschwindigkeit des mobilen Foerderbands.");

            // Gruppe 3 – Simple
            Group3Multiplier = Bind("Group3_WashPlantModules", "GroupMultiplier", DefaultMultiplier,
                "Gemeinsamer Multiplikator fuer alle Waschanlagen-Module (Einfuelltrichter, Foerderbänder, Ruettler, Derocker, Waschrinnen, Trommeln, Jigs, Miner's Moss).");

            // Gruppe 3 – Advanced
            HopperCapacity       = Bind("Group3_WashPlantModules", "Einfuelltrichter_Kapazitaet", DefaultMultiplier, "Fassungsvermoegen des Einfuelltrichters.");
            ConveyorSpeed        = Bind("Group3_WashPlantModules", "Foerderband_Geschwindigkeit", DefaultMultiplier, "Transportgeschwindigkeit des Foerderbands.");
            VibratingScreenSpeed = Bind("Group3_WashPlantModules", "Ruettler_Geschwindigkeit", DefaultMultiplier, "Siebleistung des Ruettlers.");
            DerockerSpeed        = Bind("Group3_WashPlantModules", "Derocker_Geschwindigkeit", DefaultMultiplier, "Verarbeitungsgeschwindigkeit des Derockers.");
            SluiceSpeed          = Bind("Group3_WashPlantModules", "Waschrinne_Geschwindigkeit", DefaultMultiplier, "Durchsatz der Waschrinne.");
            TrommelSpeed         = Bind("Group3_WashPlantModules", "Trommel_Geschwindigkeit", DefaultMultiplier, "Drehgeschwindigkeit der Trommelwaschanlage.");
            JigSpeed             = Bind("Group3_WashPlantModules", "Jig_Geschwindigkeit", DefaultMultiplier, "Verarbeitungsgeschwindigkeit des Jigs.");
            MinersMossCapacity   = Bind("Group3_WashPlantModules", "MinersMoss_Kapazitaet", DefaultMultiplier, "Rueckhaltekapazitaet des Miner's Moss.");

            // Gruppe 4 – Simple
            Group4Multiplier = Bind("Group4_FineProcessing", "GroupMultiplier", DefaultMultiplier,
                "Gemeinsamer Multiplikator fuer alle Feinverarbeitungsgeraete (Nuggetator, Magnetitabscheider, Wave Table).");

            // Gruppe 4 – Advanced
            NuggeterSpeed          = Bind("Group4_FineProcessing", "Nuggetator_Geschwindigkeit", DefaultMultiplier, "Verarbeitungsgeschwindigkeit des Nuggetators.");
            MagnetiteSeparatorSpeed = Bind("Group4_FineProcessing", "Magnetitabscheider_Geschwindigkeit", DefaultMultiplier, "Separationsgeschwindigkeit des Magnetitabscheiders.");
            WaveTableSpeed         = Bind("Group4_FineProcessing", "WaveTable_Geschwindigkeit", DefaultMultiplier, "Ruettelgeschwindigkeit des Wave Tables.");
            WaveTableCapacity      = Bind("Group4_FineProcessing", "WaveTable_Kapazitaet", DefaultMultiplier, "Maximale Materialmenge auf dem Wave Table.");

            // Gruppe 5 – Simple
            Group5Multiplier = Bind("Group5_Trailers", "GroupMultiplier", DefaultMultiplier,
                "Gemeinsamer Multiplikator fuer alle Anhaenger (Magnetitanhaenger, Kraftstoffanhaenger).");

            // Gruppe 5 – Advanced
            MagnetiteTrailerCapacity = Bind("Group5_Trailers", "Magnetitanhaenger_Kapazitaet", DefaultMultiplier, "Ladekapazitaet des Magnetitanhaengers.");
            FuelTrailerCapacity      = Bind("Group5_Trailers", "Kraftstoffanhaenger_Kapazitaet", DefaultMultiplier, "Ladekapazitaet des Kraftstoffanhaengers.");
        }

        private ConfigEntry<float> Bind(string section, string key, float defaultValue, string description)
        {
            return _config.Bind(section, key, defaultValue,
                new ConfigDescription(description,
                    new AcceptableValueRange<float>(MinMultiplier, MaxMultiplier)));
        }

        // ---- Gruppen-Reset-API ----

        /// <summary>
        /// Setzt alle Multiplikatoren der angegebenen Gruppe auf den Standardwert (1.0) zurück.
        /// </summary>
        public void ResetGroup(int groupIndex)
        {
            switch (groupIndex)
            {
                case 1:
                    Group1Multiplier.Value     = DefaultMultiplier;
                    ShovelFillSpeed.Value      = DefaultMultiplier;
                    BucketCapacity.Value       = DefaultMultiplier;
                    PanCapacity.Value          = DefaultMultiplier;
                    HogPanCapacity.Value       = DefaultMultiplier;
                    MobileWashPlantSpeed.Value = DefaultMultiplier;
                    break;
                case 2:
                    Group2Multiplier.Value    = DefaultMultiplier;
                    MiniBaggerDigSpeed.Value  = DefaultMultiplier;
                    BaggerDigSpeed.Value      = DefaultMultiplier;
                    RadladerLoadSpeed.Value   = DefaultMultiplier;
                    BaggerladerLoadSpeed.Value = DefaultMultiplier;
                    MobileConveyorSpeed.Value = DefaultMultiplier;
                    break;
                case 3:
                    Group3Multiplier.Value       = DefaultMultiplier;
                    HopperCapacity.Value         = DefaultMultiplier;
                    ConveyorSpeed.Value          = DefaultMultiplier;
                    VibratingScreenSpeed.Value   = DefaultMultiplier;
                    DerockerSpeed.Value          = DefaultMultiplier;
                    SluiceSpeed.Value            = DefaultMultiplier;
                    TrommelSpeed.Value           = DefaultMultiplier;
                    JigSpeed.Value               = DefaultMultiplier;
                    MinersMossCapacity.Value     = DefaultMultiplier;
                    break;
                case 4:
                    Group4Multiplier.Value           = DefaultMultiplier;
                    NuggeterSpeed.Value              = DefaultMultiplier;
                    MagnetiteSeparatorSpeed.Value    = DefaultMultiplier;
                    WaveTableSpeed.Value             = DefaultMultiplier;
                    WaveTableCapacity.Value          = DefaultMultiplier;
                    break;
                case 5:
                    Group5Multiplier.Value           = DefaultMultiplier;
                    MagnetiteTrailerCapacity.Value   = DefaultMultiplier;
                    FuelTrailerCapacity.Value        = DefaultMultiplier;
                    break;
            }
            _config.Save();
        }

        /// <summary>Setzt alle Multiplikatoren aller Gruppen zurück.</summary>
        public void ResetAll()
        {
            for (int i = 1; i <= 5; i++) ResetGroup(i);
        }
    }
}
