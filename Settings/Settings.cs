using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace InventoryEnhancer.Settings;

public class Settings : AttributeGlobalSettings<Settings>
{
    public override string Id => "Inventory Enhancer";
    public override string DisplayName => "Inventory Enhancer";
    public override string FolderName => "Inventory Enhancer";
    public override string FormatType => "json2";

    #region General
    [SettingPropertyBool("{=InvEnh_Sett_Enabled}Mod Enabled", Order = 1, RequireRestart = false, HintText = "{=InvEnh_Sett_Enabled_Hint}Master toggle for the entire mod. If disabled, all enhancements are turned off.")]
    [SettingPropertyGroup("{=InvEnh_Sett_General}General", GroupOrder = 1)]
    public bool IsModEnabled { get; set; } = true;

    [SettingPropertyBool("{=InvEnh_Sett_Debug}Enable Logging", Order = 2, RequireRestart = false, HintText = "{=InvEnh_Sett_Debug_Hint}Enables debug logging to Documents/Mount and Blade II Bannerlord/Configs/ModLogs/Inventory Enhancer.log.")]
    [SettingPropertyGroup("{=InvEnh_Sett_General}General", GroupOrder = 1)]
    public bool EnableLogging { get; set; } = false;

    [SettingPropertyBool("{=InvEnh_Sett_NativeBar}Show Status In The Vanilla Screen",
        HintText = "{=InvEnh_Sett_NativeBar_Hint}Preview: writes the active filter and sort straight into the vanilla inventory panels instead of only the overlay. Off by default because the overlay still shows the same thing, so both would appear at once. Default: off",
        RequireRestart = false, Order = 3)]
    [SettingPropertyGroup("{=InvEnh_Sett_General}General", GroupOrder = 1)]
    public bool ShowNativeStatusBar { get; set; } = false;
    #endregion

    #region Search
    [SettingPropertyBool("{=InvEnh_Sett_SearchEnabler}Enable Search in All Inventories", Order = 1, RequireRestart = false, HintText = "{=InvEnh_Sett_SearchEnabler_Hint}Enables the search bar in inventories where it's usually disabled (like loot screens).")]
    [SettingPropertyGroup("{=InvEnh_Sett_Search}Search", GroupOrder = 2)]
    public bool EnableSearchEnabler { get; set; } = true;

    [SettingPropertyBool("{=InvEnh_Sett_CaseInsensitive}Case-Insensitive Search", Order = 2, RequireRestart = false, HintText = "{=InvEnh_Sett_CaseInsensitive_Hint}Makes the inventory search bar ignore case (e.g., 'sword' matches 'Sword').")]
    [SettingPropertyGroup("{=InvEnh_Sett_Search}Search", GroupOrder = 2)]
    public bool EnableCaseInsensitiveSearch { get; set; } = true;
    #endregion

    #region Sorting
    [SettingPropertyBool("{=InvEnh_Sett_EnableSorting}Enable Custom Sorting", Order = 1, RequireRestart = false, HintText = "{=InvEnh_Sett_EnableSorting_Hint}Enables the custom sorting menu for advanced sort options like Armor, Damage, Speed, etc.")]
    [SettingPropertyGroup("{=InvEnh_Sett_Sorting}Sorting", GroupOrder = 3)]
    public bool EnableSorting { get; set; } = true;

    [SettingPropertyBool("{=InvEnh_Sett_PersistSelections}Remember Selections", Order = 3, RequireRestart = false, HintText = "{=InvEnh_Sett_PersistSelections_Hint}When enabled, your sort and filter selections will persist between inventory opens.")]
    [SettingPropertyGroup("{=InvEnh_Sett_General}General", GroupOrder = 1)]
    public bool PersistSelections { get; set; } = false;
    #endregion

    #region Filtering
    [SettingPropertyBool("{=InvEnh_Sett_EnableFiltering}Enable Custom Filtering", Order = 1, RequireRestart = false, HintText = "{=InvEnh_Sett_EnableFiltering_Hint}Enables the custom filtering menu (F button).")]
    [SettingPropertyGroup("{=InvEnh_Sett_Filtering}Filtering", GroupOrder = 4)]
    public bool EnableFiltering { get; set; } = true;
    #endregion
}
