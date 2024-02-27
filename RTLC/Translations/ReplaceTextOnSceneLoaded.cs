using RTLC.API;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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

        foreach (var ui in Object.FindObjectsOfType<UIBehaviour>(true))
        {
            if (ui is TextMeshProUGUI text)
            {
                text.text = Translation.GetLocalizedText(text.text);
                continue;
            }

            if (ui is TMP_Dropdown dropdown && dropdown.options != null)
            {
                foreach (var option in dropdown.options)
                {
                    option.text = Translation.GetLocalizedText(option.text);
                }
            }
        }
    }
}
