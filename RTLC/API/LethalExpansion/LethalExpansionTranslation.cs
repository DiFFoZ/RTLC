#if DEBUG
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace RTLC.API.LethalExpansion;
internal static class LethalExpansionTranslation
{
    public static void TryAddScrapsToUntranslated()
    {
        if (!(Chainloader.PluginInfos.ContainsKey(KnownPluginDependency.c_LethalExpansion)
            || Chainloader.PluginInfos.ContainsKey(KnownPluginDependency.c_LethalExpansionCore)))
        {
            return;
        }

        try
        {
            var assetBundleManagerType = AccessTools.TypeByName("LethalExpansionCore.Utils.AssetBundlesManager");

            var assetBundleManager = AccessTools.PropertyGetter(assetBundleManagerType, "Instance").Invoke(null, null);
            var assetBundles = (IDictionary)AccessTools.Field(assetBundleManager.GetType(), "assetBundles").GetValue(assetBundleManager);

            // <string, (AssetBundle, ModManifest)>
            //                        ^^^^^^^^^^^ <- trying to find this
            foreach (DictionaryEntry entry in assetBundles)
            {
                var tuple = (ITuple)entry.Value;
                var modManifest = tuple[1];

                var scraps = (Array)AccessTools.Field(modManifest.GetType(), "scraps").GetValue(modManifest);
                foreach (var scrap in scraps)
                {
                    var itemName = (string)AccessTools.Field(scrap.GetType(), "itemName").GetValue(scrap);
                    _ = Translation.GetLocalizedText(itemName);
                }
            }
        }
        catch (Exception ex)
        {
            RTLCPlugin.Instance.Logger.LogError(ex);
        }
    }
}
#endif