using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using Milex.GMS1.Core.Localization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Milex.GMS1.Core.UI.Modern
{
    /// <summary>
    /// Next-Generation Runtime Canvas (uGUI) Modern Dashboard Menu for Milex GMS1 Mods.
    /// Provides a responsive card-based layout, category tabs, live search filtering,
    /// dynamic mod switching, and full config binding with zero external assets.
    /// </summary>
    public class ModernCanvasMenu : MonoBehaviour, IMenuRenderer
    {
        public string EngineName => "Modern (Canvas)";
        public bool IsVisible => _canvasRoot != null && _canvasRoot.activeSelf;

        private GameObject _canvasRoot;
        private Canvas _canvas;
        private CanvasScaler _scaler;
        private GraphicRaycaster _raycaster;

        // UI References
        private RectTransform _windowPanelRt;
        private RectTransform _sidebarContentRt;
        private RectTransform _tabsContentRt;
        private RectTransform _settingsContentRt;
        private ScrollRect _settingsScrollRect;
        private InputField _searchInput;
        private Text _windowTitleText;

        // State
        private int _selectedModIndex = -1; // -1 = CoreMod, >= 0 = SubMods
        private string _activeCategory = "All";
        private string _searchFilter = "";
        private readonly List<GameObject> _createdSettingCards = new List<GameObject>();
        private readonly List<GameObject> _createdTabButtons = new List<GameObject>();
        private readonly List<GameObject> _createdSidebarButtons = new List<GameObject>();

        // Dragging state
        private bool _isDragging = false;

        private static readonly string CoreAssemblyName = typeof(CorePlugin).Assembly.GetName().Name;
        private string L(string key, string fallback) => LocalizationManager.Translate(CoreAssemblyName, key, fallback);

        public void Initialize()
        {
            // Lazy initialization: Canvas GameObject will be built on first Show()
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current == null && FindObjectOfType<EventSystem>() == null)
            {
                var esObj = new GameObject("Milex_EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                DontDestroyOnLoad(esObj);
            }
        }

        private void BuildCanvasHierarchy()
        {
            if (_canvasRoot != null) return;

            _canvasRoot = new GameObject("Milex_ModernMenu_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvasRoot.transform.SetParent(this.transform, false);

            _canvas = _canvasRoot.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 32760;

            _scaler = _canvasRoot.GetComponent<CanvasScaler>();
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = new Vector2(1920, 1080);
            _scaler.matchWidthOrHeight = 0.5f;

            _raycaster = _canvasRoot.GetComponent<GraphicRaycaster>();
            _raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            _raycaster.ignoreReversedGraphics = true;

            // Modal Dim Backdrop (semi-transparent)
            var backdrop = UIFactory.CreatePanel(_canvasRoot.transform, "Backdrop", new Color(0.04f, 0.05f, 0.08f, 0.55f));
            var bdRt = backdrop.GetComponent<RectTransform>();
            bdRt.anchorMin = Vector2.zero;
            bdRt.anchorMax = Vector2.one;
            bdRt.offsetMin = Vector2.zero;
            bdRt.offsetMax = Vector2.zero;

            // Clicking backdrop outside closes menu
            var bdBtn = backdrop.AddComponent<Button>();
            bdBtn.transition = Selectable.Transition.None;
            bdBtn.onClick.AddListener(() => CorePlugin.ToggleMenu());

            // Main Window Panel (Dark Slate Container)
            var window = UIFactory.CreatePanel(_canvasRoot.transform, "WindowPanel", new Color(0.10f, 0.12f, 0.16f, 0.98f), UIFactory.RoundedBoxSprite);
            _windowPanelRt = window.GetComponent<RectTransform>();
            _windowPanelRt.sizeDelta = new Vector2(980, 640);
            _windowPanelRt.anchoredPosition = Vector2.zero;

            BuildHeader(window.transform);
            BuildBody(window.transform);
        }

        private RectTransform _headerRt;
        private Vector2 _dragStartMousePos;
        private Vector2 _dragStartWindowPos;
        private Slider _activeDraggingSlider;

        private void Update()
        {
            if (!IsVisible) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CorePlugin.ToggleMenu();
            }
        }

        private void OnGUI()
        {
            if (!IsVisible) return;

            Event e = Event.current;
            if (e == null) return;

            // Convert IMGUI top-left mouse coords to Unity bottom-left screen space
            Vector2 screenPos = new Vector2(e.mousePosition.x, Screen.height - e.mousePosition.y);

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                // Check if clicking controls first
                if (_raycaster != null)
                {
                    var results = RaycastUI(screenPos);
                    var clickedBtn = results.Select(r => r.gameObject.GetComponentInParent<Button>()).FirstOrDefault(b => b != null && b.interactable);
                    var clickedToggle = results.Select(r => r.gameObject.GetComponentInParent<Toggle>()).FirstOrDefault(t => t != null && t.interactable);
                    var clickedSlider = results.Select(r => r.gameObject.GetComponentInParent<Slider>()).FirstOrDefault(s => s != null && s.interactable);

                    if (clickedBtn != null)
                    {
                        clickedBtn.onClick?.Invoke();
                        e.Use();
                        return;
                    }
                    else if (clickedToggle != null)
                    {
                        clickedToggle.isOn = !clickedToggle.isOn;
                        e.Use();
                        return;
                    }
                    else if (clickedSlider != null)
                    {
                        _activeDraggingSlider = clickedSlider;
                        UpdateSliderValue(clickedSlider, screenPos);
                        e.Use();
                        return;
                    }
                }

                // Header drag check
                if (_headerRt != null && RectTransformUtility.RectangleContainsScreenPoint(_headerRt, screenPos, null))
                {
                    _isDragging = true;
                    _dragStartMousePos = screenPos;
                    _dragStartWindowPos = _windowPanelRt.anchoredPosition;
                    e.Use();
                }
            }
            else if ((e.type == EventType.MouseDrag || e.type == EventType.MouseMove) && (e.button == 0 || _isDragging || _activeDraggingSlider != null))
            {
                if (_isDragging)
                {
                    Vector2 delta = screenPos - _dragStartMousePos;
                    float scale = _scaler != null && _scaler.scaleFactor > 0.01f ? _scaler.scaleFactor : 1f;
                    _windowPanelRt.anchoredPosition = _dragStartWindowPos + (delta / scale);
                    e.Use();
                }
                else if (_activeDraggingSlider != null)
                {
                    UpdateSliderValue(_activeDraggingSlider, screenPos);
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                _isDragging = false;
                _activeDraggingSlider = null;
            }
            else if (e.type == EventType.ScrollWheel && _settingsScrollRect != null)
            {
                _settingsScrollRect.verticalNormalizedPosition += e.delta.y * 0.05f;
                _settingsScrollRect.verticalNormalizedPosition = Mathf.Clamp01(_settingsScrollRect.verticalNormalizedPosition);
                e.Use();
            }
        }

        private List<RaycastResult> RaycastUI(Vector2 screenPos)
        {
            var results = new List<RaycastResult>();
            if (_raycaster == null) return results;

            var es = EventSystem.current;
            var pData = new PointerEventData(es) { position = screenPos };
            _raycaster.Raycast(pData, results);
            return results;
        }

        private void UpdateSliderValue(Slider slider, Vector2 screenPos)
        {
            if (slider == null) return;
            var sliderRt = slider.GetComponent<RectTransform>();
            if (sliderRt == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(sliderRt, screenPos, null, out Vector2 localPoint))
            {
                float width = sliderRt.rect.width;
                if (width > 0.01f)
                {
                    float normalized = Mathf.Clamp01((localPoint.x - sliderRt.rect.xMin) / width);
                    slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, normalized);
                }
            }
        }

        private void BuildHeader(Transform window)
        {
            // Header Bar
            var header = UIFactory.CreatePanel(window, "HeaderBar", new Color(0.14f, 0.16f, 0.22f, 1f), UIFactory.RoundedBoxSprite);
            var headerRt = header.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 1);
            headerRt.anchorMax = new Vector2(1, 1);
            headerRt.pivot = new Vector2(0.5f, 1);
            headerRt.sizeDelta = new Vector2(0, 56);
            headerRt.anchoredPosition = Vector2.zero;
            _headerRt = headerRt;

            // Title & Gold Accent
            var titleBar = UIFactory.CreatePanel(header.transform, "AccentBar", new Color(0.88f, 0.65f, 0.18f, 1f));
            var accentRt = titleBar.GetComponent<RectTransform>();
            accentRt.anchorMin = new Vector2(0, 0);
            accentRt.anchorMax = new Vector2(0, 1);
            accentRt.sizeDelta = new Vector2(4, 0);
            accentRt.anchoredPosition = new Vector2(2, 0);

            _windowTitleText = UIFactory.CreateText(header.transform, "TitleText", "MILEX GMS1 DASHBOARD", 16, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            var titleRt = _windowTitleText.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 0);
            titleRt.anchorMax = new Vector2(0.4f, 1);
            titleRt.offsetMin = new Vector2(18, 0);
            titleRt.offsetMax = Vector2.zero;

            // Search Bar in Header
            _searchInput = UIFactory.CreateInputField(header.transform, "SearchInput", L("UI_SearchPlaceholder", "Search settings..."), (term) =>
            {
                _searchFilter = term.Trim();
                RefreshSettingsContent();
            });
            var searchRt = _searchInput.GetComponent<RectTransform>();
            searchRt.anchorMin = new Vector2(0.42f, 0.2f);
            searchRt.anchorMax = new Vector2(0.78f, 0.8f);
            searchRt.offsetMin = Vector2.zero;
            searchRt.offsetMax = Vector2.zero;

            // Engine Switch Button
            var engineBtn = UIFactory.CreateButton(header.transform, "EngineButton", "Classic UI", new Color(0.20f, 0.23f, 0.30f, 1f), new Color(0.28f, 0.32f, 0.42f, 1f), new Color(0.15f, 0.17f, 0.22f, 1f), Color.white, () =>
            {
                CorePlugin.SwitchMenuEngine(MenuEngineType.Classic);
            }, 12);
            var engineRt = engineBtn.GetComponent<RectTransform>();
            engineRt.anchorMin = new Vector2(0.80f, 0.2f);
            engineRt.anchorMax = new Vector2(0.92f, 0.8f);
            engineRt.offsetMin = Vector2.zero;
            engineRt.offsetMax = Vector2.zero;

            // Close Button [X]
            var closeBtn = UIFactory.CreateButton(header.transform, "CloseButton", "X", new Color(0.75f, 0.20f, 0.20f, 1f), new Color(0.90f, 0.25f, 0.25f, 1f), new Color(0.60f, 0.15f, 0.15f, 1f), Color.white, () =>
            {
                CorePlugin.ToggleMenu();
            }, 14);
            var closeRt = closeBtn.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(0.94f, 0.2f);
            closeRt.anchorMax = new Vector2(0.985f, 0.8f);
            closeRt.offsetMin = Vector2.zero;
            closeRt.offsetMax = Vector2.zero;
        }

        private void BuildBody(Transform window)
        {
            // Body Container (below header)
            var body = new GameObject("BodyContainer", typeof(RectTransform));
            body.transform.SetParent(window, false);
            var bodyRt = body.GetComponent<RectTransform>();
            bodyRt.anchorMin = Vector2.zero;
            bodyRt.anchorMax = Vector2.one;
            bodyRt.offsetMin = new Vector2(10, 10);
            bodyRt.offsetMax = new Vector2(-10, -62);

            // Left Sidebar Panel
            var sidebar = UIFactory.CreatePanel(body.transform, "Sidebar", new Color(0.12f, 0.14f, 0.18f, 1f), UIFactory.RoundedBoxSprite);
            var sidebarRt = sidebar.GetComponent<RectTransform>();
            sidebarRt.anchorMin = new Vector2(0, 0);
            sidebarRt.anchorMax = new Vector2(0.28f, 1);
            sidebarRt.offsetMin = Vector2.zero;
            sidebarRt.offsetMax = Vector2.zero;

            var (sbScroll, sbContent, _) = UIFactory.CreateScrollView(sidebar.transform, "SidebarScrollView");
            var sbScrollRt = sbScroll.GetComponent<RectTransform>();
            sbScrollRt.anchorMin = Vector2.zero;
            sbScrollRt.anchorMax = Vector2.one;
            sbScrollRt.offsetMin = new Vector2(6, 6);
            sbScrollRt.offsetMax = new Vector2(-6, -6);
            _sidebarContentRt = sbContent;

            // Right Main Content Panel
            var contentPanel = UIFactory.CreatePanel(body.transform, "MainContentPanel", new Color(0.13f, 0.15f, 0.19f, 1f), UIFactory.RoundedBoxSprite);
            var cpRt = contentPanel.GetComponent<RectTransform>();
            cpRt.anchorMin = new Vector2(0.29f, 0);
            cpRt.anchorMax = new Vector2(1, 1);
            cpRt.offsetMin = Vector2.zero;
            cpRt.offsetMax = Vector2.zero;

            // Top Category Tabs Bar inside Main Content
            var tabsBar = UIFactory.CreatePanel(contentPanel.transform, "TabsBar", new Color(0.10f, 0.12f, 0.15f, 1f), UIFactory.RoundedBoxSprite);
            var tabsRt = tabsBar.GetComponent<RectTransform>();
            tabsRt.anchorMin = new Vector2(0, 1);
            tabsRt.anchorMax = new Vector2(1, 1);
            tabsRt.pivot = new Vector2(0.5f, 1);
            tabsRt.sizeDelta = new Vector2(0, 42);
            tabsRt.offsetMin = new Vector2(6, -48);
            tabsRt.offsetMax = new Vector2(-6, -6);

            var (tabsScroll, tabsContent, _) = UIFactory.CreateScrollView(tabsBar.transform, "TabsScrollView");
            var tabsScrollRt = tabsScroll.GetComponent<RectTransform>();
            tabsScrollRt.anchorMin = Vector2.zero;
            tabsScrollRt.anchorMax = Vector2.one;
            tabsScrollRt.offsetMin = Vector2.zero;
            tabsScrollRt.offsetMax = Vector2.zero;

            var hlg = tabsContent.gameObject.AddComponent<HorizontalLayoutGroup>();
            Destroy(tabsContent.GetComponent<VerticalLayoutGroup>());
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.spacing = 6f;
            hlg.padding = new RectOffset(4, 4, 4, 4);

            var tabsCsf = tabsContent.GetComponent<ContentSizeFitter>();
            tabsCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            tabsCsf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            _tabsContentRt = tabsContent;

            // Scrollable Settings List
            var (settingsScroll, settingsContent, scrollRect) = UIFactory.CreateScrollView(contentPanel.transform, "SettingsScrollView");
            var setScrollRt = settingsScroll.GetComponent<RectTransform>();
            setScrollRt.anchorMin = Vector2.zero;
            setScrollRt.anchorMax = Vector2.one;
            setScrollRt.offsetMin = new Vector2(8, 8);
            setScrollRt.offsetMax = new Vector2(-8, -54);
            _settingsContentRt = settingsContent;
            _settingsScrollRect = scrollRect;
        }

        public void Show()
        {
            if (_canvasRoot == null)
            {
                BuildCanvasHierarchy();
            }

            EnsureEventSystem();

            // Sync scale with CorePlugin.UIScale
            float scale = CorePlugin.UIScale != null ? Mathf.Clamp(CorePlugin.UIScale.Value, 0.8f, 1.8f) : 1.0f;
            if (_scaler != null) _scaler.scaleFactor = scale;

            if (_canvasRoot != null) _canvasRoot.SetActive(true);
            if (_canvas != null) _canvas.enabled = true;

            RefreshSidebar();
            RefreshActiveModView();
        }

        public void Hide()
        {
            if (_canvas != null)
            {
                _canvas.enabled = false;
            }
            if (_canvasRoot != null)
            {
                _canvasRoot.SetActive(false);
            }
        }

        public void Cleanup()
        {
            if (_canvasRoot != null)
            {
                Destroy(_canvasRoot);
            }
        }

        private List<ModInfo> GetFeatureMods()
        {
            return ModRegistry.RegisteredMods
                .Where(m => !m.Guid.Equals(CorePlugin.PluginGuid, StringComparison.OrdinalIgnoreCase)
                         && !m.AssemblyName.Equals(CoreAssemblyName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private void RefreshSidebar()
        {
            foreach (var btn in _createdSidebarButtons)
            {
                if (btn != null) Destroy(btn);
            }
            _createdSidebarButtons.Clear();

            var featureMods = GetFeatureMods();

            // Core Settings Button Card
            CreateSidebarCard(-1, CorePlugin.PluginName, CorePlugin.PluginVersion, true, false, _selectedModIndex == -1);

            // Sub-Mods Button Cards
            for (int i = 0; i < featureMods.Count; i++)
            {
                var mod = featureMods[i];
                CreateSidebarCard(i, mod.Name, mod.Version, mod.IsEnabled, mod.CanBeDisabled, _selectedModIndex == i);
            }
        }

        private void CreateSidebarCard(int modIndex, string name, string version, bool isEnabled, bool canBeDisabled, bool isSelected)
        {
            Color cardColor = isSelected ? new Color(0.22f, 0.26f, 0.35f, 1f) : new Color(0.15f, 0.17f, 0.22f, 1f);
            Color hoverColor = new Color(0.26f, 0.30f, 0.40f, 1f);

            var card = UIFactory.CreateButton(_sidebarContentRt, $"ModCard_{modIndex}", "", cardColor, hoverColor, cardColor, Color.white, () =>
            {
                _selectedModIndex = modIndex;
                _activeCategory = "All";
                RefreshSidebar();
                RefreshActiveModView();
            });

            var cardRt = card.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(0, 52);

            // Left Gold Accent if selected
            if (isSelected)
            {
                var accent = UIFactory.CreatePanel(card.transform, "SelectAccent", new Color(0.88f, 0.65f, 0.18f, 1f));
                var accRt = accent.GetComponent<RectTransform>();
                accRt.anchorMin = new Vector2(0, 0);
                accRt.anchorMax = new Vector2(0, 1);
                accRt.sizeDelta = new Vector2(4, 0);
                accRt.anchoredPosition = new Vector2(2, 0);
            }

            // Name
            var nameTxt = UIFactory.CreateText(card.transform, "Name", name, 13, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            var nameRt = nameTxt.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 0.5f);
            nameRt.anchorMax = new Vector2(0.72f, 1f);
            nameRt.offsetMin = new Vector2(12, 0);
            nameRt.offsetMax = new Vector2(0, -4);

            // Version & Status Badge
            string statusStr = isEnabled ? "v" + version : L("UI_Status_Disabled", "Disabled");
            Color statusColor = isEnabled ? new Color(0.5f, 0.8f, 0.5f, 0.9f) : new Color(0.8f, 0.4f, 0.4f, 0.9f);
            var verTxt = UIFactory.CreateText(card.transform, "Version", statusStr, 11, statusColor, TextAnchor.MiddleLeft);
            var verRt = verTxt.GetComponent<RectTransform>();
            verRt.anchorMin = new Vector2(0, 0);
            verRt.anchorMax = new Vector2(0.72f, 0.5f);
            verRt.offsetMin = new Vector2(12, 4);
            verRt.offsetMax = Vector2.zero;

            // Live ON/OFF Toggle on Sidebar
            if (canBeDisabled)
            {
                var toggle = UIFactory.CreateToggle(card.transform, "SidebarToggle", isEnabled, (val) =>
                {
                    var mods = GetFeatureMods();
                    if (modIndex >= 0 && modIndex < mods.Count)
                    {
                        mods[modIndex].Instance?.SetEnabled(val);
                        RefreshSidebar();
                        RefreshActiveModView();
                    }
                });
                var togRt = toggle.GetComponent<RectTransform>();
                togRt.anchorMin = new Vector2(1, 0.5f);
                togRt.anchorMax = new Vector2(1, 0.5f);
                togRt.anchoredPosition = new Vector2(-30, 0);
                togRt.localScale = new Vector3(0.75f, 0.75f, 1f);
            }

            _createdSidebarButtons.Add(card.gameObject);
        }

        private void RefreshActiveModView()
        {
            RefreshTabs();
            RefreshSettingsContent();
        }

        private void RefreshTabs()
        {
            foreach (var tab in _createdTabButtons)
            {
                if (tab != null) Destroy(tab);
            }
            _createdTabButtons.Clear();

            var categories = GetActiveModCategories();

            foreach (var cat in categories)
            {
                bool isSelected = _activeCategory.Equals(cat, StringComparison.OrdinalIgnoreCase);
                Color tabColor = isSelected ? new Color(0.88f, 0.65f, 0.18f, 1f) : new Color(0.18f, 0.20f, 0.26f, 1f);
                Color textColor = isSelected ? Color.black : Color.white;

                string label = cat == "All" ? L("UI_Tab_All", "All") : cat;

                var tabBtn = UIFactory.CreateButton(_tabsContentRt, $"Tab_{cat}", label, tabColor, new Color(0.28f, 0.32f, 0.42f, 1f), tabColor, textColor, () =>
                {
                    _activeCategory = cat;
                    RefreshTabs();
                    RefreshSettingsContent();
                }, 12);

                var tabRt = tabBtn.GetComponent<RectTransform>();
                float width = Mathf.Max(60f, label.Length * 9f + 20f);
                tabRt.sizeDelta = new Vector2(width, 32);

                _createdTabButtons.Add(tabBtn.gameObject);
            }
        }

        private List<string> GetActiveModCategories()
        {
            var list = new HashSet<string> { "All" };

            ConfigFile config = null;
            if (_selectedModIndex == -1)
            {
                config = CorePlugin.Instance?.Config;
            }
            else
            {
                var mods = GetFeatureMods();
                if (_selectedModIndex >= 0 && _selectedModIndex < mods.Count)
                {
                    config = mods[_selectedModIndex].Config;
                }
            }

            if (config != null)
            {
                foreach (var entry in config)
                {
                    if (entry.Key.Section.Equals("General", StringComparison.OrdinalIgnoreCase) && entry.Key.Key.Equals("Enabled", StringComparison.OrdinalIgnoreCase))
                        continue;

                    list.Add(entry.Key.Section);
                }
            }

            return list.ToList();
        }

        private void RefreshSettingsContent()
        {
            foreach (var card in _createdSettingCards)
            {
                if (card != null) Destroy(card);
            }
            _createdSettingCards.Clear();

            _settingsScrollRect.verticalNormalizedPosition = 1f;

            if (_selectedModIndex == -1)
            {
                BuildCoreSettingsCards();
            }
            else
            {
                var mods = GetFeatureMods();
                if (_selectedModIndex >= 0 && _selectedModIndex < mods.Count)
                {
                    BuildModSettingsCards(mods[_selectedModIndex]);
                }
            }
        }

        private void BuildCoreSettingsCards()
        {
            _windowTitleText.text = $"{CorePlugin.PluginName} (v{CorePlugin.PluginVersion})";

            // Language & Core Controls
            CreateCategoryHeader(L("UI_Section_General", "General Settings"));

            CreateToggleCard(L("UI_PauseGameOnMenu", "Pause Game in Menu"), L("UI_PauseGameOnMenu_Desc", "Freezes game time while menu is open."), CorePlugin.PauseGameOnMenu.Value, (val) =>
            {
                CorePlugin.PauseGameOnMenu.Value = val;
            });

            CreateSliderCard(L("UI_ScaleLabel", "UI Scale"), L("UI_Scale_Desc", "Scale interface size for 1080p, 1440p or 4K."), CorePlugin.UIScale.Value, 0.8f, 1.8f, 1.0f, (val) =>
            {
                CorePlugin.UIScale.Value = (float)Math.Round(val, 2);
                _scaler.scaleFactor = CorePlugin.UIScale.Value;
            });

            CreateToggleCard(L("UI_IgnoreTranslations", "Ignore External Translations"), L("UI_IgnoreTranslations_Desc", "Forces embedded DLL localization resources."), CorePlugin.IgnoreExternalTranslations.Value, (val) =>
            {
                CorePlugin.IgnoreExternalTranslations.Value = val;
            });

            CreateCategoryHeader(L("UI_Section_Localization", "Localization"));

            CreateToggleCard(L("UI_UseGameLanguage", "Auto-Detect Game Language"), L("UI_UseGameLanguage_Desc", "Matches the language selected in the main game."), CorePlugin.UseGameLanguage.Value, (val) =>
            {
                CorePlugin.UseGameLanguage.Value = val;
                LocalizationManager.ReloadAll();
            });
        }

        private void BuildModSettingsCards(ModInfo mod)
        {
            _windowTitleText.text = $"{mod.Name} (v{mod.Version})";

            if (mod.Config == null) return;

            string lastSection = "";

            var entries = mod.Config
                .Where(kv => !kv.Key.Section.Equals("General", StringComparison.OrdinalIgnoreCase) || !kv.Key.Key.Equals("Enabled", StringComparison.OrdinalIgnoreCase))
                .Where(kv => _activeCategory == "All" || kv.Key.Section.Equals(_activeCategory, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!string.IsNullOrEmpty(_searchFilter))
            {
                entries = entries.Where(kv =>
                    kv.Key.Key.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    kv.Key.Section.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (kv.Value.Description?.Description != null && kv.Value.Description.Description.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();
            }

            foreach (var kv in entries)
            {
                var entry = kv.Value;
                if (!kv.Key.Section.Equals(lastSection, StringComparison.OrdinalIgnoreCase))
                {
                    lastSection = kv.Key.Section;
                    CreateCategoryHeader(lastSection);
                }

                string keyName = kv.Key.Key;
                string label = mod.Translate($"Config_{keyName}", keyName);
                string desc = mod.Translate($"Config_{keyName}_Desc", entry.Description?.Description ?? "");

                if (entry.SettingType == typeof(float))
                {
                    var floatEntry = (ConfigEntry<float>)entry;
                    float defVal = (float)entry.DefaultValue;
                    float min = 0.1f;
                    float max = 10.0f;

                    if (entry.Description?.AcceptableValues is AcceptableValueRange<float> range)
                    {
                        min = (float)range.MinValue;
                        max = (float)range.MaxValue;
                    }

                    CreateSliderCard(label, desc, floatEntry.Value, min, max, defVal, (val) =>
                    {
                        floatEntry.Value = (float)Math.Round(val, 2);
                    });
                }
                else if (entry.SettingType == typeof(bool))
                {
                    var boolEntry = (ConfigEntry<bool>)entry;
                    CreateToggleCard(label, desc, boolEntry.Value, (val) =>
                    {
                        boolEntry.Value = val;
                    });
                }
                else if (entry.SettingType == typeof(int))
                {
                    var intEntry = (ConfigEntry<int>)entry;
                    int defVal = (int)entry.DefaultValue;
                    int min = 1;
                    int max = 100;

                    if (entry.Description?.AcceptableValues is AcceptableValueRange<int> range)
                    {
                        min = (int)range.MinValue;
                        max = (int)range.MaxValue;
                    }

                    CreateSliderCard(label, desc, intEntry.Value, min, max, defVal, (val) =>
                    {
                        intEntry.Value = Mathf.RoundToInt(val);
                    });
                }
            }
        }

        private void CreateCategoryHeader(string title)
        {
            var header = UIFactory.CreatePanel(_settingsContentRt, $"Header_{title}", new Color(0.16f, 0.18f, 0.24f, 0.85f), UIFactory.RoundedBoxSprite);
            var hRt = header.GetComponent<RectTransform>();
            hRt.sizeDelta = new Vector2(0, 32);

            var txt = UIFactory.CreateText(header.transform, "Title", title.ToUpperInvariant(), 12, new Color(0.88f, 0.65f, 0.18f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold);
            var tRt = txt.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = new Vector2(12, 0);
            tRt.offsetMax = new Vector2(-12, 0);

            _createdSettingCards.Add(header);
        }

        private void CreateSliderCard(string label, string desc, float currentValue, float min, float max, float defaultValue, Action<float> onValueChanged)
        {
            var card = UIFactory.CreatePanel(_settingsContentRt, $"SliderCard_{label}", new Color(0.15f, 0.17f, 0.22f, 1f), UIFactory.RoundedBoxSprite);
            var cardRt = card.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(0, 68);

            // Label & Description
            var lbl = UIFactory.CreateText(card.transform, "Label", label, 14, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            var lRt = lbl.GetComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0, 0.5f);
            lRt.anchorMax = new Vector2(0.55f, 1f);
            lRt.offsetMin = new Vector2(14, 0);
            lRt.offsetMax = new Vector2(0, -4);

            var dsc = UIFactory.CreateText(card.transform, "Desc", desc, 11, new Color(0.6f, 0.65f, 0.75f, 0.9f), TextAnchor.MiddleLeft);
            var dRt = dsc.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0, 0);
            dRt.anchorMax = new Vector2(0.55f, 0.5f);
            dRt.offsetMin = new Vector2(14, 4);
            dRt.offsetMax = Vector2.zero;

            // Value Display Text
            var valTxt = UIFactory.CreateText(card.transform, "ValueText", $"{currentValue:F1}x (Default: {defaultValue:F1}x)", 12, new Color(0.88f, 0.65f, 0.18f, 1f), TextAnchor.MiddleRight, FontStyle.Bold);
            var vRt = valTxt.GetComponent<RectTransform>();
            vRt.anchorMin = new Vector2(0.56f, 0.5f);
            vRt.anchorMax = new Vector2(0.92f, 1f);
            vRt.offsetMin = Vector2.zero;
            vRt.offsetMax = new Vector2(-6, -4);

            // Reset Button
            var resetBtn = UIFactory.CreateButton(card.transform, "ResetBtn", "R", new Color(0.22f, 0.25f, 0.32f, 1f), new Color(0.35f, 0.40f, 0.50f, 1f), new Color(0.18f, 0.20f, 0.25f, 1f), Color.white, () =>
            {
                onValueChanged?.Invoke(defaultValue);
                RefreshSettingsContent();
            }, 11);
            var rRt = resetBtn.GetComponent<RectTransform>();
            rRt.anchorMin = new Vector2(0.93f, 0.2f);
            rRt.anchorMax = new Vector2(0.985f, 0.8f);
            rRt.offsetMin = Vector2.zero;
            rRt.offsetMax = Vector2.zero;

            // Slider
            var slider = UIFactory.CreateSlider(card.transform, "Slider", min, max, currentValue, (val) =>
            {
                valTxt.text = $"{val:F1}x (Default: {defaultValue:F1}x)";
                onValueChanged?.Invoke(val);
            });
            var sRt = slider.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.56f, 0.1f);
            sRt.anchorMax = new Vector2(0.92f, 0.5f);
            sRt.offsetMin = Vector2.zero;
            sRt.offsetMax = new Vector2(-6, 0);

            _createdSettingCards.Add(card);
        }

        private void CreateToggleCard(string label, string desc, bool currentValue, Action<bool> onValueChanged)
        {
            var card = UIFactory.CreatePanel(_settingsContentRt, $"ToggleCard_{label}", new Color(0.15f, 0.17f, 0.22f, 1f), UIFactory.RoundedBoxSprite);
            var cardRt = card.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(0, 56);

            // Label & Description
            var lbl = UIFactory.CreateText(card.transform, "Label", label, 14, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            var lRt = lbl.GetComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0, 0.5f);
            lRt.anchorMax = new Vector2(0.8f, 1f);
            lRt.offsetMin = new Vector2(14, 0);
            lRt.offsetMax = new Vector2(0, -4);

            var dsc = UIFactory.CreateText(card.transform, "Desc", desc, 11, new Color(0.6f, 0.65f, 0.75f, 0.9f), TextAnchor.MiddleLeft);
            var dRt = dsc.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0, 0);
            dRt.anchorMax = new Vector2(0.8f, 0.5f);
            dRt.offsetMin = new Vector2(14, 4);
            dRt.offsetMax = Vector2.zero;

            // Toggle Switch
            var toggle = UIFactory.CreateToggle(card.transform, "Toggle", currentValue, (val) => onValueChanged?.Invoke(val));
            var tRt = toggle.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0.90f, 0.5f);
            tRt.anchorMax = new Vector2(0.90f, 0.5f);
            tRt.anchoredPosition = new Vector2(0, 0);

            _createdSettingCards.Add(card);
        }
    }
}
