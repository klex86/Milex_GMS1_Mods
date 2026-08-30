using BepInEx;
using Milex.GMS1.Core;
using Milex.GMS1.Mods.ProductionTuner.Config;
using Milex.GMS1.Mods.ProductionTuner.Services;

namespace Milex.GMS1.Mods.ProductionTuner
{
    /// <summary>
    /// Production Tuner – Stellt Regler fuer Verarbeitungsgeschwindigkeiten, Kapazitaeten
    /// und Hydrauliktempo fuer alle Komponenten, Fahrzeuge und Werkzeuge bereit.
    ///
    /// In Phase 1 werden Konfiguration, Domänenlogik und Lokalisierung vollstaendig aufgebaut.
    /// Harmony-Patches werden in Phase 2 nach Dekompilierung der Spiel-DLLs hinzugefuegt.
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CorePlugin.PluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public class ProductionTunerPlugin : ModBase
    {
        public const string PluginGuid    = "com.milex.gms1.productiontuner";
        public const string PluginName    = "Milex GMS1 Production Tuner";
        public const string PluginVersion = "1.0.0";

        public override string ModGuid    => PluginGuid;
        public override string ModName    => PluginName;
        public override string ModVersion => PluginVersion;

        /// <summary>
        /// Globale Instanz – wird von Harmony-Patch-Klassen in Phase 2 genutzt,
        /// um Multiplikatoren aus dem TuningService abzufragen.
        /// </summary>
        public static ProductionTunerPlugin Instance { get; private set; }

        /// <summary>Zugriff auf alle Einstellungen des Mods.</summary>
        public TuningConfig TuningConfig { get; private set; }

        /// <summary>Zugriff auf den Domänen-Service (Multiplikator-Berechnung).</summary>
        public static TuningService Service { get; private set; }

        protected override void Awake()
        {
            Instance = this;

            // Schicht 1: Einstellungen laden / erstellen
            TuningConfig = new TuningConfig(Config);

            // Schicht 2: Domänen-Service initialisieren
            Service = new TuningService(TuningConfig);

            // Basis-Initialisierung (Harmony, ModRegistry, Lokalisierung)
            base.Awake();

            LogInfo(Translate("log.ready", "Production Tuner loaded. Open the Mod Menu to adjust multipliers."));
        }

        /// <summary>
        /// Wird aufgerufen, wenn der Mod ueber das Mod-Menue im laufenden Spiel reaktiviert wird.
        /// </summary>
        protected override void OnModEnabled()
        {
            LogInfo(Translate("log.enabled", "Production Tuner enabled."));
        }

        /// <summary>
        /// Wird aufgerufen, wenn der Mod ueber das Mod-Menue im laufenden Spiel deaktiviert wird.
        /// Alle Harmony-Patches werden dabei automatisch entfernt – das Spiel laeuft mit
        /// seinen Original-Werten weiter.
        /// </summary>
        protected override void OnModDisabled()
        {
            LogInfo(Translate("log.disabled", "Production Tuner disabled. Original game values restored."));
        }
    }
}
