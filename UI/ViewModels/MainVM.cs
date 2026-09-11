using System;
using System.Collections.Generic;
using System.Reflection;
using InventoryEnhancer.Behaviors;
using InventoryEnhancer.Helpers;
using InventoryEnhancer.Models;
using InventoryEnhancer.Services;
using HarmonyLib;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace InventoryEnhancer.UI.ViewModels;

/// <summary>
/// Main ViewModel for the Inventory Enhancer UI layer.
/// Contains all properties and methods bound to the XML prefab.
/// </summary>
public class MainVM : ViewModel
{
    private readonly SortMenuVM _sortMenu;
    private readonly FilterMenuVM _filterMenu;

    // Staged state (not applied until user clicks Apply)
    private SortCategory _stagedSort = SortCategory.None;
    private bool _stagedSortAscending = false;
    private HashSet<FilterCategory> _stagedFilters = new();

    private static readonly MethodInfo? _processFilterMethod = AccessTools.Method(typeof(SPInventoryVM), "ProcessFilter");

    // SPInventoryVM.Filters.All == 0 - used to trigger a full re-filter via ProcessFilter
    private static readonly object[] _processFilterArgs = new object[] { (int)0 };

    // The fallback path below still works (it re-sorts and re-filters through a different
    // mechanism), so warning on every Apply would cry wolf about a call that actually
    // succeeded. Warn once per session instead, the first time the fallback ever engages.
    private static bool _hasWarnedAboutFallback;

    public MainVM()
    {
        _sortMenu = new SortMenuVM();
        _filterMenu = new FilterMenuVM();

        try
        {
            InventoryEnhancerLogger.Log("MainVM constructor called.");
            InitializeMenus();
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("Error in MainVM constructor", ex);
        }
    }

    #region DataSource Properties - Basic

    private bool _isUIHidden;

    [DataSourceProperty]
    public bool IsModEnabled => Settings.Settings.Instance?.IsModEnabled ?? true;

    [DataSourceProperty]
    public bool IsUIHidden
    {
        get => _isUIHidden;
        set
        {
            if (_isUIHidden != value)
            {
                _isUIHidden = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsUIVisible));
                OnPropertyChanged(nameof(HideButtonText));
            }
        }
    }

    [DataSourceProperty]
    public bool IsUIVisible => !_isUIHidden;

    [DataSourceProperty]
    public string HideButtonText => _isUIHidden
        ? new TextObject("{=InvEnh_UI_Show}Show").ToString()
        : new TextObject("{=InvEnh_UI_Hide}Hide").ToString();

    [DataSourceProperty]
    public bool IsSortingEnabled => (Settings.Settings.Instance?.IsModEnabled ?? true) && (Settings.Settings.Instance?.EnableSorting ?? true);

    [DataSourceProperty]
    public bool IsFilteringEnabled => (Settings.Settings.Instance?.IsModEnabled ?? true) && (Settings.Settings.Instance?.EnableFiltering ?? true);

    [DataSourceProperty]
    public SortMenuVM SortMenu => _sortMenu;

    [DataSourceProperty]
    public FilterMenuVM FilterMenu => _filterMenu;

    #endregion

    #region DataSource Properties - Active Indicators

    [DataSourceProperty]
    public bool IsSortActive => (Settings.Settings.Instance?.IsModEnabled ?? true) &&
        (SortEngine.LeftSort != SortCategory.None || SortEngine.RightSort != SortCategory.None);

    [DataSourceProperty]
    public bool IsFilterActive => (Settings.Settings.Instance?.IsModEnabled ?? true) &&
        (FilterEngine.LeftFilters.Count > 0 || FilterEngine.RightFilters.Count > 0);

    [DataSourceProperty]
    public int ActiveFilterCount => Math.Max(FilterEngine.LeftFilters.Count, FilterEngine.RightFilters.Count);

    [DataSourceProperty]
    public string FilterButtonText
    {
        get
        {
            var filterText = new TextObject("{=InvEnh_UI_Filter}Filter").ToString();
            return ActiveFilterCount > 0 ? $"{filterText} ({ActiveFilterCount})" : filterText;
        }
    }

    [DataSourceProperty]
    public string SortButtonText => new TextObject("{=InvEnh_UI_Sort}Sort").ToString();

    [DataSourceProperty]
    public string ResetButtonText => new TextObject("{=InvEnh_UI_Reset}Reset").ToString();

    [DataSourceProperty]
    public string ApplyButtonText => new TextObject("{=InvEnh_UI_Apply}Apply").ToString();

    [DataSourceProperty]
    public string SortOptionsText => new TextObject("{=InvEnh_UI_SortOptions}Sort Options").ToString();

    [DataSourceProperty]
    public string FilterOptionsText => new TextObject("{=InvEnh_UI_FilterOptions}Filter Options").ToString();

    [DataSourceProperty]
    public HintViewModel HideHint { get; } = new(new TextObject("{=InvEnh_Hint_Hide}Hide or show the Inventory Enhancer overlay."));

    [DataSourceProperty]
    public HintViewModel SortButtonHint { get; } = new(new TextObject("{=InvEnh_Hint_SortButton}Open sort options for this inventory."));

    [DataSourceProperty]
    public HintViewModel FilterButtonHint { get; } = new(new TextObject("{=InvEnh_Hint_FilterButton}Open filter options for this inventory."));

    [DataSourceProperty]
    public HintViewModel ResetHint { get; } = new(new TextObject("{=InvEnh_Hint_Reset}Clear all sort and filter selections."));

    [DataSourceProperty]
    public HintViewModel ApplyHint { get; } = new(new TextObject("{=InvEnh_Hint_Apply}Apply the selected sort and filter to the inventory."));

    [DataSourceProperty]
    public bool IsApplyVisible
    {
        get
        {
            if (Settings.Settings.Instance != null && !Settings.Settings.Instance.IsModEnabled) return false;
            return _sortMenu.IsVisible || _filterMenu.IsVisible || HasStagedChanges();
        }
    }

    private bool HasStagedChanges()
    {
        if (_stagedSort != SortCategory.None)
            return true;
        if (_stagedFilters.Count > 0)
            return true;
        return false;
    }

    #endregion

    #region Initialization

    private void InitializeMenus()
    {
        try
        {
            InventoryEnhancerLogger.Log("Initializing menus...");

            _sortMenu.GeneralOptions.Clear();
            _sortMenu.CombatOptions.Clear();
            _sortMenu.DefenseOptions.Clear();
            var activeSort = SortEngine.LeftSort != SortCategory.None
                ? SortEngine.LeftSort
                : SortEngine.RightSort;
            _stagedSort = activeSort;

            var activeAscending = SortEngine.LeftSort != SortCategory.None
                ? SortEngine.LeftSortAscending
                : SortEngine.RightSortAscending;
            _stagedSortAscending = activeAscending;
            _sortMenu.IsAscending = activeAscending;

            foreach (SortCategory category in Enum.GetValues(typeof(SortCategory)))
            {
                if (category == SortCategory.None ||
                    category == SortCategory.Type ||
                    category == SortCategory.Count ||
                    category == SortCategory.Value)
                    continue;

                string name = GetSortCategoryName(category);
                var hint = GetSortCategoryHint(category);
                bool isSelected = category == activeSort;
                var option = new SortOptionVM(name, category, hint, OnSortOptionSelected, isSelected);

                var list = SortCategoryHelper.GetGroup(category) switch
                {
                    SortGroup.Combat => _sortMenu.CombatOptions,
                    SortGroup.Defense => _sortMenu.DefenseOptions,
                    _ => _sortMenu.GeneralOptions,
                };
                list.Add(option);
            }

            _filterMenu.WeaponsOneHandedOptions.Clear();
            _filterMenu.WeaponsTwoHandedOptions.Clear();
            _filterMenu.RangedOptions.Clear();
            _filterMenu.ArmorOptions.Clear();
            _filterMenu.MountsOptions.Clear();
            _filterMenu.MiscOptions.Clear();
            _filterMenu.CultureOptions.Clear();
            _filterMenu.ModifierOptions.Clear();
            var activeFilters = FilterEngine.LeftFilters.Count > 0
                ? FilterEngine.LeftFilters
                : FilterEngine.RightFilters;
            _stagedFilters = new HashSet<FilterCategory>(activeFilters);

            foreach (FilterCategory category in Enum.GetValues(typeof(FilterCategory)))
            {
                if (category == FilterCategory.None)
                    continue;

                string name = GetFilterCategoryName(category);
                var hint = GetFilterCategoryHint(category);
                bool isSelected = activeFilters.Contains(category);
                var option = new FilterOptionVM(name, category, hint, OnFilterOptionToggled, isSelected);

                if (FilterCategoryHelper.IsModifier(category))
                {
                    _filterMenu.ModifierOptions.Add(option);
                    continue;
                }

                var list = FilterCategoryHelper.GetGroup(category) switch
                {
                    FilterGroup.WeaponsTwoHanded => _filterMenu.WeaponsTwoHandedOptions,
                    FilterGroup.ShieldsAndRanged => _filterMenu.RangedOptions,
                    FilterGroup.Armor => _filterMenu.ArmorOptions,
                    FilterGroup.Mounts => _filterMenu.MountsOptions,
                    FilterGroup.Misc => _filterMenu.MiscOptions,
                    FilterGroup.Culture => _filterMenu.CultureOptions,
                    _ => _filterMenu.WeaponsOneHandedOptions,
                };
                list.Add(option);
            }

            InventoryEnhancerLogger.Log("Menus initialized.");
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("Error in InitializeMenus", ex);
        }
    }

    private static string GetSortCategoryName(SortCategory category) => category switch
    {
        SortCategory.Name => new TextObject("{=InvEnh_Sort_Name}Name").ToString(),
        SortCategory.Tier => new TextObject("{=InvEnh_Sort_Tier}Tier").ToString(),
        SortCategory.Damage => new TextObject("{=InvEnh_Sort_Damage}Damage").ToString(),
        SortCategory.Accuracy => new TextObject("{=InvEnh_Sort_Accuracy}Accuracy").ToString(),
        SortCategory.Reach => new TextObject("{=InvEnh_Sort_Reach}Reach").ToString(),
        SortCategory.MissileSpeed => new TextObject("{=InvEnh_Sort_MissileSpeed}Missile Speed").ToString(),
        SortCategory.HitPoints => new TextObject("{=InvEnh_Sort_HitPoints}Hit Points").ToString(),
        SortCategory.ArmorCombined => new TextObject("{=InvEnh_Sort_ArmorCombined}Armor (Total)").ToString(),
        SortCategory.ArmorHighest => new TextObject("{=InvEnh_Sort_ArmorHighest}Armor (Highest)").ToString(),
        SortCategory.Speed => new TextObject("{=InvEnh_Sort_Speed}Speed").ToString(),
        SortCategory.Handling => new TextObject("{=InvEnh_Sort_Handling}Handling").ToString(),
        SortCategory.Weight => new TextObject("{=InvEnh_Sort_Weight}Weight").ToString(),
        SortCategory.ValuePerWeight => new TextObject("{=InvEnh_Sort_VPW}Value / Weight").ToString(),
        _ => category.ToString()
    };

    private static string GetFilterCategoryName(FilterCategory category) => category switch
    {
        FilterCategory.Daggers => new TextObject("{=InvEnh_Filter_Daggers}Daggers").ToString(),
        FilterCategory.OneHandedSwords => new TextObject("{=InvEnh_Filter_1HSwords}1H Swords").ToString(),
        FilterCategory.OneHandedAxes => new TextObject("{=InvEnh_Filter_1HAxes}1H Axes").ToString(),
        FilterCategory.OneHandedMaces => new TextObject("{=InvEnh_Filter_1HMaces}1H Maces").ToString(),
        FilterCategory.OneHandedPolearms => new TextObject("{=InvEnh_Filter_1HPolearms}1H Polearms").ToString(),
        FilterCategory.TwoHandedSwords => new TextObject("{=InvEnh_Filter_2HSwords}2H Swords").ToString(),
        FilterCategory.TwoHandedAxes => new TextObject("{=InvEnh_Filter_2HAxes}2H Axes").ToString(),
        FilterCategory.TwoHandedMaces => new TextObject("{=InvEnh_Filter_2HMaces}2H Maces").ToString(),
        FilterCategory.TwoHandedPolearms => new TextObject("{=InvEnh_Filter_2HPolearms}2H Polearms").ToString(),
        FilterCategory.Bows => new TextObject("{=InvEnh_Filter_Bows}Bows").ToString(),
        FilterCategory.Crossbows => new TextObject("{=InvEnh_Filter_Crossbows}Crossbows").ToString(),
        FilterCategory.ThrowingWeapons => new TextObject("{=InvEnh_Filter_Throwing}Throwing Weapons").ToString(),
        FilterCategory.Ammunition => new TextObject("{=InvEnh_Filter_Ammo}Ammunition").ToString(),
        FilterCategory.Shields => new TextObject("{=InvEnh_Filter_Shields}Shields").ToString(),
        FilterCategory.HeadArmor => new TextObject("{=InvEnh_Filter_Head}Head Armor").ToString(),
        FilterCategory.ShoulderArmor => new TextObject("{=InvEnh_Filter_Shoulders}Shoulder Armor").ToString(),
        FilterCategory.BodyArmor => new TextObject("{=InvEnh_Filter_Body}Body Armor").ToString(),
        FilterCategory.ArmArmor => new TextObject("{=InvEnh_Filter_Arms}Arm Armor").ToString(),
        FilterCategory.LegArmor => new TextObject("{=InvEnh_Filter_Legs}Leg Armor").ToString(),
        FilterCategory.Mounts => new TextObject("{=InvEnh_Filter_Mounts}Mounts").ToString(),
        FilterCategory.HorseArmor => new TextObject("{=InvEnh_Filter_HorseArmor}Horse Armor").ToString(),
        FilterCategory.Banner => new TextObject("{=InvEnh_Filter_Banners}Banners").ToString(),
        FilterCategory.Food => new TextObject("{=InvEnh_Filter_Food}Food").ToString(),
        FilterCategory.Goods => new TextObject("{=InvEnh_Filter_Goods}Trade Goods").ToString(),
        FilterCategory.Vlandian => new TextObject("{=InvEnh_Filter_Vlandian}Vlandian").ToString(),
        FilterCategory.Sturgian => new TextObject("{=InvEnh_Filter_Sturgian}Sturgian").ToString(),
        FilterCategory.Imperial => new TextObject("{=InvEnh_Filter_Imperial}Imperial").ToString(),
        FilterCategory.Battanian => new TextObject("{=InvEnh_Filter_Battanian}Battanian").ToString(),
        FilterCategory.Khuzait => new TextObject("{=InvEnh_Filter_Khuzait}Khuzait").ToString(),
        FilterCategory.Aserai => new TextObject("{=InvEnh_Filter_Aserai}Aserai").ToString(),
        FilterCategory.TierHigh => new TextObject("{=InvEnh_Filter_TierHigh}High Tier (5+)").ToString(),
        FilterCategory.TierLow => new TextObject("{=InvEnh_Filter_TierLow}Low Tier (2 or Under)").ToString(),
        FilterCategory.ValueHigh => new TextObject("{=InvEnh_Filter_ValueHigh}High Value (1000+)").ToString(),
        FilterCategory.ValueLow => new TextObject("{=InvEnh_Filter_ValueLow}Low Value (Under 100)").ToString(),
        _ => category.ToString()
    };

    private static TextObject GetSortCategoryHint(SortCategory category) => category switch
    {
        SortCategory.Name => new TextObject("{=InvEnh_Hint_Sort_Name}Sort alphabetically by item name."),
        SortCategory.Tier => new TextObject("{=InvEnh_Hint_Sort_Tier}Sort by item tier, from common to legendary."),
        SortCategory.Damage => new TextObject("{=InvEnh_Hint_Sort_Damage}Sort by the weapon's highest damage value, thrust or swing."),
        SortCategory.Accuracy => new TextObject("{=InvEnh_Hint_Sort_Accuracy}Sort by ranged weapon accuracy."),
        SortCategory.Reach => new TextObject("{=InvEnh_Hint_Sort_Reach}Sort by weapon length."),
        SortCategory.MissileSpeed => new TextObject("{=InvEnh_Hint_Sort_MissileSpeed}Sort by projectile speed for bows, crossbows, and thrown weapons."),
        SortCategory.HitPoints => new TextObject("{=InvEnh_Hint_Sort_HitPoints}Sort by durability: shield hit points or mount health."),
        SortCategory.ArmorCombined => new TextObject("{=InvEnh_Hint_Sort_ArmorCombined}Sort by total armor across every body area the item covers."),
        SortCategory.ArmorHighest => new TextObject("{=InvEnh_Hint_Sort_ArmorHighest}Sort by the item's single highest armor value."),
        SortCategory.Speed => new TextObject("{=InvEnh_Hint_Sort_Speed}Sort by weapon swing speed or mount speed."),
        SortCategory.Handling => new TextObject("{=InvEnh_Hint_Sort_Handling}Sort by weapon handling."),
        SortCategory.Weight => new TextObject("{=InvEnh_Hint_Sort_Weight}Sort by item weight."),
        SortCategory.ValuePerWeight => new TextObject("{=InvEnh_Hint_Sort_VPW}Sort by value per unit of weight, useful for judging what's worth carrying."),
        _ => TextObject.GetEmpty()
    };

    private static TextObject GetFilterCategoryHint(FilterCategory category) => category switch
    {
        FilterCategory.Daggers => new TextObject("{=InvEnh_Hint_Filter_Daggers}Show daggers."),
        FilterCategory.OneHandedSwords => new TextObject("{=InvEnh_Hint_Filter_1HSwords}Show one-handed swords."),
        FilterCategory.OneHandedAxes => new TextObject("{=InvEnh_Hint_Filter_1HAxes}Show one-handed axes."),
        FilterCategory.OneHandedMaces => new TextObject("{=InvEnh_Hint_Filter_1HMaces}Show one-handed maces."),
        FilterCategory.OneHandedPolearms => new TextObject("{=InvEnh_Hint_Filter_1HPolearms}Show one-handed polearms."),
        FilterCategory.TwoHandedSwords => new TextObject("{=InvEnh_Hint_Filter_2HSwords}Show two-handed swords."),
        FilterCategory.TwoHandedAxes => new TextObject("{=InvEnh_Hint_Filter_2HAxes}Show two-handed axes."),
        FilterCategory.TwoHandedMaces => new TextObject("{=InvEnh_Hint_Filter_2HMaces}Show two-handed maces."),
        FilterCategory.TwoHandedPolearms => new TextObject("{=InvEnh_Hint_Filter_2HPolearms}Show two-handed polearms."),
        FilterCategory.Bows => new TextObject("{=InvEnh_Hint_Filter_Bows}Show bows."),
        FilterCategory.Crossbows => new TextObject("{=InvEnh_Hint_Filter_Crossbows}Show crossbows."),
        FilterCategory.ThrowingWeapons => new TextObject("{=InvEnh_Hint_Filter_Throwing}Show throwing weapons."),
        FilterCategory.Ammunition => new TextObject("{=InvEnh_Hint_Filter_Ammo}Show arrows and bolts."),
        FilterCategory.Shields => new TextObject("{=InvEnh_Hint_Filter_Shields}Show shields."),
        FilterCategory.HeadArmor => new TextObject("{=InvEnh_Hint_Filter_Head}Show helmets."),
        FilterCategory.ShoulderArmor => new TextObject("{=InvEnh_Hint_Filter_Shoulders}Show capes and cloaks."),
        FilterCategory.BodyArmor => new TextObject("{=InvEnh_Hint_Filter_Body}Show body armor."),
        FilterCategory.ArmArmor => new TextObject("{=InvEnh_Hint_Filter_Arms}Show hand and arm armor."),
        FilterCategory.LegArmor => new TextObject("{=InvEnh_Hint_Filter_Legs}Show leg armor and boots."),
        FilterCategory.Mounts => new TextObject("{=InvEnh_Hint_Filter_Mounts}Show horses and other mounts."),
        FilterCategory.HorseArmor => new TextObject("{=InvEnh_Hint_Filter_HorseArmor}Show mount armor."),
        FilterCategory.Banner => new TextObject("{=InvEnh_Hint_Filter_Banners}Show banners."),
        FilterCategory.Food => new TextObject("{=InvEnh_Hint_Filter_Food}Show food items."),
        FilterCategory.Goods => new TextObject("{=InvEnh_Hint_Filter_Goods}Show trade goods."),
        FilterCategory.Vlandian => new TextObject("{=InvEnh_Hint_Filter_Vlandian}Show items made by Vlandia."),
        FilterCategory.Sturgian => new TextObject("{=InvEnh_Hint_Filter_Sturgian}Show items made by Sturgia."),
        FilterCategory.Imperial => new TextObject("{=InvEnh_Hint_Filter_Imperial}Show items made by the Empire."),
        FilterCategory.Battanian => new TextObject("{=InvEnh_Hint_Filter_Battanian}Show items made by Battania."),
        FilterCategory.Khuzait => new TextObject("{=InvEnh_Hint_Filter_Khuzait}Show items made by the Khuzait Khanate."),
        FilterCategory.Aserai => new TextObject("{=InvEnh_Hint_Filter_Aserai}Show items made by the Aserai."),
        FilterCategory.TierHigh => new TextObject("{=InvEnh_Hint_Filter_TierHigh}Narrow the results above to tier 5 and up."),
        FilterCategory.TierLow => new TextObject("{=InvEnh_Hint_Filter_TierLow}Narrow the results above to tier 2 and below."),
        FilterCategory.ValueHigh => new TextObject("{=InvEnh_Hint_Filter_ValueHigh}Narrow the results above to items worth 1000 gold or more."),
        FilterCategory.ValueLow => new TextObject("{=InvEnh_Hint_Filter_ValueLow}Narrow the results above to items worth less than 100 gold."),
        _ => TextObject.GetEmpty()
    };

    #endregion

    #region Option Selection Callbacks

    private void OnSortOptionSelected(SortOptionVM selectedOption)
    {
        if (selectedOption.IsSelected)
        {
            foreach (var opt in _sortMenu.AllOptions())
            {
                if (opt != selectedOption && opt.IsSelected)
                    opt.IsSelected = false;
            }
            _stagedSort = selectedOption.Category;
        }
        else
        {
            _stagedSort = SortCategory.None;
        }
        OnPropertyChanged(nameof(IsApplyVisible));
    }

    private void OnFilterOptionToggled(FilterOptionVM option)
    {
        if (option.IsSelected)
            _stagedFilters.Add(option.Category);
        else
            _stagedFilters.Remove(option.Category);
        OnPropertyChanged(nameof(IsApplyVisible));
    }

    #endregion

    #region DataSource Methods - Menu Toggles

    public void ExecuteToggleSortMenu()
    {
        if (Settings.Settings.Instance != null && !Settings.Settings.Instance.IsModEnabled) return;
        bool wasVisible = _sortMenu.IsVisible;
        CloseAllMenus();
        if (!wasVisible)
        {
            // Re-sync staged options from the live engine state every time the menu opens,
            // so a selection abandoned without Apply never survives to look active on reopen.
            InitializeMenus();
        }
        _sortMenu.IsVisible = !wasVisible;
        OnPropertyChanged(nameof(IsApplyVisible));
    }

    public void ExecuteToggleFilterMenu()
    {
        if (Settings.Settings.Instance != null && !Settings.Settings.Instance.IsModEnabled) return;
        bool wasVisible = _filterMenu.IsVisible;
        CloseAllMenus();
        if (!wasVisible)
        {
            InitializeMenus();
        }
        _filterMenu.IsVisible = !wasVisible;
        OnPropertyChanged(nameof(IsApplyVisible));
    }

    public void ExecuteToggleHide()
    {
        if (Settings.Settings.Instance != null && !Settings.Settings.Instance.IsModEnabled) return;
        IsUIHidden = !IsUIHidden;
        if (IsUIHidden)
        {
            CloseAllMenus();
        }
    }

    #endregion

    #region DataSource Methods - Apply

    public void ExecuteApply()
    {
        var sortSide = _sortMenu.SelectedSide;
        var filterSide = _filterMenu.SelectedSide;

        _stagedSortAscending = _sortMenu.IsAscending;

        InventoryEnhancerLogger.Log($"ExecuteApply called. SortSide={sortSide}, Sort={_stagedSort}, Ascending={_stagedSortAscending}, FilterSide={filterSide}, Filters={_stagedFilters.Count}");

        if (sortSide == SideSelection.Left || sortSide == SideSelection.Both)
        {
            SortEngine.LeftSort = _stagedSort;
            SortEngine.LeftSortAscending = _stagedSortAscending;
        }
        if (sortSide == SideSelection.Right || sortSide == SideSelection.Both)
        {
            SortEngine.RightSort = _stagedSort;
            SortEngine.RightSortAscending = _stagedSortAscending;
        }

        if (filterSide == SideSelection.Left || filterSide == SideSelection.Both)
            FilterEngine.LeftFilters = new HashSet<FilterCategory>(_stagedFilters);
        if (filterSide == SideSelection.Right || filterSide == SideSelection.Both)
            FilterEngine.RightFilters = new HashSet<FilterCategory>(_stagedFilters);

        CloseAllMenus();
        RefreshInventory();

        OnPropertyChanged(nameof(IsSortActive));
        OnPropertyChanged(nameof(IsFilterActive));
        OnPropertyChanged(nameof(ActiveFilterCount));
        OnPropertyChanged(nameof(FilterButtonText));
        OnPropertyChanged(nameof(IsApplyVisible));

        InventoryEnhancerLogger.Log("Apply completed.");
    }

    public void ExecuteClearAll()
    {
        _stagedSort = SortCategory.None;
        _stagedSortAscending = false;
        _stagedFilters.Clear();
        SortEngine.LeftSort = SortCategory.None;
        SortEngine.RightSort = SortCategory.None;
        SortEngine.LeftSortAscending = false;
        SortEngine.RightSortAscending = false;
        FilterEngine.LeftFilters.Clear();
        FilterEngine.RightFilters.Clear();

        foreach (var opt in _sortMenu.AllOptions())
            opt.IsSelected = false;
        foreach (var opt in _filterMenu.AllOptions())
            opt.IsSelected = false;
        _sortMenu.IsAscending = false;

        RefreshInventory();

        OnPropertyChanged(nameof(IsSortActive));
        OnPropertyChanged(nameof(IsFilterActive));
        OnPropertyChanged(nameof(ActiveFilterCount));
        OnPropertyChanged(nameof(FilterButtonText));
        OnPropertyChanged(nameof(IsApplyVisible));
    }

    #endregion

    #region Helper Methods

    private void CloseAllMenus()
    {
        _sortMenu.IsVisible = false;
        _filterMenu.IsVisible = false;
    }

    private void RefreshInventory()
    {
        InventoryEnhancerLogger.Log("Refreshing inventory...");
        var vm = Behavior.CurrentInventoryVM;
        if (vm == null)
        {
            InventoryEnhancerLogger.Log("CurrentInventoryVM is null during refresh.");
            var notReadyMsg = new TextObject("{=InvEnh_Msg_ApplyFailed}Inventory Enhancer: Could not apply changes. Try closing and reopening the inventory.");
            InformationManager.DisplayMessage(new InformationMessage(notReadyMsg.ToString(), Colors.Red));
            return;
        }

        if (_processFilterMethod != null)
        {
            try
            {
                _processFilterMethod.Invoke(vm, _processFilterArgs);
                InventoryEnhancerLogger.Log("Internal ProcessFilter invoked (sorting applied via postfix).");
            }
            catch (Exception ex)
            {
                InventoryEnhancerLogger.LogError("Failed to invoke ProcessFilter, falling back to search jiggle", ex);
                if (!_hasWarnedAboutFallback)
                {
                    _hasWarnedAboutFallback = true;
                    var fallbackMsg = new TextObject("{=InvEnh_Msg_ApplyFallback}Inventory Enhancer: Using a fallback method after a game update. If sorting or filtering looks wrong, please report it.");
                    InformationManager.DisplayMessage(new InformationMessage(fallbackMsg.ToString(), Colors.Yellow));
                }
                JiggleSearchText(vm);

                if (Settings.Settings.Instance?.EnableSorting == true)
                {
                    if (SortEngine.LeftSort != SortCategory.None)
                        SortEngine.SortList(vm.LeftItemListVM, SortEngine.LeftSort, SortEngine.LeftSortAscending);
                    if (SortEngine.RightSort != SortCategory.None)
                        SortEngine.SortList(vm.RightItemListVM, SortEngine.RightSort, SortEngine.RightSortAscending);
                }
            }
        }
        else
        {
            JiggleSearchText(vm);

            if (Settings.Settings.Instance?.EnableSorting == true)
            {
                if (SortEngine.LeftSort != SortCategory.None)
                    SortEngine.SortList(vm.LeftItemListVM, SortEngine.LeftSort, SortEngine.LeftSortAscending);
                if (SortEngine.RightSort != SortCategory.None)
                    SortEngine.SortList(vm.RightItemListVM, SortEngine.RightSort, SortEngine.RightSortAscending);
            }
        }

        InventoryEnhancerLogger.Log("Inventory refreshed.");
    }

    private void JiggleSearchText(SPInventoryVM vm)
    {
        var leftSearch = vm.LeftSearchText;
        vm.LeftSearchText = null;
        vm.LeftSearchText = leftSearch;

        var rightSearch = vm.RightSearchText;
        vm.RightSearchText = null;
        vm.RightSearchText = rightSearch;
    }

    #endregion
}
