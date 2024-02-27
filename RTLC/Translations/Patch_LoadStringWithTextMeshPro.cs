using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RTLC.Helpers;
using TMPro;

namespace RTLC.Translations;
[HarmonyPatch]
internal static class Patch_LoadStringWithTextMeshPro
{
    private static readonly MethodInfo s_TMPText_TextSetter = AccessTools.PropertySetter(typeof(TMP_Text), nameof(TMP_Text.text));

    [HarmonyTargetMethods]
    public static IEnumerable<MethodBase> EnumeratePatchingMethods()
    {
        yield return AccessTools.Method(typeof(PreInitSceneScript), nameof(PreInitSceneScript.PressContinueButton));
        yield return AccessTools.Method(typeof(PreInitSceneScript), nameof(PreInitSceneScript.SkipToFinalSetting));

        yield return AccessTools.Method(typeof(MenuManager), nameof(MenuManager.StartAClient));
        yield return AccessTools.Method(typeof(MenuManager), nameof(MenuManager.ConfirmHostButton));
        yield return AccessTools.Method(typeof(MenuManager), nameof(MenuManager.HostSetLobbyPublic));
        yield return AccessTools.Method(typeof(MenuManager), "DisplayLeaderboardSlots");
        yield return AccessToolsHelper.AsyncMoveNext(typeof(MenuManager), "GetLeaderboardForChallenge"); // async

        yield return AccessTools.Method(typeof(ChallengeLeaderboardSlot), nameof(ChallengeLeaderboardSlot.SetSlotValues));

        yield return AccessTools.Method(typeof(IngamePlayerSettings), "SetChangesNotAppliedTextVisible");

        yield return AccessTools.Method(typeof(DeleteFileButton), nameof(DeleteFileButton.SetFileToDelete));
        yield return AccessTools.Method(typeof(SettingsOption), nameof(SettingsOption.ToggleEnabledImage));
        yield return AccessTools.Method(typeof(KepRemapPanel), nameof(KepRemapPanel.LoadKeybindsUI));

        yield return AccessToolsHelper.AsyncMoveNext(typeof(SteamLobbyManager), nameof(SteamLobbyManager.LoadServerList)); // async

        yield return AccessTools.Method(typeof(TimeOfDay), nameof(TimeOfDay.UpdateProfitQuotaCurrentTime));

        yield return AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(StartOfRound), nameof(StartOfRound.openingDoorsSequence)));
        yield return AccessTools.Method(typeof(StartOfRound), "SceneManager_OnLoad");
        yield return AccessTools.Method(typeof(StartOfRound), "SceneManager_OnLoadComplete1");

        yield return AccessTools.Method(typeof(RoundManager), nameof(RoundManager.GenerateNewLevelClientRpc));

        yield return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.ApplyPenalty));
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ReplaceText(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        matcher.MatchForward(false, new CodeMatch(c => c.opcode == OpCodes.Ldstr))
            .Repeat(m =>
            {
                if (TranspilerHelper.CheckInstructionsForFormattingUsed(m, s_TMPText_TextSetter))
                {
                    while (m.IsValid && (m.Operand as MethodInfo) != s_TMPText_TextSetter)
                    {
                        if (m.Opcode == OpCodes.Ldstr)
                            m.Operand = Translation.GetLocalizedText((string)m.Operand);

                        m.Advance(1);
                    }
                    return;
                }

                var nextInstruction = m.InstructionAt(1);
                if (!nextInstruction.Calls(s_TMPText_TextSetter))
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
