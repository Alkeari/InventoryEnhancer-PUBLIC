using System;
using System.Collections.Generic;
using System.Linq;
using InventoryEnhancer.Helpers;
using InventoryEnhancer.Models;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace InventoryEnhancer.UI.ViewModels;

public class FilterMenuVM : ViewModel
{
    private bool _isVisible;

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

    // Category filters (OR logic: matches ANY selected category) render in groups, the weapon ones by
    // usage class and the rest mirroring the vanilla inventory's own filter tabs, each carrying that
    // tab's real icon. Modifier filters (AND logic: narrow whatever the categories above matched)
    // render separately below, so the two combination rules stay visibly distinct instead of one flat
    // list that silently switches behavior partway through.
    [DataSourceProperty]
    public MBBindingList<FilterOptionVM> WeaponsOneHandedOptions { get; } = new();

    [DataSourceProperty]
    public MBBindingList<FilterOptionVM> WeaponsTwoHandedOptions { get; } = new();

    [DataSourceProperty]
    public MBBindingList<FilterOptionVM> PolearmsOptions { get; } = new();

    [DataSourceProperty]
    public MBBindingList<FilterOptionVM> ThrownOptions { get; } = new();

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
    public string CategoryHeaderText => new TextObject("{=InvEnh_Filter_CategoryHeader}Match Any").ToString();

    [DataSourceProperty]
    public string WeaponsOneHandedHeaderText => new TextObject("{=InvEnh_Filter_WeaponsOneHandedHeader}Melee (One-Handed)").ToString();

    [DataSourceProperty]
    public string WeaponsTwoHandedHeaderText => new TextObject("{=InvEnh_Filter_WeaponsTwoHandedHeader}Melee (Two-Handed)").ToString();

    [DataSourceProperty]
    public string PolearmsHeaderText => new TextObject("{=InvEnh_Filter_PolearmsHeader}Polearms").ToString();

    [DataSourceProperty]
    public string ThrownHeaderText => new TextObject("{=InvEnh_Filter_ThrownHeader}Thrown").ToString();

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

    public IEnumerable<FilterOptionVM> AllOptions() =>
        WeaponsOneHandedOptions.Concat(WeaponsTwoHandedOptions).Concat(PolearmsOptions).Concat(ThrownOptions).Concat(RangedOptions).Concat(ArmorOptions).Concat(MountsOptions).Concat(MiscOptions).Concat(CultureOptions).Concat(ModifierOptions);

    #region Text Properties (bound by XML inside DataSource="{FilterMenu}")

    [DataSourceProperty]
    public string FilterOptionsText => new TextObject("{=InvEnh_UI_FilterOptions}Filter Options").ToString();

    #endregion

    public void Populate(IReadOnlyCollection<FilterCategory> activeFilters, Action<FilterOptionVM> onToggle)
    {
        foreach (var list in new[] { WeaponsOneHandedOptions, WeaponsTwoHandedOptions, PolearmsOptions, ThrownOptions, RangedOptions, ArmorOptions, MountsOptions, MiscOptions, CultureOptions, ModifierOptions })
            list.Clear();

        foreach (FilterCategory category in Enum.GetValues(typeof(FilterCategory)))
        {
            if (category == FilterCategory.None)
                continue;

            var option = new FilterOptionVM(GetFilterCategoryName(category), category, GetFilterCategoryHint(category), onToggle, activeFilters.Contains(category));

            if (FilterCategoryHelper.IsModifier(category))
            {
                ModifierOptions.Add(option);
                continue;
            }

            var list = FilterCategoryHelper.GetGroup(category) switch
            {
                FilterGroup.WeaponsTwoHanded => WeaponsTwoHandedOptions,
                FilterGroup.Polearms => PolearmsOptions,
                FilterGroup.Thrown => ThrownOptions,
                FilterGroup.ShieldsAndRanged => RangedOptions,
                FilterGroup.Armor => ArmorOptions,
                FilterGroup.Mounts => MountsOptions,
                FilterGroup.Misc => MiscOptions,
                FilterGroup.Culture => CultureOptions,
                _ => WeaponsOneHandedOptions,
            };
            list.Add(option);
        }
    }

    private static string GetFilterCategoryName(FilterCategory category) => category switch
    {
        FilterCategory.Dagger => new TextObject("{=InvEnh_Filter_Dagger}Dagger").ToString(),
        FilterCategory.OneHandedSword => new TextObject("{=InvEnh_Filter_OneHandedSword}One-Handed Sword").ToString(),
        FilterCategory.OneHandedAxe => new TextObject("{=InvEnh_Filter_OneHandedAxe}One-Handed Axe").ToString(),
        FilterCategory.Mace => new TextObject("{=InvEnh_Filter_Mace}Mace").ToString(),
        FilterCategory.TwoHandedSword => new TextObject("{=InvEnh_Filter_TwoHandedSword}Two-Handed Sword").ToString(),
        FilterCategory.TwoHandedAxe => new TextObject("{=InvEnh_Filter_TwoHandedAxe}Two-Handed Axe").ToString(),
        FilterCategory.Spear => new TextObject("{=InvEnh_Filter_Spear}Spear").ToString(),
        FilterCategory.Lance => new TextObject("{=InvEnh_Filter_Lance}Lance").ToString(),
        FilterCategory.ThrowingAxe => new TextObject("{=InvEnh_Filter_ThrowingAxe}Throwing Axe").ToString(),
        FilterCategory.Javelin => new TextObject("{=InvEnh_Filter_Javelin}Javelin").ToString(),
        FilterCategory.ThrowingKnife => new TextObject("{=InvEnh_Filter_ThrowingKnife}Throwing Knife").ToString(),
        FilterCategory.Stones => new TextObject("{=InvEnh_Filter_Stones}Stones").ToString(),
        FilterCategory.Bow => new TextObject("{=InvEnh_Filter_Bow}Bow").ToString(),
        FilterCategory.Crossbow => new TextObject("{=InvEnh_Filter_Crossbow}Crossbow").ToString(),
        FilterCategory.Arrows => new TextObject("{=InvEnh_Filter_Arrows}Arrows").ToString(),
        FilterCategory.Bolts => new TextObject("{=InvEnh_Filter_Bolts}Bolts").ToString(),
        FilterCategory.Shield => new TextObject("{=InvEnh_Filter_Shield}Shield").ToString(),
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

    private static TextObject GetFilterCategoryHint(FilterCategory category) => category switch
    {
        FilterCategory.Dagger => new TextObject("{=InvEnh_Hint_Filter_Dagger}Show daggers."),
        FilterCategory.OneHandedSword => new TextObject("{=InvEnh_Hint_Filter_OneHandedSword}Show single-handed swords for sword and shield work."),
        FilterCategory.OneHandedAxe => new TextObject("{=InvEnh_Hint_Filter_OneHandedAxe}Show single-handed axes."),
        FilterCategory.Mace => new TextObject("{=InvEnh_Hint_Filter_Mace}Show maces and picks, the blunt weapons that batter armor."),
        FilterCategory.TwoHandedSword => new TextObject("{=InvEnh_Hint_Filter_TwoHandedSword}Show two-handed swords."),
        FilterCategory.TwoHandedAxe => new TextObject("{=InvEnh_Hint_Filter_TwoHandedAxe}Show two-handed axes and mauls."),
        FilterCategory.Spear => new TextObject("{=InvEnh_Hint_Filter_Spear}Show spears and every other polearm that cannot be couched."),
        FilterCategory.Lance => new TextObject("{=InvEnh_Hint_Filter_Lance}Show only couchable lances, the polearms a rider can couch for the charge."),
        FilterCategory.ThrowingAxe => new TextObject("{=InvEnh_Hint_Filter_ThrowingAxe}Show throwing axes."),
        FilterCategory.Javelin => new TextObject("{=InvEnh_Hint_Filter_Javelin}Show javelins."),
        FilterCategory.ThrowingKnife => new TextObject("{=InvEnh_Hint_Filter_ThrowingKnife}Show throwing knives."),
        FilterCategory.Stones => new TextObject("{=InvEnh_Hint_Filter_Stones}Show throwing stones."),
        FilterCategory.Bow => new TextObject("{=InvEnh_Hint_Filter_Bow}Show bows."),
        FilterCategory.Crossbow => new TextObject("{=InvEnh_Hint_Filter_Crossbow}Show crossbows."),
        FilterCategory.Arrows => new TextObject("{=InvEnh_Hint_Filter_Arrows}Show arrows."),
        FilterCategory.Bolts => new TextObject("{=InvEnh_Hint_Filter_Bolts}Show crossbow bolts."),
        FilterCategory.Shield => new TextObject("{=InvEnh_Hint_Filter_Shield}Show shields."),
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
        _ => new TextObject("{=!}")
    };
}
