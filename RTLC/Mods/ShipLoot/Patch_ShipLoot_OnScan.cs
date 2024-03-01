using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RTLC.Helpers;
using RTLC.Translations;

namespace RTLC.Mods.ShipLoot;
[HarmonyPatch]
internal class Patch_ShipLoot_OnScan
{
    [HarmonyPrepare]
    public static bool CheckModLoaded()
        => ModHelper.TryFindMethod(KnownPluginDependency.c_ShipLoot, "ShipLoot.Patches.HudManagerPatcher", "OnScan");

    [HarmonyTargetMethod]
    public static MethodBase GetMethodPatch() => ModHelper.GetMethod();

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ReplaceString(IEnumerable<CodeInstruction> instructions)
    {
        return Patch_LoadStringBasic.ReplaceText(instructions, null);
    }
}
