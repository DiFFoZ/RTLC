using RTLC.API;
using SmartFormat.ZString;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace RTLC.Texts;
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

        using var builder = new ZStringBuilder(false);

        foreach (var ui in Object.FindObjectsOfType<UIBehaviour>(true))
        {
            if (ui is TextMeshProUGUI text)
            {
                SetCharArrayFast(text, builder);
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

    public static void SetCharArrayFast(TextMeshProUGUI text, ZStringBuilder builder)
    {
        builder.Clear();
        builder.Append(Translation.GetLocalizedText(text.text));

        var array = builder.AsArraySegment();
        text.SetCharArray(array.Array, array.Offset, array.Count);
    }
}
