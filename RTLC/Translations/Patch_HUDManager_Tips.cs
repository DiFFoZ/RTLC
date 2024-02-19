using HarmonyLib;

namespace RTLC.Translations;
[HarmonyPatch(typeof(HUDManager))]
internal static class Patch_HUDManager_Tips
{
    [HarmonyPatch(nameof(HUDManager.ChangeControlTip))]
    [HarmonyPrefix]
    public static void ChangeControlTip(ref string changeTo)
    {
        changeTo = Translation.GetLocalizedText(changeTo);
    }

    [HarmonyPatch(nameof(HUDManager.ChangeControlTipMultiple))]
    [HarmonyPrefix]
    public static void ChangeControlTipMultiple(string[] allLines)
    {
        for (var i = 0; i < allLines.Length; i++)
        {
            allLines[i] = Translation.GetLocalizedText(allLines[i]);
        }
    }

    [HarmonyPatch(nameof(HUDManager.DisplayTip))]
    [HarmonyPrefix]
    public static void DisplayTip(ref string headerText, ref string bodyText)
    {
        headerText = Translation.GetLocalizedText(headerText);
        bodyText = Translation.GetLocalizedText(bodyText);
    }
}
