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
        m_AutoClearUntranslatedOnAwake = (RTLCPlugin.Instance as BaseUnityPlugin).Config.Bind("General", "ClearUntranslatedOnAwake", true);
    }
}
