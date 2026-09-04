using Milex.GMS1.Mods.ClaimMonitor.Config;
using Milex.GMS1.Mods.ClaimMonitor.Diagnostics;
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
            "Vehicle",
            "Mat / Sluice",
            "Feeder / Conveyor",
            "Fuel",
            "Setup 1 (Mobile)",
            "Setup 2 (Stationary)",
            "Setup 3 (Beast)"
        };

        private void OnGUI()
        {
            if (Config == null || !Config.EnableDebugGroup.Value)
                return;

            _windowRect.width = Mathf.Min(900f, Screen.width - 40f);
            _windowRect.height = Mathf.Min(600f, Screen.height - 40f);

            GUI.depth = -1000;
            _windowRect = GUI.Window(992341, _windowRect, DrawWindow, "Claim Monitor - Diagnostic Inspector (Toggle: F8)");
        }

        private void DrawWindow(int windowId)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Force Rescan", GUILayout.Width(100)))
            {
                ClaimScanner.Instance?.ForceScan();
            }

            if (GUILayout.Button("Dump All to File", GUILayout.Width(130)))
            {
                try
                {
                    string path = ClaimDumper.DumpClaimToFile();
                    _lastDumpStatus = $"Dump saved to: {System.IO.Path.GetFileName(path)}";
                    ClaimMonitorPlugin.Instance?.LogInfo($"Dump written to: {path}");
                }
                catch (System.Exception ex)
                {
                    _lastDumpStatus = $"Dump failed: {ex.Message}";
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("Filter:", GUILayout.Width(45));

            foreach (var cat in _categories)
            {
                bool isActive = _selectedCategory == cat;
                if (GUILayout.Toggle(isActive, cat, "Button", GUILayout.ExpandWidth(false)))
                {
                    _selectedCategory = cat;
                }
            }

            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_lastDumpStatus))
            {
                GUILayout.Label($"<color=#7CFC00>{_lastDumpStatus}</color>");
            }

            GUILayout.Space(4);

            var items = ClaimScanner.Instance?.CurrentData?.RawInspectionItems;
            if (items == null || items.Count == 0)
            {
                GUILayout.Label("No scanned objects found yet. Make sure you are in a claim scene and press 'Force Rescan'.");
                GUI.DragWindow(new Rect(0, 0, 10000, 25));
                return;
            }

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true);

            int displayedCount = 0;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (_selectedCategory != "All" && item.Category != _selectedCategory)
                    continue;

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
            GUILayout.Label($"Total Objects: {items.Count} | Matching Filter: {displayedCount}");

            GUI.DragWindow(new Rect(0, 0, 10000, 25));
        }
    }
}