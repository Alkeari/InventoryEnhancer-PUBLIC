using System;
using System.Collections.Generic;
using InventoryEnhancer.Helpers;
using InventoryEnhancer.Models;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Library;

namespace InventoryEnhancer.Services;

internal class CachedItemValues
{
    public float[] SortValues;
    public ulong FilterMatchBits; // Bit flags for filter matches (faster than bool array)
    public ulong FilterComputedBits; // Which filters have been computed
    public string? DisplayName; // Cached display name for fast sorting

    public CachedItemValues(int sortCategoryCount)
    {
        SortValues = new float[sortCategoryCount];
        // Initialize with NaN to indicate "not computed"
        for (int i = 0; i < SortValues.Length; i++)
            SortValues[i] = float.NaN;
    }
}

public static class ItemCache
{
    private static readonly int SortCategoryCount = Enum.GetValues(typeof(SortCategory)).Length;
    private static readonly int FilterCategoryCount = Enum.GetValues(typeof(FilterCategory)).Length;

    // High-performance cache using object hash codes for fast lookup
    // Key: RuntimeHelpers.GetHashCode(SPItemVM) for stable identity
    internal static Dictionary<int, CachedItemValues> Cache = new(4096);

    /// <summary>
    /// Initialize cache and pre-compute ALL values for instant sorting/filtering later.
    /// This is called when inventory opens.
    /// </summary>
    public static void InitializeCache(SPInventoryVM? vm)
    {
        if (vm == null) return;

        try
        {
            if (vm.LeftItemListVM != null) PreComputeAllValues(vm.LeftItemListVM);
            if (vm.RightItemListVM != null) PreComputeAllValues(vm.RightItemListVM);
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("Error during cache initialization", ex);
        }
    }

    public static void ClearCache()
    {
        Cache.Clear();
    }

    /// <summary>
    /// Pre-computes ALL sort values and filter matches for all items.
    /// This runs once when inventory opens so sort/filter operations are instant.
    /// </summary>
    private static void PreComputeAllValues(MBBindingList<SPItemVM> items)
    {
        foreach (var itemVM in items)
        {
            try
            {
                if (itemVM == null) continue;

                int key = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(itemVM);
                if (Cache.ContainsKey(key)) continue;

                var cached = new CachedItemValues(SortCategoryCount);
                Cache[key] = cached;

                // Pre-compute display name (used for Name sort) - with null safety
                var item = itemVM.ItemRosterElement.EquipmentElement.Item;
                if (item?.Name != null)
                {
                    string rawName = item.Name.ToString();
                    if (!string.IsNullOrEmpty(rawName) && rawName.StartsWith("{="))
                    {
                        int closingBrace = rawName.IndexOf('}');
                        if (closingBrace >= 0 && closingBrace < rawName.Length - 1)
                            cached.DisplayName = rawName.Substring(closingBrace + 1);
                        else
                            cached.DisplayName = rawName;
                    }
                    else
                    {
                        cached.DisplayName = rawName ?? string.Empty;
                    }
                }
                else
                {
                    cached.DisplayName = string.Empty;
                }

                // Pre-compute ALL sort values upfront
                for (int i = 1; i < SortCategoryCount; i++) // Skip None (0)
                {
                    try
                    {
                        cached.SortValues[i] = SortEngine.CalculateSortValue(itemVM, (SortCategory)i);
                    }
                    catch
                    {
                        cached.SortValues[i] = 0;
                    }
                }

                // Pre-compute ALL filter matches upfront
                for (int i = 1; i < FilterCategoryCount; i++) // Skip None (0)
                {
                    try
                    {
                        bool matches = FilterEngine.CalculateFilterMatch(itemVM, (FilterCategory)i);
                        if (matches)
                            cached.FilterMatchBits |= (1UL << i);
                        cached.FilterComputedBits |= (1UL << i);
                    }
                    catch
                    {
                        cached.FilterComputedBits |= (1UL << i);
                    }
                }
            }
            catch
            {
                continue;
            }
        }
    }
}
