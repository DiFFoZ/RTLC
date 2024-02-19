using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace RTLC.Patches;
[HarmonyPatch(typeof(HUDManager))]
internal static class Patch_HUDManager
{
    [HarmonyPatch(nameof(HUDManager.SetClock))]
    [HarmonyPrefix]
    private static bool ShowClockIn24Hour(float timeNormalized, float numberOfHours)
    {
        var minutes = (int)(timeNormalized * (60f * numberOfHours)) + 360;
        var clock = string.Format("{0:00}:{1:00}", minutes / 60, minutes % 60);

        HUDManager.Instance.clockNumber.text = clock;
        return false;
    }

    [HarmonyPatch("Update")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ShowKilograms(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        // 1 lb (real) = 105 lb (in-game)
        // 1 lb (real) = 0,45359237 kg
        // 1 kg (in-game) = 0,45359237 * 105 = 47.62719885

        // replace lb to kg values
        matcher
            .Start()
            .SearchForward(c => c.opcode == OpCodes.Ldc_R4 && (float)c.operand == 105f)
            .Operand = 47.62719885f;

        // replace {0} lb -> {0} kg
        matcher
            .SearchForward(c => c.opcode == OpCodes.Ldstr && (string)c.operand == "{0} lb")
            .Operand = "{0} кг";

        return matcher.InstructionEnumeration();
    }
}
