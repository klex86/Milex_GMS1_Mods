using System;
using HarmonyLib;
using Milex.GMS1.Core;
using Milex.GMS1.Core.Localization;

namespace Milex.GMS1.Core.Patches
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
            catch (Exception ex)
            {
                CorePlugin.Instance?.LogError($"[LocaleManagerPatch] Fehler bei Sprachwechsel: {ex}");
            }
        }
    }
}