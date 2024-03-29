﻿using System.IO;
using HarmonyLib;
using RTLC.API;
using TMPro;
using UnityEngine;

namespace RTLC.Texts;

[HarmonyPatch]
internal static class LoadCyrillicFonts
{
    private static TMP_FontAsset? s_TransmitFontAsset;
    private static TMP_FontAsset? s_MainFontAsset;
    private static TMP_FontAsset? s_MainBigFontAsset;
    private static Shader s_FontShader = null!;

    // prevents crash due to StackOverflow
    private static bool s_IgnoreAwake;

    [InitializeOnAwake]
    public static void LoadFonts()
    {
        var assetBundle = AssetBundle.LoadFromFile(Path.Combine(RTLCPlugin.Instance.WorkingDirectory, "Bundles", "rtlcfont.assetbundle"));

        s_MainFontAsset = assetBundle.LoadAsset<TMP_FontAsset>("Assets/Fonts/3270-Regular.asset");
        s_MainBigFontAsset = assetBundle.LoadAsset<TMP_FontAsset>("Assets/Fonts/3270-Big.asset");
        s_TransmitFontAsset = assetBundle.LoadAsset<TMP_FontAsset>("Assets/Fonts/3716-font.asset");
        s_FontShader = assetBundle.LoadAsset<Shader>("Assets/TextMesh Pro/Shaders/TMP_SDF SSD.shader");

        TMP_Settings.fallbackFontAssets.Add(s_MainFontAsset);
    }

    [HarmonyPatch(typeof(TMP_FontAsset), "Awake")]
    [HarmonyPrefix]
    private static void LoadFontAsset(TMP_FontAsset __instance)
    {
        if (s_IgnoreAwake)
        {
            return;
        }

        __instance.atlasPopulationMode = AtlasPopulationMode.Static;

        if (__instance.name.StartsWith("edunline"))
        {
            __instance.fallbackFontAssetTable.Add(CreateVariant(__instance, s_TransmitFontAsset!));
            return;
        }

        __instance.fallbackFontAssetTable.Add(CreateVariant(__instance, s_MainFontAsset!));
    }

    private static TMP_FontAsset CreateVariant(TMP_FontAsset original, TMP_FontAsset dst)
    {
        if (dst == s_MainFontAsset && original.creationSettings.pointSize > 110)
        {
#pragma warning disable Harmony003 // Harmony non-ref patch parameters modified
            dst = s_MainBigFontAsset!;
#pragma warning restore Harmony003 // Harmony non-ref patch parameters modified
        }

        var material = new Material(s_FontShader);
        material.CopyMatchingPropertiesFromMaterial(original.material);
        material.shaderKeywords = original.material.shaderKeywords;

        material.mainTexture = dst.atlasTexture;
        material.SetFloat("_TextureWidth", dst.atlasWidth);
        material.SetFloat("_TextureHeight", dst.atlasHeight);

        s_IgnoreAwake = true;

#pragma warning disable Harmony003 // Harmony non-ref patch parameters modified
        dst = Object.Instantiate(dst);
#pragma warning restore Harmony003 // Harmony non-ref patch parameters modified
        dst.material = material;

        s_IgnoreAwake = false;

        return dst;
    }
}
