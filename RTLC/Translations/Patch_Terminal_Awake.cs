using System;
using System.Linq;
using HarmonyLib;

namespace RTLC.Translations;
[HarmonyPatch(typeof(Terminal))]
[HarmonyPriority(Priority.Last)]
internal static class Patch_Terminal_Awake
{
    private static bool s_Initialized;

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void TranslateNodes(Terminal __instance)
    {
        if (s_Initialized)
            return;

        foreach (var node in __instance.terminalNodes.allKeywords.SelectMany(x =>
                x.compatibleNouns.Select(c => c.result).Append(x.specialKeywordResult))
            .Concat(__instance.terminalNodes.specialNodes)
            .Concat(__instance.terminalNodes.terminalNodes))
        {
            if (node == null)
            {
                continue;
            }

            node.displayText = Translation.GetLocalizedText(node.name, node.displayText, false);

            var options = node.terminalOptions;
            if (options == null)
            {
                continue;
            }

            foreach (var option in options)
            {
                if (option == null || option.result == null)
                {
                    continue;
                }

                option.result.displayText = Translation.GetLocalizedText(option.result.name, option.result.displayText, false);
            }
        }

        s_Initialized = true;
    }

    [HarmonyPatch("TextPostProcess")]
    [HarmonyPostfix]
    public static void ReplaceTimeOfDay(ref string __result)
    {
        var currentDay = DateTime.Now.DayOfWeek switch
        {
            DayOfWeek.Monday => "Счастливого понедельника",
            DayOfWeek.Tuesday => "Счастливого вторника",
            DayOfWeek.Wednesday => "Счастливой среды",
            DayOfWeek.Thursday => "Счастливого четверга",
            DayOfWeek.Friday => "Счастливой пятницы",
            DayOfWeek.Saturday => "Счастливой субботы",
            DayOfWeek.Sunday => "Счастливого воскресенья",
            _ => string.Empty
        };

        __result = __result.Replace("[currentDayRussian]", currentDay);
    }
}
