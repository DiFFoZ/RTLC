using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace RTLC.SmartFormat;
internal class IsMatchFormatterEx : IFormatter
{
    private const char c_SplitChar = '|';

    public string Name { get; set; } = "ismatch";
    public bool CanAutoDetect { get; set; } = false;

    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        if (formattingInfo.CurrentValue is null)
            return false;

        var expression = formattingInfo.FormatterOptions.Replace("\n", @"\n");
        var formats = formattingInfo.Format?.Split(c_SplitChar);

        // Check whether arguments can be handled by this formatter
        if (formats is null || formats.Count != 2)
        {
            if (formats?.Count == 0)
                return true;

            // Auto detection calls just return a failure to evaluate
            if (string.IsNullOrEmpty(formattingInfo.Placeholder?.FormatterName))
                return false;

            // throw, if the formatter has been called explicitly
            throw new FormatException(
                $"Formatter named '{formattingInfo.Placeholder?.FormatterName}' requires at least 2 format options.");
        }

        var match = Regex.Match(formattingInfo.CurrentValue.ToString(), expression, RegexOptions.Multiline, TimeSpan.FromMilliseconds(500));

        if (!match.Success)
        {
            // Output the "no match" part of the format
            formattingInfo.FormatAsChild(formats[1], formattingInfo.CurrentValue);

            return true;
        }

        var matchingGroupValues = (from Group grp in match.Groups select grp.Value).ToList();
        matchingGroupValues.RemoveAt(0);

        foreach (var formatItem in formats[0].Items)
        {
            if (formatItem is Placeholder ph)
            {
                var variable = new KeyValuePair<string, object?>("m", matchingGroupValues);
                Format(formattingInfo, ph, variable);
                continue;
            }

            // so it must be a literal
            var literalText = (LiteralText)formatItem;
            // On Dispose() the Format goes back to the object pool
            using var childFormat = formattingInfo.Format?.Substring(literalText.StartIndex - formattingInfo.Format.StartIndex, literalText.Length);
            if (childFormat is null) continue;
            formattingInfo.FormatAsChild(childFormat, formattingInfo.CurrentValue);
        }

        return true;
    }

    private void Format(IFormattingInfo formattingInfo, Placeholder placeholder, object matchingGroupValues)
    {
        // On Dispose() the Format goes back to the object pool
        using var childFormat =
            formattingInfo.Format?.Substring(placeholder.StartIndex - formattingInfo.Format.StartIndex,
                placeholder.Length);
        if (childFormat is null) return;

        // Is the placeholder a "magic IsMatchFormatter" one?
        if (placeholder.GetSelectors().Count > 0 && placeholder.GetSelectors()[0]?.RawText == "m")
        {
            // The nested placeholder will output the matching group values
            formattingInfo.FormatAsChild(childFormat, matchingGroupValues);
        }
        else
        {
            formattingInfo.FormatAsChild(childFormat, formattingInfo.CurrentValue);
        }
    }
}
