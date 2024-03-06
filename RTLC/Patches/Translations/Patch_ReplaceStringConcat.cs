using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MonoMod.Cil;
using RTLC.API;
using RTLC.Helpers;

namespace RTLC.Patches.Translations;
//[HarmonyPatch]
//[HarmonyPriority(Priority.LowerThanNormal - 1)]
internal static class Patch_ReplaceStringConcat
{
    [HarmonyTargetMethods]
    public static IEnumerable<MethodBase> GetPatchingMethods()
    {
        yield break;
    }

    [HarmonyILManipulator]
    public static void ReplaceFormat(ILContext context, MethodBase originalMethod)
    {
        ILContextHelper.PatchModsPrefixesAndPostfixes(originalMethod, LoadStringPatch.ReplaceLoadStringWithTranslatedMethod);

        ReplaceStringConcatPatch.ReplaceStringConcatToTranslationFormat(context);
    }
}
