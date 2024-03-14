using System;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RTLC.Helpers;

namespace RTLC.API;
internal static class LoadStringPatch
{
    public static readonly MethodInfo ReplaceLoadStringWithTranslatedMethod = typeof(LoadStringPatch)
        .GetMethod(nameof(ReplaceLoadStringWithTranslated), AccessTools.all);

    public static void ReplaceLoadStringWithTranslated(ILContext context)
    {
        var cursor = new ILCursor(context);

        while (cursor.TryGotoNext(static i => i.OpCode == OpCodes.Ldstr))
        {
            if (ILContextHelper.TestIgnore(cursor))
            {
                cursor.Index++;
                continue;
            }


            cursor.Next.Operand = Translation.GetLocalizedText((string)cursor.Next.Operand);
            cursor.Index++;
        }
    }
}
