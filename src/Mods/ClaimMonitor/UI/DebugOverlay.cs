using Milex.GMS1.Mods.ClaimMonitor.Config;
using Milex.GMS1.Mods.ClaimMonitor.Diagnostics;
using Milex.GMS1.Mods.ClaimMonitor.Diagnostics.Models;
using Milex.GMS1.Core.Localization;
using UnityEngine;

namespace Milex.GMS1.Mods.ClaimMonitor.UI
{
    public class DebugOverlay : MonoBehaviour
    {
        public MonitorConfig Config { get; set; }

        private Rect _windowRect = new Rect(30f, 30f, 850f, 550f);
        private Vector2 _scrollPos = Vector2.zero;
        private string _selectedCategory = "All";
        private string _lastDumpStatus = "";

        private readonly string[] _categories = new[]
        {
            "All",
            "Setup 1 (Mobile)",
            "Setup 2 (Stationary)",
            "Setup 3 (Orange Beast)",
            "Conveyors & Feeders",
            "Vehicles",
            "Sluice Mats",
            "Utilities"
        };

        private void OnGUI()
        {
            if (Config == null || !Config.EnableDebugGroup.Value)
                return;

            _windowRect.width = Mathf.Min(1100f, Screen.width - 40f);
            _windowRect.height = Mathf.Min(650f, Screen.height - 40f);

            _windowRect = GUI.Window(992341, _windowRect, DrawWindow, LocalizationManager.T("debug.window.title", "Claim Monitor - Diagnostic Inspector (Toggle: F3)"), GUI.skin.window);
        }

        private void DrawWindow(int windowId)
        {
            // Row 1: Action Buttons & Status Notice
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(LocalizationManager.T("debug.button.rescan", "Force Rescan"), GUILayout.Width(110), GUILayout.Height(24)))
            {
                ClaimScanner.Instance?.ForceScan();
            }

            if (GUILayout.Button(LocalizationManager.T("debug.button.dump", "Dump All to File"), GUILayout.Width(130), GUILayout.Height(24)))
            {
                try
                {
                    string path = ClaimDumper.DumpClaimToFile();
                    _lastDumpStatus = LocalizationManager.Format("debug.dump.saved", "Dump saved to: {0}", System.IO.Path.GetFileName(path));
                    ClaimMonitorPlugin.Instance?.LogInfo($"Dump written to: {path}");
                }
                catch (System.Exception ex)
                {
                    _lastDumpStatus = LocalizationManager.Format("debug.dump.failed", "Dump failed: {0}", ex.Message);
                }
            }

            if (!string.IsNullOrEmpty(_lastDumpStatus))
            {
                GUILayout.Space(10);
                GUILayout.Label($"<color=#7CFC00>{_lastDumpStatus}</color>");
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // Row 2: Category Filter Bar
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationManager.T("debug.label.filter", "Filter:"), GUILayout.Width(45), GUILayout.Height(22));

            foreach (var cat in _categories)
            {
                bool isActive = _selectedCategory == cat;
                if (GUILayout.Toggle(isActive, cat, "Button", GUILayout.ExpandWidth(false), GUILayout.Height(22)))
                {
                    _selectedCategory = cat;
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            var items = ClaimScanner.Instance?.CurrentData?.RawInspectionItems;
            if (items == null || items.Count == 0)
            {
                GUILayout.Label(LocalizationManager.T("debug.label.empty", "No scanned objects found yet. Make sure you are in a claim scene and press 'Force Rescan'."));
                GUI.DragWindow(new Rect(0, 0, 10000, 25));
                return;
            }

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true);

            int displayedCount = 0;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                bool matches = false;
                if (_selectedCategory == "All") matches = true;
                else if (_selectedCategory == "Setup 1 (Mobile)") matches = (item.Setup == WashPlantSetupType.Setup1_Mobile || item.Category == "Setup 1 (Mobile)");
                else if (_selectedCategory == "Setup 2 (Stationary)") matches = (item.Setup == WashPlantSetupType.Setup2_Stationary || item.Category == "Setup 2 (Stationary)");
                else if (_selectedCategory == "Setup 3 (Orange Beast)") matches = (item.Setup == WashPlantSetupType.Setup3_OrangeBeast || item.Category == "Setup 3 (Orange Beast)");
                else if (_selectedCategory == item.Category) matches = true;

                if (!matches) continue;

                displayedCount++;
                GUILayout.BeginVertical("Box");
                GUILayout.BeginHorizontal();
                GUILayout.Label($"<b>[{item.Category}]</b> {item.TypeName} (GO: '{item.GameObjectName}', ID: {item.InstanceId})");
                GUILayout.FlexibleSpace();
                GUILayout.Label($"Pos: ({item.Position.x:F1}, {item.Position.y:F1}, {item.Position.z:F1})");
                GUILayout.EndHorizontal();

                GUILayout.Label($"State: {item.Details}");
                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
            GUILayout.Label(LocalizationManager.Format("debug.label.totals", "Total Objects: {0} | Matching Filter: {1}", items.Count, displayedCount));

            GUI.DragWindow(new Rect(0, 0, 10000, 25));
        }
    }
}