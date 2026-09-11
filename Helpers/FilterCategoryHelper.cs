using InventoryEnhancer.Models;

namespace InventoryEnhancer.Helpers;

public static class FilterCategoryHelper
{
    /// <summary>
    /// Modifier filters narrow results (AND logic). Category filters broaden results (OR logic).
    /// </summary>
    public static bool IsModifier(FilterCategory category) => category >= FilterCategory.TierHigh;

    // Mirrors the vanilla inventory's own filter-tab groupings (Weapons / Shields & Ranged /
    // Armors / Mounts / Misc), so each group can carry the exact icon brush the base game uses
    // for it. Weapons is further split into one-handed and two-handed sub-groups under that same
    // icon: at 9 categories it was nearly double any sibling group's size. Culture is this mod's
    // own addition (no vanilla filter tab for it), so it gets a plain text header like Sort's
    // groups instead of a borrowed icon.
    public static FilterGroup GetGroup(FilterCategory category) => category switch
    {
        FilterCategory.TwoHandedSwords => FilterGroup.WeaponsTwoHanded,
        FilterCategory.TwoHandedAxes => FilterGroup.WeaponsTwoHanded,
        FilterCategory.TwoHandedMaces => FilterGroup.WeaponsTwoHanded,
        FilterCategory.TwoHandedPolearms => FilterGroup.WeaponsTwoHanded,
        FilterCategory.Bows => FilterGroup.ShieldsAndRanged,
        FilterCategory.Crossbows => FilterGroup.ShieldsAndRanged,
        FilterCategory.ThrowingWeapons => FilterGroup.ShieldsAndRanged,
        FilterCategory.Ammunition => FilterGroup.ShieldsAndRanged,
        FilterCategory.Shields => FilterGroup.ShieldsAndRanged,
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
