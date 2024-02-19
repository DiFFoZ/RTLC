using System.Collections.Generic;
using RTLC.API;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RTLC.Translations;
internal static class ReplaceTextOnSceneLoaded
{
    [InitializeOnAwake]
    private static void Init()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var name = scene.name;
        if (!(name.Equals("InitScene")
            || name.Equals("InitSceneLaunchOptions")
            || name.Equals("MainMenu")
            || name.Equals("SampleSceneRelay")))
        {
            return;
        }

        foreach (var text in Object.FindObjectsOfType<TextMeshProUGUI>(true))
        {
            text.text = Translation.GetLocalizedText(text.text);
        }
    }
}
