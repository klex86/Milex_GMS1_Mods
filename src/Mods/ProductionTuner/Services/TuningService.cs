using Milex.GMS1.Mods.ProductionTuner.Config;

namespace Milex.GMS1.Mods.ProductionTuner.Services
{
    /// <summary>
    /// Layer 2: Domain service for Production Tuner.
    ///
    /// Provides the effective multiplier applied to each game component.
    /// All configuration values, defaults, and cascade constraints are managed directly in TuningConfig.
    /// </summary>
    public class TuningService
    {
        private readonly TuningConfig _cfg;

        public TuningService(TuningConfig cfg)
        {
            _cfg = cfg;
        }

        // ===========================================================
        // GROUP 1 – Hand Tools & Mobile Wash Plants
        // ===========================================================

        public float GetShovelFillSpeed() => _cfg.Shovel_FillSpeed.Value;

        public float GetBucketCapacity() => _cfg.Bucket_Capacity.Value;

        public float GetHogPanCapacity() => _cfg.HogPan_Capacity.Value;

        public float GetMobileWashPlantSpeed() => _cfg.MobileWashPlant_Speed.Value;

        public float GetMobileWashPlantCapacity() => _cfg.MobileWashPlant_Capacity.Value;

        // ===========================================================
        // GROUP 2 – Vehicles
        // ===========================================================

        /// <summary>Applies to all excavators (mini and large excavator).</summary>
        public float GetExcavatorDigSpeed() => _cfg.Excavator_DigSpeed.Value;

        public float GetWheelLoaderLoadSpeed() => _cfg.WheelLoader_LoadSpeed.Value;

        public float GetBackhoeLoaderLoadSpeed() => _cfg.BackhoeLoader_LoadSpeed.Value;

        public float GetDumpTruckCapacity() => _cfg.DumpTruck_Capacity.Value;

        // ===========================================================
        // GROUP 3 – Wash Plant Modules
        // ===========================================================

        public float GetHopperCapacity() => _cfg.Hopper_Capacity.Value;

        public float GetConveyorBucketCapacity() => _cfg.ConveyorBucket_Capacity.Value;

        /// <summary>Applies to all wash plants (Derocker, Glacier Creek, etc.).</summary>
        public float GetWashplantCapacity() => _cfg.Washplant_Capacity.Value;

        /// <summary>Applies to all wash plants (Derocker, Glacier Creek, etc.).</summary>
        public float GetWashplantSpeed() => _cfg.Washplant_Speed.Value;

        /// <summary>Applies to all sluice boxes.</summary>
        public float GetSluiceboxCapacity() => _cfg.Sluicebox_Capacity.Value;

        public float GetMinersMossCapacity() => _cfg.MinersMoss_Capacity.Value;

        // ===========================================================
        // GROUP 4 – Fine Processing
        // ===========================================================

        public float GetNuggetatorSpeed() => _cfg.Nuggetator_Speed.Value;

        public float GetMagnetiteSeparatorSpeed() => _cfg.MagnetiteSeparator_Speed.Value;

        public float GetMagnetiteSeparatorCapacity() => _cfg.MagnetiteSeparator_Capacity.Value;

        public float GetWaveTableSpeed() => _cfg.WaveTable_Speed.Value;

        public float GetWaveTableCapacity() => _cfg.WaveTable_Capacity.Value;

        // ===========================================================
        // GROUP 5 – Trailers
        // ===========================================================

        public float GetMagnetiteTrailerCapacity() => _cfg.MagnetiteTrailer_Capacity.Value;

        public float GetFuelTrailerCapacity() => _cfg.FuelTrailer_Capacity.Value;
    }
}
