using System;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using MonoMod.Cil;
using RTLC.Helpers;

namespace RTLC.API;
internal static class ReplaceStringConcatPatch
{
    public static readonly MethodInfo ReplaceLoadStringWithTranslatedMethod = typeof(ReplaceStringConcatPatch)
        .GetMethod(nameof(ReplaceStringConcatPatch), AccessTools.all);

    public static void ReplaceStringConcatToTranslationFormat(ILContext context)
    {
        var cursor = new ILCursor(context);

        var corLibStringTypeFullName = typeof(string).FullName;

        while (cursor.TryGotoNext(i => i.Operand is MethodReference method
            && method.DeclaringType.FullName == corLibStringTypeFullName
            && method.Name.Equals("Concat")))
        {
            if (ILContextHelper.TestIgnore(cursor))
            {
                cursor.Index++;
                continue;
            }

            var method = (MethodReference)cursor.Next.Operand;
            var args = method.Parameters;

            var methodInfo = args switch
            {
                not null when args.Count == 2 => SymbolExtensions.GetMethodInfo(() => Translation.Concat(default!, default!)),
                not null when args.Count == 3 => SymbolExtensions.GetMethodInfo(() => Translation.Concat(default!, default!, default!)),
                not null when args.Count == 4 => SymbolExtensions.GetMethodInfo(() => Translation.Concat(default!, default!, default!, default!)),
                _ => SymbolExtensions.GetMethodInfo(() => Translation.Concat(Array.Empty<string>())),
            };

            cursor.Next.Operand = context.Import(methodInfo);
            cursor.Index++;
        }
    }
}
