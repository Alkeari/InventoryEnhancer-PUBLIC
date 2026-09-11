using System;
using System.Linq;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using InventoryEnhancer.Helpers;
using InventoryEnhancer.Models;
using InventoryEnhancer.Services;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace InventoryEnhancer.UI.Extensions;

/// <summary>
///     Phase 1 of the migration to UIExtenderEx: the mod's own filter and sort state, exposed on the
///     vanilla inventory view model so vanilla markup can bind to it.
///     The refresh hook is <c>ProcessFilter</c>, not one of the Refresh methods. <c>RefreshValues</c>
///     is invoked by the engine from <c>GauntletLayer.UpdateLayout</c>, so it tracks layout changes
///     rather than state changes, and nothing inside the view model calls it after its constructor.
///     <c>ProcessFilter</c> is what every filter button press and every search keystroke reaches, so
///     it is the point at which this mod's state has actually changed.
///     Every member is prefixed, because UIExtenderEx writes mixin members into a shared dictionary
///     by name and the last mod to load wins silently. Two other installed mods already mix into this
///     same view model.
/// </summary>
[ViewModelMixin("ProcessFilter", true)]
public sealed class InventoryScreenMixin : BaseViewModelMixin<SPInventoryVM>
{
    public InventoryScreenMixin(SPInventoryVM viewModel) : base(viewModel)
    {
        Refresh();
    }

    [DataSourceProperty] public bool InvEnhHasBar { get; private set; }

    [DataSourceProperty] public string InvEnhLeftStatus { get; private set; } = string.Empty;

    [DataSourceProperty] public string InvEnhRightStatus { get; private set; } = string.Empty;

    public override void OnRefresh()
    {
        Refresh();

        ViewModel?.OnPropertyChanged(nameof(InvEnhHasBar));
        ViewModel?.OnPropertyChangedWithValue(InvEnhLeftStatus, nameof(InvEnhLeftStatus));
        ViewModel?.OnPropertyChangedWithValue(InvEnhRightStatus, nameof(InvEnhRightStatus));
    }

    private void Refresh()
    {
        try
        {
            var settings = Settings.Settings.Instance;

            InvEnhHasBar = settings is { IsModEnabled: true, ShowNativeStatusBar: true };
            if (!InvEnhHasBar)
            {
                InvEnhLeftStatus = string.Empty;
                InvEnhRightStatus = string.Empty;
                return;
            }

            InvEnhLeftStatus = Describe(FilterEngine.LeftFilters.Count, SortEngine.LeftSort);
            InvEnhRightStatus = Describe(FilterEngine.RightFilters.Count, SortEngine.RightSort);
        }
        catch (Exception ex)
        {
            InvEnhHasBar = false;
            InventoryEnhancerLogger.LogError("Refreshing the inventory mixin failed; the bar is hidden", ex);
        }
    }

    private static string Describe(int filterCount, SortCategory sort)
    {
        if (filterCount == 0 && sort == SortCategory.None)
            return new TextObject("{=InvEnh_Bar_Nothing}No filter, no sort").ToString();

        var text = new TextObject("{=InvEnh_Bar_Status}{COUNT} filters, sorted by {SORT}");
        text.SetTextVariable("COUNT", filterCount);
        text.SetTextVariable("SORT", SortName(sort));

        return text.ToString();
    }

    private static TextObject SortName(SortCategory sort)
    {
        return sort == SortCategory.None
            ? new TextObject("{=InvEnh_Bar_NoSort}nothing")
            : new TextObject("{=!}" + sort);
    }
}
