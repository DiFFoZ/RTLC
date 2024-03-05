using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace RTLC.Translations.Specific;
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
        if (allLines == null)
            return;

        for (var i = 0; i < allLines.Length; i++)
        {
            allLines[i] = Translation.GetLocalizedText(allLines[i]);
        }
    }

    [HarmonyPatch(nameof(HUDManager.ChangeControlTipMultiple))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> RemoveItemNameInDrop(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        var itemNameField = AccessTools.Field(typeof(Item), nameof(Item.itemName));

        matcher.SearchForward(c => c.LoadsField(itemNameField))
            .SetOperandAndAdvance(AccessTools.Field(typeof(string), nameof(string.Empty)));

        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch(nameof(HUDManager.DisplayTip))]
    [HarmonyPrefix]
    public static void DisplayTip(ref string headerText, ref string bodyText)
    {
        headerText = Translation.GetLocalizedText(headerText);
        bodyText = Translation.GetLocalizedText(bodyText);
    }
}
