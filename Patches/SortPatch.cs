using System;
using InventoryEnhancer.Helpers;
using InventoryEnhancer.Models;
using InventoryEnhancer.Services;
using HarmonyLib;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Library;

namespace InventoryEnhancer.Patches;

public static class SortPatch
{
    private static bool _patched;

    // Performance: Track last sort operation to avoid redundant sorts
    private static int _lastLeftListHash;
    private static int _lastRightListHash;
    private static SortCategory _lastLeftSort = SortCategory.None;
    private static SortCategory _lastRightSort = SortCategory.None;
    private static bool _lastLeftAscending;
    private static bool _lastRightAscending;
    private static DateTime _lastSortTime = DateTime.MinValue;
    private static readonly TimeSpan SortDebounceInterval = TimeSpan.FromMilliseconds(10);

    internal static void Apply(Harmony harmony)
    {
        if (_patched) return;
        _patched = true;

        try
        {
            var processFilter = AccessTools.Method(typeof(SPInventoryVM), "ProcessFilter");
            if (processFilter != null)
            {
                var sortPostfix = AccessTools.Method(typeof(SortPatch), nameof(ProcessFilterPostfix));
                harmony.Patch(processFilter, postfix: new HarmonyMethod(sortPostfix) { priority = Priority.Last });
            }
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("Failed to apply SortPatch Harmony patches", ex);
        }
    }

    public static void ResetTracking()
    {
        _lastLeftListHash = 0;
        _lastRightListHash = 0;
        _lastLeftSort = SortCategory.None;
        _lastRightSort = SortCategory.None;
        _lastLeftAscending = false;
        _lastRightAscending = false;
        _lastSortTime = DateTime.MinValue;
    }

    private static void ProcessFilterPostfix(SPInventoryVM __instance)
    {
        try
        {
            if (Settings.Settings.Instance != null && (!Settings.Settings.Instance.IsModEnabled || !Settings.Settings.Instance.EnableSorting)) return;

            var now = DateTime.UtcNow;
            if (now - _lastSortTime < SortDebounceInterval)
                return;

            bool leftNeedsSort = false;
            bool rightNeedsSort = false;

            if (SortEngine.LeftSort != SortCategory.None && __instance.LeftItemListVM != null)
            {
                int currentHash = ComputeListContentHash(__instance.LeftItemListVM);
                leftNeedsSort = currentHash != _lastLeftListHash ||
                                SortEngine.LeftSort != _lastLeftSort ||
                                SortEngine.LeftSortAscending != _lastLeftAscending;
                if (leftNeedsSort)
                {
                    _lastLeftListHash = currentHash;
                    _lastLeftSort = SortEngine.LeftSort;
                    _lastLeftAscending = SortEngine.LeftSortAscending;
                }
            }

            if (SortEngine.RightSort != SortCategory.None && __instance.RightItemListVM != null)
            {
                int currentHash = ComputeListContentHash(__instance.RightItemListVM);
                rightNeedsSort = currentHash != _lastRightListHash ||
                                 SortEngine.RightSort != _lastRightSort ||
                                 SortEngine.RightSortAscending != _lastRightAscending;
                if (rightNeedsSort)
                {
                    _lastRightListHash = currentHash;
                    _lastRightSort = SortEngine.RightSort;
                    _lastRightAscending = SortEngine.RightSortAscending;
                }
            }

            if (leftNeedsSort && __instance.LeftItemListVM != null)
                SortEngine.SortList(__instance.LeftItemListVM, SortEngine.LeftSort, SortEngine.LeftSortAscending);

            if (rightNeedsSort && __instance.RightItemListVM != null)
                SortEngine.SortList(__instance.RightItemListVM, SortEngine.RightSort, SortEngine.RightSortAscending);

            if (leftNeedsSort || rightNeedsSort)
                _lastSortTime = now;
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("SortPatch.ProcessFilterPostfix failed", ex);
        }
    }

    private static int ComputeListContentHash(MBBindingList<SPItemVM> list)
    {
        if (list == null || list.Count == 0) return 0;

        int visibleCount = 0;
        int firstVisibleHash = 0;
        int lastVisibleHash = 0;

        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].IsFiltered)
            {
                visibleCount++;
                int itemHash = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(list[i]);
                if (firstVisibleHash == 0)
                    firstVisibleHash = itemHash;
                lastVisibleHash = itemHash;
            }
        }

        return HashCode.Combine(list.Count, visibleCount, firstVisibleHash, lastVisibleHash);
    }
}
