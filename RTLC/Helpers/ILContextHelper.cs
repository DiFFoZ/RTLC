using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using RTLC.Extensions;
using Unity.Netcode;
using UnityEngine;

namespace RTLC.Helpers;
internal static class ILContextHelper
{
    private static readonly HashSet<Type> s_IgnoredTypes =
    [
        typeof(Debug),
        typeof(Animation),
        typeof(Animator),
        typeof(Regex),
        typeof(CustomMessagingManager),
        typeof(ManualLogSource),
        typeof(AssetBundle),
        typeof(GameObject),
        typeof(Transform),
        typeof(Path),
        typeof(Directory),
        typeof(File),
        typeof(FileStream),
        typeof(ConfigFile),
        typeof(ConfigDescription),
        typeof(ES3)
    ];

    private static readonly HashSet<string> s_IgnoredTypeNames = s_IgnoredTypes
        .Select(t => t.FullName)
        .ToHashSet();

    private static readonly string[] s_IgnoredStrings =
    [
        "Log",
        "SpawnPrefab",
    ];

    public static bool TestIgnore(ILCursor cursor)
    {
        var savedIndex = cursor.Index;
        var shouldBeIgnored = false;

        var ldStringCount = 0;

        for (var i = savedIndex; i < cursor.Instrs.Count; i++)
        {
            var instruction = cursor.Instrs[i];

            if (instruction.OpCode == OpCodes.Ldstr)
            {
                ldStringCount++;
                continue;
            }

            if (instruction.OpCode == OpCodes.Stfld)
            {
                shouldBeIgnored = true;
                break;
            }

            if (!(instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt))
            {
                continue;
            }

            var method = (MethodReference)instruction.Operand;
            if (s_IgnoredTypeNames.Contains(method.DeclaringType.FullName))
            {
                shouldBeIgnored = true;
                break;
            }

            if (IsMethodIgnored(method))
            {
                shouldBeIgnored = true;
                break;
            }

            var paramsWithString = method.Parameters.Count(p => p.ParameterType.Is("System.String"));
            var returnIsString = method.ReturnType?.Is("System.String") ?? false;
            ldStringCount = Mathf.Clamp(ldStringCount - paramsWithString, 0, 10);

            if (returnIsString)
            {
                ldStringCount++;
            }

            if (ldStringCount == 0)
            {
                break;
            }
        }

        cursor.Index = savedIndex;

        return shouldBeIgnored;
    }

    public static void PatchModsPrefixesAndPostfixes(MethodBase? originalMethod, MethodInfo ilManipulatorPatch)
    {
        if (originalMethod == null)
        {
            return;
        }

        // find mods that patch original method with prefix/postfix
        var patchInfo = Harmony.GetPatchInfo(originalMethod);
        if (patchInfo == null)
        {
            return;
        }

        foreach (var patch in patchInfo.Prefixes.Concat(patchInfo.Postfixes))
        {
            var modPatchInfo = Harmony.GetPatchInfo(patch.PatchMethod);
            if (modPatchInfo != null && modPatchInfo.Owners.Contains(MyPluginInfo.PLUGIN_GUID))
            {
                RTLCPlugin.Instance.Logger.LogInfo($"Ignored patching of {patch.PatchMethod.GetReadableString()} due to already patched");
                continue;
            }

            RTLCPlugin.Instance.Logger.LogInfo($"Patching {patch.PatchMethod.GetReadableString()}");

            RTLCPlugin.Instance.Harmony.CreateProcessor(patch.PatchMethod)
                .AddILManipulator(ilManipulatorPatch)
                .Patch();
        }
    }

    private static bool IsMethodIgnored(MethodReference method)
    {
        var methodName = method.Name;
        for (var j = 0; j < s_IgnoredStrings.Length; j++)
        {
            if (methodName.StartsWith(s_IgnoredStrings[j], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
