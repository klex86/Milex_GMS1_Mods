using System;
using System.Collections.Generic;
using GoldDigger;
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
    }

    public class FeederChainStatus
    {
        public bool HopperMounted { get; set; }
        public bool ConveyorBeltMounted { get; set; }
        public bool HopperHasPower { get; set; }
        public bool ConveyorBeltHasPower { get; set; }
        public bool HasPower { get; set; }
        public bool IsReadyToWork { get; set; }

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
    }

    // Setup 2: Mobile Wash Plants (Trailers / DLC Mini Trommel)
    public class MobileWashPlantStatus
    {
        public int ClaimId { get; set; }
        public string ClaimName { get; set; }
        public string VariantName { get; set; }

        public bool HasFuel { get; set; }
        public float FuelPct { get; set; }
        public bool HasPower { get; set; }
        public bool HasWater { get; set; }

        public int TotalMats { get; set; }
        public int InstalledMats { get; set; }

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
        public JigBucketStatus Jig1Bucket { get; set; } = new JigBucketStatus { SlotIndex = 1 };

        public bool Jig2Mounted { get; set; }
        public bool Jig2HasPower { get; set; }
        public JigBucketStatus Jig2Bucket { get; set; } = new JigBucketStatus { SlotIndex = 2 };

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

        // Optional Feeding Chain (Hopper & Conveyor Belt)
        public FeederChainStatus FeedingChain { get; set; } = new FeederChainStatus();

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

        // Optional Feeding Chain
        public FeederChainStatus FeedingChain { get; set; } = new FeederChainStatus();

        // Wearable parts on Orange Beast
        public List<MachinePartStatus> Parts { get; set; } = new List<MachinePartStatus>();
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

        public List<ClaimAlert> ActiveAlerts { get; } = new List<ClaimAlert>();

        public void Reset()
        {
            HogPanAreas.Clear();
            MobilePlants.Clear();
            ModularPlants.Clear();
            OrangeBeasts.Clear();
            ActiveAlerts.Clear();
        }

        public void CompileAlerts(MonitorConfig config)
        {
            ActiveAlerts.Clear();
            if (config == null) return;

            float wearThreshold = (config.ComponentWearWarningThreshold?.Value ?? 20.0f) / 100.0f;
            float matThreshold = (config.MatWarningThreshold?.Value ?? 90.0f) / 100.0f;
            bool monitorButtons = config.MonitorGeneratorSwitchButtons?.Value ?? false;

            // ==========================================
            // 1. Setup 1: Standalone HogPan Areas
            // ==========================================
            if (config.MonitorSetup1?.Value ?? true)
            {
                foreach (var hp in HogPanAreas)
                {
                    if (!hp.IsMounted) continue;
                    string header = $"{hp.ClaimName ?? $"Claim #{hp.ClaimId}"} (HogPan #{hp.AreaIndex})";

                    // Check water only if this variant actually requires water
                    if (hp.RequiresWater && !hp.HasWater)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Water",
                            Title = $"{header}: No Water",
                            Description = "HogPan water supply is turned off or disconnected."
                        });
                    }

                    if (hp.TotalMats > 0 && hp.InstalledMats < hp.TotalMats)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Mats",
                            Title = $"{header}: Mats Missing",
                            Description = $"Only {hp.InstalledMats} of {hp.TotalMats} mats installed."
                        });
                    }
                }
            }

            // ==========================================
            // 2. Setup 2: Mobile Plants (Trailers)
            // ==========================================
            // (Wird aktiviert, sobald der Trailer-Scanner implementiert ist)

            // ==========================================
            // 3. Setup 3: Modular Plant (T3 - T5)
            // ==========================================
            if (config.MonitorSetup3?.Value ?? true)
            {
                foreach (var plant in ModularPlants)
                {
                    string plantHeader = $"{plant.ClaimName ?? $"Claim #{plant.ClaimId}"} ({plant.ShakerVariant ?? "Modular Plant"})";

                    // Trommel checks
                    if (!plant.TrommelMounted)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Setup",
                            Title = $"{plantHeader}: Trommel Missing",
                            Description = "No trommel mounted in modular plant."
                        });
                    }
                    else if (!plant.TrommelHasPower)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Power",
                            Title = $"{plantHeader}: Trommel No Power",
                            Description = "Trommel has no active power supply."
                        });
                    }

                    // Shaker checks
                    if (!plant.ShakerMounted)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Setup",
                            Title = $"{plantHeader}: Shaker Missing",
                            Description = "No shaker unit mounted."
                        });
                    }
                    else
                    {
                        if (!plant.ShakerHasPower)
                        {
                            ActiveAlerts.Add(new ClaimAlert
                            {
                                Severity = AlertSeverity.Warning,
                                Category = "Power",
                                Title = $"{plantHeader}: Shaker No Power",
                                Description = "Shaker has no power supply."
                            });
                        }
                        if (!plant.ShakerHasWater)
                        {
                            ActiveAlerts.Add(new ClaimAlert
                            {
                                Severity = AlertSeverity.Warning,
                                Category = "Water",
                                Title = $"{plantHeader}: Shaker No Water",
                                Description = "Shaker water hose is missing or pump is off."
                            });
                        }
                    }

                    // Duplex Jigs checks
                    CheckJigAlerts(plantHeader, "Jig 1", plant.Jig1Mounted, plant.Jig1HasPower, plant.Jig1Bucket);
                    CheckJigAlerts(plantHeader, "Jig 2", plant.Jig2Mounted, plant.Jig2HasPower, plant.Jig2Bucket);

                    // Sluice Box 3 Grates Missing
                    if (plant.SluiceBox3Mounted && plant.SluiceGratesTotal > 0 && plant.SluiceGratesInstalled < plant.SluiceGratesTotal)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Mats",
                            Title = $"{plantHeader}: Sluice Grates Missing",
                            Description = $"Only {plant.SluiceGratesInstalled} of {plant.SluiceGratesTotal} nugget/diamond traps installed."
                        });
                    }

                    // Sluice End HogPans checks
                    if (plant.EndHogPan1Mounted && !plant.EndHogPan1HasWater)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Water",
                            Title = $"{plantHeader}: End HogPan 1 No Water",
                            Description = "Water hose disconnected or pump inactive."
                        });
                    }
                    if (plant.EndHogPan2Mounted && !plant.EndHogPan2HasWater)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Water",
                            Title = $"{plantHeader}: End HogPan 2 No Water",
                            Description = "Water hose disconnected or pump inactive."
                        });
                    }

                    // Sluice Mats & Grilles
                    if (plant.SluiceMatsTotal > 0 && plant.SluiceMatsInstalled < plant.SluiceMatsTotal)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Mats",
                            Title = $"{plantHeader}: Sluice Mats Missing",
                            Description = $"Only {plant.SluiceMatsInstalled} of {plant.SluiceMatsTotal} main mats installed."
                        });
                    }
                    if (plant.SluiceGrillesTotal > 0 && plant.SluiceGrillesInstalled < plant.SluiceGrillesTotal)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Mats",
                            Title = $"{plantHeader}: Sluice Grilles Missing",
                            Description = $"Only {plant.SluiceGrillesInstalled} of {plant.SluiceGrillesTotal} grilles installed."
                        });
                    }
                    if (plant.MaxMatFillPct >= matThreshold)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Mats",
                            Title = $"{plantHeader}: Mats Nearly Full",
                            Description = $"Mats reached {Mathf.RoundToInt(plant.MaxMatFillPct * 100f)}% capacity."
                        });
                    }

                    // End HogPan 1 Mats
                    if (plant.EndHogPan1Mounted && plant.EndHogPan1MatsTotal > 0 && plant.EndHogPan1MatsInstalled < plant.EndHogPan1MatsTotal)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Mats",
                            Title = $"{plantHeader}: End HogPan 1 Mats Missing",
                            Description = $"Only {plant.EndHogPan1MatsInstalled} of {plant.EndHogPan1MatsTotal} mats installed."
                        });
                    }

                    // End HogPan 2 Mats
                    if (plant.EndHogPan2Mounted && plant.EndHogPan2MatsTotal > 0 && plant.EndHogPan2MatsInstalled < plant.EndHogPan2MatsTotal)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Mats",
                            Title = $"{plantHeader}: End HogPan 2 Mats Missing",
                            Description = $"Only {plant.EndHogPan2MatsInstalled} of {plant.EndHogPan2MatsTotal} mats installed."
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
                    string beastHeader = $"{beast.ClaimName ?? $"Claim #{beast.ClaimId}"} (Orange Beast #{beast.BeastIndex})";

                    if (!beast.HasPower)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Power",
                            Title = $"{beastHeader}: No Power",
                            Description = "Generator disconnected or turned off."
                        });
                    }
                    if (!beast.HasWater)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Water",
                            Title = $"{beastHeader}: No Water",
                            Description = "Water pump disconnected or turned off."
                        });
                    }

                    // Mats & Grilles
                    if (beast.TotalMats > 0 && beast.InstalledMats < beast.TotalMats)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Mats",
                            Title = $"{beastHeader}: Sluice Mats Missing",
                            Description = $"Only {beast.InstalledMats} of {beast.TotalMats} mats installed."
                        });
                    }
                    if (beast.TotalGrilles > 0 && beast.InstalledGrilles < beast.TotalGrilles)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Mats",
                            Title = $"{beastHeader}: Sluice Grilles Missing",
                            Description = $"Only {beast.InstalledGrilles} of {beast.TotalGrilles} grilles installed."
                        });
                    }
                    if (beast.MaxMatFillPct >= matThreshold)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Mats",
                            Title = $"{beastHeader}: Mats Nearly Full",
                            Description = $"Mats reached {Mathf.RoundToInt(beast.MaxMatFillPct * 100f)}% capacity."
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

            ActiveAlerts.Sort((a, b) => b.Severity.CompareTo(a.Severity));
        }

        // ==========================================
        // Helper Methods for Alert Evaluation
        // ==========================================

        private void CheckJigAlerts(string header, string jigName, bool isMounted, bool hasPower, JigBucketStatus bucket)
        {
            if (!isMounted) return;

            if (!hasPower)
            {
                ActiveAlerts.Add(new ClaimAlert
                {
                    Severity = AlertSeverity.Warning,
                    Category = "Power",
                    Title = $"{header}: {jigName} No Power",
                    Description = $"{jigName} has no power supply."
                });
            }

            if (bucket != null)
            {
                if (!bucket.IsMounted)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "Jigs",
                        Title = $"{header}: {jigName} Bucket Missing",
                        Description = $"Concentrate bucket is not mounted on {jigName}."
                    });
                }
                else if (bucket.FillPct >= 0.95f)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "Jigs",
                        Title = $"{header}: {jigName} Bucket Full",
                        Description = $"{jigName} bucket reached {Mathf.RoundToInt(bucket.FillPct * 100f)}% capacity."
                    });
                }
            }
        }

        private void EvaluateMachineParts(string header, List<MachinePartStatus> parts, float wearThreshold, bool monitorButtons)
        {
            if (parts == null) return;

            foreach (var part in parts)
            {
                if (part == null) continue;
                if (part.Category == MachinePartCategory.SwitchButton && !monitorButtons) continue;

                string source = !string.IsNullOrEmpty(part.SourceComponent) ? $"[{part.SourceComponent}] " : string.Empty;

                if (!part.IsInPlace || part.IsDestroyed)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Critical,
                        Category = "Maintenance",
                        Title = $"{header}: {source}Part Fault",
                        Description = $"{part.Name} is missing or destroyed."
                    });
                }
                else if (part.Durability <= wearThreshold)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "Maintenance",
                        Title = $"{header}: {source}Low Durability",
                        Description = $"{part.Name} durability is down to {Mathf.RoundToInt(part.Durability * 100f)}%."
                    });
                }
            }
        }

        private void EvaluateFeedingChain(string header, FeederChainStatus chain, float wearThreshold)
        {
            if (chain == null) return;

            // Hopper Alert
            if (chain.HopperMounted && !chain.HopperHasPower)
            {
                ActiveAlerts.Add(new ClaimAlert
                {
                    Severity = AlertSeverity.Warning,
                    Category = "Power",
                    Title = $"{header}: Hopper No Power",
                    Description = "Hopper is mounted but has no power supply."
                });
            }

            // Elevator Alert
            if (chain.ConveyorBeltMounted && !chain.ConveyorBeltHasPower)
            {
                ActiveAlerts.Add(new ClaimAlert
                {
                    Severity = AlertSeverity.Warning,
                    Category = "Power",
                    Title = $"{header}: Conveyor Elevator No Power",
                    Description = "Elevator is mounted but has no power supply."
                });
            }

            if (chain.Parts != null)
            {
                foreach (var part in chain.Parts)
                {
                    if (part == null) continue;
                    string source = !string.IsNullOrEmpty(part.SourceComponent) ? $"[{part.SourceComponent}] " : string.Empty;

                    if (!part.IsInPlace || part.IsDestroyed)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Critical,
                            Category = "Maintenance",
                            Title = $"{header}: {source}Feeder Part Broken",
                            Description = $"{part.Name} is missing or broken."
                        });
                    }
                    else if (part.Durability <= wearThreshold)
                    {
                        ActiveAlerts.Add(new ClaimAlert
                        {
                            Severity = AlertSeverity.Warning,
                            Category = "Maintenance",
                            Title = $"{header}: {source}Low Durability",
                            Description = $"{part.Name} is at {Mathf.RoundToInt(part.Durability * 100f)}% durability."
                        });
                    }
                }
            }
        }
    }
}