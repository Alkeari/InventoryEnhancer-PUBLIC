using System;
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
            case FilterCategory.Dagger:
                return weaponClass == WeaponClass.Dagger;
            case FilterCategory.OneHandedSword:
                return weaponClass == WeaponClass.OneHandedSword;
            case FilterCategory.OneHandedAxe:
                return weaponClass == WeaponClass.OneHandedAxe;
            case FilterCategory.Mace:
                return weaponClass is WeaponClass.Mace or WeaponClass.Pick;
            case FilterCategory.TwoHandedSword:
                return weaponClass == WeaponClass.TwoHandedSword;
            case FilterCategory.TwoHandedAxe:
                return weaponClass is WeaponClass.TwoHandedAxe or WeaponClass.TwoHandedMace;
            case FilterCategory.Spear:
                return IsPolearm(weaponClass) && !IsCouchable(item);
            case FilterCategory.Lance:
                return IsPolearm(weaponClass) && IsCouchable(item);
            case FilterCategory.ThrowingAxe:
                return weaponClass == WeaponClass.ThrowingAxe;
            case FilterCategory.Javelin:
                return weaponClass == WeaponClass.Javelin;
            case FilterCategory.ThrowingKnife:
                return weaponClass == WeaponClass.ThrowingKnife;
            case FilterCategory.Stones:
                return weaponClass == WeaponClass.Stone;
            case FilterCategory.Bow:
                return weaponClass == WeaponClass.Bow;
            case FilterCategory.Crossbow:
                return weaponClass == WeaponClass.Crossbow;
            case FilterCategory.Arrows:
                return weaponClass == WeaponClass.Arrow;
            case FilterCategory.Bolts:
                return weaponClass == WeaponClass.Bolt;
            case FilterCategory.Shield:
                return type == ItemObject.ItemTypeEnum.Shield;
            case FilterCategory.ArmArmor:
                return type == ItemObject.ItemTypeEnum.HandArmor;
            case FilterCategory.Banner:
                return item.HasBannerComponent;
            case FilterCategory.BodyArmor:
                return type == ItemObject.ItemTypeEnum.BodyArmor;
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
            case FilterCategory.ShoulderArmor:
                return type == ItemObject.ItemTypeEnum.Cape;
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

    private static bool IsPolearm(WeaponClass? weaponClass) =>
        weaponClass is WeaponClass.OneHandedPolearm or WeaponClass.TwoHandedPolearm or WeaponClass.LowGripPolearm;

    // A lance is a polearm with the vanilla "couch" usage in any of its weapon modes, whatever technical
    // class the item database gives it; every other polearm is a spear.
    private static bool IsCouchable(ItemObject item)
    {
        if (item.Weapons == null) return false;

        foreach (var weapon in item.Weapons)
        {
            var usage = weapon?.ItemUsage;
            if (usage != null && usage.IndexOf("couch", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }
}
