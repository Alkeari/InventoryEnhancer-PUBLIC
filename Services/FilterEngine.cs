using System.Collections.Generic;
using InventoryEnhancer.Models;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;

namespace InventoryEnhancer.Services;

public static class FilterEngine
{
    // Active filters for each side
    public static HashSet<FilterCategory> LeftFilters = new();
    public static HashSet<FilterCategory> RightFilters = new();

    public static bool MatchesFilter(SPItemVM? itemVM, FilterCategory filter)
    {
        if (itemVM == null) return false;

        int key = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(itemVM);
        if (ItemCache.Cache.TryGetValue(key, out var cached))
        {
            ulong filterBit = 1UL << (int)filter;
            return (cached.FilterMatchBits & filterBit) != 0;
        }

        // Fallback: compute on demand if not in cache
        return CalculateFilterMatch(itemVM, filter);
    }

    internal static bool CalculateFilterMatch(SPItemVM itemVM, FilterCategory filter)
    {
        var item = itemVM.ItemRosterElement.EquipmentElement.Item;
        if (item == null) return false;

        var type = item.ItemType;
        var weaponClass = item.PrimaryWeapon?.WeaponClass;

        switch (filter)
        {
            case FilterCategory.Ammunition:
                return type == ItemObject.ItemTypeEnum.Arrows || type == ItemObject.ItemTypeEnum.Bolts;
            case FilterCategory.ArmArmor:
                return type == ItemObject.ItemTypeEnum.HandArmor;
            case FilterCategory.Banner:
                return item.HasBannerComponent;
            case FilterCategory.BodyArmor:
                return type == ItemObject.ItemTypeEnum.BodyArmor;
            case FilterCategory.Bows:
                return type == ItemObject.ItemTypeEnum.Bow;
            case FilterCategory.Crossbows:
                return type == ItemObject.ItemTypeEnum.Crossbow;
            case FilterCategory.Daggers:
                return weaponClass == WeaponClass.Dagger;
            case FilterCategory.Food:
                return item.IsFood;
            case FilterCategory.HeadArmor:
                return type == ItemObject.ItemTypeEnum.HeadArmor;
            case FilterCategory.HorseArmor:
                return type == ItemObject.ItemTypeEnum.HorseHarness;
            case FilterCategory.LegArmor:
                return type == ItemObject.ItemTypeEnum.LegArmor;
            case FilterCategory.Mounts:
                return type == ItemObject.ItemTypeEnum.Horse;
            case FilterCategory.OneHandedAxes:
                return weaponClass == WeaponClass.OneHandedAxe;
            case FilterCategory.OneHandedMaces:
                return weaponClass == WeaponClass.Mace;
            case FilterCategory.OneHandedPolearms:
                return weaponClass == WeaponClass.OneHandedPolearm;
            case FilterCategory.OneHandedSwords:
                return weaponClass == WeaponClass.OneHandedSword;
            case FilterCategory.Shields:
                return type == ItemObject.ItemTypeEnum.Shield;
            case FilterCategory.ShoulderArmor:
                return type == ItemObject.ItemTypeEnum.Cape;
            case FilterCategory.ThrowingWeapons:
                return type == ItemObject.ItemTypeEnum.Thrown;
            case FilterCategory.TwoHandedAxes:
                return weaponClass == WeaponClass.TwoHandedAxe;
            case FilterCategory.TwoHandedMaces:
                return weaponClass == WeaponClass.TwoHandedMace;
            case FilterCategory.TwoHandedPolearms:
                return weaponClass == WeaponClass.TwoHandedPolearm;
            case FilterCategory.TwoHandedSwords:
                return weaponClass == WeaponClass.TwoHandedSword;
            case FilterCategory.ValueHigh:
                return item.Value >= 1000;
            case FilterCategory.ValueLow:
                return item.Value < 100;
            case FilterCategory.TierHigh:
                return (int)item.Tier >= 5;
            case FilterCategory.TierLow:
                return (int)item.Tier <= 2;
            case FilterCategory.Goods:
                return (type == ItemObject.ItemTypeEnum.Goods ||
                        (item.ItemCategory != null && item.ItemCategory.IsTradeGood)) &&
                       type != ItemObject.ItemTypeEnum.Horse;
            case FilterCategory.Vlandian:
                return item.Culture?.StringId == "vlandia";
            case FilterCategory.Sturgian:
                return item.Culture?.StringId == "sturgia";
            case FilterCategory.Imperial:
                return item.Culture?.StringId == "empire";
            case FilterCategory.Battanian:
                return item.Culture?.StringId == "battania";
            case FilterCategory.Khuzait:
                return item.Culture?.StringId == "khuzait";
            case FilterCategory.Aserai:
                return item.Culture?.StringId == "aserai";
            default:
                return false;
        }
    }
}
