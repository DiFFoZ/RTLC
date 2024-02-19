using HarmonyLib;

namespace RTLC.Translations;
[HarmonyPatch(typeof(KepRemapPanel), "OnEnable")]
internal static class Patch_KepRemapPanel_OnEnable
{
    [HarmonyPrefix]
    public static void TranslateKeybinds(KepRemapPanel __instance)
    {
        foreach (var keybind in __instance.remappableKeys)
        {
            keybind.ControlName = Translation.GetLocalizedText(keybind.ControlName);
        }
    }
}
