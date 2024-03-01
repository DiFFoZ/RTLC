using BepInEx;

namespace RTLC;
[BepInDependency(KnownPluginDependency.c_HDLethalCompany, BepInDependency.DependencyFlags.SoftDependency)] // see UpdateScanNode patch for the reason
[BepInDependency(KnownPluginDependency.c_ShipLoot, BepInDependency.DependencyFlags.SoftDependency)]
public partial class RTLCPlugin
{
}

internal static class KnownPluginDependency
{
    // https://github.com/Sligili/HDLethalCompany
    public const string c_HDLethalCompany = "HDLethalCompany";

    // https://thunderstore.io/c/lethal-company/p/tinyhoot/ShipLoot/
    public const string c_ShipLoot = "com.github.tinyhoot.ShipLoot";
}
