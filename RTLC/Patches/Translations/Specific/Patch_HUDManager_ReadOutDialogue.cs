using HarmonyLib;

namespace RTLC.Patches.Translations.Specific;
[HarmonyPatch(typeof(HUDManager))]
internal static class Patch_HUDManager_ReadOutDialogue
{
    [HarmonyPatch(nameof(HUDManager.ReadDialogue))]
    [HarmonyPrefix]
    public static void ReadDialogue(DialogueSegment[] dialogueArray)
    {
        if (dialogueArray == null)
        {
            return;
        }

        foreach (var dialogue in dialogueArray)
        {
            if (dialogue == null)
            {
                return;
            }

            dialogue.bodyText = Translation.GetLocalizedText(dialogue.bodyText);
            dialogue.speakerText = Translation.GetLocalizedText(dialogue.speakerText);
        }
    }
}
