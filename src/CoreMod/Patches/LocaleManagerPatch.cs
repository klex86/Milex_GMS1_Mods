using HarmonyLib;
using Milex.GMS1.Core.Localization;

namespace Milex_GMS1_CoreMod.Patches
{
    [HarmonyPatch(typeof(LocaleManager))]
    public static class LocaleManagerPatches
    {
        [HarmonyPatch(nameof(LocaleManager.LanguageChanged))]
        [HarmonyPostfix]
        public static void LanguageChanged_Postfix(int languageId)
        {
            try
            {
                if (Singleton<LocaleManager>.IsInstanced())
                {
                    var mgr = Singleton<LocaleManager>.Instance;
                    if (mgr.LanguagesShortNames != null && languageId >= 0 && languageId < mgr.LanguagesShortNames.Length)
                    {
                        string newLang = mgr.LanguagesShortNames[languageId];
                        LocalizationManager.NotifyLanguageChanged(newLang);
                    }
                }
            }
            catch (System.Exception ex)
            {
                CorePlugin.Logger?.LogError($"[LocaleManagerPatch] Fehler bei Sprachwechsel: {ex}");
            }
        }
    }
}