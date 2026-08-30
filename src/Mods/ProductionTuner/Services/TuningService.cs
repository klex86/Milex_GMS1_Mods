using Milex.GMS1.Mods.ProductionTuner.Config;

namespace Milex.GMS1.Mods.ProductionTuner.Services
{
    /// <summary>
    /// Schicht 2: Domänen-Service für den Production Tuner.
    ///
    /// Berechnet den effektiven Multiplikator, der auf ein Spielobjekt angewendet wird.
    /// Im Simple Mode skaliert der Gruppen-Multiplikator die Einzelmultiplikatoren der Gruppe.
    /// Im Advanced Mode bestimmt der Einzelregler der jeweiligen Komponente direkt den Wert.
    /// Bei aktivem Kaskadenschutz wird für Folgegeräte automatisch mindestens das Eimervolumen garantiert.
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
                ? (_cfg.Group1Multiplier.Value * _cfg.Shovel_FillSpeed.Value)
                : _cfg.Shovel_FillSpeed.Value;
        }

        public float GetBucketCapacity()
        {
            // TODO: Hook into [GameClass: Bucket / BucketItem] [Field/Method: capacity or maxVolume]
            return _cfg.Group1SimpleMode.Value
                ? (_cfg.Group1Multiplier.Value * _cfg.Bucket_Capacity.Value)
                : _cfg.Bucket_Capacity.Value;
        }

        /// <summary>Kaskadenschutz: mindestens so gross wie der Eimer-Multiplikator.</summary>
        public float GetHogPanCapacity()
        {
            // TODO: Hook into [GameClass: HogPan] [Field/Method: capacity]
            float val = _cfg.Group1SimpleMode.Value
                ? (_cfg.Group1Multiplier.Value * _cfg.HogPan_Capacity.Value)
                : _cfg.HogPan_Capacity.Value;
            return _cfg.AutoScaleDependentInputs.Value
                ? System.Math.Max(val, GetBucketCapacity())
                : val;
        }

        public float GetMobileWashPlantSpeed()
        {
            // TODO: Hook into [GameClass: MobileWashPlant] [Field/Method: processingSpeed]
            return _cfg.Group1SimpleMode.Value
                ? (_cfg.Group1Multiplier.Value * _cfg.MobileWashPlant_Speed.Value)
                : _cfg.MobileWashPlant_Speed.Value;
        }

        public float GetMobileWashPlantCapacity()
        {
            // TODO: Hook into [GameClass: MobileWashPlant] [Field/Method: capacity]
            return _cfg.Group1SimpleMode.Value
                ? (_cfg.Group1Multiplier.Value * _cfg.MobileWashPlant_Capacity.Value)
                : _cfg.MobileWashPlant_Capacity.Value;
        }

        // ===========================================================
        // GRUPPE 2 – Vehicles
        // ===========================================================

        /// <summary>Gilt fuer alle Bagger (Minibagger und Grossbagger).</summary>
        public float GetExcavatorDigSpeed()
        {
            // TODO: Hook into [GameClass: ExcavatorController / MiniExcavatorController] [Field/Method: digSpeed]
            return _cfg.Group2SimpleMode.Value
                ? (_cfg.Group2Multiplier.Value * _cfg.Excavator_DigSpeed.Value)
                : _cfg.Excavator_DigSpeed.Value;
        }

        public float GetWheelLoaderLoadSpeed()
        {
            // TODO: Hook into [GameClass: WheelLoaderController] [Field/Method: loadSpeed]
            return _cfg.Group2SimpleMode.Value
                ? (_cfg.Group2Multiplier.Value * _cfg.WheelLoader_LoadSpeed.Value)
                : _cfg.WheelLoader_LoadSpeed.Value;
        }

        public float GetBackhoeLoaderLoadSpeed()
        {
            // TODO: Hook into [GameClass: BackhoeController] [Field/Method: loadSpeed]
            return _cfg.Group2SimpleMode.Value
                ? (_cfg.Group2Multiplier.Value * _cfg.BackhoeLoader_LoadSpeed.Value)
                : _cfg.BackhoeLoader_LoadSpeed.Value;
        }

        public float GetDumpTruckCapacity()
        {
            // TODO: Hook into [GameClass: DumpTruckController] [Field/Method: capacity]
            return _cfg.Group2SimpleMode.Value
                ? (_cfg.Group2Multiplier.Value * _cfg.DumpTruck_Capacity.Value)
                : _cfg.DumpTruck_Capacity.Value;
        }

        // ===========================================================
        // GRUPPE 3 – Wash Plant Modules
        // ===========================================================

        public float GetHopperCapacity()
        {
            // TODO: Hook into [GameClass: Hopper] [Field/Method: maxCapacity]
            return _cfg.Group3SimpleMode.Value
                ? (_cfg.Group3Multiplier.Value * _cfg.Hopper_Capacity.Value)
                : _cfg.Hopper_Capacity.Value;
        }

        public float GetConveyorBucketCapacity()
        {
            // TODO: Hook into [GameClass: ConveyorBelt] [Field/Method: bucketCapacity / capacity]
            return _cfg.Group3SimpleMode.Value
                ? (_cfg.Group3Multiplier.Value * _cfg.ConveyorBucket_Capacity.Value)
                : _cfg.ConveyorBucket_Capacity.Value;
        }

        /// <summary>Gilt fuer alle Waschanlagen (Derocker, Glacier Creek usw.).</summary>
        public float GetWashplantCapacity()
        {
            // TODO: Hook into [GameClass: WashPlant / Derocker / GlacierCreek] [Field/Method: capacity]
            return _cfg.Group3SimpleMode.Value
                ? (_cfg.Group3Multiplier.Value * _cfg.Washplant_Capacity.Value)
                : _cfg.Washplant_Capacity.Value;
        }

        /// <summary>Gilt fuer alle Waschanlagen (Derocker, Glacier Creek usw.).</summary>
        public float GetWashplantSpeed()
        {
            // TODO: Hook into [GameClass: WashPlant / Derocker / GlacierCreek] [Field/Method: processingSpeed]
            return _cfg.Group3SimpleMode.Value
                ? (_cfg.Group3Multiplier.Value * _cfg.Washplant_Speed.Value)
                : _cfg.Washplant_Speed.Value;
        }

        /// <summary>Gilt fuer alle Waschrinnen (Sluice Boxes).</summary>
        public float GetSluiceboxCapacity()
        {
            // TODO: Hook into [GameClass: SluiceBox] [Field/Method: capacity]
            return _cfg.Group3SimpleMode.Value
                ? (_cfg.Group3Multiplier.Value * _cfg.Sluicebox_Capacity.Value)
                : _cfg.Sluicebox_Capacity.Value;
        }

        public float GetMinersMossCapacity()
        {
            // TODO: Hook into [GameClass: MinersMoss] [Field/Method: capacity]
            return _cfg.Group3SimpleMode.Value
                ? (_cfg.Group3Multiplier.Value * _cfg.MinersMoss_Capacity.Value)
                : _cfg.MinersMoss_Capacity.Value;
        }

        // ===========================================================
        // GRUPPE 4 – Fine Processing
        // ===========================================================

        public float GetNuggetatorSpeed()
        {
            // TODO: Hook into [GameClass: Nuggetator] [Field/Method: processingSpeed]
            return _cfg.Group4SimpleMode.Value
                ? (_cfg.Group4Multiplier.Value * _cfg.Nuggetator_Speed.Value)
                : _cfg.Nuggetator_Speed.Value;
        }

        public float GetMagnetiteSeparatorSpeed()
        {
            // TODO: Hook into [GameClass: MagnetiteSeparator] [Field/Method: separationSpeed]
            return _cfg.Group4SimpleMode.Value
                ? (_cfg.Group4Multiplier.Value * _cfg.MagnetiteSeparator_Speed.Value)
                : _cfg.MagnetiteSeparator_Speed.Value;
        }

        /// <summary>Kaskadenschutz: mindestens so gross wie der Eimer-Multiplikator.</summary>
        public float GetMagnetiteSeparatorCapacity()
        {
            // TODO: Hook into [GameClass: MagnetiteSeparator] [Field/Method: inputCapacity / maxVolume]
            float val = _cfg.Group4SimpleMode.Value
                ? (_cfg.Group4Multiplier.Value * _cfg.MagnetiteSeparator_Capacity.Value)
                : _cfg.MagnetiteSeparator_Capacity.Value;
            return _cfg.AutoScaleDependentInputs.Value
                ? System.Math.Max(val, GetBucketCapacity())
                : val;
        }

        public float GetWaveTableSpeed()
        {
            // TODO: Hook into [GameClass: WaveTable] [Field/Method: vibrationSpeed]
            return _cfg.Group4SimpleMode.Value
                ? (_cfg.Group4Multiplier.Value * _cfg.WaveTable_Speed.Value)
                : _cfg.WaveTable_Speed.Value;
        }

        /// <summary>Kaskadenschutz: mindestens so gross wie der Eimer-Multiplikator.</summary>
        public float GetWaveTableCapacity()
        {
            // TODO: Hook into [GameClass: WaveTable] [Field/Method: maxCapacity]
            float val = _cfg.Group4SimpleMode.Value
                ? (_cfg.Group4Multiplier.Value * _cfg.WaveTable_Capacity.Value)
                : _cfg.WaveTable_Capacity.Value;
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
    }
}
