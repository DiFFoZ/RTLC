using HarmonyLib;

namespace RTLC.Translations.Specific;
[HarmonyPatch(typeof(KepRemapPanel), nameof(KepRemapPanel.OnEnable))]
internal static class Patch_KepRemapPanel_OnEnable
{
    [HarmonyPrefix]
    public static void TranslateKeybinds(KepRemapPanel __instance)
    {
        if (__instance.remappableKeys == null)
            return;

        foreach (var keybind in __instance.remappableKeys)
        {
            if (keybind == null)
                continue;

            keybind.ControlName = Translation.GetLocalizedText(keybind.ControlName);
        }
    }
}
