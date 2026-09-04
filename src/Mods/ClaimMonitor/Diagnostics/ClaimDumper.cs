using System;
using System.IO;
using System.Reflection;
using System.Text;
using BepInEx;
using UnityEngine;

namespace Milex.GMS1.Mods.ClaimMonitor.Diagnostics
{
    public static class ClaimDumper
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static string DumpClaimToFile()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"================================================================================");
            sb.AppendLine($"Milex Claim Monitor - Deep Object Dump");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"================================================================================");
            sb.AppendLine();

            var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            int objectCount = 0;

            foreach (var root in rootObjects)
            {
                var components = root.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var comp in components)
                {
                    if (comp == null) continue;
                    var type = comp.GetType();

                    if (!IsCandidateComponent(type.Name))
                        continue;

                    objectCount++;
                    DumpComponentHierarchy(sb, comp, type);
                }
            }

            sb.AppendLine($"Total Dumped Components: {objectCount}");

            string dir = Path.Combine(Paths.PluginPath, "Milex_ClaimMonitor_Dumps");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string filePath = Path.Combine(dir, $"Dump_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            File.WriteAllText(filePath, sb.ToString());

            return filePath;
        }

        private static bool IsCandidateComponent(string name)
        {
            return name.Contains("Moss") ||
                   name.Contains("Sluice") ||
                   name.Contains("Wash") ||
                   name.Contains("Trommel") ||
                   name.Contains("Shaker") ||
                   name.Contains("Conveyor") ||
                   name.Contains("Fuel") ||
                   name.Contains("Trailer") ||
                   name.Contains("Power") ||
                   name.Contains("Water") ||
                   name.Contains("Koparka") ||
                   name.Contains("Ladowarka") ||
                   name.Contains("DumpTruck") ||
                   name.Contains("Doozer") ||
                   name.Contains("Drill") ||
                   name.Contains("Pump");
        }

        private static void DumpComponentHierarchy(StringBuilder sb, MonoBehaviour comp, Type initialType)
        {
            var go = comp.gameObject;
            sb.AppendLine($"--------------------------------------------------------------------------------");
            sb.AppendLine($"[OBJECT] '{go.name}' (ID: {go.GetInstanceID()}) | Position: {go.transform.position}");
            sb.AppendLine($"[COMPONENT] {initialType.FullName}");

            Type currentType = initialType;
            while (currentType != null && currentType != typeof(MonoBehaviour) && currentType != typeof(Component) && currentType != typeof(object))
            {
                sb.AppendLine($"  >>> Type Level: {currentType.Name}");

                // Fields
                var fields = currentType.GetFields(Flags | BindingFlags.DeclaredOnly);
                if (fields.Length > 0)
                {
                    sb.AppendLine("    -- Fields --");
                    foreach (var f in fields)
                    {
                        string valStr = "null";
                        try
                        {
                            var val = f.GetValue(comp);
                            valStr = FormatValue(val);
                        }
                        catch (Exception ex)
                        {
                            valStr = $"<Error: {ex.Message}>";
                        }
                        sb.AppendLine($"      {f.Name} ({f.FieldType.Name}) = {valStr}");
                    }
                }

                // Properties
                var props = currentType.GetProperties(Flags | BindingFlags.DeclaredOnly);
                if (props.Length > 0)
                {
                    sb.AppendLine("    -- Properties --");
                    foreach (var p in props)
                    {
                        if (!p.CanRead || p.GetIndexParameters().Length > 0) continue;
                        string valStr = "null";
                        try
                        {
                            var val = p.GetValue(comp, null);
                            valStr = FormatValue(val);
                        }
                        catch (Exception ex)
                        {
                            valStr = $"<Error: {ex.Message}>";
                        }
                        sb.AppendLine($"      {p.Name} ({p.PropertyType.Name}) = {valStr}");
                    }
                }

                currentType = currentType.BaseType;
            }

            sb.AppendLine();
        }

        private static string FormatValue(object val)
        {
            if (val == null) return "null";
            if (val is string s) return $"\"{s}\"";
            if (val is bool b) return b ? "true" : "false";
            if (val is float f) return f.ToString("F3");
            if (val is double d) return d.ToString("F3");
            if (val is Vector3 v) return $"({v.x:F1}, {v.y:F1}, {v.z:F1})";
            return val.ToString();
        }
    }
}