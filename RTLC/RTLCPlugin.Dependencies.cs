using BepInEx;

namespace RTLC;
[BepInDependency(KnownPluginDependency.c_HDLethalCompany, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(KnownPluginDependency.c_ShipLoot, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(KnownPluginDependency.c_LethalExpansionCore, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(KnownPluginDependency.c_LethalExpansion, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(KnownPluginDependency.c_LethalThings, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(KnownPluginDependency.c_FacilityMeltdown, BepInDependency.DependencyFlags.SoftDependency)]
public partial class RTLCPlugin
{
}

internal static class KnownPluginDependency
{
    /// <summary>
    /// <para>https://github.com/Sligili/HDLethalCompany/</para>
    /// Additional graphics settings for Lethal Company, such as resolution, anti-aliasing. fog quality etc.
    /// </summary>
    public const string c_HDLethalCompany = "HDLethalCompany"; // see UpdateScanNode patch for the reason

    /// <summary>
    /// <para>https://thunderstore.io/c/lethal-company/p/tinyhoot/ShipLoot/</para>
    /// Reliably shows the total value of all scrap in your ship.
    /// </summary>
    public const string c_ShipLoot = "com.github.tinyhoot.ShipLoot";

    /// <summary>
    /// <para>https://thunderstore.io/c/lethal-company/p/jockie/LethalExpansionCore/</para>
    /// Fork of <see cref="c_LethalExpansion"/> to only include LethalSDK (modding tool for devs)
    /// </summary>
    public const string c_LethalExpansionCore = "com.github.lethalmods.lethalexpansioncore";

    /// <summary>
    /// <para>https://thunderstore.io/c/lethal-company/p/HolographicWings/LethalExpansion/</para>
    /// Adds LethalSDK and a lot of other modding modules.
    /// </summary>
    public const string c_LethalExpansion = "LethalExpansion";

    /// <summary>
    /// <para>https://thunderstore.io/c/lethal-company/p/Evaisa/LethalThings/</para>
    /// Adds 11 scrap, 6 store items, 1 enemy, 4 decor, 1 map hazard, and 1 game mechanic.
    /// </summary>
    public const string c_LethalThings = "evaisa.lethalthings";

    /// <summary>
    /// <para>https://thunderstore.io/c/lethal-company/p/loaforc/FacilityMeltdown/</para>
    /// Meltdown is a bad idea..
    /// </summary>
    public const string c_FacilityMeltdown = "me.loaforc.facilitymeltdown";
}
