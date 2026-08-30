using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using Milex.GMS1.Core.Localization;
using UnityEngine;

namespace Milex.GMS1.Core.UI
{
    /// <summary>
    /// Ingame Mod Configuration GUI using Unity IMGUI.
    /// Features:
    /// - Matrix-based UI Scaling (High-DPI / 4K support)
    /// - Stylized single-toggle checkboxes
    /// - Scrollable dropdown with native endonym language selection
    /// - Missing translation detection & template creation dialog
    /// - Excludes CoreMod from feature mods list to prevent duplication
    /// </summary>
    public class ModMenuUI : MonoBehaviour
    {
        private Rect _baseWindowRect = new Rect(100, 100, 840, 560);
        private float _lastScale = 1.0f; // Tracks previous scale to anchor window top-left on scale change
        private float _currentScale = 1.0f; // Current frame scale, used inside DrawWindowContent

        // Navigation: -1 = Core Settings, >= 0 = Index in non-core mods list
        private int _selectedModIndex = -1;
        private Vector2 _sidebarScrollPos;
        private Vector2 _contentScrollPos;

        // Key rebinding state
        private ConfigEntry<KeyCode> _rebindingEntry;
        private string _rebindingLabel;

        // Language dropdown state
        private bool _isLanguageDropdownOpen = false;
        private Vector2 _langDropdownScrollPos;

        // Missing translation dialog state
        private bool _showMissingDialog = false;
        private string _missingDialogTargetLang;
        private List<string> _missingModsList = new List<string>();

        // Custom GUI styles & textures
        private GUIStyle _windowStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _subHeaderStyle;
        private GUIStyle _sidebarButtonStyle;
        private GUIStyle _sidebarButtonActiveStyle;
        private GUIStyle _sectionHeaderStyle;
        private GUIStyle _entryLabelStyle;
        private GUIStyle _entryDescStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _buttonActiveStyle;
        private GUIStyle _checkboxStyle;
        private GUIStyle _checkboxActiveStyle;
        private GUIStyle _dropdownHeaderStyle;
        private GUIStyle _dropdownItemStyle;
        private GUIStyle _dropdownItemActiveStyle;
        private GUIStyle _dialogStyle;
        private GUIStyle _textFieldStyle;
        private GUIStyle _sliderTrackStyle;
        private GUIStyle _sliderThumbStyle;

        private Texture2D _bgTexture;
        private Texture2D _sidebarBgTexture;
        private Texture2D _buttonTex;
        private Texture2D _buttonActiveTex;
        private Texture2D _dialogBgTex;

        private bool _stylesInitialized = false;

        private static readonly string CoreAssemblyName = typeof(CorePlugin).Assembly.GetName().Name;

        private string L(string key, string fallback) => LocalizationManager.Translate(CoreAssemblyName, key, fallback);

        private List<ModInfo> GetFeatureMods()
        {
            return ModRegistry.RegisteredMods
                .Where(m => !m.Guid.Equals(CorePlugin.PluginGuid, StringComparison.OrdinalIgnoreCase) 
                         && !m.AssemblyName.Equals(CoreAssemblyName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private void Update()
        {
            if (_rebindingEntry != null)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    _rebindingEntry = null;
                    _rebindingLabel = null;
                }
                else
                {
                    foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                    {
                        if (key == KeyCode.None || key == KeyCode.Escape) continue;

                        if (Input.GetKeyDown(key))
                        {
                            _rebindingEntry.Value = key;
                            _rebindingEntry.ConfigFile?.Save();
                            _rebindingEntry = null;
                            _rebindingLabel = null;
                            break;
                        }
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (!CorePlugin.IsMenuOpen) return;

            InitStyles();

            // Handle Matrix-based UI scaling for High-DPI / 4K displays
            float scale = CorePlugin.UIScale != null ? Mathf.Clamp(CorePlugin.UIScale.Value, 0.8f, 1.8f) : 1.0f;

            // When scale changes, adjust the GUI-space position so the window's
            // top-left screen-space corner stays anchored. The matrix scales from
            // the screen origin (0,0), so we compensate: newGuiPos = oldGuiPos * (oldScale / newScale).
            if (!Mathf.Approximately(scale, _lastScale))
            {
                float ratio = _lastScale / scale;
                _baseWindowRect.x *= ratio;
                _baseWindowRect.y *= ratio;
                _lastScale = scale;
            }

            _currentScale = scale;

            Matrix4x4 originalMatrix = GUI.matrix;

            if (Math.Abs(scale - 1.0f) > 0.01f)
            {
                GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1.0f));
            }

            // Clamp window dimensions to scaled screen space
            float scaledScreenWidth = Screen.width / scale;
            float scaledScreenHeight = Screen.height / scale;

            _baseWindowRect.x = Mathf.Clamp(_baseWindowRect.x, 0, scaledScreenWidth - _baseWindowRect.width);
            _baseWindowRect.y = Mathf.Clamp(_baseWindowRect.y, 0, scaledScreenHeight - _baseWindowRect.height);

            // Draw main window
            _baseWindowRect = GUI.Window(987654, _baseWindowRect, DrawWindowContent, "", _windowStyle);

            // Draw Missing Translation Modal Dialog if open
            if (_showMissingDialog)
            {
                Rect dialogRect = new Rect((scaledScreenWidth - 560) / 2, (scaledScreenHeight - 340) / 2, 560, 340);
                GUI.Window(987655, dialogRect, DrawMissingTranslationDialog, "", _dialogStyle);
            }

            // Consume all mouse and scroll wheel events over the menu to prevent clicking through into game world
            Vector2 mousePos = Event.current.mousePosition;
            if (_baseWindowRect.Contains(mousePos) || _showMissingDialog)
            {
                int eventType = (int)Event.current.type;
                // 0 = MouseDown, 1 = MouseUp, 6 = ScrollWheel
                if (eventType == 0 || eventType == 1 || eventType == 6)
                {
                    Event.current.Use();
                }
            }

            GUI.matrix = originalMatrix;
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;

            _bgTexture = MakeTex(2, 2, new Color(0.11f, 0.11f, 0.13f, 0.97f));
            _sidebarBgTexture = MakeTex(2, 2, new Color(0.08f, 0.08f, 0.10f, 0.98f));
            _buttonTex = MakeTex(2, 2, new Color(0.20f, 0.20f, 0.24f, 1.0f));
            _buttonActiveTex = MakeTex(2, 2, new Color(0.85f, 0.65f, 0.13f, 1.0f)); // Gold accent
            _dialogBgTex = MakeTex(2, 2, new Color(0.14f, 0.14f, 0.17f, 0.99f));

            _windowStyle = new GUIStyle(GUI.skin.window)
            {
                normal = { background = _bgTexture },
                onNormal = { background = _bgTexture },
                border = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(0, 0, 0, 0)
            };

            _dialogStyle = new GUIStyle(GUI.skin.window)
            {
                normal = { background = _dialogBgTex },
                border = new RectOffset(6, 6, 6, 6),
                padding = new RectOffset(16, 16, 16, 16)
            };

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.80f, 0.20f) },
                alignment = TextAnchor.MiddleLeft
            };

            _subHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.70f, 0.70f, 0.70f) },
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false
            };

            _sidebarButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Normal,
                richText = true,
                normal = { background = _buttonTex, textColor = new Color(0.85f, 0.85f, 0.85f) },
                hover = { background = MakeTex(2, 2, new Color(0.28f, 0.28f, 0.32f, 1.0f)), textColor = Color.white },
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 8, 8)
            };

            _sidebarButtonActiveStyle = new GUIStyle(_sidebarButtonStyle)
            {
                fontStyle = FontStyle.Bold,
                richText = true,
                normal = { background = _buttonActiveTex, textColor = Color.black },
                hover = { background = _buttonActiveTex, textColor = Color.black }
            };

            _sectionHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.80f, 0.20f) },
                margin = new RectOffset(0, 0, 10, 4),
                wordWrap = false
            };

            _entryLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                wordWrap = false
            };

            _entryDescStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Italic,
                normal = { textColor = new Color(0.65f, 0.65f, 0.65f) },
                wordWrap = true
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                normal = { background = _buttonTex, textColor = Color.white },
                hover = { background = MakeTex(2, 2, new Color(0.30f, 0.30f, 0.35f, 1.0f)), textColor = Color.white },
                padding = new RectOffset(8, 8, 5, 5)
            };

            _buttonActiveStyle = new GUIStyle(_buttonStyle)
            {
                normal = { background = _buttonActiveTex, textColor = Color.black },
                hover = { background = _buttonActiveTex, textColor = Color.black }
            };

            _checkboxStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                normal = { background = _buttonTex, textColor = new Color(0.8f, 0.8f, 0.8f) },
                hover = { background = MakeTex(2, 2, new Color(0.28f, 0.28f, 0.32f, 1.0f)), textColor = Color.white },
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 6, 6)
            };

            _checkboxActiveStyle = new GUIStyle(_checkboxStyle)
            {
                fontStyle = FontStyle.Bold,
                normal = { background = MakeTex(2, 2, new Color(0.20f, 0.45f, 0.20f, 1.0f)), textColor = new Color(0.4f, 1.0f, 0.4f) },
                hover = { background = MakeTex(2, 2, new Color(0.25f, 0.55f, 0.25f, 1.0f)), textColor = Color.white }
            };

            _dropdownHeaderStyle = new GUIStyle(_buttonStyle)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 6, 6)
            };

            _dropdownItemStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                normal = { background = MakeTex(2, 2, new Color(0.18f, 0.18f, 0.22f, 1.0f)), textColor = Color.white },
                hover = { background = MakeTex(2, 2, new Color(0.32f, 0.32f, 0.38f, 1.0f)), textColor = Color.white },
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(12, 10, 6, 6)
            };

            _dropdownItemActiveStyle = new GUIStyle(_dropdownItemStyle)
            {
                fontStyle = FontStyle.Bold,
                normal = { background = _buttonActiveTex, textColor = Color.black },
                hover = { background = _buttonActiveTex, textColor = Color.black }
            };

            _textFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 12,
                normal = { textColor = Color.white },
                padding = new RectOffset(4, 4, 3, 3)
            };

            // Slider track: clearly visible contrasting trough with bright border
            Texture2D trackTex = MakeBorderedTex(12, 12, new Color(0.22f, 0.23f, 0.28f, 1.0f), new Color(0.48f, 0.50f, 0.60f, 1.0f), 1);
            _sliderTrackStyle = new GUIStyle(GUI.skin.horizontalSlider)
            {
                normal   = { background = trackTex },
                border   = new RectOffset(2, 2, 2, 2),
                padding  = new RectOffset(0, 0, 0, 0),
                fixedHeight = 10f,
                margin   = new RectOffset(0, 0, 6, 6)
            };

            // Slider thumb: vibrant gold grabber symmetrically centered via overflow
            Texture2D thumbNormal = MakeBorderedTex(12, 12, new Color(0.88f, 0.70f, 0.15f, 1.0f), new Color(1.0f, 0.92f, 0.50f, 1.0f), 1);
            Texture2D thumbHover  = MakeBorderedTex(12, 12, new Color(1.00f, 0.82f, 0.25f, 1.0f), new Color(1.0f, 1.0f, 0.80f, 1.0f), 1);
            Texture2D thumbActive = MakeBorderedTex(12, 12, new Color(1.00f, 0.95f, 0.50f, 1.0f), Color.white, 1);

            _sliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb)
            {
                normal   = { background = thumbNormal },
                hover    = { background = thumbHover },
                active   = { background = thumbActive },
                fixedWidth  = 14f,
                fixedHeight = 10f,
                overflow = new RectOffset(0, 0, 5, 5)
            };

            _stylesInitialized = true;
        }

        private void DrawWindowContent(int windowId)
        {
            // Title Bar
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.BeginVertical();
            GUILayout.Space(8);
            GUILayout.Label(L("menu.title", "MILEX GMS1 MODS"), _headerStyle);
            GUILayout.Label(L("menu.subtitle", "Gold Mining Simulator Modding Framework"), _subHeaderStyle);
            GUILayout.Space(6);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(L("menu.close", "Schließen"), _buttonStyle, GUILayout.Width(95), GUILayout.Height(28)))
            {
                CorePlugin.ToggleMenu();
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            // Rebinding Overlay Banner
            if (_rebindingEntry != null)
            {
                GUI.backgroundColor = new Color(0.85f, 0.20f, 0.20f, 0.95f);
                GUILayout.BeginHorizontal("box");
                GUILayout.Label(string.Format(L("btn.rebind_prompt", "Drücke eine neue Taste für '{0}' (ESC zum Abbrechen)..."), _rebindingLabel), _entryLabelStyle);
                GUILayout.EndHorizontal();
                GUI.backgroundColor = Color.white;
            }

            // Main Split Body
            GUILayout.BeginHorizontal();

            // === LEFT SIDEBAR ===
            // Calculate dynamic width based on loaded mod names
            var featureMods = GetFeatureMods();
            float maxNameWidth = 160f;
            Vector2 coreSize = _sidebarButtonStyle.CalcSize(new GUIContent(L("menu.tab.core_settings", "Allgemein")));
            if (coreSize.x > maxNameWidth) maxNameWidth = coreSize.x;

            foreach (var m in featureMods)
            {
                string displayModName = m.Translate("mod.name", m.Name);
                Vector2 textSize = _sidebarButtonStyle.CalcSize(new GUIContent("[v] " + displayModName));
                if (textSize.x > maxNameWidth) maxNameWidth = textSize.x;
            }

            float sidebarScreenWidth = Mathf.Clamp(maxNameWidth + 36f, 200f, 340f);
            float sidebarGuiWidth = sidebarScreenWidth / _currentScale;

            // Ensure window width accommodates dynamic sidebar + content
            float desiredWindowWidth = sidebarGuiWidth + 640f;
            if (_baseWindowRect.width < desiredWindowWidth)
            {
                _baseWindowRect.width = desiredWindowWidth;
            }

            GUILayout.BeginVertical(GUILayout.Width(sidebarGuiWidth), GUILayout.ExpandHeight(true));
            _sidebarScrollPos = GUILayout.BeginScrollView(_sidebarScrollPos, false, false, GUILayout.Width(sidebarGuiWidth));

            // Core Settings Button
            bool isCoreSelected = (_selectedModIndex == -1);
            if (GUILayout.Button(L("menu.tab.core_settings", "Allgemein"), isCoreSelected ? _sidebarButtonActiveStyle : _sidebarButtonStyle, GUILayout.Height(40)))
            {
                _selectedModIndex = -1;
            }
            GUILayout.Space(8);
            GUILayout.Label(L("menu.tab.mods", "Geladene Mods"), _subHeaderStyle);
            GUILayout.Space(4);

            // Feature Mods List (CoreMod excluded!)
            if (featureMods.Count == 0)
            {
                GUILayout.Label(L("menu.no_mods", "Keine Feature-Mods geladen"), _subHeaderStyle);
            }
            else
            {
                for (int i = 0; i < featureMods.Count; i++)
                {
                    var mod = featureMods[i];
                    bool isSelected = (i == _selectedModIndex);
                    GUIStyle btnStyle = isSelected ? _sidebarButtonActiveStyle : _sidebarButtonStyle;

                    string displayModName = mod.Translate("mod.name", mod.Name);
                    string statusDot = mod.IsEnabled ? "<color=green>[v]</color>" : "<color=red>[ ]</color>";
                    // Version in smaller rich-text below the name
                    string btnLabel = $"{statusDot} <b>{displayModName}</b>\n<size=10>v{mod.Version}</size>";
                    if (GUILayout.Button(btnLabel, btnStyle, GUILayout.Height(44)))
                    {
                        _selectedModIndex = i;
                    }
                    GUILayout.Space(3);
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            // Divider line
            GUILayout.Box("", GUILayout.Width(2), GUILayout.ExpandHeight(true));

            // === RIGHT CONTENT ===
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            _contentScrollPos = GUILayout.BeginScrollView(_contentScrollPos, false, true);

            if (_selectedModIndex == -1)
            {
                DrawCoreSettingsView();
            }
            else if (featureMods.Count > 0 && _selectedModIndex >= 0 && _selectedModIndex < featureMods.Count)
            {
                var currentMod = featureMods[_selectedModIndex];
                DrawModConfigView(currentMod);
            }
            else
            {
                GUILayout.Label(L("menu.select_mod", "Wähle einen Mod aus der linken Liste aus."), _subHeaderStyle);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            // Status Bar
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            int activeModsCount = featureMods.Count(m => m.IsEnabled);
            string status = string.Format(L("menu.status_footer", "Aktive Mods: {0} | Menü-Taste: {1} | Sprache: {2}"),
                $"{activeModsCount} / {featureMods.Count}", CorePlugin.MenuToggleKey.Value, LocalizationManager.CurrentLanguage);
            GUILayout.Label(status, _subHeaderStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(6);

            GUI.DragWindow(new Rect(0, 0, _baseWindowRect.width - 100, 36));
        }

        private void DrawCoreSettingsView()
        {
            GUILayout.BeginVertical();

            GUILayout.Label(L("menu.tab.core_settings", "Core-Optionen"), _headerStyle);
            GUILayout.Label("Version: " + CorePlugin.PluginVersion + " | GUID: " + CorePlugin.PluginGuid, _subHeaderStyle);
            GUILayout.Space(12);

            // === SECTION: LANGUAGE ===
            GUILayout.Label(L("core.section.language", "Sprach-Einstellungen"), _sectionHeaderStyle);
            
            // 1. Stylized Checkbox: Use Game Language
            GUILayout.BeginVertical("box");
            bool useGameLang = CorePlugin.UseGameLanguage.Value;
            string checkText = useGameLang 
                ? L("btn.enabled", "[v] Aktiviert") + " - " + L("core.use_game_language.name", "Spiel-Sprache verwenden") 
                : L("btn.disabled", "[  ] Deaktiviert") + " - " + L("core.use_game_language.name", "Spiel-Sprache verwenden");

            if (GUILayout.Button(checkText, useGameLang ? _checkboxActiveStyle : _checkboxStyle, GUILayout.Height(30)))
            {
                CorePlugin.UseGameLanguage.Value = !useGameLang;
                CorePlugin.UseGameLanguage.ConfigFile?.Save();
                if (CorePlugin.UseGameLanguage.Value)
                {
                    _isLanguageDropdownOpen = false;
                }
            }
            GUILayout.Label(L("core.use_game_language.desc", "Verwendet automatisch die im Spiel bzw. System eingestellte Sprache"), _entryDescStyle);
            GUILayout.EndVertical();
            GUILayout.Space(6);

            // 2. Language Dropdown Selection (Disabled if UseGameLanguage is true)
            GUILayout.BeginVertical("box");
            GUILayout.Label(L("core.selected_language.name", "Sprache wählen"), _entryLabelStyle);
            GUILayout.Label(L("core.selected_language.desc", "Manuelle Sprachauswahl (nur aktiv, wenn 'Spiel-Sprache verwenden' abgewählt ist)"), _entryDescStyle);
            GUILayout.Space(6);

            GUI.enabled = !useGameLang;

            string currentLangCode = CorePlugin.SelectedLanguage.Value ?? "en";
            string currentLangName = LocalizationManager.GetLanguageNativeName(currentLangCode);

            // Dropdown trigger button
            string dropdownLabel = $"{currentLangName} ({currentLangCode}) " + (_isLanguageDropdownOpen ? "^" : "v");
            if (GUILayout.Button(dropdownLabel, _dropdownHeaderStyle, GUILayout.Width(280), GUILayout.Height(30)))
            {
                _isLanguageDropdownOpen = !_isLanguageDropdownOpen;
            }

            // Dropdown list
            if (_isLanguageDropdownOpen && !useGameLang)
            {
                GUILayout.BeginVertical("box", GUILayout.Width(320));
                _langDropdownScrollPos = GUILayout.BeginScrollView(_langDropdownScrollPos, GUILayout.Height(180));

                foreach (var langDef in LocalizationManager.SupportedLanguages)
                {
                    bool isSelected = langDef.Code.Equals(currentLangCode, StringComparison.OrdinalIgnoreCase);
                    GUIStyle itemStyle = isSelected ? _dropdownItemActiveStyle : _dropdownItemStyle;

                    if (GUILayout.Button($"{langDef.NativeName} ({langDef.Code})", itemStyle, GUILayout.Height(28)))
                    {
                        CorePlugin.SelectedLanguage.Value = langDef.Code;
                        CorePlugin.SelectedLanguage.ConfigFile?.Save();
                        _isLanguageDropdownOpen = false;

                        LocalizationManager.NotifyLanguageChanged(langDef.Code);

                        // Check if any mods are missing translations for this selected language
                        var missingMods = LocalizationManager.GetModsMissingLanguage(langDef.Code);
                        if (missingMods.Count > 0)
                        {
                            _showMissingDialog = true;
                            _missingDialogTargetLang = langDef.Code;
                            _missingModsList = missingMods;
                        }
                    }
                }

                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }

            GUI.enabled = true;
            GUILayout.EndVertical();
            GUILayout.Space(12);

            // === SECTION: APPEARANCE & UI SCALING ===
            GUILayout.Label(L("core.section.appearance", "Darstellung & UI-Skalierung"), _sectionHeaderStyle);
            GUILayout.BeginVertical("box");
            GUILayout.Label(L("core.ui_scale.name", "UI-Skalierung (Schrift- & Fenstergröße)"), _entryLabelStyle);
            GUILayout.Label(L("core.ui_scale.desc", "Vergrößert oder verkleinert das Menü für 1440p / 4K Monitore"), _entryDescStyle);
            GUILayout.Space(6);

            float currentScale = CorePlugin.UIScale.Value;
            GUILayout.BeginHorizontal();

            // Button: -5%
            if (GUILayout.Button("-5 %", _buttonStyle, GUILayout.Width(80), GUILayout.Height(28)))
            {
                float targetScale = Mathf.Max(0.70f, Mathf.Round((currentScale - 0.05f) * 20.0f) / 20.0f);
                CorePlugin.UIScale.Value = targetScale;
                CorePlugin.UIScale.ConfigFile?.Save();
            }

            GUILayout.Space(6);

            // Display current percentage in center
            int displayPct = (int)Math.Round(currentScale * 100.0);
            GUILayout.Label($"<b>{displayPct} %</b>", _entryLabelStyle, GUILayout.Width(70));

            GUILayout.Space(6);

            // Button: +5%
            if (GUILayout.Button("+5 %", _buttonStyle, GUILayout.Width(80), GUILayout.Height(28)))
            {
                float targetScale = Mathf.Min(1.60f, Mathf.Round((currentScale + 0.05f) * 20.0f) / 20.0f);
                CorePlugin.UIScale.Value = targetScale;
                CorePlugin.UIScale.ConfigFile?.Save();
            }

            GUILayout.Space(12);

            // Reset Button: 100%
            if (GUILayout.Button(L("btn.reset", "Zurücksetzen (100 %)"), _buttonStyle, GUILayout.Width(150), GUILayout.Height(28)))
            {
                CorePlugin.UIScale.Value = 1.0f;
                CorePlugin.UIScale.ConfigFile?.Save();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(12);

            // === SECTION: GENERAL BEHAVIOR ===
            GUILayout.Label(L("core.section.general", "Allgemeines Verhalten"), _sectionHeaderStyle);
            DrawConfigEntryCard(CorePlugin.PauseGameOnMenu, CoreAssemblyName);
            GUILayout.Space(6);
            DrawConfigEntryCard(CorePlugin.IgnoreExternalTranslations, CoreAssemblyName);
            GUILayout.Space(6);

            // === SECTION: HOTKEYS ===
            GUILayout.Label(L("core.section.controls", "Tastenbelegung"), _sectionHeaderStyle);
            DrawConfigEntryCard(CorePlugin.MenuToggleKey, CoreAssemblyName);

            GUILayout.EndVertical();
        }

        private void DrawModConfigView(ModInfo mod)
        {
            GUILayout.BeginVertical();

            string modTitle = mod.Translate("mod.name", mod.Name);
            GUILayout.Label(modTitle, _headerStyle);
            GUILayout.Label($"Version: {mod.Version}", _subHeaderStyle);
            GUILayout.Space(10);

            // ---- Enable / Disable Toggle ----
            if (mod.CanBeDisabled)
            {
                bool isEnabled = mod.IsEnabled;
                string stateLabel = isEnabled
                    ? L("btn.enabled", "[v] Aktiviert")
                    : L("btn.disabled", "[  ] Deaktiviert");
                string toggleLabel = L("mod.toggle.label", "Mod-Status") + ": " + stateLabel;

                GUILayout.BeginVertical("box");
                if (GUILayout.Button(toggleLabel, isEnabled ? _checkboxActiveStyle : _checkboxStyle, GUILayout.Height(32)))
                {
                    mod.Instance?.SetEnabled(!isEnabled);
                }
                GUILayout.Label(L("mod.toggle.desc", "Schaltet den Mod inkl. aller Harmony-Patches sofort an oder aus. Der Zustand wird gespeichert."), _entryDescStyle);
                GUILayout.EndVertical();
                GUILayout.Space(8);
            }

            // Gray out config when disabled
            GUI.enabled = mod.IsEnabled;

            if (!mod.IsEnabled)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label(L("mod.disabled.hint", "Dieser Mod ist deaktiviert. Einstellungen sind gesperrt."), _subHeaderStyle);
                GUILayout.EndVertical();
                GUILayout.Space(6);
            }

            var config = mod.Config;
            if (config == null || config.Keys.Count == 0)
            {
                GUI.enabled = true;
                GUILayout.Label("Dieser Mod besitzt keine konfigurierbaren Einstellungen.", _subHeaderStyle);
                GUILayout.EndVertical();
                return;
            }

            var entriesBySection = config.Keys
                .GroupBy(k => k.Section)
                .OrderBy(g => g.Key);

            foreach (var sectionGroup in entriesBySection)
            {
                string sectionKey = sectionGroup.Key;
                string sectionTranslated = mod.Translate($"config.{sectionKey.ToLowerInvariant()}.section", sectionKey);

                GUILayout.BeginHorizontal();
                GUILayout.Label(sectionTranslated, _sectionHeaderStyle);
                GUILayout.FlexibleSpace();

                // Section Reset Button
                if (!sectionKey.Equals("General", StringComparison.OrdinalIgnoreCase))
                {
                    string resetLabel = mod.Translate("ui.reset_group", "[ Gruppe zurücksetzen ]");
                    if (GUILayout.Button(resetLabel, _buttonStyle, GUILayout.Width(170), GUILayout.Height(24)))
                    {
                        foreach (var def in sectionGroup)
                        {
                            if (config.ContainsKey(def))
                            {
                                var entry = config[def];
                                entry.BoxedValue = entry.DefaultValue;
                            }
                        }
                        config.Save();
                    }
                }
                GUILayout.EndHorizontal();

                // Detect if this section has a SimpleMode toggle
                ConfigDefinition simpleModeDef = sectionGroup.FirstOrDefault(d => d.Key.Equals("SimpleMode", StringComparison.OrdinalIgnoreCase));
                bool hasSimpleMode = (simpleModeDef != null && config.ContainsKey(simpleModeDef));
                bool isSimpleModeActive = false;
                if (hasSimpleMode)
                {
                    var simpleEntry = config[simpleModeDef] as ConfigEntry<bool>;
                    if (simpleEntry != null) isSimpleModeActive = simpleEntry.Value;
                }

                foreach (var definition in sectionGroup)
                {
                    // Skip the 'Enabled' entry — it is already shown as the dedicated Enable/Disable toggle above.
                    if (definition.Key.Equals("Enabled", StringComparison.OrdinalIgnoreCase)
                        && definition.Section.Equals("General", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (config.ContainsKey(definition))
                    {
                        ConfigEntryBase entryBase = config[definition];

                        // Control enabled/disabled state per entry based on SimpleMode
                        bool entryEnabled = mod.IsEnabled;
                        if (mod.IsEnabled && hasSimpleMode)
                        {
                            bool isSimpleToggle = definition.Key.Equals("SimpleMode", StringComparison.OrdinalIgnoreCase);
                            bool isGroupMult = definition.Key.Equals("Group_Multiplier", StringComparison.OrdinalIgnoreCase)
                                            || definition.Key.Equals("GroupMultiplier", StringComparison.OrdinalIgnoreCase);

                            if (isSimpleModeActive)
                            {
                                // In Simple Mode: SimpleMode toggle & GroupMultiplier are enabled.
                                // All other individual component sliders are disabled/greyed out!
                                entryEnabled = isSimpleToggle || isGroupMult;
                            }
                            else
                            {
                                // In Advanced Mode: SimpleMode toggle & individual component sliders are enabled.
                                // GroupMultiplier is disabled/greyed out.
                                entryEnabled = !isGroupMult;
                            }
                        }

                        GUI.enabled = entryEnabled;
                        DrawConfigEntryCard(entryBase, mod.AssemblyName, mod);
                        GUI.enabled = mod.IsEnabled;
                        GUILayout.Space(6);
                    }
                }

                GUILayout.Space(8);
            }

            GUI.enabled = true; // Always restore after mod config view
            GUILayout.EndVertical();
        }

        private void DrawConfigEntryCard(ConfigEntryBase entry, string modName, ModInfo mod = null)
        {
            GUILayout.BeginVertical("box");

            string section = entry.Definition.Section.ToLowerInvariant();
            string key = entry.Definition.Key.ToLowerInvariant();

            string titleTranslated = LocalizationManager.Translate(modName, $"config.{section}.{key}.name", entry.Definition.Key);
            string descTranslated = LocalizationManager.Translate(modName, $"config.{section}.{key}.desc", entry.Description?.Description);

            // If boolean, render as clean single-toggle checkbox
            if (entry.SettingType == typeof(bool))
            {
                var boolEntry = (ConfigEntry<bool>)entry;
                bool isChecked = boolEntry.Value;
                string checkLabel = isChecked ? L("btn.enabled", "[v] Aktiviert") : L("btn.disabled", "[  ] Deaktiviert");

                GUILayout.BeginHorizontal();
                if (GUILayout.Button($"{checkLabel} - {titleTranslated}", isChecked ? _checkboxActiveStyle : _checkboxStyle, GUILayout.Height(28)))
                {
                    boolEntry.Value = !isChecked;
                    boolEntry.ConfigFile?.Save();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (!string.IsNullOrEmpty(descTranslated))
                {
                    GUILayout.Space(2);
                    GUILayout.Label(descTranslated, _entryDescStyle);
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(titleTranslated, _entryLabelStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (!string.IsNullOrEmpty(descTranslated))
                {
                    GUILayout.Label(descTranslated, _entryDescStyle);
                    GUILayout.Space(4);
                }

                DrawSettingControl(entry, mod);
            }

            GUILayout.EndVertical();
        }

        private void DrawSettingControl(ConfigEntryBase entry, ModInfo mod = null)
        {
            Type settingType = entry.SettingType;

            // 1. KeyCode Rebinding
            if (settingType == typeof(KeyCode))
            {
                var keyEntry = (ConfigEntry<KeyCode>)entry;
                bool isRebindingThis = (_rebindingEntry == keyEntry);

                GUILayout.BeginHorizontal();
                string buttonText = isRebindingThis 
                    ? L("btn.rebind_prompt", "👉 Taste drücken...") 
                    : string.Format(L("btn.rebind", "Taste: [ {0} ]"), keyEntry.Value);
                
                GUIStyle keyBtnStyle = isRebindingThis ? _buttonActiveStyle : _buttonStyle;

                if (GUILayout.Button(buttonText, keyBtnStyle, GUILayout.Width(180), GUILayout.Height(26)))
                {
                    _rebindingEntry = keyEntry;
                    _rebindingLabel = keyEntry.Definition.Key;
                }

                if (GUILayout.Button(L("btn.reset", "Zurücksetzen"), _buttonStyle, GUILayout.Width(95), GUILayout.Height(26)))
                {
                    keyEntry.Value = (KeyCode)keyEntry.DefaultValue;
                    keyEntry.ConfigFile?.Save();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            // 2. Floating Point Numbers
            else if (settingType == typeof(float))
            {
                var floatEntry = (ConfigEntry<float>)entry;
                float currentVal = floatEntry.Value;
                float minVal = 0.0f;
                float maxVal = 10.0f;
                float step = 0.0f; // 0 = continuous

                // Detect AcceptableValueList<float> (e.g. 0.5-step multipliers)
                if (floatEntry.Description?.AcceptableValues is AcceptableValueList<float> valueList)
                {
                    var values = valueList.AcceptableValues;
                    if (values != null && values.Length > 0)
                    {
                        minVal = values[0];
                        maxVal = values[values.Length - 1];
                        // Detect uniform step size from first two elements
                        if (values.Length >= 2)
                            step = values[1] - values[0];
                    }
                }
                else if (floatEntry.Description?.AcceptableValues is AcceptableValueRange<float> range)
                {
                    minVal = range.MinValue;
                    maxVal = range.MaxValue;
                }

                // Query dynamic min and max bounds from mod if supported
                if (mod?.Instance != null)
                {
                    minVal = mod.Instance.GetMinSliderValue(entry.Definition, minVal);
                    maxVal = mod.Instance.GetMaxSliderValue(entry.Definition, maxVal);
                    if (minVal > maxVal) minVal = maxVal;
                }

                GUILayout.BeginHorizontal();

                float rawSlider = GUILayout.HorizontalSlider(currentVal, minVal, maxVal, _sliderTrackStyle, _sliderThumbStyle, GUILayout.Width(220));

                // Snap to step if a fixed step size was detected
                float newVal;
                if (step > 0.0001f)
                {
                    newVal = Mathf.Round(rawSlider / step) * step;
                    newVal = Mathf.Clamp(newVal, minVal, maxVal);
                    newVal = (float)System.Math.Round(newVal, 2);
                }
                else
                {
                    newVal = rawSlider;
                }

                GUILayout.Space(8);
                string textVal = GUILayout.TextField(newVal.ToString("0.0#", CultureInfo.InvariantCulture), _textFieldStyle, GUILayout.Width(52));

                if (float.TryParse(textVal, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedVal))
                {
                    if (step > 0.0001f)
                    {
                        parsedVal = Mathf.Round(parsedVal / step) * step;
                        parsedVal = (float)System.Math.Round(parsedVal, 2);
                    }
                    newVal = Mathf.Clamp(parsedVal, minVal, maxVal);
                }

                if (Math.Abs(newVal - currentVal) > 0.001f)
                {
                    floatEntry.Value = newVal;
                    floatEntry.ConfigFile?.Save();
                }

                // Show default value label
                GUILayout.Space(8);
                string defaultStr = ((float)floatEntry.DefaultValue).ToString("0.0#", CultureInfo.InvariantCulture);
                string defaultLabel = string.Format(L("ui.default_value", "(Standard: {0})"), defaultStr);
                GUILayout.Label(defaultLabel, _subHeaderStyle);

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            // 3. Integer Numbers
            else if (settingType == typeof(int))
            {
                var intEntry = (ConfigEntry<int>)entry;
                int currentVal = intEntry.Value;

                GUILayout.BeginHorizontal();
                int newVal = Mathf.RoundToInt(GUILayout.HorizontalSlider(currentVal, 0, 100, GUILayout.Width(200)));
                GUILayout.Space(8);
                string textVal = GUILayout.TextField(newVal.ToString(), _textFieldStyle, GUILayout.Width(60));

                if (int.TryParse(textVal, out int parsedVal))
                {
                    newVal = parsedVal;
                }

                if (newVal != currentVal)
                {
                    intEntry.Value = newVal;
                    intEntry.ConfigFile?.Save();
                }

                // Show default value label
                GUILayout.Space(8);
                string defaultStr = intEntry.DefaultValue.ToString();
                string defaultLabel = string.Format(L("ui.default_value", "(Standard: {0})"), defaultStr);
                GUILayout.Label(defaultLabel, _subHeaderStyle);

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            // 4. Strings
            else if (settingType == typeof(string))
            {
                var stringEntry = (ConfigEntry<string>)entry;
                GUILayout.BeginHorizontal();
                string newVal = GUILayout.TextField(stringEntry.Value ?? "", _textFieldStyle, GUILayout.Width(260));
                if (newVal != stringEntry.Value)
                {
                    stringEntry.Value = newVal;
                    stringEntry.ConfigFile?.Save();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label($"Wert: {entry.BoxedValue} (Typ: {settingType.Name})", _subHeaderStyle);
            }
        }

        private void DrawMissingTranslationDialog(int windowId)
        {
            GUILayout.BeginVertical();

            // Dialog Header
            GUILayout.Label(L("dialog.missing_trans.title", "🌐 Fehlende Sprachdateien erkannt"), _headerStyle);
            GUILayout.Space(6);

            string promptText = string.Format(L("dialog.missing_trans.prompt", "Für folgende Mods existiert noch keine Übersetzung für '{0}':"), 
                LocalizationManager.GetLanguageNativeName(_missingDialogTargetLang));
            GUILayout.Label(promptText, _entryLabelStyle);
            GUILayout.Space(4);

            // Mod List
            GUILayout.BeginVertical("box", GUILayout.Height(90));
            foreach (var modName in _missingModsList)
            {
                GUILayout.Label($"• {modName}", _entryLabelStyle);
            }
            GUILayout.EndVertical();
            GUILayout.Space(6);

            GUILayout.Label(L("dialog.missing_trans.question", "Möchtest du, dass wir dir dafür Vorlagen-Dateien zur Übersetzung anlegen?"), _entryLabelStyle);
            GUILayout.Label(L("dialog.missing_trans.info", "Die Dateien werden in 'BepInEx/plugins/Milex GMS1 Mod Localization/' erstellt. Du kannst sie übersetzen und im NexusMods-Eintrag des Mods posten!"), _entryDescStyle);
            GUILayout.Space(12);

            // Action Buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(L("dialog.missing_trans.btn_yes", "Vorlagen erstellen"), _buttonActiveStyle, GUILayout.Height(32)))
            {
                LocalizationManager.GenerateTemplatesForMods(_missingModsList, _missingDialogTargetLang);
                _showMissingDialog = false;
            }
            GUILayout.Space(10);
            if (GUILayout.Button(L("dialog.missing_trans.btn_no", "Nein, Standard behalten"), _buttonStyle, GUILayout.Height(32)))
            {
                _showMissingDialog = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private Texture2D MakeBorderedTex(int width, int height, Color fillColor, Color borderColor, int borderWidth = 1)
        {
            Color[] pix = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = (x < borderWidth || x >= width - borderWidth || y < borderWidth || y >= height - borderWidth);
                    pix[y * width + x] = isBorder ? borderColor : fillColor;
                }
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
