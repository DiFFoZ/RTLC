using HarmonyLib;

namespace RTLC.Patches.Translations.Specific;
[HarmonyPatch(typeof(HUDManager), nameof(HUDManager.DisplayStatusEffect))]
[HarmonyPriority(Priority.Last)]
internal static class Patch_HUDManager_DisplayStatusEffect
{
    [HarmonyPatch]
    [HarmonyPrefix]
    public static void DisplayStatusEffect(ref string statusEffect)
    {
        statusEffect = Translation.GetLocalizedText(statusEffect);
    }
}
