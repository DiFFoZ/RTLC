using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace RTLC.Translations;
[HarmonyPatch(typeof(MenuManager), nameof(MenuManager.DisplayMenuNotification))]
internal static class Patch_MenuManager_DisplayMenuNotification
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ReplaceText(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        matcher.SearchForward(c => c.opcode == OpCodes.Brfalse || c.opcode == OpCodes.Brfalse_S);
        var label = (Label)matcher.Operand;

        matcher.SearchForward(c => c.labels.Contains(label));

        var @ref = string.Empty;

        matcher.Insert(new CodeInstruction(OpCodes.Ldarga_S, 0),
            new(OpCodes.Ldarga_S, 1),
            CodeInstruction.Call(() => ReplaceDisplayText(ref @ref, ref @ref)));

        return matcher.InstructionEnumeration();
    }

    public static void ReplaceDisplayText(ref string notificationText, ref string buttonText)
    {
        notificationText = Translation.GetLocalizedText(notificationText);
        buttonText = Translation.GetLocalizedText(buttonText);
    }
}
