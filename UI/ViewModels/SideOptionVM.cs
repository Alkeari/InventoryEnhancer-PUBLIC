using System;
using InventoryEnhancer.Models;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace InventoryEnhancer.UI.ViewModels;

public class SideOptionVM : ViewModel
{
    private bool _isSelected;
    private readonly Action<SideOptionVM> _onSelect;

    [DataSourceProperty]
    public string Name { get; }

    public SideSelection Side { get; }

    [DataSourceProperty]
    public HintViewModel Hint { get; }

    [DataSourceProperty]
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (value != _isSelected)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public void ExecuteSelect()
    {
        _onSelect(this);
    }

    public SideOptionVM(string name, SideSelection side, TextObject hintText, Action<SideOptionVM> onSelect, bool isSelected)
    {
        Name = name;
        Side = side;
        Hint = new HintViewModel(hintText);
        _onSelect = onSelect;
        _isSelected = isSelected;
    }
}
