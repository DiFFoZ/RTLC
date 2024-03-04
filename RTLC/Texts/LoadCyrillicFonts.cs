using System;
using System.IO;
using HarmonyLib;
using RTLC.API;
using TMPro;
using UnityEngine;

namespace RTLC.Texts;

[HarmonyPatch]
internal static class LoadCyrillicFonts
{
    private static TMP_FontAsset? s_TransmitFontAsset;

    [InitializeOnAwake]
    public static void LoadFonts()
    {
        var assetBundle = AssetBundle.LoadFromFile(Path.Combine(RTLCPlugin.Instance.WorkingDirectory, "Bundles", "rtlc.font"));

        var mainFont = assetBundle.LoadAsset<TMP_FontAsset>("Assets/3270_font.asset");
        s_TransmitFontAsset = assetBundle.LoadAsset<TMP_FontAsset>("Assets/edunline_font.asset");

        if (mainFont == null)
        {
            return;
        }

        TMP_Settings.fallbackFontAssets.Add(mainFont);
    }

    [HarmonyPatch(typeof(TMP_FontAsset), "Awake")]
    [HarmonyPrefix]
    private static void LoadFontAsset(TMP_FontAsset __instance)
    {
        if (__instance.name.StartsWith("edunline"))
        {
            __instance.fallbackFontAssetTable.Add(s_TransmitFontAsset);
        }

        __instance.atlasPopulationMode = AtlasPopulationMode.Static;
    }
}
