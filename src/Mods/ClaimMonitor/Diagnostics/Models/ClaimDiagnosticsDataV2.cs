using System;
using System.Collections.Generic;
using Milex.GMS1.Core.Localization;
using Milex.GMS1.Mods.ClaimMonitor.Config;
using UnityEngine;

namespace Milex.GMS1.Mods.ClaimMonitor.Diagnostics.Models
{
    public enum MachinePartCategory
    {
        Spring,
        HydraulicHose,
        Engine,
        SluiceMat,
        SluiceGrille,
        Other
    }

    public class MachinePartStatus
    {
        public string Name { get; set; }
        public string PrefabName { get; set; }
        public MachinePartCategory Category { get; set; }
        public bool IsInPlace { get; set; }
        public bool IsDestroyed { get; set; }
        public float Durability { get; set; } // 0.0 to 1.0
        public bool IsReinforced { get; set; }
    }

    public enum WashPlantTier
    {
        Unknown,
        Tier3,
        Tier4,
        Tier5GlacierCreek,
        OrangeBeast
    }

    public class WashPlantStatusV2
    {
        public int ClaimId { get; set; }
        public string ClaimName { get; set; }
        public WashPlantTier Tier { get; set; }
        public bool IsReadyToOperate { get; set; }

        // Operating conditions (Power & Water)
        public bool HasPower { get; set; }
        public bool HasWater { get; set; }
        public bool IsPowerReady { get; set; }
        public bool IsWaterReady { get; set; }
        public bool IsReadyToWork { get; set; }

        // Sluice capacities
        public int TotalMats { get; set; }
        public int InstalledMats { get; set; }
        public int TotalGrilles { get; set; }
        public int InstalledGrilles { get; set; }

        // Mounted machine parts (Springs, Hoses, Engine, etc.)
        public List<MachinePartStatus> Parts { get; set; } = new List<MachinePartStatus>();

        // Overall snapshot state
        public bool AllSpringsMounted => Parts.TrueForAll(p => p.Category != MachinePartCategory.Spring || (p.IsInPlace && !p.IsDestroyed));
        public bool AllHosesMounted => Parts.TrueForAll(p => p.Category != MachinePartCategory.HydraulicHose || (p.IsInPlace && !p.IsDestroyed));
        public bool AllMatsMounted => TotalMats > 0 && InstalledMats >= TotalMats;
        public bool AllGrillesMounted => TotalGrilles > 0 && InstalledGrilles >= TotalGrilles;
    }


    public class ClaimDiagnosticsDataV2
    {
        public List<WashPlantStatusV2> WashPlants { get; } = new List<WashPlantStatusV2>();
        public List<ClaimAlert> ActiveAlerts { get; } = new List<ClaimAlert>();

        public void Reset()
        {
            WashPlants.Clear();
            ActiveAlerts.Clear();
        }

        public void CompileAlerts(MonitorConfig config)
        {
            ActiveAlerts.Clear();
            if (config == null) return;

            foreach (var plant in WashPlants)
            {
                string claimHeader = !string.IsNullOrEmpty(plant.ClaimName) ? plant.ClaimName : $"Claim #{plant.ClaimId}";
                claimHeader = $"{claimHeader} ({plant.Tier})";

                // 1. Critical: Springs check
                if (!plant.AllSpringsMounted)
                {
                    int missingOrBroken = plant.Parts.FindAll(p => p.Category == MachinePartCategory.Spring && (!p.IsInPlace || p.IsDestroyed)).Count;
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Critical,
                        Category = "Maintenance",
                        Title = $"{claimHeader}: Shaker Springs Missing/Broken",
                        Description = $"{missingOrBroken} shaker spring(s) are missing or destroyed on {plant.Tier}."
                    });
                }

                // 2. Critical: Hydraulic hoses check
                if (!plant.AllHosesMounted)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Critical,
                        Category = "Maintenance",
                        Title = $"{claimHeader}: Hydraulic Hose Fault",
                        Description = "Hydraulic hose on shaker is missing or damaged."
                    });
                }

                // 3. Warning: Sluice mats / grilles
                if (!plant.AllMatsMounted)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "Mats",
                        Title = $"{claimHeader}: Sluice Mats Missing",
                        Description = $"Only {plant.InstalledMats} of {plant.TotalMats} mats installed."
                    });
                }

                if (!plant.AllGrillesMounted)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "Mats",
                        Title = $"{claimHeader}: Sluice Grilles Missing",
                        Description = $"Only {plant.InstalledGrilles} of {plant.TotalGrilles} grilles installed."
                    });
                }

                // 4. Operational state check

                if (!plant.HasPower)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "WashPlant",
                        Title = $"{claimHeader}: No Power",
                        Description = "Plant has no Power."
                    });
                }
                if (!plant.HasWater)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "WashPlant",
                        Title = $"{claimHeader}: No Water",
                        Description = "Plant has no Water."
                    });
                }
                if (!plant.IsReadyToWork)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "WashPlant",
                        Title = $"{claimHeader}: Not Operating",
                        Description = "Plant has power and water but is not operating."
                    });
                }



            }

            ActiveAlerts.Sort((a, b) => b.Severity.CompareTo(a.Severity));
        }
    }
}
