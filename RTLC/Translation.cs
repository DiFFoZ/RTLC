using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DunGen;
using Newtonsoft.Json;
using RTLC.API;
using RTLC.SmartFormat;
using SmartFormat;
using SmartFormat.Core.Settings;

namespace RTLC;
internal static class Translation
{
    private static readonly Dictionary<string, string> s_Translations = [];
    private static readonly Dictionary<Regex, string> s_RegexTranslations = [];
    private static readonly CultureInfo s_RussianCultureInfo = new("ru-RU");

    private static Dictionary<string, string> s_UntranslatedCache = [];
    private static string s_UntranslatedFilePath = string.Empty;
    private static readonly SmartFormatter s_SmartFormatter = Smart.CreateDefaultSmartFormat().AddExtensions(new StringToNumberSource());

    [InitializeOnAwake]
    public static void LoadTranslation()
    {
        SmartSettings.IsThreadSafeMode = false;

        var directory = Path.Combine(RTLCPlugin.Instance.WorkingDirectory, "Translations");
        s_UntranslatedFilePath = Path.Combine(directory, "Untranslated.json");

        if (File.Exists(s_UntranslatedFilePath))
        {
            s_UntranslatedCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(s_UntranslatedFilePath))!;
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
                if (kvp.Key.StartsWith("r:"))
                {
                    s_RegexTranslations.Add(new Regex(kvp.Key[2..]), kvp.Value);
                    continue;
                }

                if (!s_Translations.ContainsKey(kvp.Key))
                    s_Translations[kvp.Key] = kvp.Value;
            }
        }
    }

    internal static string GetLocalizedText(string key)
    {
        if (ShouldIgnoreTranslation(key))
        {
            return key;
        }

        if (s_Translations.TryGetValue(key, out var translation))
        {
            return s_SmartFormatter.Format(s_RussianCultureInfo, translation, new { Original = key, Translations = s_Translations });
        }

        if (s_UntranslatedCache.ContainsKey(key))
        {
            return key;
        }

        foreach (var kvp in s_RegexTranslations)
        {
            var match = kvp.Key.Match(key);
            if (!match.Success)
            {
                continue;
            }

            var matchingGroupValues = (from Group grp in match.Groups select grp.Value).ToList();

            return s_SmartFormatter.Format(s_RussianCultureInfo, kvp.Value, new { Original = key, Translations = s_Translations, Matches = matchingGroupValues });
        }

        AddUntranslatedText(key);
        return key;
    }

    private static void AddUntranslatedText(string text)
    {
        if (s_UntranslatedCache.ContainsKey(text))
        {
            return;
        }

        s_UntranslatedCache[text] = text;

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
