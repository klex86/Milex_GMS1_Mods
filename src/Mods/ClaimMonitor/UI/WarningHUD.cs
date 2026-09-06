using System;
using System.Collections.Generic;
using Milex.GMS1.Core;
using Milex.GMS1.Mods.ClaimMonitor.Config;
using Milex.GMS1.Mods.ClaimMonitor.Diagnostics;
using Milex.GMS1.Mods.ClaimMonitor.Diagnostics.Models;
using Milex.GMS1.Core.Localization;
using UnityEngine;

namespace Milex.GMS1.Mods.ClaimMonitor.UI
{
    public class WarningHUD : MonoBehaviour
    {
        public MonitorConfig Config { get; set; }

        private Rect _windowRect;
        private Vector2 _scrollPos = Vector2.zero;
        private float _lastSaveTime = 0f;

        // Custom GUIStyles
        private GUIStyle _windowStyle;
        private GUIStyle _compactWindowStyle;
        private GUIStyle _criticalBoxStyle;
        private GUIStyle _warningBoxStyle;
        private GUIStyle _nominalBoxStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _descStyle;
        private GUIStyle _compactItemStyle;

        private Texture2D _bgTex;
        private Texture2D _compactBgTex;
        private Texture2D _criticalTex;
        private Texture2D _warningTex;
        private Texture2D _nominalTex;

        private void Awake()
        {
            _windowRect = new Rect(20f, 100f, 340f, 380f);
        }

        private void Start()
        {
            float posX = PlayerPrefs.GetFloat("Milex_ClaimMonitor_HudPosX", 20f);
            float posY = PlayerPrefs.GetFloat("Milex_ClaimMonitor_HudPosY", 100f);
            float width = Config != null ? Config.HudMaxWidth.Value : 340f;
            float height = Config != null ? Config.HudMaxHeight.Value : 380f;
            _windowRect = new Rect(posX, posY, width, height);
        }

        private void InitStyles()
        {
            if (_windowStyle != null) return;

            _bgTex = MakeTex(2, 2, new Color(0.09f, 0.11f, 0.16f, 0.94f));
            _compactBgTex = MakeTex(2, 2, new Color(0.06f, 0.08f, 0.12f, 0.82f));
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

            _compactWindowStyle = new GUIStyle(GUI.skin.window)
            {
                normal = { background = _compactBgTex, textColor = new Color(0.92f, 0.72f, 0.20f, 1f) },
                onNormal = { background = _compactBgTex, textColor = new Color(0.92f, 0.72f, 0.20f, 1f) },
                padding = new RectOffset(8, 8, 20, 6),
                fontSize = 11,
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

            _compactItemStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = Color.white },
                wordWrap = true,
                padding = new RectOffset(0, 0, 1, 1),
                margin = new RectOffset(0, 0, 1, 1)
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

            bool isCompact = Config?.HudCompactMode?.Value ?? false;

            // Apply configured sizes and screen boundaries
            float currentWidth = isCompact
                ? Mathf.Clamp(Config.HudMaxWidth.Value, 280f, 450f)
                : Mathf.Min(Config.HudMaxWidth.Value, Screen.width - 20f);

            float currentHeight;
            if (isCompact)
            {
                if (!hasAlerts)
                {
                    currentHeight = 54f;
                }
                else
                {
                    // Calculate dynamic height based on text content length to avoid any truncation
                    float estimatedTotalTextHeight = 28f; // Window Header
                    float contentWidth = Mathf.Max(200f, currentWidth - 30f);
                    float charsPerLine = contentWidth / 7.2f;

                    for (int i = 0; i < alerts.Count; i++)
                    {
                        string line = $"• [WARN] {alerts[i].Title}: {alerts[i].Description}";
                        int estimatedLines = Mathf.Max(1, Mathf.CeilToInt(line.Length / charsPerLine));
                        estimatedTotalTextHeight += (estimatedLines * 16f) + 6f;
                    }
                    currentHeight = Mathf.Clamp(estimatedTotalTextHeight + 14f, 54f, Screen.height - 40f);
                }
            }
            else
            {
                if (!hasAlerts)
                {
                    currentHeight = 120f;
                }
                else
                {
                    // Full window mode: window title (28) + equipment summary header (28) + padding (16)
                    float estimatedTotalTextHeight = 72f;
                    float contentWidth = Mathf.Max(250f, currentWidth - 40f);
                    float charsPerLine = contentWidth / 7.5f;

                    for (int i = 0; i < alerts.Count; i++)
                    {
                        // Card header: ~20px + Box padding/margins: ~16px
                        float cardHeight = 36f;
                        string desc = alerts[i].Description ?? "";
                        int descLines = Mathf.Max(1, Mathf.CeilToInt(desc.Length / charsPerLine));
                        cardHeight += (descLines * 16f);
                        estimatedTotalTextHeight += cardHeight;
                    }

                    // Dynamically fit all content without scrolling, constrained only by the screen itself
                    currentHeight = Mathf.Clamp(estimatedTotalTextHeight, 120f, Screen.height - 40f);
                }
            }

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

            string headerBadge = LocalizationManager.T("hud.badge.ok", "[OK]");
            if (criticalCount > 0)
                headerBadge = LocalizationManager.Format("hud.badge.critical", "[! {0} CRITICAL]", criticalCount);
            else if (warningCount > 0)
                headerBadge = LocalizationManager.Format("hud.badge.warning", "[^ {0} WARNINGS]", warningCount);

            string title = isCompact
                ? LocalizationManager.Format("hud.header.warnings", "Claim Warnings {0}", headerBadge)
                : LocalizationManager.Format("hud.header.monitor", "Claim Monitor {0}", headerBadge);

            GUI.depth = -500;
            var activeStyle = isCompact ? _compactWindowStyle : _windowStyle;
            _windowRect = GUI.Window(887123, _windowRect, DrawWindow, title, activeStyle);

            // Persist dragged position to PlayerPrefs
            float savedX = PlayerPrefs.GetFloat("Milex_ClaimMonitor_HudPosX", 20f);
            float savedY = PlayerPrefs.GetFloat("Milex_ClaimMonitor_HudPosY", 100f);
            if (Event.current.type == EventType.Repaint && (Mathf.Abs(_windowRect.x - savedX) > 1f || Mathf.Abs(_windowRect.y - savedY) > 1f))
            {
                if (Time.realtimeSinceStartup - _lastSaveTime > 1.5f)
                {
                    _lastSaveTime = Time.realtimeSinceStartup;
                    PlayerPrefs.SetFloat("Milex_ClaimMonitor_HudPosX", (float)Math.Round(_windowRect.x, 0));
                    PlayerPrefs.SetFloat("Milex_ClaimMonitor_HudPosY", (float)Math.Round(_windowRect.y, 0));
                    PlayerPrefs.Save();
                }
            }
        }

        private void DrawWindow(int windowId)
        {
            var data = ClaimScanner.Instance?.CurrentData;
            var alerts = data?.ActiveAlerts;
            bool hasAlerts = alerts != null && alerts.Count > 0;
            bool isCompact = Config?.HudCompactMode?.Value ?? false;

            // Compact View: Compact list of active warnings
            if (isCompact)
            {
                if (!hasAlerts)
                {
                    GUILayout.Label(string.Format("<color=#7CFC00>[OK]</color> {0}", LocalizationManager.T("hud.compact.all_operational", "All Systems Operational")), _compactItemStyle);
                }
                else
                {
                    for (int i = 0; i < alerts.Count; i++)
                    {
                        var alert = alerts[i];
                        string bulletColor = alert.Severity == AlertSeverity.Critical ? "#FF4500" : "#FFD700";
                        string tag = alert.Severity == AlertSeverity.Critical ? LocalizationManager.T("hud.tag.critical", "CRITICAL") : LocalizationManager.T("hud.tag.warn", "WARN");
                        GUILayout.Label($"<color={bulletColor}><b>• [{tag}]</b></color> <b>{alert.Title}:</b> {alert.Description}", _compactItemStyle);
                    }
                }

                GUI.DragWindow(new Rect(0, 0, 10000, 10000));
                return;
            }

            // Overview Summary (Full Window View)
            int matCount = data?.Mats.Count ?? 0;
            int plantCount = data?.PlantComponents.Count ?? 0;
            int vehicleCount = data?.Vehicles.Count ?? 0;

            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationManager.Format("hud.equipment_summary", "<color=#D4AF37>Equipment:</color> {0} Plants | {1} Mats | {2} Vehicles", plantCount, matCount, vehicleCount), _descStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            // Alerts View (Full Window View)
            if (!hasAlerts)
            {
                GUILayout.BeginVertical(_nominalBoxStyle);
                GUILayout.Label($"<color=#7CFC00><b>{LocalizationManager.T("hud.all_nominal", "ALL SYSTEMS NOMINAL")}</b></color>", _titleStyle);
                GUILayout.Label(LocalizationManager.T("hud.nominal_desc", "No component failures, low fuel, or mat overfills detected across active claims."), _descStyle);
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
                    string badgeText = alert.Severity == AlertSeverity.Critical ? $"[{LocalizationManager.T("hud.critical", "CRITICAL")}]" : $"[{LocalizationManager.T("hud.warning", "WARNING")}]";

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
