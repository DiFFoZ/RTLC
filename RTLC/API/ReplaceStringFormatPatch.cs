using System;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using MonoMod.Cil;
using RTLC.Helpers;
using System.Linq;

namespace RTLC.API;
internal static class ReplaceStringFormatPatch
{
    public static readonly MethodInfo ReplaceLoadStringWithTranslatedMethod = typeof(ReplaceStringFormatPatch)
        .GetMethod(nameof(ReplaceStringFormatToTranslationFormat), AccessTools.all);

    public static void ReplaceStringFormatToTranslationFormat(ILContext context)
    {
        var cursor = new ILCursor(context);

        var corLibStringTypeFullName = typeof(string).FullName;

        while (cursor.TryGotoNext(i => i.Operand is MethodReference method
            && method.DeclaringType.FullName == corLibStringTypeFullName
            && method.Name.Equals("Format")))
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
                not null when args.Count == 3 => SymbolExtensions.GetMethodInfo(() => Translation.Format(default!, default!, default!)),
                not null when args.Count == 4 => SymbolExtensions.GetMethodInfo(() => Translation.Format(default!, default!, default!, default!)),
                not null when args[1].CustomAttributes.Any(a => a.Constructor.DeclaringType.FullName == typeof(ParamArrayAttribute).FullName) => SymbolExtensions.GetMethodInfo(() => Translation.Format(default!, default!)),
                _ => SymbolExtensions.GetMethodInfo(() => Translation.Format(default!, cursor)),
            };

            cursor.Next.Operand = context.Import(methodInfo);
            cursor.Index++;
        }
    }
}
