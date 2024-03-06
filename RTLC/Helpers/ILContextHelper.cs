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

    private static readonly string[] s_IgnoredStrings =
    [
        "Log",
        "SpawnPrefab",
        "System.Boolean System.String::Equals(System.String",
        "System.Boolean System.String::Contains(System.String"
    ];

    private static readonly HashSet<int> s_Vars = [];

    public static bool TestIgnore(ILCursor cursor)
    {
        var savedIndex = cursor.Index;
        var shouldBeIgnored = false;

        var ldStringCount = 0;
        s_Vars.Clear();

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
            /*            else if (IsStoreLocal(instruction.OpCode))
                        {
                            s_Vars.Add(GetStoreOrLoadLocalIndex(instruction));
                            continue;
                        }
                        else if (IsLoadLocal(instruction.OpCode))
                        {
                            if (s_Vars.Contains(GetStoreOrLoadLocalIndex(instruction)))
                            {

                                break;
                            }
                            shouldBeIgnored = true;
                            break;
                        }*/
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

    private static bool IsStoreLocal(OpCode opCode) => opCode == OpCodes.Stloc
        || opCode == OpCodes.Stloc_0
        || opCode == OpCodes.Stloc_1
        || opCode == OpCodes.Stloc_2
        || opCode == OpCodes.Stloc_3
        || opCode == OpCodes.Stloc_S;

    private static bool IsLoadLocal(OpCode opCode) => opCode == OpCodes.Ldloc
        || opCode == OpCodes.Ldloc_0
        || opCode == OpCodes.Ldloc_1
        || opCode == OpCodes.Ldloc_2
        || opCode == OpCodes.Ldloc_3
        || opCode == OpCodes.Ldloc_S;

    private static int GetStoreOrLoadLocalIndex(Instruction instruction)
    {
        var opCode = instruction.OpCode;

        if (opCode == OpCodes.Stloc || opCode == OpCodes.Stloc_S
            || opCode == OpCodes.Ldloc || opCode == OpCodes.Ldloc_S)
        {
            return (int)instruction.Operand;
        }

        if (opCode == OpCodes.Stloc_0 || opCode == OpCodes.Ldloc_0)
            return 0;
        if (opCode == OpCodes.Stloc_1 || opCode == OpCodes.Ldloc_1)
            return 1;
        if (opCode == OpCodes.Stloc_2 || opCode == OpCodes.Ldloc_2)
            return 2;
        if (opCode == OpCodes.Stloc_3 || opCode == OpCodes.Ldloc_3)
            return 3;

        return -1;
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
        if (method.FullName is "System.Boolean System.String::op_Inequality(System.String,System.String)"
            or "System.Boolean System.String::op_Equality(System.String,System.String)")
        {
            return true;
        }

        var methodName = method.Name;
        var methodFullName = method.FullName;
        for (var j = 0; j < s_IgnoredStrings.Length; j++)
        {
            if (methodName.StartsWith(s_IgnoredStrings[j], StringComparison.OrdinalIgnoreCase)
                || methodFullName.StartsWith(s_IgnoredStrings[j], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
