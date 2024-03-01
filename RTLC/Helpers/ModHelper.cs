using System.Reflection;
using HarmonyLib;

namespace RTLC.Helpers;
internal static class ModHelper
{
    private static MethodInfo? s_MethodToPatch;

    public static bool TryFindMethod(string modId, string type, string method)
    {
        if (!BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(modId, out var pluginInfo))
        {
            return false;
        }

        var assembly = pluginInfo.Instance.GetType().Assembly;
        var patchingType = assembly.GetType(type, false);

        s_MethodToPatch = AccessTools.Method(patchingType, method);
        return s_MethodToPatch != null;
    }

    public static MethodBase GetMethod() => s_MethodToPatch!;
}
