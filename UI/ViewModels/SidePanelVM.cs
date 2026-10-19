using System;
using System.Collections.Generic;
using InventoryEnhancer.Models;
using InventoryEnhancer.Services;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace InventoryEnhancer.UI.ViewModels;

/// <summary>
///     One inventory side's Sort &amp; Filter menu, opened from that side's square button. The menu
///     stages its selection and writes it to this side only on Apply. "Both Sides" makes this side's
///     sort and filter drive the other side as well; while it does, the other side's button is
///     disabled and its menu cannot open, so the two sides can never both push onto each other.
/// </summary>
public sealed class SidePanelVM : ViewModel
{
    private static readonly LinkedFeature[] Features = { LinkedFeature.Sort, LinkedFeature.Filter };

    private readonly SideSelection _side;
    private readonly SideSelections _selections;
    private readonly Action _onApplied;

    private SortCategory _stagedSort = SortCategory.None;
    private readonly HashSet<FilterCategory> _stagedFilters = new();
    private bool _isBothSides;
    private bool _isMenuOpen;

    public SidePanelVM(SideSelection side, SideSelections selections, Action onApplied)
    {
        _side = side;
        _selections = selections;
        _onApplied = onApplied;
    }

    [DataSourceProperty]
    public SortMenuVM SortMenu { get; } = new();

    [DataSourceProperty]
    public FilterMenuVM FilterMenu { get; } = new();

    private static bool IsModEnabled => Settings.Settings.Instance?.IsModEnabled ?? true;

    [DataSourceProperty]
    public bool IsSortEnabled => IsModEnabled && (Settings.Settings.Instance?.EnableSorting ?? true);

    [DataSourceProperty]
    public bool IsFilterEnabled => IsModEnabled && (Settings.Settings.Instance?.EnableFiltering ?? true);

    [DataSourceProperty]
    public bool IsButtonVisible => IsSortEnabled || IsFilterEnabled;


    [DataSourceProperty]
    public bool IsLocked => _selections.Link.IsLockedFor(LinkedFeature.Sort, _side) || _selections.Link.IsLockedFor(LinkedFeature.Filter, _side);

    [DataSourceProperty]
    public bool IsActive => _selections.EffectiveSort(_side) != SortCategory.None || _selections.EffectiveFilters(_side).Count > 0;

    [DataSourceProperty]
    public bool IsMenuOpen
    {
        get => _isMenuOpen;
        private set
        {
            if (value != _isMenuOpen)
            {
                _isMenuOpen = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsBothSides
    {
        get => _isBothSides;
        set
        {
            if (value != _isBothSides)
            {
                _isBothSides = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string TitleText => new TextObject("{=InvEnh_Side_Title}Sort & Filter").ToString();

    // The menu opens in the middle of the screen for either side, so it names the side it belongs to.
    [DataSourceProperty]
    public string SideText => _side == SideSelection.Left
        ? new TextObject("{=InvEnh_Side_Left}Left Side").ToString()
        : new TextObject("{=InvEnh_Side_Right}Right Side").ToString();

    [DataSourceProperty]
    public string BothSidesText => new TextObject("{=InvEnh_Side_BothSides}Both Sides").ToString();

    [DataSourceProperty]
    public string ResetButtonText => new TextObject("{=InvEnh_UI_Reset}Reset").ToString();

    [DataSourceProperty]
    public string ApplyButtonText => new TextObject("{=InvEnh_UI_Apply}Apply").ToString();

    [DataSourceProperty]
    public string CloseButtonText => new TextObject("{=InvEnh_UI_Close}Close").ToString();

    [DataSourceProperty]
    public HintViewModel CloseHint { get; } = new(new TextObject("{=InvEnh_Hint_SideClose}Close this menu without applying its changes."));

    [DataSourceProperty]
    public HintViewModel ButtonHint { get; } = new(new TextObject("{=InvEnh_Hint_SideButton}Open Sort & Filter for this side's inventory."));

    [DataSourceProperty]
    public HintViewModel LockedHint { get; } = new(new TextObject("{=InvEnh_Hint_SideLocked}The other side's Sort & Filter applies to both sides. Uncheck Both Sides there to use this side on its own."));

    [DataSourceProperty]
    public HintViewModel BothSidesHint { get; } = new(new TextObject("{=InvEnh_Hint_BothSides}Apply this side's sort and filter to both inventories. The other side's Sort & Filter cannot be used while this is on."));

    [DataSourceProperty]
    public HintViewModel ResetHint { get; } = new(new TextObject("{=InvEnh_Hint_SideReset}Clear this side's sort and filter."));

    [DataSourceProperty]
    public HintViewModel ApplyHint { get; } = new(new TextObject("{=InvEnh_Hint_SideApply}Apply this side's sort and filter."));

    public void ExecuteToggleMenu()
    {
        if (IsMenuOpen)
        {
            CloseMenu();
            return;
        }

        if (!IsButtonVisible || IsLocked) return;

        var own = _selections.ChoiceOf(_side);

        _stagedSort = own.Sort;
        SortMenu.IsAscending = own.SortAscending;
        SortMenu.Populate(own.Sort, OnSortSelected);
        SortMenu.IsVisible = IsSortEnabled;

        _stagedFilters.Clear();
        _stagedFilters.UnionWith(own.Filters);
        FilterMenu.Populate(own.Filters, OnFilterToggled);
        FilterMenu.IsVisible = IsFilterEnabled;

        IsBothSides = _selections.Link.IsLinkedFrom(LinkedFeature.Sort, _side) || _selections.Link.IsLinkedFrom(LinkedFeature.Filter, _side);
        IsMenuOpen = true;
    }

    public void ExecuteClose()
    {
        CloseMenu();
    }

    public void ExecuteToggleBothSides()
    {
        IsBothSides = !IsBothSides;
    }

    public void ExecuteApply()
    {
        if (!IsMenuOpen || IsLocked) return;

        var own = _selections.ChoiceOf(_side);
        if (IsSortEnabled)
        {
            own.Sort = _stagedSort;
            own.SortAscending = SortMenu.IsAscending;
        }

        if (IsFilterEnabled)
        {
            own.Filters.Clear();
            own.Filters.UnionWith(_stagedFilters);
        }

        foreach (var feature in Features)
        {
            if (IsBothSides)
                _selections.Link.TryLink(feature, _side);
            else
                _selections.Link.Unlink(feature, _side);
        }

        CloseMenu();
        _onApplied();
    }

    public void ExecuteReset()
    {
        if (IsLocked) return;

        _selections.ChoiceOf(_side).Clear();
        foreach (var feature in Features)
            _selections.Link.Unlink(feature, _side);

        IsBothSides = false;
        CloseMenu();
        _onApplied();
    }

    public void CloseMenu()
    {
        SortMenu.IsVisible = false;
        FilterMenu.IsVisible = false;
        IsMenuOpen = false;
    }

    public void RefreshState()
    {
        if (IsLocked && IsMenuOpen)
            CloseMenu();

        OnPropertyChanged(nameof(IsSortEnabled));
        OnPropertyChanged(nameof(IsFilterEnabled));
        OnPropertyChanged(nameof(IsButtonVisible));
        OnPropertyChanged(nameof(IsLocked));
        OnPropertyChanged(nameof(IsActive));
    }

    private void OnSortSelected(SortOptionVM selected)
    {
        if (selected.IsSelected)
        {
            foreach (var option in SortMenu.AllOptions())
            {
                if (option != selected && option.IsSelected)
                    option.IsSelected = false;
            }

            _stagedSort = selected.Category;
        }
        else
        {
            _stagedSort = SortCategory.None;
        }
    }

    private void OnFilterToggled(FilterOptionVM option)
    {
        if (option.IsSelected)
            _stagedFilters.Add(option.Category);
        else
            _stagedFilters.Remove(option.Category);
    }
}
