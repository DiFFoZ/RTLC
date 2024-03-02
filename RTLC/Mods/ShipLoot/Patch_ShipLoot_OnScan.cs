using System.Reflection;
using HarmonyLib;
using MonoMod.Cil;
using RTLC.API;
using RTLC.Helpers;

namespace RTLC.Mods.ShipLoot;
[HarmonyPatch]
internal class Patch_ShipLoot_OnScan
{
    [HarmonyPrepare]
    public static bool CheckModLoaded()
        => ModHelper.TryFindMethod(KnownPluginDependency.c_ShipLoot, "ShipLoot.Patches.HudManagerPatcher", "OnScan");

    [HarmonyTargetMethod]
    public static MethodBase GetMethodPatch() => ModHelper.GetMethod();

    [HarmonyILManipulator]
    public static void ReplaceString(ILContext context)
    {
        LoadStringPatch.ReplaceLoadStringWithTranslated(context);
    }
}
