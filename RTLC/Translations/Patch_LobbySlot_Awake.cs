using HarmonyLib;
using TMPro;

namespace RTLC.Translations;
[HarmonyPatch(typeof(LobbySlot), "Awake")]
internal static class Patch_LobbySlot_Awake
{
    [HarmonyPrefix]
    public static void ReplaceText(LobbySlot __instance)
    {
        var button = __instance.transform.Find("JoinButton");
        if (button == null)
        {
            return;
        }

        var text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = Translation.GetLocalizedText(text.text);
        }
    }
}
