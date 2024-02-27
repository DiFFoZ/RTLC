using HarmonyLib;

namespace RTLC.Translations;
[HarmonyPatch(typeof(StartOfRound), "Awake")]
internal static class Patch_StartOfRound_Awake
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void TranslateLevelDescription(StartOfRound __instance)
    {
        var levels = __instance.levels;
        if (levels == null) return;

        foreach (var level in levels)
        {
            if (level == null)
                continue;

            level.LevelDescription = Translation.GetLocalizedText(level.LevelDescription);
        }
    }
}
