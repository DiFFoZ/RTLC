using System;
using System.IO;
using RTLC.API;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RTLC.Textures;
internal static class ReplaceTextures
{
    [InitializeOnAwake]
    public static void LoadTextures()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // wait for ship loaded scene
        if (!scene.name.Equals("SampleSceneRelay", StringComparison.Ordinal))
        {
            return;
        }

        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;

        var assetBundle = AssetBundle.LoadFromFile(Path.Combine(RTLCPlugin.Instance.WorkingDirectory, "Bundles", "rtlc.main"));
        var mainTextureId = Shader.PropertyToID("_MainTex");

        foreach (var material in Resources.FindObjectsOfTypeAll<Material>())
        {
            if (!material.HasProperty(mainTextureId))
            {
                continue;
            }

            var texture = material.mainTexture as Texture2D;
            if (texture == null)
            {
                continue;
            }

            var name = texture.name;
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            var newTexture = assetBundle.LoadAsset<Texture2D>(name);
            if (newTexture == null)
            {
                continue;
            }

            RTLCPlugin.Instance.Logger.LogInfo($"Found {texture.name} to replace");

            material.mainTexture = newTexture;
            if (newTexture.isReadable)
            {
                newTexture.Apply(true, true);
            }
        }
    }
}
