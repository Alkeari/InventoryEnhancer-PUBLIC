using System.Collections.Generic;
using InventoryEnhancer.Models;

namespace InventoryEnhancer.Services;

/// <summary>
///     The per-side choices the in-screen controls edit, and the step that turns them into the engine
///     state the filter and sort patches read.
/// </summary>
public static class SelectionStore
{
    public static SideSelections Current { get; } = new();

    public static void ApplyToEngines()
    {
        SortEngine.LeftSort = Current.EffectiveSort(SideSelection.Left);
        SortEngine.LeftSortAscending = Current.EffectiveSortAscending(SideSelection.Left);
        SortEngine.RightSort = Current.EffectiveSort(SideSelection.Right);
        SortEngine.RightSortAscending = Current.EffectiveSortAscending(SideSelection.Right);
        FilterEngine.LeftFilters = new HashSet<FilterCategory>(Current.EffectiveFilters(SideSelection.Left));
        FilterEngine.RightFilters = new HashSet<FilterCategory>(Current.EffectiveFilters(SideSelection.Right));
    }
}
