using System;
using System.Collections.Generic;
using Milex.GMS1.Core;
using Milex.GMS1.Mods.ClaimMonitor.Config;
using Milex.GMS1.Mods.ClaimMonitor.Diagnostics;
using Milex.GMS1.Mods.ClaimMonitor.Diagnostics.Models;
using UnityEngine;

namespace Milex.GMS1.Mods.ClaimMonitor.UI
{
    public class WarningHUD : MonoBehaviour
    {
        public MonitorConfig Config { get; set; }

        private Rect _windowRect;
        private Vector2 _scrollPos = Vector2.zero;
        private bool _isMinimized = false;
        private float _lastSaveTime = 0f;

        // Custom GUIStyles
        private GUIStyle _windowStyle;
        private GUIStyle _criticalBoxStyle;
        private GUIStyle _warningBoxStyle;
        private GUIStyle _nominalBoxStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _descStyle;

        private Texture2D _bgTex;
        private Texture2D _criticalTex;
        private Texture2D _warningTex;
        private Texture2D _nominalTex;

        private void Awake()
        {
            _windowRect = new Rect(20f, 100f, 340f, 380f);
        }

        private void Start()
        {
            if (Config != null)
            {
                _windowRect = new Rect(Config.HudPosX.Value, Config.HudPosY.Value, Config.HudMaxWidth.Value, Config.HudMaxHeight.Value);
            }
        }

        private void InitStyles()
        {
            if (_windowStyle != null) return;

            _bgTex = MakeTex(2, 2, new Color(0.09f, 0.11f, 0.16f, 0.94f));
            _criticalTex = MakeTex(2, 2, new Color(0.42f, 0.12f, 0.12f, 0.92f));
            _warningTex = MakeTex(2, 2, new Color(0.38f, 0.28f, 0.08f, 0.92f));
            _nominalTex = MakeTex(2, 2, new Color(0.10f, 0.25f, 0.16f, 0.92f));

            _windowStyle = new GUIStyle(GUI.skin.window)
            {
                normal = { background = _bgTex, textColor = new Color(0.92f, 0.72f, 0.20f, 1f) },
                onNormal = { background = _bgTex, textColor = new Color(0.92f, 0.72f, 0.20f, 1f) },
                padding = new RectOffset(8, 8, 22, 8),
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };

            _criticalBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = _criticalTex, textColor = Color.white },
                padding = new RectOffset(6, 6, 4, 4),
                margin = new RectOffset(0, 0, 2, 4)
            };

            _warningBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = _warningTex, textColor = Color.white },
                padding = new RectOffset(6, 6, 4, 4),
                margin = new RectOffset(0, 0, 2, 4)
            };

            _nominalBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = _nominalTex, textColor = new Color(0.85f, 1f, 0.85f, 1f) },
                padding = new RectOffset(6, 6, 6, 6),
                margin = new RectOffset(0, 0, 2, 4)
            };

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };

            _descStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                normal = { textColor = new Color(0.88f, 0.88f, 0.88f, 1f) },
                wordWrap = true
            };
        }

        private void OnGUI()
        {
            if (Config == null || !Config.HudEnabled.Value)
                return;

            InitStyles();

            var data = ClaimScanner.Instance?.CurrentData;
            var alerts = data?.ActiveAlerts;
            bool hasAlerts = alerts != null && alerts.Count > 0;

            // Optional setting: Only show HUD if alerts exist
            if (Config.HudOnlyShowWarnings.Value && !hasAlerts)
                return;

            // Apply configured sizes and screen boundaries
            float currentWidth = Mathf.Min(Config.HudMaxWidth.Value, Screen.width - 20f);
            float currentHeight = _isMinimized || Config.HudCompactMode.Value
                ? 62f
                : Mathf.Min(Config.HudMaxHeight.Value, Screen.height - 40f);

            _windowRect.width = currentWidth;
            _windowRect.height = currentHeight;
            _windowRect.x = Mathf.Clamp(_windowRect.x, 0f, Screen.width - _windowRect.width);
            _windowRect.y = Mathf.Clamp(_windowRect.y, 0f, Screen.height - _windowRect.height);

            // Determine header title
            int criticalCount = 0;
            int warningCount = 0;
            if (hasAlerts)
            {
                foreach (var a in alerts)
                {
                    if (a.Severity == AlertSeverity.Critical) criticalCount++;
                    else if (a.Severity == AlertSeverity.Warning) warningCount++;
                }
            }

            string headerBadge = "[OK]";
            if (criticalCount > 0)
                headerBadge = $"[! {criticalCount} CRITICAL]";
            else if (warningCount > 0)
                headerBadge = $"[^ {warningCount} WARNINGS]";

            string title = $"Claim Monitor  {headerBadge}";

            GUI.depth = -500;
            _windowRect = GUI.Window(887123, _windowRect, DrawWindow, title, _windowStyle);

            // Persist dragged position
            if (Event.current.type == EventType.Repaint && (_windowRect.x != Config.HudPosX.Value || _windowRect.y != Config.HudPosY.Value))
            {
                if (Time.realtimeSinceStartup - _lastSaveTime > 1.5f)
                {
                    _lastSaveTime = Time.realtimeSinceStartup;
                    Config.HudPosX.Value = (float)Math.Round(_windowRect.x, 0);
                    Config.HudPosY.Value = (float)Math.Round(_windowRect.y, 0);
                    Config.HudPosX.ConfigFile?.Save();
                }
            }
        }

        private void DrawWindow(int windowId)
        {
            var data = ClaimScanner.Instance?.CurrentData;
            var alerts = data?.ActiveAlerts;
            bool hasAlerts = alerts != null && alerts.Count > 0;

            // Header Bar Buttons
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            string toggleLabel = _isMinimized ? "[ + Expand ]" : "[ - Minimize ]";
            if (GUILayout.Button(toggleLabel, GUILayout.Height(18), GUILayout.Width(80)))
            {
                _isMinimized = !_isMinimized;
            }
            GUILayout.EndHorizontal();

            // Compact / Minimized View
            if (_isMinimized || Config.HudCompactMode.Value)
            {
                GUILayout.BeginVertical(_nominalBoxStyle);
                if (!hasAlerts)
                {
                    GUILayout.Label("<b>STATUS:</b> All Systems Operational", _titleStyle);
                }
                else
                {
                    GUILayout.Label($"<b>ATTENTION:</b> {alerts.Count} issue(s) detected. Click Expand.", _titleStyle);
                }
                GUILayout.EndVertical();

                GUI.DragWindow(new Rect(0, 0, 10000, 24));
                return;
            }

            // Overview Summary
            int matCount = data?.Mats.Count ?? 0;
            int plantCount = data?.PlantComponents.Count ?? 0;
            int vehicleCount = data?.Vehicles.Count ?? 0;

            GUILayout.BeginHorizontal();
            GUILayout.Label($"<color=#D4AF37>Equipment:</color> {plantCount} Plants | {matCount} Mats | {vehicleCount} Vehicles", _descStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            // Alerts View
            if (!hasAlerts)
            {
                GUILayout.BeginVertical(_nominalBoxStyle);
                GUILayout.Label("<color=#7CFC00><b>ALL SYSTEMS NOMINAL</b></color>", _titleStyle);
                GUILayout.Label("No component failures, low fuel, or mat overfills detected across active claims.", _descStyle);
                GUILayout.EndVertical();
            }
            else
            {
                _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, false);

                for (int i = 0; i < alerts.Count; i++)
                {
                    var alert = alerts[i];
                    GUIStyle boxStyle = alert.Severity == AlertSeverity.Critical ? _criticalBoxStyle : _warningBoxStyle;
                    string badgeColor = alert.Severity == AlertSeverity.Critical ? "#FF4500" : "#FFD700";
                    string badgeText = alert.Severity == AlertSeverity.Critical ? "[CRITICAL]" : "[WARNING]";

                    GUILayout.BeginVertical(boxStyle);
                    GUILayout.Label($"<color={badgeColor}><b>{badgeText}</b></color> <b>{alert.Title}</b>", _titleStyle);
                    GUILayout.Label(alert.Description, _descStyle);
                    GUILayout.EndVertical();
                }

                GUILayout.EndScrollView();
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 24));
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void OnDestroy()
        {
            if (_bgTex) Destroy(_bgTex);
            if (_criticalTex) Destroy(_criticalTex);
            if (_warningTex) Destroy(_warningTex);
            if (_nominalTex) Destroy(_nominalTex);
        }
    }
}
