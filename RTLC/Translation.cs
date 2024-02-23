using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RTLC.API;
using RTLC.SmartFormat;
using SmartFormat;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace RTLC;
internal static class Translation
{
    private static readonly Dictionary<string, string> s_Translations = [];
    private static readonly Dictionary<Regex, string> s_RegexTranslations = [];
    private static readonly HashSet<string> s_IgnoredTranslations = [];
    private static readonly CultureInfo s_RussianCultureInfo = new("ru-RU");
    private static readonly SmartFormatter s_SmartFormatter;

    private static Dictionary<string, string> s_UntranslatedCache = [];
    private static string s_UntranslatedFilePath = string.Empty;

    static Translation()
    {
        SmartSettings.IsThreadSafeMode = false;

        var parser = new ParserSettings();
        parser.AddCustomSelectorChars(['"']);

        var settings = new SmartSettings() { Parser = parser };

        s_SmartFormatter = Smart.CreateDefaultSmartFormat(settings)
            .AddExtensions(new StringToNumberSource(), new TranslationSource(), new SelectorTextToStringSource(), new ReplaceNewLinesSource());

        s_SmartFormatter.RemoveFormatterExtension<IsMatchFormatter>();
        s_SmartFormatter.AddExtensions(new IsMatchFormatterEx(), new IsMatchesFormatterEx());
    }

    [InitializeOnAwake]
    public static void LoadTranslation()
    {
        var directory = Path.Combine(RTLCPlugin.Instance.WorkingDirectory, "Translations");
        s_UntranslatedFilePath = Path.Combine(directory, "Untranslated.json");

        if (File.Exists(s_UntranslatedFilePath))
        {
            if (RTLCPlugin.Instance.Config.AutoClearUntranslatedOnAwake)
            {
                File.Delete(s_UntranslatedFilePath);
                s_UntranslatedCache = [];
            }
            else
            {
                s_UntranslatedCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(s_UntranslatedFilePath)) ?? [];
            }
        }

        foreach (var translationFile in Directory.EnumerateFiles(directory, "*.json", SearchOption.AllDirectories))
        {
            if (translationFile == s_UntranslatedFilePath)
            {
                continue;
            }

            var dict = JsonConvert.DeserializeObject<IDictionary<string, string>>(File.ReadAllText(translationFile))!;
            foreach (var kvp in dict)
            {
                // ignore comments
                if (kvp.Key.StartsWith("__"))
                {
                    continue;
                }

                // regex
                if (kvp.Key.StartsWith("r:"))
                {
                    s_RegexTranslations.Add(new Regex(kvp.Key[2..], RegexOptions.Compiled | RegexOptions.Multiline, TimeSpan.FromMilliseconds(500)), kvp.Value);
                    continue;
                }

                var originalTranslation = kvp.Key;
                // reference key
                if (originalTranslation.StartsWith("key:"))
                {
                    var slice = originalTranslation.AsSpan().Slice(4);
                    var index = slice.IndexOf(':');

                    if (index == -1)
                    {
                        RTLCPlugin.Instance.Logger.LogWarning($"{kvp.Key} has invalid key reference. Check the file: {Path.GetFileName(translationFile)}");
                        continue;
                    }

                    originalTranslation = slice.Slice(index + 1).ToString();
                    var key = slice.Slice(0, index).ToString();

                    AddTranslation(key, kvp.Value);
                }

                AddTranslation(originalTranslation, kvp.Value);

                void AddTranslation(string key, string translation)
                {
                    if (s_Translations.ContainsKey(key))
                    {
                        RTLCPlugin.Instance.Logger.LogWarning($"{key} already exists. Check the file for duplicate: {Path.GetFileName(translationFile)}");
                        return;
                    }

                    s_Translations[key] = translation;
                }
            }
        }
    }

    internal static string GetLocalizedText(string key) => GetLocalizedText(key, false);

    internal static string GetLocalizedText(string key, bool shouldIgnoreKey) => GetLocalizedText(key, key, shouldIgnoreKey);

    internal static string GetLocalizedText(string key, string originalTranslation) => GetLocalizedText(key, originalTranslation, false);

    internal static string GetLocalizedText(string key, string originalTranslation, bool shouldIgnoreKey)
    {
        if (ShouldIgnoreTranslation(originalTranslation)
            || s_Translations.ContainsValue(key)
            || s_IgnoredTranslations.Contains(key)
            || s_IgnoredTranslations.Contains(originalTranslation))
        {
            return originalTranslation;
        }

        if (s_Translations.TryGetValue(key, out var translation))
        {
            return s_SmartFormatter.Format(s_RussianCultureInfo, translation, new { Original = originalTranslation });
        }

        if (s_UntranslatedCache.ContainsKey(key))
        {
            return originalTranslation;
        }

        foreach (var kvp in s_RegexTranslations)
        {
            var match = kvp.Key.Match(originalTranslation);
            if (!match.Success)
            {
                continue;
            }

            var matchingGroupValues = (from Group grp in match.Groups select grp.Value).ToList();
            matchingGroupValues.RemoveAt(0);

            return s_SmartFormatter.Format(s_RussianCultureInfo, kvp.Value, new { Original = originalTranslation, Matches = matchingGroupValues });
        }

        if (shouldIgnoreKey)
        {
            s_IgnoredTranslations.Add(key);
            s_IgnoredTranslations.Add(originalTranslation);

            return originalTranslation;
        }

        AddUntranslatedText(key, originalTranslation);
        return originalTranslation;
    }

    private static void AddUntranslatedText(string key, string? originalTranslation = null)
    {
        if (s_UntranslatedCache.ContainsKey(key))
        {
            return;
        }

        s_UntranslatedCache[key] = originalTranslation ?? key;

        File.WriteAllText(s_UntranslatedFilePath, JsonConvert.SerializeObject(s_UntranslatedCache));
    }

    private const char c_LineFeedChar = '\n';
    private const char c_CarriageReturnChar = '\r';
    private const char c_EmptySpaceChar = (char)0x200b;
    private const char c_SpaceChar = ' ';

    private static bool ShouldIgnoreTranslation(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length < 4)
        {
            return true;
        }

        var @char = text[0];
        var errorCharCount = 0;

        foreach (var c in text.AsSpan())
        {
            if (c == c_LineFeedChar
                || c == c_CarriageReturnChar
                || c == c_EmptySpaceChar
                || c == c_SpaceChar
                || c == @char
                || (c >= (char)0x0400 && c <= (char)0x04ff))
            {
                errorCharCount++;
                continue;
            }

            break;
        }

        return errorCharCount == text.Length;
    }
}
