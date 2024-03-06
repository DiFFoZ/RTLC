using HarmonyLib;

namespace RTLC.Patches.Translations.Specific;
[HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Start))]
[HarmonyPriority(Priority.Last)]
internal static class Patch_HUDManager_PlayerLevels
{
    public static void TranslatePlayerLevels(HUDManager __instance)
    {
        foreach (var level in __instance.playerLevels)
        {
            if (level != null)
            {
                level.levelName = Translation.GetLocalizedText(level.levelName);
            }
        }
    }
}
