using System;
using System.Collections.Generic;
using GoldDigger;
using Milex.GMS1.Core.Localization;
using Milex.GMS1.Mods.ClaimMonitor.Config;
using UnityEngine;

namespace Milex.GMS1.Mods.ClaimMonitor.Diagnostics.Models
{
    // ==========================================
    // Base Types for Components & Maintenance
    // ==========================================

    public enum MachinePartCategory
    {
        Spring,
        HydraulicHose,
        Engine,
        DriveBelt,        // Conveyor Hopper drive belt
        ConveyorBucket,   // Conveyor Belt buckets
        SluiceMat,
        SluiceGrille,
        SwitchButton,
        Other
    }

    public class MachinePartStatus
    {
        public string Name { get; set; }
        public string PrefabName { get; set; }
        public string SourceComponent { get; set; } // e.g. "Shaker", "Trommel", "Jig 1", "Hopper", "ConveyorBelt"
        public MachinePartCategory Category { get; set; }
        public bool IsInPlace { get; set; }
        public bool IsDestroyed { get; set; }
        public float Durability { get; set; } // 0.0 to 1.0
        public bool IsReinforced { get; set; }
    }

    public class JigBucketStatus
    {
        public int SlotIndex { get; set; }
        public bool IsMounted { get; set; }
        public float CurrentVolumeM3 { get; set; }
        public float FillPct { get; set; } // 0.0 to 1.0

        public bool HasSlot { get; set; }
    }

    public class FeederChainStatus
    {
        public bool HopperMounted { get; set; }
        public bool ConveyorBeltMounted { get; set; }
        public bool HopperHasPower { get; set; }
        public bool ConveyorBeltHasPower { get; set; }
        public bool HasPower { get; set; }
        public bool IsReadyToWork { get; set; }
        public float HopperFillPct { get; set; }

        // Maintenance parts specifically on Hopper (Drive Belt) and Conveyor (Buckets)
        public List<MachinePartStatus> Parts { get; set; } = new List<MachinePartStatus>();
    }

    // ==========================================
    // Specific Setup Models
    // ==========================================

    // Setup 1: Standalone Hog Pan Areas on the claim (No wear parts, only mats & water)
    public class HogPanAreaStatus
    {
        public int ClaimId { get; set; }
        public string ClaimName { get; set; }
        public int AreaIndex { get; set; } // 1 or 2
        public bool IsMounted { get; set; }

        public bool RequiresWater { get; set; }
        public bool HasWater { get; set; }
        public float DirtFillPct { get; set; }

        // Primary sluice (2 mats) + Extension sluice (2 mats) = max 4 mats
        public int TotalMats { get; set; }
        public int InstalledMats { get; set; }
        public float MaxMatFillPct { get; set; }

    }

    public enum MobilePlantType
    {
        MobileWashPlant, // Standard electric trailer
        MiniWashPlant    // DLC diesel trommel trailer
    }

    // Setup 2: Mobile Wash Plants (Trailers / DLC Mini Trommel)
    public class MobileWashPlantStatus
    {
        public int ClaimId { get; set; }
        public string ClaimName { get; set; }
        public int PlantIndex { get; set; }
        public MobilePlantType PlantType { get; set; }
        public string VariantName { get; set; }

        // Operating state filter: hose connected means active
        public bool IsHoseConnected { get; set; }
        public bool IsReadyToOperate { get; set; }

        // Water
        public bool HasWater { get; set; }

        // Power (Only MobileWashPlant)
        public bool RequiresPower { get; set; }
        public bool HasPower { get; set; }

        // Fuel (Only MiniWashPlant)
        public bool RequiresFuel { get; set; }
        public bool HasFuel { get; set; }
        public float FuelPct { get; set; } // 0.0 to 1.0

        // Intake & Bucket
        public float DirtFillPct { get; set; }
        public bool BucketMounted { get; set; }
        public float BucketFillPct { get; set; } // 0.0 to 1.0
        public float BucketCurrentVolumeM3 { get; set; }

        public List<MachinePartStatus> Parts { get; set; } = new List<MachinePartStatus>();
    }

    // Setup 3: Stationary Modular Wash Plant (T3 - T5)
    public class ModularWashPlantStatus
    {
        public int ClaimId { get; set; }
        public string ClaimName { get; set; }
        public string ShakerVariant { get; set; }
        public string TrommelVariant { get; set; }
        public bool IsReadyToOperate { get; set; }

        // Unit: Trommel (Requires power, has wear parts)
        public bool TrommelMounted { get; set; }
        public bool TrommelHasPower { get; set; }
        public bool TrommelReady { get; set; }

        // Unit: Shaker (Requires power & water, has springs/hoses/engine)
        public bool ShakerMounted { get; set; }
        public bool ShakerHasPower { get; set; }
        public bool ShakerHasWater { get; set; }
        public bool ShakerReady { get; set; }

        // Unit: Duplex Jigs / Gravel Pumps (Requires power, buckets & wear parts)
        public bool Jig1Mounted { get; set; }
        public bool Jig1HasPower { get; set; }
        public JigBucketStatus Jig1Bucket1 { get; set; } = new JigBucketStatus { SlotIndex = 1 };
        public JigBucketStatus Jig1Bucket2 { get; set; } = new JigBucketStatus { SlotIndex = 2 };

        public bool Jig2Mounted { get; set; }
        public bool Jig2HasPower { get; set; }
        public JigBucketStatus Jig2Bucket1 { get; set; } = new JigBucketStatus { SlotIndex = 1 };
        public JigBucketStatus Jig2Bucket2 { get; set; } = new JigBucketStatus { SlotIndex = 2 };

        // Sluice Box 3 (Nugget / Diamond traps)
        public bool SluiceBox3Mounted { get; set; }
        public int SluiceGratesInstalled { get; set; }
        public int SluiceGratesTotal { get; set; }


        // Internal references to exclude them from standalone scans
        public HogPan ModularHogPanRef1 { get; set; }
        public bool EndHogPan1Mounted { get; set; }
        public bool EndHogPan1HasWater { get; set; }
        public int EndHogPan1MatsInstalled { get; set; }
        public int EndHogPan1MatsTotal { get; set; }

        public HogPan ModularHogPanRef2 { get; set; }
        public bool EndHogPan2Mounted { get; set; }
        public bool EndHogPan2HasWater { get; set; }
        public int EndHogPan2MatsInstalled { get; set; }
        public int EndHogPan2MatsTotal { get; set; }

        // Main Sluice Boxes
        public int SluiceMatsInstalled { get; set; }
        public int SluiceMatsTotal { get; set; }
        public int SluiceGrillesInstalled { get; set; }
        public int SluiceGrillesTotal { get; set; }
        public float MaxMatFillPct { get; set; }

        public float PlantInputFillPct { get; set; }

        // Optional Feeding Chain (Hopper & Conveyor Belt)
        public FeederChainStatus FeedingChain { get; set; } = new FeederChainStatus();

        public float MaxCrateFillPct { get; set; }

        // Wearable maintenance parts across all mounted modules (Trommel, Shaker, Jigs)
        public List<MachinePartStatus> Parts { get; set; } = new List<MachinePartStatus>();
    }

    // Setup 4: Orange Beast (T6)
    public class OrangeBeastStatus
    {
        public int ClaimId { get; set; }
        public string ClaimName { get; set; }
        public int BeastIndex { get; set; }
        public bool IsReadyToOperate { get; set; }

        // Shaker & Utilities
        public bool HasPower { get; set; }
        public bool HasWater { get; set; }
        public bool IsPowerReady { get; set; }
        public bool IsWaterReady { get; set; }
        public bool IsReadyToWork { get; set; }

        // Sluice Runs (2 runs with 8 mats & 8 grilles each = 16)
        public int InstalledMats { get; set; }
        public int TotalMats { get; set; }
        public int InstalledGrilles { get; set; }
        public int TotalGrilles { get; set; }
        public float MaxMatFillPct { get; set; }

        public float PlantInputFillPct { get; set; }

        // Optional Feeding Chain
        public FeederChainStatus FeedingChain { get; set; } = new FeederChainStatus();

        // Wearable parts on Orange Beast
        public List<MachinePartStatus> Parts { get; set; } = new List<MachinePartStatus>();
    }

    // ==========================================
    // Vehicle & Conveyor Models
    // ==========================================

    public class VehicleTankInfo
    {
        public string Label { get; set; } = "Drive";
        public float CurrentLiters { get; set; }
        public float MaxLiters { get; set; }
        public float FuelPct => MaxLiters > 0.001f ? Mathf.Clamp01(CurrentLiters / MaxLiters) : 0f;
    }

    public class VehicleConveyorInfo
    {
        public bool HasConveyor { get; set; }
        public bool IsRunning { get; set; }
        public float SpeedMultiplier { get; set; }

        // Intake hopper fill
        public float DirtVolume { get; set; }
        public float MaxVolume { get; set; }
        public float FillPct => MaxVolume > 0.001f ? Mathf.Clamp01(DirtVolume / MaxVolume) : 0f;
    }

    public class VehicleStatus
    {
        public int InstanceId { get; set; }
        public int ClaimId { get; set; }
        public string ClaimName { get; set; }
        public string DisplayName { get; set; }
        public string TypeName { get; set; }
        public int SwitcherSlotIndex { get; set; }

        public bool IsEngineStarted { get; set; }

        // Primary fuel tank (Chassis / drive engine)
        public VehicleTankInfo PrimaryTank { get; set; } = new VehicleTankInfo { Label = "Drive" };

        // Secondary fuel tank (Optional: Frankenstein / Cordylus belt engine)
        public VehicleTankInfo SecondaryTank { get; set; }
        public bool HasDualTanks => SecondaryTank != null;

        // Conveyor subsystem (Optional: Frankenstein / Cordylus)
        public VehicleConveyorInfo Conveyor { get; set; } = new VehicleConveyorInfo();
    }



    // ==========================================
    // Diagnostics Root Container
    // ==========================================

    public class ClaimDiagnosticsDataV2
    {
        public List<HogPanAreaStatus> HogPanAreas { get; } = new List<HogPanAreaStatus>();
        public List<MobileWashPlantStatus> MobilePlants { get; } = new List<MobileWashPlantStatus>();
        public List<ModularWashPlantStatus> ModularPlants { get; } = new List<ModularWashPlantStatus>();
        public List<OrangeBeastStatus> OrangeBeasts { get; } = new List<OrangeBeastStatus>();
        public List<VehicleStatus> Vehicles { get; } = new List<VehicleStatus>();

        public List<ClaimAlert> ActiveAlerts { get; } = new List<ClaimAlert>();

        public void CopyFrom(ClaimDiagnosticsDataV2 source)
        {
            if (source == null) return;

            HogPanAreas.Clear();
            HogPanAreas.AddRange(source.HogPanAreas);

            MobilePlants.Clear();
            MobilePlants.AddRange(source.MobilePlants);

            ModularPlants.Clear();
            ModularPlants.AddRange(source.ModularPlants);

            OrangeBeasts.Clear();
            OrangeBeasts.AddRange(source.OrangeBeasts);

            Vehicles.Clear();
            Vehicles.AddRange(source.Vehicles);

            ActiveAlerts.Clear();
            ActiveAlerts.AddRange(source.ActiveAlerts);
        }

        public void Reset()
        {
            HogPanAreas.Clear();
            MobilePlants.Clear();
            ModularPlants.Clear();
            OrangeBeasts.Clear();
            ActiveAlerts.Clear();
            Vehicles.Clear();
        }

        public void CompileAlerts(MonitorConfig config)
        {
            ActiveAlerts.Clear();
            if (config == null) return;

            float wearThreshold = (config.ComponentWearWarningThreshold?.Value ?? 20.0f) / 100.0f;
            float fillThreshold = (config.MatWarningThreshold?.Value ?? 90.0f) / 100.0f;
            float fuelThreshold = (config.VehicleLowFuelThreshold?.Value ?? 20.0f) / 100.0f;
            //bool monitorButtons = config.MonitorGeneratorSwitchButtons?.Value ?? false;
            bool monitorButtons = false;

            // ==========================================
            // 1. Setup 1: Standalone HogPan Areas
            // ==========================================
            if (config.MonitorSetup1?.Value ?? true)
            {
                foreach (var hp in HogPanAreas)
                {
                    if (!hp.IsMounted) continue;

                    string hogPanName = GameLocResolver.Resolve(GameLocResolver.KeyHogPanContainer, "Hog Pan");
                    string matsName = GameLocResolver.Resolve(GameLocResolver.KeyHogPanMat, "Hog Pan Mat");
                    string header = $"{hp.ClaimName ?? $"Claim #{hp.ClaimId}"} ({hogPanName} #{hp.AreaIndex})";


                    // Check water only if this variant actually requires water
                    if (hp.RequiresWater && !hp.HasWater)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.water", "Water"),
                            Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", header, LocalizationManager.T("alert.cat.water", "Water")),
                            Description = LocalizationManager.Format("issue.machinery.no_water", "{0} has no water supply.", hogPanName)
                        });
                    }

                    // Missing mats alert
                    if (hp.TotalMats > 0 && hp.InstalledMats < hp.TotalMats)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.mats", "Mats"),
                            Title = LocalizationManager.Format("alert.wear.missing.title", "{0}: {1} Missing!", header, matsName),
                            Description = LocalizationManager.Format("issue.machinery.part_missing", "{0} is missing a required part ({1})!", header, matsName)
                        });
                    }

                    // HogPan Mats Fill Alert
                    if (hp.MaxMatFillPct >= fillThreshold)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.mats", "Mats"),
                            Title = LocalizationManager.Format("alert.mats.near_full.title", "{0}: Sluice Mats Nearly Full", header),
                            Description = LocalizationManager.Format("alert.mats.near_full.desc", "{0} mat(s) above {1:F0}% (Highest: {2:F1}%).", hp.InstalledMats, fillThreshold * 100f, hp.MaxMatFillPct * 100f)
                        });
                    }
                }
            }

            // ==========================================
            // 2. Setup 2: Mobile Plants (Trailers)
            // ==========================================
            if (config.MonitorSetup2?.Value ?? true)
            {
                foreach (var mp in MobilePlants)
                {
                    // Only monitor if hose is connected (in active operation)
                    if (!mp.IsHoseConnected) continue;

                    string plantHeader = $"{mp.ClaimName ?? $"Claim #{mp.ClaimId}"} ({mp.VariantName} #{mp.PlantIndex})";
                    string bucketDisplayName = $"{mp.VariantName} - {GameLocResolver.Resolve(GameLocResolver.KeyBucket, "Bucket")}";

                    // Water Alert (Hose connected, but no water arriving)
                    if (!mp.HasWater)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.water", "Water"),
                            Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", plantHeader, LocalizationManager.T("alert.cat.water", "Water")),
                            Description = LocalizationManager.Format("issue.machinery.no_water", "{0} has no water supply.", mp.VariantName)
                        });
                    }

                    // Electric Power Alert (MobileWashPlant only)
                    if (mp.RequiresPower && !mp.HasPower)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.power", "Power"),
                            Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", plantHeader, LocalizationManager.T("alert.cat.power", "Power")),
                            Description = LocalizationManager.Format("issue.machinery.no_power", "{0} has no electric power.", mp.VariantName)
                        });
                    }

                    // Diesel Fuel Alert (MiniWashPlant only)
                    if (mp.RequiresFuel)
                    {
                        if (!mp.HasFuel || mp.FuelPct <= 0.01f)
                        {
                            ActiveAlerts.Add(new ClaimAlert
                            {
                                Severity = AlertSeverity.Critical,
                                Category = LocalizationManager.T("alert.cat.fuel", "Fuel"),
                                Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", plantHeader, LocalizationManager.T("alert.cat.fuel", "Fuel")),
                                Description = LocalizationManager.Format("issue.miniwashplant.no_fuel", "{0} is out of fuel.", mp.VariantName)
                            });
                        }
                        else if (mp.FuelPct <= fuelThreshold)
                        {
                            ActiveAlerts.Add(new ClaimAlert
                            {
                                Severity = AlertSeverity.Warning,
                                Category = LocalizationManager.T("alert.cat.fuel", "Fuel"),
                                Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", plantHeader, LocalizationManager.T("alert.cat.fuel", "Fuel")),
                                Description = LocalizationManager.Format("alert.fuel.low.desc", "Fuel level is at {0:F1}% ({1:F1} L).", mp.FuelPct * 100f, 0f)
                            });
                        }
                    }

                    // Bucket Missing
                    if (!mp.BucketMounted)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.washplant", "Wash Plant"),
                            Title = LocalizationManager.Format("alert.wear.missing.title", "{0}: {1} Missing!", plantHeader, bucketDisplayName),
                            Description = LocalizationManager.Format("issue.machinery.part_missing", "{0} is missing a required part ({1})!", plantHeader, bucketDisplayName)
                        });
                    }
                    // Bucket Nearly Full / Overfill
                    else if (mp.BucketFillPct >= fillThreshold)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.washplant", "Wash Plant"),
                            Title = LocalizationManager.Format("alert.bucket.near_full.title", "{0}: Bucket Nearly Full", plantHeader),
                            Description = LocalizationManager.Format("issue.duplex.bucket_full", "{0} bucket is full (replace bucket).", bucketDisplayName)
                        });
                    }

                    // Wear parts evaluation
                    EvaluateMachineParts(plantHeader, mp.Parts, wearThreshold, monitorButtons);
                }
            }

            // ==========================================
            // 3. Setup 3: Modular Plant (T3 - T5)
            // ==========================================
            if (config.MonitorSetup3?.Value ?? true)
            {
                foreach (var plant in ModularPlants)
                {
                    string plantTypeName = LocalizationManager.T("setup.name.stationary", "Modular Wash Plant");
                    string plantHeader = $"{plant.ClaimName ?? $"Claim #{plant.ClaimId}"} ({plantTypeName})";

                    // Trommel checks
                    string trommelName = GameLocResolver.GetTrommelName(plant.TrommelVariant);
                    if (!plant.TrommelMounted)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.washplant", "Wash Plant"),
                            Title = LocalizationManager.Format("alert.wear.missing.title", "{0}: {1} Missing!", plantHeader, trommelName),
                            Description = LocalizationManager.Format("issue.machinery.part_missing", "{0} is missing a required part ({1})!", plantHeader, trommelName)
                        });
                    }
                    else if (!plant.TrommelHasPower)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.power", "Power"),
                            Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", plantHeader, trommelName),
                            Description = LocalizationManager.Format("issue.machinery.no_power", "{0} has no electric power.", trommelName)
                        });
                    }

                    // Shaker checks
                    string shakerName = GameLocResolver.GetShakerName(plant.ShakerVariant);
                    if (!plant.ShakerMounted)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.washplant", "Wash Plant"),
                            Title = LocalizationManager.Format("alert.wear.missing.title", "{0}: {1} Missing!", plantHeader, shakerName),
                            Description = LocalizationManager.Format("issue.machinery.part_missing", "{0} is missing a required part ({1})!", plantHeader, shakerName)
                        });
                    }
                    else
                    {
                        if (!plant.ShakerHasPower)
                        {
                            ActiveAlerts.Add(new ClaimAlert
                            {
                                Severity = AlertSeverity.Warning,
                                Category = LocalizationManager.T("alert.cat.power", "Power"),
                                Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", plantHeader, shakerName),
                                Description = LocalizationManager.Format("issue.machinery.no_power", "{0} has no electric power.", shakerName)
                            });
                        }
                        if (!plant.ShakerHasWater)
                        {
                            ActiveAlerts.Add(new ClaimAlert
                            {
                                Severity = AlertSeverity.Warning,
                                Category = LocalizationManager.T("alert.cat.water", "Water"),
                                Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", plantHeader, shakerName),
                                Description = LocalizationManager.Format("issue.machinery.no_water", "{0} has no water supply.", shakerName)
                            });
                        }
                    }

                    // Duplex Jigs / Concentrator checks
                    string jig1Name = $"{GameLocResolver.Resolve(GameLocResolver.KeyDuplexJig, "Duplex Jig")} 1";
                    string jig2Name = $"{GameLocResolver.Resolve(GameLocResolver.KeyDuplexJig, "Duplex Jig")} 2";

                    if (plant.Jig1Mounted && !plant.Jig1HasPower)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.power", "Power"),
                            Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", plantHeader, jig1Name),
                            Description = LocalizationManager.Format("issue.machinery.no_power", "{0} has no electric power.", jig1Name)
                        });
                    }
                    CheckJigBucketAlert(plantHeader, jig1Name, plant.Jig1Mounted, plant.Jig1Bucket1, 1, fillThreshold);
                    CheckJigBucketAlert(plantHeader, jig1Name, plant.Jig1Mounted, plant.Jig1Bucket2, 2, fillThreshold);

                    if (plant.Jig2Mounted && !plant.Jig2HasPower)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.power", "Power"),
                            Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", plantHeader, jig2Name),
                            Description = LocalizationManager.Format("issue.machinery.no_power", "{0} has no electric power.", jig2Name)
                        });
                    }
                    CheckJigBucketAlert(plantHeader, jig2Name, plant.Jig2Mounted, plant.Jig2Bucket1, 1, fillThreshold);
                    CheckJigBucketAlert(plantHeader, jig2Name, plant.Jig2Mounted, plant.Jig2Bucket2, 2, fillThreshold);

                    // Sluice Box 3 Grates Missing
                    string grateName = GameLocResolver.Resolve(GameLocResolver.KeySluiceGrate, "Sluicebox Grate");
                    if (plant.SluiceBox3Mounted && plant.SluiceGratesTotal > 0 && plant.SluiceGratesInstalled < plant.SluiceGratesTotal)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.mats", "Mats"),
                            Title = LocalizationManager.Format("alert.wear.missing.title", "{0}: {1} Missing!", plantHeader, grateName),
                            Description = LocalizationManager.Format("alert.crates.missing.desc", "Only {0} of {1} nugget/diamond traps installed.", plant.SluiceGratesInstalled, plant.SluiceGratesTotal)
                        });
                    }

                    // Sluice Box 3 Crates Fill Alert
                    if (plant.SluiceBox3Mounted && plant.MaxCrateFillPct >= fillThreshold)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.mats", "Mats"),
                            Title = LocalizationManager.Format("alert.crates.near_full.title", "{0}: Crates Nearly Full", plantHeader),
                            Description = LocalizationManager.Format("alert.crates.near_full.desc", "Sluice crates reached {0:F0}% capacity.", plant.MaxCrateFillPct * 100f)
                        });
                    }

                    // Sluice End HogPans checks
                    string endHogPanName = GameLocResolver.Resolve(GameLocResolver.KeyHogPanContainer, "Hog Pan");
                    if (plant.EndHogPan1Mounted && !plant.EndHogPan1HasWater)
                    {
                        string targetName = $"{endHogPanName} 1";
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.water", "Water"),
                            Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", plantHeader, targetName),
                            Description = LocalizationManager.Format("issue.machinery.no_water", "{0} has no water supply.", targetName)
                        });
                    }
                    if (plant.EndHogPan2Mounted && !plant.EndHogPan2HasWater)
                    {
                        string targetName = $"{endHogPanName} 2";
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.water", "Water"),
                            Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", plantHeader, targetName),
                            Description = LocalizationManager.Format("issue.machinery.no_water", "{0} has no water supply.", targetName)
                        });
                    }

                    // Sluice Mats & Grilles Missing
                    string mainMatName = GameLocResolver.Resolve(GameLocResolver.KeyMinersMoss, "Miner's Moss");
                    string grilleName = GameLocResolver.Resolve(GameLocResolver.KeyMinersGrille, "Miner's Grille");

                    if (plant.SluiceMatsTotal > 0 && plant.SluiceMatsInstalled < plant.SluiceMatsTotal)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.mats", "Mats"),
                            Title = LocalizationManager.Format("alert.wear.missing.title", "{0}: {1} Missing!", plantHeader, mainMatName),
                            Description = LocalizationManager.Format("alert.mats.missing.desc", "Only {0} of {1} main mats installed.", plant.SluiceMatsInstalled, plant.SluiceMatsTotal)
                        });
                    }
                    if (plant.SluiceGrillesTotal > 0 && plant.SluiceGrillesInstalled < plant.SluiceGrillesTotal)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.mats", "Mats"),
                            Title = LocalizationManager.Format("alert.wear.missing.title", "{0}: {1} Missing!", plantHeader, grilleName),
                            Description = LocalizationManager.Format("alert.grilles.missing.desc", "Only {0} of {1} grilles installed.", plant.SluiceGrillesInstalled, plant.SluiceGrillesTotal)
                        });
                    }

                    // Sluice Mats Fill Alert (covers Main Mats and End HogPan Mats)
                    if (plant.MaxMatFillPct >= fillThreshold)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.mats", "Mats"),
                            Title = LocalizationManager.Format("alert.mats.near_full.title", "{0}: Sluice Mats Nearly Full", plantHeader),
                            Description = LocalizationManager.Format("alert.mats.near_full.desc", "{0} mat(s) above {1:F0}% (Highest: {2:F1}%).", plant.SluiceMatsInstalled, fillThreshold * 100f, plant.MaxMatFillPct * 100f)
                        });
                    }

                    // Wear parts (Trommel, Shaker, Jigs)
                    EvaluateMachineParts(plantHeader, plant.Parts, wearThreshold, monitorButtons);

                    // Optional Feeding Chain
                    if (config.Setup3IncludeFeedingChain?.Value ?? false)
                    {
                        EvaluateFeedingChain(plantHeader, plant.FeedingChain, wearThreshold);
                    }
                }
            }

            // ==========================================
            // 4. Setup 4: Orange Beast (T6)
            // ==========================================
            if (config.MonitorSetup4?.Value ?? true)
            {
                foreach (var beast in OrangeBeasts)
                {
                    string beastTypeName = LocalizationManager.T("setup.name.orange_beast", "Orange Beast");
                    string beastHeader = $"{beast.ClaimName ?? $"Claim #{beast.ClaimId}"} ({beastTypeName} #{beast.BeastIndex})";

                    if (!beast.HasPower)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.power", "Power"),
                            Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", beastHeader, beastTypeName),
                            Description = LocalizationManager.Format("issue.machinery.no_power", "{0} has no electric power.", beastTypeName)
                        });
                    }
                    if (!beast.HasWater)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.water", "Water"),
                            Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", beastHeader, beastTypeName),
                            Description = LocalizationManager.Format("issue.machinery.no_water", "{0} has no water supply.", beastTypeName)
                        });
                    }

                    // Mats & Grilles
                    string beastMatName = GameLocResolver.Resolve(GameLocResolver.KeyMinersMoss, "Miner's Moss");
                    string beastGrilleName = GameLocResolver.Resolve(GameLocResolver.KeyMinersGrille, "Miner's Grille");

                    if (beast.TotalMats > 0 && beast.InstalledMats < beast.TotalMats)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.mats", "Mats"),
                            Title = LocalizationManager.Format("alert.wear.missing.title", "{0}: {1} Missing!", beastHeader, beastMatName),
                            Description = LocalizationManager.Format("alert.mats.missing.desc", "Only {0} of {1} main mats installed.", beast.InstalledMats, beast.TotalMats)
                        });
                    }
                    if (beast.TotalGrilles > 0 && beast.InstalledGrilles < beast.TotalGrilles)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.mats", "Mats"),
                            Title = LocalizationManager.Format("alert.wear.missing.title", "{0}: {1} Missing!", beastHeader, beastGrilleName),
                            Description = LocalizationManager.Format("alert.grilles.missing.desc", "Only {0} of {1} grilles installed.", beast.InstalledGrilles, beast.TotalGrilles)
                        });
                    }

                    // Beast Mats Fill Alert
                    if (beast.MaxMatFillPct >= fillThreshold)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.mats", "Mats"),
                            Title = LocalizationManager.Format("alert.mats.near_full.title", "{0}: Sluice Mats Nearly Full", beastHeader),
                            Description = LocalizationManager.Format("alert.mats.near_full.desc", "{0} mat(s) above {1:F0}% (Highest: {2:F1}%).", beast.InstalledMats, fillThreshold * 100f, beast.MaxMatFillPct * 100f)
                        });
                    }

                    // Wear parts
                    EvaluateMachineParts(beastHeader, beast.Parts, wearThreshold, monitorButtons);

                    // Optional Feeding Chain
                    if (config.Setup4IncludeFeedingChain?.Value ?? false)
                    {
                        EvaluateFeedingChain(beastHeader, beast.FeedingChain, wearThreshold);
                    }
                }
            }

            // ==========================================
            // 5. Vehicles (Primary Fuel, Secondary Fuel & Conveyor)
            // ==========================================
            if (config.ShowFuelInVehicleSwitcher?.Value ?? true)
            {
                foreach (var v in Vehicles)
                {
                    if (v == null) continue;

                    string vehicleHeader = $"{v.ClaimName ?? $"Claim #{v.ClaimId}"} ({v.DisplayName})";
                    string fuelCat = LocalizationManager.T("alert.cat.fuel", "Fuel");

                    // 1. Primary Tank (Fahrwerk)
                    if (v.PrimaryTank.MaxLiters > 0.001f && v.PrimaryTank.FuelPct <= fuelThreshold)
                    {
                        bool isCritical = v.PrimaryTank.FuelPct <= (fuelThreshold * 0.4f);
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = isCritical ? AlertSeverity.Critical : AlertSeverity.Warning,
                            Category = fuelCat,
                            SourceId = v.InstanceId,
                            Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", vehicleHeader, fuelCat),
                            Description = LocalizationManager.Format("alert.fuel.low.desc", "Fuel level is at {0:F1}% ({1:F1} L).", v.PrimaryTank.FuelPct * 100f, v.PrimaryTank.CurrentLiters)
                        });
                    }

                    // 2. Secondary Tank (Förderband-Motor bei Frankenstein & Cordylus)
                    if (v.HasDualTanks && v.SecondaryTank.MaxLiters > 0.001f && v.SecondaryTank.FuelPct <= fuelThreshold)
                    {
                        bool isCritical = v.SecondaryTank.FuelPct <= (fuelThreshold * 0.4f);
                        string secHeader = $"{vehicleHeader} [Conveyor]";
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = isCritical ? AlertSeverity.Critical : AlertSeverity.Warning,
                            Category = fuelCat,
                            SourceId = v.InstanceId,
                            Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", secHeader, fuelCat),
                            Description = LocalizationManager.Format("alert.fuel.low.desc", "Fuel level is at {0:F1}% ({1:F1} L).", v.SecondaryTank.FuelPct * 100f, v.SecondaryTank.CurrentLiters)
                        });
                    }

                    // 3. Conveyor Trichter fast voll (Frankenstein / Cordylus)
                    if (v.Conveyor.HasConveyor && v.Conveyor.MaxVolume > 0.001f && v.Conveyor.FillPct >= 0.90f)
                    {
                        string hopperCat = LocalizationManager.T("alert.cat.feeding_chain", "Feeding Chain");
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = hopperCat,
                            SourceId = v.InstanceId,
                            Title = LocalizationManager.Format("alert.crates.near_full.title", "{0}: Hopper Nearly Full", vehicleHeader),
                            Description = LocalizationManager.Format("alert.crates.near_full.desc", "Intake hopper reached {0:F0}% capacity ({1:F1} m³).", v.Conveyor.FillPct * 100f, v.Conveyor.DirtVolume)
                        });
                    }
                }
            }

            ActiveAlerts.Sort((a, b) => b.Severity.CompareTo(a.Severity));
        }

        // ==========================================
        // Helper Methods for Alert Evaluation
        // ==========================================

        private void CheckJigBucketAlert(string header, string jigName, bool isMounted, JigBucketStatus bucket, int slotIndex, float fillThreshold)
        {
            if (!isMounted || bucket == null || !bucket.HasSlot) return;

            string bucketItemName = GameLocResolver.Resolve(GameLocResolver.KeyBucket, "Bucket");
            string bucketDisplayName = $"{jigName} - {bucketItemName} {slotIndex}";

            if (!bucket.IsMounted)
            {
                ActiveAlerts.Add(new ClaimAlert
                {
                    Severity = AlertSeverity.Warning,
                    Category = LocalizationManager.T("alert.cat.washplant", "Wash Plant"),
                    Title = LocalizationManager.Format("alert.wear.missing.title", "{0}: {1} Missing!", header, bucketDisplayName),
                    Description = LocalizationManager.Format("issue.machinery.part_missing", "{0} is missing a required part ({1})!", header, bucketDisplayName)
                });
            }
            else if (bucket.FillPct >= fillThreshold)
            {
                ActiveAlerts.Add(new ClaimAlert
                {
                    Severity = AlertSeverity.Warning,
                    Category = LocalizationManager.T("alert.cat.washplant", "Wash Plant"),
                    Title = LocalizationManager.Format("alert.bucket.near_full.title", "{0}: Bucket Nearly Full", header),
                    Description = LocalizationManager.Format("issue.duplex.bucket_full", "{0} bucket is full (replace bucket).", bucketDisplayName)
                });
            }
        }

        private void EvaluateMachineParts(string header, List<MachinePartStatus> parts, float wearThreshold, bool monitorButtons)
        {
            if (parts == null) return;

            foreach (var part in parts)
            {
                if (part == null) continue;
                if (part.Category == MachinePartCategory.SwitchButton && !monitorButtons) continue;

                string resolvedPartName = GameLocResolver.Resolve(part.Name, part.Name);
                string source = !string.IsNullOrEmpty(part.SourceComponent) ? $"[{part.SourceComponent}] " : string.Empty;

                if (!part.IsInPlace || part.IsDestroyed)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Critical,
                        Category = LocalizationManager.T("alert.cat.maintenance", "Maintenance"),
                        Title = LocalizationManager.Format("alert.wear.broken.title", "{0}: {1} Broken!", header, $"{source}{resolvedPartName}"),
                        Description = LocalizationManager.Format("alert.wear.broken.desc", "{0} {1} is destroyed and must be replaced immediately.", header, $"{source}{resolvedPartName}")
                    });
                }
                else if (part.Durability <= wearThreshold)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = LocalizationManager.T("alert.cat.maintenance", "Maintenance"),
                        Title = LocalizationManager.Format("alert.wear.high.title", "{0}: {1} Wear High", header, $"{source}{resolvedPartName}"),
                        Description = LocalizationManager.Format("alert.wear.high.desc", "{0} {1} durability low ({2:F0}% remaining). Prepare replacement.", header, $"{source}{resolvedPartName}", part.Durability * 100f)
                    });
                }
            }
        }

        private void EvaluateFeedingChain(string header, FeederChainStatus chain, float wearThreshold)
        {
            if (chain == null) return;

            string hopperName = GameLocResolver.Resolve(GameLocResolver.KeyConveyorHopper, "Hopper");
            string elevatorName = GameLocResolver.Resolve(GameLocResolver.KeyConveyorBelt, "Conveyor Belt");

            // Hopper Alert
            if (chain.HopperMounted && !chain.HopperHasPower)
            {
                ActiveAlerts.Add(new ClaimAlert
                {
                    Severity = AlertSeverity.Warning,
                    Category = LocalizationManager.T("alert.cat.feeding_chain", "Feeding Chain"),
                    Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", header, hopperName),
                    Description = LocalizationManager.Format("issue.machinery.no_power", "{0} has no electric power.", hopperName)
                });
            }

            // Elevator Alert
            if (chain.ConveyorBeltMounted && !chain.ConveyorBeltHasPower)
            {
                ActiveAlerts.Add(new ClaimAlert
                {
                    Severity = AlertSeverity.Warning,
                    Category = LocalizationManager.T("alert.cat.feeding_chain", "Feeding Chain"),
                    Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", header, elevatorName),
                    Description = LocalizationManager.Format("issue.machinery.no_power", "{0} has no electric power.", elevatorName)
                });
            }

            if (chain.Parts != null)
            {
                foreach (var part in chain.Parts)
                {
                    if (part == null) continue;
                    string resolvedPartName = GameLocResolver.Resolve(part.Name, part.Name);
                    string source = !string.IsNullOrEmpty(part.SourceComponent) ? $"[{part.SourceComponent}] " : string.Empty;

                    if (!part.IsInPlace || part.IsDestroyed)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Critical,
                            Category = LocalizationManager.T("alert.cat.feeding_chain", "Feeding Chain"),
                            Title = LocalizationManager.Format("alert.wear.broken.title", "{0}: {1} Broken!", header, $"{source}{resolvedPartName}"),
                            Description = LocalizationManager.Format("alert.wear.broken.desc", "{0} {1} is destroyed and must be replaced immediately.", header, $"{source}{resolvedPartName}")
                        });
                    }
                    else if (part.Durability <= wearThreshold)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = LocalizationManager.T("alert.cat.feeding_chain", "Feeding Chain"),
                            Title = LocalizationManager.Format("alert.wear.high.title", "{0}: {1} Wear High", header, $"{source}{resolvedPartName}"),
                            Description = LocalizationManager.Format("alert.wear.high.desc", "{0} {1} durability low ({2:F0}% remaining). Prepare replacement.", header, $"{source}{resolvedPartName}", part.Durability * 100f)
                        });
                    }
                }
            }
        }
    }
}