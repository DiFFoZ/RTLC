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
internal static partial class Translation
{
    private const char c_LineFeedChar = '\n';
    private const char c_CarriageReturnChar = '\r';
    private const char c_EmptySpaceChar = (char)0x200b;
    private const char c_SpaceChar = ' ';

    private static readonly Dictionary<string, string> s_Translations = [];
    private static readonly Dictionary<Regex, string> s_RegexTranslations = [];
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
            .AddExtensions(new StringToNumberSource(), new TranslationSource(), new SelectorTextToStringSource(), new ReplaceNewLinesSource(), new AppendStringSource());

        s_SmartFormatter.RemoveFormatterExtension<IsMatchFormatter>();
        s_SmartFormatter.AddExtensions(new IsMatchFormatterEx(), new IsMatchesFormatterEx());
    }

    [InitializeOnAwake]
    public static void LoadTranslation()
    {
        var directory = Path.Combine(RTLCPlugin.Instance.WorkingDirectory, "Translations");
        s_UntranslatedFilePath = Path.Combine(directory, "Untranslated.json");

        CreateUntranslatedFile();

        foreach (var translationFile in Directory.EnumerateFiles(directory, "*.json", SearchOption.AllDirectories))
        {
            var fileName = Path.GetFileName(translationFile);

            if (fileName == "Untranslated.json")
            {
                continue;
            }

            ReadTranslationFile(translationFile);
        }
    }

    private static void ReadTranslationFile(string? translationFile)
    {
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
        }
    }

    private static void CreateUntranslatedFile()
    {
        if (File.Exists(s_UntranslatedFilePath))
        {
            if (RTLCPlugin.Instance.Config.AutoClearUntranslatedOnAwake.Value)
            {
                File.Delete(s_UntranslatedFilePath);
                s_UntranslatedCache = [];
            }
            else
            {
                s_UntranslatedCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(s_UntranslatedFilePath)) ?? [];
            }
        }
    }

    private static void AddTranslation(string key, string translation)
    {
        if (s_Translations.ContainsKey(key))
        {
            RTLCPlugin.Instance.Logger.LogWarning($"{key} already exists.");
            return;
        }

        s_Translations[key] = translation;
    }

    private static void AddUntranslatedText(string? key, string? originalTranslation = null)
    {
        if (s_UntranslatedCache.ContainsKey(key!))
        {
            return;
        }

        s_UntranslatedCache[key!] = originalTranslation ?? key!;

        File.WriteAllText(s_UntranslatedFilePath, JsonConvert.SerializeObject(s_UntranslatedCache, Formatting.Indented));
    }

    private static bool ShouldIgnoreTranslation(string? text)
    {
        const int minLength = 3;

        if (string.IsNullOrWhiteSpace(text) || text.Length < minLength)
        {
            return true;
        }

        // probably used for keybinds and terminal vars. e.g:
        // [totalCost]
        // [playerCredits]
        if (text.StartsWith('[') && (text.EndsWith(']') || text.EndsWith("]\n")) && text.IndexOf(c_SpaceChar) == -1)
        {
            return true;
        }

        var sameChar = text[0];
        var errorCharCount = -1;

        foreach (var c in text.AsSpan())
        {
            if (c == c_LineFeedChar
                || c == c_CarriageReturnChar
                || c == c_EmptySpaceChar
                || c == c_SpaceChar
                || c == sameChar
                || (c >= 0x0 && c <= 0x3F)
                || (c >= 0x5B && c <= 0x60)
                || (c >= 0x7B && c <= 0x7F)
                || (c >= 0x400 && c <= 0x4FF))
            {
                errorCharCount++;
            }
        }

        return (text.Length - errorCharCount) < minLength;
    }
}
