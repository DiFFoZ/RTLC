using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using RTLC.API;

namespace RTLC;
internal static class Translation
{
    private static readonly Dictionary<string, string> s_Translations = [];

    private static Dictionary<string, string> s_UntranslatedCache = [];
    private static string s_UntranslatedFilePath = string.Empty;

    [InitializeOnAwake]
    public static void LoadTranslation()
    {
        var directory = Path.Combine(RTLCPlugin.Instance.WorkingDirectory, "Translations");
        s_UntranslatedFilePath = Path.Combine(directory, "Untranslated.json");

        if (File.Exists(s_UntranslatedFilePath))
        {
            s_UntranslatedCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(s_UntranslatedFilePath))!;
        }

        foreach (var translationFile in Directory.EnumerateFiles(directory, "*.json", SearchOption.AllDirectories))
        {
            var dict = JsonConvert.DeserializeObject<IDictionary<string, string>>(File.ReadAllText(translationFile))!;
            foreach (var kvp in dict)
            {
                if (!s_Translations.ContainsKey(kvp.Key))
                    s_Translations[kvp.Key] = kvp.Value;
            }
        }
    }

    internal static string GetLocalizedText(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length < 3)
        {
            return key;
        }

        if (s_Translations.TryGetValue(key, out var translation))
        {
            return translation;
        }

        RTLCPlugin.Instance.Logger.LogWarning("Failed to find localization for: " + key.Replace("\n", @"\n"));

        AddUntranslatedText(key);
        return key;
    }

    private static void AddUntranslatedText(string text)
    {
        if (s_UntranslatedCache.ContainsKey(text))
        {
            return;
        }

        s_Translations[text] = text;
        s_UntranslatedCache[text] = text;

        File.WriteAllText(s_UntranslatedFilePath, JsonConvert.SerializeObject(s_UntranslatedCache));
    }
}
