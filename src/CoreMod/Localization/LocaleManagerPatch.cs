using HarmonyLib;
using Milex_GMS1_CoreMod.Localization;

namespace Milex_GMS1_CoreMod.Patches
{
    [HarmonyPatch(typeof(LocaleManager))]
    public static class LocaleManagerPatches
    {
        [HarmonyPatch(nameof(LocaleManager.LanguageChanged))]
        [HarmonyPostfix]
        public static void LanguageChanged_Postfix(int languageId)
        {
            if (Singleton<LocaleManager>.IsInstanced())
            {
                var mgr = Singleton<LocaleManager>.Instance;
                if (mgr.LanguagesShortNames != null && languageId >= 0 && languageId < mgr.LanguagesShortNames.Length)
                {
                    string iso = mgr.LanguagesShortNames[languageId];
                    LocalizationManager.SetLanguage(iso);
                }
            }
        }
    }
}