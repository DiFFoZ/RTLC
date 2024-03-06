using System.Collections.Generic;
using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using MonoMod.Cil;
using RTLC.API;
using RTLC.Helpers;

namespace RTLC.Translations;
[HarmonyPatch]
[HarmonyPriority(Priority.LowerThanNormal)]
internal static class Patch_LoadStringBasic
{
    [HarmonyTargetMethods]
    public static IEnumerable<MethodBase> EnumeratePatchingMethods()
    {
        yield return AccessTools.Method(typeof(SteamValveHazard), nameof(SteamValveHazard.Update));
        yield return AccessTools.Method(typeof(ShipTeleporter), nameof(ShipTeleporter.Update));
        yield return AccessTools.Method(typeof(ElevatorAnimationEvents), nameof(ElevatorAnimationEvents.ElevatorFullyRunning));
        yield return AccessTools.Method(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject));
        yield return AccessTools.Method(typeof(Terminal), nameof(Terminal.TextPostProcess));
        yield return AccessTools.Method(typeof(ChallengeLeaderboardSlot), nameof(ChallengeLeaderboardSlot.SetSlotValues));
        yield return AccessTools.Method(typeof(DeleteFileButton), nameof(DeleteFileButton.SetFileToDelete));
        yield return AccessTools.Method(typeof(SettingsOption), nameof(SettingsOption.ToggleEnabledImage));
        yield return AccessTools.Method(typeof(KepRemapPanel), nameof(KepRemapPanel.LoadKeybindsUI));
        yield return AccessTools.Method(typeof(SaveFileUISlot), nameof(SaveFileUISlot.OnEnable));
        yield return AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(ManualCameraRenderer), nameof(ManualCameraRenderer.updateMapTarget)));

        yield return AccessTools.Method(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.RefreshAndDisplayCurrentMicrophone));
        yield return AccessTools.Method(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.SetMicPushToTalk));
        yield return AccessTools.Method(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.SwitchMicrophoneSetting));
        yield return AccessTools.Method(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.UpdateMicPushToTalkButton));
        yield return AccessTools.Method(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.SetChangesNotAppliedTextVisible));

        yield return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.ChangeControlTipMultiple));
        yield return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.ScanNewCreatureServerRpc));
        yield return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.ScanNewCreatureClientRpc));
        yield return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.GetNewStoryLogServerRpc));
        yield return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.GetNewStoryLogClientRpc));
        yield return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.ApplyPenalty));
        yield return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.DisplayNewScrapFound));
        yield return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.DisplayNewDeadline));
        yield return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.FillEndGameStats));
        yield return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.SetShipLeaveEarlyVotesText));
        yield return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.Update));
        yield return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.SetSpectatingTextToPlayer));

        yield return AccessTools.Method(typeof(StartOfRound), nameof(StartOfRound.SetMapScreenInfoToCurrentLevel));
        yield return AccessTools.Method(typeof(StartOfRound), nameof(StartOfRound.EndGameClientRpc));
        yield return AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(StartOfRound), nameof(StartOfRound.openingDoorsSequence)));
        yield return AccessTools.Method(typeof(StartOfRound), nameof(StartOfRound.SceneManager_OnLoad));
        yield return AccessTools.Method(typeof(StartOfRound), nameof(StartOfRound.SceneManager_OnLoadComplete1));

        yield return AccessTools.Method(typeof(PreInitSceneScript), nameof(PreInitSceneScript.PressContinueButton));
        yield return AccessTools.Method(typeof(PreInitSceneScript), nameof(PreInitSceneScript.SkipToFinalSetting));

        yield return AccessTools.Method(typeof(MenuManager), nameof(MenuManager.StartAClient));
        yield return AccessTools.Method(typeof(MenuManager), nameof(MenuManager.ConfirmHostButton));
        yield return AccessTools.Method(typeof(MenuManager), nameof(MenuManager.HostSetLobbyPublic));
        yield return AccessTools.Method(typeof(MenuManager), nameof(MenuManager.DisplayLeaderboardSlots));
        yield return AccessToolsHelper.AsyncMoveNext(typeof(MenuManager), nameof(MenuManager.GetLeaderboardForChallenge)); // async

        yield return AccessToolsHelper.AsyncMoveNext(typeof(SteamLobbyManager), nameof(SteamLobbyManager.LoadServerList)); // async

        yield return AccessTools.Method(typeof(TimeOfDay), nameof(TimeOfDay.UpdateProfitQuotaCurrentTime));

        yield return AccessTools.Method(typeof(RoundManager), nameof(RoundManager.GenerateNewLevelClientRpc));
    }

    [HarmonyILManipulator]
    public static void ReplaceText(ILContext context, MethodBase? originalMethod)
    {
        ILContextHelper.PatchModsPrefixesAndPostfixes(originalMethod, LoadStringPatch.ReplaceLoadStringWithTranslatedMethod);

        LoadStringPatch.ReplaceLoadStringWithTranslated(context);
    }
}
