using System;
using System.Reflection;
using InventoryEnhancer.Helpers;
using InventoryEnhancer.Models;
using InventoryEnhancer.Patches;
using InventoryEnhancer.Services;
using InventoryEnhancer.UI;
using HarmonyLib;
using SandBox.GauntletUI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;
using TaleWorlds.Localization;

namespace InventoryEnhancer.Behaviors;

internal sealed class Behavior : CampaignBehaviorBase
{
    private static SPInventoryVM? _currentInventoryVM;

    // Lazy on purpose: TutorialContextChangedEvent(InventoryScreen) fires before
    // GauntletInventoryScreen.OnReady() assigns _dataSource, so a one-shot capture
    // in OnInventoryOpened loses that race and never gets a value. Resolving on
    // every access lets it succeed as soon as the screen finishes initializing.
    public static SPInventoryVM? CurrentInventoryVM
    {
        get
        {
            TryCaptureInventoryVM(_current?._inventoryScreen);
            return _currentInventoryVM;
        }
    }

    private static readonly FieldInfo? _dataSourceField =
        AccessTools.Field(typeof(GauntletInventoryScreen), "_dataSource");

    private static bool TryCaptureInventoryVM(GauntletInventoryScreen? screen)
    {
        if (_currentInventoryVM != null) return true;

        if (screen != null && _dataSourceField?.GetValue(screen) is SPInventoryVM inventoryVM)
        {
            _currentInventoryVM = inventoryVM;
            ItemCache.InitializeCache(inventoryVM);
            InventoryEnhancerLogger.Log($"  Captured SPInventoryVM. Left={inventoryVM.LeftItemListVM?.Count ?? 0}, Right={inventoryVM.RightItemListVM?.Count ?? 0}");
            return true;
        }

        return false;
    }

    private static Behavior? _current;

    private GauntletInventoryScreen? _inventoryScreen;
    private Layer? _enhancerLayer;
    private bool _layerAttached;

    public override void RegisterEvents()
    {
        InventoryEnhancerLogger.Log(">>> Behavior.RegisterEvents");
        try
        {
            _current?.UnregisterStaticEvents();
            _current = this;

            Game.Current.EventManager.RegisterEvent<TutorialContextChangedEvent>(OnTutorialContextChanged);
            InventoryEnhancerLogger.Log("  TutorialContextChangedEvent registered.");

            ScreenManager.OnPushScreen += OnScreenPushed;
            ScreenManager.OnPopScreen += OnScreenPopped;
            InventoryEnhancerLogger.Log("  Screen push/pop events registered.");

            InventoryEnhancerLogger.Log("<<< Behavior.RegisterEvents [SUCCESS]");
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("Behavior.RegisterEvents FAILED", ex);
        }
    }

    private void OnTutorialContextChanged(TutorialContextChangedEvent contextEvent)
    {
        InventoryEnhancerLogger.Log($">>> OnTutorialContextChanged (NewContext={contextEvent.NewContext})");
        try
        {
            // TutorialContexts.InventoryScreen == 2
            if (contextEvent.NewContext == TutorialContexts.InventoryScreen)
            {
                OnInventoryOpened();
            }
            InventoryEnhancerLogger.Log("<<< OnTutorialContextChanged [SUCCESS]");
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("OnTutorialContextChanged FAILED", ex);
            var errorMsg = new TextObject("{=InvEnh_Msg_Error}Inventory Enhancer: Error - {ERROR_MESSAGE}");
            errorMsg.SetTextVariable("ERROR_MESSAGE", ex.Message);
            InformationManager.DisplayMessage(new InformationMessage(errorMsg.ToString(), Colors.Red));
        }
    }

    private void OnScreenPushed(ScreenBase pushedScreen)
    {
        InventoryEnhancerLogger.Log($">>> OnScreenPushed ({pushedScreen?.GetType().Name})");
    }

    private void OnScreenPopped(ScreenBase poppedScreen)
    {
        InventoryEnhancerLogger.Log($">>> OnScreenPopped ({poppedScreen?.GetType().Name})");
        try
        {
            if (poppedScreen == _inventoryScreen)
            {
                OnInventoryClosed();
            }
            else if (_inventoryScreen != null && _enhancerLayer != null &&
                     ScreenManager.TopScreen == _inventoryScreen)
            {
                InventoryEnhancerLogger.Log("  Returning to inventory screen - refreshing ViewModel.");
                _enhancerLayer.RefreshViewModel();
            }
            InventoryEnhancerLogger.Log("<<< OnScreenPopped [SUCCESS]");
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("OnScreenPopped FAILED", ex);
        }
    }

    private void OnInventoryOpened()
    {
        InventoryEnhancerLogger.Log(">>> OnInventoryOpened");
        try
        {
            var topScreen = ScreenManager.TopScreen;
            InventoryEnhancerLogger.Log($"  TopScreen type: {topScreen?.GetType().Name ?? "null"}");

            if (topScreen is not GauntletInventoryScreen inventoryScreen)
            {
                InventoryEnhancerLogger.Log("  TopScreen is not GauntletInventoryScreen - aborting.");
                return;
            }

            if (_inventoryScreen == inventoryScreen && _layerAttached && _enhancerLayer != null)
            {
                InventoryEnhancerLogger.Log("  Layer already attached to this screen.");
                return;
            }

            _inventoryScreen = inventoryScreen;

            InventoryEnhancerLogger.Log($"  _dataSourceField: {(_dataSourceField != null ? "found" : "NULL")}");
            if (!TryCaptureInventoryVM(_inventoryScreen))
            {
                InventoryEnhancerLogger.Log("  SPInventoryVM not yet assigned by the screen - will capture lazily on first use.");
            }

            InventoryEnhancerLogger.Log("  Creating Layer...");
            _enhancerLayer = new Layer(1000);
            InventoryEnhancerLogger.Log("  Layer created. Adding to screen...");
            _inventoryScreen.AddLayer(_enhancerLayer);
            _layerAttached = true;
            InventoryEnhancerLogger.Log("  Layer added to screen.");

            _enhancerLayer.InputRestrictions.SetInputRestrictions(false);
            InventoryEnhancerLogger.Log("<<< OnInventoryOpened [SUCCESS]");
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("OnInventoryOpened FAILED", ex);
            var errorMsg = new TextObject("{=InvEnh_Msg_Error}Inventory Enhancer: Error - {ERROR_MESSAGE}");
            errorMsg.SetTextVariable("ERROR_MESSAGE", ex.Message);
            InformationManager.DisplayMessage(new InformationMessage(errorMsg.ToString(), Colors.Red));
        }
    }

    private void OnInventoryClosed()
    {
        InventoryEnhancerLogger.Log(">>> OnInventoryClosed");
        try
        {
            // CRITICAL: Do NOT call RemoveLayer here. The screen is being popped/torn down
            // by the engine. Calling RemoveLayer on a screen that's mid-teardown causes
            // native access violations (0xC0000005) in the GauntletUI rendering pipeline.
            // The engine will clean up the screen's layers when it disposes the screen.
            if (_inventoryScreen != null && _enhancerLayer != null && _layerAttached)
            {
                InventoryEnhancerLogger.Log("  Inventory screen closing - releasing references (engine handles layer cleanup).");
            }

            _enhancerLayer = null;
            _inventoryScreen = null;
            _layerAttached = false;
            _currentInventoryVM = null;
            ItemCache.ClearCache();
            SortPatch.ResetTracking();

            if (Settings.Settings.Instance == null || !Settings.Settings.Instance.PersistSelections)
            {
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
        InventoryEnhancerLogger.Log(">>> UnregisterStaticEvents");
        try
        {
            ScreenManager.OnPushScreen -= OnScreenPushed;
            ScreenManager.OnPopScreen -= OnScreenPopped;
            OnInventoryClosed();
            InventoryEnhancerLogger.Log("<<< UnregisterStaticEvents [SUCCESS]");
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("UnregisterStaticEvents FAILED", ex);
        }
    }
}
