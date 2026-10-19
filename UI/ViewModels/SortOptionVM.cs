using System;
using InventoryEnhancer.Models;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace InventoryEnhancer.UI.ViewModels;

public class SortOptionVM : ViewModel
{
    private bool _isSelected;
    private readonly Action<SortOptionVM> _onSelect;

    [DataSourceProperty]
    public string Name { get; }

    public SortCategory Category { get; }

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
        IsSelected = !IsSelected;
        _onSelect(this);
    }

    public SortOptionVM(string name, SortCategory category, TextObject hintText, Action<SortOptionVM> onSelect, bool isSelected = false)
    {
        Name = name;
        Category = category;
        Hint = new HintViewModel(hintText);
        _onSelect = onSelect;
        _isSelected = isSelected;
    }
}
