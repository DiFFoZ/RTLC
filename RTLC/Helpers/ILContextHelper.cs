using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using BepInEx.Logging;
using Dissonance.Extensions;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using RTLC.Extensions;
using Steamworks.Data;
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
        typeof(ES3),
        typeof(LobbyQuery)
    ];

    private static readonly HashSet<string> s_IgnoredTypeNames = s_IgnoredTypes
        .Select(t => t.FullName)
        .ToHashSet();

    private static readonly HashSet<MethodInfo> s_IgnoredMethods =
    [
        AccessTools.PropertySetter(typeof(UnityEngine.Object), nameof(UnityEngine.Object.name))
    ];

    private static readonly string[] s_IgnoredMethodsNames = s_IgnoredMethods
        .Select(t => t.GetFullName())
        .Concat(
        [
        "Log",
        "SpawnPrefab",
        "System.Boolean System.String::Equals(System.String",
        "System.Boolean System.String::Contains(System.String"
        ])
        .ToArray();

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
            else if (instruction.OpCode == OpCodes.Stfld)
            {
                shouldBeIgnored = true;
                break;
            }
            else if (!(instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt))
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

            var paramsWithString = method.Parameters.Any(p => p.ParameterType.Is("System.String[]")) ? 99 : 0
                 + method.Parameters.Count(p => p.ParameterType.Is("System.String"));
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
        if (originalMethod == null || originalMethod.DeclaringType.Assembly == typeof(ILContextHelper).Assembly)
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
            if (patch.PatchMethod.DeclaringType.Assembly == typeof(ILContextHelper).Assembly)
            {
                continue;
            }

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
        var methodFullName = method.FullName;
        if (methodFullName is "System.Boolean System.String::op_Inequality(System.String,System.String)"
            or "System.Boolean System.String::op_Equality(System.String,System.String)")
        {
            return true;
        }

        var methodName = method.Name;
        for (var j = 0; j < s_IgnoredMethodsNames.Length; j++)
        {
            if (methodName.StartsWith(s_IgnoredMethodsNames[j], StringComparison.OrdinalIgnoreCase)
                || methodFullName.StartsWith(s_IgnoredMethodsNames[j], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
