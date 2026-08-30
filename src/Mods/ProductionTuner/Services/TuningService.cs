using Milex.GMS1.Mods.ProductionTuner.Config;

namespace Milex.GMS1.Mods.ProductionTuner.Services
{
    /// <summary>
    /// Schicht 2: Domänen-Service für den Production Tuner.
    ///
    /// Liefert den effektiven Multiplikator, der auf ein Spielobjekt angewendet wird.
    /// Alle Komponenten besitzen ihren eigenen Regler und Standardwert.
    /// Bei aktivem Kaskadenschutz wird für Folgegeräte mindestens das Eimervolumen garantiert.
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

        public float GetShovelFillSpeed() => _cfg.Shovel_FillSpeed.Value;

        public float GetBucketCapacity() => _cfg.Bucket_Capacity.Value;

        /// <summary>Kaskadenschutz: mindestens so gross wie der Eimer-Multiplikator.</summary>
        public float GetHogPanCapacity()
        {
            return _cfg.AutoScaleDependentInputs.Value
                ? System.Math.Max(_cfg.HogPan_Capacity.Value, GetBucketCapacity())
                : _cfg.HogPan_Capacity.Value;
        }

        public float GetMobileWashPlantSpeed() => _cfg.MobileWashPlant_Speed.Value;

        public float GetMobileWashPlantCapacity() => _cfg.MobileWashPlant_Capacity.Value;

        // ===========================================================
        // GRUPPE 2 – Vehicles
        // ===========================================================

        /// <summary>Gilt fuer alle Bagger (Minibagger und Grossbagger).</summary>
        public float GetExcavatorDigSpeed() => _cfg.Excavator_DigSpeed.Value;

        public float GetWheelLoaderLoadSpeed() => _cfg.WheelLoader_LoadSpeed.Value;

        public float GetBackhoeLoaderLoadSpeed() => _cfg.BackhoeLoader_LoadSpeed.Value;

        public float GetDumpTruckCapacity() => _cfg.DumpTruck_Capacity.Value;

        // ===========================================================
        // GRUPPE 3 – Wash Plant Modules
        // ===========================================================

        public float GetHopperCapacity() => _cfg.Hopper_Capacity.Value;

        public float GetConveyorBucketCapacity() => _cfg.ConveyorBucket_Capacity.Value;

        /// <summary>Gilt fuer alle Waschanlagen (Derocker, Glacier Creek usw.).</summary>
        public float GetWashplantCapacity() => _cfg.Washplant_Capacity.Value;

        /// <summary>Gilt fuer alle Waschanlagen (Derocker, Glacier Creek usw.).</summary>
        public float GetWashplantSpeed() => _cfg.Washplant_Speed.Value;

        /// <summary>Gilt fuer alle Waschrinnen (Sluice Boxes).</summary>
        public float GetSluiceboxCapacity() => _cfg.Sluicebox_Capacity.Value;

        public float GetMinersMossCapacity() => _cfg.MinersMoss_Capacity.Value;

        // ===========================================================
        // GRUPPE 4 – Fine Processing
        // ===========================================================

        public float GetNuggetatorSpeed() => _cfg.Nuggetator_Speed.Value;

        public float GetMagnetiteSeparatorSpeed() => _cfg.MagnetiteSeparator_Speed.Value;

        /// <summary>Kaskadenschutz: mindestens so gross wie der Eimer-Multiplikator.</summary>
        public float GetMagnetiteSeparatorCapacity()
        {
            return _cfg.AutoScaleDependentInputs.Value
                ? System.Math.Max(_cfg.MagnetiteSeparator_Capacity.Value, GetBucketCapacity())
                : _cfg.MagnetiteSeparator_Capacity.Value;
        }

        public float GetWaveTableSpeed() => _cfg.WaveTable_Speed.Value;

        /// <summary>Kaskadenschutz: mindestens so gross wie der Eimer-Multiplikator.</summary>
        public float GetWaveTableCapacity()
        {
            return _cfg.AutoScaleDependentInputs.Value
                ? System.Math.Max(_cfg.WaveTable_Capacity.Value, GetBucketCapacity())
                : _cfg.WaveTable_Capacity.Value;
        }

        // ===========================================================
        // GRUPPE 5 – Trailers
        // ===========================================================

        /// <summary>Kaskadenschutz: mindestens so gross wie der Eimer-Multiplikator.</summary>
        public float GetMagnetiteTrailerCapacity()
        {
            return _cfg.AutoScaleDependentInputs.Value
                ? System.Math.Max(_cfg.MagnetiteTrailer_Capacity.Value, GetBucketCapacity())
                : _cfg.MagnetiteTrailer_Capacity.Value;
        }

        public float GetFuelTrailerCapacity() => _cfg.FuelTrailer_Capacity.Value;
    }
}
