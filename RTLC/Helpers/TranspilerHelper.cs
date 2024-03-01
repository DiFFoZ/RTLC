using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using HarmonyLib;
using RTLC.Translations;
using Unity.Netcode;
using UnityEngine;

namespace RTLC.Helpers;
internal static class TranspilerHelper
{
    private static readonly HashSet<Type> s_IgnoredTypes = [typeof(Debug), typeof(Animation), typeof(Animator), typeof(Regex), typeof(CustomMessagingManager)];

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

    public static bool FindTryCatchInstructions(CodeMatcher matcher)
    {
        var index = matcher.Pos;

        var didFindLeaveInstruction = matcher.SearchForward(c => c.opcode == OpCodes.Leave || c.opcode == OpCodes.Leave_S).IsValid;

        matcher.Start();
        matcher.Advance(index);

        return didFindLeaveInstruction;
    }

    public static void PatchModsPrefixesAndPostfixes(MethodBase? method, MethodInfo transpilerMethod)
    {
        if (method == null)
        {
            return;
        }    

        // find mods that patch original method with prefix/postfix
        var patchInfo = Harmony.GetPatchInfo(method);
        if (patchInfo == null)
        {
            return;
        }

        foreach (var patch in patchInfo.Prefixes.Concat(patchInfo.Postfixes))
        {
            var modPatchInfo = Harmony.GetPatchInfo(patch.PatchMethod);
            if (modPatchInfo != null && modPatchInfo.Owners.Contains(MyPluginInfo.PLUGIN_GUID))
            {
                RTLCPlugin.Instance.Logger.LogInfo($"Ignored patching of {patch.PatchMethod.FullDescription()} due to already patched");
                continue;
            }

            RTLCPlugin.Instance.Logger.LogInfo($"Patching {patch.PatchMethod.FullDescription()}");

            RTLCPlugin.Instance.Harmony.CreateProcessor(patch.PatchMethod)
                .AddTranspiler(transpilerMethod)
                .Patch();
        }
    }
}
