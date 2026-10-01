using System.Reflection;
using GoldDigger;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Milex.GMS1.Mods.ClaimMonitor.Patches
{
    /// <summary>
    /// Harmony patches for displaying vehicle fuel status badges directly inside the vanilla vehicle switcher UI.
    /// Renders vertical status bar(s) and percentage label(s) directly in front of the vehicle row.
    /// Supports dual-tank architectures (Drive + Belt) for Frankenstein and Cordylus.
    /// </summary>
    [HarmonyPatch]
    public static class VehicleSwitcherFuelPatch
    {
        private static Sprite _barSprite;
        private static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private static Texture2D _bgTexture;

        [HarmonyPatch(typeof(VehicleInPanel), "UpdateRare")]
        [HarmonyPostfix]
        public static void UpdateRare_Postfix(VehicleInPanel __instance)
        {
            UpdateFuelDisplay(__instance);
        }

        [HarmonyPatch(typeof(VehicleInPanel), "SetVehicleName")]
        [HarmonyPostfix]
        public static void SetVehicleName_Postfix(VehicleInPanel __instance)
        {
            UpdateFuelDisplay(__instance);
        }

        public static void UpdateFuelDisplay(VehicleInPanel panelItem)
        {
            if (panelItem == null) return;

            var plugin = ClaimMonitorPlugin.Instance;
            var config = plugin?.MonitorConfig;
            if (plugin == null || !plugin.IsEnabled || config == null || !config.ShowFuelInVehicleSwitcher.Value)
            {
                var existing = panelItem.transform.Find("ClaimMonitor_FuelBadge");
                if (existing != null && existing.gameObject.activeSelf)
                {
                    existing.gameObject.SetActive(false);
                }
                return;
            }

            var vehicleField = typeof(VehicleInPanel).GetField("vehicle", Flags);
            var vehicle = vehicleField?.GetValue(panelItem) as MachineController;
            if (vehicle == null) return;

            // 1. Primary Tank (Drive)
            float drivePct = 0f;
            object fuelObj = typeof(MachineController).GetField("Fuel", Flags)?.GetValue(vehicle);
            if (fuelObj != null)
            {
                var fcType = fuelObj.GetType();
                var pctProp = fcType.GetProperty("TankPct", Flags);
                if (pctProp != null)
                {
                    float raw = (float)pctProp.GetValue(fuelObj, null);
                    drivePct = raw <= 1.0f ? raw * 100f : raw;
                }
                else
                {
                    float cur = (float)(fcType.GetField("FuelCurrentCapacity", Flags)?.GetValue(fuelObj)
                                     ?? fcType.GetField("CurrentCapacity", Flags)?.GetValue(fuelObj) ?? 0f);
                    float max = (float)(fcType.GetField("FuelMaxCapacity", Flags)?.GetValue(fuelObj)
                                     ?? fcType.GetField("MaxCapacity", Flags)?.GetValue(fuelObj) ?? 0f);
                    drivePct = max > 0.001f ? Mathf.Clamp01(cur / max) * 100f : 0f;
                }
            }

            // 2. Secondary Tank (Belt: Frankenstein / Cordylus)
            bool hasDualTanks = false;
            float beltPct = 0f;

            var beltComp = vehicle.GetComponentInChildren<FrankensteinBelt>(true);
            if (beltComp != null && beltComp.MyFuel != null)
            {
                hasDualTanks = true;
                float bCur = beltComp.MyFuel.CurrentCapacity;
                float bMax = beltComp.MyFuel.MaxCapacity;
                beltPct = bMax > 0.001f ? Mathf.Clamp01(bCur / bMax) * 100f : 0f;
            }

            float lowThreshold = config.VehicleLowFuelThreshold?.Value ?? 20.0f;
            Color driveColor = GetFuelStatusColor(drivePct, lowThreshold);
            Color beltColor = GetFuelStatusColor(beltPct, lowThreshold);

            Transform badgeTransform = panelItem.transform.Find("ClaimMonitor_FuelBadge");
            RectTransform badgeRect;
            Image barDriveImage;
            Image barBeltImage;
            Text driveText;
            Text beltText;

            // Breiten: Single Tank 95px, Dual Tank 175px
            float panelWidth = hasDualTanks ? 175f : 95f;

            if (badgeTransform == null)
            {
                var badgeGo = new GameObject("ClaimMonitor_FuelBadge");
                badgeGo.transform.SetParent(panelItem.transform, false);

                badgeRect = badgeGo.AddComponent<RectTransform>();
                // Leichtes Padding nach oben/unten (0.05 bis 0.95), um mit der Vanilla-Kachel bündig abzuschließen
                badgeRect.anchorMin = new Vector2(1f, 0.05f);
                badgeRect.anchorMax = new Vector2(1f, 0.95f);
                badgeRect.pivot = new Vector2(0f, 0.5f);
                badgeRect.anchoredPosition = new Vector2(5f, 0f);
                badgeRect.sizeDelta = new Vector2(panelWidth, 0f);

                var bgImage = badgeGo.AddComponent<Image>();
                bgImage.sprite = GetOrCreateBackgroundSprite();
                bgImage.type = Image.Type.Sliced;
                bgImage.color = new Color(0.03f, 0.03f, 0.04f, 0.88f);

                var nameText = typeof(VehicleInPanel).GetField("vehicleName", Flags)?.GetValue(panelItem) as Text;
                Font uiFont = (nameText != null && nameText.font != null)
                    ? nameText.font
                    : Resources.GetBuiltinResource<Font>("Arial.ttf");

                // ==================== ZELLE 1: Drive Bar ====================
                var barDGo = new GameObject("BarDrive");
                barDGo.transform.SetParent(badgeGo.transform, false);
                var barDRect = barDGo.AddComponent<RectTransform>();
                barDRect.anchorMin = new Vector2(0f, 0.12f);
                barDRect.anchorMax = new Vector2(0f, 0.88f);
                barDRect.pivot = new Vector2(0f, 0.5f);
                barDRect.anchoredPosition = new Vector2(7f, 0f);
                barDRect.sizeDelta = new Vector2(5f, 0f);

                barDriveImage = barDGo.AddComponent<Image>();
                barDriveImage.sprite = GetOrCreateBarSprite();

                // ==================== ZELLE 2: Drive % Text ====================
                var textDGo = new GameObject("TextDrive");
                textDGo.transform.SetParent(badgeGo.transform, false);
                var textDRect = textDGo.AddComponent<RectTransform>();
                textDRect.anchorMin = new Vector2(0f, 0f);
                textDRect.anchorMax = new Vector2(0f, 1f);
                textDRect.pivot = new Vector2(0f, 0.5f);
                textDRect.anchoredPosition = new Vector2(16f, 0f);
                textDRect.sizeDelta = new Vector2(68f, 0f);

                driveText = textDGo.AddComponent<Text>();
                driveText.font = uiFont;
                driveText.fontSize = 19;
                driveText.resizeTextForBestFit = true;
                driveText.resizeTextMinSize = 14;
                driveText.resizeTextMaxSize = 20;
                driveText.fontStyle = FontStyle.Bold;
                driveText.alignment = TextAnchor.MiddleLeft;
                driveText.horizontalOverflow = HorizontalWrapMode.Overflow;
                driveText.verticalOverflow = VerticalWrapMode.Overflow;

                // ==================== ZELLE 3: Belt Bar ====================
                var barBGo = new GameObject("BarBelt");
                barBGo.transform.SetParent(badgeGo.transform, false);
                var barBRect = barBGo.AddComponent<RectTransform>();
                barBRect.anchorMin = new Vector2(0f, 0.12f);
                barBRect.anchorMax = new Vector2(0f, 0.88f);
                barBRect.pivot = new Vector2(0f, 0.5f);
                barBRect.anchoredPosition = new Vector2(92f, 0f);
                barBRect.sizeDelta = new Vector2(5f, 0f);

                barBeltImage = barBGo.AddComponent<Image>();
                barBeltImage.sprite = GetOrCreateBarSprite();

                // ==================== ZELLE 4: Belt % Text ====================
                var textBGo = new GameObject("TextBelt");
                textBGo.transform.SetParent(badgeGo.transform, false);
                var textBRect = textBGo.AddComponent<RectTransform>();
                textBRect.anchorMin = new Vector2(0f, 0f);
                textBRect.anchorMax = new Vector2(0f, 1f);
                textBRect.pivot = new Vector2(0f, 0.5f);
                textBRect.anchoredPosition = new Vector2(101f, 0f);
                textBRect.sizeDelta = new Vector2(68f, 0f);

                beltText = textBGo.AddComponent<Text>();
                beltText.font = uiFont;
                beltText.fontSize = 19;
                beltText.resizeTextForBestFit = true;
                beltText.resizeTextMinSize = 14;
                beltText.resizeTextMaxSize = 20;
                beltText.fontStyle = FontStyle.Bold;
                beltText.alignment = TextAnchor.MiddleLeft;
                beltText.horizontalOverflow = HorizontalWrapMode.Overflow;
                beltText.verticalOverflow = VerticalWrapMode.Overflow;
            }
            else
            {
                badgeTransform.gameObject.SetActive(true);
                badgeRect = badgeTransform.GetComponent<RectTransform>();
                barDriveImage = badgeTransform.Find("BarDrive")?.GetComponent<Image>();
                driveText = badgeTransform.Find("TextDrive")?.GetComponent<Text>();
                barBeltImage = badgeTransform.Find("BarBelt")?.GetComponent<Image>();
                beltText = badgeTransform.Find("TextBelt")?.GetComponent<Text>();

                // Auch bei bereits existierenden Instanzen BestFit und Font-Größe sicherstellen
                if (driveText != null)
                {
                    driveText.fontSize = 19;
                    driveText.resizeTextForBestFit = true;
                    driveText.resizeTextMinSize = 14;
                    driveText.resizeTextMaxSize = 20;
                }
                if (beltText != null)
                {
                    beltText.fontSize = 19;
                    beltText.resizeTextForBestFit = true;
                    beltText.resizeTextMinSize = 14;
                    beltText.resizeTextMaxSize = 20;
                }
            }

            // Dynamische Kachelbreite
            if (badgeRect != null)
                badgeRect.sizeDelta = new Vector2(panelWidth, 0f);

            // Drive-Zellen füllen
            if (barDriveImage != null)
                barDriveImage.color = driveColor;

            if (driveText != null)
            {
                driveText.text = $"{drivePct:F0}%";
                driveText.color = driveColor;
            }

            // Belt-Zellen füllen (nur bei Frankenstein & Cordylus aktiv)
            if (barBeltImage != null)
            {
                barBeltImage.gameObject.SetActive(hasDualTanks);
                if (hasDualTanks)
                    barBeltImage.color = beltColor;
            }

            if (beltText != null)
            {
                beltText.gameObject.SetActive(hasDualTanks);
                if (hasDualTanks)
                {
                    beltText.text = $"{beltPct:F0}%";
                    beltText.color = beltColor;
                }
            }
        }

        private static Sprite GetOrCreateBackgroundSprite()
        {
            if (_bgTexture != null)
                return Sprite.Create(_bgTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));

            _bgTexture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            _bgTexture.hideFlags = HideFlags.HideAndDontSave;
            Color[] pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            _bgTexture.SetPixels(pixels);
            _bgTexture.Apply();

            return Sprite.Create(_bgTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
        }

        private static Color GetFuelStatusColor(float percent, float lowThreshold)
        {
            if (percent >= 50f)
                return new Color(0.18f, 0.80f, 0.44f, 1f); // Grün (#2ECC71)
            if (percent >= lowThreshold)
                return new Color(0.95f, 0.77f, 0.06f, 1f); // Gelb (#F1C40F)
            if (percent >= (lowThreshold * 0.5f))
                return new Color(0.90f, 0.49f, 0.13f, 1f); // Orange (#E67E22)

            return new Color(0.91f, 0.30f, 0.24f, 1f); // Rot (#E74C3C)
        }

        private static Sprite GetOrCreateBarSprite()
        {
            if (_barSprite != null) return _barSprite;

            int w = 8;
            int h = 32;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isCorner = (x == 0 || x == w - 1) && (y == 0 || y == h - 1);
                    tex.SetPixel(x, y, isCorner ? new Color(1f, 1f, 1f, 0.3f) : Color.white);
                }
            }

            tex.Apply();
            _barSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
            return _barSprite;
        }

        public static void HideAllBadges()
        {
            try
            {
                var panels = Object.FindObjectsOfType<VehicleInPanel>();
                if (panels == null) return;
                foreach (var panel in panels)
                {
                    if (panel == null) continue;
                    var existing = panel.transform.Find("ClaimMonitor_FuelBadge");
                    if (existing != null)
                    {
                        existing.gameObject.SetActive(false);
                    }
                }
            }
            catch { }
        }
    }
}