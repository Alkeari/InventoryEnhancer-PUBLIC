namespace InventoryEnhancer.Models;

public enum FilterCategory
{
    None,
    // Category filters (OR logic: show items matching ANY selected category)
    // Weapons, by how they are used rather than by the item database's technical class
    Dagger,
    OneHandedSword,
    OneHandedAxe,
    Mace,
    TwoHandedSword,
    TwoHandedAxe,
    Spear,
    Lance,
    ThrowingAxe,
    Javelin,
    ThrowingKnife,
    Stones,
    Bow,
    Crossbow,
    Arrows,
    Bolts,
    Shield,
    // Defense
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
