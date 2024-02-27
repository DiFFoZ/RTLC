using System.Linq;
using System.Text.RegularExpressions;

namespace RTLC;
internal static partial class Translation
{
    internal static string GetLocalizedText(string? key) => GetLocalizedText(key, key, false, null);

    internal static string GetLocalizedText(string? key, bool shouldIgnoreKey) => GetLocalizedText(key, key, shouldIgnoreKey, null);
    internal static string GetLocalizedText(string? key, bool shouldIgnoreKey, object @params) => GetLocalizedText(key, key, shouldIgnoreKey, @params);

    internal static string GetLocalizedText(string? key, string? originalTranslation) => GetLocalizedText(key, originalTranslation, false, null);
    internal static string GetLocalizedText(string? key, string? originalTranslation, bool shouldIgnoreKey) => GetLocalizedText(key, originalTranslation, shouldIgnoreKey, null);

    internal static string GetLocalizedText(string? key, string? originalTranslation, bool shouldIgnoreKey, object? @params)
    {
        if (ShouldIgnoreTranslation(originalTranslation!)
            || s_Translations.ContainsValue(key!)
            || s_IgnoredTranslations.Contains(key!)
            || s_IgnoredTranslations.Contains(originalTranslation!))
        {
            return originalTranslation!;
        }

        if (s_Translations.TryGetValue(key!, out var translation))
        {
            return s_SmartFormatter.Format(s_RussianCultureInfo, translation, new { Original = originalTranslation });
        }

        if (s_UntranslatedCache.ContainsKey(key!))
        {
            return originalTranslation!;
        }

        foreach (var kvp in s_RegexTranslations)
        {
            var match = kvp.Key.Match(originalTranslation);
            if (!match.Success)
            {
                continue;
            }

            var matchingGroupValues = (from Group grp in match.Groups select grp.Value).Skip(1).ToList();

            return s_SmartFormatter.Format(s_RussianCultureInfo, kvp.Value, new { Original = originalTranslation, Matches = matchingGroupValues });
        }

        if (shouldIgnoreKey)
        {
            s_IgnoredTranslations.Add(key!);
            s_IgnoredTranslations.Add(originalTranslation!);

            return originalTranslation!;
        }

        AddUntranslatedText(key, originalTranslation);
        return originalTranslation!;
    }
}
