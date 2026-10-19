using System;
using System.Reflection;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using HarmonyLib;
using InventoryEnhancer.Helpers;
using InventoryEnhancer.Models;
using InventoryEnhancer.Services;
using InventoryEnhancer.UI.ViewModels;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Library;

namespace InventoryEnhancer.UI.Extensions;

/// <summary>
///     One Sort &amp; Filter panel per side, exposed on the vanilla inventory view model so the markup
///     injected into the vanilla screen can bind to it.
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
    private static readonly MethodInfo? ProcessFilterMethod = AccessTools.Method(typeof(SPInventoryVM), "ProcessFilter");

    // SPInventoryVM.Filters.All is 0: a full re-filter, which the sort postfix also runs after.
    private static readonly object[] ProcessFilterAllArgs = { 0 };

    public InventoryScreenMixin(SPInventoryVM viewModel) : base(viewModel)
    {
        InvEnhLeftPanel = new SidePanelVM(SideSelection.Left, SelectionStore.Current, OnPanelApplied);
        InvEnhRightPanel = new SidePanelVM(SideSelection.Right, SelectionStore.Current, OnPanelApplied);

        // The mixin is built once the view model has filled both item lists, which makes this the
        // first point at which every item the screen will filter or sort exists.
        ItemCache.InitializeCache(viewModel);
    }

    [DataSourceProperty] public SidePanelVM InvEnhLeftPanel { get; }

    [DataSourceProperty] public SidePanelVM InvEnhRightPanel { get; }

    public override void OnRefresh()
    {
        InvEnhLeftPanel.RefreshState();
        InvEnhRightPanel.RefreshState();
    }

    private void OnPanelApplied()
    {
        try
        {
            SelectionStore.ApplyToEngines();
            if (ViewModel != null)
                ProcessFilterMethod?.Invoke(ViewModel, ProcessFilterAllArgs);

            InvEnhLeftPanel.RefreshState();
            InvEnhRightPanel.RefreshState();
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("Applying a side's sort or filter failed", ex);
        }
    }
}
