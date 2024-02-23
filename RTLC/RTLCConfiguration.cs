using BepInEx;
using BepInEx.Configuration;

namespace RTLC;
internal class RTLCConfiguration
{
    private ConfigEntry<bool> m_AutoClearUntranslatedOnAwake = null!;

    public bool AutoClearUntranslatedOnAwake
    {
        get => m_AutoClearUntranslatedOnAwake.Value;
        set => m_AutoClearUntranslatedOnAwake.Value = value;
    }

    public void Initialize()
    {
        var config = (RTLCPlugin.Instance as BaseUnityPlugin).Config;

        m_AutoClearUntranslatedOnAwake = config.Bind("General", "ClearUntranslatedOnAwake", true);
    }
}
