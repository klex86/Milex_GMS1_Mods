using BepInEx;
using BepInEx.Configuration;
using Milex.GMS1.Core;
using UnityEngine;

namespace Milex.GMS1.Mods.HelloMod
{
    /// <summary>
    /// Proof-of-concept mod demonstrating Milex GMS1 CoreMod integration,
    /// config persistence, localization support, and customizable hotkeys.
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CorePlugin.PluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public class HelloModPlugin : ModBase
    {
        public const string PluginGuid = "com.milex.gms1.hellomod";
        public const string PluginName = "HelloMod";
        public const string PluginVersion = "1.1.0";

        public override string ModGuid => PluginGuid;
        public override string ModName => PluginName;
        public override string ModVersion => PluginVersion;

        private ConfigEntry<KeyCode> _testHotkey;
        private ConfigEntry<bool> _showPrefix;
        private ConfigEntry<string> _customGreeting;

        protected override void Awake()
        {
            // Bind customizable settings
            _testHotkey = Config.Bind("Controls", "TestHotkey", KeyCode.F2, "Key to trigger hello test log");
            _showPrefix = Config.Bind("General", "ShowPrefix", true, "Determines whether [Greeting] prefix is shown");
            _customGreeting = Config.Bind("General", "GreetingMessage", "Hello from Gold Mining Simulator Modding!", "Custom greeting message");

            base.Awake();

            string instruction = string.Format(Translate("log.instruction", "Press {0} to trigger test log output (or rebind via Core Mod Menu on Insert)."), _testHotkey.Value);
            LogInfo(instruction);
        }

        private void Update()
        {
            if (Input.GetKeyDown(_testHotkey.Value))
            {
                string greeting = !string.IsNullOrEmpty(_customGreeting.Value) 
                    ? _customGreeting.Value 
                    : Translate("log.greeting_default", "Hello from Gold Mining Simulator Modding!");

                string msg = _showPrefix.Value 
                    ? $"[Greeting] {greeting} ({_testHotkey.Value} Key Pressed)" 
                    : $"{greeting} ({_testHotkey.Value} Key Pressed)";

                LogInfo(msg);
            }
        }
    }
}
