using System;
using InventoryEnhancer.Models;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace InventoryEnhancer.UI.ViewModels;

public class FilterOptionVM : ViewModel
{
    private bool _isSelected;
    private readonly Action<FilterOptionVM> _onToggle;

    [DataSourceProperty]
    public string Name { get; }

    public FilterCategory Category { get; }

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

    public void ExecuteToggle()
    {
        IsSelected = !IsSelected;
        _onToggle(this);
    }

    public FilterOptionVM(string name, FilterCategory category, TextObject hintText, Action<FilterOptionVM> onToggle, bool isSelected = false)
    {
        Name = name;
        Category = category;
        Hint = new HintViewModel(hintText);
        _onToggle = onToggle;
        _isSelected = isSelected;
    }
}
