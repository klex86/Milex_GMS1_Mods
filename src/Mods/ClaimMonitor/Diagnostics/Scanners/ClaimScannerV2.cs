using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GoldDigger;
using Milex.GMS1.Core;
using Milex.GMS1.Core.Localization;
using Milex.GMS1.Mods.ClaimMonitor.Config;
using Milex.GMS1.Mods.ClaimMonitor.Diagnostics.Models;
using UnityEngine;

namespace Milex.GMS1.Mods.ClaimMonitor.Diagnostics.Scanners
{
    public class ClaimScannerV2 : MonoBehaviour, IClaimScanner
    {
        // ==========================================
        // Internal Tracking Contexts
        // ==========================================
        private class ModularPlantTracker
        {
            public WashPlantGoldCounter WashPlantCounter;
            public ModularWashPlantStatus Status;
            public List<Transform> SluiceGrateHoldables = new List<Transform>();
            public ConveyorHolder ConveyorHolder { get; set; }
        }

        private class StandaloneHogPanTracker
        {
            public HogPan Pan;
            public HogPanAreaStatus Status;
        }

        private class OrangeBeastTracker
        {
            public OrangeBeastWashPlantGoldCounter Beast;
            public OrangeBeastStatus Status;
        }

        public ClaimDiagnosticsDataV2 CurrentData { get; } = new ClaimDiagnosticsDataV2();

        public List<ClaimAlert> ActiveAlerts => CurrentData.ActiveAlerts;
        public int PlantCount => CurrentData.ModularPlants.Count + CurrentData.OrangeBeasts.Count;


        public int MatCount
        {
            get
            {
                int total = 0;
                foreach (var plant in CurrentData.ModularPlants) total += plant.SluiceMatsInstalled;
                foreach (var beast in CurrentData.OrangeBeasts) total += beast.InstalledMats;
                return total;
            }
        }
        public int VehicleCount => 0;

        public static ClaimScannerV2 Instance { get; private set; }
        public MonitorConfig Config { get; set; }

        private readonly List<ModularPlantTracker> _trackedModularPlants = new List<ModularPlantTracker>();
        private readonly List<StandaloneHogPanTracker> _trackedStandalonePans = new List<StandaloneHogPanTracker>();
        private readonly List<OrangeBeastTracker> _trackedBeasts = new List<OrangeBeastTracker>();
        private readonly HashSet<int> _modularHogPanInstanceIds = new HashSet<int>();

        private Coroutine _scanRoutine;
        private float _lastTopologyDiscoveryTime = -999f;
        private const float TopologyIntervalSeconds = 30f;
        private const BindingFlags FieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        private void Awake()
        {
            Instance = this;
        }

        public void StartScanning()
        {
            if (_scanRoutine != null)
                StopCoroutine(_scanRoutine);

            _scanRoutine = StartCoroutine(DiagnosticLoop());
        }

        public void StopScanning()
        {
            if (_scanRoutine != null)
            {
                StopCoroutine(_scanRoutine);
                _scanRoutine = null;
            }
        }

        public void ForceScan()
        {
            DiscoverTopology();
            PollState();
        }

        private IEnumerator DiagnosticLoop()
        {
            // Warten, bis der Ladebildschirm vollständig weg ist
            while (Singleton<LevelLoadingManager>.IsInstanced() && Singleton<LevelLoadingManager>.Instance.IsLoading())
            {
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(1.0f);
            ForceScan();

            while (true)
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                bool isMainMenu = string.IsNullOrEmpty(sceneName) || sceneName.ToLower().Contains("menu") || sceneName.ToLower().Contains("buffor");
                bool isLoading = Singleton<LevelLoadingManager>.IsInstanced() && Singleton<LevelLoadingManager>.Instance.IsLoading();

                if (!isMainMenu && !isLoading)
                {
                    // 1. Topology Discovery alle 30s oder wenn Caches leer sind
                    if (Time.time - _lastTopologyDiscoveryTime >= TopologyIntervalSeconds ||
                        (_trackedModularPlants.Count == 0 && _trackedStandalonePans.Count == 0 && _trackedBeasts.Count == 0))
                    {
                        DiscoverTopology();
                    }

                    // 2. Schnelles Polling auf bekannten Referenzen
                    PollState();
                }

                float interval = Config?.ScanIntervalSeconds?.Value ?? 2.0f;
                yield return new WaitForSeconds(Mathf.Max(0.5f, interval));
            }
        }


        // ==========================================
        // STAGE 1: Topology Discovery (Heavy Scan, infrequent)
        // ==========================================
        private void DiscoverTopology()
        {
            _lastTopologyDiscoveryTime = Time.time;
            CurrentData.Reset();

            _trackedModularPlants.Clear();
            _trackedStandalonePans.Clear();
            _trackedBeasts.Clear();
            _modularHogPanInstanceIds.Clear();

            DiscoverOrangeBeasts();
            DiscoverModularPlants();
            DiscoverStandaloneHogPans();
        }

        private void DiscoverModularPlants()
        {
            if (!(Config?.MonitorSetup2?.Value ?? true)) return;

            WashPlantGoldCounter[] counters = UnityEngine.Object.FindObjectsOfType<WashPlantGoldCounter>();
            if (counters == null || counters.Length == 0) return;

            foreach (var plant in counters)
            {
                if (plant == null) continue;

                bool hasShaker = plant.WashPlantShaker != null && plant.WashPlantShaker.gameObject.activeInHierarchy;
                bool hasTrommel = plant.Trommel != null && plant.Trommel.gameObject.activeInHierarchy;
                if (!hasShaker && !hasTrommel) continue;

                int claimId = GetFieldValue<int>(plant, plant.GetType(), "MyClaimId");
                var lotDesc = GetFieldValue<LotDescriptor>(plant, plant.GetType(), "myLot");
                string claimName = lotDesc != null ? lotDesc.Name.ToString() : $"Claim #{claimId}";

                var status = new ModularWashPlantStatus
                {
                    ClaimId = claimId,
                    ClaimName = claimName,
                    IsReadyToOperate = plant.WashplantReady
                };

                var tracker = new ModularPlantTracker
                {
                    WashPlantCounter = plant,
                    Status = status
                };

                // Conveyor Holder (Hopper & Elevator) lokalisieren und merken
                var allHolders = UnityEngine.Object.FindObjectsOfType<GoldDigger.ConveyorHolder>();
                for (int h = 0; h < allHolders.Length; h++)
                {
                    var ch = allHolders[h];
                    if (ch != null && Vector3.Distance(plant.transform.position, ch.transform.position) < 15f)
                    {
                        tracker.ConveyorHolder = ch;
                        break;
                    }
                }

                // Grate-Transforms auf Sluice Box 3 lokalisieren und cachen
                if (plant.SluiceBox3 != null && plant.SluiceBox3.ObjectInHolder != null)
                {
                    var cratesRoot = plant.SluiceBox3.ObjectInHolder.gameObject.transform.Find("SluiceCrates");
                    if (cratesRoot != null)
                    {
                        for (int i = 0; i < cratesRoot.childCount; i++)
                        {
                            var holderTr = cratesRoot.GetChild(i);
                            if (holderTr != null && holderTr.gameObject.activeInHierarchy && holderTr.name.Contains("SluiceCrateHolder"))
                            {
                                var holdableTr = holderTr.Find("SluiceCrateHoldable");
                                if (holdableTr != null)
                                {
                                    tracker.SluiceGrateHoldables.Add(holdableTr);
                                }
                            }
                        }
                    }
                }

                // Registriere modulare End-HogPans
                if (plant.MyHogPan != null) _modularHogPanInstanceIds.Add(plant.MyHogPan.GetInstanceID());
                if (plant.MyHogPan2 != null) _modularHogPanInstanceIds.Add(plant.MyHogPan2.GetInstanceID());

                _trackedModularPlants.Add(tracker);
                CurrentData.ModularPlants.Add(status);
            }
        }

        private void DiscoverStandaloneHogPans()
        {
            if (!(Config?.MonitorSetup1?.Value ?? true)) return;

            HogPan[] pans = UnityEngine.Object.FindObjectsOfType<HogPan>();
            if (pans == null || pans.Length == 0) return;

            int areaCounter = 1;
            for (int i = 0; i < pans.Length; i++)
            {
                var pan = pans[i];
                if (pan == null || !pan.gameObject.activeInHierarchy) continue;
                if (_modularHogPanInstanceIds.Contains(pan.GetInstanceID())) continue;
                if (pan.MinerMoss == null || pan.MinerMoss.Count == 0) continue;

                int claimId = GetFieldValue<int>(pan, pan.GetType(), "MyClaimId");
                var lotDesc = GetFieldValue<LotDescriptor>(pan, pan.GetType(), "myLot");
                string claimName = lotDesc != null ? lotDesc.Name.ToString() : $"Claim #{claimId}";

                bool requiresWater = pan.DirtBox != null && pan.DirtBox.MyWaterConsumer != null;

                var status = new HogPanAreaStatus
                {
                    ClaimId = claimId,
                    ClaimName = claimName,
                    AreaIndex = areaCounter++,
                    IsMounted = true,
                    RequiresWater = requiresWater,
                    TotalMats = pan.MinerMoss.Count
                };

                _trackedStandalonePans.Add(new StandaloneHogPanTracker
                {
                    Pan = pan,
                    Status = status
                });
                CurrentData.HogPanAreas.Add(status);
            }
        }

        private void DiscoverOrangeBeasts()
        {
            if (!(Config?.MonitorSetup3?.Value ?? true)) return;

            OrangeBeastWashPlantGoldCounter[] beasts = UnityEngine.Object.FindObjectsOfType<OrangeBeastWashPlantGoldCounter>();
            if (beasts == null || beasts.Length == 0) return;

            int beastCounter = 1;
            foreach (var beast in beasts)
            {
                if (beast == null || !beast.gameObject.activeInHierarchy) continue;

                WashplantShakerBase shaker = beast.WashPlantShaker;
                if (shaker == null || !shaker.gameObject.activeInHierarchy) continue;

                int claimId = GetFieldValue<int>(beast, beast.GetType(), "MyClaimId");
                var lotDesc = GetFieldValue<LotDescriptor>(beast, beast.GetType(), "myLot");
                string claimName = lotDesc != null ? lotDesc.Name.ToString() : $"Claim #{claimId}";

                var status = new OrangeBeastStatus
                {
                    ClaimId = claimId,
                    ClaimName = claimName,
                    BeastIndex = beastCounter++
                };

                _trackedBeasts.Add(new OrangeBeastTracker
                {
                    Beast = beast,
                    Status = status
                });
                CurrentData.OrangeBeasts.Add(status);
            }
        }


        // ==========================================
        // Setup 3: Modular Wash Plant (T3 - T5)
        // ==========================================
        private void ScanModularWashPlants()
        {
            if (!(Config?.MonitorSetup2?.Value ?? true)) return;

            WashPlantGoldCounter[] counters = UnityEngine.Object.FindObjectsOfType<WashPlantGoldCounter>();
            if (counters == null || counters.Length == 0) return;

            foreach (var plant in counters)
            {
                if (plant == null) continue;

                bool hasShaker = plant.WashPlantShaker != null && plant.WashPlantShaker.gameObject.activeInHierarchy;
                bool hasTrommel = plant.Trommel != null && plant.Trommel.gameObject.activeInHierarchy;

                // Active installation requires at least a Shaker or a Trommel mounted
                if (!hasShaker && !hasTrommel) continue;

                int claimId = GetFieldValue<int>(plant, plant.GetType(), "MyClaimId");
                var lotDesc = GetFieldValue<LotDescriptor>(plant, plant.GetType(), "myLot");
                string claimName = lotDesc != null ? lotDesc.Name.ToString() : $"Claim #{claimId}";

                var status = new ModularWashPlantStatus
                {
                    ClaimId = claimId,
                    ClaimName = claimName,
                    IsReadyToOperate = plant.WashplantReady
                };

                // 1. Shaker Module (Requires Power and Water)
                if (hasShaker)
                {
                    var s = plant.WashPlantShaker;
                    status.ShakerMounted = true;
                    status.ShakerVariant = s.GetType().Name;
                    status.ShakerHasPower = GetFieldValue<bool>(s, s.GetType(), "_hasPower");
                    status.ShakerHasWater = GetFieldValue<bool>(s, s.GetType(), "_hasWater");
                    status.ShakerReady = s.IsReadyToWork;
                    CollectCheckAndRepairParts(s, "Shaker", status.Parts);
                }

                // 2. Trommel Module (Requires Power only)
                if (hasTrommel)
                {
                    var t = plant.Trommel;
                    status.TrommelMounted = true;
                    status.TrommelVariant = t.GetType().Name;
                    status.TrommelHasPower = GetFieldValue<bool>(t, t.GetType(), "_hasPower");
                    status.TrommelReady = t.IsReadyToWork;
                    CollectCheckAndRepairParts(t, "Trommel", status.Parts);
                }

                // Sluice Box 3: Nugget & Diamond Grates
                if (plant.SluiceBox3 != null && plant.SluiceBox3.ObjectInHolder != null && plant.SluiceBox3.ObjectInHolder.gameObject.activeInHierarchy)
                {
                    status.SluiceBox3Mounted = true;
                    var boxObj = plant.SluiceBox3.ObjectInHolder.gameObject;
                    var cratesRoot = boxObj.transform.Find("SluiceCrates");

                    int totalGrates = 0;
                    int installedGrates = 0;

                    if (cratesRoot != null)
                    {
                        for (int i = 0; i < cratesRoot.childCount; i++)
                        {
                            var holderTr = cratesRoot.GetChild(i);

                            // Nur aktive Halter berücksichtigen (SluiceCrateHolder3 bis 6)
                            if (holderTr != null && holderTr.gameObject.activeInHierarchy && holderTr.name.Contains("SluiceCrateHolder"))
                            {
                                totalGrates++;

                                var holdableTr = holderTr.Find("SluiceCrateHoldable");
                                if (holdableTr != null && holdableTr.gameObject.activeInHierarchy)
                                {
                                    installedGrates++;
                                }
                            }
                        }
                    }

                    status.SluiceGratesTotal = totalGrates;
                    status.SluiceGratesInstalled = installedGrates;
                }
                // 3. Duplex Jig 1 (Requires Power and Bucket)
                if (plant.WashPlantDuplex != null && plant.WashPlantDuplex.gameObject.activeInHierarchy)
                {
                    var j1 = plant.WashPlantDuplex;
                    status.Jig1Mounted = true;
                    status.Jig1HasPower = GetJigPower(j1);
                    ScanJigBucket(j1, status.Jig1Bucket);
                    CollectCheckAndRepairParts(j1, "Jig 1", status.Parts);
                }

                // 4. Duplex Jig 2 (Requires Power and Bucket)
                if (plant.WashPlantDuplex2 != null && plant.WashPlantDuplex2.gameObject.activeInHierarchy)
                {
                    var j2 = plant.WashPlantDuplex2;
                    status.Jig2Mounted = true;
                    status.Jig2HasPower = GetJigPower(j2);
                    ScanJigBucket(j2, status.Jig2Bucket);
                    CollectCheckAndRepairParts(j2, "Jig 2", status.Parts);
                }

                // 5. Sluice End HogPan 1 (Requires Water and Mats)
                if (plant.MyHogPan != null && plant.MyHogPan.gameObject.activeInHierarchy)
                {
                    _modularHogPanInstanceIds.Add(plant.MyHogPan.GetInstanceID());

                    status.EndHogPan1Mounted = true;
                    status.EndHogPan1HasWater = plant.MyHogPan.DirtBox?.MyWaterConsumer?.HaveWater ?? false;
                    var (total, installed) = CountHogPanMats(plant.MyHogPan);
                    status.EndHogPan1MatsTotal = total;
                    status.EndHogPan1MatsInstalled = installed;
                }

                // 6. Sluice End HogPan 2 (Requires Water and Mats)
                if (plant.MyHogPan2 != null && plant.MyHogPan2.gameObject.activeInHierarchy)
                {
                    _modularHogPanInstanceIds.Add(plant.MyHogPan.GetInstanceID());

                    status.EndHogPan2Mounted = true;
                    status.EndHogPan2HasWater = plant.MyHogPan2.DirtBox?.MyWaterConsumer?.HaveWater ?? false;
                    var (total, installed) = CountHogPanMats(plant.MyHogPan2);
                    status.EndHogPan2MatsTotal = total;
                    status.EndHogPan2MatsInstalled = installed;
                }

                // 7. Main Sluice Boxes (MinerMoss & MinerGrille)
                var mats = CountHolders(plant.MinerMoss);
                status.SluiceMatsTotal = mats.total;
                status.SluiceMatsInstalled = mats.installed;

                var grilles = CountHolders(plant.MinerGrille);
                status.SluiceGrillesTotal = grilles.total;
                status.SluiceGrillesInstalled = grilles.installed;

                CurrentData.ModularPlants.Add(status);
            }
        }


        // ==========================================
        // Setup 4: Orange Beast (T6)
        // ==========================================
        private void ScanOrangeBeastPlants()
        {
            if (!(Config?.MonitorSetup3?.Value ?? true)) return;

            OrangeBeastWashPlantGoldCounter[] beasts = UnityEngine.Object.FindObjectsOfType<OrangeBeastWashPlantGoldCounter>();
            if (beasts == null || beasts.Length == 0) return;

            int beastCounter = 1;
            foreach (var plant in beasts)
            {
                if (plant == null || !plant.gameObject.activeInHierarchy) continue;
                WashplantShakerBase shaker = plant.WashPlantShaker;
                if (shaker == null || !shaker.gameObject.activeInHierarchy) continue;

                int claimId = GetFieldValue<int>(plant, plant.GetType(), "MyClaimId");
                var lotDesc = GetFieldValue<LotDescriptor>(plant, plant.GetType(), "myLot");
                string claimName = lotDesc != null ? lotDesc.Name.ToString() : $"Claim #{claimId}";

                var status = new OrangeBeastStatus
                {
                    ClaimId = claimId,
                    ClaimName = claimName,
                    BeastIndex = beastCounter++,
                    IsReadyToOperate = plant.WashplantReady,
                    HasPower = GetFieldValue<bool>(shaker, shaker.GetType(), "_hasPower"),
                    HasWater = GetFieldValue<bool>(shaker, shaker.GetType(), "_hasWater"),
                    IsPowerReady = shaker.IsPowerReady,
                    IsWaterReady = shaker.IsWaterReady,
                    IsReadyToWork = shaker.IsReadyToWork
                };

                // Mats & Grilles
                var mats = CountHolders(plant.MinerMoss);
                status.TotalMats = mats.total;
                status.InstalledMats = mats.installed;

                var grilles = CountHolders(plant.MinerGrille);
                status.TotalGrilles = grilles.total;
                status.InstalledGrilles = grilles.installed;

                // Shaker wear parts
                CollectCheckAndRepairParts(shaker, "Beast Shaker", status.Parts);

                CurrentData.OrangeBeasts.Add(status);
            }
        }

        // ==========================================
        // STAGE 2: State Polling (Ultra-fast, frequent)
        // ==========================================
        private void PollState()
        {
            PollModularPlants();
            PollStandaloneHogPans();
            PollOrangeBeasts();

            CurrentData.CompileAlerts(Config);
        }

        private void PollModularPlants()
        {
            for (int i = 0; i < _trackedModularPlants.Count; i++)
            {
                var tracker = _trackedModularPlants[i];
                var plant = tracker.WashPlantCounter;
                var status = tracker.Status;

                if (plant == null || !plant.gameObject.activeInHierarchy)
                {
                    status.IsReadyToOperate = false;
                    continue;
                }

                status.IsReadyToOperate = plant.WashplantReady;

                // 1. Shaker
                if (plant.WashPlantShaker != null && plant.WashPlantShaker.gameObject.activeInHierarchy)
                {
                    var s = plant.WashPlantShaker;
                    status.ShakerMounted = true;
                    status.ShakerVariant = s.GetType().Name;
                    status.ShakerHasPower = GetFieldValue<bool>(s, s.GetType(), "_hasPower");
                    status.ShakerHasWater = GetFieldValue<bool>(s, s.GetType(), "_hasWater");
                    status.ShakerReady = s.IsReadyToWork;

                    status.Parts.Clear();
                    CollectCheckAndRepairParts(s, "Shaker", status.Parts);
                }
                else
                {
                    status.ShakerMounted = false;
                }

                // 2. Trommel
                if (plant.Trommel != null && plant.Trommel.gameObject.activeInHierarchy)
                {
                    var t = plant.Trommel;
                    status.TrommelMounted = true;
                    status.TrommelVariant = t.GetType().Name;
                    status.TrommelHasPower = GetFieldValue<bool>(t, t.GetType(), "_hasPower");
                    status.TrommelReady = t.IsReadyToWork;

                    CollectCheckAndRepairParts(t, "Trommel", status.Parts);
                }
                else
                {
                    status.TrommelMounted = false;
                }

                // 3. Sluice Box 3 Grates
                if (plant.SluiceBox3 != null && plant.SluiceBox3.ObjectInHolder != null && plant.SluiceBox3.ObjectInHolder.gameObject.activeInHierarchy)
                {
                    status.SluiceBox3Mounted = true;
                    status.SluiceGratesTotal = tracker.SluiceGrateHoldables.Count;
                    int installed = 0;
                    for (int g = 0; g < tracker.SluiceGrateHoldables.Count; g++)
                    {
                        var holdable = tracker.SluiceGrateHoldables[g];
                        if (holdable != null && holdable.gameObject.activeInHierarchy) installed++;
                    }
                    status.SluiceGratesInstalled = installed;
                }
                else
                {
                    status.SluiceBox3Mounted = false;
                    status.SluiceGratesInstalled = 0;
                }

                // 4. Jigs
                if (plant.WashPlantDuplex != null && plant.WashPlantDuplex.gameObject.activeInHierarchy)
                {
                    var j1 = plant.WashPlantDuplex;
                    status.Jig1Mounted = true;
                    status.Jig1HasPower = GetJigPower(j1);
                    ScanJigBucket(j1, status.Jig1Bucket);
                    CollectCheckAndRepairParts(j1, "Jig 1", status.Parts);
                }
                else
                {
                    status.Jig1Mounted = false;
                }


                if (plant.WashPlantDuplex2 != null && plant.WashPlantDuplex2.gameObject.activeInHierarchy)
                {
                    var j2 = plant.WashPlantDuplex2;
                    status.Jig2Mounted = true;
                    status.Jig2HasPower = GetJigPower(j2);
                    ScanJigBucket(j2, status.Jig2Bucket);
                    CollectCheckAndRepairParts(j2, "Jig 2", status.Parts);
                }
                else
                {
                    status.Jig2Mounted = false;
                }

                // 5. End HogPans

                if (plant.MyHogPan != null && plant.MyHogPan.gameObject.activeInHierarchy)
                {
                    status.EndHogPan1Mounted = true;
                    status.EndHogPan1HasWater = plant.MyHogPan.DirtBox?.MyWaterConsumer?.HaveWater ?? false;
                    var (total, installed) = CountHogPanMats(plant.MyHogPan);
                    status.EndHogPan1MatsTotal = total;
                    status.EndHogPan1MatsInstalled = installed;
                }
                else
                {
                    status.EndHogPan1Mounted = false;
                }


                if (plant.MyHogPan2 != null && plant.MyHogPan2.gameObject.activeInHierarchy)
                {
                    status.EndHogPan2Mounted = true;
                    status.EndHogPan2HasWater = plant.MyHogPan2.DirtBox?.MyWaterConsumer?.HaveWater ?? false;
                    var (total, installed) = CountHogPanMats(plant.MyHogPan2);
                    status.EndHogPan2MatsTotal = total;
                    status.EndHogPan2MatsInstalled = installed;
                }
                else
                {
                    status.EndHogPan2Mounted = false;
                }

                // 6. Main Mats & Grilles
                var mats = CountHolders(plant.MinerMoss);
                status.SluiceMatsTotal = mats.total;
                status.SluiceMatsInstalled = mats.installed;

                var grilles = CountHolders(plant.MinerGrille);
                status.SluiceGrillesTotal = grilles.total;
                status.SluiceGrillesInstalled = grilles.installed;

                // 8. Feeding Chain (Hopper & Conveyor Elevator)
                if (tracker.ConveyorHolder != null && tracker.ConveyorHolder.gameObject.activeInHierarchy)
                {
                    var chain = status.FeedingChain;
                    var beltHolder = tracker.ConveyorHolder.ConveyorBelt;
                    var elevHolder = tracker.ConveyorHolder.ConveyorElevator;

                    // Hopper / Ground Belt
                    bool hopperMounted = beltHolder?.ObjectInHolder != null && beltHolder.ObjectInHolder.gameObject.activeInHierarchy;
                    chain.HopperMounted = hopperMounted;

                    // Elevator
                    bool elevMounted = elevHolder?.ObjectInHolder != null && elevHolder.ObjectInHolder.gameObject.activeInHierarchy;
                    chain.ConveyorBeltMounted = elevMounted;

                    // Power Checks & Wear Parts
                    chain.Parts.Clear();
                    bool hopperPower = false;
                    bool elevPower = false;

                    // 1. Hopper: Strom & Riemen
                    if (hopperMounted)
                    {
                        var ground = beltHolder.ObjectInHolder.GetComponent<GoldDigger.ConveyorGround>();
                        if (ground != null)
                        {
                            var pc = ground.GetComponent<GoldDigger.PowerConsumer>();
                            hopperPower = pc != null && GetFieldValue<bool>(pc, typeof(GoldDigger.PowerConsumer), "_hasPower");

                            // Riemen aus EngineBelt auslesen
                            var belt = GetFieldValue<object>(ground, typeof(GoldDigger.ConveyorGround), "EngineBelt");
                            if (belt != null)
                            {
                                AddCheckAndRepairPart(belt, "Hopper", chain.Parts);
                            }
                        }
                    }

                    // 2. Elevator: Strom & die 4 Becher
                    if (elevMounted)
                    {
                        var elev = elevHolder.ObjectInHolder.GetComponent<GoldDigger.ConveyorElevator>();
                        if (elev != null)
                        {
                            elevPower = GetFieldValue<bool>(elev, typeof(GoldDigger.ConveyorElevator), "_hasPower");

                            // Die Becher mit MyCheckAndRepair aus MyBuckets auslesen
                            var buckets = GetFieldValue<System.Collections.IList>(elev, typeof(GoldDigger.ConveyorElevator), "MyBuckets");
                            if (buckets != null)
                            {
                                for (int b = 0; b < buckets.Count; b++)
                                {
                                    var bucket = buckets[b];
                                    if (bucket == null) continue;

                                    var cr = GetFieldValue<object>(bucket, bucket.GetType(), "MyCheckAndRepair");
                                    if (cr != null)
                                    {
                                        AddCheckAndRepairPart(cr, $"Elevator Bucket #{b + 1}", chain.Parts);
                                    }
                                }
                            }
                        }
                    }

                    // Die Kette gilt als mit Strom versorgt, wenn alle montierten Elemente Strom haben
                    chain.HopperHasPower = hopperPower;
                    chain.ConveyorBeltHasPower = elevPower;
                    chain.HasPower = (!hopperMounted || hopperPower) && (!elevMounted || elevPower);
                    chain.IsReadyToWork = chain.HasPower && (hopperMounted || elevMounted);
                }
                else
                {
                    status.FeedingChain.HopperMounted = false;
                    status.FeedingChain.ConveyorBeltMounted = false;
                    status.FeedingChain.HopperHasPower = false;
                    status.FeedingChain.ConveyorBeltHasPower = false;
                    status.FeedingChain.HasPower = false;
                    status.FeedingChain.IsReadyToWork = false;
                    status.FeedingChain.Parts.Clear();
                }
            }
        }

        private void PollStandaloneHogPans()
        {
            for (int i = 0; i < _trackedStandalonePans.Count; i++)
            {
                var tracker = _trackedStandalonePans[i];
                var pan = tracker.Pan;
                var status = tracker.Status;

                if (pan == null || !pan.gameObject.activeInHierarchy)
                {
                    status.IsMounted = false;
                    continue;
                }

                status.IsMounted = true;
                status.HasWater = status.RequiresWater && (pan.DirtBox?.MyWaterConsumer?.HaveWater ?? false);

                var (total, installed) = CountHogPanMats(pan);
                status.TotalMats = total;
                status.InstalledMats = installed;
            }
        }

        private void PollOrangeBeasts()
        {
            for (int i = 0; i < _trackedBeasts.Count; i++)
            {
                var tracker = _trackedBeasts[i];
                var beast = tracker.Beast;
                var status = tracker.Status;

                if (beast == null || !beast.gameObject.activeInHierarchy)
                {
                    status.IsReadyToOperate = false;
                    continue;
                }

                WashplantShakerBase shaker = beast.WashPlantShaker;
                if (shaker != null && shaker.gameObject.activeInHierarchy)
                {
                    status.IsReadyToOperate = beast.WashplantReady;
                    status.HasPower = GetFieldValue<bool>(shaker, shaker.GetType(), "_hasPower");
                    status.HasWater = GetFieldValue<bool>(shaker, shaker.GetType(), "_hasWater");
                    status.IsPowerReady = shaker.IsPowerReady;
                    status.IsWaterReady = shaker.IsWaterReady;
                    status.IsReadyToWork = shaker.IsReadyToWork;

                    status.Parts.Clear();
                    CollectCheckAndRepairParts(shaker, "Beast Shaker", status.Parts);
                }

                var mats = CountHolders(beast.MinerMoss);
                status.TotalMats = mats.total;
                status.InstalledMats = mats.installed;

                var grilles = CountHolders(beast.MinerGrille);
                status.TotalGrilles = grilles.total;
                status.InstalledGrilles = grilles.installed;
            }
        }

        // ==========================================
        // Helper Routines
        // ==========================================
        private void ScanJigBucket(WashplantDuplexJigBase jig, JigBucketStatus outBucket)
        {
            if (jig == null || outBucket == null) return;

            var gravelPump = jig as GravelPump;
            var bucket = gravelPump?.Bucket1;

            if (bucket != null)
            {
                outBucket.IsMounted = bucket.IsMounted;
                outBucket.CurrentVolumeM3 = bucket.CurrentVolumeM3;
                outBucket.FillPct = bucket.FillPct;
            }
            else
            {
                outBucket.IsMounted = false;
                outBucket.CurrentVolumeM3 = 0f;
                outBucket.FillPct = 0f;
            }
        }

        // Helper routine to extract power state from WashplantDuplexJigBase -> PowerConsumer -> _hasPower
        private bool GetJigPower(WashplantDuplexJigBase jig)
        {
            if (jig == null) return false;

            var powerConsumer = GetFieldValue<GoldDigger.PowerConsumer>(jig, typeof(WashplantDuplexJigBase), "Power")
                            ?? GetPropertyValue<GoldDigger.PowerConsumer>(jig, typeof(WashplantDuplexJigBase), "Power");

            if (powerConsumer == null) return false;

            return GetFieldValue<bool>(powerConsumer, typeof(GoldDigger.PowerConsumer), "_hasPower");
        }

        private (int total, int installed) CountHogPanMats(HogPan hogPan)
        {
            if (hogPan == null || hogPan.MinerMoss == null) return (0, 0);

            int total = hogPan.MinerMoss.Count;
            int installed = 0;
            for (int i = 0; i < hogPan.MinerMoss.Count; i++)
            {
                var moss = hogPan.MinerMoss[i];
                if (moss != null && moss.gameObject.activeInHierarchy)
                {
                    installed++;
                }
            }

            return (total, installed);
        }

        private (int total, int installed) CountHolders(List<RepairHolder> holders)
        {
            if (holders == null) return (0, 0);

            int total = holders.Count;
            int installed = 0;
            for (int i = 0; i < holders.Count; i++)
            {
                RepairHolder holder = holders[i];
                if (holder != null && holder.ObjectInHolder != null && holder.ObjectInHolder.gameObject.activeInHierarchy)
                    installed++;
            }
            return (total, installed);
        }

        private void CollectCheckAndRepairParts(MonoBehaviour module, string componentTag, List<MachinePartStatus> outParts)
        {
            if (module == null || outParts == null) return;

            var partsArray = GetFieldValue<Array>(module, module.GetType(), "MyCheckAndRepair")
                          ?? GetPropertyValue<Array>(module, module.GetType(), "MyCheckAndRepair");

            if (partsArray == null) return;

            for (int i = 0; i < partsArray.Length; i++)
            {
                object part = partsArray.GetValue(i);
                if (part == null) continue;

                Type pType = part.GetType();
                string name = GetPropertyValue<string>(part, pType, "Name") ?? GetFieldValue<string>(part, pType, "Name") ?? "Unknown";
                string prefabName = GetPropertyValue<string>(part, pType, "OriginalPrefabName") ?? GetFieldValue<string>(part, pType, "OriginalPrefabName") ?? name;
                bool inPlace = GetPropertyValue<bool>(part, pType, "IsInPlace") || GetFieldValue<bool>(part, pType, "IsInPlace");
                bool destroyed = GetPropertyValue<bool>(part, pType, "IsDestroyed") || GetFieldValue<bool>(part, pType, "IsDestroyed");
                float durability = GetPropertyValue<float>(part, pType, "Durability");
                if (durability == 0f) durability = GetFieldValue<float>(part, pType, "Durability");
                bool reinforced = GetPropertyValue<bool>(part, pType, "IsReinforced") || GetFieldValue<bool>(part, pType, "IsReinforced");

                outParts.Add(new MachinePartStatus
                {
                    Name = name,
                    PrefabName = prefabName,
                    SourceComponent = componentTag,
                    IsInPlace = inPlace,
                    IsDestroyed = destroyed,
                    Durability = durability,
                    IsReinforced = reinforced,
                    Category = MapPartCategory(prefabName, name)
                });
            }
        }

        private void AddCheckAndRepairPart(object part, string componentTag, List<MachinePartStatus> outParts)
        {
            if (part == null || outParts == null) return;

            Type pType = part.GetType();
            string name = GetPropertyValue<string>(part, pType, "Name") ?? GetFieldValue<string>(part, pType, "Name") ?? "Unknown";
            string prefabName = GetPropertyValue<string>(part, pType, "OriginalPrefabName") ?? GetFieldValue<string>(part, pType, "OriginalPrefabName") ?? name;
            bool inPlace = GetPropertyValue<bool>(part, pType, "IsInPlace") || GetFieldValue<bool>(part, pType, "IsInPlace");
            bool destroyed = GetPropertyValue<bool>(part, pType, "IsDestroyed") || GetFieldValue<bool>(part, pType, "IsDestroyed");

            float durability = GetPropertyValue<float>(part, pType, "Durability");
            if (durability == 0f) durability = GetFieldValue<float>(part, pType, "Durability");

            bool reinforced = GetPropertyValue<bool>(part, pType, "IsReinforced") || GetFieldValue<bool>(part, pType, "IsReinforced");

            outParts.Add(new MachinePartStatus
            {
                Name = name,
                PrefabName = prefabName,
                SourceComponent = componentTag,
                IsInPlace = inPlace,
                IsDestroyed = destroyed,
                Durability = durability,
                IsReinforced = reinforced,
                Category = MapPartCategory(prefabName, name)
            });
        }

        private static MachinePartCategory MapPartCategory(string prefabName, string displayName)
        {
            string p = prefabName ?? string.Empty;
            string d = displayName ?? string.Empty;

            if (p.IndexOf("Spring", StringComparison.OrdinalIgnoreCase) >= 0 ||
                d.IndexOf("SPRING", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MachinePartCategory.Spring;
            }

            if (p.IndexOf("Hose", StringComparison.OrdinalIgnoreCase) >= 0 ||
                d.IndexOf("HOSE", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MachinePartCategory.HydraulicHose;
            }

            if (p.IndexOf("Engine", StringComparison.OrdinalIgnoreCase) >= 0 ||
                d.IndexOf("ENGINE", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MachinePartCategory.Engine;
            }

            if (p.IndexOf("Belt", StringComparison.OrdinalIgnoreCase) >= 0 ||
                d.IndexOf("BELT", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MachinePartCategory.DriveBelt;
            }

            if (p.IndexOf("Bucket", StringComparison.OrdinalIgnoreCase) >= 0 ||
                d.IndexOf("BUCKET", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MachinePartCategory.ConveyorBucket;
            }

            if (p.IndexOf("Button", StringComparison.OrdinalIgnoreCase) >= 0 ||
                d.IndexOf("SWITCH", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MachinePartCategory.SwitchButton;
            }

            return MachinePartCategory.Other;
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