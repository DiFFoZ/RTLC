using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;

namespace RTLC.Terminals;
[HarmonyPatch(typeof(Terminal))]
public static class Patch_Terminal_TextPostProcess
{
    [HarmonyPatch("TextPostProcess")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ReplaceToRussian(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        matcher
            .Start()
            .SearchForward(c => c.opcode == OpCodes.Constrained && (Type)c.operand == typeof(LevelWeatherType))
            .ThrowIfInvalid("Failed to find weather")
            .RemoveInstruction()
            .Advance(-1)
            .SetOpcodeAndAdvance(OpCodes.Ldfld)
            .ThrowIfFalse("Is not Object.ToString method", c => c.Opcode == OpCodes.Callvirt)
            .SetInstruction(CodeInstruction.Call(() => WeatherToRussian(LevelWeatherType.None)));

        return matcher.InstructionEnumeration();
    }

    public static string WeatherToRussian(LevelWeatherType weather) => weather switch
    {
        LevelWeatherType.Rainy => "Дождь",
        LevelWeatherType.Stormy => "Шторм",
        LevelWeatherType.Foggy => "Туман",
        LevelWeatherType.Flooded => "Наводнение",
        LevelWeatherType.Eclipsed => "Затмение",
        _ => string.Empty
    };
}
