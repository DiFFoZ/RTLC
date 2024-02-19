using HarmonyLib;

namespace RTLC.Translations;
[HarmonyPatch(typeof(StartOfRound), "Awake")]
[HarmonyPriority(Priority.Last)]
internal static class Patch_StartOfRound_Awake
{
    [HarmonyPrefix]
    public static void TranslateItems(StartOfRound __instance)
    {
        foreach (var item in __instance.allItemsList.itemsList)
        {
            item.itemName = Translation.GetLocalizedText(item.itemName);
        }
    }
}
