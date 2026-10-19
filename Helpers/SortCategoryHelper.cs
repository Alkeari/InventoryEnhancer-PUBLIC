using InventoryEnhancer.Models;

namespace InventoryEnhancer.Helpers;

public static class SortCategoryHelper
{
    // Groups the 13 exposed sort categories into the same section rhythm the Filter
    // popup already uses (icon-headed groups), minus icons: Sort has no per-category
    // vanilla icon to borrow, unlike Filter's five real filter-tab brushes.
    public static SortGroup GetGroup(SortCategory category) => category switch
    {
        SortCategory.Damage => SortGroup.Combat,
        SortCategory.Accuracy => SortGroup.Combat,
        SortCategory.Reach => SortGroup.Combat,
        SortCategory.MissileSpeed => SortGroup.Combat,
        SortCategory.Speed => SortGroup.Combat,
        SortCategory.Handling => SortGroup.Combat,
        SortCategory.HitPoints => SortGroup.Defense,
        SortCategory.ArmorCombined => SortGroup.Defense,
        SortCategory.ArmorHighest => SortGroup.Defense,
        _ => SortGroup.General,
    };
}
