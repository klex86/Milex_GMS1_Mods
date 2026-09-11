using System;
using System.Collections;
using System.Reflection;
using System.Text;
using GoldDigger;
using Milex.GMS1.Core;
using Milex.GMS1.Mods.ClaimMonitor.Config;
using Milex.GMS1.Mods.ClaimMonitor.Diagnostics.Models;
using Milex.GMS1.Core.Localization;
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
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                bool isMainMenu = string.IsNullOrEmpty(sceneName) || sceneName.ToLower().Contains("menu") || sceneName.ToLower().Contains("buffor");
                bool isLoading = Singleton<LevelLoadingManager>.IsInstanced() && Singleton<LevelLoadingManager>.Instance.IsLoading();

                if (!isMainMenu && !isLoading)
                {
                    ForceScan();
                }

                float interval = Config?.ScanIntervalSeconds?.Value ?? 3.0f;
                yield return new WaitForSeconds(Mathf.Max(1.0f, interval));
            }
        }

        public void ForceScan()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName) || sceneName.ToLower().Contains("menu") || sceneName.ToLower().Contains("buffor"))
            {
                CurrentData.Reset();
                return;
            }

            if (Singleton<LevelLoadingManager>.IsInstanced() && Singleton<LevelLoadingManager>.Instance.IsLoading())
            {
                return;
            }

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
                    ScanPlantMachinery(mb, hasStationaryCenter, hasOrangeBeastCenter);
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
                Category = "Sluice Mats",
                TypeName = "MinersMoss",
                GameObjectName = comp.gameObject.name,
                InstanceId = status.InstanceId,
                Position = status.Position,
                Details = details,
                Setup = setup
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
                currentFuel = GetPropertyValue<float>(fuelController, fuelController.GetType(), "GetCurrentCapacity");
                maxFuel = GetFieldValue<float>(fuelController, fuelController.GetType(), "MaxCapacity");
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
                Category = "Vehicles",
                TypeName = type.Name,
                GameObjectName = go.name,
                InstanceId = go.GetInstanceID(),
                Position = go.transform.position,
                Details = details,
                Setup = WashPlantSetupType.None
            });
        }

        private void ScanPlantMachinery(MonoBehaviour comp, bool hasStationaryCenter, bool hasOrangeBeastCenter)
        {
            var go = comp.gameObject;
            int goId = go.GetInstanceID();

            // Prevent duplicate entries for multi-component objects
            for (int i = 0; i < CurrentData.PlantComponents.Count; i++)
            {
                if (CurrentData.PlantComponents[i].InstanceId == goId)
                    return;
            }

            var type = comp.GetType();
            string typeName = type.Name;
            string goName = go.name;

            bool isOrangeBeast = goName.Contains("Washplant_Shaker_Beast") 
                              || goName.Contains("OrangeBeast") 
                              || typeName == "OrangeBeastWashPlantGoldCounter";

            // If Orange Beast is NOT installed on this claim, ignore all Orange Beast components
            if (isOrangeBeast && !hasOrangeBeastCenter)
                return;

            // Only allow ONE Orange Beast Shaker in the list
            if (isOrangeBeast)
            {
                for (int i = 0; i < CurrentData.PlantComponents.Count; i++)
                {
                    if (CurrentData.PlantComponents[i].Setup == WashPlantSetupType.Setup3_OrangeBeast)
                        return;
                }
            }

            string displayName = GetMachineryDisplayName(comp, typeName);
            bool isWorking = true;
            bool isCritical = false;
            bool hasPower = true;
            bool hasWater = true;
            string issue = null;
            WashPlantSetupType setup = WashPlantSetupType.Setup2_Stationary;

            if (isOrangeBeast)
            {
                setup = WashPlantSetupType.Setup3_OrangeBeast;
                displayName = LocalizationManager.T("equipment.orange_beast_shaker", "Orange Beast Shaker");
                hasPower = CheckPowerState(comp);
                hasWater = CheckWaterState(comp);

                if (!hasPower)
                {
                    isWorking = false;
                    issue = LocalizationManager.T("issue.shaker.no_power", "Orange Beast Shaker has no electric power.");
                }
                else if (!hasWater)
                {
                    isWorking = false;
                    issue = LocalizationManager.T("issue.shaker.no_water", "Orange Beast Shaker has no water supply.");
                }
            }
            else if (comp is WashplantShakerBase || typeName.Contains("Shaker") || typeName == "GlacierCreek" || typeName == "DeRocker")
            {
                setup = WashPlantSetupType.Setup2_Stationary;
                bool stopped = false;
                if (comp is WashPlantShaker wps) stopped = wps.ShakerStopped;
                else stopped = GetFieldValue<bool>(comp, type, "ShakerStopped");

                hasPower = CheckPowerState(comp);
                hasWater = CheckWaterState(comp);

                if (stopped)
                {
                    isWorking = false;
                    issue = LocalizationManager.Format("issue.machinery.stopped", "{0} is stopped.", displayName);
                }
                else if (!hasPower)
                {
                    isWorking = false;
                    issue = LocalizationManager.Format("issue.machinery.no_power", "{0} has no electric power.", displayName);
                }
                else if (!hasWater)
                {
                    isWorking = false;
                    if (comp is WashplantShakerBase shaker && shaker.Water != null)
                    {
                        if (shaker.Water.Producent == null)
                            issue = LocalizationManager.Format("issue.machinery.water_hose_disconnected", "{0} water hose disconnected.", displayName);
                        else if (!shaker.Water.Producent.IsWorking)
                            issue = LocalizationManager.Format("issue.machinery.pump_off", "{0} water pump is turned off.", displayName);
                        else if (!shaker.Water.Producent.HaveWaterIn)
                            issue = LocalizationManager.Format("issue.machinery.pump_no_water", "{0} water pump has no water.", displayName);
                        else if (!shaker.Water.Producent.IsEnabled)
                            issue = LocalizationManager.Format("issue.machinery.pump_disabled", "{0} water pump is disabled.", displayName);
                        else if (GetFieldValue<bool>(shaker.Water, typeof(WaterConsumer), "_hasBrokenRopes"))
                            issue = LocalizationManager.Format("issue.machinery.hose_broken_frozen", "{0} water hose is broken / frozen!", displayName);
                        else
                            issue = LocalizationManager.Format("issue.machinery.no_water", "{0} has no water supply.", displayName);
                    }
                    else
                    {
                        issue = LocalizationManager.Format("issue.machinery.no_water", "{0} has no water supply.", displayName);
                    }
                }
            }
            else if (comp is WashplantTrommelBase || typeName.Contains("Trommel"))
            {
                if (typeName == "WashPlantMobileTrommel" || comp.name.Contains("Mobile") || goName.Contains("Mobile"))
                {
                    // Ignore internal drum of mobile wash plant!
                    return;
                }
                setup = WashPlantSetupType.Setup2_Stationary;
                bool stopped = false;
                bool chainBroken = false;
                if (comp is WashplantTrommelBase tb)
                {
                    stopped = tb.TrommelStopped;
                    chainBroken = GetFieldValue<bool>(comp, typeof(WashplantTrommelBase), "_TrommelChainDestroyed");
                }
                else
                {
                    stopped = GetFieldValue<bool>(comp, type, "TrommelStopped");
                    chainBroken = GetFieldValue<bool>(comp, type, "_TrommelChainDestroyed");
                }
                hasPower = CheckPowerState(comp);
                hasWater = true; // Trommel does not require or consume water directly

                if (chainBroken)
                {
                    isWorking = false;
                    isCritical = true;
                    issue = LocalizationManager.Format("issue.trommel.chain_broken", "{0} drive chain destroyed / broken!", displayName);
                }
                else if (stopped)
                {
                    isWorking = false;
                    issue = LocalizationManager.Format("issue.machinery.stopped", "{0} is stopped.", displayName);
                }
                else if (!hasPower)
                {
                    isWorking = false;
                    issue = LocalizationManager.Format("issue.machinery.no_power", "{0} has no electric power.", displayName);
                }
            }
            else if (comp is WashplantDuplexJigBase || comp is GravelPump || typeName.Contains("Duplex") || typeName == "GravelPump")
            {
                setup = WashPlantSetupType.Setup2_Stationary;
                bool pumpBroken = GetFieldValue<bool>(comp, comp.GetType(), "_DuplexJigBroken");
                hasPower = CheckPowerState(comp);
                hasWater = true; // Duplex Jigs and Gravel Pumps only consume electric power, not water

                // Check Buckets
                object b1 = GetFieldValue<object>(comp, comp.GetType(), "Bucket1");
                object b2 = GetFieldValue<object>(comp, comp.GetType(), "Bucket2");
                bool bucketFull = false;
                if (b1 != null)
                {
                    float vol1 = GetFieldValue<float>(b1, b1.GetType(), "GroundVolume");
                    float max1 = GetFieldValue<float>(b1, b1.GetType(), "MaxVolume");
                    if (max1 > 0f && vol1 >= max1 * 0.98f) bucketFull = true;
                }
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
                    issue = LocalizationManager.Format("issue.duplex.mechanism_broken", "{0} mechanism broken!", displayName);
                }
                else if (bucketFull)
                {
                    issue = LocalizationManager.Format("issue.duplex.bucket_full", "{0} bucket is full (replace bucket).", displayName);
                }
                else if (!hasPower)
                {
                    isWorking = false;
                    issue = LocalizationManager.Format("issue.machinery.no_power", "{0} has no electric power.", displayName);
                }
            }
            else if (comp is MobileWashplant || comp is MiniWashplant || typeName == "MobileWashplant" || typeName == "MiniWashplant")
            {
                setup = WashPlantSetupType.Setup1_Mobile;

                // 1. Water connection verification: Both mobile wash plants require a connected water supply to be active
                WaterConsumer wc = (comp is MobileWashplant m1) ? m1._WaterConsumer
                                 : (comp is MiniWashplant m2) ? m2._WaterConsumer
                                 : (comp.GetComponent<WaterConsumer>() ?? comp.GetComponentInChildren<WaterConsumer>());

                if (!IsWaterConsumerConnected(wc, comp))
                {
                    // Parked / disconnected on claim -> completely ignore in Warning HUD (no status, no alerts, no wear)
                    // But record in RawInspectionItems so diagnostic inspector (F3) clearly indicates it is disconnected
                    CurrentData.RawInspectionItems.Add(new RawDebugItem
                    {
                        Category = "Setup 1 (Mobile)",
                        TypeName = typeName,
                        GameObjectName = go.name,
                        InstanceId = goId,
                        Position = go.transform.position,
                        Details = $"Setup: Setup1_Mobile, Name: {displayName}, Connected: False (No water hose connected)",
                        Setup = WashPlantSetupType.Setup1_Mobile
                    });
                    return;
                }

                bool ready = false;
                if (comp is MobileWashplant mwp)
                {
                    ready = mwp.CheckIfIsReadyToWork() && (mwp.OnOff?.IsOn() ?? false);
                    hasPower = CheckPowerState(comp);
                    hasWater = CheckWaterState(comp);

                    if (!ready)
                    {
                        isWorking = false;
                        issue = LocalizationManager.Format("issue.machinery.turned_off", "{0} is turned off.", displayName);
                    }
                    else if (!hasWater)
                    {
                        isWorking = false;
                        issue = LocalizationManager.Format("issue.machinery.no_water_pressure", "{0} has no water pressure.", displayName);
                    }
                    else if (!hasPower)
                    {
                        isWorking = false;
                        issue = LocalizationManager.Format("issue.machinery.no_power", "{0} has no electric power.", displayName);
                    }
                }
                else if (comp is MiniWashplant minip)
                {
                    ready = minip.IsOn;
                    hasPower = true; // Internal diesel engine
                    hasWater = CheckWaterState(comp);

                    bool hasFuel = true;
                    if (minip._FuelController != null)
                    {
                        hasFuel = minip._FuelController.HaveFuel && minip._FuelController.CurrentCapacity > 0.1f;
                    }

                    if (!ready)
                    {
                        isWorking = false;
                        issue = LocalizationManager.Format("issue.machinery.turned_off", "{0} is turned off.", displayName);
                    }
                    else if (!hasFuel)
                    {
                        isWorking = false;
                        isCritical = true;
                        issue = LocalizationManager.Format("issue.miniwashplant.no_fuel", "{0} is out of fuel.", displayName);
                    }
                    else if (!hasWater)
                    {
                        isWorking = false;
                        issue = LocalizationManager.Format("issue.machinery.no_water_pressure", "{0} has no water pressure.", displayName);
                    }
                }
                else
                {
                    ready = GetFieldValue<bool>(comp, type, "IsReadyToWork") || GetFieldValue<bool>(comp, type, "IsOn");
                    hasPower = typeName == "MiniWashplant" ? true : CheckPowerState(comp);
                    hasWater = CheckWaterState(comp);

                    if (!ready)
                    {
                        isWorking = false;
                        issue = LocalizationManager.Format("issue.machinery.turned_off", "{0} is turned off.", displayName);
                    }
                    else if (!hasWater)
                    {
                        isWorking = false;
                        issue = LocalizationManager.Format("issue.machinery.no_water_pressure", "{0} has no water pressure.", displayName);
                    }
                    else if (!hasPower)
                    {
                        isWorking = false;
                        issue = LocalizationManager.Format("issue.machinery.no_power", "{0} has no electric power.", displayName);
                    }
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

            // Scan all installed wear-and-tear parts (CheckAndRepair) belonging to this machinery
            ScanEquipmentWear(go, setup, displayName);

            string details = $"Setup: {setup}, Name: {displayName}, Working: {isWorking}, Power: {hasPower}, Water: {hasWater}, Issue: {issue ?? "None"}";
            string debugCat = "Setup 1 (Mobile)";
            if (setup == WashPlantSetupType.Setup2_Stationary) debugCat = "Setup 2 (Stationary)";
            else if (setup == WashPlantSetupType.Setup3_OrangeBeast) debugCat = "Setup 3 (Orange Beast)";

            CurrentData.RawInspectionItems.Add(new RawDebugItem
            {
                Category = debugCat,
                TypeName = typeName,
                GameObjectName = go.name,
                InstanceId = goId,
                Position = go.transform.position,
                Details = details,
                Setup = setup
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

            bool isConnected = false;

            // Check PowerConsumer
            object powerConsumer = GetFieldValue<object>(comp, type, "MyPower")
                                ?? comp.GetComponent("PowerConsumer");
            if (powerConsumer != null)
            {
                var pcType = powerConsumer.GetType();
                hasPower = GetPropertyValue<bool>(powerConsumer, pcType, "HavePower")
                        || GetFieldValue<bool>(powerConsumer, pcType, "_hasPower");

                object ind = GetFieldValue<object>(powerConsumer, pcType, "PowerIndicator");
                if (CheckIndicatorActive(ind))
                {
                    hasPower = true;
                    isConnected = true;
                }
                else if (CheckIndicatorInactive(ind))
                {
                    hasPower = false;
                }
                else
                {
                    object prod = GetFieldValue<object>(powerConsumer, pcType, "Producent");
                    bool brokenRopes = GetFieldValue<bool>(powerConsumer, pcType, "_hasBrokenRopes");
                    if (prod != null)
                    {
                        isConnected = true;
                    }
                    if (prod == null || brokenRopes)
                    {
                        hasPower = false;
                    }
                    else
                    {
                        bool prodWorking = GetPropertyValue<bool>(prod, prod.GetType(), "IsWorking")
                                        || GetFieldValue<bool>(prod, prod.GetType(), "_isWorking");
                        bool prodEnabled = GetFieldValue<bool>(prod, prod.GetType(), "isEnabled");
                        bool prodOverload = GetFieldValue<bool>(prod, prod.GetType(), "IsOverLoaded");
                        hasPower = prodWorking && prodEnabled && !prodOverload;
                    }
                }

                // Check physical ropes/holders
                var ropes = GetFieldValue<System.Collections.IList>(powerConsumer, pcType, "MyRopes");
                if (ropes != null && ropes.Count > 0)
                {
                    isConnected = true;
                }
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

            // Associate with nearest setup ONLY if that setup actually exists on the claim!
            WashPlantSetupType assigned = WashPlantSetupType.Setup2_Stationary;
            bool setupExists = false;
            if (hasOb && (!hasStat || Vector3.Distance(go.transform.position, obPos) < Vector3.Distance(go.transform.position, statPos)))
            {
                assigned = WashPlantSetupType.Setup3_OrangeBeast;
                setupExists = true;
            }
            else if (hasStat)
            {
                assigned = WashPlantSetupType.Setup2_Stationary;
                setupExists = true;
            }

            // If neither stationary nor orange beast exists on claim, or unplaced world conveyor, do not treat as connected/monitored
            if (!setupExists)
            {
                isConnected = false;
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
                IsConnected = isConnected,
                AssignedSetup = assigned
            };

            CurrentData.Conveyors.Add(status);

            // Scan wear parts on conveyors (motor belt, buckets, rollers)
            bool monitorConveyor = setupExists && isConnected && ((assigned == WashPlantSetupType.Setup2_Stationary && Config.Setup2IncludeFeedingChain.Value)
                                || (assigned == WashPlantSetupType.Setup3_OrangeBeast && Config.Setup3IncludeFeedingChain.Value));
            if (monitorConveyor)
            {
                ScanEquipmentWear(go, assigned, go.name.Replace("(Clone)", "").Trim(), isConnected);
            }

            string details = $"Setup: {assigned}, Power: {hasPower}, Working: {isWorking}, Dirt: {currentDirt:F1}/{maxDirt:F1} m³";
            CurrentData.RawInspectionItems.Add(new RawDebugItem
            {
                Category = "Conveyors & Feeders",
                TypeName = typeName,
                GameObjectName = go.name,
                InstanceId = go.GetInstanceID(),
                Position = go.transform.position,
                Details = details,
                Setup = assigned
            });
        }

        private void ScanUtility(MonoBehaviour comp)
        {
            var go = comp.gameObject;
            var type = comp.GetType();
            string typeName = type.Name;

            string uType = "Utility";
            float current = 0f;
            float max = 0f;
            bool isWorking = true;
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

            bool isConnected = IsUtilityConnected(comp, uType);
            string connTag = isConnected ? " [Connected]" : " [Unused/Disconnected]";
            details += connTag;

            string uName = GetMachineryDisplayName(comp, typeName);
            if (string.IsNullOrEmpty(uName) || uName == typeName)
                uName = go.name.Replace("(Clone)", "").Trim();

            var status = new UtilityStatus
            {
                InstanceId = go.GetInstanceID(),
                Name = uName,
                UtilityType = uType,
                Position = go.transform.position,
                IsWorking = isWorking,
                IsConnected = isConnected,
                CurrentLevel = current,
                MaxLevel = max,
                Details = details
            };

            CurrentData.Utilities.Add(status);

            // Scan filters and wear parts on water pumps, towers, and generators ONLY IF CONNECTED!
            if (isConnected && (uType == "WaterPump" || uType == "WaterTower" || uType == "Generator"))
            {
                ScanEquipmentWear(go, WashPlantSetupType.None, status.Name, true);
            }

            CurrentData.RawInspectionItems.Add(new RawDebugItem
            {
                Category = "Utilities",
                TypeName = typeName,
                GameObjectName = go.name,
                InstanceId = go.GetInstanceID(),
                Position = go.transform.position,
                Details = details,
                Setup = WashPlantSetupType.None
            });
        }

        private bool IsUtilityConnected(MonoBehaviour comp, string uType)
        {
            if (comp == null) return false;

            // 1. Water Pump or Water Tower
            if (uType == "WaterPump" || uType == "WaterTower" || comp is WaterStationController)
            {
                // Specialized check for WaterTowerController (DLC / Tier 4-5)
                if (comp is WaterTowerController wtc)
                {
                    if (wtc.WaterRopeOut != null && !wtc.WaterRopeOut.IsEmpty() && wtc.WaterRopeOut.ObjectInHolder != null)
                        return true;
                    if (wtc.WaterRopeOut2 != null && !wtc.WaterRopeOut2.IsEmpty() && wtc.WaterRopeOut2.ObjectInHolder != null)
                        return true;
                    if (wtc.WaterRopeOut3 != null && !wtc.WaterRopeOut3.IsEmpty() && wtc.WaterRopeOut3.ObjectInHolder != null)
                        return true;

                    var towerConsumers = GetFieldValue<System.Collections.IList>(wtc, typeof(WaterStationController), "_WaterStationConsumerList");
                    if (towerConsumers != null && towerConsumers.Count > 0)
                        return true;

                    return false;
                }

                // Standard WaterStationController (WaterPumpElectric, WaterPumpMobile, etc.)
                if (comp is WaterStationController wsc)
                {
                    // Strict output hose connection check: ONLY in use if an output hose is plugged in
                    if (wsc.WaterRopeOut != null && !wsc.WaterRopeOut.IsEmpty() && wsc.WaterRopeOut.ObjectInHolder != null)
                        return true;

                    // Registered water consumers attached to this water station
                    var consumers = GetFieldValue<System.Collections.IList>(wsc, typeof(WaterStationController), "_WaterStationConsumerList");
                    if (consumers != null && consumers.Count > 0)
                        return true;

                    return false;
                }

                return false;
            }

            // 2. Power Generator / Power Station
            if (uType == "Generator" || comp is PowerGenerator || comp is PowerStationController)
            {
                PowerGenerator pg = comp as PowerGenerator ?? comp.GetComponent<PowerGenerator>() ?? comp.GetComponentInChildren<PowerGenerator>();
                if (pg != null)
                {
                    // Check socket plugins
                    if (pg.MySockets != null && pg.MySockets.Count > 0)
                    {
                        for (int i = 0; i < pg.MySockets.Count; i++)
                        {
                            var sock = pg.MySockets[i];
                            if (sock == null) continue;
                            if (sock.PluginHolder != null && !sock.PluginHolder.IsEmpty() && sock.PluginHolder.ObjectInHolder != null)
                                return true;
                            if (sock.MyConsumer != null)
                                return true;
                        }
                    }

                    // Check power station consumers
                    if (pg.MyPower != null)
                    {
                        var consumers = GetFieldValue<System.Collections.IList>(pg.MyPower, typeof(PowerStationController), "_PowerStationConsumerList");
                        if (consumers != null && consumers.Count > 0)
                            return true;
                    }
                }

                PowerStationController psc = comp as PowerStationController ?? comp.GetComponent<PowerStationController>() ?? comp.GetComponentInChildren<PowerStationController>();
                if (psc != null)
                {
                    var consumers = GetFieldValue<System.Collections.IList>(psc, typeof(PowerStationController), "_PowerStationConsumerList");
                    if (consumers != null && consumers.Count > 0)
                        return true;

                    if (psc.MyPowerGenerator != null && psc.MyPowerGenerator.MySockets != null)
                    {
                        for (int i = 0; i < psc.MyPowerGenerator.MySockets.Count; i++)
                        {
                            var sock = psc.MyPowerGenerator.MySockets[i];
                            if (sock == null) continue;
                            if (sock.PluginHolder != null && !sock.PluginHolder.IsEmpty() && sock.PluginHolder.ObjectInHolder != null)
                                return true;
                            if (sock.MyConsumer != null)
                                return true;
                        }
                    }
                }

                return false;
            }

            // Default for other utilities (e.g. FuelStation)
            return true;
        }

        private void ScanEquipmentWear(GameObject rootGo, WashPlantSetupType setup, string equipmentDisplayName, bool isConnected = true)
        {
            if (rootGo == null) return;

            var wearComponents = rootGo.GetComponentsInChildren<CheckAndRepair>(true);
            if (wearComponents == null || wearComponents.Length == 0) return;

            for (int i = 0; i < wearComponents.Length; i++)
            {
                var cr = wearComponents[i];
                if (cr == null) continue;

                // CRITICAL REQUIREMENT: Only consider parts that are physically installed on the machine!
                // Disassembled/abandoned parts lying loose in the claim world must be completely ignored.
                if (!cr.IsInPlace) continue;

                // Generator breaker/switch buttons: ignored by default to prevent HUD spam unless enabled in config
                bool isGenSwitchButton = cr.gameObject.name.Contains("Switch_Button")
                                      || cr.gameObject.name.Contains("Power_Generator_Switch")
                                      || (!string.IsNullOrEmpty(cr.Name) && cr.Name.Contains("SWITCH_BUTTON"));
                if (isGenSwitchButton && (Config == null || !Config.MonitorGeneratorSwitchButtons.Value))
                {
                    continue;
                }

                string rawPartName = !string.IsNullOrEmpty(cr.Name) ? cr.Name : cr.gameObject.name.Replace("(Clone)", "").Trim();
                string partName = LocalizationManager.ResolveGameText(rawPartName);

                string internalMachineName = GetFieldValue<string>(cr, typeof(CheckAndRepair), "MyMachineName");
                string rawMachine = !string.IsNullOrEmpty(equipmentDisplayName) ? equipmentDisplayName : internalMachineName;
                string machine = LocalizationManager.ResolveGameText(rawMachine);

                var wearStatus = new EquipmentWearStatus
                {
                    InstanceId = cr.GetInstanceID(),
                    PartName = partName,
                    ParentMachineName = machine,
                    Position = cr.transform.position,
                    Durability = cr.Durability,
                    IsDestroyed = cr.IsDestroyed,
                    IsInPlace = cr.IsInPlace,
                    IsConnected = isConnected,
                    Setup = setup
                };

                CurrentData.WearParts.Add(wearStatus);

                string wearCat = "Utilities";
                if (setup == WashPlantSetupType.Setup1_Mobile) wearCat = "Setup 1 (Mobile)";
                else if (setup == WashPlantSetupType.Setup2_Stationary) wearCat = "Setup 2 (Stationary)";
                else if (setup == WashPlantSetupType.Setup3_OrangeBeast) wearCat = "Setup 3 (Orange Beast)";

                CurrentData.RawInspectionItems.Add(new RawDebugItem
                {
                    Category = wearCat,
                    TypeName = "CheckAndRepair",
                    GameObjectName = cr.gameObject.name,
                    InstanceId = cr.GetInstanceID(),
                    Position = cr.transform.position,
                    Details = $"{machine} -> {partName}: Durability={cr.Durability * 100f:F0}%, Destroyed={cr.IsDestroyed}, InPlace={cr.IsInPlace}, Connected={isConnected}",
                    Setup = setup
                });
            }
        }

        private bool IsWaterConsumerConnected(WaterConsumer wc, MonoBehaviour comp = null)
        {
            if (wc != null)
            {
                if (wc.Producent != null) return true;
                if (wc.MyRopes != null && wc.MyRopes.Count > 0) return true;
                if (wc.MyRopeHolder != null && !wc.MyRopeHolder.IsEmpty() && wc.MyRopeHolder.ObjectInHolder != null)
                    return true;
                var otherRh = GetFieldValue<RopeHolder>(wc, typeof(WaterConsumer), "otherrh");
                if (otherRh != null && !otherRh.IsEmpty() && otherRh.ObjectInHolder != null)
                    return true;
            }

            if (comp != null)
            {
                var holders = comp.GetComponentsInChildren<RopeHolder>(true);
                if (holders != null)
                {
                    for (int i = 0; i < holders.Length; i++)
                    {
                        var rh = holders[i];
                        if (rh == null) continue;
                        bool isWaterSocket = rh.Type.ToString().StartsWith("WaterRope");
                        if (isWaterSocket && rh.In && !rh.IsEmpty() && rh.ObjectInHolder != null)
                            return true;
                    }
                }
            }

            return false;
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

            // Ignore internal drum of mobile wash plant
            if (name == "WashPlantMobileTrommel" || comp is WashPlantMobileTrommel || goName.Contains("MobileTrommel"))
                return false;

            if (comp is WashplantShakerBase || name == "WashPlantShaker" || name == "WashplantShakerBase"
                || comp is GlacierCreek || name == "GlacierCreek"
                || comp is DeRocker || name == "DeRocker"
                || comp is WashplantTrommelBase || name == "WashPlantTrommel" || name == "WashplantTrommelBase"
                || comp is WashplantDuplexJigBase || name == "WashPlantDuplex" || name == "WashplantDuplexJigBase"
                || comp is GravelPump || name == "GravelPump"
                || comp is MobileWashplant || name == "MobileWashplant"
                || comp is MiniWashplant || name == "MiniWashplant")
            {
                return true;
            }

            // Only recognize the actual shaker unit of the Orange Beast (not the frame or counters)
            if (goName.Contains("Washplant_Shaker_Beast"))
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
            string typeName = compType.Name;

            // Mini Wash Plant runs on fuel engine, not electricity
            if (comp is MiniWashplant || typeName == "MiniWashplant") return true;

            // 1. Direct typed check for WashplantShakerBase
            if (comp is WashplantShakerBase shaker)
            {
                if (shaker.Power != null)
                {
                    return shaker.Power.HavePower;
                }
                return GetFieldValue<bool>(comp, typeof(WashplantShakerBase), "_hasPower");
            }

            // 2. Direct typed check for WashplantTrommelBase
            if (comp is WashplantTrommelBase tb)
            {
                if (tb.Power != null)
                {
                    return tb.Power.HavePower;
                }
                return GetFieldValue<bool>(comp, typeof(WashplantTrommelBase), "_hasPower");
            }

            // 3. Direct typed check for WashplantDuplexJigBase
            if (comp is WashplantDuplexJigBase dj)
            {
                if (dj.Power != null)
                {
                    return dj.Power.HavePower;
                }
                return GetFieldValue<bool>(comp, typeof(WashplantDuplexJigBase), "_hasPower");
            }

            // 4. Direct typed check for MobileWashplant
            if (comp is MobileWashplant mwp)
            {
                if (mwp._PowerConsumer != null)
                {
                    return mwp._PowerConsumer.HavePower;
                }
                return GetFieldValue<bool>(comp, typeof(MobileWashplant), "_HasPower");
            }

            // 5. Find PowerConsumer on component or in hierarchy
            var pc = comp.GetComponent<PowerConsumer>()
                  ?? comp.GetComponentInChildren<PowerConsumer>()
                  ?? comp.GetComponentInParent<PowerConsumer>();

            if (pc != null)
            {
                return pc.HavePower;
            }

            // 6. Reflection fallback for _hasPower or HasPower
            if (GetFieldValue<bool>(comp, compType, "_hasPower") 
                || GetFieldValue<bool>(comp, compType, "_HasPower") 
                || GetFieldValue<bool>(comp, compType, "HasPower"))
            {
                return true;
            }

            return false;
        }

        private bool CheckWaterState(MonoBehaviour comp)
        {
            if (comp == null) return true;

            var compType = comp.GetType();
            string typeName = compType.Name;
            string goName = comp.gameObject.name;

            // Trommels, Duplex Jigs, and Gravel Pumps do not consume water directly
            if (comp is WashplantTrommelBase || typeName.Contains("Trommel") || goName.Contains("Trommel")
                || comp is WashplantDuplexJigBase || typeName.Contains("Duplex") || goName.Contains("Duplex")
                || comp is GravelPump || typeName == "GravelPump" || goName.Contains("GravelPump"))
            {
                return true;
            }

            // 1. Direct typed check for WashplantShakerBase (WashPlantShaker, GlacierCreek, DeRocker)
            if (comp is WashplantShakerBase shaker)
            {
                if (shaker.Water != null)
                {
                    return shaker.Water.HaveWater || shaker.Water.CheckHasWater();
                }
                return GetFieldValue<bool>(comp, typeof(WashplantShakerBase), "_hasWater");
            }

            // 2. Direct typed check for MobileWashplant
            if (comp is MobileWashplant mwp)
            {
                if (mwp._WaterConsumer != null)
                {
                    return mwp._WaterConsumer.HaveWater || mwp._WaterConsumer.CheckHasWater();
                }
                return GetFieldValue<bool>(comp, typeof(MobileWashplant), "_HasWater");
            }

            // 3. Direct typed check for MiniWashplant
            if (comp is MiniWashplant minip)
            {
                if (minip._WaterConsumer != null)
                {
                    return minip._WaterConsumer.HaveWater || minip._WaterConsumer.CheckHasWater();
                }
                return GetFieldValue<bool>(comp, typeof(MiniWashplant), "_HasWater");
            }

            // 4. Find WaterConsumer on component or in hierarchy
            var wc = comp.GetComponent<WaterConsumer>()
                  ?? comp.GetComponentInChildren<WaterConsumer>()
                  ?? comp.GetComponentInParent<WaterConsumer>();

            if (wc != null)
            {
                return wc.HaveWater || wc.CheckHasWater();
            }

            // 5. Reflection fallback for _hasWater or HasWater field/property
            if (GetFieldValue<bool>(comp, compType, "_hasWater") 
                || GetFieldValue<bool>(comp, compType, "_HasWater") 
                || GetFieldValue<bool>(comp, compType, "HasWater"))
            {
                return true;
            }

            // 6. Check WaterChangePhysicsMaterial
            var wcpm = comp.GetComponent<WaterChangePhysicsMaterial>()
                    ?? comp.GetComponentInChildren<WaterChangePhysicsMaterial>();
            if (wcpm != null)
            {
                return wcpm.HasWater;
            }

            return false;
        }

        private bool CheckIndicatorActive(object indicator)
        {
            if (indicator == null) return false;
            var indType = indicator.GetType();

            // Check LastState: 2 == Green (Active / Working)
            var lastStateField = indType.GetField("LastState", FieldFlags);
            if (lastStateField != null)
            {
                object val = lastStateField.GetValue(indicator);
                if (val != null && (int)val == 2)
                    return true;
            }

            // Check Green GameObject
            var greenField = indType.GetField("Green", FieldFlags);
            if (greenField != null)
            {
                var greenGo = greenField.GetValue(indicator) as GameObject;
                if (greenGo != null && greenGo.activeSelf)
                    return true;
            }

            return false;
        }

        private bool CheckIndicatorInactive(object indicator)
        {
            if (indicator == null) return false;
            var indType = indicator.GetType();

            // Check LastState: 0 == White (Producer Off), 1 == Gray (Disconnected), 3 == Red (Overloaded / Broken)
            var lastStateField = indType.GetField("LastState", FieldFlags);
            if (lastStateField != null)
            {
                object val = lastStateField.GetValue(indicator);
                if (val != null)
                {
                    int state = (int)val;
                    if (state == 0 || state == 1 || state == 3)
                        return true;
                }
            }

            // Check Gray GameObject
            var grayField = indType.GetField("Gray", FieldFlags);
            if (grayField != null)
            {
                var grayGo = grayField.GetValue(indicator) as GameObject;
                if (grayGo != null && grayGo.activeSelf)
                    return true;
            }

            // Check Red GameObject
            var redField = indType.GetField("Red", FieldFlags);
            if (redField != null)
            {
                var redGo = redField.GetValue(indicator) as GameObject;
                if (redGo != null && redGo.activeSelf)
                    return true;
            }

            // Check White GameObject
            var whiteField = indType.GetField("White", FieldFlags);
            if (whiteField != null)
            {
                var whiteGo = whiteField.GetValue(indicator) as GameObject;
                if (whiteGo != null && whiteGo.activeSelf)
                    return true;
            }

            return false;
        }

        private string GetMachineryDisplayName(MonoBehaviour comp, string typeName)
        {
            string goName = comp?.gameObject?.name ?? "";
            
            if (comp is GravelPump || typeName == "GravelPump" || goName.Contains("GravelPump"))
                return LocalizationManager.T("equipment.gravel_pump", "Gravel Pump");
            if (comp is WashplantDuplexJigBase || typeName == "WashPlantDuplex" || typeName == "WashplantDuplexJigBase" || goName.Contains("Duplex"))
                return LocalizationManager.T("equipment.duplex_jig", "Duplex Jig");
            if (comp is GlacierCreek || typeName == "GlacierCreek" || goName.Contains("Glacier"))
                return LocalizationManager.T("equipment.glacier_creek", "Glacier Creek");
            if (comp is DeRocker || typeName == "DeRocker" || goName.Contains("DeRocker") || goName.Contains("Rocker"))
                return LocalizationManager.T("equipment.derocker", "Derocker");
            if (comp is WashplantShakerBase || typeName.Contains("Shaker"))
            {
                if (goName.Contains("Orange") || goName.Contains("OB")) return LocalizationManager.T("equipment.orange_beast_shaker", "Orange Beast Shaker");
                return LocalizationManager.T("equipment.shaker", "Shaker");
            }
            if (comp is WashplantTrommelBase || typeName.Contains("Trommel"))
            {
                if (goName.Contains("Reinforced")) return LocalizationManager.T("equipment.reinforced_trommel", "Reinforced Trommel");
                if (goName.Contains("Arnold")) return LocalizationManager.T("equipment.arnold_trommel", "Old Arnold's Trommel");
                return LocalizationManager.T("equipment.trommel", "Trommel");
            }
            if (comp is MobileWashplant || typeName == "MobileWashplant")
                return LocalizationManager.T("equipment.mobile_plant", "Mobile Wash Plant");
            if (comp is MiniWashplant || typeName == "MiniWashplant")
                return LocalizationManager.T("equipment.mini_plant", "Mini Wash Plant");
            if (typeName == "OrangeBeastWashPlantGoldCounter")
                return LocalizationManager.T("equipment.orange_beast", "Orange Beast");
            if (typeName == "WaterPumpElectric" || goName.Contains("WaterPumpElectric"))
                return LocalizationManager.T("equipment.water_pump_electric", "Electric Water Pump");
            if (typeName == "WaterPumpMobile" || typeName == "WaterPumpElectricMobile" || goName.Contains("WaterPumpMobile"))
                return LocalizationManager.T("equipment.water_pump_mobile", "Mobile Water Pump");
            if (typeName == "WaterTowerController" || goName.Contains("WaterTower"))
                return LocalizationManager.T("equipment.water_tower", "Water Tower");
            if (typeName == "PowerGenerator" || goName.Contains("PowerGenerator") || typeName == "PowerStationController")
                return LocalizationManager.T("equipment.generator", "Power Generator");

            return typeName;
        }

        private string NormalizeVehicleName(string goName, string typeName)
        {
            string clean = goName.Replace("(Clone)", "").Trim();
            if (clean.Contains("Koparka") || typeName == "Koparka")
                return clean.Contains("Small") ? LocalizationManager.T("vehicle.small_excavator", "Small Excavator") : LocalizationManager.T("vehicle.large_excavator", "Large Excavator");
            if (clean.Contains("Ladowarka") || typeName == "Ladowarka")
                return LocalizationManager.T("vehicle.wheel_loader", "Wheel Loader");
            if (clean.Contains("KoparkoLadowarka") || typeName == "KoparkoLadowarka")
                return LocalizationManager.T("vehicle.backhoe_loader", "Backhoe Loader");
            if (clean.Contains("DumpTruck") || typeName == "DumpTruck")
                return LocalizationManager.T("vehicle.dump_truck", "Dump Truck");
            if (clean.Contains("Doozer") || typeName.Contains("Doozer"))
                return LocalizationManager.T("vehicle.bulldozer", "Bulldozer");
            if (clean.Contains("Drill") || typeName.Contains("Drill"))
                return LocalizationManager.T("vehicle.drill_rig", "Drill Rig");

            return clean;
        }

        private string GetCategoryForSetup(WashPlantSetupType setup)
        {
            switch (setup)
            {
                case WashPlantSetupType.Setup1_Mobile: return LocalizationManager.T("setup.name.mobile", "Mobile Plant");
                case WashPlantSetupType.Setup2_Stationary: return LocalizationManager.T("setup.name.stationary", "Setup T3-T5");
                case WashPlantSetupType.Setup3_OrangeBeast: return LocalizationManager.T("setup.name.orange_beast", "Orange Beast");
                default: return LocalizationManager.T("setup.name.default", "Wash Plant");
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