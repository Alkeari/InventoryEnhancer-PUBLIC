namespace InventoryEnhancer.Models;

public enum FilterCategory
{
    None,
    // Category filters (OR logic: show items matching ANY selected category)
    // Weapons (Melee)
    Daggers,
    OneHandedSwords,
    OneHandedAxes,
    OneHandedMaces,
    OneHandedPolearms,
    TwoHandedSwords,
    TwoHandedAxes,
    TwoHandedMaces,
    TwoHandedPolearms,
    // Weapons (Ranged)
    Bows,
    Crossbows,
    ThrowingWeapons,
    Ammunition,
    // Defense
    Shields,
    HeadArmor,
    ShoulderArmor,
    BodyArmor,
    ArmArmor,
    LegArmor,
    // Mounts
    Mounts,
    HorseArmor,
    // Misc
    Banner,
    Food,
    Goods,
    // Culture (the item's country of origin, e.g. for troop-tree/roleplay use, not the player's own culture)
    Vlandian,
    Sturgian,
    Imperial,
    Battanian,
    Khuzait,
    Aserai,

    // Modifier filters (AND logic: narrow results from category filters)
    TierHigh,
    TierLow,
    ValueHigh,
    ValueLow,
}
