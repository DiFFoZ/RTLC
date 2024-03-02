using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace RTLC.Translations.Specific;
[HarmonyPatch]
internal static class Patch_HUDManager_UpdateScanNodes
{
    [HarmonyTargetMethod]
    public static MethodBase GetPatchingMethod()
    {
        // https://github.com/Sligili/HDLethalCompany/blob/master/HDLethalCompanyRemake/HDLethalCompany.cs#L205
        // HDLethalCompany breaks combability with this postfix

        if (BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(KnownPluginDependency.c_HDLethalCompany, out _))
        {
            var method = AccessTools.Method("HDLethalCompany.Patch.GraphicsPatch:UpdateScanNodesPostfix");
            if (method != null)
            {
                return method;
            }
        }

        return AccessTools.Method(typeof(HUDManager), nameof(HUDManager.UpdateScanNodes));
    }

    [HarmonyILManipulator]
    public static void ReplaceText(ILContext ctx)
    {
        // HarmonyX bugs with transpiler, using ILManipulator for the workaround
        // https://github.com/BepInEx/HarmonyX/pull/96

        var headerTextField = AccessTools.Field(typeof(ScanNodeProperties), nameof(ScanNodeProperties.headerText));
        var subTextField = AccessTools.Field(typeof(ScanNodeProperties), nameof(ScanNodeProperties.subText));

        var cursor = new ILCursor(ctx);

        while (cursor.TryGotoNext(MoveType.After, c => c.MatchLdfld(headerTextField) || c.MatchLdfld(subTextField)))
        {
            cursor.Emit(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => Translation.GetLocalizedText(default!)));
            cursor.Index++;
        }
    }
}
