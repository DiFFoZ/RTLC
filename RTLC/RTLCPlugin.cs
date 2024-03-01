﻿using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using RTLC.API;

namespace RTLC;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public partial class RTLCPlugin : BaseUnityPlugin
{
    internal static RTLCPlugin Instance { get; private set; } = null!;

    internal new ManualLogSource Logger { get; private set; } = null!;

    internal new RTLCConfiguration Config { get; private set; } = null!;

    internal string WorkingDirectory { get; private set; } = null!;

    internal Harmony Harmony { get; private set; } = null!;

    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;
        WorkingDirectory = new FileInfo(Info.Location).DirectoryName;

        Config = new RTLCConfiguration();
        Config.Initialize();

        foreach (var method in typeof(RTLCPlugin)
            .Assembly
            .GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(m => m.GetCustomAttribute<InitializeOnAwakeAttribute>() != null))
        {
            method.Invoke(null, null);
            Logger.LogInfo($"Initialized {method.DeclaringType.Namespace}.{method.Name}");
        }

        // HarmonyFileLog.Enabled = true;

        Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        Harmony.PatchAll(typeof(RTLCPlugin).Assembly);

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}
