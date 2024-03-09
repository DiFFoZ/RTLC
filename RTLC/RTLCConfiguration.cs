using BepInEx;
using BepInEx.Configuration;

namespace RTLC;
public class RTLCConfiguration
{
    public ConfigEntry<bool> AutoClearUntranslatedOnAwake { get; private set; }

    internal RTLCConfiguration()
    {
        var config = (RTLCPlugin.Instance as BaseUnityPlugin).Config;

        AutoClearUntranslatedOnAwake = config.Bind("General", "ClearUntranslatedOnAwake", true);
    }
}
