using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using BepInEx;
using UnityEngine;

namespace Milex.GMS1.Mods.ClaimMonitor.Diagnostics
{
    /// <summary>
    /// Live runtime dumper that queries Unity's memory (Resources.FindObjectsOfTypeAll)
    /// to extract 100% authentic serialized prefabs and active scene values for all
    /// equipment, vehicles, wash plants, and hand tools into a clean, structured JSON format.
    /// </summary>
    public static class EquipmentBaselineDumper
    {
        private const BindingFlags AllFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        private static readonly HashSet<string> InterestingTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Hand Tools
            "Shovel", "Bucket", "GoldPan", "HogPanDirtBox", "HogPan", "MinersMoss", "VirtualVolumeDirt",
            // Processing & Wash Plants
            "WaveTable", "MagnetiteSeparator", "MagnetiteTrailer", "MobileWashplant", "MiniWashplant",
            "WashPlantTrommel", "WashPlantShaker", "WashPlantShakerBase", "WashPlantDuplex", "GravelPump",
            "GlacierCreek", "BigWashplant", "OrangeBeastWashPlantGoldCounter", "MatScrubber", "DeRocker",
            // Conveyors
            "ConveyorGround", "ConveyorElevator", "ConveyorBase", "ConveyorDirectDump",
            // Fuel & Infrastructure
            "FuelTankStationary", "MobileFuelTank", "FuelStationController", "FuelController",
            "FuelPistolHoldable", "ShovelRopeDestruction", "WaterStationController", "WaterTowerController", "WaterPumpController",
            // Vehicles & Heavy Equipment
            "DumpTruck", "Koparka", "Ladowarka", "BackhoeLoader", "Doozer", "Drill", "DiggingController"
        };

        public static string DumpBaselineToJson()
        {
            var allMono = Resources.FindObjectsOfTypeAll<MonoBehaviour>();

            var categorizedObjects = new Dictionary<string, List<object>>
            {
                ["HandTools"] = new List<object>(),
                ["ProcessingAndWashplants"] = new List<object>(),
                ["Conveyors"] = new List<object>(),
                ["FuelAndInfrastructure"] = new List<object>(),
                ["Vehicles"] = new List<object>(),
                ["Other"] = new List<object>()
            };

            var processedSet = new HashSet<int>();

            foreach (var comp in allMono)
            {
                if (comp == null) continue;

                int id = comp.GetInstanceID();
                if (processedSet.Contains(id)) continue;

                Type compType = comp.GetType();
                string typeName = compType.Name;

                if (!IsRelevantType(compType)) continue;

                processedSet.Add(id);

                var entry = ExtractComponentData(comp, compType);
                string category = Categorize(typeName);
                categorizedObjects[category].Add(entry);
            }

            string json = SerializeToJson(categorizedObjects);

            string dir = Path.Combine(Paths.PluginPath, "Milex_ClaimMonitor_Dumps");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = Path.Combine(dir, $"Equipment_Baseline_{timestamp}.json");
            File.WriteAllText(filePath, json, Encoding.UTF8);

            // Also keep an easily accessible "Latest" copy
            string latestPath = Path.Combine(dir, "Equipment_Baseline_Latest.json");
            try
            {
                File.WriteAllText(latestPath, json, Encoding.UTF8);
            }
            catch
            {
                // Best effort for convenience
            }

            return filePath;
        }

        private static bool IsRelevantType(Type type)
        {
            Type curr = type;
            while (curr != null && curr != typeof(MonoBehaviour) && curr != typeof(Component) && curr != typeof(object))
            {
                if (InterestingTypes.Contains(curr.Name))
                {
                    return true;
                }
                curr = curr.BaseType;
            }
            return false;
        }

        private static string Categorize(string typeName)
        {
            string lower = typeName.ToLowerInvariant();
            if (lower.Contains("shovel") || lower.Contains("bucket") || lower.Contains("goldpan") ||
                lower.Contains("hogpan") || lower.Contains("moss"))
                return "HandTools";

            if (lower.Contains("wash") || lower.Contains("separator") || lower.Contains("table") ||
                lower.Contains("trommel") || lower.Contains("shaker") || lower.Contains("duplex") ||
                lower.Contains("gravelpump") || lower.Contains("creek") || lower.Contains("scrubber") || lower.Contains("derocker"))
                return "ProcessingAndWashplants";

            if (lower.Contains("conveyor"))
                return "Conveyors";

            if (lower.Contains("fuel") || lower.Contains("water") || lower.Contains("pump") || lower.Contains("rope"))
                return "FuelAndInfrastructure";

            if (lower.Contains("truck") || lower.Contains("koparka") || lower.Contains("ladowarka") ||
                lower.Contains("backhoe") || lower.Contains("doozer") || lower.Contains("drill") || lower.Contains("digging"))
                return "Vehicles";

            return "Other";
        }

        private static Dictionary<string, object> ExtractComponentData(MonoBehaviour comp, Type compType)
        {
            var data = new Dictionary<string, object>
            {
                ["TypeName"] = compType.Name,
                ["FullTypeName"] = compType.FullName,
                ["GameObjectName"] = comp.gameObject.name,
                ["InstanceID"] = comp.GetInstanceID(),
                ["IsPrefab"] = !comp.gameObject.scene.IsValid() || string.IsNullOrEmpty(comp.gameObject.scene.name),
                ["SceneName"] = comp.gameObject.scene.IsValid() ? comp.gameObject.scene.name : "Prefab/AssetBundle",
                ["ActiveInHierarchy"] = comp.gameObject.activeInHierarchy
            };

            var fieldsDict = new Dictionary<string, object>();

            // Inspect type hierarchy
            Type t = compType;
            while (t != null && t != typeof(MonoBehaviour) && t != typeof(Component) && t != typeof(object))
            {
                FieldInfo[] fields = t.GetFields(AllFlags);
                foreach (var f in fields)
                {
                    if (f.IsStatic) continue;

                    string fName = f.Name;
                    bool isBalanceSheet = false;
                    foreach (var attr in f.GetCustomAttributes(true))
                    {
                        if (attr != null && attr.GetType().Name.IndexOf("BalanceSheet", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            isBalanceSheet = true;
                            break;
                        }
                    }

                    bool isTargetField = isBalanceSheet || IsKeyName(fName);

                    if (isTargetField && !fieldsDict.ContainsKey(fName))
                    {
                        try
                        {
                            object val = f.GetValue(comp);
                            fieldsDict[fName] = FormatValue(val);
                        }
                        catch (Exception ex)
                        {
                            fieldsDict[fName] = $"<Error: {ex.Message}>";
                        }
                    }
                }

                PropertyInfo[] props = t.GetProperties(AllFlags);
                foreach (var p in props)
                {
                    if (!p.CanRead || p.GetIndexParameters().Length > 0) continue;

                    string pName = p.Name;
                    bool isTargetProp = IsKeyName(pName);

                    if (isTargetProp && !fieldsDict.ContainsKey(pName))
                    {
                        try
                        {
                            object val = p.GetValue(comp, null);
                            fieldsDict[pName] = FormatValue(val);
                        }
                        catch
                        {
                            // Skip inaccessible properties
                        }
                    }
                }

                t = t.BaseType;
            }

            data["ExtractedProperties"] = fieldsDict;
            return data;
        }

        private static bool IsKeyName(string name)
        {
            string lower = name.ToLowerInvariant();
            return lower.Contains("volume") ||
                   lower.Contains("capacity") ||
                   lower.Contains("speed") ||
                   lower.Contains("rate") ||
                   lower.Contains("max") ||
                   lower.Contains("limit") ||
                   lower.Contains("depth") ||
                   lower.Contains("blade") ||
                   lower.Contains("torque") ||
                   lower.Contains("force") ||
                   lower.Contains("flow") ||
                   lower.Contains("consumption") ||
                   lower.Contains("fill");
        }

        private static object FormatValue(object val)
        {
            if (val == null) return "null";
            if (val is float f) return float.IsNaN(f) ? "NaN" : f;
            if (val is double d) return double.IsNaN(d) ? "NaN" : d;
            if (val is int || val is bool || val is long) return val;
            if (val is Vector3 v3) return $"({v3.x:F3}, {v3.y:F3}, {v3.z:F3})";
            if (val is BoxCollider box) return $"BoxCollider(Center: {box.center}, Size: {box.size})";
            return val.ToString();
        }

        private static string SerializeToJson(Dictionary<string, List<object>> categories)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"DumpGenerated\": \"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\",");
            sb.AppendLine($"  \"ActiveScene\": \"{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}\",");
            sb.AppendLine("  \"Categories\": {");

            int catIndex = 0;
            foreach (var kvp in categories)
            {
                sb.AppendLine($"    \"{Escape(kvp.Key)}\": [");
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    WriteObject(sb, kvp.Value[i] as Dictionary<string, object>, 6);
                    if (i < kvp.Value.Count - 1) sb.Append(",");
                    sb.AppendLine();
                }
                sb.Append("    ]");
                if (catIndex < categories.Count - 1) sb.Append(",");
                sb.AppendLine();
                catIndex++;
            }

            sb.AppendLine("  }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static void WriteObject(StringBuilder sb, Dictionary<string, object> dict, int indentLevel)
        {
            string indent = new string(' ', indentLevel);
            sb.AppendLine($"{indent}{{");

            int count = dict.Count;
            int idx = 0;

            foreach (var kvp in dict)
            {
                sb.Append($"{indent}  \"{Escape(kvp.Key)}\": ");
                WriteVal(sb, kvp.Value, indentLevel + 2);
                if (idx < count - 1) sb.Append(",");
                sb.AppendLine();
                idx++;
            }

            sb.Append($"{indent}}}");
        }

        private static void WriteVal(StringBuilder sb, object val, int indentLevel)
        {
            if (val == null)
            {
                sb.Append("null");
            }
            else if (val is bool b)
            {
                sb.Append(b ? "true" : "false");
            }
            else if (val is float f)
            {
                sb.Append(f.ToString("G", CultureInfo.InvariantCulture));
            }
            else if (val is double d)
            {
                sb.Append(d.ToString("G", CultureInfo.InvariantCulture));
            }
            else if (val is int || val is long)
            {
                sb.Append(val.ToString());
            }
            else if (val is Dictionary<string, object> nested)
            {
                WriteObject(sb, nested, indentLevel);
            }
            else
            {
                sb.Append($"\"{Escape(val.ToString())}\"");
            }
        }

        private static string Escape(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");
        }
    }
}
