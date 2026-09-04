using System;
using System.Collections;
using System.Reflection;
using System.Text;
using Milex.GMS1.Core;
using Milex.GMS1.Mods.ClaimMonitor.Config;
using Milex.GMS1.Mods.ClaimMonitor.Diagnostics.Models;
using UnityEngine;

namespace Milex.GMS1.Mods.ClaimMonitor.Diagnostics
{
    public class ClaimScanner : MonoBehaviour
    {
        public static ClaimScanner Instance { get; private set; }
        public ClaimDiagnosticsData CurrentData { get; } = new ClaimDiagnosticsData();
        public MonitorConfig Config { get; set; }

        private Coroutine _scanRoutine;
        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private void Awake()
        {
            Instance = this;
        }

        public void StartScanning()
        {
            if (_scanRoutine != null)
                StopCoroutine(_scanRoutine);

            _scanRoutine = StartCoroutine(PeriodicScan());
        }

        public void StopScanning()
        {
            if (_scanRoutine != null)
            {
                StopCoroutine(_scanRoutine);
                _scanRoutine = null;
            }
        }

        private IEnumerator PeriodicScan()
        {
            while (true)
            {
                ForceScan();
                float interval = Config?.ScanIntervalSeconds?.Value ?? 3.0f;
                yield return new WaitForSeconds(Mathf.Max(1.0f, interval));
            }
        }

        public void ForceScan()
        {
            CurrentData.Reset();
            var allComponents = FindObjectsOfType<MonoBehaviour>();
            int matchedCount = 0;

            // 1. First pass: Locate active Gold Counters to anchor setup positions
            Vector3 stationaryCenter = Vector3.zero;
            bool hasStationaryCenter = false;
            Vector3 orangeBeastCenter = Vector3.zero;
            bool hasOrangeBeastCenter = false;

            foreach (var mb in allComponents)
            {
                if (mb == null) continue;
                string tName = mb.GetType().Name;
                if (tName == "WashPlantGoldCounter")
                {
                    stationaryCenter = mb.transform.position;
                    hasStationaryCenter = true;
                }
                else if (tName == "OrangeBeastWashPlantGoldCounter")
                {
                    orangeBeastCenter = mb.transform.position;
                    hasOrangeBeastCenter = true;
                }
            }

            // 2. Second pass: Process all relevant equipment
            foreach (var mb in allComponents)
            {
                if (mb == null) continue;

                string typeName = mb.GetType().Name;

                // 1. Sluice Mats (MinersMoss)
                if (typeName == "MinersMoss")
                {
                    matchedCount++;
                    ScanMinersMoss(mb);
                    continue;
                }

                // 2. Vehicles (MachineController subclasses)
                if (IsVehicleType(mb))
                {
                    matchedCount++;
                    ScanVehicle(mb);
                    continue;
                }

                // 3. Wash plant setups & components
                if (IsPlantMachinery(mb))
                {
                    matchedCount++;
                    ScanPlantMachinery(mb);
                    continue;
                }

                // 4. Feeders & Conveyors
                if (typeName == "ConveyorGround" || typeName == "ConveyorElevator")
                {
                    matchedCount++;
                    ScanConveyor(mb, stationaryCenter, hasStationaryCenter, orangeBeastCenter, hasOrangeBeastCenter);
                    continue;
                }

                // 5. Utilities (Fuel, Power, Water)
                if (IsUtilityType(typeName))
                {
                    matchedCount++;
                    ScanUtility(mb);
                }
            }

            // 3. Compile prioritized alerts for the Warning HUD
            CurrentData.CompileAlerts(Config);
        }

        private void ScanMinersMoss(MonoBehaviour comp)
        {
            var type = comp.GetType();
            float currentVolume = GetFieldValue<float>(comp, type, "GroundVolume");
            float maxCapacity = GetFieldValue<float>(comp, type, "MaxGroundVolume");
            float gold = GetFieldValue<float>(comp, type, "GoldForRent");
            bool inHolder = GetFieldValue<bool>(comp, type, "IsInHolder");
            bool isBonus = GetFieldValue<bool>(comp, type, "_IsBonusMat");
            string drawerKey = GetFieldValue<string>(comp, type, "MaxGroundVolumePropertyDrawerKey") ?? "";

            // Deduce wash plant setup
            WashPlantSetupType setup = WashPlantSetupType.Setup2_Stationary;
            if (drawerKey.Contains("OBMATS") || comp.name.Contains("OB") || comp.name.Contains("Orange"))
            {
                setup = WashPlantSetupType.Setup3_OrangeBeast;
            }
            else if (drawerKey.Contains("HOGPAN") || comp.name.Contains("HogPan") || comp.name.Contains("Mini"))
            {
                setup = WashPlantSetupType.Setup1_Mobile;
            }
            else if (drawerKey.Contains("WASHPLANT"))
            {
                setup = WashPlantSetupType.Setup2_Stationary;
            }
            else
            {
                // Fallback check on parent hierarchy
                if (comp.GetComponentInParent(Type.GetType("GoldDigger.OrangeBeastWashPlantGoldCounter, Assembly-CSharp")) != null)
                    setup = WashPlantSetupType.Setup3_OrangeBeast;
                else if (comp.GetComponentInParent(Type.GetType("GoldDigger.MobileWashplantGoldCounter, Assembly-CSharp")) != null ||
                         comp.GetComponentInParent(Type.GetType("GoldDigger.MiniWashplantGoldCounter, Assembly-CSharp")) != null)
                    setup = WashPlantSetupType.Setup1_Mobile;
            }

            var status = new MatStatus
            {
                InstanceId = comp.gameObject.GetInstanceID(),
                Name = comp.gameObject.name,
                Position = comp.transform.position,
                CurrentVolume = currentVolume,
                Capacity = maxCapacity,
                GoldForRent = gold,
                IsInHolder = inHolder,
                IsBonusMat = isBonus,
                Setup = setup
            };

            CurrentData.Mats.Add(status);

            string details = $"Setup: {setup}, Volume: {status.CurrentVolume:F3}/{status.Capacity:F3} ({status.FillPercentage:F1}%), Mounted: {inHolder}";
            CurrentData.RawInspectionItems.Add(new RawDebugItem
            {
                Category = "Mat / Sluice",
                TypeName = "MinersMoss",
                GameObjectName = comp.gameObject.name,
                InstanceId = status.InstanceId,
                Position = status.Position,
                Details = details
            });
        }

        private void ScanVehicle(MonoBehaviour comp)
        {
            var go = comp.gameObject;
            var type = comp.GetType();

            object fuelController = GetFieldValue<object>(comp, type, "Fuel");
            float currentFuel = 0f;
            float maxFuel = 0f;

            if (fuelController != null)
            {
                var fcType = fuelController.GetType();
                currentFuel = GetFieldValue<float>(fuelController, fcType, "CurrentCapacity")
                           + GetPropertyValue<float>(fuelController, fcType, "GetCurrentCapacity");
                maxFuel = GetFieldValue<float>(fuelController, fcType, "MaxCapacity");
            }

            bool engineRunning = GetPropertyValue<bool>(comp, type, "IsEngineStarted")
                              || GetFieldValue<bool>(comp, type, "<IsEngineStarted>k__BackingField");

            int slotIndex = GetFieldValue<int>(comp, type, "numberInVehicleSwitchingList");
            string cleanName = NormalizeVehicleName(go.name, type.Name);

            var status = new VehicleFuelStatus
            {
                InstanceId = go.GetInstanceID(),
                VehicleName = cleanName,
                Position = go.transform.position,
                CurrentFuel = currentFuel,
                MaxFuel = maxFuel,
                IsEngineRunning = engineRunning,
                SwitchSlotIndex = slotIndex
            };

            CurrentData.Vehicles.Add(status);

            string details = $"Slot: {slotIndex}, Fuel: {status.CurrentFuel:F1}/{status.MaxFuel:F1} L ({status.FuelPercentage:F1}%), Engine: {(engineRunning ? "RUNNING" : "OFF")}";
            CurrentData.RawInspectionItems.Add(new RawDebugItem
            {
                Category = "Vehicle",
                TypeName = type.Name,
                GameObjectName = go.name,
                InstanceId = go.GetInstanceID(),
                Position = go.transform.position,
                Details = details
            });
        }

        private void ScanPlantMachinery(MonoBehaviour comp)
        {
            var go = comp.gameObject;
            int goId = go.GetInstanceID();

            // Prevent duplicate entries for multi-component objects (like Orange Beast Shaker)
            for (int i = 0; i < CurrentData.PlantComponents.Count; i++)
            {
                if (CurrentData.PlantComponents[i].InstanceId == goId)
                    return;
            }

            var type = comp.GetType();
            string typeName = type.Name;
            string displayName = GetMachineryDisplayName(comp, typeName);

            bool isWorking = true;
            bool isCritical = false;
            bool hasPower = true;
            bool hasWater = true;
            string issue = null;
            WashPlantSetupType setup = WashPlantSetupType.Setup2_Stationary;

            if (go.name.Contains("Washplant_Shaker_Beast") || go.name.Contains("OrangeBeast_Frame") || typeName == "OrangeBeastWashPlantGoldCounter")
            {
                setup = WashPlantSetupType.Setup3_OrangeBeast;
                displayName = "Orange Beast Shaker";
                hasPower = CheckPowerState(comp);
                hasWater = CheckWaterState(comp);
                bool ready = GetFieldValue<bool>(comp, type, "WashplantReady") || (hasPower && hasWater);

                if (!hasPower)
                {
                    isWorking = false;
                    issue = "Orange Beast Shaker has no electric power.";
                }
                else if (!hasWater)
                {
                    isWorking = false;
                    issue = "Orange Beast Shaker has no water supply.";
                }
                else if (!ready)
                {
                    isWorking = false;
                    issue = "Orange Beast not ready to work.";
                }
            }
            else if (typeName.Contains("Shaker"))
            {
                bool stopped = GetFieldValue<bool>(comp, type, "ShakerStopped");
                hasPower = CheckPowerState(comp);
                hasWater = CheckWaterState(comp);
                
                // Check if Orange Beast
                if (comp.name.Contains("Orange") || comp.name.Contains("OB") || comp.GetComponentInParent(Type.GetType("GoldDigger.OrangeBeastWashPlantGoldCounter, Assembly-CSharp")) != null)
                    setup = WashPlantSetupType.Setup3_OrangeBeast;
                else
                    setup = WashPlantSetupType.Setup2_Stationary;

                if (stopped)
                {
                    isWorking = false;
                    issue = $"{displayName} is stopped.";
                }
                else if (!hasPower)
                {
                    isWorking = false;
                    issue = $"{displayName} has no electric power.";
                }
                else if (!hasWater)
                {
                    isWorking = false;
                    issue = $"{displayName} has no water supply.";
                }
            }
            else if (typeName.Contains("Trommel"))
            {
                setup = typeName.Contains("Mobile") ? WashPlantSetupType.Setup1_Mobile : WashPlantSetupType.Setup2_Stationary;
                bool stopped = GetFieldValue<bool>(comp, type, "TrommelStopped");
                bool chainBroken = GetFieldValue<bool>(comp, type, "_TrommelChainDestroyed");
                hasPower = CheckPowerState(comp);

                if (chainBroken)
                {
                    isWorking = false;
                    isCritical = true;
                    issue = $"{displayName} drive chain destroyed / broken!";
                }
                else if (stopped)
                {
                    isWorking = false;
                    issue = $"{displayName} is stopped.";
                }
                else if (!hasPower)
                {
                    isWorking = false;
                    issue = $"{displayName} has no electric power.";
                }
            }
            else if (typeName.Contains("Duplex") || typeName == "GravelPump")
            {
                setup = WashPlantSetupType.Setup2_Stationary;
                bool pumpBroken = GetFieldValue<bool>(comp, type, "_DuplexJigBroken");
                hasPower = CheckPowerState(comp);
                hasWater = CheckWaterState(comp);

                // Check Buckets
                object b1 = GetFieldValue<object>(comp, type, "Bucket1");
                object b2 = GetFieldValue<object>(comp, type, "Bucket2");
                bool bucketFull = false;
                if (b1 != null)
                {
                    float vol1 = GetFieldValue<float>(b1, b1.GetType(), "GroundVolume");
                    float max1 = GetFieldValue<float>(b1, b1.GetType(), "MaxVolume");
                    if (max1 > 0f && vol1 >= max1 * 0.98f) bucketFull = true;
                }
                // GravelPump only uses 1 bucket
                if (typeName != "GravelPump" && b2 != null)
                {
                    float vol2 = GetFieldValue<float>(b2, b2.GetType(), "GroundVolume");
                    float max2 = GetFieldValue<float>(b2, b2.GetType(), "MaxVolume");
                    if (max2 > 0f && vol2 >= max2 * 0.98f) bucketFull = true;
                }

                if (pumpBroken)
                {
                    isWorking = false;
                    isCritical = true;
                    issue = $"{displayName} mechanism broken!";
                }
                else if (bucketFull)
                {
                    issue = $"{displayName} bucket is full (replace bucket).";
                }
                else if (!hasPower)
                {
                    isWorking = false;
                    issue = $"{displayName} has no electric power.";
                }
            }
            else if (typeName == "MobileWashplant" || typeName == "MiniWashplant")
            {
                setup = WashPlantSetupType.Setup1_Mobile;
                bool ready = GetFieldValue<bool>(comp, type, "IsReadyToWork") || GetFieldValue<bool>(comp, type, "IsOn");
                hasPower = CheckPowerState(comp);
                hasWater = CheckWaterState(comp);

                if (!ready)
                {
                    isWorking = false;
                    issue = $"{displayName} is turned off.";
                }
                else if (!hasWater)
                {
                    isWorking = false;
                    issue = $"{displayName} has no water pressure.";
                }
                else if (!hasPower)
                {
                    isWorking = false;
                    issue = $"{displayName} has no electric power.";
                }
            }

            var status = new PlantComponentStatus
            {
                InstanceId = goId,
                TypeName = typeName,
                DisplayName = displayName,
                GameObjectName = go.name,
                Position = go.transform.position,
                IsWorking = isWorking,
                IsCritical = isCritical,
                HasPower = hasPower,
                HasWater = hasWater,
                SpecificIssue = issue,
                Setup = setup
            };

            CurrentData.PlantComponents.Add(status);

            string details = $"Setup: {setup}, Name: {displayName}, Working: {isWorking}, Power: {hasPower}, Water: {hasWater}, Issue: {issue ?? "None"}";
            CurrentData.RawInspectionItems.Add(new RawDebugItem
            {
                Category = GetCategoryForSetup(setup),
                TypeName = typeName,
                GameObjectName = go.name,
                InstanceId = goId,
                Position = go.transform.position,
                Details = details
            });
        }

        private void ScanConveyor(MonoBehaviour comp, Vector3 statPos, bool hasStat, Vector3 obPos, bool hasOb)
        {
            var go = comp.gameObject;
            var type = comp.GetType();
            string typeName = type.Name;

            float currentDirt = 0f;
            float maxDirt = 0f;
            bool hasPower = true;
            bool isWorking = true;

            // Check PowerConsumer
            object powerConsumer = GetFieldValue<object>(comp, type, "MyPower");
            if (powerConsumer != null)
            {
                hasPower = GetPropertyValue<bool>(powerConsumer, powerConsumer.GetType(), "HavePower");
            }

            if (typeName == "ConveyorGround")
            {
                currentDirt = GetFieldValue<float>(comp, type, "DirtVolume");
                maxDirt = GetFieldValue<float>(comp, type, "MaxDirt");
                float speed = GetFieldValue<float>(comp, type, "Speed");
                isWorking = hasPower && speed > 0.01f;
            }
            else if (typeName == "ConveyorElevator")
            {
                maxDirt = GetFieldValue<float>(comp, type, "BucketCapacity");
                float speed = GetFieldValue<float>(comp, type, "TrackSpeed") + GetFieldValue<float>(comp, type, "speed");
                isWorking = hasPower && speed > 0.01f;
            }

            // Associate with nearest setup
            WashPlantSetupType assigned = WashPlantSetupType.Setup2_Stationary;
            if (hasOb && (!hasStat || Vector3.Distance(go.transform.position, obPos) < Vector3.Distance(go.transform.position, statPos)))
            {
                assigned = WashPlantSetupType.Setup3_OrangeBeast;
            }

            var status = new ConveyorStatus
            {
                InstanceId = go.GetInstanceID(),
                Name = go.name.Replace("(Clone)", "").Trim(),
                Position = go.transform.position,
                CurrentDirt = currentDirt,
                MaxDirt = maxDirt,
                HasPower = hasPower,
                IsWorking = isWorking,
                AssignedSetup = assigned
            };

            CurrentData.Conveyors.Add(status);

            string details = $"Setup: {assigned}, Power: {hasPower}, Working: {isWorking}, Dirt: {currentDirt:F1}/{maxDirt:F1} m³";
            CurrentData.RawInspectionItems.Add(new RawDebugItem
            {
                Category = "Feeder / Conveyor",
                TypeName = typeName,
                GameObjectName = go.name,
                InstanceId = go.GetInstanceID(),
                Position = go.transform.position,
                Details = details
            });
        }

        private void ScanUtility(MonoBehaviour comp)
        {
            var go = comp.gameObject;
            var type = comp.GetType();
            string typeName = type.Name;

            string uType = "Utility";
            bool isWorking = true;
            float current = 0f;
            float max = 0f;
            string details = "";

            if (typeName == "WaterTowerController")
            {
                uType = "WaterTower";
                current = GetFieldValue<float>(comp, type, "CurrentCapacity");
                max = GetFieldValue<float>(comp, type, "MaxCapacity");
                isWorking = GetFieldValue<bool>(comp, type, "_isWorking");
                details = $"Water: {current:F0}/{max:F0} L, PumpRunning: {isWorking}";
            }
            else if (typeName == "WaterPumpElectric" || typeName == "WaterPumpMobile" || typeName == "WaterPumpElectricMobile")
            {
                uType = "WaterPump";
                isWorking = GetFieldValue<bool>(comp, type, "_isWorking") || GetFieldValue<bool>(comp, type, "IsWorking");
                details = $"Pump Working: {isWorking}";
            }
            else if (typeName == "PowerGenerator" || typeName == "PowerStationController")
            {
                uType = "Generator";
                isWorking = GetPropertyValue<bool>(comp, type, "HavePower") || GetFieldValue<bool>(comp, type, "isEnabled");
                details = $"Power Output: {(isWorking ? "ACTIVE" : "INACTIVE")}";
            }
            else if (typeName == "FuelStationController")
            {
                uType = "FuelStation";
                current = GetPropertyValue<float>(comp, type, "GetCurrentCapacity");
                max = GetFieldValue<float>(comp, type, "MaxCapacity");
                bool isInfinity = GetFieldValue<bool>(comp, type, "IsInfinitySource");
                details = isInfinity ? "Fuel: Infinite Source" : $"Fuel: {current:F1}/{max:F1} L";
            }

            var status = new UtilityStatus
            {
                InstanceId = go.GetInstanceID(),
                Name = go.name.Replace("(Clone)", "").Trim(),
                UtilityType = uType,
                Position = go.transform.position,
                IsWorking = isWorking,
                CurrentLevel = current,
                MaxLevel = max,
                Details = details
            };

            CurrentData.Utilities.Add(status);

            CurrentData.RawInspectionItems.Add(new RawDebugItem
            {
                Category = "Fuel",
                TypeName = typeName,
                GameObjectName = go.name,
                InstanceId = go.GetInstanceID(),
                Position = go.transform.position,
                Details = details
            });
        }

        private bool IsVehicleType(MonoBehaviour comp)
        {
            Type cur = comp.GetType();
            while (cur != null && cur != typeof(MonoBehaviour) && cur != typeof(object))
            {
                if (cur.Name == "MachineController") return true;
                cur = cur.BaseType;
            }
            return false;
        }

        private bool IsPlantMachinery(MonoBehaviour comp)
        {
            if (comp == null) return false;
            string name = comp.GetType().Name;
            string goName = comp.gameObject.name;

            if (name == "WashPlantShaker" || name == "WashplantShakerBase"
                || name == "WashPlantTrommel" || name == "WashplantTrommelBase"
                || name == "WashPlantDuplex" || name == "GravelPump"
                || name == "MobileWashplant" || name == "MiniWashplant"
                || name == "OrangeBeastWashPlantGoldCounter")
            {
                return true;
            }

            if (goName.Contains("Washplant_Shaker_Beast") || goName.Contains("OrangeBeast_Frame"))
            {
                return true;
            }

            return false;
        }

        private bool IsUtilityType(string name)
        {
            return name == "WaterTowerController" || name == "WaterPumpElectric"
                || name == "WaterPumpMobile" || name == "WaterPumpElectricMobile"
                || name == "PowerGenerator" || name == "PowerStationController"
                || name == "FuelStationController";
        }

        private bool CheckPowerState(MonoBehaviour comp)
        {
            if (comp == null) return true;

            var compType = comp.GetType();

            // 1. Direct boolean flags on component (e.g. WashplantShakerBase.IsPowerReady)
            if (GetFieldValue<bool>(comp, compType, "IsPowerReady") || GetPropertyValue<bool>(comp, compType, "IsPowerReady"))
                return true;

            // 2. Check PowerConsumer component attached directly to GameObject or parent/child
            var pc = comp.GetComponent("PowerConsumer") 
                  ?? comp.GetComponentInChildren(Type.GetType("GoldDigger.PowerConsumer, Assembly-CSharp"))
                  ?? comp.GetComponentInParent(Type.GetType("GoldDigger.PowerConsumer, Assembly-CSharp"));
            if (pc != null)
            {
                var pcType = pc.GetType();
                bool havePower = GetPropertyValue<bool>(pc, pcType, "HavePower") || GetFieldValue<bool>(pc, pcType, "_hasPower");
                object prod = GetFieldValue<object>(pc, pcType, "Producent");
                bool brokenRopes = GetFieldValue<bool>(pc, pcType, "_hasBrokenRopes");
                if (!brokenRopes && (havePower || prod != null)) return true;
                if (brokenRopes) return false;
            }

            // 3. Check fields referencing PowerConsumer
            string[] fieldNames = { "Power", "MyPower", "_powerConsumer", "_PowerConsumer", "MyPowerConsumer", "PowerConsumer" };
            foreach (var fn in fieldNames)
            {
                object obj = GetFieldValue<object>(comp, compType, fn);
                if (obj != null)
                {
                    var objType = obj.GetType();
                    if (objType.Name.Contains("PowerConsumer") || objType.Name.Contains("PowerSplitterConsumer"))
                    {
                        bool havePower = GetPropertyValue<bool>(obj, objType, "HavePower") || GetFieldValue<bool>(obj, objType, "_hasPower");
                        object prod = GetFieldValue<object>(obj, objType, "Producent");
                        bool brokenRopes = GetFieldValue<bool>(obj, objType, "_hasBrokenRopes");
                        if (!brokenRopes && (havePower || prod != null)) return true;
                        if (brokenRopes) return false;
                    }
                }
            }

            // 4. Fallback direct field on component
            bool directField = GetFieldValue<bool>(comp, compType, "_hasPower") 
                            || GetFieldValue<bool>(comp, compType, "_HasPower")
                            || GetPropertyValue<bool>(comp, compType, "HavePower");

            return directField;
        }

        private bool CheckWaterState(MonoBehaviour comp)
        {
            if (comp == null) return true;

            var compType = comp.GetType();

            // 1. Direct boolean flags on component (e.g. WashplantShakerBase.IsWaterReady)
            if (GetFieldValue<bool>(comp, compType, "IsWaterReady") || GetPropertyValue<bool>(comp, compType, "IsWaterReady"))
                return true;

            // 2. Check WaterChangePhysicsMaterial on GameObject or children/parent
            var wcpm = comp.GetComponent("WaterChangePhysicsMaterial") 
                    ?? comp.GetComponentInChildren(Type.GetType("GoldDigger.WaterChangePhysicsMaterial, Assembly-CSharp"))
                    ?? comp.GetComponentInParent(Type.GetType("GoldDigger.WaterChangePhysicsMaterial, Assembly-CSharp"));
            if (wcpm != null)
            {
                var wcpmType = wcpm.GetType();
                bool hasWater = GetPropertyValue<bool>(wcpm, wcpmType, "HasWater") || GetFieldValue<bool>(wcpm, wcpmType, "HasWater");
                if (hasWater) return true;
            }

            // 3. Check WaterConsumer component attached directly to GameObject or children/parent
            var wc = comp.GetComponent("WaterConsumer") 
                  ?? comp.GetComponentInChildren(Type.GetType("GoldDigger.WaterConsumer, Assembly-CSharp"))
                  ?? comp.GetComponentInParent(Type.GetType("GoldDigger.WaterConsumer, Assembly-CSharp"));
            if (wc != null)
            {
                var wcType = wc.GetType();
                bool haveWater = GetPropertyValue<bool>(wc, wcType, "HaveWater") || GetFieldValue<bool>(wc, wcType, "_hasWater");
                object prod = GetFieldValue<object>(wc, wcType, "Producent");
                if (haveWater || prod != null) return true;
            }

            // 4. Check fields referencing WaterConsumer
            string[] fieldNames = { "Water", "MyWater", "_waterConsumer", "MyWaterConsumer", "WaterConsumer" };
            foreach (var fn in fieldNames)
            {
                object obj = GetFieldValue<object>(comp, compType, fn);
                if (obj != null)
                {
                    var objType = obj.GetType();
                    if (objType.Name.Contains("WaterConsumer"))
                    {
                        bool haveWater = GetPropertyValue<bool>(obj, objType, "HaveWater") || GetFieldValue<bool>(obj, objType, "_hasWater");
                        object prod = GetFieldValue<object>(obj, objType, "Producent");
                        if (haveWater || prod != null) return true;
                    }
                }
            }

            // 5. Fallback direct field on component
            bool directField = GetFieldValue<bool>(comp, compType, "_hasWater") 
                            || GetFieldValue<bool>(comp, compType, "_HasWater")
                            || GetPropertyValue<bool>(comp, compType, "HaveWater");

            return directField;
        }

        private string GetMachineryDisplayName(MonoBehaviour comp, string typeName)
        {
            string goName = comp?.gameObject?.name ?? "";
            
            if (typeName == "GravelPump" || goName.Contains("GravelPump"))
                return "Gravel Pump";
            if (typeName == "WashPlantDuplex" || goName.Contains("Duplex"))
                return "Duplex Jig";
            if (typeName.Contains("Shaker"))
            {
                if (goName.Contains("Glacier") || typeName.Contains("Glacier")) return "Glacier Creek";
                if (goName.Contains("Orange") || goName.Contains("OB")) return "Orange Beast Shaker";
                return "Shaker";
            }
            if (typeName.Contains("Trommel"))
            {
                if (goName.Contains("Reinforced")) return "Reinforced Trommel";
                if (goName.Contains("Arnold")) return "Old Arnold's Trommel";
                return "Trommel";
            }
            if (typeName == "MobileWashplant")
                return "Mobile Wash Plant";
            if (typeName == "MiniWashplant")
                return "Mini Wash Plant";
            if (typeName == "OrangeBeastWashPlantGoldCounter")
                return "Orange Beast";

            return typeName;
        }

        private string NormalizeVehicleName(string goName, string typeName)
        {
            string clean = goName.Replace("(Clone)", "").Trim();
            if (clean.Contains("Koparka") || typeName == "Koparka")
                return clean.Contains("Small") ? "Small Excavator" : "Large Excavator";
            if (clean.Contains("Ladowarka") || typeName == "Ladowarka")
                return "Wheel Loader";
            if (clean.Contains("KoparkoLadowarka") || typeName == "KoparkoLadowarka")
                return "Backhoe Loader";
            if (clean.Contains("DumpTruck") || typeName == "DumpTruck")
                return "Dump Truck";
            if (clean.Contains("Doozer") || typeName.Contains("Doozer"))
                return "Bulldozer";
            if (clean.Contains("Drill") || typeName.Contains("Drill"))
                return "Drill Rig";

            return clean;
        }

        private string GetCategoryForSetup(WashPlantSetupType setup)
        {
            switch (setup)
            {
                case WashPlantSetupType.Setup1_Mobile: return "Setup 1 (Mobile)";
                case WashPlantSetupType.Setup2_Stationary: return "Setup 2 (Stationary)";
                case WashPlantSetupType.Setup3_OrangeBeast: return "Setup 3 (Beast)";
                default: return "Wash Plant";
            }
        }

        private T GetFieldValue<T>(object target, Type type, string fieldName)
        {
            Type current = type;
            while (current != null && current != typeof(MonoBehaviour) && current != typeof(object))
            {
                var field = current.GetField(fieldName, FieldFlags);
                if (field != null)
                {
                    try
                    {
                        var val = field.GetValue(target);
                        if (val is T castVal) return castVal;
                    }
                    catch { }
                }
                current = current.BaseType;
            }
            return default;
        }

        private T GetPropertyValue<T>(object target, Type type, string propertyName)
        {
            Type current = type;
            while (current != null && current != typeof(MonoBehaviour) && current != typeof(object))
            {
                var prop = current.GetProperty(propertyName, FieldFlags);
                if (prop != null && prop.CanRead && prop.GetIndexParameters().Length == 0)
                {
                    try
                    {
                        var val = prop.GetValue(target, null);
                        if (val is T castVal) return castVal;
                    }
                    catch { }
                }
                current = current.BaseType;
            }
            return default;
        }
    }
}