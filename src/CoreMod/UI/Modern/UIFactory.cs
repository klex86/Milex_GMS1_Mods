using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Milex.GMS1.Core.UI.Modern
{
    /// <summary>
    /// Procedural UI Factory creating stylized uGUI elements (Sprites, Buttons, Toggles, Sliders, Cards, ScrollViews)
    /// purely via C# code with zero external assets.
    /// </summary>
    public static class UIFactory
    {
        private static Font _defaultFont;
        private static Sprite _roundedBoxSprite;
        private static Sprite _flatSprite;
        private static Sprite _pillSprite;
        private static Sprite _circleSprite;

        public static Font DefaultFont
        {
            get
            {
                if (_defaultFont == null)
                {
                    try
                    {
                        _defaultFont = Font.CreateDynamicFontFromOSFont(new string[] { "Segoe UI", "Arial", "Calibri", "Liberation Sans", "Tahoma" }, 14);
                    }
                    catch { }

                    if (_defaultFont == null)
                    {
                        _defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    }

                    if (_defaultFont == null)
                    {
                        var fonts = Resources.FindObjectsOfTypeAll<Font>();
                        if (fonts != null && fonts.Length > 0)
                        {
                            _defaultFont = fonts[0];
                        }
                    }
                }
                return _defaultFont;
            }
        }

        public static Sprite FlatSprite
        {
            get
            {
                if (_flatSprite == null)
                {
                    var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    Color[] colors = new Color[] { Color.white, Color.white, Color.white, Color.white };
                    tex.SetPixels(colors);
                    tex.Apply();
                    _flatSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
                }
                return _flatSprite;
            }
        }

        public static Sprite RoundedBoxSprite
        {
            get
            {
                if (_roundedBoxSprite == null)
                {
                    _roundedBoxSprite = CreateRoundedRectSprite(64, 64, 12, 16);
                }
                return _roundedBoxSprite;
            }
        }

        public static Sprite PillSprite
        {
            get
            {
                if (_pillSprite == null)
                {
                    _pillSprite = CreateRoundedRectSprite(64, 32, 16, 16);
                }
                return _pillSprite;
            }
        }

        public static Sprite CircleSprite
        {
            get
            {
                if (_circleSprite == null)
                {
                    _circleSprite = CreateCircleSprite(32);
                }
                return _circleSprite;
            }
        }

        public static Sprite CreateRoundedRectSprite(int width, int height, int radius, int border)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] colors = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = 0;
                    float dy = 0;

                    if (x < radius) dx = radius - x;
                    else if (x >= width - radius) dx = x - (width - radius - 1);

                    if (y < radius) dy = radius - y;
                    else if (y >= height - radius) dy = y - (height - radius - 1);

                    float distSq = dx * dx + dy * dy;
                    float radSq = radius * radius;

                    if (distSq <= radSq)
                    {
                        // Soft antialiasing on outer radius
                        float dist = Mathf.Sqrt(distSq);
                        float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                        colors[y * width + x] = new Color(1f, 1f, 1f, alpha);
                    }
                    else
                    {
                        colors[y * width + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        public static Sprite CreateCircleSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            float radius = (size - 1) / 2f;
            Vector2 center = new Vector2(radius, radius);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                    colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        // ---- Element Creators ----

        public static GameObject CreatePanel(Transform parent, string name, Color color, Sprite sprite = null, Image.Type imageType = Image.Type.Sliced)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            img.sprite = sprite ?? FlatSprite;
            img.type = imageType;
            img.color = color;
            img.raycastTarget = true;

            return go;
        }

        public static Text CreateText(Transform parent, string name, string text, int fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleLeft, FontStyle fontStyle = FontStyle.Normal)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            var txt = go.GetComponent<Text>();
            txt.font = DefaultFont;
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = alignment;
            txt.fontStyle = fontStyle;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            txt.raycastTarget = false;

            return txt;
        }

        public static Button CreateButton(Transform parent, string name, string label, Color normalColor, Color hoverColor, Color clickColor, Color textColor, UnityAction onClick, int fontSize = 14)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            img.sprite = RoundedBoxSprite;
            img.type = Image.Type.Sliced;
            img.color = normalColor;

            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = hoverColor;
            colors.pressedColor = clickColor;
            colors.disabledColor = new Color(normalColor.r, normalColor.g, normalColor.b, 0.3f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            btn.colors = colors;

            if (onClick != null)
            {
                btn.onClick.AddListener(onClick);
            }

            if (!string.IsNullOrEmpty(label))
            {
                var txt = CreateText(go.transform, "Label", label, fontSize, textColor, TextAnchor.MiddleCenter, FontStyle.Bold);
                var rt = txt.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(4, 2);
                rt.offsetMax = new Vector2(-4, -2);
            }

            return btn;
        }

        public static Slider CreateSlider(Transform parent, string name, float min, float max, float value, UnityAction<float> onValueChanged)
        {
            var sliderObj = new GameObject(name, typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(parent, false);

            var slider = sliderObj.GetComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;

            var sliderRt = sliderObj.GetComponent<RectTransform>();
            sliderRt.sizeDelta = new Vector2(240, 24);

            // Background Track
            var bgObj = CreatePanel(sliderObj.transform, "Background", new Color(0.18f, 0.20f, 0.25f, 1f), RoundedBoxSprite);
            var bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0.35f);
            bgRt.anchorMax = new Vector2(1, 0.65f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            // Fill Area
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            var fillAreaRt = fillArea.GetComponent<RectTransform>();
            fillAreaRt.anchorMin = new Vector2(0, 0.35f);
            fillAreaRt.anchorMax = new Vector2(1, 0.65f);
            fillAreaRt.offsetMin = new Vector2(4, 0);
            fillAreaRt.offsetMax = new Vector2(-4, 0);

            var fillObj = CreatePanel(fillArea.transform, "Fill", new Color(0.88f, 0.65f, 0.18f, 1f), RoundedBoxSprite);
            var fillRt = fillObj.GetComponent<RectTransform>();
            fillRt.sizeDelta = Vector2.zero;
            slider.fillRect = fillRt;

            // Handle Slide Area
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderObj.transform, false);
            var handleAreaRt = handleArea.GetComponent<RectTransform>();
            handleAreaRt.anchorMin = Vector2.zero;
            handleAreaRt.anchorMax = Vector2.one;
            handleAreaRt.offsetMin = new Vector2(10, 0);
            handleAreaRt.offsetMax = new Vector2(-10, 0);

            var handleObj = CreatePanel(handleArea.transform, "Handle", new Color(0.95f, 0.95f, 0.95f, 1f), CircleSprite, Image.Type.Simple);
            var handleRt = handleObj.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(18, 18);
            slider.handleRect = handleRt;
            slider.targetGraphic = handleObj.GetComponent<Image>();

            if (onValueChanged != null)
            {
                slider.onValueChanged.AddListener(onValueChanged);
            }

            return slider;
        }

        public static Toggle CreateToggle(Transform parent, string name, bool isOn, UnityAction<bool> onValueChanged)
        {
            var toggleObj = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            toggleObj.transform.SetParent(parent, false);

            var toggle = toggleObj.GetComponent<Toggle>();
            var toggleRt = toggleObj.GetComponent<RectTransform>();
            toggleRt.sizeDelta = new Vector2(50, 26);

            // Background Pill
            var bgObj = CreatePanel(toggleObj.transform, "Background", isOn ? new Color(0.20f, 0.68f, 0.38f, 1f) : new Color(0.25f, 0.28f, 0.35f, 1f), PillSprite);
            var bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bgObj.GetComponent<Image>();

            // Handle Dot
            var handleObj = CreatePanel(toggleObj.transform, "Checkmark", Color.white, CircleSprite, Image.Type.Simple);
            var handleRt = handleObj.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(20, 20);
            handleRt.anchorMin = new Vector2(0, 0.5f);
            handleRt.anchorMax = new Vector2(0, 0.5f);
            handleRt.anchoredPosition = isOn ? new Vector2(37, 0) : new Vector2(13, 0);

            toggle.targetGraphic = bgImg;
            toggle.graphic = handleObj.GetComponent<Image>();
            toggle.isOn = isOn;

            toggle.onValueChanged.AddListener(val =>
            {
                bgImg.color = val ? new Color(0.20f, 0.68f, 0.38f, 1f) : new Color(0.25f, 0.28f, 0.35f, 1f);
                handleRt.anchoredPosition = val ? new Vector2(37, 0) : new Vector2(13, 0);
                onValueChanged?.Invoke(val);
            });

            return toggle;
        }

        public static (GameObject scrollRoot, RectTransform contentRt, ScrollRect scrollRect) CreateScrollView(Transform parent, string name)
        {
            var scrollRoot = new GameObject(name, typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollRoot.transform.SetParent(parent, false);

            var rootImg = scrollRoot.GetComponent<Image>();
            rootImg.color = Color.clear; // Transparent background

            var scrollRect = scrollRoot.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 35f;

            // Viewport
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
            viewport.transform.SetParent(scrollRoot.transform, false);
            var vpRt = viewport.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = Vector2.zero;

            var vpImg = viewport.GetComponent<Image>();
            vpImg.color = Color.white;
            var mask = viewport.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content
            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 8f;
            vlg.padding = new RectOffset(6, 6, 6, 6);

            var csf = content.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            scrollRect.viewport = vpRt;
            scrollRect.content = contentRt;

            return (scrollRoot, contentRt, scrollRect);
        }

        public static InputField CreateInputField(Transform parent, string name, string placeholderText, UnityAction<string> onValueChanged)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            img.sprite = RoundedBoxSprite;
            img.type = Image.Type.Sliced;
            img.color = new Color(0.12f, 0.14f, 0.18f, 1f);

            var input = go.GetComponent<InputField>();

            // Text
            var textObj = CreateText(go.transform, "Text", "", 14, Color.white, TextAnchor.MiddleLeft);
            var textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10, 4);
            textRt.offsetMax = new Vector2(-10, -4);

            // Placeholder
            var placeholderObj = CreateText(go.transform, "Placeholder", placeholderText, 14, new Color(0.5f, 0.55f, 0.65f, 0.8f), TextAnchor.MiddleLeft, FontStyle.Italic);
            var phRt = placeholderObj.GetComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = new Vector2(10, 4);
            phRt.offsetMax = new Vector2(-10, -4);

            input.textComponent = textObj;
            input.placeholder = placeholderObj;

            if (onValueChanged != null)
            {
                input.onValueChanged.AddListener(onValueChanged);
            }

            return input;
        }
    }
}
