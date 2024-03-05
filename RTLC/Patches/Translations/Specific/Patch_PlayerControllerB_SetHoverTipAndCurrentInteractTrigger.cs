using System.Collections.Generic;
using System.Reflection.Emit;
using GameNetcodeStuff;
using HarmonyLib;
using TMPro;

namespace RTLC.Translations.Specific;
[HarmonyPatch(typeof(PlayerControllerB), "SetHoverTipAndCurrentInteractTrigger")]
internal static class Patch_PlayerControllerB_SetHoverTipAndCurrentInteractTrigger
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ReplaceText(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        var cursorTipField = AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.cursorTip));
        var getTextProperty = AccessTools.PropertyGetter(typeof(TMP_Text), nameof(TMP_Text.text));

        matcher.MatchForward(false, new CodeMatch(c => c.LoadsField(cursorTipField)))
            .Repeat(m =>
            {
                var nextInstruction = m.InstructionAt(1);
                if (nextInstruction.opcode == OpCodes.Ldstr)
                {
                    nextInstruction.operand = Translation.GetLocalizedText((string)nextInstruction.operand);
                    m.Advance(1);
                    return;
                }

                if (nextInstruction.Calls(getTextProperty)
                    || nextInstruction.opcode == OpCodes.Ldloc || nextInstruction.opcode == OpCodes.Ldloc_S)
                {
                    m.Advance(1);
                    return;
                }

                var instructionCheckItself = m.InstructionAt(2);
                if (instructionCheckItself.LoadsField(cursorTipField))
                {
                    m.Advance(4);
                    return;
                }

                m.Advance(3)
                .Insert(CodeInstruction.Call(() => Translation.GetLocalizedText(string.Empty)));
            });

        return matcher.InstructionEnumeration();
    }
}
