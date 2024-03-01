using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RTLC.Translations;

namespace RTLC.Mods.ShipLoot;
[HarmonyPatch]
internal class Patch_ShipLoot_OnScan
{
    private static MethodInfo? s_MethodToPatch;

    [HarmonyPrepare]
    public static bool CheckModLoaded()
    {
        if (!BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(KnownPluginDependency.c_ShipLoot, out var pluginInfo))
        {
            return false;
        }

        var assembly = pluginInfo.Instance.GetType().Assembly;
        var patchingType = assembly.GetType("ShipLoot.Patches.HudManagerPatcher", false);

        s_MethodToPatch = AccessTools.Method(patchingType, "OnScan");
        return s_MethodToPatch != null;
    }

    [HarmonyTargetMethod]
    public static MethodBase GetMethodPatch()
    {
        return s_MethodToPatch!;
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ReplaceString(IEnumerable<CodeInstruction> instructions)
    {
        return Patch_LoadStringBasic.ReplaceText(instructions, null);
    }
}
