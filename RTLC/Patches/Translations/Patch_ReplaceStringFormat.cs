using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MonoMod.Cil;
using RTLC.API;
using RTLC.Helpers;

namespace RTLC.Translations;
[HarmonyPatch]
[HarmonyPriority(Priority.LowerThanNormal)]
internal static class Patch_ReplaceStringFormat
{
    [HarmonyTargetMethods]
    public static IEnumerable<MethodBase> GetPatchingMethods()
    {
        yield return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.DisplayNewScrapFound));
        yield return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.DisplayDaysLeft));
        yield return AccessTools.Method(typeof(TimeOfDay), nameof(TimeOfDay.UpdateProfitQuotaCurrentTime));
    }

    [HarmonyILManipulator]
    public static void ReplaceFormat(ILContext context, MethodBase originalMethod)
    {
        ILContextHelper.PatchModsPrefixesAndPostfixes(originalMethod, LoadStringPatch.ReplaceLoadStringWithTranslatedMethod);

        ReplaceStringFormatPatch.ReplaceStringFormatToTranslationFormat(context);
    }
}
