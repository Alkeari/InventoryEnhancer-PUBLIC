using System.Collections.Generic;
using InventoryEnhancer.Models;

namespace InventoryEnhancer.Services;

/// <summary>
///     Which side, if any, applies its own selection to both sides, tracked per feature. At most one
///     side holds a feature's link: two sides each pushing their own selection onto both would overwrite
///     each other on every apply.
///     Each side keeps its own selection while locked, so releasing the link hands it back intact.
/// </summary>
public sealed class SideLink
{
    private readonly Dictionary<LinkedFeature, SideSelection> _holders = new();

    public SideSelection? HolderOf(LinkedFeature feature)
    {
        return _holders.TryGetValue(feature, out var holder) ? holder : null;
    }

    public bool IsLinkedFrom(LinkedFeature feature, SideSelection side)
    {
        return HolderOf(feature) == side;
    }

    public bool IsLockedFor(LinkedFeature feature, SideSelection side)
    {
        var holder = HolderOf(feature);
        return holder != null && holder != side;
    }

    public SideSelection SourceFor(LinkedFeature feature, SideSelection side)
    {
        return HolderOf(feature) ?? side;
    }

    public bool TryLink(LinkedFeature feature, SideSelection side)
    {
        var holder = HolderOf(feature);
        if (holder != null)
            return holder == side;

        _holders[feature] = side;
        return true;
    }

    public void Unlink(LinkedFeature feature, SideSelection side)
    {
        if (HolderOf(feature) == side)
            _holders.Remove(feature);
    }

    public void Clear()
    {
        _holders.Clear();
    }
}
