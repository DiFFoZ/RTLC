using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using RTLC.API;
using TMPro;
using UnityEngine;

namespace RTLC.Fonts;

[HarmonyPatch]
internal static class LoadCyrillicFonts
{
    [InitializeOnAwake]
    public static void LoadFonts()
    {
        var assetBundle = AssetBundle.LoadFromFile(Path.Combine(RTLCPlugin.Instance.WorkingDirectory, "Bundles", "rtlc.font"));

        var mainFont = assetBundle.LoadAsset<TMP_FontAsset>("3270_font");
        var transmitFont = assetBundle.LoadAsset<TMP_FontAsset>("eduline_font");

        if (mainFont == null || transmitFont == null)
        {
            return;
        }

        TMP_Settings.fallbackFontAssets.Add(mainFont);

        foreach (var font in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
        {
            if (font.name.Equals("eduline_font"))
            {
                font.fallbackFontAssetTable.Add(transmitFont);
                return;
            }
        }
    }

    [HarmonyPatch(typeof(TMP_FontAsset), "Awake")]
    [HarmonyPrefix]
    private static void LoadFontAsset(TMP_FontAsset __instance)
    {
        __instance.atlasPopulationMode = AtlasPopulationMode.Static;
    }
}
