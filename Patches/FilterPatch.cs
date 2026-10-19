using System;
using InventoryEnhancer.Helpers;
using InventoryEnhancer.Models;
using InventoryEnhancer.Services;
using HarmonyLib;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;

namespace InventoryEnhancer.Patches;

public static class FilterPatch
{
    private static bool _patched;

    internal static void Apply(Harmony harmony)
    {
        if (_patched) return;
        _patched = true;

        try
        {
            var original = AccessTools.Method(typeof(SPInventoryVM), "UpdateFilteredStatusOfItem");
            var postfix = AccessTools.Method(typeof(FilterPatch), nameof(UpdateFilteredStatusOfItemPostfix));
            harmony.Patch(original, postfix: new HarmonyMethod(postfix) { priority = Priority.Last });
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("Failed to apply FilterPatch Harmony patches", ex);
        }
    }

    /// <summary>
    ///     Runs once per item on every filter pass, so an exception here would surface inside the
    ///     game's own loop. Every patch body is guarded, and a filter that
    ///     cannot decide leaves the item as the game left it rather than taking the screen down.
    /// </summary>
    private static void UpdateFilteredStatusOfItemPostfix(SPItemVM item)
    {
        try
        {
            Filter(item);
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("FilterPatch postfix failed; the item is left as the game filtered it", ex);
        }
    }

    private static void Filter(SPItemVM item)
    {
        if (Settings.Settings.Instance != null && (!Settings.Settings.Instance.IsModEnabled || !Settings.Settings.Instance.EnableFiltering)) return;

        bool isLeft = item.InventorySide == InventoryLogic.InventorySide.OtherInventory;
        var activeFilters = isLeft ? FilterEngine.LeftFilters : FilterEngine.RightFilters;

        if (activeFilters.Count == 0) return;

        // If the game already hid the item (e.g. search text doesn't match), keep it hidden
        if (item.IsFiltered) return;

        // Smart filter logic:
        // - Category filters (item types) use OR: "Daggers + Body Armor" shows daggers AND body armor
        // - Modifier filters (tier/value) use AND: "Body Armor + High Tier" shows only high-tier body armor
        bool hasCategories = false;
        bool matchesAnyCategory = false;

        foreach (var filter in activeFilters)
        {
            if (FilterCategoryHelper.IsModifier(filter))
            {
                // Modifiers must ALL match (AND)
                if (!FilterEngine.MatchesFilter(item, filter))
                {
                    item.IsFiltered = true;
                    return;
                }
            }
            else
            {
                // Categories: track if ANY match (OR)
                hasCategories = true;
                if (FilterEngine.MatchesFilter(item, filter))
                    matchesAnyCategory = true;
            }
        }

        // If category filters were selected but none matched, hide the item
        if (hasCategories && !matchesAnyCategory)
        {
            item.IsFiltered = true;
        }
    }
}
