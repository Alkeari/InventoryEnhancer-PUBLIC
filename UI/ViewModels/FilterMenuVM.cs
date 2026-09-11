using System.Collections.Generic;
using System.Linq;
using InventoryEnhancer.Models;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace InventoryEnhancer.UI.ViewModels;

public class FilterMenuVM : ViewModel
{
    private bool _isVisible;
    private SideSelection _selectedSide = SideSelection.Both;

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

    // Category filters (OR logic: matches ANY selected category) render as five groups mirroring
    // the vanilla inventory's own filter tabs (Weapons / Shields & Ranged / Armor / Mounts / Misc),
    // each carrying that tab's real icon brush. Modifier filters (AND logic: narrow whatever the
    // categories above matched) render separately below a divider, so the two combination rules
    // stay visibly distinct instead of one flat list that silently switches behavior partway through.
    [DataSourceProperty]
    public MBBindingList<FilterOptionVM> WeaponsOneHandedOptions { get; } = new();

    [DataSourceProperty]
    public MBBindingList<FilterOptionVM> WeaponsTwoHandedOptions { get; } = new();

    [DataSourceProperty]
    public MBBindingList<FilterOptionVM> RangedOptions { get; } = new();

    [DataSourceProperty]
    public MBBindingList<FilterOptionVM> ArmorOptions { get; } = new();

    [DataSourceProperty]
    public MBBindingList<FilterOptionVM> MountsOptions { get; } = new();

    [DataSourceProperty]
    public MBBindingList<FilterOptionVM> MiscOptions { get; } = new();

    [DataSourceProperty]
    public MBBindingList<FilterOptionVM> CultureOptions { get; } = new();

    [DataSourceProperty]
    public MBBindingList<FilterOptionVM> ModifierOptions { get; } = new();

    [DataSourceProperty]
    public MBBindingList<SideOptionVM> SideOptions { get; } = new();

    [DataSourceProperty]
    public string CategoryHeaderText => new TextObject("{=InvEnh_Filter_CategoryHeader}Match Any").ToString();

    [DataSourceProperty]
    public string WeaponsOneHandedHeaderText => new TextObject("{=InvEnh_Filter_WeaponsOneHandedHeader}Melee (One-Handed)").ToString();

    [DataSourceProperty]
    public string WeaponsTwoHandedHeaderText => new TextObject("{=InvEnh_Filter_WeaponsTwoHandedHeader}Melee (Two-Handed)").ToString();

    [DataSourceProperty]
    public string RangedHeaderText => new TextObject("{=InvEnh_Filter_RangedHeader}Shields & Ranged").ToString();

    [DataSourceProperty]
    public string ArmorHeaderText => new TextObject("{=InvEnh_Filter_ArmorHeader}Armor").ToString();

    [DataSourceProperty]
    public string MountsHeaderText => new TextObject("{=InvEnh_Filter_MountsHeader}Mounts").ToString();

    [DataSourceProperty]
    public string MiscHeaderText => new TextObject("{=InvEnh_Filter_MiscHeader}Misc").ToString();

    // No vanilla filter tab covers this (it's not one of the base game's own inventory tabs),
    // so it gets a plain text header like Sort's groups instead of a borrowed icon.
    [DataSourceProperty]
    public string CultureHeaderText => new TextObject("{=InvEnh_Filter_CultureHeader}Culture").ToString();

    [DataSourceProperty]
    public string ModifierHeaderText => new TextObject("{=InvEnh_Filter_ModifierHeader}Then Narrow By").ToString();

    public FilterMenuVM()
    {
        SideOptions.Add(new SideOptionVM(
            new TextObject("{=InvEnh_UI_Left}Left").ToString(), SideSelection.Left,
            new TextObject("{=InvEnh_Hint_FilterLeft}Filter only the left inventory."), OnSideSelected, isSelected: false));
        SideOptions.Add(new SideOptionVM(
            new TextObject("{=InvEnh_UI_Both}Both").ToString(), SideSelection.Both,
            new TextObject("{=InvEnh_Hint_FilterBoth}Filter both inventories."), OnSideSelected, isSelected: true));
        SideOptions.Add(new SideOptionVM(
            new TextObject("{=InvEnh_UI_Right}Right").ToString(), SideSelection.Right,
            new TextObject("{=InvEnh_Hint_FilterRight}Filter only the right inventory."), OnSideSelected, isSelected: false));
    }

    private void OnSideSelected(SideOptionVM selected)
    {
        SelectedSide = selected.Side;
    }

    public IEnumerable<FilterOptionVM> AllOptions() =>
        WeaponsOneHandedOptions.Concat(WeaponsTwoHandedOptions).Concat(RangedOptions).Concat(ArmorOptions).Concat(MountsOptions).Concat(MiscOptions).Concat(CultureOptions).Concat(ModifierOptions);

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

    #region Text Properties (bound by XML inside DataSource="{FilterMenu}")

    [DataSourceProperty]
    public string FilterOptionsText => new TextObject("{=InvEnh_UI_FilterOptions}Filter Options").ToString();

    #endregion

    public void Toggle() => IsVisible = !IsVisible;
    public void Close() => IsVisible = false;
}
