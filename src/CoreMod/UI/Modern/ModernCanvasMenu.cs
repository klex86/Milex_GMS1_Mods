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
        private Text _windowSubtitleText;

        // State
        private int _selectedModIndex = -1; // -1 = CoreMod, >= 0 = SubMods
        private string _activeCategory = "All";
        private string _searchFilter = "";
        private bool _isLangDropdownOpen = false;
        private GameObject _modalOverlay;
        private readonly List<GameObject> _createdSettingCards = new List<GameObject>();
        private readonly List<GameObject> _createdTabButtons = new List<GameObject>();
        private readonly List<GameObject> _createdSidebarButtons = new List<GameObject>();

        private static readonly string CoreAssemblyName = typeof(CorePlugin).Assembly.GetName().Name;
        private string L(string key, string fallback) => LocalizationManager.Translate(CoreAssemblyName, key, fallback);

        public void Initialize()
        {
            LocalizationManager.OnLanguageChanged += HandleLanguageChanged;
        }

        private void HandleLanguageChanged(string newLang)
        {
            if (IsVisible)
            {
                RefreshSidebar();
                RefreshActiveModView();
            }
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
            _canvas.pixelPerfect = true; // Pixel-perfect rendering prevents blurry font rasterization

            _scaler = _canvasRoot.GetComponent<CanvasScaler>();
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = new Vector2(1920, 1080);
            _scaler.matchWidthOrHeight = 0.5f;
            _scaler.dynamicPixelsPerUnit = 1.0f;
            _scaler.referencePixelsPerUnit = 100f;

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
            // The only purpose of OnGUI now is to consume Unity's legacy GUI events 
            // so they don't fall through to the game world (Rewired ignores consumed events).
            if (!IsVisible) return;
            
            Event e = Event.current;
            if (e != null && (e.type == EventType.MouseDown || e.type == EventType.MouseUp || e.type == EventType.ScrollWheel || e.type == EventType.MouseDrag))
            {
                e.Use();
            }
        }

        private class WindowDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler
        {
            public RectTransform TargetWindow;
            private CanvasScaler _scaler;
            private Vector2 _dragStartPos;
            private Vector2 _windowStartPos;

            private void Start()
            {
                _scaler = GetComponentInParent<CanvasScaler>();
            }

            public void OnBeginDrag(PointerEventData eventData)
            {
                _dragStartPos = eventData.position;
                if (TargetWindow != null) _windowStartPos = TargetWindow.anchoredPosition;
            }

            public void OnDrag(PointerEventData eventData)
            {
                if (TargetWindow == null) return;
                Vector2 delta = eventData.position - _dragStartPos;
                float scale = _scaler != null && _scaler.scaleFactor > 0.01f ? _scaler.scaleFactor : 1f;
                TargetWindow.anchoredPosition = _windowStartPos + (delta / scale);
            }
        }

        private void BuildHeader(Transform window)
        {
            var header = UIFactory.CreatePanel(window, "HeaderBar", new Color(0.14f, 0.16f, 0.22f, 1f), UIFactory.RoundedBoxSprite);
            var headerRt = header.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 1);
            headerRt.anchorMax = new Vector2(1, 1);
            headerRt.pivot = new Vector2(0.5f, 1);
            headerRt.sizeDelta = new Vector2(0, 56);
            headerRt.anchoredPosition = Vector2.zero;

            // Make the header act as a drag handle for the window
            var dragHandler = header.AddComponent<WindowDragHandler>();
            dragHandler.TargetWindow = _windowPanelRt;

            // Title & Gold Accent
            var titleBar = UIFactory.CreatePanel(header.transform, "AccentBar", new Color(0.88f, 0.65f, 0.18f, 1f));
            var accentRt = titleBar.GetComponent<RectTransform>();
            accentRt.anchorMin = new Vector2(0, 0);
            accentRt.anchorMax = new Vector2(0, 1);
            accentRt.sizeDelta = new Vector2(4, 0);
            accentRt.anchoredPosition = new Vector2(2, 0);

            // Fixed Main Title (Milex GMS1 CoreMod v1.3.0)
            _windowTitleText = UIFactory.CreateText(header.transform, "TitleText", $"{CorePlugin.PluginName} (v{CorePlugin.PluginVersion})", 13, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            var titleRt = _windowTitleText.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 0.48f);
            titleRt.anchorMax = new Vector2(0.40f, 1f);
            titleRt.offsetMin = new Vector2(16, 0);
            titleRt.offsetMax = new Vector2(0, -2);

            // Subtitle showing currently active mod
            _windowSubtitleText = UIFactory.CreateText(header.transform, "SubtitleText", "> Allgemeine Einstellungen", 11, new Color(0.88f, 0.65f, 0.18f, 1f), TextAnchor.MiddleLeft);
            var subRt = _windowSubtitleText.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0, 0);
            subRt.anchorMax = new Vector2(0.40f, 0.48f);
            subRt.offsetMin = new Vector2(16, 2);
            subRt.offsetMax = Vector2.zero;

            // Search Bar in Header
            _searchInput = UIFactory.CreateInputField(header.transform, "SearchInput", L("UI_SearchPlaceholder", "Search settings..."), (term) =>
            {
                _searchFilter = term.Trim();
                FilterCards();
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
            tabsRt.sizeDelta = new Vector2(0, 32);
            tabsRt.offsetMin = new Vector2(6, -38);
            tabsRt.offsetMax = new Vector2(-6, -6);

            tabsBar.AddComponent<RectMask2D>();

            var hlg = tabsBar.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.spacing = 4f;
            hlg.padding = new RectOffset(3, 3, 3, 3);
            _tabsContentRt = tabsRt;

            // Scrollable Settings List
            var (settingsScroll, settingsContent, scrollRect) = UIFactory.CreateScrollView(contentPanel.transform, "SettingsScrollView", horizontal: false);
            var setScrollRt = settingsScroll.GetComponent<RectTransform>();
            setScrollRt.anchorMin = Vector2.zero;
            setScrollRt.anchorMax = Vector2.one;
            setScrollRt.offsetMin = new Vector2(6, 6);
            setScrollRt.offsetMax = new Vector2(-6, -44);
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

            // Sync scale with CorePlugin.UIScale & enforce high-DPI font rendering
            float scale = CorePlugin.UIScale != null ? Mathf.Clamp(CorePlugin.UIScale.Value, 0.8f, 1.8f) : 1.0f;
            if (_scaler != null)
            {
                _scaler.referenceResolution = new Vector2(1920f / scale, 1080f / scale);
                _scaler.dynamicPixelsPerUnit = 1.0f;
            }

            if (_canvasRoot != null) _canvasRoot.SetActive(true);
            if (_canvas != null) _canvas.enabled = true;

            RefreshSidebar();
            RefreshActiveModView();

            Canvas.ForceUpdateCanvases();
            
            // Force a full layout rebuild on all content containers after everything is instantiated
            if (_sidebarContentRt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(_sidebarContentRt);
            if (_tabsContentRt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(_tabsContentRt);
            if (_settingsContentRt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(_settingsContentRt);
            if (_windowPanelRt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(_windowPanelRt);
        }

        public void Hide()
        {
            CloseModal();
            _isLangDropdownOpen = false;
            if (_canvas != null)
            {
                _canvas.enabled = false;
            }
            if (_canvasRoot != null)
            {
                _canvasRoot.SetActive(false);
            }
        }

        public void Toggle()
        {
            if (IsVisible) Hide();
            else Show();
        }

        public void Render()
        {
            // uGUI Canvas renders automatically every frame via Unity's native graphic pipeline
        }

        public void Cleanup()
        {
            LocalizationManager.OnLanguageChanged -= HandleLanguageChanged;
            CloseModal();
            _isLangDropdownOpen = false;
            if (_canvasRoot != null)
            {
                Destroy(_canvasRoot);
                _canvasRoot = null;
            }
        }

        private List<ModInfo> GetFeatureMods()
        {
            return ModRegistry.RegisteredMods
                .Where(m => !m.Guid.Equals(CorePlugin.PluginGuid, StringComparison.OrdinalIgnoreCase)
                         && !m.AssemblyName.Equals(CoreAssemblyName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private class SidebarCardData : MonoBehaviour
        {
            public int ModIndex;
            public GameObject Accent;
            public Button Button;
            public Image BackgroundImage;
        }

        private void RefreshSidebar()
        {
            foreach (var btn in _createdSidebarButtons)
            {
                if (btn != null)
                {
                    btn.SetActive(false);
                    Destroy(btn);
                }
            }
            _createdSidebarButtons.Clear();

            var featureMods = GetFeatureMods();

            // CoreMod Button Card
            CreateSidebarCard(-1, CorePlugin.PluginName, CorePlugin.PluginVersion, true, false, _selectedModIndex == -1);

            // Sub-Mods Button Cards
            for (int i = 0; i < featureMods.Count; i++)
            {
                var mod = featureMods[i];
                CreateSidebarCard(i, mod.Name, mod.Version, mod.IsEnabled, mod.CanBeDisabled, _selectedModIndex == i);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_sidebarContentRt);
        }

        private void SelectSidebarMod(int modIndex)
        {
            _selectedModIndex = modIndex;
            _activeCategory = "All";

            for (int i = 0; i < _createdSidebarButtons.Count; i++)
            {
                var go = _createdSidebarButtons[i];
                if (go == null) continue;
                var data = go.GetComponent<SidebarCardData>();
                if (data == null) continue;

                bool isSelected = (data.ModIndex == _selectedModIndex);
                Color cardColor = isSelected ? new Color(0.20f, 0.25f, 0.36f, 1f) : new Color(0.14f, 0.16f, 0.22f, 1f);
                Color hoverColor = new Color(0.28f, 0.35f, 0.48f, 1f);

                if (data.BackgroundImage != null)
                {
                    data.BackgroundImage.color = cardColor;
                }

                if (data.Button != null)
                {
                    var colors = data.Button.colors;
                    colors.normalColor = cardColor;
                    colors.highlightedColor = hoverColor;
                    colors.pressedColor = cardColor;
                    colors.selectedColor = cardColor; // Prevent button turning white!
                    data.Button.colors = colors;
                }

                if (data.Accent != null) data.Accent.SetActive(isSelected);
            }

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            RefreshActiveModView();
        }

        private void CreateSidebarCard(int modIndex, string name, string version, bool isEnabled, bool canBeDisabled, bool isSelected)
        {
            Color cardColor = isSelected ? new Color(0.20f, 0.25f, 0.36f, 1f) : new Color(0.14f, 0.16f, 0.22f, 1f);
            Color hoverColor = new Color(0.28f, 0.35f, 0.48f, 1f); // Prominent slate-blue hover highlight!

            var card = UIFactory.CreateButton(_sidebarContentRt, $"ModCard_{modIndex}", "", cardColor, hoverColor, cardColor, Color.white, () =>
            {
                SelectSidebarMod(modIndex);
            });

            var cardRt = card.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(0, 46);

            var le = card.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 46;
            le.preferredHeight = 46;
            le.flexibleWidth = 1;

            // Left Gold Accent if selected
            var accent = UIFactory.CreatePanel(card.transform, "SelectAccent", new Color(0.88f, 0.65f, 0.18f, 1f), raycastTarget: false);
            var accRt = accent.GetComponent<RectTransform>();
            accRt.anchorMin = new Vector2(0, 0);
            accRt.anchorMax = new Vector2(0, 1);
            accRt.sizeDelta = new Vector2(4, 0);
            accRt.anchoredPosition = new Vector2(2, 0);
            accent.SetActive(isSelected);

            var img = card.GetComponent<Image>();
            if (img != null) img.color = cardColor;

            var colors = card.colors;
            colors.selectedColor = cardColor;
            card.colors = colors;

            var data = card.gameObject.AddComponent<SidebarCardData>();
            data.ModIndex = modIndex;
            data.Accent = accent;
            data.Button = card;
            data.BackgroundImage = img;

            // Name (direct fixed mod name)
            var nameTxt = UIFactory.CreateText(card.transform, "Name", name, 12, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            var nameRt = nameTxt.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 0.48f);
            nameRt.anchorMax = new Vector2(0.72f, 1f);
            nameRt.offsetMin = new Vector2(10, 0);
            nameRt.offsetMax = new Vector2(0, -2);
            nameTxt.raycastTarget = false;

            // Version & Status Badge
            string statusStr = isEnabled ? "v" + version : L("UI_Status_Disabled", "Disabled");
            Color statusColor = isEnabled ? new Color(0.5f, 0.85f, 0.5f, 0.95f) : new Color(0.85f, 0.4f, 0.4f, 0.95f);
            var verTxt = UIFactory.CreateText(card.transform, "Version", statusStr, 10, statusColor, TextAnchor.MiddleLeft);
            var verRt = verTxt.GetComponent<RectTransform>();
            verRt.anchorMin = new Vector2(0, 0);
            verRt.anchorMax = new Vector2(0.72f, 0.48f);
            verRt.offsetMin = new Vector2(10, 2);
            verRt.offsetMax = Vector2.zero;
            verTxt.raycastTarget = false;

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
                togRt.anchoredPosition = new Vector2(-26, 0);
                togRt.localScale = new Vector3(0.75f, 0.75f, 1f);
            }

            _createdSidebarButtons.Add(card.gameObject);
        }

        private void RefreshActiveModView()
        {
            RefreshTabs();
            RefreshSettingsContent(resetScroll: true);
        }

        private void RefreshTabs()
        {
            foreach (var tab in _createdTabButtons)
            {
                if (tab != null)
                {
                    tab.SetActive(false);
                    Destroy(tab);
                }
            }
            _createdTabButtons.Clear();

            var categories = GetActiveModCategories();
            var featureMods = GetFeatureMods();
            ModInfo currentMod = (_selectedModIndex >= 0 && _selectedModIndex < featureMods.Count) 
                ? featureMods[_selectedModIndex] 
                : null;

            foreach (var cat in categories)
            {
                bool isSelected = _activeCategory.Equals(cat, StringComparison.OrdinalIgnoreCase);
                Color tabColor = isSelected ? new Color(0.88f, 0.65f, 0.18f, 1f) : new Color(0.16f, 0.20f, 0.28f, 1f);
                Color hoverColor = isSelected ? new Color(0.96f, 0.75f, 0.25f, 1f) : new Color(0.28f, 0.36f, 0.50f, 1f);
                Color textColor = isSelected ? new Color(0.08f, 0.08f, 0.10f, 1f) : Color.white;

                string label = GetShortCategoryLabel(currentMod, cat);

                var tabBtn = UIFactory.CreateButton(_tabsContentRt, $"Tab_{cat}", label, tabColor, hoverColor, tabColor, textColor, () =>
                {
                    _activeCategory = cat;
                    UpdateTabVisuals();
                    FilterCards();
                }, 11);

                float width = Mathf.Max(42f, label.Length * 7.5f + 16f);
                var tabRt = tabBtn.GetComponent<RectTransform>();
                tabRt.sizeDelta = new Vector2(width, 26);

                var le = tabBtn.gameObject.AddComponent<LayoutElement>();
                le.minWidth = 32f;
                le.preferredWidth = width;
                le.minHeight = 26;
                le.preferredHeight = 26;

                _createdTabButtons.Add(tabBtn.gameObject);
            }

            if (_tabsContentRt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(_tabsContentRt);
        }

        private void UpdateTabVisuals()
        {
            var categories = GetActiveModCategories();
            for (int i = 0; i < _createdTabButtons.Count && i < categories.Count; i++)
            {
                var tabGo = _createdTabButtons[i];
                if (tabGo == null) continue;
                string cat = categories[i];
                bool isSelected = _activeCategory.Equals(cat, StringComparison.OrdinalIgnoreCase);
                Color tabColor = isSelected ? new Color(0.88f, 0.65f, 0.18f, 1f) : new Color(0.16f, 0.20f, 0.28f, 1f);
                Color hoverColor = isSelected ? new Color(0.96f, 0.75f, 0.25f, 1f) : new Color(0.28f, 0.36f, 0.50f, 1f);
                Color textColor = isSelected ? new Color(0.08f, 0.08f, 0.10f, 1f) : Color.white;

                var img = tabGo.GetComponent<Image>();
                if (img != null) img.color = tabColor;

                var btn = tabGo.GetComponent<Button>();
                if (btn != null)
                {
                    var colors = btn.colors;
                    colors.normalColor = tabColor;
                    colors.highlightedColor = hoverColor;
                    colors.pressedColor = tabColor;
                    colors.selectedColor = tabColor;
                    btn.colors = colors;
                }

                var txt = tabGo.GetComponentInChildren<Text>();
                if (txt != null) txt.color = textColor;
            }
        }

        private string GetShortCategoryLabel(ModInfo mod, string rawCat)
        {
            if (rawCat.Equals("All", StringComparison.OrdinalIgnoreCase))
                return L("UI_Tab_All", "Alle");

            if (rawCat.Equals("General", StringComparison.OrdinalIgnoreCase) || rawCat.Equals("Allgemein", StringComparison.OrdinalIgnoreCase))
                return L("UI_Tab_General", "Allgemein");

            string fullLabel = mod != null 
                ? mod.Translate($"config.{rawCat.ToLowerInvariant()}.section", rawCat) 
                : L($"config.{rawCat.ToLowerInvariant()}.section", rawCat);

            // Clean short labels
            if (fullLabel.IndexOf("Handwerkzeuge", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Werkzeuge";
            if (fullLabel.IndexOf("Baufahrzeuge", StringComparison.OrdinalIgnoreCase) >= 0 || fullLabel.IndexOf("HeavyMachinery", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Fahrzeuge";
            if (fullLabel.IndexOf("Waschanlagen", StringComparison.OrdinalIgnoreCase) >= 0 || fullLabel.IndexOf("WashPlants", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Waschanlagen";
            if (fullLabel.IndexOf("Feinverarbeitung", StringComparison.OrdinalIgnoreCase) >= 0 || fullLabel.IndexOf("Refinement", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Veredelung";
            if (fullLabel.IndexOf("Anhänger", StringComparison.OrdinalIgnoreCase) >= 0 || fullLabel.IndexOf("Logistik", StringComparison.OrdinalIgnoreCase) >= 0 || fullLabel.IndexOf("Trailers", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Logistik";

            if (fullLabel.Contains(" - "))
            {
                string afterDash = fullLabel.Substring(fullLabel.IndexOf(" - ") + 3).Trim();
                if (afterDash.Contains(" & ")) return afterDash.Split(new[] { " & " }, StringSplitOptions.None)[0].Trim();
                if (afterDash.Contains(" (")) return afterDash.Split('(')[0].Trim();
                return afterDash;
            }

            if (fullLabel.StartsWith("Group", StringComparison.OrdinalIgnoreCase))
                return fullLabel.Replace("Group", "Gr. ").Replace('_', ' ');

            return fullLabel;
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
                    if (entry.Key == null || string.IsNullOrEmpty(entry.Key.Section))
                        continue;

                    if (entry.Key.Section.Equals("General", StringComparison.OrdinalIgnoreCase) && entry.Key.Key.Equals("Enabled", StringComparison.OrdinalIgnoreCase))
                        continue;

                    list.Add(entry.Key.Section);
                }
            }

            return list.ToList();
        }

        private void RefreshSettingsContent(bool resetScroll = false)
        {
            float prevPos = _settingsScrollRect != null ? _settingsScrollRect.verticalNormalizedPosition : 1f;

            foreach (var card in _createdSettingCards)
            {
                if (card != null)
                {
                    card.SetActive(false);
                    Destroy(card);
                }
            }
            _createdSettingCards.Clear();

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

            LayoutRebuilder.ForceRebuildLayoutImmediate(_settingsContentRt);
            if (_settingsScrollRect != null)
            {
                _settingsScrollRect.verticalNormalizedPosition = resetScroll ? 1f : prevPos;
            }
        }
        private class SettingCardMeta : MonoBehaviour
        {
            public string Section;
            public string SearchText;
        }

        private void FilterCards()
        {
            string search = (_searchFilter ?? "").ToLowerInvariant();

            foreach (var card in _createdSettingCards)
            {
                if (card == null) continue;
                var meta = card.GetComponent<SettingCardMeta>();
                if (meta == null) continue;

                bool matchesCategory = (_activeCategory == "All") || 
                                       meta.Section.Equals(_activeCategory, StringComparison.OrdinalIgnoreCase);

                bool matchesSearch = string.IsNullOrEmpty(search) || 
                                     meta.SearchText.Contains(search);

                bool shouldShow = matchesCategory && matchesSearch;
                if (card.activeSelf != shouldShow)
                {
                    card.SetActive(shouldShow);
                }
            }

            if (_settingsContentRt != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_settingsContentRt);
            }
        }

        private void BuildCoreSettingsCards()
        {
            if (_windowSubtitleText != null)
            {
                _windowSubtitleText.text = "> CoreMod & Allgemeine Einstellungen";
            }

            // General Section
            CreateCategoryHeader(L("UI_Section_General", "Allgemeine Einstellungen"), "General");
            CreateToggleCard("General", L("UI_PauseGameOnMenu", "Pause Game in Menu"), L("UI_PauseGameOnMenu_Desc", "Freezes game time while menu is open."), CorePlugin.PauseGameOnMenu.Value, (val) => CorePlugin.PauseGameOnMenu.Value = val);
            CreateSliderCard("General", L("UI_ScaleLabel", "UI Scale"), L("UI_Scale_Desc", "Scale interface size for 1080p, 1440p or 4K."), CorePlugin.UIScale.Value, 0.8f, 1.8f, 1.0f, (val) =>
            {
                CorePlugin.UIScale.Value = (float)Math.Round(val, 2);
                if (_scaler != null) _scaler.referenceResolution = new Vector2(1920f / CorePlugin.UIScale.Value, 1080f / CorePlugin.UIScale.Value);
            });
            CreateToggleCard("General", L("UI_IgnoreTranslations", "Ignore External Translations"), L("UI_IgnoreTranslations_Desc", "Forces embedded DLL localization resources."), CorePlugin.IgnoreExternalTranslations.Value, (val) => CorePlugin.IgnoreExternalTranslations.Value = val);

            // Localization Section
            CreateCategoryHeader(L("UI_Section_Localization", "Sprache & Lokalisierung"), "Localization");
            CreateToggleCard("Localization", L("core.use_game_language.name", "Spiel-Sprache verwenden"), L("core.use_game_language.desc", "Verwendet automatisch die im Spiel bzw. System eingestellte Sprache"), CorePlugin.UseGameLanguage.Value, (val) =>
            {
                CorePlugin.UseGameLanguage.Value = val;
                CorePlugin.UseGameLanguage.ConfigFile?.Save();
                _isLangDropdownOpen = false;
                LocalizationManager.ReloadAll();
                RefreshSidebar();
                RefreshActiveModView();
            });

            // Language Selector Card - ALWAYS VISIBLE, only active when automatic detection is off
            CreateLanguageSelectorCard("Localization");

            FilterCards();
        }

        private void CreateLanguageSelectorCard(string section = "Localization")
        {
            bool useGameLang = CorePlugin.UseGameLanguage != null && CorePlugin.UseGameLanguage.Value;
            bool isEnabled = !useGameLang;
            if (!isEnabled)
            {
                _isLangDropdownOpen = false;
            }

            float cardHeight = _isLangDropdownOpen ? 240f : 48f;

            var card = UIFactory.CreatePanel(_settingsContentRt, "LanguageSelectorCard", new Color(0.13f, 0.15f, 0.20f, 1f), UIFactory.CardBoxSprite);
            var cardRt = card.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(0, cardHeight);

            var le = card.AddComponent<LayoutElement>();
            le.minHeight = cardHeight;
            le.preferredHeight = cardHeight;
            le.flexibleWidth = 1;

            var meta = card.AddComponent<SettingCardMeta>();
            meta.Section = section;
            meta.SearchText = "sprache language deutsch english polski francais";

            // Top Row Container (Height 48)
            var topRow = new GameObject("TopRow", typeof(RectTransform));
            topRow.transform.SetParent(card.transform, false);
            var trRt = topRow.GetComponent<RectTransform>();
            trRt.anchorMin = new Vector2(0, 1);
            trRt.anchorMax = new Vector2(1, 1);
            trRt.pivot = new Vector2(0.5f, 1);
            trRt.sizeDelta = new Vector2(0, 48);
            trRt.anchoredPosition = Vector2.zero;

            string selLabel = L("core.selected_language.name", "Sprache wählen");
            string selDesc = L("core.selected_language.desc", "Manuelle Sprachauswahl (nur aktiv, wenn 'Spiel-Sprache verwenden' abgewählt ist)");

            Color labelColor = isEnabled ? Color.white : new Color(0.55f, 0.58f, 0.65f, 0.6f);
            Color descColor = isEnabled ? new Color(0.6f, 0.65f, 0.75f, 0.9f) : new Color(0.45f, 0.48f, 0.55f, 0.5f);

            var lbl = UIFactory.CreateText(topRow.transform, "Label", selLabel, 13, labelColor, TextAnchor.MiddleLeft, FontStyle.Bold);
            var lRt = lbl.GetComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0, 0.48f);
            lRt.anchorMax = new Vector2(0.50f, 1f);
            lRt.offsetMin = new Vector2(12, 0);
            lRt.offsetMax = new Vector2(0, -3);

            var dsc = UIFactory.CreateText(topRow.transform, "Desc", selDesc, 10, descColor, TextAnchor.MiddleLeft);
            var dRt = dsc.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0, 0);
            dRt.anchorMax = new Vector2(0.50f, 0.48f);
            dRt.offsetMin = new Vector2(12, 3);
            dRt.offsetMax = Vector2.zero;

            // Current language display
            string curCode = isEnabled ? (CorePlugin.SelectedLanguage?.Value ?? "en") : LocalizationManager.CurrentLanguage;
            string curName = LocalizationManager.GetLanguageNativeName(curCode);
            string arrowStr = _isLangDropdownOpen ? "^" : "v";
            string dropdownBtnText = $"{curName} ({curCode.ToUpperInvariant()})  {arrowStr}";

            // Trigger button on the right
            Color btnNorm = isEnabled ? new Color(0.20f, 0.24f, 0.32f, 1f) : new Color(0.15f, 0.17f, 0.22f, 0.4f);
            Color btnHover = isEnabled ? new Color(0.28f, 0.35f, 0.46f, 1f) : btnNorm;
            Color btnTextCol = isEnabled ? Color.white : new Color(0.55f, 0.58f, 0.65f, 0.5f);

            var triggerBtn = UIFactory.CreateButton(topRow.transform, "LangDropdownTrigger", dropdownBtnText,
                btnNorm, btnHover, btnNorm, btnTextCol, () =>
            {
                if (!isEnabled) return;
                _isLangDropdownOpen = !_isLangDropdownOpen;
                RefreshSettingsContent(resetScroll: false);
            }, 12);

            var tbRt = triggerBtn.GetComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0.52f, 0.16f);
            tbRt.anchorMax = new Vector2(0.985f, 0.84f);
            tbRt.offsetMin = Vector2.zero;
            tbRt.offsetMax = Vector2.zero;

            if (!isEnabled)
            {
                triggerBtn.interactable = false;
            }

            // If dropdown is open, render the scrollable list of all supported languages
            if (_isLangDropdownOpen && isEnabled)
            {
                var listPanel = UIFactory.CreatePanel(card.transform, "LangListContainer", new Color(0.09f, 0.11f, 0.14f, 1f), UIFactory.RoundedBoxSprite);
                var lpRt = listPanel.GetComponent<RectTransform>();
                lpRt.anchorMin = new Vector2(0, 0);
                lpRt.anchorMax = new Vector2(1, 1);
                lpRt.offsetMin = new Vector2(10, 8);
                lpRt.offsetMax = new Vector2(-10, -50);

                var (scrollRoot, scrollContent, scrollRect) = UIFactory.CreateScrollView(listPanel.transform, "LangScrollView", horizontal: false);
                var sRt = scrollRoot.GetComponent<RectTransform>();
                sRt.anchorMin = Vector2.zero;
                sRt.anchorMax = Vector2.one;
                sRt.offsetMin = new Vector2(4, 4);
                sRt.offsetMax = new Vector2(-4, -4);

                foreach (var lang in LocalizationManager.SupportedLanguages)
                {
                    bool isCur = lang.Code.Equals(curCode, StringComparison.OrdinalIgnoreCase);
                    string itemText = isCur ? $"{lang.NativeName} ({lang.Code})  [v]" : $"{lang.NativeName} ({lang.Code})";

                    Color itemNorm = isCur ? new Color(0.88f, 0.65f, 0.18f, 1f) : new Color(0.16f, 0.19f, 0.26f, 1f);
                    Color itemHover = isCur ? new Color(0.96f, 0.75f, 0.25f, 1f) : new Color(0.26f, 0.33f, 0.46f, 1f);
                    Color itemTxtCol = isCur ? new Color(0.08f, 0.08f, 0.10f, 1f) : Color.white;

                    var itemBtn = UIFactory.CreateButton(scrollContent, $"LangItem_{lang.Code}", itemText,
                        itemNorm, itemHover, itemNorm, itemTxtCol, () =>
                    {
                        _isLangDropdownOpen = false;
                        CorePlugin.SelectedLanguage.Value = lang.Code;
                        CorePlugin.SelectedLanguage.ConfigFile?.Save();
                        LocalizationManager.NotifyLanguageChanged(lang.Code);

                        var missingMods = LocalizationManager.GetModsMissingLanguage(lang.Code);
                        if (missingMods.Count > 0)
                        {
                            ShowMissingTranslationModal(missingMods, lang.Code);
                        }
                        else
                        {
                            RefreshSidebar();
                            RefreshActiveModView();
                        }
                    }, 11);

                    var itemRt = itemBtn.GetComponent<RectTransform>();
                    itemRt.sizeDelta = new Vector2(0, 26);

                    var itemLe = itemBtn.gameObject.AddComponent<LayoutElement>();
                    itemLe.minHeight = 26;
                    itemLe.preferredHeight = 26;
                    itemLe.flexibleWidth = 1;
                }
            }

            _createdSettingCards.Add(card);
        }

        private void CloseModal()
        {
            if (_modalOverlay != null)
            {
                Destroy(_modalOverlay);
                _modalOverlay = null;
            }
        }

        private void ShowMissingTranslationModal(List<string> missingMods, string targetLang)
        {
            CloseModal();

            if (_canvasRoot == null) return;

            // Full-screen dark backdrop overlay
            _modalOverlay = new GameObject("MissingTranslationModal", typeof(RectTransform), typeof(Image));
            _modalOverlay.transform.SetParent(_canvasRoot.transform, false);
            var moRt = _modalOverlay.GetComponent<RectTransform>();
            moRt.anchorMin = Vector2.zero;
            moRt.anchorMax = Vector2.one;
            moRt.offsetMin = Vector2.zero;
            moRt.offsetMax = Vector2.zero;

            var moImg = _modalOverlay.GetComponent<Image>();
            moImg.color = new Color(0f, 0f, 0f, 0.65f);
            moImg.raycastTarget = true;

            // Centered Classic Modal Dialog Box (Compact: 460 x 252)
            var dialog = new GameObject("DialogBox", typeof(RectTransform), typeof(Image));
            dialog.transform.SetParent(_modalOverlay.transform, false);
            var dRt = dialog.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0.5f, 0.5f);
            dRt.anchorMax = new Vector2(0.5f, 0.5f);
            dRt.pivot = new Vector2(0.5f, 0.5f);
            dRt.sizeDelta = new Vector2(460, 252);
            dRt.anchoredPosition = Vector2.zero;

            var dImg = dialog.GetComponent<Image>();
            dImg.sprite = UIFactory.CardBoxSprite;
            dImg.type = Image.Type.Sliced;
            dImg.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);

            // Dialog Header (Height 32)
            var dHeader = new GameObject("Header", typeof(RectTransform), typeof(Image));
            dHeader.transform.SetParent(dialog.transform, false);
            var dhRt = dHeader.GetComponent<RectTransform>();
            dhRt.anchorMin = new Vector2(0, 1);
            dhRt.anchorMax = new Vector2(1, 1);
            dhRt.pivot = new Vector2(0.5f, 1);
            dhRt.sizeDelta = new Vector2(0, 32);
            dhRt.anchoredPosition = Vector2.zero;

            var dhImg = dHeader.GetComponent<Image>();
            dhImg.sprite = UIFactory.RoundedBoxSprite;
            dhImg.type = Image.Type.Sliced;
            dhImg.color = new Color(0.16f, 0.19f, 0.26f, 1f);

            // Left Gold Accent Bar
            var acc = new GameObject("Accent", typeof(RectTransform), typeof(Image));
            acc.transform.SetParent(dHeader.transform, false);
            var accRt = acc.GetComponent<RectTransform>();
            accRt.anchorMin = new Vector2(0, 0);
            accRt.anchorMax = new Vector2(0, 1);
            accRt.sizeDelta = new Vector2(3, 0);
            accRt.anchoredPosition = new Vector2(2, 0);
            var accImg = acc.GetComponent<Image>();
            accImg.color = new Color(0.88f, 0.65f, 0.18f, 1f);

            string titleStr = L("dialog.missing_trans.title", "Fehlende Sprachdateien erkannt");
            var titleTxt = UIFactory.CreateText(dHeader.transform, "Title", titleStr.ToUpperInvariant(), 11, new Color(0.92f, 0.72f, 0.20f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold);
            var ttRt = titleTxt.GetComponent<RectTransform>();
            ttRt.anchorMin = Vector2.zero;
            ttRt.anchorMax = Vector2.one;
            ttRt.offsetMin = new Vector2(12, 0);
            ttRt.offsetMax = new Vector2(-12, 0);

            // Close button [X] on top right of modal
            var closeBtn = UIFactory.CreateButton(dHeader.transform, "ModalCloseBtn", "X",
                new Color(0.75f, 0.20f, 0.20f, 1f),
                new Color(0.90f, 0.25f, 0.25f, 1f),
                new Color(0.60f, 0.15f, 0.15f, 1f),
                Color.white, () =>
            {
                CloseModal();
                RefreshSidebar();
                RefreshActiveModView();
            }, 12);
            var cbRt = closeBtn.GetComponent<RectTransform>();
            cbRt.anchorMin = new Vector2(1, 0.5f);
            cbRt.anchorMax = new Vector2(1, 0.5f);
            cbRt.pivot = new Vector2(1, 0.5f);
            cbRt.sizeDelta = new Vector2(22, 22);
            cbRt.anchoredPosition = new Vector2(-6, 0);

            // Prompt text (Top portion of content)
            string nativeLang = LocalizationManager.GetLanguageNativeName(targetLang);
            string promptStr = string.Format(L("dialog.missing_trans.prompt", "Für folgende Mods existiert noch keine Übersetzung für '{0}':"), nativeLang);
            var promptTxt = UIFactory.CreateText(dialog.transform, "Prompt", promptStr, 11, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            var pRt = promptTxt.GetComponent<RectTransform>();
            pRt.anchorMin = new Vector2(0, 1);
            pRt.anchorMax = new Vector2(1, 1);
            pRt.pivot = new Vector2(0.5f, 1);
            pRt.sizeDelta = new Vector2(0, 24);
            pRt.anchoredPosition = new Vector2(0, -38);
            pRt.offsetMin = new Vector2(14, pRt.offsetMin.y);
            pRt.offsetMax = new Vector2(-14, pRt.offsetMax.y);

            // Missing Mods Box (Middle portion)
            var modsBox = new GameObject("ModsBox", typeof(RectTransform), typeof(Image));
            modsBox.transform.SetParent(dialog.transform, false);
            var mbRt = modsBox.GetComponent<RectTransform>();
            mbRt.anchorMin = new Vector2(0, 1);
            mbRt.anchorMax = new Vector2(1, 1);
            mbRt.pivot = new Vector2(0.5f, 1);
            mbRt.sizeDelta = new Vector2(0, 44);
            mbRt.anchoredPosition = new Vector2(0, -64);
            mbRt.offsetMin = new Vector2(14, mbRt.offsetMin.y);
            mbRt.offsetMax = new Vector2(-14, mbRt.offsetMax.y);

            var mbImg = modsBox.GetComponent<Image>();
            mbImg.sprite = UIFactory.RoundedBoxSprite;
            mbImg.type = Image.Type.Sliced;
            mbImg.color = new Color(0.08f, 0.09f, 0.12f, 1f);

            string modsListText = string.Join("\n", missingMods.Select(m => $"• {m}").ToArray());
            var modsTxt = UIFactory.CreateText(modsBox.transform, "ModsText", modsListText, 10, new Color(0.85f, 0.88f, 0.92f, 1f), TextAnchor.MiddleLeft);
            var mtRt = modsTxt.GetComponent<RectTransform>();
            mtRt.anchorMin = Vector2.zero;
            mtRt.anchorMax = Vector2.one;
            mtRt.offsetMin = new Vector2(8, 4);
            mtRt.offsetMax = new Vector2(-8, -4);

            // Question text
            string questionStr = L("dialog.missing_trans.question", "Möchtest du, dass wir dir dafür Vorlagen-Dateien zur Übersetzung anlegen?");
            var questionTxt = UIFactory.CreateText(dialog.transform, "Question", questionStr, 10, new Color(0.92f, 0.72f, 0.20f, 1f), TextAnchor.MiddleLeft);
            var qRt = questionTxt.GetComponent<RectTransform>();
            qRt.anchorMin = new Vector2(0, 1);
            qRt.anchorMax = new Vector2(1, 1);
            qRt.pivot = new Vector2(0.5f, 1);
            qRt.sizeDelta = new Vector2(0, 20);
            qRt.anchoredPosition = new Vector2(0, -112);
            qRt.offsetMin = new Vector2(14, qRt.offsetMin.y);
            qRt.offsetMax = new Vector2(-14, qRt.offsetMax.y);

            // Info text and [ Open Folder ] button row
            string infoStr = L("dialog.missing_trans.info", "Die Dateien werden in 'BepInEx/plugins/Milex GMS1 Mod Localization/' erstellt. Du kannst sie übersetzen und im NexusMods-Eintrag des Mods posten!");
            var infoTxt = UIFactory.CreateText(dialog.transform, "Info", infoStr, 9, new Color(0.62f, 0.68f, 0.78f, 0.9f), TextAnchor.MiddleLeft);
            var iRt = infoTxt.GetComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0, 1);
            iRt.anchorMax = new Vector2(1, 1);
            iRt.pivot = new Vector2(0, 1);
            iRt.sizeDelta = new Vector2(0, 32);
            iRt.anchoredPosition = new Vector2(0, -134);
            iRt.offsetMin = new Vector2(14, iRt.offsetMin.y);
            iRt.offsetMax = new Vector2(-112, iRt.offsetMax.y);

            string openLabel = L("dialog.missing_trans.btn_open", "Ordner öffnen");
            var btnOpen = UIFactory.CreateButton(dialog.transform, "BtnOpenFolder", openLabel,
                new Color(0.20f, 0.24f, 0.32f, 1f),
                new Color(0.28f, 0.35f, 0.48f, 1f),
                new Color(0.16f, 0.20f, 0.28f, 1f),
                Color.white, () =>
            {
                LocalizationManager.OpenLocalizationFolder();
            }, 10);
            var boRt = btnOpen.GetComponent<RectTransform>();
            boRt.anchorMin = new Vector2(1, 1);
            boRt.anchorMax = new Vector2(1, 1);
            boRt.pivot = new Vector2(1, 1);
            boRt.sizeDelta = new Vector2(94, 26);
            boRt.anchoredPosition = new Vector2(-14, -138);

            // Action Buttons (Bottom row: Y = 14px from bottom, Height 32px)
            // Yes Button
            string yesLabel = L("dialog.missing_trans.btn_yes", "Vorlagen erstellen");
            var btnYes = UIFactory.CreateButton(dialog.transform, "BtnYes", yesLabel,
                new Color(0.88f, 0.65f, 0.18f, 1f),
                new Color(0.96f, 0.75f, 0.25f, 1f),
                new Color(0.70f, 0.50f, 0.12f, 1f),
                new Color(0.08f, 0.08f, 0.10f, 1f), () =>
            {
                LocalizationManager.GenerateTemplatesForMods(missingMods, targetLang);
                LocalizationManager.OpenLocalizationFolder();
                CloseModal();
                RefreshSidebar();
                RefreshActiveModView();
            }, 11);
            var byRt = btnYes.GetComponent<RectTransform>();
            byRt.anchorMin = new Vector2(0, 0);
            byRt.anchorMax = new Vector2(0.5f, 0);
            byRt.pivot = new Vector2(0.5f, 0);
            byRt.sizeDelta = new Vector2(0, 32);
            byRt.anchoredPosition = new Vector2(0, 14);
            byRt.offsetMin = new Vector2(14, 14);
            byRt.offsetMax = new Vector2(-6, 46);

            // No Button
            string noLabel = L("dialog.missing_trans.btn_no", "Nein, Standard behalten");
            var btnNo = UIFactory.CreateButton(dialog.transform, "BtnNo", noLabel,
                new Color(0.20f, 0.24f, 0.32f, 1f),
                new Color(0.28f, 0.35f, 0.46f, 1f),
                new Color(0.16f, 0.20f, 0.28f, 1f),
                Color.white, () =>
            {
                CloseModal();
                RefreshSidebar();
                RefreshActiveModView();
            }, 11);
            var bnRt = btnNo.GetComponent<RectTransform>();
            bnRt.anchorMin = new Vector2(0.5f, 0);
            bnRt.anchorMax = new Vector2(1f, 0);
            bnRt.pivot = new Vector2(0.5f, 0);
            bnRt.sizeDelta = new Vector2(0, 32);
            bnRt.anchoredPosition = new Vector2(0, 14);
            bnRt.offsetMin = new Vector2(6, 14);
            bnRt.offsetMax = new Vector2(-14, 46);
        }

        private void BuildModSettingsCards(ModInfo mod)
        {
            if (_windowSubtitleText != null)
            {
                _windowSubtitleText.text = $"> {mod.Name} (v{mod.Version})";
            }

            if (mod.Config == null) return;

            string lastSection = "";

            var entries = mod.Config
                .Where(kv => kv.Key != null && !string.IsNullOrEmpty(kv.Key.Section))
                .Where(kv => !kv.Key.Section.Equals("General", StringComparison.OrdinalIgnoreCase) || !kv.Key.Key.Equals("Enabled", StringComparison.OrdinalIgnoreCase))
                .OrderBy(kv => kv.Key.Section)
                .ThenBy(kv => kv.Key.Key)
                .ToList();

            foreach (var kv in entries)
            {
                var entry = kv.Value;
                if (!kv.Key.Section.Equals(lastSection, StringComparison.OrdinalIgnoreCase))
                {
                    lastSection = kv.Key.Section;
                    string currentSec = lastSection;
                    string sectionTitle = mod.Translate($"config.{currentSec.ToLowerInvariant()}.section", currentSec);
                    if (sectionTitle.Equals(currentSec, StringComparison.OrdinalIgnoreCase))
                    {
                        sectionTitle = currentSec.Replace('_', ' ');
                    }

                    Action resetGroupAction = () =>
                    {
                        foreach (var e in mod.Config.Where(k => k.Key != null && k.Key.Section.Equals(currentSec, StringComparison.OrdinalIgnoreCase)))
                        {
                            e.Value.BoxedValue = e.Value.DefaultValue;
                        }
                        mod.Config.Save();
                        RefreshSettingsContent(resetScroll: false);
                    };

                    CreateCategoryHeader(sectionTitle, currentSec, resetGroupAction);
                }

                string rawKey = kv.Key.Key;
                string rawSec = kv.Key.Section;
                string entryNameKey = $"config.{rawSec.ToLowerInvariant()}.{rawKey.ToLowerInvariant()}.name";
                string entryDescKey = $"config.{rawSec.ToLowerInvariant()}.{rawKey.ToLowerInvariant()}.desc";

                string label = mod.Translate(entryNameKey, rawKey);
                if (label.Equals(rawKey, StringComparison.OrdinalIgnoreCase))
                {
                    label = mod.Translate($"Config_{rawKey}", rawKey.Replace('_', ' '));
                }

                string desc = mod.Translate(entryDescKey, entry.Description?.Description ?? "");
                if (string.IsNullOrEmpty(desc))
                {
                    desc = mod.Translate($"Config_{rawKey}_Desc", "");
                }

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

                    CreateSliderCard(rawSec, label, desc, floatEntry.Value, min, max, defVal, (val) =>
                    {
                        floatEntry.Value = (float)Math.Round(val, 2);
                    });
                }
                else if (entry.SettingType == typeof(bool))
                {
                    var boolEntry = (ConfigEntry<bool>)entry;
                    CreateToggleCard(rawSec, label, desc, boolEntry.Value, (val) =>
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

                    CreateSliderCard(rawSec, label, desc, intEntry.Value, min, max, defVal, (val) =>
                    {
                        intEntry.Value = Mathf.RoundToInt(val);
                    });
                }
            }

            FilterCards();
        }

        private void CreateCategoryHeader(string title, string section, Action onReset = null)
        {
            var header = UIFactory.CreatePanel(_settingsContentRt, $"Header_{title}", new Color(0.15f, 0.18f, 0.25f, 0.95f), UIFactory.CardBoxSprite);
            var hRt = header.GetComponent<RectTransform>();
            hRt.sizeDelta = new Vector2(0, 28);

            var le = header.AddComponent<LayoutElement>();
            le.minHeight = 28;
            le.preferredHeight = 28;
            le.flexibleWidth = 1;

            var meta = header.AddComponent<SettingCardMeta>();
            meta.Section = section;
            meta.SearchText = title.ToLowerInvariant();

            // Gold accent indicator on the left
            var accent = UIFactory.CreatePanel(header.transform, "HeaderAccent", new Color(0.88f, 0.65f, 0.18f, 1f));
            var accRt = accent.GetComponent<RectTransform>();
            accRt.anchorMin = new Vector2(0, 0);
            accRt.anchorMax = new Vector2(0, 1);
            accRt.sizeDelta = new Vector2(3, 0);
            accRt.anchoredPosition = new Vector2(2, 0);

            var txt = UIFactory.CreateText(header.transform, "Title", title.ToUpperInvariant(), 11, new Color(0.92f, 0.72f, 0.20f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold);
            var tRt = txt.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = onReset != null ? new Vector2(0.68f, 1f) : Vector2.one;
            tRt.offsetMin = new Vector2(12, 0);
            tRt.offsetMax = new Vector2(-10, 0);

            if (onReset != null)
            {
                string resetLabel = L("UI_ResetGroup", "Gruppe zurücksetzen");
                var resetBtn = UIFactory.CreateButton(header.transform, "ResetGroupBtn", resetLabel,
                    new Color(0.20f, 0.24f, 0.32f, 0.95f),
                    new Color(0.88f, 0.65f, 0.18f, 1f),
                    new Color(0.70f, 0.50f, 0.12f, 1f),
                    Color.white, () => onReset(), 10);

                var rRt = resetBtn.GetComponent<RectTransform>();
                rRt.anchorMin = new Vector2(0.70f, 0.12f);
                rRt.anchorMax = new Vector2(0.99f, 0.88f);
                rRt.offsetMin = Vector2.zero;
                rRt.offsetMax = new Vector2(-4, 0);
            }

            _createdSettingCards.Add(header);
        }

        private void CreateSliderCard(string section, string label, string desc, float currentValue, float min, float max, float defaultValue, Action<float> onValueChanged)
        {
            var card = UIFactory.CreatePanel(_settingsContentRt, $"SliderCard_{label}", new Color(0.13f, 0.15f, 0.20f, 1f), UIFactory.CardBoxSprite);
            var cardRt = card.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(0, 50);

            var le = card.AddComponent<LayoutElement>();
            le.minHeight = 50;
            le.preferredHeight = 50;
            le.flexibleWidth = 1;

            var meta = card.AddComponent<SettingCardMeta>();
            meta.Section = section;
            meta.SearchText = $"{label} {desc}".ToLowerInvariant();

            // Label & Description
            var lbl = UIFactory.CreateText(card.transform, "Label", label, 13, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            var lRt = lbl.GetComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0, 0.48f);
            lRt.anchorMax = new Vector2(0.53f, 1f);
            lRt.offsetMin = new Vector2(12, 0);
            lRt.offsetMax = new Vector2(0, -3);

            var dsc = UIFactory.CreateText(card.transform, "Desc", desc, 10, new Color(0.6f, 0.65f, 0.75f, 0.9f), TextAnchor.MiddleLeft);
            var dRt = dsc.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0, 0);
            dRt.anchorMax = new Vector2(0.53f, 0.48f);
            dRt.offsetMin = new Vector2(12, 3);
            dRt.offsetMax = Vector2.zero;

            // Value Display Text
            var valTxt = UIFactory.CreateText(card.transform, "ValueText", $"{currentValue:F1}x (Default: {defaultValue:F1}x)", 11, new Color(0.88f, 0.65f, 0.18f, 1f), TextAnchor.MiddleRight, FontStyle.Bold);
            var vRt = valTxt.GetComponent<RectTransform>();
            vRt.anchorMin = new Vector2(0.54f, 0.48f);
            vRt.anchorMax = new Vector2(0.92f, 1f);
            vRt.offsetMin = Vector2.zero;
            vRt.offsetMax = new Vector2(-4, -3);

            // Slider reference for direct value reset
            Slider slider = null;

            // Reset Button ("R" turns gold on hover)
            var resetBtn = UIFactory.CreateButton(card.transform, "ResetBtn", "R",
                new Color(0.20f, 0.23f, 0.30f, 1f),
                new Color(0.88f, 0.65f, 0.18f, 1f),
                new Color(0.70f, 0.50f, 0.12f, 1f),
                Color.white, () =>
            {
                onValueChanged?.Invoke(defaultValue);
                valTxt.text = $"{defaultValue:F1}x (Default: {defaultValue:F1}x)";
                if (slider != null) slider.value = defaultValue;
            }, 11);
            var rRt = resetBtn.GetComponent<RectTransform>();
            rRt.anchorMin = new Vector2(0.93f, 0.18f);
            rRt.anchorMax = new Vector2(0.985f, 0.82f);
            rRt.offsetMin = Vector2.zero;
            rRt.offsetMax = Vector2.zero;

            // Slider
            slider = UIFactory.CreateSlider(card.transform, "Slider", min, max, currentValue, (val) =>
            {
                valTxt.text = $"{val:F1}x (Default: {defaultValue:F1}x)";
                onValueChanged?.Invoke(val);
            });
            var sRt = slider.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.54f, 0.08f);
            sRt.anchorMax = new Vector2(0.92f, 0.48f);
            sRt.offsetMin = Vector2.zero;
            sRt.offsetMax = new Vector2(-4, 0);

            _createdSettingCards.Add(card);
        }

        private void CreateToggleCard(string section, string label, string desc, bool currentValue, Action<bool> onValueChanged)
        {
            var card = UIFactory.CreatePanel(_settingsContentRt, $"ToggleCard_{label}", new Color(0.13f, 0.15f, 0.20f, 1f), UIFactory.CardBoxSprite);
            var cardRt = card.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(0, 44);

            var le = card.AddComponent<LayoutElement>();
            le.minHeight = 44;
            le.preferredHeight = 44;
            le.flexibleWidth = 1;

            var meta = card.AddComponent<SettingCardMeta>();
            meta.Section = section;
            meta.SearchText = $"{label} {desc}".ToLowerInvariant();

            // Label & Description
            var lbl = UIFactory.CreateText(card.transform, "Label", label, 13, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            var lRt = lbl.GetComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0, 0.48f);
            lRt.anchorMax = new Vector2(0.82f, 1f);
            lRt.offsetMin = new Vector2(12, 0);
            lRt.offsetMax = new Vector2(0, -3);

            var dsc = UIFactory.CreateText(card.transform, "Desc", desc, 10, new Color(0.6f, 0.65f, 0.75f, 0.9f), TextAnchor.MiddleLeft);
            var dRt = dsc.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0, 0);
            dRt.anchorMax = new Vector2(0.82f, 0.48f);
            dRt.offsetMin = new Vector2(12, 3);
            dRt.offsetMax = Vector2.zero;

            // Toggle Switch
            var toggle = UIFactory.CreateToggle(card.transform, "Toggle", currentValue, (val) => onValueChanged?.Invoke(val));
            var tRt = toggle.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(1, 0.5f);
            tRt.anchorMax = new Vector2(1, 0.5f);
            tRt.anchoredPosition = new Vector2(-36, 0);

            _createdSettingCards.Add(card);
        }
    }
}
