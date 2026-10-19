using InventoryEnhancer.Models;

namespace InventoryEnhancer.Helpers;

public static class FilterCategoryHelper
{
    /// <summary>
    /// Modifier filters narrow results (AND logic). Category filters broaden results (OR logic).
    /// </summary>
    public static bool IsModifier(FilterCategory category) => category >= FilterCategory.TierHigh;

    // Weapons follow the usage classes of Cultured Start Reloaded's Start Editor weapon slots, so
    // spears and couchable lances are separate filters. Melee splits into one-handed, two-handed and
    // polearms, and thrown weapons get their own group beside Shields & Ranged, which keeps every group
    // near the size of its siblings. Culture has no vanilla filter tab, so it gets a plain text header
    // like Sort's groups instead of a borrowed icon.
    public static FilterGroup GetGroup(FilterCategory category) => category switch
    {
        FilterCategory.TwoHandedSword => FilterGroup.WeaponsTwoHanded,
        FilterCategory.TwoHandedAxe => FilterGroup.WeaponsTwoHanded,
        FilterCategory.Spear => FilterGroup.Polearms,
        FilterCategory.Lance => FilterGroup.Polearms,
        FilterCategory.ThrowingAxe => FilterGroup.Thrown,
        FilterCategory.Javelin => FilterGroup.Thrown,
        FilterCategory.ThrowingKnife => FilterGroup.Thrown,
        FilterCategory.Stones => FilterGroup.Thrown,
        FilterCategory.Bow => FilterGroup.ShieldsAndRanged,
        FilterCategory.Crossbow => FilterGroup.ShieldsAndRanged,
        FilterCategory.Arrows => FilterGroup.ShieldsAndRanged,
        FilterCategory.Bolts => FilterGroup.ShieldsAndRanged,
        FilterCategory.Shield => FilterGroup.ShieldsAndRanged,
        FilterCategory.HeadArmor => FilterGroup.Armor,
        FilterCategory.ShoulderArmor => FilterGroup.Armor,
        FilterCategory.BodyArmor => FilterGroup.Armor,
        FilterCategory.ArmArmor => FilterGroup.Armor,
        FilterCategory.LegArmor => FilterGroup.Armor,
        FilterCategory.Mounts => FilterGroup.Mounts,
        FilterCategory.HorseArmor => FilterGroup.Mounts,
        FilterCategory.Banner => FilterGroup.Misc,
        FilterCategory.Food => FilterGroup.Misc,
        FilterCategory.Goods => FilterGroup.Misc,
        FilterCategory.Vlandian => FilterGroup.Culture,
        FilterCategory.Sturgian => FilterGroup.Culture,
        FilterCategory.Imperial => FilterGroup.Culture,
        FilterCategory.Battanian => FilterGroup.Culture,
        FilterCategory.Khuzait => FilterGroup.Culture,
        FilterCategory.Aserai => FilterGroup.Culture,
        _ => FilterGroup.WeaponsOneHanded,
    };
}
