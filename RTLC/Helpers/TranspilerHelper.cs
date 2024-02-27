using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace RTLC.Helpers;
internal static class TranspilerHelper
{
    private static readonly HashSet<Type> s_IgnoredTypes = [typeof(Debug), typeof(Animation), typeof(Animator)];

    public static bool CheckInstructionsForIgnoredMethods(CodeMatcher matcher)
    {
        var index = matcher.Pos;
        var shouldBeIgnored = false;

        while (matcher.IsValid)
        {
            if (matcher.Opcode == OpCodes.Stfld)
            {
                shouldBeIgnored = true;
                break;
            }

            if (!(matcher.Opcode == OpCodes.Call || matcher.Opcode == OpCodes.Callvirt))
            {
                matcher.Advance(1);
                continue;
            }

            var method = (MethodInfo)matcher.Operand;

            if (s_IgnoredTypes.Contains(method.DeclaringType))
            {
                shouldBeIgnored = true;
                break;
            }

            matcher.Advance(1);
        }

        matcher.Start();
        matcher.Advance(index);

        return shouldBeIgnored;
    }

    public static bool CheckInstructionsForFormattingUsed(CodeMatcher matcher, MethodInfo allowedMethodCall)
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
            var methodName = method.Name;
            if (methodName.Equals("Format")
                || methodName.Equals("Concat")
                || methodName.Equals("Join"))
            {
                isFormating = true;
                matcher.Advance(1);
                continue;
            }

            if (s_IgnoredTypes.Contains(method.DeclaringType))
            {
                isFormating = false;
                break;
            }

            if (method == allowedMethodCall)
            {
                break;
            }

            matcher.Advance(1);
        }

        matcher.Start();
        matcher.Advance(index);

        return isFormating;
    }
}
