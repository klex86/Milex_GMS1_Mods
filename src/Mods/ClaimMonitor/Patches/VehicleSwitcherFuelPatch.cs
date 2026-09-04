using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Milex.GMS1.Mods.ClaimMonitor.Patches
{
    /// <summary>
    /// Harmony patches for displaying vehicle fuel status badges directly inside the vanilla vehicle switcher UI.
    /// </summary>
    [HarmonyPatch]
    public static class VehicleSwitcherFuelPatch
    {
        private static Sprite _circleSprite;
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
            Image dotImage;
            Text fuelText;

            if (badgeTransform == null)
            {
                var badgeGo = new GameObject("ClaimMonitor_FuelBadge");
                badgeGo.transform.SetParent(panelItem.transform, false);

                var rect = badgeGo.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(1f, 0.5f);
                rect.anchorMax = new Vector2(1f, 0.5f);
                rect.pivot = new Vector2(1f, 0.5f);
                rect.anchoredPosition = new Vector2(-12f, 0f);
                rect.sizeDelta = new Vector2(64f, 22f);

                // 1. Dot Image
                var dotGo = new GameObject("Dot");
                dotGo.transform.SetParent(badgeGo.transform, false);
                var dotRect = dotGo.AddComponent<RectTransform>();
                dotRect.anchorMin = new Vector2(0f, 0.5f);
                dotRect.anchorMax = new Vector2(0f, 0.5f);
                dotRect.pivot = new Vector2(0f, 0.5f);
                dotRect.anchoredPosition = new Vector2(0f, 0f);
                dotRect.sizeDelta = new Vector2(10f, 10f);

                dotImage = dotGo.AddComponent<Image>();
                dotImage.sprite = GetOrCreateCircleSprite();

                // 2. Fuel Text
                var textGo = new GameObject("Text");
                textGo.transform.SetParent(badgeGo.transform, false);
                var textRect = textGo.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0f, 0f);
                textRect.anchorMax = new Vector2(1f, 1f);
                textRect.pivot = new Vector2(0f, 0.5f);
                textRect.anchoredPosition = new Vector2(14f, 0f);
                textRect.sizeDelta = new Vector2(50f, 22f);

                fuelText = textGo.AddComponent<Text>();
                var nameText = typeof(VehicleInPanel).GetField("vehicleName", Flags)?.GetValue(panelItem) as Text;
                if (nameText != null && nameText.font != null)
                    fuelText.font = nameText.font;
                else
                    fuelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

                fuelText.fontSize = 12;
                fuelText.fontStyle = FontStyle.Bold;
                fuelText.alignment = TextAnchor.MiddleLeft;
            }
            else
            {
                badgeTransform.gameObject.SetActive(true);
                dotImage = badgeTransform.Find("Dot")?.GetComponent<Image>();
                fuelText = badgeTransform.Find("Text")?.GetComponent<Text>();
            }

            if (dotImage != null)
            {
                dotImage.color = statusColor;
            }

            if (fuelText != null)
            {
                fuelText.text = $"{fuelPercent:F0}%";
                fuelText.color = statusColor;
            }
        }

        private static Sprite GetOrCreateCircleSprite()
        {
            if (_circleSprite != null) return _circleSprite;

            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            float r = size / 2f;
            Vector2 center = new Vector2(r, r);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    if (dist <= r - 1.5f)
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                    else if (dist <= r)
                    {
                        float alpha = Mathf.Clamp01(r - dist);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.Apply();
            _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return _circleSprite;
        }
    }
}
