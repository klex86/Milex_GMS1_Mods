using Milex.GMS1.Mods.ProductionTuner.Config;

namespace Milex.GMS1.Mods.ProductionTuner.Services
{
    /// <summary>
    /// Schicht 2: Domänen-Service für den Production Tuner.
    ///
    /// Dieser Service berechnet den effektiven Multiplikator, der auf ein Spielobjekt
    /// angewendet werden soll. Er unterscheidet zwischen dem einfachen Modus (Simple Mode,
    /// ein Regler pro Gruppe) und dem erweiterten Modus (Advanced Mode, Einzelregler pro
    /// Komponente), und wendet den Kaskadenschutz für Eimer und Folgegeräte an.
    ///
    /// Phase 2 (Harmony-Patches):
    ///   Sobald die Spiel-DLLs (Assembly-CSharp.dll) dekompiliert sind, werden die
    ///   Harmony-Patch-Klassen in Patches/ angelegt. Der TuningService stellt dann
    ///   statische Get-Methoden bereit, die die Patches aufrufen koennen.
    ///
    ///   Die TODO-Kommentare markieren, welche Spielklassen/Methoden/Felder in Phase 2
    ///   gepatcht werden muessen.
    /// </summary>
    public class TuningService
    {
        private readonly TuningConfig _cfg;

        public TuningService(TuningConfig cfg)
        {
            _cfg = cfg;
        }

        // ===========================================================
        // GRUPPE 1 – Handwerkzeuge & Mobile Waschanlagen
        // ===========================================================

        /// <summary>Effektiver Multiplikator fuer die Schaufel-Fuellgeschwindigkeit.</summary>
        public float GetShovelFillSpeed()
        {
            // TODO: Hook into [GameClass: Shovel / ShovelBehaviour] [Field/Method: fillRate or similar]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group1Multiplier.Value, _cfg.ShovelFillSpeed.Value)
                : _cfg.Group1Multiplier.Value;
        }

        /// <summary>Effektiver Multiplikator fuer die Eimer-Kapazitaet.</summary>
        public float GetBucketCapacity()
        {
            // TODO: Hook into [GameClass: Bucket / BucketItem] [Field/Method: capacity or maxVolume]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group1Multiplier.Value, _cfg.BucketCapacity.Value)
                : _cfg.Group1Multiplier.Value;
        }

        /// <summary>Effektiver Multiplikator fuer die Pfannen-Kapazitaet.
        /// Kaskadenschutz: Wird mindestens so gross wie der Eimer-Multiplikator gesetzt,
        /// wenn AutoScaleDependentInputs aktiv ist.</summary>
        public float GetPanCapacity()
        {
            // TODO: Hook into [GameClass: GoldPan / PanningBehaviour] [Field/Method: capacity or maxCapacity]
            float value = _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group1Multiplier.Value, _cfg.PanCapacity.Value)
                : _cfg.Group1Multiplier.Value;
            return _cfg.AutoScaleDependentInputs.Value
                ? System.Math.Max(value, GetBucketCapacity())
                : value;
        }

        /// <summary>Effektiver Multiplikator fuer die Hog Pan-Kapazitaet.</summary>
        public float GetHogPanCapacity()
        {
            // TODO: Hook into [GameClass: HogPan] [Field/Method: capacity or maxCapacity]
            float value = _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group1Multiplier.Value, _cfg.HogPanCapacity.Value)
                : _cfg.Group1Multiplier.Value;
            return _cfg.AutoScaleDependentInputs.Value
                ? System.Math.Max(value, GetBucketCapacity())
                : value;
        }

        /// <summary>Effektiver Multiplikator fuer die mobile Waschanlage.</summary>
        public float GetMobileWashPlantSpeed()
        {
            // TODO: Hook into [GameClass: MobileWashPlant] [Field/Method: processingSpeed or similar]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group1Multiplier.Value, _cfg.MobileWashPlantSpeed.Value)
                : _cfg.Group1Multiplier.Value;
        }

        // ===========================================================
        // GRUPPE 2 – Baufahrzeuge & Mobiles Foerderband
        // ===========================================================

        /// <summary>Effektiver Multiplikator fuer die Minibagger-Aushubgeschwindigkeit.</summary>
        public float GetMiniBaggerDigSpeed()
        {
            // TODO: Hook into [GameClass: MiniBagger / MiniExcavatorController] [Field/Method: digSpeed or similar]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group2Multiplier.Value, _cfg.MiniBaggerDigSpeed.Value)
                : _cfg.Group2Multiplier.Value;
        }

        /// <summary>Effektiver Multiplikator fuer die Bagger-Aushubgeschwindigkeit.</summary>
        public float GetBaggerDigSpeed()
        {
            // TODO: Hook into [GameClass: Bagger / ExcavatorController] [Field/Method: digSpeed or similar]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group2Multiplier.Value, _cfg.BaggerDigSpeed.Value)
                : _cfg.Group2Multiplier.Value;
        }

        /// <summary>Effektiver Multiplikator fuer die Radlader-Ladegeschwindigkeit.</summary>
        public float GetRadladerLoadSpeed()
        {
            // TODO: Hook into [GameClass: Radlader / WheelLoaderController] [Field/Method: loadSpeed or similar]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group2Multiplier.Value, _cfg.RadladerLoadSpeed.Value)
                : _cfg.Group2Multiplier.Value;
        }

        /// <summary>Effektiver Multiplikator fuer die Baggerlader-Ladegeschwindigkeit.</summary>
        public float GetBaggerladerLoadSpeed()
        {
            // TODO: Hook into [GameClass: Baggerlader / BackhoeController] [Field/Method: loadSpeed or similar]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group2Multiplier.Value, _cfg.BaggerladerLoadSpeed.Value)
                : _cfg.Group2Multiplier.Value;
        }

        /// <summary>Effektiver Multiplikator fuer das mobile Foerderband.</summary>
        public float GetMobileConveyorSpeed()
        {
            // TODO: Hook into [GameClass: MobileConveyor / ConveyorBeltController] [Field/Method: speed or beltSpeed]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group2Multiplier.Value, _cfg.MobileConveyorSpeed.Value)
                : _cfg.Group2Multiplier.Value;
        }

        // ===========================================================
        // GRUPPE 3 – Waschanlagen-Module (Tier 3-6)
        // ===========================================================

        /// <summary>Effektiver Multiplikator fuer den Einfuelltrichter.</summary>
        public float GetHopperCapacity()
        {
            // TODO: Hook into [GameClass: Hopper] [Field/Method: maxCapacity or volume]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group3Multiplier.Value, _cfg.HopperCapacity.Value)
                : _cfg.Group3Multiplier.Value;
        }

        /// <summary>Effektiver Multiplikator fuer stationaere Foerderbänder.</summary>
        public float GetConveyorSpeed()
        {
            // TODO: Hook into [GameClass: ConveyorBelt] [Field/Method: speed or beltSpeed]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group3Multiplier.Value, _cfg.ConveyorSpeed.Value)
                : _cfg.Group3Multiplier.Value;
        }

        /// <summary>Effektiver Multiplikator fuer den Ruettler (Vibrating Screen).</summary>
        public float GetVibratingScreenSpeed()
        {
            // TODO: Hook into [GameClass: VibratingScreen] [Field/Method: processingSpeed or vibrationRate]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group3Multiplier.Value, _cfg.VibratingScreenSpeed.Value)
                : _cfg.Group3Multiplier.Value;
        }

        /// <summary>Effektiver Multiplikator fuer den Derocker.</summary>
        public float GetDerockerSpeed()
        {
            // TODO: Hook into [GameClass: Derocker] [Field/Method: processingSpeed or similar]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group3Multiplier.Value, _cfg.DerockerSpeed.Value)
                : _cfg.Group3Multiplier.Value;
        }

        /// <summary>Effektiver Multiplikator fuer die Waschrinne (Sluice Box).</summary>
        public float GetSluiceSpeed()
        {
            // TODO: Hook into [GameClass: SluiceBox] [Field/Method: throughput or flowRate]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group3Multiplier.Value, _cfg.SluiceSpeed.Value)
                : _cfg.Group3Multiplier.Value;
        }

        /// <summary>Effektiver Multiplikator fuer die Trommelwaschanlage.</summary>
        public float GetTrommelSpeed()
        {
            // TODO: Hook into [GameClass: Trommel / TrommelWasher] [Field/Method: rotationSpeed or processingRate]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group3Multiplier.Value, _cfg.TrommelSpeed.Value)
                : _cfg.Group3Multiplier.Value;
        }

        /// <summary>Effektiver Multiplikator fuer den Jig.</summary>
        public float GetJigSpeed()
        {
            // TODO: Hook into [GameClass: Jig] [Field/Method: processingSpeed or jiggingRate]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group3Multiplier.Value, _cfg.JigSpeed.Value)
                : _cfg.Group3Multiplier.Value;
        }

        /// <summary>Effektiver Multiplikator fuer Miner's Moss Kapazitaet.</summary>
        public float GetMinersMossCapacity()
        {
            // TODO: Hook into [GameClass: MinersMoss] [Field/Method: capacity or retentionCapacity]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group3Multiplier.Value, _cfg.MinersMossCapacity.Value)
                : _cfg.Group3Multiplier.Value;
        }

        // ===========================================================
        // GRUPPE 4 – Feinverarbeitung
        // ===========================================================

        /// <summary>Effektiver Multiplikator fuer den Nuggetator.</summary>
        public float GetNuggeterSpeed()
        {
            // TODO: Hook into [GameClass: Nuggetator] [Field/Method: processingSpeed or similar]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group4Multiplier.Value, _cfg.NuggeterSpeed.Value)
                : _cfg.Group4Multiplier.Value;
        }

        /// <summary>Effektiver Multiplikator fuer den Magnetitabscheider.
        /// Kaskadenschutz: Wird mindestens so gross wie der Eimer-Multiplikator gesetzt.</summary>
        public float GetMagnetiteSeparatorSpeed()
        {
            // TODO: Hook into [GameClass: MagnetiteSeparator] [Field/Method: separationSpeed or processingRate]
            float value = _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group4Multiplier.Value, _cfg.MagnetiteSeparatorSpeed.Value)
                : _cfg.Group4Multiplier.Value;
            return _cfg.AutoScaleDependentInputs.Value
                ? System.Math.Max(value, GetBucketCapacity())
                : value;
        }

        /// <summary>Effektiver Multiplikator fuer die Wave Table Geschwindigkeit.
        /// Kaskadenschutz: Kapazitaet wird mindestens so gross wie der Eimer-Multiplikator gesetzt.</summary>
        public float GetWaveTableSpeed()
        {
            // TODO: Hook into [GameClass: WaveTable / WaveTableController] [Field/Method: vibrationSpeed or processingRate]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group4Multiplier.Value, _cfg.WaveTableSpeed.Value)
                : _cfg.Group4Multiplier.Value;
        }

        /// <summary>Effektiver Multiplikator fuer die Wave Table Kapazitaet.</summary>
        public float GetWaveTableCapacity()
        {
            // TODO: Hook into [GameClass: WaveTable / WaveTableController] [Field/Method: maxCapacity or volume]
            float value = _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group4Multiplier.Value, _cfg.WaveTableCapacity.Value)
                : _cfg.Group4Multiplier.Value;
            return _cfg.AutoScaleDependentInputs.Value
                ? System.Math.Max(value, GetBucketCapacity())
                : value;
        }

        // ===========================================================
        // GRUPPE 5 – Anhaenger
        // ===========================================================

        /// <summary>Effektiver Multiplikator fuer den Magnetitanhaenger.
        /// Kaskadenschutz: Wird mindestens so gross wie der Eimer-Multiplikator gesetzt.</summary>
        public float GetMagnetiteTrailerCapacity()
        {
            // TODO: Hook into [GameClass: MagnetiteTrailer] [Field/Method: capacity or maxLoad]
            float value = _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group5Multiplier.Value, _cfg.MagnetiteTrailerCapacity.Value)
                : _cfg.Group5Multiplier.Value;
            return _cfg.AutoScaleDependentInputs.Value
                ? System.Math.Max(value, GetBucketCapacity())
                : value;
        }

        /// <summary>Effektiver Multiplikator fuer den Kraftstoffanhaenger.</summary>
        public float GetFuelTrailerCapacity()
        {
            // TODO: Hook into [GameClass: FuelTrailer] [Field/Method: capacity or maxLoad]
            return _cfg.AdvancedMode.Value
                ? Combine(_cfg.Group5Multiplier.Value, _cfg.FuelTrailerCapacity.Value)
                : _cfg.Group5Multiplier.Value;
        }

        // ===========================================================
        // Hilfsmethoden
        // ===========================================================

        /// <summary>
        /// Kombiniert Gruppen-Multiplikator und Einzel-Multiplikator.
        /// Beide werden miteinander multipliziert (1.0 x 1.0 = kein Effekt).
        /// </summary>
        private static float Combine(float groupMult, float specificMult)
        {
            return groupMult * specificMult;
        }
    }
}
