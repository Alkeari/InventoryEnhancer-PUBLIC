using System;
using InventoryEnhancer.Helpers;
using HarmonyLib;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;

namespace InventoryEnhancer.Patches;

public static class SearchEnablerPatch
{
    private static bool _patched;

    internal static void Apply(Harmony harmony)
    {
        if (_patched)
            return;

        _patched = true;

        try
        {
            var getter =
                AccessTools.PropertyGetter(typeof(SPInventoryVM), "IsSearchAvailable") ??
                AccessTools.Method(typeof(SPInventoryVM), "get_IsSearchAvailable");

            if (getter != null)
            {
                harmony.Patch(
                    getter,
                    postfix: new HarmonyMethod(typeof(SearchEnablerPatch), nameof(IsSearchAvailableGetterPostfix)));
            }

            var setter =
                AccessTools.PropertySetter(typeof(SPInventoryVM), "IsSearchAvailable") ??
                AccessTools.Method(typeof(SPInventoryVM), "set_IsSearchAvailable");

            if (setter != null)
            {
                harmony.Patch(
                    setter,
                    prefix: new HarmonyMethod(typeof(SearchEnablerPatch), nameof(IsSearchAvailableSetterPrefix)));
            }
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("Failed to apply SearchEnablerPatch Harmony patches", ex);
        }
    }

    private static void IsSearchAvailableGetterPostfix(ref bool __result)
    {
        if (Settings.Settings.Instance != null && (!Settings.Settings.Instance.IsModEnabled || !Settings.Settings.Instance.EnableSearchEnabler)) return;
        __result = true;
    }

    private static void IsSearchAvailableSetterPrefix(ref bool value)
    {
        if (Settings.Settings.Instance != null && (!Settings.Settings.Instance.IsModEnabled || !Settings.Settings.Instance.EnableSearchEnabler)) return;
        value = true;
    }
}
