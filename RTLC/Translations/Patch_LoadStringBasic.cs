using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using GameNetcodeStuff;
using HarmonyLib;
using RTLC.Helpers;

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

        yield return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.ChangeControlTipMultiple));

        yield return AccessTools.Method(typeof(StartOfRound), nameof(StartOfRound.SetMapScreenInfoToCurrentLevel));

        yield return AccessTools.Method(typeof(SteamValveHazard), "Update");
        yield return AccessTools.Method(typeof(ShipTeleporter), "Update");

        yield return AccessTools.Method(typeof(ElevatorAnimationEvents), nameof(ElevatorAnimationEvents.ElevatorFullyRunning));
        yield return AccessTools.Method(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject));
        yield return AccessTools.Method(typeof(Terminal), "TextPostProcess");
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ReplaceText(IEnumerable<CodeInstruction> instructions, MethodBase? originalMethod)
    {
        TranspilerHelper.PatchModsPrefixesAndPostfixes(originalMethod,
            SymbolExtensions.GetMethodInfo(() => ReplaceText(default!, default!)));

        return PatchMethod(instructions);
    }

    private static IEnumerable<CodeInstruction> PatchMethod(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        if (TranspilerHelper.FindTryCatchInstructions(matcher))
        {
            // todo: add logging
            return instructions;
        }

        matcher.MatchForward(false, new CodeMatch(c => c.opcode == OpCodes.Ldstr))
           .Repeat(m =>
           {
               var nextInstruction = m.InstructionAt(1);
               if (nextInstruction.opcode == OpCodes.Stfld)
               {
                   m.Advance(1);
                   return;
               }

               if (TranspilerHelper.CheckInstructionsForIgnoredMethods(matcher))
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
