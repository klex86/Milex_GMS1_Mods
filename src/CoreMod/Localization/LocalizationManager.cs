using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;


namespace Milex.GMS1.Core.Localization
{
    /// <summary>
    /// Information on a supported language with its ISO code and native endonym.
    /// </summary>
    public class LanguageDefinition
    {
        public string Code { get; }
        public string NativeName { get; }

        public LanguageDefinition(string code, string nativeName)
        {
            Code = code;
            NativeName = nativeName;
        }

        public override string ToString() => $"{NativeName} ({Code})";
    }

    /// <summary>
    /// Central multi-language localization manager.
    /// Handles disk extraction of embedded EN/DE templates,
    /// dynamic translation fallback cascade, on-demand template creation dialogs,
    /// and language change detection.
    /// </summary>
    public static class LocalizationManager
    {
        public const string LocalizationFolderName = "Milex GMS1 Mod Localization";

        /// <summary>
        /// Supported languages with their native endonyms.
        /// </summary>
        public static readonly LanguageDefinition[] SupportedLanguages = new[]
        {
            new LanguageDefinition("en", "English"),
            new LanguageDefinition("de", "Deutsch"),
            new LanguageDefinition("fr", "Français"),
            new LanguageDefinition("es", "Español"),
            new LanguageDefinition("pl", "Polski"),
            new LanguageDefinition("ru", "Русский"),
            new LanguageDefinition("it", "Italiano"),
            new LanguageDefinition("pt", "Português"),
            new LanguageDefinition("tr", "Türkçe"),
            new LanguageDefinition("nl", "Nederlands"),
            new LanguageDefinition("sv", "Svenska"),
            new LanguageDefinition("da", "Dansk"),
            new LanguageDefinition("no", "Norsk"),
            new LanguageDefinition("ro", "Română"),
            new LanguageDefinition("cs", "Čeština"),
            new LanguageDefinition("bg", "Български"),
            new LanguageDefinition("el", "Ελληνικά"),
            new LanguageDefinition("ja", "日本語"),
            new LanguageDefinition("ko", "한국어"),
            new LanguageDefinition("zh-CN", "简体中文"),
            new LanguageDefinition("zh-TD", "繁體中文")
        };

        private static string _localizationDirPath;
        private static readonly Dictionary<string, Assembly> _registeredMods = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
        // [ModName][LangCode][Key] = TranslatedText
        private static readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> _translations 
            = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>(StringComparer.OrdinalIgnoreCase);

        private static readonly object _lock = new object();

        public static event Action<string> OnLanguageChanged;

        public static string LocalizationDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_localizationDirPath))
                {
                    _localizationDirPath = Path.Combine(Paths.PluginPath, LocalizationFolderName);
                    if (!Directory.Exists(_localizationDirPath))
                    {
                        Directory.CreateDirectory(_localizationDirPath);
                    }
                }
                return _localizationDirPath;
            }
        }

        public static string CurrentLanguage
        {
            get
            {
                if (CorePlugin.UseGameLanguage != null && CorePlugin.UseGameLanguage.Value)
                {
                    return GetGameLanguage();
                }

                return CorePlugin.SelectedLanguage != null ? CorePlugin.SelectedLanguage.Value : "en";
            }
        }

        public static string GetLanguageNativeName(string code)
        {
            var def = SupportedLanguages.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            return def != null ? def.NativeName : code;
        }

        public static string GetGameLanguage()
        {
            string[] shortNames = new string[]
            {
                "en", "pl", "de", "fr", "es", "ru", "it", "pt", "zh-CN", "zh-TD",
                "ja", "ko", "nl", "tr", "no", "cs", "ro", "da", "bg", "el", "sv"
            };

            try
            {
                // 1. Wenn LocaleManager instanziiert ist
                if (Singleton<LocaleManager>.IsInstanced())
                {
                    var localeMgr = Singleton<LocaleManager>.Instance;
                    int langId = localeMgr.GetLanguageId();

                    if (langId >= 0 && langId < shortNames.Length)
                    {
                        return shortNames[langId];
                    }
                }

                // 2. Fallback OS-Sprache
                return UnityEngine.Application.systemLanguage switch
                {
                    UnityEngine.SystemLanguage.German => "de",
                    UnityEngine.SystemLanguage.Polish => "pl",
                    UnityEngine.SystemLanguage.French => "fr",
                    UnityEngine.SystemLanguage.Spanish => "es",
                    UnityEngine.SystemLanguage.Russian => "ru",
                    _ => "en"
                };
            }
            catch
            {
                return "en";
            }
        
        }

        public static void RegisterMod(string modName, Assembly assembly)
        {
            if (string.IsNullOrEmpty(modName) || assembly == null) return;

            lock (_lock)
            {
                _registeredMods[modName] = assembly;

                // Ensure disk templates for EN and DE exist
                EnsureDiskTemplate(modName, "en", assembly);
                EnsureDiskTemplate(modName, "de", assembly);

                // Pre-load English default
                LoadTranslationsForMod(modName, "en", assembly);
                string currentLang = CurrentLanguage;
                if (!currentLang.Equals("en", StringComparison.OrdinalIgnoreCase))
                {
                    LoadTranslationsForMod(modName, currentLang, assembly);
                }
            }
        }

        /// <summary>
        /// Checks which registered mods are missing a translation file or embedded resource for the given target language.
        /// </summary>
        public static List<string> GetModsMissingLanguage(string targetLang)
        {
            var missing = new List<string>();
            if (targetLang.Equals("en", StringComparison.OrdinalIgnoreCase)) return missing; // English is base

            lock (_lock)
            {
                foreach (var kvp in _registeredMods)
                {
                    string modName = kvp.Key;
                    Assembly assembly = kvp.Value;

                    // 1. Check disk
                    string diskFile = Path.Combine(LocalizationDirectory, $"{modName}_{targetLang}.json");
                    if (File.Exists(diskFile)) continue;

                    // 2. Check embedded
                    string embeddedRes = FindResourceName(assembly, modName, targetLang);
                    if (!string.IsNullOrEmpty(embeddedRes)) continue;

                    missing.Add(modName);
                }
            }
            return missing;
        }

        /// <summary>
        /// Generates template JSON files for missing mods with instructions and NexusMods submission notice.
        /// </summary>
        public static void GenerateTemplatesForMods(List<string> modNames, string targetLang)
        {
            lock (_lock)
            {
                foreach (string modName in modNames)
                {
                    if (!_registeredMods.TryGetValue(modName, out Assembly assembly)) continue;

                    string targetFile = Path.Combine(LocalizationDirectory, $"{modName}_{targetLang}.json");
                    if (File.Exists(targetFile)) continue;

                    // Get English base translations
                    string enResource = FindResourceName(assembly, modName, "en");
                    string enDisk = Path.Combine(LocalizationDirectory, $"{modName}_en.json");

                    var baseEntries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    if (File.Exists(enDisk))
                    {
                        ParseFlatJson(File.ReadAllText(enDisk, Encoding.UTF8), baseEntries);
                    }
                    else if (!string.IsNullOrEmpty(enResource))
                    {
                        using (var s = assembly.GetManifestResourceStream(enResource))
                        using (var r = new StreamReader(s, Encoding.UTF8))
                        {
                            ParseFlatJson(r.ReadToEnd(), baseEntries);
                        }
                    }

                    // Build formatted JSON with header comments
                    var sb = new StringBuilder();
                    sb.AppendLine("{");
                    sb.AppendLine($"  \"_instructions_1\": \"=== TRANSLATION TEMPLATE FOR: {modName} ({targetLang.ToUpper()}) ===\",");
                    sb.AppendLine("  \"_instructions_2\": \"1. Translate the values on the right side into your language.\",");
                    sb.AppendLine("  \"_instructions_3\": \"2. Keep the keys on the left side exactly as they are.\",");
                    sb.AppendLine($"  \"_instructions_4\": \"3. Share your translation by posting it in the NexusMods page for '{modName}' so we can include it officially!\",");
                    sb.AppendLine("  \"_instructions_5\": \"===================================================\",");
                    sb.AppendLine();

                    int count = 0;
                    var keys = baseEntries.Keys.ToList();
                    for (int i = 0; i < keys.Count; i++)
                    {
                        string k = keys[i];
                        if (k.StartsWith("_instructions_")) continue;

                        string val = baseEntries[k];
                        string escapedKey = EscapeJson(k);
                        string escapedVal = EscapeJson(val);

                        count++;
                        bool isLast = (i == keys.Count - 1);
                        sb.AppendLine($"  \"{escapedKey}\": \"{escapedVal}\"{(isLast ? "" : ",")}");
                    }

                    sb.AppendLine("}");

                    try
                    {
                        File.WriteAllText(targetFile, sb.ToString(), Encoding.UTF8);
                        // Reload in memory
                        LoadTranslationsForMod(modName, targetLang, assembly);
                    }
                    catch (Exception ex)
                    {
                        CorePlugin.Instance?.LogError($"Failed to write template for {modName}_{targetLang}: {ex.Message}");
                    }
                }
            }
        }

        public static void ReloadAll()
        {
            lock (_lock)
            {
                _translations.Clear();
                foreach (var kvp in _registeredMods)
                {
                    LoadTranslationsForMod(kvp.Key, "en", kvp.Value);
                    string currentLang = CurrentLanguage;
                    if (!currentLang.Equals("en", StringComparison.OrdinalIgnoreCase))
                    {
                        LoadTranslationsForMod(kvp.Key, currentLang, kvp.Value);
                    }
                }
            }
            OnLanguageChanged?.Invoke(CurrentLanguage);
        }

        public static void NotifyLanguageChanged(string newLang)
        {
            lock (_lock)
            {
                foreach (var kvp in _registeredMods)
                {
                    LoadTranslationsForMod(kvp.Key, newLang, kvp.Value);
                }
            }
            OnLanguageChanged?.Invoke(newLang);
        }

        public static string Translate(string modName, string key, string defaultValue = null)
        {
            if (string.IsNullOrEmpty(modName) || string.IsNullOrEmpty(key))
            {
                return defaultValue ?? key ?? string.Empty;
            }

            string lang = CurrentLanguage;

            lock (_lock)
            {
                if (!_translations.ContainsKey(modName))
                {
                    if (_registeredMods.TryGetValue(modName, out var asm))
                    {
                        LoadTranslationsForMod(modName, lang, asm);
                    }
                }

                if (_translations.TryGetValue(modName, out var langDict))
                {
                    // 1. Active language
                    if (langDict.TryGetValue(lang, out var keyDict) && keyDict.TryGetValue(key, out string translated))
                    {
                        return translated;
                    }

                    // Try dynamic load
                    if (_registeredMods.TryGetValue(modName, out var asm))
                    {
                        LoadTranslationsForMod(modName, lang, asm);
                        if (langDict.TryGetValue(lang, out var newlyLoaded) && newlyLoaded.TryGetValue(key, out string newlyTranslated))
                        {
                            return newlyTranslated;
                        }
                    }

                    // 2. English fallback
                    if (langDict.TryGetValue("en", out var enDict) && enDict.TryGetValue(key, out string enText))
                    {
                        return enText;
                    }
                }
            }

            return defaultValue ?? key;
        }

        private static void EnsureDiskTemplate(string modName, string langCode, Assembly assembly)
        {
            try
            {
                string targetFileName = $"{modName}_{langCode}.json";
                string targetFilePath = Path.Combine(LocalizationDirectory, targetFileName);

                if (!File.Exists(targetFilePath))
                {
                    string resourceName = FindResourceName(assembly, modName, langCode);
                    if (!string.IsNullOrEmpty(resourceName))
                    {
                        using (var stream = assembly.GetManifestResourceStream(resourceName))
                        {
                            if (stream != null)
                            {
                                using (var reader = new StreamReader(stream, Encoding.UTF8))
                                {
                                    string content = reader.ReadToEnd();
                                    File.WriteAllText(targetFilePath, content, Encoding.UTF8);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CorePlugin.Instance?.LogWarning($"[Localization] Failed to write template for {modName}_{langCode}: {ex.Message}");
            }
        }

        private static void LoadTranslationsForMod(string modName, string langCode, Assembly assembly)
        {
            if (!_translations.TryGetValue(modName, out var langDict))
            {
                langDict = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
                _translations[modName] = langDict;
            }

            if (langDict.ContainsKey(langCode)) return;

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            langDict[langCode] = dict;

            bool ignoreExternal = CorePlugin.IgnoreExternalTranslations != null && CorePlugin.IgnoreExternalTranslations.Value;

            // 1. Disk (skipped if IgnoreExternalTranslations is true)
            if (!ignoreExternal)
            {
                string diskFile = Path.Combine(LocalizationDirectory, $"{modName}_{langCode}.json");
                if (File.Exists(diskFile))
                {
                    try
                    {
                        string json = File.ReadAllText(diskFile, Encoding.UTF8);
                        ParseFlatJson(json, dict);
                        return;
                    }
                    catch (Exception ex)
                    {
                        CorePlugin.Instance?.LogWarning($"[Localization] Error reading {diskFile}: {ex.Message}");
                    }
                }
            }

            // 2. Embedded resource
            if (assembly != null)
            {
                string resourceName = FindResourceName(assembly, modName, langCode);
                if (!string.IsNullOrEmpty(resourceName))
                {
                    try
                    {
                        using (var stream = assembly.GetManifestResourceStream(resourceName))
                        {
                            if (stream != null)
                            {
                                using (var reader = new StreamReader(stream, Encoding.UTF8))
                                {
                                    string json = reader.ReadToEnd();
                                    ParseFlatJson(json, dict);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        CorePlugin.Instance?.LogWarning($"[Localization] Error reading embedded resource {resourceName}: {ex.Message}");
                    }
                }
            }
        }

        private static string FindResourceName(Assembly assembly, string modName, string langCode)
        {
            string[] names = assembly.GetManifestResourceNames();
            string suffixA = $"{modName}_{langCode}.json";
            string suffixB = $"{langCode}.json";

            foreach (var n in names)
            {
                if (n.EndsWith(suffixA, StringComparison.OrdinalIgnoreCase)) return n;
            }
            foreach (var n in names)
            {
                if (n.EndsWith(suffixB, StringComparison.OrdinalIgnoreCase)) return n;
            }
            return null;
        }

        private static void ParseFlatJson(string json, Dictionary<string, string> targetDict)
        {
            if (string.IsNullOrEmpty(json) || targetDict == null) return;

            var matches = Regex.Matches(json, @"""((?:\\.|[^""\\])*)""\s*:\s*""((?:\\.|[^""\\])*)""");
            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    string key = Unescape(match.Groups[1].Value);
                    string val = Unescape(match.Groups[2].Value);
                    targetDict[key] = val;
                }
            }
        }

        private static string Unescape(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return Regex.Unescape(str);
        }

        private static string EscapeJson(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");
        }
    }
}
