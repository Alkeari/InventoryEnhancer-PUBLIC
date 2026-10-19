using System.Collections.Generic;

namespace InventoryEnhancer.Models;

public sealed class SideChoice
{
    public SortCategory Sort { get; set; } = SortCategory.None;

    public bool SortAscending { get; set; }

    public HashSet<FilterCategory> Filters { get; } = new();

    public void Clear()
    {
        Sort = SortCategory.None;
        SortAscending = false;
        Filters.Clear();
    }
}
