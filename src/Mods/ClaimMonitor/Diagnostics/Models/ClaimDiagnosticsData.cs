using System;
using System.Collections.Generic;
using Milex.GMS1.Mods.ClaimMonitor.Config;
using Milex.GMS1.Core.Localization;
using UnityEngine;

namespace Milex.GMS1.Mods.ClaimMonitor.Diagnostics.Models
{
    public enum WashPlantSetupType
    {
        None,
        Setup1_Mobile,      // Tier 2: Mini & Mobile Wash Plants
        Setup2_Stationary,  // Tier 3-5: Stationary Wash Plant (Shaker, Trommel, Duplex Jigs, Sluices)
        Setup3_OrangeBeast  // Tier 5/6: Orange Beast Wash Plant
    }

    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical
    }

    public class ClaimAlert
    {
        public AlertSeverity Severity { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Vector3 Position { get; set; }
        public int SourceId { get; set; }
    }

    public class MatStatus
    {
        public int InstanceId { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public float CurrentVolume { get; set; }
        public float Capacity { get; set; }
        public float GoldForRent { get; set; }
        public bool IsInHolder { get; set; }
        public bool IsBonusMat { get; set; }
        public WashPlantSetupType Setup { get; set; }

        public float FillPercentage => Capacity > 0f ? Mathf.Clamp01(CurrentVolume / Capacity) * 100f : 0f;
    }

    public class PlantComponentStatus
    {
        public int InstanceId { get; set; }
        public string TypeName { get; set; }
        public string DisplayName { get; set; }
        public string GameObjectName { get; set; }
        public Vector3 Position { get; set; }
        public bool IsWorking { get; set; }
        public bool HasPower { get; set; }
        public bool HasWater { get; set; }
        public string SpecificIssue { get; set; }
        public bool IsCritical { get; set; }
        public WashPlantSetupType Setup { get; set; }
    }

    public class ConveyorStatus
    {
        public int InstanceId { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public float CurrentDirt { get; set; }
        public float MaxDirt { get; set; }
        public bool HasPower { get; set; }
        public bool IsWorking { get; set; }
        public WashPlantSetupType AssignedSetup { get; set; }
    }

    public class VehicleFuelStatus
    {
        public int InstanceId { get; set; }
        public string VehicleName { get; set; }
        public Vector3 Position { get; set; }
        public float CurrentFuel { get; set; }
        public float MaxFuel { get; set; }
        public bool IsEngineRunning { get; set; }
        public int SwitchSlotIndex { get; set; } = -1;

        public float FuelPercentage => MaxFuel > 0f ? Mathf.Clamp01(CurrentFuel / MaxFuel) * 100f : 0f;
    }

    public class UtilityStatus
    {
        public int InstanceId { get; set; }
        public string Name { get; set; }
        public string UtilityType { get; set; } // "Generator", "WaterPump", "WaterTower", "FuelStation"
        public Vector3 Position { get; set; }
        public float CurrentLevel { get; set; }
        public float MaxLevel { get; set; }
        public bool IsWorking { get; set; }
        public bool IsConnected { get; set; } = true;
        public string Details { get; set; }
    }

    public class EquipmentWearStatus
    {
        public int InstanceId { get; set; }
        public string PartName { get; set; }
        public string ParentMachineName { get; set; }
        public Vector3 Position { get; set; }
        public float Durability { get; set; } // 0.0 to 1.0
        public bool IsDestroyed { get; set; }
        public bool IsInPlace { get; set; }
        public bool IsConnected { get; set; } = true;
        public WashPlantSetupType Setup { get; set; }

        public float DurabilityPercentage => Mathf.Clamp01(Durability) * 100f;
    }

    public class RawDebugItem
    {
        public string Category { get; set; }
        public string TypeName { get; set; }
        public string GameObjectName { get; set; }
        public int InstanceId { get; set; }
        public Vector3 Position { get; set; }
        public string Details { get; set; }
        public WashPlantSetupType Setup { get; set; }
    }

    public class ClaimDiagnosticsData
    {
        public List<MatStatus> Mats { get; } = new List<MatStatus>();
        public List<PlantComponentStatus> PlantComponents { get; } = new List<PlantComponentStatus>();
        public List<ConveyorStatus> Conveyors { get; } = new List<ConveyorStatus>();
        public List<VehicleFuelStatus> Vehicles { get; } = new List<VehicleFuelStatus>();
        public List<UtilityStatus> Utilities { get; } = new List<UtilityStatus>();
        public List<EquipmentWearStatus> WearParts { get; } = new List<EquipmentWearStatus>();
        public List<RawDebugItem> RawInspectionItems { get; } = new List<RawDebugItem>();

        public List<ClaimAlert> ActiveAlerts { get; } = new List<ClaimAlert>();

        public void Reset()
        {
            Mats.Clear();
            PlantComponents.Clear();
            Conveyors.Clear();
            Vehicles.Clear();
            Utilities.Clear();
            WearParts.Clear();
            RawInspectionItems.Clear();
            ActiveAlerts.Clear();
        }

        public void CompileAlerts(MonitorConfig config)
        {
            ActiveAlerts.Clear();
            if (config == null) return;

            float matWarnThreshold = config.MatWarningThreshold?.Value ?? 90f;
            float fuelWarnThreshold = config.VehicleLowFuelThreshold?.Value ?? 15f;
            float wearWarnThreshold = config.ComponentWearWarningThreshold?.Value ?? 20f;

            // 1. Mats Evaluation
            CompileMatAlerts(config, matWarnThreshold);

            // 2. Wash Plant Machinery Evaluation
            CompileMachineryAlerts(config);

            // 3. Equipment Parts Wear Evaluation (CheckAndRepair)
            CompileWearAlerts(config, wearWarnThreshold);

            // 4. Feeding Chain Evaluation (Hoppers & Conveyors)
            CompileConveyorAlerts(config);

            // 5. Vehicle Fuel Evaluation
            CompileVehicleAlerts(fuelWarnThreshold);

            // 6. Utilities Evaluation (Generators, Water Towers, Pumps)
            CompileUtilityAlerts();

            // Sort alerts: Critical first, then Warning, then Info
            ActiveAlerts.Sort((a, b) => b.Severity.CompareTo(a.Severity));
        }

        private void CompileMatAlerts(MonitorConfig config, float threshold)
        {
            var setupMats = new Dictionary<WashPlantSetupType, List<MatStatus>>();
            setupMats[WashPlantSetupType.Setup1_Mobile] = new List<MatStatus>();
            setupMats[WashPlantSetupType.Setup2_Stationary] = new List<MatStatus>();
            setupMats[WashPlantSetupType.Setup3_OrangeBeast] = new List<MatStatus>();

            foreach (var mat in Mats)
            {
                if (!mat.IsInHolder) continue;

                if (mat.Setup == WashPlantSetupType.Setup1_Mobile && config.MonitorSetup1.Value)
                    setupMats[WashPlantSetupType.Setup1_Mobile].Add(mat);
                else if (mat.Setup == WashPlantSetupType.Setup2_Stationary && config.MonitorSetup2.Value)
                    setupMats[WashPlantSetupType.Setup2_Stationary].Add(mat);
                else if (mat.Setup == WashPlantSetupType.Setup3_OrangeBeast && config.MonitorSetup3.Value)
                    setupMats[WashPlantSetupType.Setup3_OrangeBeast].Add(mat);
            }

            foreach (var kvp in setupMats)
            {
                var list = kvp.Value;
                if (list.Count == 0) continue;

                int overflowingCount = 0;
                int warningCount = 0;
                float maxPct = 0f;

                foreach (var mat in list)
                {
                    float pct = mat.FillPercentage;
                    if (pct > maxPct) maxPct = pct;

                    if (pct >= 98f)
                        overflowingCount++;
                    else if (pct >= threshold)
                        warningCount++;
                }

                string setupName = GetSetupName(kvp.Key);

                if (overflowingCount > 0)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Critical,
                        Category = "Mats",
                        Title = LocalizationManager.Format("alert.mats.overflow.title", "{0}: Sluice Mats Overflowing!", setupName),
                        Description = LocalizationManager.Format("alert.mats.overflow.desc", "{0} mat(s) at 100% capacity! Clean immediately to prevent gold loss.", overflowingCount)
                    });
                }
                else if (warningCount > 0)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "Mats",
                        Title = LocalizationManager.Format("alert.mats.near_full.title", "{0}: Sluice Mats Nearly Full", setupName),
                        Description = LocalizationManager.Format("alert.mats.near_full.desc", "{0} mat(s) above {1:F0}% (Highest: {2:F1}%).", warningCount, threshold, maxPct)
                    });
                }
            }
        }

        private void CompileMachineryAlerts(MonitorConfig config)
        {
            foreach (var comp in PlantComponents)
            {
                if (comp.Setup == WashPlantSetupType.Setup1_Mobile && !config.MonitorSetup1.Value) continue;
                if (comp.Setup == WashPlantSetupType.Setup2_Stationary && !config.MonitorSetup2.Value) continue;
                if (comp.Setup == WashPlantSetupType.Setup3_OrangeBeast && !config.MonitorSetup3.Value) continue;

                string setupName = GetSetupName(comp.Setup);
                string name = !string.IsNullOrEmpty(comp.DisplayName) ? comp.DisplayName : comp.TypeName;

                if (comp.IsCritical)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Critical,
                        Category = "WashPlant",
                        Title = LocalizationManager.Format("alert.machinery.failure.title", "{0}: {1} Failure", setupName, name),
                        Description = comp.SpecificIssue ?? LocalizationManager.Format("alert.machinery.failure.desc", "{0} has suffered a critical failure.", name),
                        Position = comp.Position,
                        SourceId = comp.InstanceId
                    });
                }
                else if (!comp.IsWorking)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "WashPlant",
                        Title = LocalizationManager.Format("alert.machinery.issue.title", "{0}: {1} Issue", setupName, name),
                        Description = comp.SpecificIssue ?? LocalizationManager.Format("alert.machinery.issue.desc", "{0} is not operational.", name),
                        Position = comp.Position,
                        SourceId = comp.InstanceId
                    });
                }
            }
        }

        private void CompileWearAlerts(MonitorConfig config, float wearThreshold)
        {
            foreach (var part in WearParts)
            {
                // Only alert on installed parts belonging to actively monitored setups
                if (!part.IsInPlace) continue;

                // Only monitor wear on standalone infrastructure (pumps, towers, generators) if connected to cables/hoses
                if (part.Setup == WashPlantSetupType.None && !part.IsConnected) continue;

                if (part.Setup == WashPlantSetupType.Setup1_Mobile && !config.MonitorSetup1.Value) continue;
                if (part.Setup == WashPlantSetupType.Setup2_Stationary && !config.MonitorSetup2.Value) continue;
                if (part.Setup == WashPlantSetupType.Setup3_OrangeBeast && !config.MonitorSetup3.Value) continue;

                string setupName = part.Setup == WashPlantSetupType.None
                    ? LocalizationManager.T("setup.name.infrastructure", "Infrastructure")
                    : GetSetupName(part.Setup);
                string machine = !string.IsNullOrEmpty(part.ParentMachineName) ? part.ParentMachineName : LocalizationManager.T("equipment.default", "Equipment");
                string partName = !string.IsNullOrEmpty(part.PartName) ? part.PartName : LocalizationManager.T("equipment.part_default", "Component");

                if (part.IsDestroyed || part.DurabilityPercentage <= 0.5f)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Critical,
                        Category = "Maintenance",
                        Title = LocalizationManager.Format("alert.wear.broken.title", "{0}: {1} Broken!", setupName, partName),
                        Description = LocalizationManager.Format("alert.wear.broken.desc", "{0} {1} is destroyed and must be replaced immediately.", machine, partName),
                        Position = part.Position,
                        SourceId = part.InstanceId
                    });
                }
                else if (part.DurabilityPercentage <= wearThreshold)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "Maintenance",
                        Title = LocalizationManager.Format("alert.wear.high.title", "{0}: {1} Wear High", setupName, partName),
                        Description = LocalizationManager.Format("alert.wear.high.desc", "{0} {1} durability low ({2:F0}% remaining). Prepare replacement.", machine, partName, part.DurabilityPercentage),
                        Position = part.Position,
                        SourceId = part.InstanceId
                    });
                }
            }
        }

        private void CompileConveyorAlerts(MonitorConfig config)
        {
            foreach (var conv in Conveyors)
            {
                bool shouldMonitor = false;
                if (conv.AssignedSetup == WashPlantSetupType.Setup2_Stationary && config.Setup2IncludeFeedingChain.Value)
                    shouldMonitor = true;
                else if (conv.AssignedSetup == WashPlantSetupType.Setup3_OrangeBeast && config.Setup3IncludeFeedingChain.Value)
                    shouldMonitor = true;

                if (!shouldMonitor) continue;

                string setupName = GetSetupName(conv.AssignedSetup);

                if (!conv.HasPower)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "FeedingChain",
                        Title = LocalizationManager.Format("alert.conveyor.power_loss.title", "{0}: Conveyor Power Loss", setupName),
                        Description = LocalizationManager.Format("alert.conveyor.power_loss.desc", "'{0}' has no electric power supply.", conv.Name),
                        Position = conv.Position,
                        SourceId = conv.InstanceId
                    });
                }
                else if (conv.MaxDirt > 0f && conv.CurrentDirt >= conv.MaxDirt * 0.98f)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "FeedingChain",
                        Title = LocalizationManager.Format("alert.conveyor.backlog.title", "{0}: Hopper Backlog", setupName),
                        Description = LocalizationManager.Format("alert.conveyor.backlog.desc", "'{0}' is full ({1:F1}/{2:F1} m³).", conv.Name, conv.CurrentDirt, conv.MaxDirt),
                        Position = conv.Position,
                        SourceId = conv.InstanceId
                    });
                }
            }
        }

        private void CompileVehicleAlerts(float fuelThreshold)
        {
            foreach (var v in Vehicles)
            {
                if (v.MaxFuel <= 0f) continue;

                if (v.CurrentFuel <= 0.05f)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Critical,
                        Category = "Fuel",
                        Title = LocalizationManager.Format("alert.fuel.empty.title", "Vehicle Out of Fuel: {0}", v.VehicleName),
                        Description = LocalizationManager.T("alert.fuel.empty.desc", "Fuel tank is completely empty. Refueling required."),
                        Position = v.Position,
                        SourceId = v.InstanceId
                    });
                }
                else if (v.FuelPercentage <= fuelThreshold && v.IsEngineRunning)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "Fuel",
                        Title = LocalizationManager.Format("alert.fuel.low.title", "Low Fuel: {0}", v.VehicleName),
                        Description = LocalizationManager.Format("alert.fuel.low.desc", "Fuel level is at {0:F1}% ({1:F1} L).", v.FuelPercentage, v.CurrentFuel),
                        Position = v.Position,
                        SourceId = v.InstanceId
                    });
                }
            }
        }

        private void CompileUtilityAlerts()
        {
            foreach (var util in Utilities)
            {
                // Disconnected or unused utilities parked on the claim are ignored in HUD alerts
                if (!util.IsConnected) continue;

                if (util.UtilityType == "WaterTower" && util.MaxLevel > 0f && util.CurrentLevel <= 1.0f)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "Water",
                        Title = LocalizationManager.T("alert.utility.water_tower.title", "Water Tower Depleted"),
                        Description = LocalizationManager.T("alert.utility.water_tower.desc", "Water tower reservoir is empty. Pumps or delivery required."),
                        Position = util.Position,
                        SourceId = util.InstanceId
                    });
                }
                else if (util.UtilityType == "Generator" && !util.IsWorking)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "Power",
                        Title = LocalizationManager.Format("alert.utility.generator.title", "Power Generator Inactive: {0}", util.Name),
                        Description = LocalizationManager.T("alert.utility.generator.desc", "Generator is stopped or disconnected."),
                        Position = util.Position,
                        SourceId = util.InstanceId
                    });
                }
            }
        }

        private static string GetSetupName(WashPlantSetupType type)
        {
            switch (type)
            {
                case WashPlantSetupType.Setup1_Mobile: return LocalizationManager.T("setup.name.mobile", "Mobile Plant");
                case WashPlantSetupType.Setup2_Stationary: return LocalizationManager.T("setup.name.stationary", "Setup T3-T5");
                case WashPlantSetupType.Setup3_OrangeBeast: return LocalizationManager.T("setup.name.orange_beast", "Orange Beast");
                default: return LocalizationManager.T("setup.name.default", "Wash Plant");
            }
        }
    }
}