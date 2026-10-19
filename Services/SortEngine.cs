using System;
using System.Collections.Generic;
using InventoryEnhancer.Models;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace InventoryEnhancer.Services;

public static class SortEngine
{
    // Active sort for each side
    public static SortCategory LeftSort = SortCategory.None;
    public static SortCategory RightSort = SortCategory.None;
    public static bool LeftSortAscending = false;
    public static bool RightSortAscending = false;

    public static void SortList(MBBindingList<SPItemVM> list, SortCategory sortCategory, bool ascending = false)
    {
        if (sortCategory == SortCategory.None || list == null || list.Count <= 1) return;

        int totalCount = list.Count;

        // Extract visible and filtered items into arrays (fast)
        var allItems = new SPItemVM[totalCount];
        var visibleIndices = new List<int>(totalCount);

        for (int i = 0; i < totalCount; i++)
        {
            allItems[i] = list[i];
            if (!list[i].IsFiltered)
                visibleIndices.Add(i);
        }

        int visibleCount = visibleIndices.Count;
        if (visibleCount <= 1) return;

        // Create arrays for efficient sorting
        var visibleItems = new SPItemVM[visibleCount];
        var sortKeys = new float[visibleCount];
        string[]? nameKeys = null;

        bool isNameSort = sortCategory == SortCategory.Name;
        if (isNameSort)
            nameKeys = new string[visibleCount];

        // Extract visible items and their pre-computed sort keys
        for (int i = 0; i < visibleCount; i++)
        {
            var item = allItems[visibleIndices[i]];
            visibleItems[i] = item;

            int key = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(item);
            if (ItemCache.Cache.TryGetValue(key, out var cached))
            {
                if (isNameSort)
                    nameKeys![i] = cached.DisplayName ?? string.Empty;
                else
                    sortKeys[i] = cached.SortValues[(int)sortCategory];
            }
            else
            {
                if (isNameSort)
                    nameKeys![i] = GetDisplayNameFast(item);
                else
                    sortKeys[i] = CalculateSortValue(item, sortCategory);
            }
        }

        // Sort using Array.Sort with keys (very fast - avoids repeated comparisons)
        if (isNameSort)
        {
            Array.Sort(nameKeys!, visibleItems, StringComparer.OrdinalIgnoreCase);
            if (!ascending)
                Array.Reverse(visibleItems);
        }
        else
        {
            Array.Sort(sortKeys, visibleItems);
            if (!ascending)
                Array.Reverse(visibleItems);
        }

        // Check if order actually changed
        bool changed = false;
        int visibleIdx = 0;
        for (int i = 0; i < totalCount && visibleIdx < visibleCount; i++)
        {
            if (!allItems[i].IsFiltered)
            {
                if (allItems[i] != visibleItems[visibleIdx])
                {
                    changed = true;
                    break;
                }
                visibleIdx++;
            }
        }

        if (!changed) return;

        // Rebuild final order: filtered items stay in place, visible items in sorted order
        var finalOrder = new SPItemVM[totalCount];
        visibleIdx = 0;
        for (int i = 0; i < totalCount; i++)
        {
            if (!allItems[i].IsFiltered)
                finalOrder[i] = visibleItems[visibleIdx++];
            else
                finalOrder[i] = allItems[i];
        }

        // Clear and re-add to trigger MBBindingList UI notifications
        list.Clear();
        for (int i = 0; i < totalCount; i++)
            list.Add(finalOrder[i]);
    }

    internal static string GetDisplayNameFast(SPItemVM itemVM)
    {
        var item = itemVM.ItemRosterElement.EquipmentElement.Item;
        if (item?.Name == null) return string.Empty;

        string rawName = item.Name.ToString();
        if (string.IsNullOrEmpty(rawName)) return string.Empty;

        if (rawName.StartsWith("{="))
        {
            int closingBrace = rawName.IndexOf('}');
            if (closingBrace >= 0 && closingBrace < rawName.Length - 1)
                return rawName.Substring(closingBrace + 1);
        }

        return rawName;
    }

    internal static float GetCachedSortValue(SPItemVM itemVM, SortCategory category)
    {
        int key = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(itemVM);
        if (ItemCache.Cache.TryGetValue(key, out var cached))
        {
            return cached.SortValues[(int)category];
        }
        return CalculateSortValue(itemVM, category);
    }

    internal static float CalculateSortValue(SPItemVM itemVM, SortCategory category)
    {
        // Handle Count specially - uses ItemCount directly
        if (category == SortCategory.Count)
            return itemVM.ItemCount;

        var ee = itemVM.ItemRosterElement.EquipmentElement;
        var item = ee.Item;
        if (item == null) return 0;

        var weapon = item.PrimaryWeapon;
        var horse = item.HorseComponent;
        var modifier = ee.ItemModifier;

        switch (category)
        {
            case SortCategory.Name:
                return 0;
            case SortCategory.Type:
                return CampaignUIHelper.GetItemObjectTypeSortIndex(item);
            case SortCategory.Value:
                return ee.ItemValue;
            case SortCategory.ValuePerWeight:
                {
                    float w = item.Weight;
                    float v = ee.ItemValue;
                    if (w > 0f)
                        return v / w;
                    return v > 0 ? float.PositiveInfinity : 0f;
                }
            case SortCategory.Tier:
                return (int)item.Tier;
            case SortCategory.Accuracy:
                return weapon != null ? weapon.Accuracy : 0;
            case SortCategory.ArmorCombined:
                if (item.ItemType == ItemObject.ItemTypeEnum.HorseHarness)
                    return ee.GetModifiedMountBodyArmor();
                return ee.GetModifiedHeadArmor() + ee.GetModifiedBodyArmor() + ee.GetModifiedArmArmor() + ee.GetModifiedLegArmor();
            case SortCategory.ArmorHighest:
                if (item.ItemType == ItemObject.ItemTypeEnum.HorseHarness)
                    return ee.GetModifiedMountBodyArmor();
                return Math.Max(Math.Max(ee.GetModifiedHeadArmor(), ee.GetModifiedBodyArmor()), Math.Max(ee.GetModifiedArmArmor(), ee.GetModifiedLegArmor()));
            case SortCategory.Damage:
                if (weapon == null) return 0;
                float thrust = weapon.GetModifiedThrustDamage(modifier);
                float swing = weapon.GetModifiedSwingDamage(modifier);
                return thrust > swing ? thrust : swing;
            case SortCategory.Handling:
                return weapon != null ? weapon.GetModifiedHandling(modifier) : 0;
            case SortCategory.HitPoints:
                if (item.ItemType == ItemObject.ItemTypeEnum.Shield)
                    return weapon != null ? weapon.GetModifiedStackCount(modifier) : 0;
                if (horse != null)
                    return ee.GetModifiedMountHitPoints();
                return 0;
            case SortCategory.MissileSpeed:
                return weapon != null ? weapon.GetModifiedMissileSpeed(modifier) : 0;
            case SortCategory.Reach:
                return weapon?.WeaponLength ?? 0;
            case SortCategory.Speed:
                if (horse != null) return ee.GetModifiedMountSpeed(in EquipmentElement.Invalid);
                if (weapon == null) return 0;
                float thrustSpeed = weapon.GetModifiedThrustSpeed(modifier);
                float swingSpeed = weapon.GetModifiedSwingSpeed(modifier);
                return thrustSpeed > swingSpeed ? thrustSpeed : swingSpeed;
            case SortCategory.Weight:
                return item.Weight;
            default:
                return 0;
        }
    }
}
