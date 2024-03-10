using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using RTLC.API;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RTLC.Textures;
[HarmonyPatch]
internal static class ReplaceTexturesHandler
{
    private static AssetBundle s_AssetBundle = null!;
    private static readonly HashSet<int> s_UniqueTextures = [];

    [InitializeOnAwake]
    public static void LoadTextures()
    {
        s_AssetBundle = AssetBundle.LoadFromFile(Path.Combine(RTLCPlugin.Instance.WorkingDirectory, "Bundles", "rtlc.main"));

        if (s_AssetBundle == null)
        {
            return;
        }
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void StartOfRoundLoaded()
    {
        ReplaceTextures();
    }

    private static void ReplaceTextures()
    {
        foreach (var texture in Resources.FindObjectsOfTypeAll<Texture2D>())
        {
            var name = texture.name;
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            if (!s_UniqueTextures.Add(texture.GetInstanceID()))
            {
                continue;
            }

            var newTexture = s_AssetBundle.LoadAsset<Texture2D>(name);
            if (newTexture == null)
            {
                continue;
            }

            texture.LoadImage(newTexture.EncodeToPNG());
        }
    }
}
