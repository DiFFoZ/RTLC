using System;
using System.IO;
using RTLC.API;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RTLC.Textures;
internal static class ReplaceTextures
{
    private static AssetBundle s_AssetBundle = null!;

    [InitializeOnAwake]
    public static void LoadTextures()
    {
        s_AssetBundle = AssetBundle.LoadFromFile(Path.Combine(RTLCPlugin.Instance.WorkingDirectory, "Bundles", "rtlc.main"));

        if (s_AssetBundle == null)
        {
            return;
        }

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // wait for ship loaded scene
        if (!scene.name.Equals("SampleSceneRelay", StringComparison.Ordinal))
        {
            return;
        }

        foreach (var texture in Resources.FindObjectsOfTypeAll<Texture2D>())
        {
            var name = texture.name;
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            var newTexture = s_AssetBundle.LoadAsset<Texture2D>(name);
            if (newTexture == null)
            {
                continue;
            }

            RTLCPlugin.Instance.Logger.LogInfo($"Found {name} to replace");

            texture.LoadImage(newTexture.EncodeToPNG());
        }
    }
}
