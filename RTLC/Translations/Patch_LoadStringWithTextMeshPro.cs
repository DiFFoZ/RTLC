using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
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
        yield return AsyncMoveNext(typeof(MenuManager), "GetLeaderboardForChallenge"); // async

        yield return AccessTools.Method(typeof(ChallengeLeaderboardSlot), nameof(ChallengeLeaderboardSlot.SetSlotValues));

        yield return AccessTools.Method(typeof(IngamePlayerSettings), "SetChangesNotAppliedTextVisible");

        yield return AccessTools.Method(typeof(DeleteFileButton), nameof(DeleteFileButton.SetFileToDelete));
        yield return AccessTools.Method(typeof(SettingsOption), nameof(SettingsOption.ToggleEnabledImage));
        yield return AccessTools.Method(typeof(KepRemapPanel), nameof(KepRemapPanel.LoadKeybindsUI));

        yield return AsyncMoveNext(typeof(SteamLobbyManager), nameof(SteamLobbyManager.LoadServerList)); // async

        yield return AccessTools.Method(typeof(TimeOfDay), nameof(TimeOfDay.UpdateProfitQuotaCurrentTime));
    }

    public static MethodInfo AsyncMoveNext(Type type, string name)
    {
        var method = AccessTools.Method(type, name);
        if (method is null)
        {
            return null!;
        }

        var asyncAttribute = method.GetCustomAttribute<AsyncStateMachineAttribute>();
        if (asyncAttribute is null)
        {
            return null!;
        }

        var asyncStateMachineType = asyncAttribute.StateMachineType;
        var asyncMethodBody = AccessTools.DeclaredMethod(asyncStateMachineType, nameof(IAsyncStateMachine.MoveNext));
        if (asyncMethodBody is null)
        {
            return null!;
        }

        return asyncMethodBody;
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ReplaceText(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        matcher.MatchForward(false, new CodeMatch(c => c.opcode == OpCodes.Ldstr))
            .Repeat(m =>
            {
                if (CheckIsConcating(m))
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

    private static bool CheckIsConcating(CodeMatcher matcher)
    {
        var index = matcher.Pos;
        var isFormating = false;

        while (matcher.IsValid)
        {
            if (!(matcher.Opcode == OpCodes.Call || matcher.Opcode == OpCodes.Callvirt))
            {
                matcher.Advance(1);
                continue;
            }

            var method = (MethodInfo)matcher.Operand;
            if (method.Name.Equals("Format")
                || method.Name.Equals("Concat"))
            {
                isFormating = true;
                matcher.Advance(1);
                continue;
            }

            if (method == s_TMPText_TextSetter)
            {
                break;
            }

            if (isFormating)
            {
                isFormating = false;
                break;
            }

            matcher.Advance(1);
        }

        matcher.Start();
        matcher.Advance(index);

        return isFormating;
    }
}
