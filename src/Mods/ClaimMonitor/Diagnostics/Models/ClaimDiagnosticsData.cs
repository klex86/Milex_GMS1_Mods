using System;
using System.Collections.Generic;
using Milex.GMS1.Mods.ClaimMonitor.Config;
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

        public float FuelPercentage => MaxFuel > 0f ? Mathf.Clamp01(CurrentFuel / MaxFuel) * 100f : 0f;
    }

    public class UtilityStatus
    {
        public int InstanceId { get; set; }
        public string Name { get; set; }
        public string UtilityType { get; set; } // "Generator", "WaterPump", "WaterTower", "FuelStation"
        public Vector3 Position { get; set; }
        public bool IsWorking { get; set; }
        public float CurrentLevel { get; set; }
        public float MaxLevel { get; set; }
        public string Details { get; set; }
    }

    public class RawDebugItem
    {
        public string Category { get; set; }
        public string TypeName { get; set; }
        public string GameObjectName { get; set; }
        public int InstanceId { get; set; }
        public Vector3 Position { get; set; }
        public string Details { get; set; }
    }

    public class ClaimDiagnosticsData
    {
        public List<MatStatus> Mats { get; } = new List<MatStatus>();
        public List<PlantComponentStatus> PlantComponents { get; } = new List<PlantComponentStatus>();
        public List<ConveyorStatus> Conveyors { get; } = new List<ConveyorStatus>();
        public List<VehicleFuelStatus> Vehicles { get; } = new List<VehicleFuelStatus>();
        public List<UtilityStatus> Utilities { get; } = new List<UtilityStatus>();
        public List<RawDebugItem> RawInspectionItems { get; } = new List<RawDebugItem>();

        public List<ClaimAlert> ActiveAlerts { get; } = new List<ClaimAlert>();

        public void Reset()
        {
            Mats.Clear();
            PlantComponents.Clear();
            Conveyors.Clear();
            Vehicles.Clear();
            Utilities.Clear();
            RawInspectionItems.Clear();
            ActiveAlerts.Clear();
        }

        public void CompileAlerts(MonitorConfig config)
        {
            ActiveAlerts.Clear();
            if (config == null) return;

            float matWarnThreshold = config.MatWarningThreshold?.Value ?? 90f;
            float fuelWarnThreshold = config.VehicleLowFuelThreshold?.Value ?? 15f;

            // 1. Mats Evaluation
            CompileMatAlerts(config, matWarnThreshold);

            // 2. Wash Plant Machinery Evaluation
            CompileMachineryAlerts(config);

            // 3. Feeding Chain Evaluation (Hoppers & Conveyors)
            CompileConveyorAlerts(config);

            // 4. Vehicle Fuel Evaluation
            CompileVehicleAlerts(fuelWarnThreshold);

            // 5. Utilities Evaluation (Generators, Water Towers, Pumps)
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
                        Title = $"{setupName}: Sluice Mats Overflowing!",
                        Description = $"{overflowingCount} mat(s) at 100% capacity! Clean immediately to prevent gold loss."
                    });
                }
                else if (warningCount > 0)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "Mats",
                        Title = $"{setupName}: Sluice Mats Nearly Full",
                        Description = $"{warningCount} mat(s) above {threshold:F0}% (Highest: {maxPct:F1}%)."
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
                        Title = $"{setupName}: {name} Failure",
                        Description = comp.SpecificIssue ?? $"{name} has suffered a critical failure.",
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
                        Title = $"{setupName}: {name} Issue",
                        Description = comp.SpecificIssue ?? $"{name} is not operational.",
                        Position = comp.Position,
                        SourceId = comp.InstanceId
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
                        Title = $"{setupName}: Conveyor Power Loss",
                        Description = $"'{conv.Name}' has no electric power supply.",
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
                        Title = $"{setupName}: Hopper Backlog",
                        Description = $"'{conv.Name}' is full ({conv.CurrentDirt:F1}/{conv.MaxDirt:F1} m³).",
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
                        Title = $"Vehicle Out of Fuel: {v.VehicleName}",
                        Description = $"Fuel tank is completely empty. Refueling required.",
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
                        Title = $"Low Fuel: {v.VehicleName}",
                        Description = $"Fuel level is at {v.FuelPercentage:F1}% ({v.CurrentFuel:F1} L).",
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
                if (util.UtilityType == "WaterTower" && util.MaxLevel > 0f && util.CurrentLevel <= 1.0f)
                {
                    ActiveAlerts.Add(new ClaimAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = "Water",
                        Title = "Water Tower Depleted",
                        Description = "Water tower reservoir is empty. Pumps or delivery required.",
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
                        Title = $"Power Generator Inactive: {util.Name}",
                        Description = "Generator is stopped or disconnected.",
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
                case WashPlantSetupType.Setup1_Mobile: return "Mobile Plant";
                case WashPlantSetupType.Setup2_Stationary: return "Setup T3-T5";
                case WashPlantSetupType.Setup3_OrangeBeast: return "Orange Beast";
                default: return "Wash Plant";
            }
        }
    }
}