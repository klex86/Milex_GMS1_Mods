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
        public ClaimDiagnosticsDataV2 CurrentData { get; } = new ClaimDiagnosticsDataV2();

        public List<ClaimAlert> ActiveAlerts => CurrentData.ActiveAlerts;
        public int PlantCount => CurrentData.WashPlants.Count;
        public int MatCount => CurrentData.WashPlants.Count > 0 ? CurrentData.WashPlants[0].InstalledMats : 0;
        public int VehicleCount => 0; // Not yet tracked in V2

        public static ClaimScannerV2 Instance { get; private set; }

        private Coroutine _scanRoutine;
        public MonitorConfig Config { get; set; }

        private const BindingFlags FieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

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

            OrangeBeastWashPlantGoldCounter[] beasts = UnityEngine.Object.FindObjectsOfType<OrangeBeastWashPlantGoldCounter>();
            if (beasts == null || beasts.Length == 0)
            {
                CurrentData.CompileAlerts(Config);
                return;
            }

            foreach (var plant in beasts)
            {
                if (plant == null) continue;
                WashplantShakerBase shaker = plant.WashPlantShaker;

                if (shaker == null) continue;

                int claimId = GetFieldValue<int>(plant, plant.GetType(), "MyClaimId");
                var lotDesc = GetFieldValue<LotDescriptor>(plant, plant.GetType(), "myLot");
                string claimName = lotDesc != null ? lotDesc.Name.ToString() : $"Claim #{claimId}";

                var status = new WashPlantStatusV2
                {
                    ClaimId = claimId,
                    ClaimName = claimName,
                    Tier = WashPlantTier.OrangeBeast,
                    IsReadyToOperate = plant.WashplantReady
                };

                // 1. Sluice Mats (MinerMoss)
                if (plant.MinerMoss != null)
                {
                    status.TotalMats = plant.MinerMoss.Count;
                    for (int i = 0; i < plant.MinerMoss.Count; i++)
                    {
                        RepairHolder holder = plant.MinerMoss[i];
                        if (holder != null && holder.ObjectInHolder != null && holder.ObjectInHolder.gameObject.activeInHierarchy)
                        {
                            status.InstalledMats++;
                        }
                    }
                }

                // 2. Sluice Grilles (MinerGrille)
                if (plant.MinerGrille != null)
                {
                    status.TotalGrilles = plant.MinerGrille.Count;
                    for (int i = 0; i < plant.MinerGrille.Count; i++)
                    {
                        RepairHolder holder = plant.MinerGrille[i];
                        if (holder != null && holder.ObjectInHolder != null && holder.ObjectInHolder.gameObject.activeInHierarchy)
                        {
                            status.InstalledGrilles++;
                        }
                    }
                }

                // 3. Shaker & Mounted Parts (CheckAndRepair)                                
                status.HasPower = GetFieldValue<bool>(shaker, shaker.GetType(), "_hasPower");
                status.HasWater = GetFieldValue<bool>(shaker, shaker.GetType(), "_hasWater");
                status.IsPowerReady = shaker.IsPowerReady;
                status.IsWaterReady = shaker.IsWaterReady;
                status.IsReadyToWork = shaker.IsReadyToWork;

                if (shaker.MyCheckAndRepair != null)
                {
                    for (int i = 0; i < shaker.MyCheckAndRepair.Length; i++)
                    {
                        CheckAndRepair part = shaker.MyCheckAndRepair[i];
                        if (part == null) continue;

                        string prefabName = GetFieldValue<string>(part, part.GetType(), "OriginalPrefabName") ?? part.Name;

                        status.Parts.Add(new MachinePartStatus
                        {
                            Name = part.Name,
                            PrefabName = prefabName,
                            IsInPlace = part.IsInPlace,
                            IsDestroyed = part.IsDestroyed,
                            Durability = part.Durability,
                            IsReinforced = part.IsReinforced,
                            Category = MapPartCategory(prefabName, part.Name)
                        });
                    }
                }


                CurrentData.WashPlants.Add(status);
            }

            CurrentData.CompileAlerts(Config);
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