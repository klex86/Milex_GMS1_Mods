using Milex.GMS1.Mods.ProductionTuner.Config;

namespace Milex.GMS1.Mods.ProductionTuner.Services
{
    /// <summary>
    /// Schicht 2: Domänen-Service für den Production Tuner.
    ///
    /// Berechnet den effektiven Multiplikator, der auf ein Spielobjekt angewendet wird.
    /// Unterscheidet per Gruppe zwischen Simple Mode (Gruppen-Regler) und Advanced Mode
    /// (Einzelregler pro Komponente), und wendet den Kaskadenschutz für Eimer und
    /// Folgegeraete an.
    ///
    /// Phase 2: Nach Dekompilierung der Spiel-DLLs werden die Harmony-Patch-Klassen in
    /// Patches/ angelegt. Bis dahin markieren die TODO-Kommentare, welche Spielklassen
    /// und Methoden/Felder gepatcht werden muessen.
    /// </summary>
    public class TuningService
    {
        private readonly TuningConfig _cfg;

        public TuningService(TuningConfig cfg)
        {
            _cfg = cfg;
        }

        // ===========================================================
        // GRUPPE 1 – Hand Tools & Mobile Wash Plants
        // ===========================================================

        public float GetShovelFillSpeed()
        {
            // TODO: Hook into [GameClass: Shovel] [Field/Method: fillRate]
            return _cfg.Group1SimpleMode.Value
                ? _cfg.Group1Multiplier.Value
                : Combine(_cfg.Group1Multiplier.Value, _cfg.Shovel_FillSpeed.Value);
        }

        public float GetBucketCapacity()
        {
            // TODO: Hook into [GameClass: Bucket / BucketItem] [Field/Method: capacity or maxVolume]
            return _cfg.Group1SimpleMode.Value
                ? _cfg.Group1Multiplier.Value
                : Combine(_cfg.Group1Multiplier.Value, _cfg.Bucket_Capacity.Value);
        }

        /// <summary>Kaskadenschutz: mindestens so gross wie der Eimer-Multiplikator.</summary>
        public float GetPanCapacity()
        {
            // TODO: Hook into [GameClass: GoldPan] [Field/Method: capacity]
            float val = _cfg.Group1SimpleMode.Value
                ? _cfg.Group1Multiplier.Value
                : Combine(_cfg.Group1Multiplier.Value, _cfg.Pan_Capacity.Value);
            return _cfg.AutoScaleDependentInputs.Value
                ? System.Math.Max(val, GetBucketCapacity())
                : val;
        }

        /// <summary>Kaskadenschutz: mindestens so gross wie der Eimer-Multiplikator.</summary>
        public float GetHogPanCapacity()
        {
            // TODO: Hook into [GameClass: HogPan] [Field/Method: capacity]
            float val = _cfg.Group1SimpleMode.Value
                ? _cfg.Group1Multiplier.Value
                : Combine(_cfg.Group1Multiplier.Value, _cfg.HogPan_Capacity.Value);
            return _cfg.AutoScaleDependentInputs.Value
                ? System.Math.Max(val, GetBucketCapacity())
                : val;
        }

        public float GetMobileWashPlantSpeed()
        {
            // TODO: Hook into [GameClass: MobileWashPlant] [Field/Method: processingSpeed]
            return _cfg.Group1SimpleMode.Value
                ? _cfg.Group1Multiplier.Value
                : Combine(_cfg.Group1Multiplier.Value, _cfg.MobileWashPlant_Speed.Value);
        }

        // ===========================================================
        // GRUPPE 2 – Vehicles & Mobile Conveyor
        // ===========================================================

        public float GetMiniBaggerDigSpeed()
        {
            // TODO: Hook into [GameClass: MiniExcavatorController] [Field/Method: digSpeed]
            return _cfg.Group2SimpleMode.Value
                ? _cfg.Group2Multiplier.Value
                : Combine(_cfg.Group2Multiplier.Value, _cfg.MiniBagger_DigSpeed.Value);
        }

        public float GetBaggerDigSpeed()
        {
            // TODO: Hook into [GameClass: ExcavatorController] [Field/Method: digSpeed]
            return _cfg.Group2SimpleMode.Value
                ? _cfg.Group2Multiplier.Value
                : Combine(_cfg.Group2Multiplier.Value, _cfg.Bagger_DigSpeed.Value);
        }

        public float GetRadladerLoadSpeed()
        {
            // TODO: Hook into [GameClass: WheelLoaderController] [Field/Method: loadSpeed]
            return _cfg.Group2SimpleMode.Value
                ? _cfg.Group2Multiplier.Value
                : Combine(_cfg.Group2Multiplier.Value, _cfg.Radlader_LoadSpeed.Value);
        }

        public float GetBaggerladerLoadSpeed()
        {
            // TODO: Hook into [GameClass: BackhoeController] [Field/Method: loadSpeed]
            return _cfg.Group2SimpleMode.Value
                ? _cfg.Group2Multiplier.Value
                : Combine(_cfg.Group2Multiplier.Value, _cfg.Baggerlader_LoadSpeed.Value);
        }

        public float GetMobileConveyorSpeed()
        {
            // TODO: Hook into [GameClass: MobileConveyor] [Field/Method: speed or beltSpeed]
            return _cfg.Group2SimpleMode.Value
                ? _cfg.Group2Multiplier.Value
                : Combine(_cfg.Group2Multiplier.Value, _cfg.MobileConveyor_Speed.Value);
        }

        // ===========================================================
        // GRUPPE 3 – Wash Plant Modules (Tier 3-6)
        // ===========================================================

        public float GetHopperCapacity()
        {
            // TODO: Hook into [GameClass: Hopper] [Field/Method: maxCapacity]
            return _cfg.Group3SimpleMode.Value
                ? _cfg.Group3Multiplier.Value
                : Combine(_cfg.Group3Multiplier.Value, _cfg.Hopper_Capacity.Value);
        }

        public float GetConveyorSpeed()
        {
            // TODO: Hook into [GameClass: ConveyorBelt] [Field/Method: speed]
            return _cfg.Group3SimpleMode.Value
                ? _cfg.Group3Multiplier.Value
                : Combine(_cfg.Group3Multiplier.Value, _cfg.Conveyor_Speed.Value);
        }

        public float GetVibratingScreenSpeed()
        {
            // TODO: Hook into [GameClass: VibratingScreen] [Field/Method: processingSpeed]
            return _cfg.Group3SimpleMode.Value
                ? _cfg.Group3Multiplier.Value
                : Combine(_cfg.Group3Multiplier.Value, _cfg.VibratingScreen_Speed.Value);
        }

        public float GetDerockerSpeed()
        {
            // TODO: Hook into [GameClass: Derocker] [Field/Method: processingSpeed]
            return _cfg.Group3SimpleMode.Value
                ? _cfg.Group3Multiplier.Value
                : Combine(_cfg.Group3Multiplier.Value, _cfg.Derocker_Speed.Value);
        }

        public float GetSluiceSpeed()
        {
            // TODO: Hook into [GameClass: SluiceBox] [Field/Method: throughput]
            return _cfg.Group3SimpleMode.Value
                ? _cfg.Group3Multiplier.Value
                : Combine(_cfg.Group3Multiplier.Value, _cfg.Sluice_Speed.Value);
        }

        public float GetTrommelSpeed()
        {
            // TODO: Hook into [GameClass: TrommelWasher] [Field/Method: rotationSpeed]
            return _cfg.Group3SimpleMode.Value
                ? _cfg.Group3Multiplier.Value
                : Combine(_cfg.Group3Multiplier.Value, _cfg.Trommel_Speed.Value);
        }

        public float GetJigSpeed()
        {
            // TODO: Hook into [GameClass: Jig] [Field/Method: processingSpeed]
            return _cfg.Group3SimpleMode.Value
                ? _cfg.Group3Multiplier.Value
                : Combine(_cfg.Group3Multiplier.Value, _cfg.Jig_Speed.Value);
        }

        public float GetMinersMossCapacity()
        {
            // TODO: Hook into [GameClass: MinersMoss] [Field/Method: capacity]
            return _cfg.Group3SimpleMode.Value
                ? _cfg.Group3Multiplier.Value
                : Combine(_cfg.Group3Multiplier.Value, _cfg.MinersMoss_Capacity.Value);
        }

        // ===========================================================
        // GRUPPE 4 – Fine Processing
        // ===========================================================

        public float GetNuggetatorSpeed()
        {
            // TODO: Hook into [GameClass: Nuggetator] [Field/Method: processingSpeed]
            return _cfg.Group4SimpleMode.Value
                ? _cfg.Group4Multiplier.Value
                : Combine(_cfg.Group4Multiplier.Value, _cfg.Nuggetator_Speed.Value);
        }

        /// <summary>Kaskadenschutz: mindestens so gross wie der Eimer-Multiplikator.</summary>
        public float GetMagnetiteSeparatorSpeed()
        {
            // TODO: Hook into [GameClass: MagnetiteSeparator] [Field/Method: separationSpeed]
            float val = _cfg.Group4SimpleMode.Value
                ? _cfg.Group4Multiplier.Value
                : Combine(_cfg.Group4Multiplier.Value, _cfg.MagnetiteSeparator_Speed.Value);
            return _cfg.AutoScaleDependentInputs.Value
                ? System.Math.Max(val, GetBucketCapacity())
                : val;
        }

        public float GetWaveTableSpeed()
        {
            // TODO: Hook into [GameClass: WaveTable] [Field/Method: vibrationSpeed]
            return _cfg.Group4SimpleMode.Value
                ? _cfg.Group4Multiplier.Value
                : Combine(_cfg.Group4Multiplier.Value, _cfg.WaveTable_Speed.Value);
        }

        /// <summary>Kaskadenschutz: mindestens so gross wie der Eimer-Multiplikator.</summary>
        public float GetWaveTableCapacity()
        {
            // TODO: Hook into [GameClass: WaveTable] [Field/Method: maxCapacity]
            float val = _cfg.Group4SimpleMode.Value
                ? _cfg.Group4Multiplier.Value
                : Combine(_cfg.Group4Multiplier.Value, _cfg.WaveTable_Capacity.Value);
            return _cfg.AutoScaleDependentInputs.Value
                ? System.Math.Max(val, GetBucketCapacity())
                : val;
        }

        // ===========================================================
        // GRUPPE 5 – Trailers (kein Gruppen-Multiplikator)
        // ===========================================================

        /// <summary>Kaskadenschutz: mindestens so gross wie der Eimer-Multiplikator.</summary>
        public float GetMagnetiteTrailerCapacity()
        {
            // TODO: Hook into [GameClass: MagnetiteTrailer] [Field/Method: capacity]
            float val = _cfg.MagnetiteTrailer_Capacity.Value;
            return _cfg.AutoScaleDependentInputs.Value
                ? System.Math.Max(val, GetBucketCapacity())
                : val;
        }

        public float GetFuelTrailerCapacity()
        {
            // TODO: Hook into [GameClass: FuelTrailer] [Field/Method: capacity]
            return _cfg.FuelTrailer_Capacity.Value;
        }

        // ===========================================================
        // Hilfsmethoden
        // ===========================================================

        /// <summary>
        /// Im Advanced Mode: Gruppen- und Einzel-Multiplikator werden multipliziert.
        /// Im Simple Mode wird nur der Gruppen-Multiplikator verwendet (diese Methode
        /// wird dann nicht aufgerufen).
        /// </summary>
        private static float Combine(float groupMult, float specificMult)
        {
            return groupMult * specificMult;
        }
    }
}
