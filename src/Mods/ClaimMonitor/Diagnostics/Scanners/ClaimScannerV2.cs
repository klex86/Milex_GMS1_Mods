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

            public List<WashPlantSluiceBoxDirt> CachedDirtComponents = new List<WashPlantSluiceBoxDirt>();
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
            public ConveyorHolder ConveyorHolder { get; set; }
        }

        private class MobilePlantTracker
        {
            public GoldDigger.MobileWashplant Plant;
            public MobileWashPlantStatus Status;
        }

        private class MiniPlantTracker
        {
            public GoldDigger.MiniWashplant Plant;
            public MobileWashPlantStatus Status;
        }

        private class VehicleTracker
        {
            public MachineController Machine;
            public FrankensteinBelt Belt;
            public VehicleStatus Status;
        }

        public class ClaimTracker
        {
            public int ClaimId;
            public string Name;
            public string DisplayName;
            public ParcelTypeSO ParcelType;

            public Vector3 Center2D;
            public float RadiusSqr;
            public bool IsTown;

            public bool Contains(Vector3 playerPos)
            {
                float dx = playerPos.x - Center2D.x;
                float dz = playerPos.z - Center2D.z;
                return (dx * dx + dz * dz) <= RadiusSqr;
            }

            public void Reset()
            {
                ClaimId = -1;
                Name = string.Empty;
                DisplayName = string.Empty;
                ParcelType = null;
                Center2D = Vector3.zero;
                RadiusSqr = 0f;
                IsTown = false;
            }
        }


        public ClaimDiagnosticsDataV2 CurrentData { get; } = new ClaimDiagnosticsDataV2();

        public List<ClaimAlert> ActiveAlerts => CurrentData.ActiveAlerts;
        public int PlantCount => CurrentData.ModularPlants.Count + CurrentData.OrangeBeasts.Count + CurrentData.MobilePlants.Count;

        // Interner Arbeitspuffer für die Scans im Hintergrund
        private ClaimDiagnosticsDataV2 _workingData = new ClaimDiagnosticsDataV2();


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
        public int VehicleCount => CurrentData.Vehicles.Count;

        public static ClaimScannerV2 Instance { get; private set; }
        public MonitorConfig Config { get; set; }

        private readonly List<ModularPlantTracker> _trackedModularPlants = new List<ModularPlantTracker>();
        private readonly List<StandaloneHogPanTracker> _trackedStandalonePans = new List<StandaloneHogPanTracker>();
        private readonly List<OrangeBeastTracker> _trackedBeasts = new List<OrangeBeastTracker>();
        private readonly List<MobilePlantTracker> _trackedMobilePlants = new List<MobilePlantTracker>();
        private readonly List<MiniPlantTracker> _trackedMiniPlants = new List<MiniPlantTracker>();
        private readonly List<VehicleTracker> _trackedVehicles = new List<VehicleTracker>();

        private readonly List<ClaimTracker> _trackedClaims = new List<ClaimTracker>();
        private ClaimTracker _townTracker;

        private static readonly Vector3 TownCenter = new Vector3(-150f, 0f, 1000f);
        private const float TownRadiusSqr = 350f * 350f;

        private readonly HashSet<int> _modularHogPanInstanceIds = new HashSet<int>();
        private static readonly Dictionary<(Type, string), FieldInfo> _fieldCache = new Dictionary<(Type, string), FieldInfo>();
        private static readonly Dictionary<(Type, string), PropertyInfo> _propertyCache = new Dictionary<(Type, string), PropertyInfo>();

        private Coroutine _scanRoutine;
        private float _lastTopologyDiscoveryTime = -999f;
        private float _lastPollTime = -999f;
        private float _nextPollTime = 0f;
        private float _nextTopologyTime = 0f;
        private const float TopologyIntervalSeconds = 30f;
        private const BindingFlags FieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        private void Awake()
        {
            Instance = this;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void PurgeAllData()
        {
            CurrentData.Reset();
            _workingData.Reset();
            _trackedModularPlants.Clear();
            _trackedStandalonePans.Clear();
            _trackedBeasts.Clear();
            _trackedMobilePlants.Clear();
            _trackedMiniPlants.Clear();
            _trackedVehicles.Clear();
            for (int i = 0; i < _trackedClaims.Count; i++)
            {
                _trackedClaims[i].Reset();
            }
            _trackedClaims.Clear();
            _modularHogPanInstanceIds.Clear();
            _nextTopologyTime = 0f;
            _nextPollTime = 0f;
            _townTracker?.Reset();
            _townTracker = null;
        }

        private bool IsMenuScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return true;
            string lower = sceneName.ToLower();
            return lower.Contains("menu") || lower.Contains("buffor");
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // Sobald eine Menü-Szene geladen wird, sofort rigoros alles abräumen
            if (IsMenuScene(scene.name))
            {
                PurgeAllData();
            }
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
            _nextTopologyTime = Time.time + TopologyIntervalSeconds;
            float pollInterval = Mathf.Max(0.5f, Config?.ScanIntervalSeconds?.Value ?? 2.0f);
            _nextPollTime = Time.time + pollInterval;

            //DiscoverClaims();
            DiscoverTopology();
            PollState();
        }

        public float NextTopologyScanSeconds
        {
            get
            {
                return Mathf.Max(0f, _nextTopologyTime - Time.time);
            }
        }

        public float NextPollSeconds
        {
            get
            {
                return Mathf.Max(0f, _nextPollTime - Time.time);
            }
        }

        private IEnumerator DiagnosticLoop()
        {
            string lastSceneName = string.Empty;

            while (true)
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                bool isMainMenu = IsMenuScene(sceneName);

                // ========================================================
                // 1. HAUPTMENÜ / BUFFER: KEIN SCANNING, SOFORT LEEREN
                // ========================================================
                if (isMainMenu)
                {
                    PurgeAllData();
                    lastSceneName = string.Empty; // Erzwingt beim nächsten Savegame sauberen Neustart

                    yield return new WaitForSecondsRealtime(0.5f);
                    continue;
                }

                // ========================================================
                // 2. SZENENWECHSEL / LADEBILDSCHIRM INS GAMEPLAY
                // ========================================================
                bool sceneChanged = sceneName != lastSceneName;
                bool isLoading = Singleton<LevelLoadingManager>.IsInstanced() && Singleton<LevelLoadingManager>.Instance.IsLoading();

                if (sceneChanged || isLoading)
                {
                    while (Singleton<LevelLoadingManager>.IsInstanced() && Singleton<LevelLoadingManager>.Instance.IsLoading())
                    {
                        yield return new WaitForSecondsRealtime(0.5f);
                    }

                    // 4 Sekunden Karenzzeit nach Ladeende
                    yield return new WaitForSeconds(4.0f);

                    lastSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

                    // Erster Vollscan für die neue Spielwelt
                    ForceScan();
                }

                // ========================================================
                // 3. REGULÄRER GAMEPLAY-LOOP (100ms Taktung)
                // ========================================================

                // Topology Scan: Alle 30 Sekunden
                if (Time.time >= _nextTopologyTime)
                {
                    _nextTopologyTime = Time.time + TopologyIntervalSeconds;
                    try
                    {
                        DiscoverTopology();
                        PollState();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ClaimMonitor] Topology scan error: {ex}");
                    }
                }

                // Status Poll: Alle Config.ScanIntervalSeconds Sekunden
                if (Time.time >= _nextPollTime)
                {
                    float pollInterval = Mathf.Max(0.5f, Config?.ScanIntervalSeconds?.Value ?? 2.0f);
                    _nextPollTime = Time.time + pollInterval;

                    try
                    {
                        PollState();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ClaimMonitor] Poll error: {ex}");
                    }
                }

                yield return new WaitForSeconds(0.1f);
            }
        }


        // ==========================================
        // STAGE 1: Topology Discovery (Heavy Scan, infrequent)
        // ==========================================
        private void DiscoverTopology()
        {
            _lastTopologyDiscoveryTime = Time.time;

            // 1. NUR den Arbeits-Puffer leeren – CurrentData bleibt für das UI unverändert sichtbar!
            _workingData.Reset();

            _trackedModularPlants.Clear();
            _trackedStandalonePans.Clear();
            _trackedBeasts.Clear();
            _trackedMobilePlants.Clear();
            _trackedMiniPlants.Clear();
            _trackedVehicles.Clear();
            _modularHogPanInstanceIds.Clear();

            // Cache conveyor holders once for both setups
            GoldDigger.ConveyorHolder[] cachedHolders = null;
            bool needHolders = (Config?.Setup3IncludeFeedingChain?.Value ?? false) || (Config?.Setup4IncludeFeedingChain?.Value ?? false);
            if (needHolders)
            {
                cachedHolders = UnityEngine.Object.FindObjectsOfType<GoldDigger.ConveyorHolder>();
            }

            // 2. Deine bestehenden Discovery-Methoden aufrufen (unverändert!)
            DiscoverOrangeBeasts(cachedHolders);
            DiscoverModularPlants(cachedHolders);
            DiscoverStandaloneHogPans();
            DiscoverMobilePlants();
            DiscoverVehicles();
        }

        private void DiscoverModularPlants(GoldDigger.ConveyorHolder[] cachedHolders)
        {
            if (!(Config?.MonitorSetup3?.Value ?? true)) return;

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

                string rawName = lotDesc != null ? lotDesc.Name.ToString() : null;
                string claimName = GameLocResolver.GetClaimName(claimId, rawName);

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

                // Conveyor Holder (Hopper & Elevator)
                bool includeFeedingChain = Config?.Setup3IncludeFeedingChain?.Value ?? false;
                if (includeFeedingChain && cachedHolders != null)
                {
                    for (int h = 0; h < cachedHolders.Length; h++)
                    {
                        var ch = cachedHolders[h];
                        if (ch != null && Vector3.Distance(plant.transform.position, ch.transform.position) < 15f)
                        {
                            tracker.ConveyorHolder = ch;
                            break;
                        }
                    }
                }

                // Grate-Transforms and Dirt components on Sluice Box 3
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

                            // Cache dirt component once during discovery
                            var dirtComp = holderTr != null ? holderTr.GetComponentInChildren<WashPlantSluiceBoxDirt>(true) : null;
                            if (dirtComp != null)
                            {
                                tracker.CachedDirtComponents.Add(dirtComp);
                            }
                        }
                    }
                }

                // Registriere modulare End-HogPans
                if (plant.MyHogPan != null) _modularHogPanInstanceIds.Add(plant.MyHogPan.GetInstanceID());
                if (plant.MyHogPan2 != null) _modularHogPanInstanceIds.Add(plant.MyHogPan2.GetInstanceID());

                _trackedModularPlants.Add(tracker);
                _workingData.ModularPlants.Add(status);
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
                string rawName = lotDesc != null ? lotDesc.Name.ToString() : null;
                string claimName = GameLocResolver.GetClaimName(claimId, rawName);

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
                _workingData.HogPanAreas.Add(status);
            }
        }

        private void DiscoverOrangeBeasts(GoldDigger.ConveyorHolder[] cachedHolders)
        {
            if (!(Config?.MonitorSetup4?.Value ?? true)) return;

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
                string rawName = lotDesc != null ? lotDesc.Name.ToString() : null;
                string claimName = GameLocResolver.GetClaimName(claimId, rawName);

                var status = new OrangeBeastStatus
                {
                    ClaimId = claimId,
                    ClaimName = claimName,
                    BeastIndex = beastCounter++
                };

                var tracker = new OrangeBeastTracker
                {
                    Beast = beast,
                    Status = status
                };

                // Locate and cache Conveyor Holder near the Beast
                bool includeFeedingChain = Config?.Setup4IncludeFeedingChain?.Value ?? false;
                if (includeFeedingChain && cachedHolders != null)
                {
                    for (int h = 0; h < cachedHolders.Length; h++)
                    {
                        var ch = cachedHolders[h];
                        if (ch != null && Vector3.Distance(beast.transform.position, ch.transform.position) < 20f)
                        {
                            tracker.ConveyorHolder = ch;
                            break;
                        }
                    }
                }

                _trackedBeasts.Add(tracker);
                _workingData.OrangeBeasts.Add(status);
            }
        }

        private void DiscoverMobilePlants()
        {
            if (!(Config?.MonitorSetup2?.Value ?? true)) return;

            int plantIndex = 1;

            // 1. Standard Mobile Wash Plant
            var mobilePlants = UnityEngine.Object.FindObjectsOfType<GoldDigger.MobileWashplant>();
            if (mobilePlants != null)
            {
                for (int i = 0; i < mobilePlants.Length; i++)
                {
                    var plant = mobilePlants[i];
                    if (plant == null || !plant.gameObject.activeInHierarchy) continue;

                    int claimId = GetFieldValue<int>(plant, plant.GetType(), "MyClaimId");
                    var lotDesc = GetFieldValue<LotDescriptor>(plant, plant.GetType(), "myLot");
                    if (lotDesc == null)
                    {
                        lotDesc = Singleton<LaptopGlobalManager>.Instance?.FindClosestLotFromAll(plant.transform.position);
                    }
                    string rawName = lotDesc != null ? lotDesc.Name.ToString() : null;
                    string claimName = GameLocResolver.GetClaimName(claimId, rawName);
                    string variantName = GameLocResolver.Resolve(GameLocResolver.KeyMobileWashPlant, "Mobile Wash Plant");

                    var status = new MobileWashPlantStatus
                    {
                        ClaimId = claimId,
                        ClaimName = claimName,
                        PlantIndex = plantIndex++,
                        PlantType = MobilePlantType.MobileWashPlant,
                        VariantName = variantName,
                        RequiresPower = true,
                        RequiresFuel = false
                    };

                    _trackedMobilePlants.Add(new MobilePlantTracker
                    {
                        Plant = plant,
                        Status = status
                    });
                    _workingData.MobilePlants.Add(status);
                }
            }

            // 2. DLC Mini Wash Plant
            var miniPlants = UnityEngine.Object.FindObjectsOfType<GoldDigger.MiniWashplant>();
            if (miniPlants != null)
            {
                for (int i = 0; i < miniPlants.Length; i++)
                {
                    var plant = miniPlants[i];
                    if (plant == null || !plant.gameObject.activeInHierarchy) continue;

                    int claimId = GetFieldValue<int>(plant, plant.GetType(), "MyClaimId");
                    var lotDesc = GetFieldValue<LotDescriptor>(plant, plant.GetType(), "myLot");
                    if (lotDesc == null)
                    {
                        lotDesc = Singleton<LaptopGlobalManager>.Instance?.FindClosestLotFromAll(plant.transform.position);
                    }
                    string rawName = lotDesc != null ? lotDesc.Name.ToString() : null;
                    string claimName = GameLocResolver.GetClaimName(claimId, rawName);
                    string variantName = GameLocResolver.Resolve(GameLocResolver.KeyMiniWashPlant, "Mini Wash Plant");

                    var status = new MobileWashPlantStatus
                    {
                        ClaimId = claimId,
                        ClaimName = claimName,
                        PlantIndex = plantIndex++,
                        PlantType = MobilePlantType.MiniWashPlant,
                        VariantName = variantName,
                        RequiresPower = false,
                        RequiresFuel = true
                    };

                    _trackedMiniPlants.Add(new MiniPlantTracker
                    {
                        Plant = plant,
                        Status = status
                    });
                    CurrentData.MobilePlants.Add(status);
                }
            }
        }

        private void DiscoverVehicles()
        {
            var machines = UnityEngine.Object.FindObjectsOfType<MachineController>();
            if (machines == null || machines.Length == 0) return;

            for (int i = 0; i < machines.Length; i++)
            {
                var machine = machines[i];
                if (machine == null || !machine.gameObject.activeInHierarchy) continue;

                int claimId = GetFieldValue<int>(machine, machine.GetType(), "MyClaimId");
                var lotDesc = GetFieldValue<LotDescriptor>(machine, machine.GetType(), "myLot");
                if (lotDesc == null && Singleton<LaptopGlobalManager>.IsInstanced())
                {
                    lotDesc = Singleton<LaptopGlobalManager>.Instance.FindClosestLotFromAll(machine.transform.position);
                }

                string rawName = lotDesc != null ? lotDesc.Name.ToString() : null;
                string claimName = GameLocResolver.GetClaimName(claimId, rawName);
                string displayName = GameLocResolver.GetVehicleName(machine);

                int slot = -1;
                try
                {
                    var f = machine.GetType().GetField("numberInVehicleSwitchingList", FieldFlags);
                    if (f != null)
                    {
                        object val = f.GetValue(machine);
                        if (val is int s) slot = s;
                    }
                }
                catch { }

                var status = new VehicleStatus
                {
                    InstanceId = machine.gameObject.GetInstanceID(),
                    ClaimId = claimId,
                    ClaimName = claimName,
                    TypeName = machine.GetType().Name,
                    DisplayName = displayName,
                    SwitcherSlotIndex = slot
                };

                var tracker = new VehicleTracker
                {
                    Machine = machine,
                    Belt = machine.GetComponentInChildren<FrankensteinBelt>(true),
                    Status = status
                };

                _trackedVehicles.Add(tracker);
                _workingData.Vehicles.Add(status);
            }
        }


        // ==========================================
        // STAGE 2: State Polling (Ultra-fast, frequent)
        // ==========================================
        private void PollState()
        {
            _lastPollTime = Time.time;

            //PollPlayerLocation();
            PollModularPlants();
            PollStandaloneHogPans();
            PollOrangeBeasts();
            PollMobilePlants();
            PollVehicles();

            _workingData.CompileAlerts(Config);

            CurrentData.CopyFrom(_workingData);
        }

        private void PollPlayerLocation()
        {
            var player = Singleton<Player>.Instance;
            if (player == null)
                return;

            Vector3 playerPos = player.transform.position;
            _workingData.PlayerPosition = playerPos;
            _workingData.PlayerHeading = player.transform.eulerAngles.y;

            if (_trackedClaims.Count == 0)
            {
                DiscoverClaims();
            }

            ClaimTracker detectedClaim = null;

            // 1. Check claim boundaries (0-5)
            for (int i = 0; i < _trackedClaims.Count; i++)
            {
                if (_trackedClaims[i].Contains(playerPos))
                {
                    detectedClaim = _trackedClaims[i];
                    break;
                }
            }

            // 2. Check town boundary
            if (detectedClaim == null && _townTracker != null && _townTracker.Contains(playerPos))
            {
                detectedClaim = _townTracker;
            }

            // 3. Write directly into working data model
            if (detectedClaim != null)
            {
                if (detectedClaim.IsTown)
                {
                    _workingData.LocationType = PlayerLocationType.Town;
                    _workingData.CurrentClaimId = -1;
                    _workingData.CurrentClaimName = detectedClaim.Name;
                    _workingData.CurrentLocationDisplayName = detectedClaim.DisplayName;
                }
                else
                {
                    _workingData.LocationType = PlayerLocationType.Claim;
                    _workingData.CurrentClaimId = detectedClaim.ClaimId;
                    _workingData.CurrentClaimName = detectedClaim.Name;
                    _workingData.CurrentLocationDisplayName = detectedClaim.DisplayName;
                }
            }
            else
            {
                _workingData.LocationType = PlayerLocationType.Wilderness;
                _workingData.CurrentClaimId = -1;
                _workingData.CurrentClaimName = string.Empty;
                _workingData.CurrentLocationDisplayName = GameLocResolver.Resolve("LOCATION_WILDERNESS", "Wilderness");
            }
        }


        private void DiscoverClaims()
        {
            _trackedClaims.Clear();
            _townTracker = null;

            var mgr = GoldDigger.LaptopGlobalManager.Instance;
            if (mgr == null || mgr.LotList == null)
            {
                Debug.LogWarning("[ClaimMonitor] DiscoverClaims: LaptopGlobalManager or LotList not available.");
                return;
            }

            const float claimRadius = 450f;
            const float claimRadiusSqr = claimRadius * claimRadius;

            // 1. Regular claims (Index 0 to 5)
            for (int i = 0; i < mgr.LotList.Count; i++)
            {
                var lot = mgr.LotList[i];
                if (lot?.Terrains == null || lot.Terrains.Count == 0 || lot.Terrains[0] == null)
                    continue;

                string claimName = lot.Name.ToString();
                string locKey = lot.LotName;
                string displayName = !string.IsNullOrEmpty(locKey)
                    ? GameLocResolver.Resolve(locKey, claimName)
                    : claimName;

                Vector3 pos = lot.Terrains[0].transform.position;

                _trackedClaims.Add(new ClaimTracker
                {
                    ClaimId = i,
                    Name = claimName,
                    DisplayName = displayName,
                    ParcelType = lot.MyParcelType,
                    Center2D = new Vector3(pos.x, 0f, pos.z),
                    RadiusSqr = claimRadiusSqr,
                    IsTown = false
                });
            }

            // 2. Town center (verified at bank/store hub: X=693, Z=-391)
            string townLocKey = mgr.TownLot != null ? mgr.TownLot.LotName : "ALLOTMENT_TOWN_NAME";
            const float townRadius = 350f;

            _townTracker = new ClaimTracker
            {
                ClaimId = -1,
                Name = "Town",
                DisplayName = GameLocResolver.Resolve(townLocKey, "Town"),
                ParcelType = mgr.TownLot != null ? mgr.TownLot.MyParcelType : null,
                Center2D = new Vector3(693f, 0f, -391f),
                RadiusSqr = townRadius * townRadius,
                IsTown = true
            };

            Debug.Log($"[ClaimMonitor] DiscoverClaims: Registered {_trackedClaims.Count} claims and town tracker.");
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

                float plantInputFill = 0f;

                // 1. Shaker
                if (plant.WashPlantShaker != null && plant.WashPlantShaker.gameObject.activeInHierarchy)
                {
                    var s = plant.WashPlantShaker;
                    status.ShakerMounted = true;
                    status.ShakerVariant = s.GetType().Name;
                    status.ShakerHasPower = GetFieldValue<bool>(s, s.GetType(), "_hasPower");
                    status.ShakerHasWater = GetFieldValue<bool>(s, s.GetType(), "_hasWater");
                    status.ShakerReady = s.IsReadyToWork;

                    float dirt = GetFieldValue<float>(s, s.GetType(), "DirtVolume");
                    float max = GetFieldValue<float>(s, s.GetType(), "MaxFill");
                    if (max > 0f) plantInputFill = Mathf.Clamp01(dirt / max);

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

                status.PlantInputFillPct = plantInputFill;

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

                    status.MaxCrateFillPct = GetMaxCrateFillPct(tracker);
                }
                else
                {
                    status.SluiceBox3Mounted = false;
                    status.SluiceGratesInstalled = 0;
                    status.MaxCrateFillPct = 0f;
                }

                // 4. Jigs
                if (plant.WashPlantDuplex != null && plant.WashPlantDuplex.gameObject.activeInHierarchy)
                {
                    var j1 = plant.WashPlantDuplex;
                    status.Jig1Mounted = true;
                    status.Jig1HasPower = GetJigPower(j1);
                    ScanJigBuckets(j1, status.Jig1Bucket1, status.Jig1Bucket2);
                    CollectCheckAndRepairParts(j1, "Jig 1", status.Parts);
                }
                else
                {
                    status.Jig1Mounted = false;
                    ScanJigBuckets(null, status.Jig1Bucket1, status.Jig1Bucket2);
                }


                if (plant.WashPlantDuplex2 != null && plant.WashPlantDuplex2.gameObject.activeInHierarchy)
                {
                    var j2 = plant.WashPlantDuplex2;
                    status.Jig2Mounted = true;
                    status.Jig2HasPower = GetJigPower(j2);
                    ScanJigBuckets(j2, status.Jig2Bucket1, status.Jig2Bucket2);
                    CollectCheckAndRepairParts(j2, "Jig 2", status.Parts);
                }
                else
                {
                    status.Jig2Mounted = false;
                    ScanJigBuckets(null, status.Jig2Bucket1, status.Jig2Bucket2);
                }

                float maxMatFill = 0f;

                // 5. End HogPans

                if (plant.MyHogPan != null && plant.MyHogPan.gameObject.activeInHierarchy)
                {
                    status.EndHogPan1Mounted = true;
                    status.EndHogPan1HasWater = plant.MyHogPan.DirtBox?.MyWaterConsumer?.HaveWater ?? false;
                    CountHogPanMats(plant.MyHogPan, out int pan1Total, out int pan1Installed, out float pan1Fill);
                    status.EndHogPan1MatsTotal = pan1Total;
                    status.EndHogPan1MatsInstalled = pan1Installed;
                    if (pan1Fill > maxMatFill) maxMatFill = pan1Fill;
                }
                else
                {
                    status.EndHogPan1Mounted = false;
                }


                if (plant.MyHogPan2 != null && plant.MyHogPan2.gameObject.activeInHierarchy)
                {
                    status.EndHogPan2Mounted = true;
                    status.EndHogPan2HasWater = plant.MyHogPan2.DirtBox?.MyWaterConsumer?.HaveWater ?? false;
                    CountHogPanMats(plant.MyHogPan, out int pan2Total, out int pan2Installed, out float pan2Fill);
                    status.EndHogPan2MatsTotal = pan2Total;
                    status.EndHogPan2MatsInstalled = pan2Installed;
                    if (pan2Fill > maxMatFill) maxMatFill = pan2Fill;
                }
                else
                {
                    status.EndHogPan2Mounted = false;
                }

                // 6. Main Mats & Grilles
                CountHolders(plant.MinerMoss, out int matsTotal, out int matsInstalled, out float matsFill);
                status.SluiceMatsTotal = matsTotal;
                status.SluiceMatsInstalled = matsInstalled;
                if (matsFill > maxMatFill) maxMatFill = matsFill;

                status.MaxMatFillPct = maxMatFill;

                CountHolders(plant.MinerGrille, out int grillesTotal, out int grillesInstalled, out _);
                status.SluiceGrillesTotal = grillesTotal;
                status.SluiceGrillesInstalled = grillesInstalled;

                // 8. Feeding Chain (Hopper & Conveyor Elevator)
                bool monitorFeedingChain = Config?.Setup3IncludeFeedingChain?.Value ?? false;

                if (monitorFeedingChain && tracker.ConveyorHolder != null && tracker.ConveyorHolder.gameObject.activeInHierarchy)
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

                            // Hopper fill level
                            float curDirt = GetFieldValue<float>(ground, typeof(GoldDigger.ConveyorGround), "DirtVolume");
                            float maxDirt = GetFieldValue<float>(ground, typeof(GoldDigger.ConveyorGround), "MaxFill");
                            chain.HopperFillPct = maxDirt > 0f ? Mathf.Clamp01(curDirt / maxDirt) : 0f;

                            // Riemen aus EngineBelt auslesen
                            var belt = GetFieldValue<object>(ground, typeof(GoldDigger.ConveyorGround), "EngineBelt");
                            if (belt != null)
                            {
                                AddCheckAndRepairPart(belt, "Hopper", chain.Parts);
                            }
                        }
                    }
                    else
                    {
                        chain.HopperFillPct = 0f;
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
                    status.FeedingChain.HopperFillPct = 0f;
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

                CountHogPanMats(pan, out int panTotal, out int panInstalled, out float panFill);
                status.TotalMats = panTotal;
                status.InstalledMats = panInstalled;
                status.MaxMatFillPct = panFill;

                if (pan.DirtBox != null && pan.DirtBox.PlaneVolumeMax > 0f)
                {
                    status.DirtFillPct = Mathf.Clamp01(pan.DirtBox.PlaneVolume / pan.DirtBox.PlaneVolumeMax);
                }
                else
                {
                    status.DirtFillPct = 0f;
                }

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

                    float dirt = GetFieldValue<float>(shaker, shaker.GetType(), "DirtVolume");
                    float max = GetFieldValue<float>(shaker, shaker.GetType(), "MaxFill");
                    status.PlantInputFillPct = max > 0f ? Mathf.Clamp01(dirt / max) : 0f;

                    status.Parts.Clear();
                    CollectCheckAndRepairParts(shaker, "Beast Shaker", status.Parts);
                }
                else
                {
                    status.PlantInputFillPct = 0f;
                }

                CountHolders(beast.MinerMoss, out int matsTotal, out int matsInstalled, out float matsFill);
                status.TotalMats = matsTotal;
                status.InstalledMats = matsInstalled;
                status.MaxMatFillPct = matsFill;

                CountHolders(beast.MinerGrille, out int grillesTotal, out int grillesInstalled, out _);
                status.TotalGrilles = grillesTotal;
                status.InstalledGrilles = grillesInstalled;


                // Feeding Chain (Hopper & Conveyor Elevator)
                bool monitorFeedingChain = Config?.Setup4IncludeFeedingChain?.Value ?? false;

                if (monitorFeedingChain && tracker.ConveyorHolder != null && tracker.ConveyorHolder.gameObject.activeInHierarchy)
                {
                    var chain = status.FeedingChain;
                    var beltHolder = tracker.ConveyorHolder.ConveyorBelt;
                    var elevHolder = tracker.ConveyorHolder.ConveyorElevator;

                    bool hopperMounted = beltHolder?.ObjectInHolder != null && beltHolder.ObjectInHolder.gameObject.activeInHierarchy;
                    chain.HopperMounted = hopperMounted;

                    bool elevMounted = elevHolder?.ObjectInHolder != null && elevHolder.ObjectInHolder.gameObject.activeInHierarchy;
                    chain.ConveyorBeltMounted = elevMounted;

                    chain.Parts.Clear();
                    bool hopperPower = false;
                    bool elevPower = false;

                    // 1. Hopper: Power, Belt & Fill level
                    if (hopperMounted)
                    {
                        var ground = beltHolder.ObjectInHolder.GetComponent<GoldDigger.ConveyorGround>();
                        if (ground != null)
                        {
                            var pc = ground.GetComponent<GoldDigger.PowerConsumer>();
                            hopperPower = pc != null && GetFieldValue<bool>(pc, typeof(GoldDigger.PowerConsumer), "_hasPower");

                            // Read hopper fill level
                            float curDirt = GetFieldValue<float>(ground, typeof(GoldDigger.ConveyorGround), "DirtVolume");
                            float maxDirt = GetFieldValue<float>(ground, typeof(GoldDigger.ConveyorGround), "MaxFill");
                            chain.HopperFillPct = maxDirt > 0f ? Mathf.Clamp01(curDirt / maxDirt) : 0f;

                            var belt = GetFieldValue<object>(ground, typeof(GoldDigger.ConveyorGround), "EngineBelt");
                            if (belt != null)
                            {
                                AddCheckAndRepairPart(belt, "Hopper", chain.Parts);
                            }
                        }
                    }
                    else
                    {
                        chain.HopperFillPct = 0f;
                    }

                    // 2. Elevator: Power & Buckets
                    if (elevMounted)
                    {
                        var elev = elevHolder.ObjectInHolder.GetComponent<GoldDigger.ConveyorElevator>();
                        if (elev != null)
                        {
                            elevPower = GetFieldValue<bool>(elev, typeof(GoldDigger.ConveyorElevator), "_hasPower");

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
                    status.FeedingChain.HopperFillPct = 0f;
                    status.FeedingChain.Parts.Clear();
                }
            }
        }

        private void PollMobilePlants()
        {
            // 1. Standard Mobile Wash Plant
            for (int i = 0; i < _trackedMobilePlants.Count; i++)
            {
                var tracker = _trackedMobilePlants[i];
                var plant = tracker.Plant;
                var status = tracker.Status;

                if (plant == null || !plant.gameObject.activeInHierarchy)
                {
                    status.IsReadyToOperate = false;
                    status.IsHoseConnected = false;
                    continue;
                }

                // Water and Hose Connection check
                var waterConsumer = plant._WaterConsumer;
                bool isHoseConnected = false;
                bool hasWater = false;

                if (waterConsumer != null)
                {
                    hasWater = waterConsumer.HaveWater || plant.ForceWater;
                    isHoseConnected = waterConsumer.Producent != null;
                }

                status.IsHoseConnected = isHoseConnected;
                status.HasWater = hasWater;

                // Power consumer check
                var powerConsumer = plant._PowerConsumer;
                bool hasPower = false;
                if (powerConsumer != null)
                {
                    hasPower = GetFieldValue<bool>(powerConsumer, typeof(GoldDigger.PowerConsumer), "_hasPower") || plant.ForcePower;
                }
                status.HasPower = hasPower;

                // Dirt fill
                status.DirtFillPct = plant.MaxFill > 0f ? Mathf.Clamp01(plant.CurrentFill / plant.MaxFill) : 0f;

                // Bucket 1
                var bucket = plant.Bucket1;
                bool bucketMounted = bucket != null && bucket.IsAttached();
                status.BucketMounted = bucketMounted;
                status.BucketFillPct = (bucketMounted && bucket.MaxVolume > 0f) ? Mathf.Clamp01(bucket.CurrentVolumeM3 / bucket.MaxVolume) : 0f;
                status.BucketCurrentVolumeM3 = bucketMounted ? bucket.CurrentVolumeM3 : 0f;

                status.IsReadyToOperate = plant.CheckIfIsReadyToWork();

                // Maintenance parts (Engine, Pipe, Wheels, etc.)
                status.Parts.Clear();
                var partsArray = GetFieldValue<CheckAndRepair[]>(plant, typeof(GoldDigger.MobileWashplant), "MyCheckAndRepair");
                if (partsArray != null)
                {
                    for (int p = 0; p < partsArray.Length; p++)
                    {
                        var part = partsArray[p];
                        if (part == null) continue;
                        AddCheckAndRepairPart(part, "Mobile Plant", status.Parts);
                    }
                }
            }

            // 2. DLC Mini Wash Plant
            for (int i = 0; i < _trackedMiniPlants.Count; i++)
            {
                var tracker = _trackedMiniPlants[i];
                var plant = tracker.Plant;
                var status = tracker.Status;

                if (plant == null || !plant.gameObject.activeInHierarchy)
                {
                    status.IsReadyToOperate = false;
                    status.IsHoseConnected = false;
                    continue;
                }

                // Water and Hose Connection check
                var waterConsumer = plant._WaterConsumer;
                bool isHoseConnected = false;
                bool hasWater = false;

                if (waterConsumer != null)
                {
                    hasWater = waterConsumer.HaveWater || plant.ForceWater;
                    isHoseConnected = waterConsumer.Producent != null;
                }

                status.IsHoseConnected = isHoseConnected;
                status.HasWater = hasWater;

                // Fuel controller check
                var fuelController = plant._FuelController;
                float fuel = 0f;
                float maxFuel = 0f;
                if (fuelController != null)
                {
                    fuel = fuelController.CurrentCapacity;
                    maxFuel = fuelController.MaxCapacity;
                }

                status.FuelPct = maxFuel > 0f ? Mathf.Clamp01(fuel / maxFuel) : 0f;
                status.HasFuel = fuel > 0.05f;

                // Dirt fill
                status.DirtFillPct = plant.MaxFill > 0f ? Mathf.Clamp01(plant.CurrentFill / plant.MaxFill) : 0f;

                // Bucket
                var bucket = plant.Bucket;
                bool bucketMounted = bucket != null && bucket.IsAttached();
                status.BucketMounted = bucketMounted;
                status.BucketFillPct = (bucketMounted && bucket.MaxVolume > 0f) ? Mathf.Clamp01(bucket.CurrentVolumeM3 / bucket.MaxVolume) : 0f;
                status.BucketCurrentVolumeM3 = bucketMounted ? bucket.CurrentVolumeM3 : 0f;

                // Pipes repair components
                status.Parts.Clear();
                if (plant.Pipes_RepairComp_01 != null) AddCheckAndRepairPart(plant.Pipes_RepairComp_01, "Mini Plant", status.Parts);
                if (plant.Pipes_RepairComp_02 != null) AddCheckAndRepairPart(plant.Pipes_RepairComp_02, "Mini Plant", status.Parts);

                status.IsReadyToOperate = status.HasWater && status.HasFuel && status.BucketMounted;
            }
        }

        private void PollVehicles()
        {
            for (int i = 0; i < _trackedVehicles.Count; i++)
            {
                var tracker = _trackedVehicles[i];
                var machine = tracker.Machine;
                var status = tracker.Status;

                if (machine == null || !machine.gameObject.activeInHierarchy)
                {
                    status.IsEngineStarted = false;
                    continue;
                }

                status.IsEngineStarted = machine.IsEngineStarted;

                // 1. Primary Tank (Fahrwerk)
                if (machine.Fuel != null)
                {
                    status.PrimaryTank.CurrentLiters = machine.Fuel.FuelCurrentCapacity;
                    status.PrimaryTank.MaxLiters = machine.Fuel.FuelMaxCapacity;
                }

                // 2. Secondary Tank & Conveyor (Frankenstein / Cordylus)
                var belt = tracker.Belt;
                if (belt != null && belt.gameObject.activeInHierarchy)
                {
                    status.Conveyor.HasConveyor = true;
                    status.Conveyor.IsRunning = belt.IsEnabled;
                    status.Conveyor.SpeedMultiplier = belt.SpeedMultiplier;
                    status.Conveyor.DirtVolume = belt.DirtVolume;
                    status.Conveyor.MaxVolume = belt.MaxVolume;

                    if (belt.MyFuel != null)
                    {
                        if (status.SecondaryTank == null)
                        {
                            status.SecondaryTank = new VehicleTankInfo { Label = "Conveyor" };
                        }
                        status.SecondaryTank.CurrentLiters = belt.MyFuel.CurrentCapacity;
                        status.SecondaryTank.MaxLiters = belt.MyFuel.MaxCapacity;
                    }
                }
            }
        }

        // ==========================================
        // Helper Routines
        // ==========================================
        private void ScanJigBuckets(WashplantDuplexJigBase jig, JigBucketStatus b1Status, JigBucketStatus b2Status)
        {
            if (jig == null)
            {
                if (b1Status != null)
                {
                    b1Status.HasSlot = false;
                    b1Status.IsMounted = false;
                    b1Status.CurrentVolumeM3 = 0f;
                    b1Status.FillPct = 0f;
                }
                if (b2Status != null)
                {
                    b2Status.HasSlot = false;
                    b2Status.IsMounted = false;
                    b2Status.CurrentVolumeM3 = 0f;
                    b2Status.FillPct = 0f;
                }
                return;
            }

            bool isPlanter = jig is Planter;

            // Helper to extract Bucket from a direct reference or through a Holder component
            Bucket extractBucket(string fieldName)
            {
                // 1. Try direct Bucket reference
                var directBucket = GetFieldValue<Bucket>(jig, typeof(WashplantDuplexJigBase), fieldName)
                    ?? GetPropertyValue<Bucket>(jig, typeof(WashplantDuplexJigBase), fieldName);
                if (directBucket != null) return directBucket;

                // 2. Try Holder component reference whose ObjectInHolder contains the Bucket
                var holder = GetFieldValue<Component>(jig, typeof(WashplantDuplexJigBase), fieldName)
                    ?? GetPropertyValue<Component>(jig, typeof(WashplantDuplexJigBase), fieldName);
                if (holder != null)
                {
                    var objInHolderProp = holder.GetType().GetProperty("ObjectInHolder", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (objInHolderProp != null)
                    {
                        var val = objInHolderProp.GetValue(holder, null) as MonoBehaviour;
                        if (val is Bucket b) return b;
                        if (val != null)
                        {
                            var bComp = val.GetComponent<Bucket>();
                            if (bComp != null) return bComp;
                        }
                    }
                }
                return null;
            }

            // Bucket 1 (Duplex Jig, Gravel Pump, Planter)
            if (b1Status != null)
            {
                var b1 = extractBucket("Bucket1");
                b1Status.HasSlot = true;
                b1Status.IsMounted = b1 != null && b1.IsAttached();
                b1Status.CurrentVolumeM3 = b1 != null ? b1.CurrentVolumeM3 : 0f;
                b1Status.FillPct = b1 != null ? b1.FillPct : 0f;
            }

            // Bucket 2 (Planter only)
            if (b2Status != null)
            {
                if (isPlanter)
                {
                    var b2 = extractBucket("Bucket2");
                    b2Status.HasSlot = true;
                    b2Status.IsMounted = b2 != null && b2.IsAttached();
                    b2Status.CurrentVolumeM3 = b2 != null ? b2.CurrentVolumeM3 : 0f;
                    b2Status.FillPct = b2 != null ? b2.FillPct : 0f;
                }
                else
                {
                    b2Status.HasSlot = false;
                    b2Status.IsMounted = false;
                    b2Status.CurrentVolumeM3 = 0f;
                    b2Status.FillPct = 0f;
                }
            }
        }


        private bool GetJigPower(WashplantDuplexJigBase jig)
        {
            if (jig == null) return false;

            var powerConsumer = GetFieldValue<GoldDigger.PowerConsumer>(jig, typeof(WashplantDuplexJigBase), "Power")
                            ?? GetPropertyValue<GoldDigger.PowerConsumer>(jig, typeof(WashplantDuplexJigBase), "Power");

            if (powerConsumer == null) return false;

            return GetFieldValue<bool>(powerConsumer, typeof(GoldDigger.PowerConsumer), "_hasPower");
        }

        // Current in ClaimScannerV2.cs:
        private (int total, int installed, float maxMatFill) CountHogPanMats(HogPan hogPan)
        {
            if (hogPan == null || hogPan.MinerMoss == null) return (0, 0, 0f);

            int total = hogPan.MinerMoss.Count;
            int installed = 0;
            float maxFill = 0f;

            for (int i = 0; i < hogPan.MinerMoss.Count; i++)
            {
                var moss = hogPan.MinerMoss[i];
                if (moss != null && moss.gameObject.activeInHierarchy)
                {
                    installed++;
                    if (moss.MaxGroundVolume > 0f)
                    {
                        float fill = Mathf.Clamp01(moss.GroundVolume / moss.MaxGroundVolume);
                        if (fill > maxFill) maxFill = fill;
                    }
                }
            }

            return (total, installed, maxFill);
        }

        private void CountHogPanMats(HogPan hogPan, out int total, out int installed, out float maxMatFill)
        {
            total = 0;
            installed = 0;
            maxMatFill = 0f;

            if (hogPan == null || hogPan.MinerMoss == null) return;

            var mats = hogPan.MinerMoss;
            total = mats.Count;

            for (int i = 0; i < mats.Count; i++)
            {
                var moss = mats[i];
                if (moss != null && moss.gameObject.activeInHierarchy)
                {
                    installed++;
                    if (moss.MaxGroundVolume > 0f)
                    {
                        float fill = Mathf.Clamp01(moss.GroundVolume / moss.MaxGroundVolume);
                        if (fill > maxMatFill) maxMatFill = fill;
                    }
                }
            }
        }

        private void CountHolders(List<RepairHolder> holders, out int total, out int installed, out float maxFill)
        {
            total = 0;
            installed = 0;
            maxFill = 0f;

            if (holders == null) return;

            total = holders.Count;
            for (int i = 0; i < holders.Count; i++)
            {
                var holder = holders[i];
                if (holder != null && holder.ObjectInHolder != null && holder.ObjectInHolder.gameObject.activeInHierarchy)
                {
                    installed++;
                    var moss = holder.ObjectInHolder.GetComponent<MinersMoss>();
                    if (moss != null && moss.MaxGroundVolume > 0f)
                    {
                        float fill = Mathf.Clamp01(moss.GroundVolume / moss.MaxGroundVolume);
                        if (fill > maxFill) maxFill = fill;
                    }
                }
            }
        }

        /*private (int total, int installed, float maxMatFill) CountHogPanMats(HogPan hogPan)
        {
            if (hogPan == null || hogPan.MinerMoss == null) return (0, 0, 0f);

            int total = hogPan.MinerMoss.Count;
            int installed = 0;
            float maxFill = 0f;

            for (int i = 0; i < hogPan.MinerMoss.Count; i++)
            {
                var moss = hogPan.MinerMoss[i];
                if (moss != null && moss.gameObject.activeInHierarchy)
                {
                    installed++;
                    if (moss.MaxGroundVolume > 0f)
                    {
                        float fill = Mathf.Clamp01(moss.GroundVolume / moss.MaxGroundVolume);
                        if (fill > maxFill) maxFill = fill;
                    }
                }
            }

            return (total, installed, maxFill);
        }

        private (int total, int installed, float maxFill) CountHolders(List<RepairHolder> holders)
        {
            if (holders == null) return (0, 0, 0f);

            int total = holders.Count;
            int installed = 0;
            float maxFill = 0f;

            for (int i = 0; i < holders.Count; i++)
            {
                RepairHolder holder = holders[i];
                if (holder != null && holder.ObjectInHolder != null && holder.ObjectInHolder.gameObject.activeInHierarchy)
                {
                    installed++;
                    var moss = holder.ObjectInHolder.GetComponent<MinersMoss>();
                    if (moss != null && moss.MaxGroundVolume > 0f)
                    {
                        float fill = Mathf.Clamp01(moss.GroundVolume / moss.MaxGroundVolume);
                        if (fill > maxFill) maxFill = fill;
                    }
                }
            }

            return (total, installed, maxFill);
        }*/

        private float GetMaxCrateFillPct(ModularPlantTracker tracker)
        {
            var plant = tracker?.WashPlantCounter;
            if (plant?.SluiceBox3?.ObjectInHolder == null || !plant.SluiceBox3.ObjectInHolder.gameObject.activeInHierarchy)
                return 0f;

            var holdable = plant.SluiceBox3.ObjectInHolder as ShovelHoldable;
            if (holdable == null) return 0f;

            float maxFill = 0f;
            var indicators = GetFieldValue<Indicator[]>(holdable, holdable.GetType(), "_MyIndicators");

            if (indicators != null && indicators.Length > 0)
            {
                for (int i = 0; i < indicators.Length; i++)
                {
                    var ind = indicators[i];
                    if (ind == null || !ind.gameObject.activeInHierarchy) continue;

                    string text = GetFieldValue<string>(ind, ind.GetType(), "_MyText");
                    if (!string.IsNullOrEmpty(text))
                    {
                        string clean = text.Replace("%", "").Trim();
                        if (float.TryParse(clean, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float parsed))
                        {
                            float pct = Mathf.Clamp01(parsed / 100f);
                            if (pct > maxFill) maxFill = pct;
                        }
                    }
                }
                return maxFill;
            }

            // Fast fallback: Use pre-cached dirt components instead of recursive hierarchy searches
            for (int i = 0; i < tracker.CachedDirtComponents.Count; i++)
            {
                var dirtComp = tracker.CachedDirtComponents[i];
                if (dirtComp != null && dirtComp.MaxFill > 0f)
                {
                    float pct = Mathf.Clamp01(dirtComp.CurrentFill / dirtComp.MaxFill);
                    if (pct > maxFill) maxFill = pct;
                }
            }

            return maxFill;
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
            if (target == null || type == null) return default;

            var key = (type, fieldName);
            if (!_fieldCache.TryGetValue(key, out var field))
            {
                Type current = type;
                while (current != null && current != typeof(MonoBehaviour) && current != typeof(object))
                {
                    field = current.GetField(fieldName, FieldFlags);
                    if (field != null) break;
                    current = current.BaseType;
                }
                _fieldCache[key] = field; // Cache result even if null to avoid repeating failed searches
            }

            if (field != null)
            {
                try
                {
                    var val = field.GetValue(target);
                    if (val is T castVal) return castVal;
                }
                catch { }
            }
            return default;
        }

        private T GetPropertyValue<T>(object target, Type type, string propertyName)
        {
            if (target == null || type == null) return default;

            var key = (type, propertyName);
            if (!_propertyCache.TryGetValue(key, out var prop))
            {
                Type current = type;
                while (current != null && current != typeof(MonoBehaviour) && current != typeof(object))
                {
                    prop = current.GetProperty(propertyName, FieldFlags);
                    if (prop != null && prop.CanRead && prop.GetIndexParameters().Length == 0) break;
                    current = current.BaseType;
                }
                _propertyCache[key] = prop; // Cache result even if null
            }

            if (prop != null)
            {
                try
                {
                    var val = prop.GetValue(target, null);
                    if (val is T castVal) return castVal;
                }
                catch { }
            }
            return default;
        }
    }
}