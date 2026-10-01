using System;
using System.Collections.Generic;
using System.IO;
using Milex.GMS1.Core.Localization;
using UnityEngine;

namespace Milex.GMS1.Mods.ClaimMonitor.Diagnostics
{
    public static class GameLocResolver
    {
        private static readonly Dictionary<string, string> _csvCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static string _cachedLanguage = null;
        // Shakers
        public const string KeyShaker = "Shaker";
        public const string KeyDerocker = "DE_ROCKER_NAME";
        public const string KeyGlacierCreek = "GALCIER_CREEK_NAME";
        public const string KeyOrangeBeastShaker = "SHOP_MACHINES_ORANGEBEAST_SHAKER_NAME";

        // Trommels
        public const string KeyTrommel = "Trommel";
        public const string KeyTrommelReinforced = "TROMMEL_NEW_NAME";
        public const string KeyTrommelOldArnold = "TROMMEL_OLD_NAME";

        // Jigs
        public const string KeyDuplexJig = "DuplexJig";
        public const string KeyPlanter = "SHOP_MACHINES_EQUIPMENT_PLANTER_NAME";
        public const string KeyGravelPump = "GRAVEL_PUMP_NAME";

        // Sluices & Mats
        public const string KeyHogPanContainer = "HogPanContainer";
        public const string KeyHogPanSluiceBox = "HogPanSluiceBox";
        public const string KeyHogPanMat = "SHOP_ITEM_PARTS_HOGPAN_MINERSMOSS_NAME";
        public const string KeyMinersMoss = "SHOP_ITEM_EQUIPMENT_MINERSMOSS_NAME";
        public const string KeyMinersGrille = "SHOP_ITEM_EQUIPMENT_MINERSGRILLE_NAME";
        public const string KeySluiceGrate = "SHOP_ITEM_EQUIPMENT_SLUICECRATE_NAME";
        public const string KeyBucket = "SHOP_ITEM_EQUIPMENT_BUCKET_NAME";
        public const string KeyConveyorHopper = "ConveyorsContainer";
        public const string KeyConveyorBelt = "ConveyorsBelt";
        public const string KeyOrangeBeastSluice = "SHOP_MACHINES_EQUIPMENT_SLUICEBOX_BEAST_NAME";

        // Mobile Plants & Trailers
        public const string KeyMobileWashPlant = "SHOP_ITEM_TRAILER_WASHPLANT_NAME";
        public const string KeyMiniWashPlant = "SHOP_ITEM_TRAILER_MINI_WASHPLANT_NAME";
        public const string KeyTrailerSmall = "SHOP_ITEM_TRAILER_SMALL_NAME";
        public const string KeyTrailerBig = "SHOP_ITEM_TRAILER_BIG_NAME";
        public const string KeyTrailerFuelTank = "SHOP_ITEM_TRAILER_FUELTANK_NAME";
        public const string KeyTrailerMagnetite = "SHOP_ITEM_TRAILER_MAGNETITE_NAME";
        public const string KeyTrailerLight = "SHOP_ITEM_TRAILER_LIGHT_NAME";

        // Vehicles
        public const string KeyVehiclePickup = "MACHINE_PICKUP";
        public const string KeyVehicleMiniExcavator = "SHOP_MACHINES_VEHICLE_EX303_NAME";
        public const string KeyVehicleSmallExcavator = "SHOP_MACHINES_VEHICLE_EX270_NAME";
        public const string KeyVehicleBigExcavator = "SHOP_MACHINES_VEHICLE_EX400_NAME";
        public const string KeyVehicleHugeExcavator = "SHOP_MACHINES_VEHICLE_EX400_WINTER_NAME";
        public const string KeyVehicleLoader = "SHOP_MACHINES_VEHICLE_980B_NAME";
        public const string KeyVehicleMiniLoader = "SHOP_MACHINES_VEHICLE_BOBCAT_LOADER_NAME";
        public const string KeyVehicleBulldozer = "SHOP_MACHINES_VEHICLE_D6H_NAME";
        public const string KeyVehicleBigBulldozer = "SHOP_MACHINES_VEHICLE_D6H_BIGGER_NAME";
        public const string KeyVehicleDumpTruck = "SHOP_MACHINES_VEHICLE_DUMPTRUCK_NAME";
        public const string KeyVehicleBigDumpTruck = "SHOP_MACHINES_VEHICLE_DUMPTRUCK_BIG_NAME";
        public const string KeyVehicleDrill = "SHOP_MACHINES_VEHICLE_D6H_DRILL_NAME";
        public const string KeyVehicleMiniDrill = "SHOP_MACHINES_VEHICLE_BOBCAT_DRILL_NAME";
        public const string KeyVehicleBackhoeLoader = "SHOP_MACHINES_VEHICLE_LEX192_NAME";
        public const string KeyVehicleFuelTruck = "SHOP_MACHINES_VEHICLE_FUELTRUCK_BIG_NAME";
        public const string KeyVehicleFrankenstein = "SHOP_MACHINES_VEHICLE_FRANKENSTEIN_NAME";
        public const string KeyVehicleCordylus = "SHOP_MACHINES_VEHICLE_MAXIMUS_NAME";
        public const string KeyVehicleQuad = "VEHICLE_QUAD";

        // Infrastructure & Utilities
        public const string KeyPowerGeneratorBig = "SHOP_MACHINES_EQUIPMENT_POWERGENERATOR_STATIONARY_NAME";
        public const string KeyPowerGeneratorMobile = "SHOP_ITEM_EQUIPMENT_MOBILEGENERATOR_NAME";
        public const string KeyWaterPumpElectricBig = "SHOP_ITEM_EQUIPMENT_WATERPUMP_BIG_ELECTRIC_NAME";
        public const string KeyWaterPumpElectricSmall = "SHOP_ITEM_EQUIPMENT_WATERPUMP_SMALL_ELECTRIC_NAME";
        public const string KeyWaterPumpDieselBig = "SHOP_ITEM_EQUIPMENT_WATERPUMP_BIG_NAME";
        public const string KeyWaterPumpDieselSmall = "SHOP_ITEM_EQUIPMENT_WATERPUMP_SMALL_NAME";
        public const string KeyWaterTower = "SHOP_ITEM_EQUIPMENT_WATER_TOWER_NAME";

        // Resolves official game claim name from claim ID
        public static string GetClaimName(int claimId, string fallback = null)
        {
            string key = claimId switch
            {
                1 => "ALLOTMENT_1_NAME_SHORT",
                2 => "ALLOTMENT_2_NAME",
                3 => "ALLOTMENT_3_NAME_SHORT",
                4 => "ALLOTMENT_4_NAME",
                5 => "ALLOTMENT_DLC_NAME",
                6 => "ALLOTMENT_DLC4_NAME",
                _ => null
            };

            if (key != null)
            {
                string resolved = Resolve(key);
                if (!string.IsNullOrEmpty(resolved) && resolved != key)
                    return resolved;
            }

            return !string.IsNullOrEmpty(fallback) ? fallback : $"Claim #{claimId}";
        }

        // Resolves localized display name for any MachineController
        public static string GetVehicleName(MachineController machine)
        {
            if (machine == null) return "Vehicle";

            string typeName = machine.GetType().Name;
            string goName = machine.gameObject.name;

            string key = typeName switch
            {
                "Pickup" => KeyVehiclePickup,
                "SmallExcavator" => KeyVehicleMiniExcavator, // EX303
                "BobCatLoader" => KeyVehicleMiniLoader,
                "BobCatDrill" => KeyVehicleMiniDrill,
                "Ladowarka" => KeyVehicleLoader,
                "DumpTruck" => goName.IndexOf("Big", StringComparison.OrdinalIgnoreCase) >= 0 ? KeyVehicleBigDumpTruck : KeyVehicleDumpTruck,
                "KoparkoLadowarka" => KeyVehicleBackhoeLoader,
                "Doozer_D6H_Drill" => KeyVehicleDrill,
                "Doozer_D6H" => goName.IndexOf("BD11", StringComparison.OrdinalIgnoreCase) >= 0 ? KeyVehicleBigBulldozer : KeyVehicleBulldozer,
                "Koparka" => goName.IndexOf("Winter", StringComparison.OrdinalIgnoreCase) >= 0 
                    ? KeyVehicleHugeExcavator 
                    : (goName.IndexOf("270", StringComparison.OrdinalIgnoreCase) >= 0 ? KeyVehicleSmallExcavator : KeyVehicleBigExcavator),
                "FrankensteinExcavator" => KeyVehicleFrankenstein,
                "MaximusMachineController" => KeyVehicleCordylus,
                "Quad" => KeyVehicleQuad,
                _ => null
            };

            if (!string.IsNullOrEmpty(key))
            {
                string resolved = Resolve(key);
                if (!string.IsNullOrEmpty(resolved) && resolved != key)
                    return resolved;
            }

            // Fallback: stripped GameObject name
            return goName.Replace("(Clone)", "").Trim();
        }

        // Resolves shaker variant by class name or identifier
        public static string GetShakerName(string variantName)
        {
            if (string.IsNullOrEmpty(variantName)) return Resolve(KeyShaker, "Shaker");

            if (variantName.IndexOf("Derocker", StringComparison.OrdinalIgnoreCase) >= 0)
                return Resolve(KeyDerocker, "Derocker");

            if (variantName.IndexOf("Glacier", StringComparison.OrdinalIgnoreCase) >= 0)
                return Resolve(KeyGlacierCreek, "Glacier Creek");

            return Resolve(KeyShaker, "Shaker");
        }


        // Resolves trommel variant by class name or identifier
        public static string GetTrommelName(string variantName)
        {
            if (string.IsNullOrEmpty(variantName)) return Resolve(KeyTrommel, "Trommel");

            if (variantName.IndexOf("Old", StringComparison.OrdinalIgnoreCase) >= 0 ||
                variantName.IndexOf("Arnold", StringComparison.OrdinalIgnoreCase) >= 0)
                return Resolve(KeyTrommelOldArnold, "Old Arnold's Trommel");

            if (variantName.IndexOf("New", StringComparison.OrdinalIgnoreCase) >= 0 ||
                variantName.IndexOf("Big", StringComparison.OrdinalIgnoreCase) >= 0 ||
                variantName.IndexOf("Reinforced", StringComparison.OrdinalIgnoreCase) >= 0)
                return Resolve(KeyTrommelReinforced, "Reinforced Trommel");

            return Resolve(KeyTrommel, "Trommel");
        }

        // Resolves official game text directly from the matching language CSV
        public static string Resolve(string gameKey, string fallback = "")
        {
            if (string.IsNullOrEmpty(gameKey)) return fallback;

            EnsureLanguageCacheLoaded();

            if (_csvCache.TryGetValue(gameKey, out string val) && !string.IsNullOrEmpty(val))
                return val;

            return !string.IsNullOrEmpty(fallback) ? fallback : gameKey;
        }

        private static void EnsureLanguageCacheLoaded()
        {
            string currentLang = LocalizationManager.CurrentLanguage ?? "en";

            // Nur neu laden, wenn sich die Sprache gegenüber dem letzten Stand geändert hat
            if (_cachedLanguage == currentLang)
                return;

            _csvCache.Clear();
            _cachedLanguage = currentLang;

            // <GameRoot>\GoldMiningSimulator_Data\StreamingAssets\local\<lang>.csv
            string localDir = Path.Combine(Application.streamingAssetsPath, "local");
            string targetCsvPath = Path.Combine(localDir, $"{currentLang}.csv");

            // Fallback auf en.csv, falls eine Sprache nicht existieren sollte
            if (!File.Exists(targetCsvPath))
            {
                targetCsvPath = Path.Combine(localDir, "en.csv");
            }

            if (!File.Exists(targetCsvPath))
            {
                Debug.LogWarning($"[ClaimMonitor] Game localization CSV not found at: {targetCsvPath}");
                return;
            }

            try
            {
                using var reader = new StreamReader(targetCsvPath, System.Text.Encoding.UTF8);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    int commaIdx = line.IndexOf(',');
                    if (commaIdx <= 0) continue;

                    string key = line.Substring(0, commaIdx).Trim().Trim('"');
                    string val = line.Substring(commaIdx + 1).Trim().Trim('"');

                    if (!_csvCache.ContainsKey(key))
                        _csvCache[key] = val;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ClaimMonitor] Error loading game CSV '{targetCsvPath}': {ex.Message}");
            }
        }
    }
}