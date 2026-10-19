using System;
using InventoryEnhancer.Helpers;
using InventoryEnhancer.Models;
using InventoryEnhancer.Patches;
using InventoryEnhancer.Services;
using SandBox.GauntletUI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.ScreenSystem;

namespace InventoryEnhancer.Behaviors;

/// <summary>
///     Clears the per-screen caches when the inventory screen closes, and the player's selections too
///     unless Remember Selections is on. The controls themselves live in the vanilla screen's own
///     markup and view model, so nothing here has to open or attach anything.
/// </summary>
internal sealed class Behavior : CampaignBehaviorBase
{
    private static Behavior? _current;

    public override void RegisterEvents()
    {
        InventoryEnhancerLogger.Log(">>> Behavior.RegisterEvents");
        try
        {
            _current?.UnregisterStaticEvents();
            _current = this;

            ScreenManager.OnPopScreen += OnScreenPopped;
            InventoryEnhancerLogger.Log("<<< Behavior.RegisterEvents [SUCCESS]");
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("Behavior.RegisterEvents FAILED", ex);
        }
    }

    private static void OnScreenPopped(ScreenBase poppedScreen)
    {
        if (poppedScreen is GauntletInventoryScreen)
            OnInventoryClosed();
    }

    private static void OnInventoryClosed()
    {
        InventoryEnhancerLogger.Log(">>> OnInventoryClosed");
        try
        {
            ItemCache.ClearCache();
            SortPatch.ResetTracking();

            if (Settings.Settings.Instance == null || !Settings.Settings.Instance.PersistSelections)
            {
                SelectionStore.Current.Clear();
                SortEngine.LeftSort = SortCategory.None;
                SortEngine.RightSort = SortCategory.None;
                SortEngine.LeftSortAscending = false;
                SortEngine.RightSortAscending = false;
                FilterEngine.LeftFilters.Clear();
                FilterEngine.RightFilters.Clear();
                InventoryEnhancerLogger.Log("  Selections cleared (persistence disabled).");
            }

            InventoryEnhancerLogger.Log("<<< OnInventoryClosed [SUCCESS]");
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("OnInventoryClosed FAILED", ex);
        }
    }

    public override void SyncData(IDataStore dataStore)
    {
    }

    private void UnregisterStaticEvents()
    {
        try
        {
            ScreenManager.OnPopScreen -= OnScreenPopped;
            OnInventoryClosed();
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("UnregisterStaticEvents FAILED", ex);
        }
    }
}
