using Milex.GMS1.Core.Localization;
using Milex.GMS1.Mods.ClaimMonitor.Config;
using Milex.GMS1.Mods.ClaimMonitor.Diagnostics;
using Milex.GMS1.Mods.ClaimMonitor.Diagnostics.Models;
using Milex.GMS1.Mods.ClaimMonitor.Diagnostics.Scanners;
using UnityEngine;

namespace Milex.GMS1.Mods.ClaimMonitor.UI
{
    public class DebugOverlay : MonoBehaviour
    {
        public MonitorConfig Config { get; set; }

        private Rect _windowRect = new Rect(30f, 30f, 950f, 600f);
        private Vector2 _scrollPos = Vector2.zero;
        private int _selectedCategoryIndex = 0;
        private string _lastDumpStatus = "";

        // Internal category keys matching the tab indices
        private readonly string[] _categoryKeys = new[]
        {
            "debug.tab.all",
            "debug.tab.setup1",
            "debug.tab.setup2",
            "debug.tab.setup3",
            "debug.tab.setup4",
            "debug.tab.alerts"
        };

        private void OnGUI()
        {
            if (Config == null || !Config.EnableDebugGroup.Value)
                return;

            _windowRect.width = Mathf.Min(1100f, Screen.width - 40f);
            _windowRect.height = Mathf.Min(700f, Screen.height - 40f);

            _windowRect = GUI.Window(992341, _windowRect, DrawWindow, LocalizationManager.T("debug.window.title", "Claim Monitor - Diagnostics Inspector V2 (Toggle: F3)"), GUI.skin.window);
        }

        private void DrawWindow(int windowId)
        {
            var scanner = ClaimScannerV2.Instance;

            // Row 1: Action Buttons & Live Scan Timers
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(LocalizationManager.T("debug.button.rescan", "Force Rescan"), GUILayout.Width(110), GUILayout.Height(24)))
            {
                scanner?.ForceScan();
            }

            GUILayout.Space(12);

            // Live Timers
            if (scanner != null)
            {
                string pollTime = $"{scanner.NextPollSeconds:F1}s";
                string topoTime = $"{scanner.NextTopologyScanSeconds:F1}s";

                string pollLabel = LocalizationManager.T("debug.timer.poll", "Poll Loop");
                string topoLabel = LocalizationManager.T("debug.timer.topo", "Topology Discovery");

                GUILayout.Label($"<b>{pollLabel}:</b> <color=#00FFFF>{pollTime}</color> | <b>{topoLabel}:</b> <color=#FFD700>{topoTime}</color>", GUILayout.Height(24));
            }

            if (!string.IsNullOrEmpty(_lastDumpStatus))
            {
                GUILayout.Space(10);
                GUILayout.Label($"<color=#7CFC00>{_lastDumpStatus}</color>", GUILayout.Height(24));
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // Row 2: Category Filter Bar
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationManager.T("debug.label.filter", "Filter:"), GUILayout.Width(45), GUILayout.Height(22));

            string[] categoryNames = new[]
            {
                LocalizationManager.T("debug.tab.all", "All"),
                LocalizationManager.T("debug.tab.setup1", "Setup 1 (HogPan)"),
                LocalizationManager.T("debug.tab.setup2", "Setup 2 (Mobile)"),
                LocalizationManager.T("debug.tab.setup3", "Setup 3 (Modular)"),
                LocalizationManager.T("debug.tab.setup4", "Setup 4 (Orange Beast)"),
                LocalizationManager.T("debug.tab.alerts", "Active Alerts")
            };

            for (int i = 0; i < categoryNames.Length; i++)
            {
                bool isActive = _selectedCategoryIndex == i;
                if (GUILayout.Toggle(isActive, categoryNames[i], "Button", GUILayout.ExpandWidth(false), GUILayout.Height(22)))
                {
                    _selectedCategoryIndex = i;
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            var data = ClaimScannerV2.Instance?.CurrentData;
            if (data == null)
            {
                GUILayout.Label(LocalizationManager.T("debug.label.empty", "No diagnostic data available."));
                GUI.DragWindow(new Rect(0, 0, 10000, 25));
                return;
            }

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true);

            int displayedCount = 0;

            // 1. Setup 1: Standalone HogPans
            if (_selectedCategoryIndex == 0 || _selectedCategoryIndex == 1)
            {
                foreach (var hp in data.HogPanAreas)
                {
                    displayedCount++;
                    DrawHogPanCard(hp);
                }
            }

            // 2. Setup 2: Mobile Plants (Trailers)
            if (_selectedCategoryIndex == 0 || _selectedCategoryIndex == 2)
            {
                foreach (var mp in data.MobilePlants)
                {
                    displayedCount++;
                    DrawMobilePlantCard(mp);
                }
            }

            // 3. Setup 3: Modular Plants
            if (_selectedCategoryIndex == 0 || _selectedCategoryIndex == 3)
            {
                foreach (var plant in data.ModularPlants)
                {
                    displayedCount++;
                    DrawModularPlantCard(plant);
                }
            }

            // 4. Setup 4: Orange Beast
            if (_selectedCategoryIndex == 0 || _selectedCategoryIndex == 4)
            {
                foreach (var beast in data.OrangeBeasts)
                {
                    displayedCount++;
                    DrawOrangeBeastCard(beast);
                }
            }

            // 5. Active Alerts
            if (_selectedCategoryIndex == 0 || _selectedCategoryIndex == 5)
            {
                if (data.ActiveAlerts.Count > 0)
                {
                    GUILayout.Space(6);
                    GUILayout.Label($"<b>--- {LocalizationManager.T("debug.header.alerts", "Active Alerts")} ---</b>");
                    foreach (var alert in data.ActiveAlerts)
                    {
                        displayedCount++;
                        DrawAlertCard(alert);
                    }
                }
            }

            GUILayout.EndScrollView();
            GUILayout.Label(LocalizationManager.Format("debug.label.totals", "Matching Items: {0} | Active Alerts: {1}", displayedCount, data.ActiveAlerts.Count));

            GUI.DragWindow(new Rect(0, 0, 10000, 25));
        }

        // ==========================================
        // Sub-Renderers for Setups
        // ==========================================

        private void DrawHogPanCard(HogPanAreaStatus hp)
        {
            string hogPanName = GameLocResolver.Resolve(GameLocResolver.KeyHogPanContainer, "Hog Pan");
            string waterNotReq = LocalizationManager.T("debug.status.not_required", "Not required");
            string waterStr = hp.RequiresWater ? FormatBool(hp.HasWater) : $"<color=grey>{waterNotReq}</color>";

            GUILayout.BeginVertical("Box");
            GUILayout.Label($"<b>[Setup 1] {hogPanName} #{hp.AreaIndex}</b> ({hp.ClaimName ?? $"Claim #{hp.ClaimId}"})");
            GUILayout.Label($"{LocalizationManager.T("debug.status.mounted", "Mounted")}: {FormatBool(hp.IsMounted)} | {LocalizationManager.T("alert.cat.water", "Water")}: {waterStr}");
            GUILayout.Label($"{LocalizationManager.T("debug.status.dirt_fill", "Dirt Box Fill")}: <b>{Mathf.RoundToInt(hp.DirtFillPct * 100f)}%</b> | {LocalizationManager.T("alert.cat.mats", "Mats")}: <b>{hp.InstalledMats}/{hp.TotalMats}</b> ({LocalizationManager.T("debug.status.max_fill", "Max Fill")}: <b>{Mathf.RoundToInt(hp.MaxMatFillPct * 100f)}%</b>)");
            GUILayout.EndVertical();
        }

        private void DrawMobilePlantCard(MobileWashPlantStatus mp)
        {
            string plantName = !string.IsNullOrEmpty(mp.VariantName) ? mp.VariantName : GameLocResolver.Resolve(GameLocResolver.KeyMobileWashPlant, "Mobile Wash Plant");

            GUILayout.BeginVertical("Box");
            GUILayout.Label($"<b>[Setup 2] {plantName}</b> ({mp.ClaimName ?? $"Claim #{mp.ClaimId}"})");
            GUILayout.Label($"{LocalizationManager.T("alert.cat.power", "Power")}: {FormatBool(mp.HasPower)} | {LocalizationManager.T("alert.cat.water", "Water")}: {FormatBool(mp.HasWater)} | {LocalizationManager.T("alert.cat.fuel", "Fuel")}: <b>{Mathf.RoundToInt(mp.FuelPct * 100f)}%</b>");
            GUILayout.Label($"{LocalizationManager.T("debug.status.intake_fill", "Intake Dirt")}: <b>{Mathf.RoundToInt(mp.DirtFillPct * 100f)}%</b> | {LocalizationManager.T("debug.status.bucket_fill", "Bucket Fill")}: <b>{Mathf.RoundToInt(mp.BucketFillPct * 100f)}%</b>");
            DrawPartsList(mp.Parts);
            GUILayout.EndVertical();
        }

        private void DrawModularPlantCard(ModularWashPlantStatus plant)
        {
            string plantTypeName = LocalizationManager.T("setup.name.stationary", "Modular Wash Plant");
            GUILayout.BeginVertical("Box");
            GUILayout.Label($"<b>[Setup 3] {plantTypeName}</b> ({plant.ClaimName ?? $"Claim #{plant.ClaimId}"}) - {LocalizationManager.T("debug.status.ready", "Ready")}: {FormatBool(plant.IsReadyToOperate)}");

            // Modules summary
            string mountedTag = LocalizationManager.T("debug.status.mounted", "Mounted");
            string offTag = LocalizationManager.T("debug.status.off", "Off");
            string pwrTag = LocalizationManager.T("alert.cat.power", "Power");
            string wtrTag = LocalizationManager.T("alert.cat.water", "Water");

            string shakerResolved = GameLocResolver.GetShakerName(plant.ShakerVariant);
            string trommelResolved = GameLocResolver.GetTrommelName(plant.TrommelVariant);

            string shakerStr = plant.ShakerMounted ? $"{shakerResolved} (<color=#7CFC00>{mountedTag}</color>, {pwrTag}: {FormatBool(plant.ShakerHasPower)}, {wtrTag}: {FormatBool(plant.ShakerHasWater)})" : $"{shakerResolved} (<color=red>{offTag}</color>)";
            string trommelStr = plant.TrommelMounted ? $"{trommelResolved} (<color=#7CFC00>{mountedTag}</color>, {pwrTag}: {FormatBool(plant.TrommelHasPower)})" : $"{trommelResolved} (<color=red>{offTag}</color>)";
            GUILayout.Label($"{shakerStr} | {trommelStr}");

            // Intake & Containers
            string intakeLabel = LocalizationManager.T("debug.status.intake_fill", "Intake Fill");
            string cratesLabel = LocalizationManager.T("debug.status.crates_fill", "Crates Fill");
            string trapsLabel = LocalizationManager.T("debug.status.traps", "Traps");
            GUILayout.Label($"{intakeLabel}: <b>{Mathf.RoundToInt(plant.PlantInputFillPct * 100f)}%</b> | {cratesLabel}: <b>{Mathf.RoundToInt(plant.MaxCrateFillPct * 100f)}%</b> ({trapsLabel}: {plant.SluiceGratesInstalled}/{plant.SluiceGratesTotal})");

            // Mats & Grilles
            string mainMatsLabel = LocalizationManager.T("debug.status.main_mats", "Main Mats");
            string grillesLabel = LocalizationManager.T("debug.status.grilles", "Grilles");
            string matsMaxFillLabel = LocalizationManager.T("debug.status.mats_max_fill", "Mats Max Fill");
            GUILayout.Label($"{mainMatsLabel}: <b>{plant.SluiceMatsInstalled}/{plant.SluiceMatsTotal}</b> | {grillesLabel}: <b>{plant.SluiceGrillesInstalled}/{plant.SluiceGrillesTotal}</b> | {matsMaxFillLabel}: <b>{Mathf.RoundToInt(plant.MaxMatFillPct * 100f)}%</b>");

            // Jigs & Buckets
            string jigBaseName = GameLocResolver.Resolve(GameLocResolver.KeyDuplexJig, "Duplex Jig");
            DrawJigSummary($"{jigBaseName} 1", plant.Jig1Mounted, plant.Jig1HasPower, plant.Jig1Bucket1, plant.Jig1Bucket2);
            DrawJigSummary($"{jigBaseName} 2", plant.Jig2Mounted, plant.Jig2HasPower, plant.Jig2Bucket1, plant.Jig2Bucket2);

            // Feeding Chain
            if (plant.FeedingChain != null && (plant.FeedingChain.HopperMounted || plant.FeedingChain.ConveyorBeltMounted))
            {
                DrawFeedingChainSummary(plant.FeedingChain);
            }

            // Maintenance Parts
            DrawPartsList(plant.Parts);

            GUILayout.EndVertical();
        }

        private void DrawOrangeBeastCard(OrangeBeastStatus beast)
        {
            string beastTypeName = LocalizationManager.T("setup.name.orange_beast", "Orange Beast");
            string pwrTag = LocalizationManager.T("alert.cat.power", "Power");
            string wtrTag = LocalizationManager.T("alert.cat.water", "Water");
            string intakeLabel = LocalizationManager.T("debug.status.intake_fill", "Intake Fill");
            string matsLabel = LocalizationManager.T("debug.status.xxl_mats", "XXL Mats");
            string grillesLabel = LocalizationManager.T("debug.status.grilles", "Grilles");
            string maxFillLabel = LocalizationManager.T("debug.status.max_fill", "Max Fill");

            GUILayout.BeginVertical("Box");
            GUILayout.Label($"<b>[Setup 4] {beastTypeName} #{beast.BeastIndex}</b> ({beast.ClaimName ?? $"Claim #{beast.ClaimId}"}) - {LocalizationManager.T("debug.status.ready", "Ready")}: {FormatBool(beast.IsReadyToOperate)}");
            GUILayout.Label($"{pwrTag}: {FormatBool(beast.HasPower)} | {wtrTag}: {FormatBool(beast.HasWater)} | {intakeLabel}: <b>{Mathf.RoundToInt(beast.PlantInputFillPct * 100f)}%</b>");
            GUILayout.Label($"{matsLabel}: <b>{beast.InstalledMats}/{beast.TotalMats}</b> ({maxFillLabel}: <b>{Mathf.RoundToInt(beast.MaxMatFillPct * 100f)}%</b>) | {grillesLabel}: <b>{beast.InstalledGrilles}/{beast.TotalGrilles}</b>");

            if (beast.FeedingChain != null && (beast.FeedingChain.HopperMounted || beast.FeedingChain.ConveyorBeltMounted))
            {
                DrawFeedingChainSummary(beast.FeedingChain);
            }

            DrawPartsList(beast.Parts);
            GUILayout.EndVertical();
        }

        private void DrawJigSummary(string name, bool mounted, bool power, JigBucketStatus b1, JigBucketStatus b2)
        {
            if (!mounted)
            {
                string notMountedStr = LocalizationManager.T("debug.status.not_mounted", "Not Mounted");
                GUILayout.Label($"{name}: <color=grey>{notMountedStr}</color>");
                return;
            }

            string pwr = FormatBool(power);
            string missingStr = LocalizationManager.T("debug.status.missing", "Missing");
            string noneStr = LocalizationManager.T("debug.status.none", "None");

            string b1Str = b1 != null && b1.HasSlot
                ? (b1.IsMounted ? $"B1: {Mathf.RoundToInt(b1.FillPct * 100f)}% ({b1.CurrentVolumeM3:F2}m³)" : $"B1: <color=yellow>{missingStr}</color>")
                : $"B1: <color=grey>{noneStr}</color>";

            string b2Str = b2 != null && b2.HasSlot
                ? (b2.IsMounted ? $"B2: {Mathf.RoundToInt(b2.FillPct * 100f)}% ({b2.CurrentVolumeM3:F2}m³)" : $"B2: <color=yellow>{missingStr}</color>")
                : $"B2: <color=grey>{noneStr}</color>";

            GUILayout.Label($"{name}: {LocalizationManager.T("alert.cat.power", "Power")}: {pwr} | {b1Str} | {b2Str}");
        }

        private void DrawFeedingChainSummary(FeederChainStatus chain)
        {
            string hopperTitle = GameLocResolver.Resolve(GameLocResolver.KeyConveyorHopper, "Hopper");
            string elevatorTitle = GameLocResolver.Resolve(GameLocResolver.KeyConveyorBelt, "Elevator");
            string pwrTag = LocalizationManager.T("alert.cat.power", "Power");
            string fillTag = LocalizationManager.T("debug.status.fill", "Fill");
            string offTag = LocalizationManager.T("debug.status.off", "Off");

            string hStr = chain.HopperMounted ? $"{hopperTitle} ({pwrTag}: {FormatBool(chain.HopperHasPower)}, {fillTag}: <b>{Mathf.RoundToInt(chain.HopperFillPct * 100f)}%</b>)" : $"{hopperTitle}: {offTag}";
            string eStr = chain.ConveyorBeltMounted ? $"{elevatorTitle} ({pwrTag}: {FormatBool(chain.ConveyorBeltHasPower)})" : $"{elevatorTitle}: {offTag}";

            GUILayout.Label($"<b>{LocalizationManager.T("alert.cat.feeding_chain", "Feeding Chain")}:</b> {hStr} | {eStr}");
            if (chain.Parts != null && chain.Parts.Count > 0)
            {
                DrawPartsList(chain.Parts);
            }
        }

        private void DrawPartsList(System.Collections.Generic.List<MachinePartStatus> parts)
        {
            if (parts == null || parts.Count == 0) return;

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginVertical();

            string reinforcedTag = $" [{LocalizationManager.T("debug.status.reinforced", "Reinforced")}]";

            foreach (var part in parts)
            {
                if (part == null) continue;
                string statusColor = (!part.IsInPlace || part.IsDestroyed) ? "red" : (part.Durability <= 0.2f ? "yellow" : "#7CFC00");
                string reinforced = part.IsReinforced ? reinforcedTag : "";
                string source = !string.IsNullOrEmpty(part.SourceComponent) ? $"[{part.SourceComponent}] " : "";

                string localizedName = LocalizationManager.ResolveGameText(part.Name);

                GUILayout.Label($"<color={statusColor}>• {source}{localizedName}: {Mathf.RoundToInt(part.Durability * 100f)}%{reinforced}</color>");
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawAlertCard(ClaimAlert alert)
        {
            string color = alert.Severity == AlertSeverity.Critical ? "red" : "yellow";
            GUILayout.BeginVertical("Box");
            GUILayout.Label($"<color={color}><b>[{alert.Severity}] {alert.Title}</b></color>");
            GUILayout.Label(alert.Description);
            GUILayout.EndVertical();
        }

        private string FormatBool(bool val)
        {
            return val ? "<color=#7CFC00>OK</color>" : "<color=red>NO</color>";
        }
    }
}