using System.Collections.Generic;
using InventoryEnhancer.Models;

namespace InventoryEnhancer.Services;

/// <summary>
///     Each side's own sort and filter choice, and what each side's list actually shows once a
///     "Both Sides" link is taken into account. A locked side's own choice is never overwritten, so
///     releasing the link shows that side's earlier selections again.
/// </summary>
public sealed class SideSelections
{
    public SideLink Link { get; } = new();

    public SideChoice Left { get; } = new();

    public SideChoice Right { get; } = new();

    public SideChoice ChoiceOf(SideSelection side)
    {
        return side == SideSelection.Right ? Right : Left;
    }

    public SortCategory EffectiveSort(SideSelection side)
    {
        return ChoiceOf(Link.SourceFor(LinkedFeature.Sort, side)).Sort;
    }

    public bool EffectiveSortAscending(SideSelection side)
    {
        return ChoiceOf(Link.SourceFor(LinkedFeature.Sort, side)).SortAscending;
    }

    public IReadOnlyCollection<FilterCategory> EffectiveFilters(SideSelection side)
    {
        return ChoiceOf(Link.SourceFor(LinkedFeature.Filter, side)).Filters;
    }

    public void Clear()
    {
        Link.Clear();
        Left.Clear();
        Right.Clear();
    }
}
