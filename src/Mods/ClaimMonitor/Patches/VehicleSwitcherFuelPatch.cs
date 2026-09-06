using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Milex.GMS1.Mods.ClaimMonitor.Patches
{
    /// <summary>
    /// Harmony patches for displaying vehicle fuel status badges directly inside the vanilla vehicle switcher UI.
    /// Renders a vertical status bar and percentage label directly in front of the vehicle row.
    /// </summary>
    [HarmonyPatch]
    public static class VehicleSwitcherFuelPatch
    {
        private static Sprite _barSprite;
        private static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

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
            if (plugin == null || !plugin.IsEnabled || plugin.MonitorConfig == null || !plugin.MonitorConfig.ShowFuelInVehicleSwitcher.Value)
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

            float fuelPercent = 0f;
            object fuelObj = typeof(MachineController).GetField("Fuel", Flags)?.GetValue(vehicle);
            if (fuelObj != null)
            {
                var fcType = fuelObj.GetType();
                var pctProp = fcType.GetProperty("TankPct", Flags);
                if (pctProp != null)
                {
                    float raw = (float)pctProp.GetValue(fuelObj, null);
                    fuelPercent = raw <= 1.0f ? raw * 100f : raw;
                }
                else
                {
                    float cur = (float)(fcType.GetField("FuelCurrentCapacity", Flags)?.GetValue(fuelObj) 
                                     ?? fcType.GetField("CurrentCapacity", Flags)?.GetValue(fuelObj) ?? 0f);
                    float max = (float)(fcType.GetField("FuelMaxCapacity", Flags)?.GetValue(fuelObj) 
                                     ?? fcType.GetField("MaxCapacity", Flags)?.GetValue(fuelObj) ?? 0f);
                    fuelPercent = max > 0.001f ? Mathf.Clamp01(cur / max) * 100f : 0f;
                }
            }

            // Status color grading:
            // Green: >= 50%
            // Yellow: 25% - 49.9%
            // Orange: 15% - 24.9%
            // Red: < 15%
            Color statusColor;
            if (fuelPercent >= 50f)
                statusColor = new Color(0.18f, 0.80f, 0.44f, 1f); // Green (#2ECC71)
            else if (fuelPercent >= 25f)
                statusColor = new Color(0.95f, 0.77f, 0.06f, 1f); // Yellow (#F1C40F)
            else if (fuelPercent >= 15f)
                statusColor = new Color(0.90f, 0.49f, 0.13f, 1f); // Orange (#E67E22)
            else
                statusColor = new Color(0.91f, 0.30f, 0.24f, 1f); // Red (#E74C3C)

            Transform badgeTransform = panelItem.transform.Find("ClaimMonitor_FuelBadge");
            Image barImage;
            Text fuelText;

            if (badgeTransform == null)
            {
                // Container positioned to the LEFT of the vehicle card (in front of the yellow selection area)
                var badgeGo = new GameObject("ClaimMonitor_FuelBadge");
                badgeGo.transform.SetParent(panelItem.transform, false);

                var rect = badgeGo.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(1f, 0.5f);
                rect.anchoredPosition = new Vector2(-6f, 0f);
                rect.sizeDelta = new Vector2(70f, 0f);

                // 1. Vertical Status Bar (in front of vehicle selection area)
                var barGo = new GameObject("Bar");
                barGo.transform.SetParent(badgeGo.transform, false);
                var barRect = barGo.AddComponent<RectTransform>();
                barRect.anchorMin = new Vector2(1f, 0.12f);
                barRect.anchorMax = new Vector2(1f, 0.88f);
                barRect.pivot = new Vector2(1f, 0.5f);
                barRect.anchoredPosition = new Vector2(0f, 0f);
                barRect.sizeDelta = new Vector2(6f, 0f);

                barImage = barGo.AddComponent<Image>();
                barImage.sprite = GetOrCreateBarSprite();

                // 2. Fuel Text directly to the left of the vertical bar
                var textGo = new GameObject("Text");
                textGo.transform.SetParent(badgeGo.transform, false);
                var textRect = textGo.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0f, 0f);
                textRect.anchorMax = new Vector2(1f, 1f);
                textRect.pivot = new Vector2(1f, 0.5f);
                textRect.anchoredPosition = new Vector2(-10f, 0f);
                textRect.sizeDelta = new Vector2(55f, 0f);

                fuelText = textGo.AddComponent<Text>();
                var nameText = typeof(VehicleInPanel).GetField("vehicleName", Flags)?.GetValue(panelItem) as Text;
                if (nameText != null && nameText.font != null)
                    fuelText.font = nameText.font;
                else
                    fuelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

                fuelText.fontSize = 13;
                fuelText.fontStyle = FontStyle.Bold;
                fuelText.alignment = TextAnchor.MiddleRight;
            }
            else
            {
                badgeTransform.gameObject.SetActive(true);
                barImage = badgeTransform.Find("Bar")?.GetComponent<Image>();
                fuelText = badgeTransform.Find("Text")?.GetComponent<Text>();
            }

            if (barImage != null)
            {
                barImage.color = statusColor;
            }

            if (fuelText != null)
            {
                fuelText.text = $"{fuelPercent:F0}%";
                fuelText.color = statusColor;
            }
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
                    // Slightly rounded top and bottom corners
                    bool isCorner = (x == 0 || x == w - 1) && (y == 0 || y == h - 1);
                    tex.SetPixel(x, y, isCorner ? new Color(1f, 1f, 1f, 0.3f) : Color.white);
                }
            }

            tex.Apply();
            _barSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
            return _barSprite;
        }
    }
}
