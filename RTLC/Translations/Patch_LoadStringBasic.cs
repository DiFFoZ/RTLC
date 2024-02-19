using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;

namespace RTLC.Translations;
[HarmonyPatch]
internal static class Patch_LoadStringBasic
{
    [HarmonyTargetMethods]
    public static IEnumerable<MethodBase> EnumeratePatchingMethods()
    {
        yield return AccessTools.Method(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.RefreshAndDisplayCurrentMicrophone));
        yield return AccessTools.Method(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.SetMicPushToTalk));
        yield return AccessTools.Method(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.SwitchMicrophoneSetting));
        yield return AccessTools.Method(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.UpdateMicPushToTalkButton));
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ReplaceText(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        matcher.MatchForward(false, new CodeMatch(c => c.opcode == OpCodes.Ldstr))
           .Repeat(m =>
           {
               var nextInstruction = m.InstructionAt(1);
               if (nextInstruction.opcode == OpCodes.Stfld || nextInstruction.operand is MethodInfo { Name: "Log" })
               {
                   m.Advance(1);
                   return;
               }

               m.Operand = Translation.GetLocalizedText((string)m.Operand);
               m.Advance(1);
           });

        return matcher.InstructionEnumeration();
    }
}
