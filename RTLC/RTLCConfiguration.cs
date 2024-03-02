using BepInEx;
using BepInEx.Configuration;

namespace RTLC;
internal class RTLCConfiguration
{
    public ConfigEntry<bool> AutoClearUntranslatedOnAwake { get; private set; }

    public RTLCConfiguration()
    {
        var config = (RTLCPlugin.Instance as BaseUnityPlugin).Config;

        AutoClearUntranslatedOnAwake = config.Bind("General", "ClearUntranslatedOnAwake", true);
    }
}
