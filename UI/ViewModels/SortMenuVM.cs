using System;
using System.Collections.Generic;
using System.Linq;
using InventoryEnhancer.Helpers;
using InventoryEnhancer.Models;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace InventoryEnhancer.UI.ViewModels;

public class SortMenuVM : ViewModel
{
    private bool _isVisible;
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

    #region Text Properties (bound by XML inside DataSource="{SortMenu}")

    [DataSourceProperty]
    public string SortOptionsText => new TextObject("{=InvEnh_UI_SortOptions}Sort Options").ToString();

    #endregion

    public void Populate(SortCategory activeSort, Action<SortOptionVM> onSelect)
    {
        GeneralOptions.Clear();
        CombatOptions.Clear();
        DefenseOptions.Clear();

        foreach (SortCategory category in Enum.GetValues(typeof(SortCategory)))
        {
            if (category == SortCategory.None ||
                category == SortCategory.Type ||
                category == SortCategory.Count ||
                category == SortCategory.Value)
                continue;

            var option = new SortOptionVM(GetSortCategoryName(category), category, GetSortCategoryHint(category), onSelect, category == activeSort);

            var list = SortCategoryHelper.GetGroup(category) switch
            {
                SortGroup.Combat => CombatOptions,
                SortGroup.Defense => DefenseOptions,
                _ => GeneralOptions,
            };
            list.Add(option);
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
        _ => new TextObject("{=!}")
    };
}
