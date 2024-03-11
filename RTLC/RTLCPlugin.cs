using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using RTLC.API;
using RTLC.Extensions;

namespace RTLC;
// --- MOD GUID: RTLC
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public partial class RTLCPlugin : BaseUnityPlugin
{
    public static RTLCPlugin Instance { get; private set; } = null!;

    public new RTLCConfiguration Config { get; private set; } = null!;

    internal new ManualLogSource Logger { get; private set; } = null!;

    internal string WorkingDirectory { get; private set; } = null!;

    internal Harmony Harmony { get; private set; } = null!;

    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;
        WorkingDirectory = new FileInfo(Info.Location).DirectoryName;
        Config = new RTLCConfiguration();

        foreach (var method in typeof(RTLCPlugin)
            .Assembly
            .GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(m => m.GetCustomAttribute<InitializeOnAwakeAttribute>() != null))
        {
            method.Invoke(null, null);
            Logger.LogInfo($"Initialized {method.GetReadableString()}");
        }

        Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        Harmony.PatchAll(typeof(RTLCPlugin).Assembly);

#if DEBUG
        API.LethalExpansion.LethalExpansionTranslation.TryAddScrapsToUntranslated();
#endif

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}
