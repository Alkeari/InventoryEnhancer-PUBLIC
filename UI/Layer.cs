using System;
using InventoryEnhancer.Helpers;
using InventoryEnhancer.UI.ViewModels;
using TaleWorlds.Engine.GauntletUI;

namespace InventoryEnhancer.UI;

internal sealed class Layer : GauntletLayer
{
    private readonly MainVM _viewModel;

    public MainVM ViewModel => _viewModel;

    // API: GauntletLayer(string name, int localOrder, bool shouldClear = false)
    public Layer(int localOrder, string name = "GauntletLayer")
        : base(name, localOrder)
    {
        _viewModel = new MainVM();

        // Movie name must match file name without extension: InventoryEnhancer.xml
        LoadMovie("InventoryEnhancer", _viewModel);

        InventoryEnhancerLogger.Log("  Layer created and movie loaded.");
    }

    public void RefreshViewModel()
    {
        try
        {
            _viewModel.RefreshValues();
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("Layer.RefreshViewModel FAILED", ex);
        }
    }
}
