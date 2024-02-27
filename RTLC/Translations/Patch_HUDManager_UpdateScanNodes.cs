using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace RTLC.Translations;
[HarmonyPatch]
internal static class Patch_HUDManager_UpdateScanNodes
{
    [HarmonyTargetMethod]
    public static MethodBase GetPatchingMethod()
    {
        // https://github.com/Sligili/HDLethalCompany/blob/master/HDLethalCompanyRemake/HDLethalCompany.cs#L205
        // HDLethalCompany breaks compability with this postfix

        if (!BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(KnownPluginDependency.c_HDLethalCompany, out _))
        {
            // patch original method
            return AccessTools.Method(typeof(HUDManager), "UpdateScanNodes");
        }

        return AccessTools.Method("HDLethalCompany.Patch.GraphicsPatch:UpdateScanNodesPostfix");
    }

    [HarmonyILManipulator]
    public static void ReplaceText(ILContext ctx)
    {
        // HarmonyX bugs with transpiler, using ILManipulator for the workaround
        // https://github.com/BepInEx/HarmonyX/pull/96

        var headerTextField = AccessTools.Field(typeof(ScanNodeProperties), nameof(ScanNodeProperties.headerText));
        var subTextField = AccessTools.Field(typeof(ScanNodeProperties), nameof(ScanNodeProperties.subText));

        var cursor = new ILCursor(ctx);

        while (cursor.TryGotoNext(c => c.MatchLdfld(headerTextField) || c.MatchLdfld(subTextField)))
        {
            cursor.Index++;
            cursor.Emit(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => Translation.GetLocalizedText(default!)));
        }
    }
}
