using System;
using System.Collections.Generic;
using System.Linq;
using InventoryEnhancer.Models;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace InventoryEnhancer.UI.ViewModels;

public class SortMenuVM : ViewModel
{
    private bool _isVisible;
    private SideSelection _selectedSide = SideSelection.Both;
    private bool _isAscending = false;

    [DataSourceProperty]
    public HintViewModel AscendingHint { get; } = new(new TextObject("{=InvEnh_Hint_Ascending}Sort from lowest to highest."));

    [DataSourceProperty]
    public HintViewModel DescendingHint { get; } = new(new TextObject("{=InvEnh_Hint_Descending}Sort from highest to lowest."));

    [DataSourceProperty]
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (value != _isVisible)
            {
                _isVisible = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    // Grouped the same way Filter's categories are: three section headers (General / Combat /
    // Defense) so the 13 sort options get the same chunking discipline the Filter popup already
    // has, instead of one flat scrolling list. No icons here - unlike Filter's real vanilla
    // filter-tab brushes, there is no per-category vanilla icon for these groups to borrow.
    [DataSourceProperty]
    public MBBindingList<SortOptionVM> GeneralOptions { get; } = new();

    [DataSourceProperty]
    public MBBindingList<SortOptionVM> CombatOptions { get; } = new();

    [DataSourceProperty]
    public MBBindingList<SortOptionVM> DefenseOptions { get; } = new();

    [DataSourceProperty]
    public MBBindingList<SideOptionVM> SideOptions { get; } = new();

    // Sort's own list uses the identical checkbox visual Filter's true multi-select lists use,
    // but behaves as single-select (picking one deselects the last). This header states that
    // plainly, the same way Filter's own "Match Any"/"Then Narrow By" headers state its AND/OR
    // logic, rather than relying on the checkbox shape alone to communicate it.
    [DataSourceProperty]
    public string ChooseOneHeaderText => new TextObject("{=InvEnh_Sort_ChooseOneHeader}Choose One").ToString();

    [DataSourceProperty]
    public string GeneralHeaderText => new TextObject("{=InvEnh_Sort_GeneralHeader}General").ToString();

    [DataSourceProperty]
    public string CombatHeaderText => new TextObject("{=InvEnh_Sort_CombatHeader}Combat").ToString();

    [DataSourceProperty]
    public string DefenseHeaderText => new TextObject("{=InvEnh_Sort_DefenseHeader}Defense").ToString();

    public IEnumerable<SortOptionVM> AllOptions() =>
        GeneralOptions.Concat(CombatOptions).Concat(DefenseOptions);

    public SortMenuVM()
    {
        SideOptions.Add(new SideOptionVM(
            new TextObject("{=InvEnh_UI_Left}Left").ToString(), SideSelection.Left,
            new TextObject("{=InvEnh_Hint_SortLeft}Sort only the left inventory."), OnSideSelected, isSelected: false));
        SideOptions.Add(new SideOptionVM(
            new TextObject("{=InvEnh_UI_Both}Both").ToString(), SideSelection.Both,
            new TextObject("{=InvEnh_Hint_SortBoth}Sort both inventories."), OnSideSelected, isSelected: true));
        SideOptions.Add(new SideOptionVM(
            new TextObject("{=InvEnh_UI_Right}Right").ToString(), SideSelection.Right,
            new TextObject("{=InvEnh_Hint_SortRight}Sort only the right inventory."), OnSideSelected, isSelected: false));
    }

    private void OnSideSelected(SideOptionVM selected)
    {
        SelectedSide = selected.Side;
    }

    #region Ascending/Descending

    [DataSourceProperty]
    public bool IsAscending
    {
        get => _isAscending;
        set
        {
            if (_isAscending != value)
            {
                _isAscending = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDescending));
            }
        }
    }

    [DataSourceProperty]
    public bool IsDescending => !_isAscending;

    [DataSourceProperty]
    public string AscendingText => new TextObject("{=InvEnh_Sort_Ascending}Ascending").ToString();

    [DataSourceProperty]
    public string DescendingText => new TextObject("{=InvEnh_Sort_Descending}Descending").ToString();

    public void ExecuteSetAscending()
    {
        IsAscending = true;
    }

    public void ExecuteSetDescending()
    {
        IsAscending = false;
    }

    #endregion

    public SideSelection SelectedSide
    {
        get => _selectedSide;
        set
        {
            if (_selectedSide != value)
            {
                _selectedSide = value;
                foreach (var option in SideOptions)
                    option.IsSelected = option.Side == value;
            }
        }
    }

    #region Text Properties (bound by XML inside DataSource="{SortMenu}")

    [DataSourceProperty]
    public string SortOptionsText => new TextObject("{=InvEnh_UI_SortOptions}Sort Options").ToString();

    #endregion

    public void Toggle() => IsVisible = !IsVisible;
    public void Close() => IsVisible = false;
}
