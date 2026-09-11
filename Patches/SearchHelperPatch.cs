using System;
using InventoryEnhancer.Helpers;
using HarmonyLib;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;

namespace InventoryEnhancer.Patches;

public static class SearchHelperPatch
{
    private static AccessTools.FieldRef<SPInventoryVM, string?>? _leftSearchTextRef;
    private static AccessTools.FieldRef<SPInventoryVM, string?>? _rightSearchTextRef;
    private static bool _initialized;
    private static bool _patched;

    internal static void Warmup()
    {
        if (_initialized)
            return;

        try
        {
            _leftSearchTextRef = AccessTools.FieldRefAccess<SPInventoryVM, string?>("_leftSearchText");
            _rightSearchTextRef = AccessTools.FieldRefAccess<SPInventoryVM, string?>("_rightSearchText");
        }
        finally
        {
            _initialized = true;
        }
    }

    internal static void Apply(Harmony harmony)
    {
        if (_patched)
            return;

        _patched = true;

        try
        {
            var target = AccessTools.Method(typeof(SPInventoryVM), "UpdateFilteredStatusOfItem");
            if (target == null)
                return;

            harmony.Patch(
                target,
                prefix: new HarmonyMethod(typeof(SearchHelperPatch), nameof(Prefix)),
                postfix: new HarmonyMethod(typeof(SearchHelperPatch), nameof(Postfix)),
                finalizer: new HarmonyMethod(typeof(SearchHelperPatch), nameof(Finalizer)));
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("Failed to apply SearchHelperPatch Harmony patches", ex);
        }
    }

    public static void Prefix(SPInventoryVM __instance, out string?[] __state)
    {
        __state = new string?[2];
        try
        {
            if (Settings.Settings.Instance != null && (!Settings.Settings.Instance.IsModEnabled || !Settings.Settings.Instance.EnableCaseInsensitiveSearch)) return;

            Warmup();

            if (_leftSearchTextRef != null)
            {
                var value = _leftSearchTextRef(__instance);
                if (value != null)
                {
                    __state[0] = value;
                    _leftSearchTextRef(__instance) = value.ToLowerInvariant();
                }
            }

            if (_rightSearchTextRef != null)
            {
                var value = _rightSearchTextRef(__instance);
                if (value != null)
                {
                    __state[1] = value;
                    _rightSearchTextRef(__instance) = value.ToLowerInvariant();
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    public static void Postfix(SPInventoryVM __instance, string?[]? __state)
    {
        if (__state == null)
            return;

        Restore(__instance, __state);
    }

    public static Exception? Finalizer(SPInventoryVM __instance, string?[]? __state, Exception? __exception)
    {
        if (__exception != null && __state != null)
        {
            Restore(__instance, __state);
        }

        return __exception;
    }

    private static void Restore(SPInventoryVM __instance, string?[] __state)
    {
        try
        {
            if (_leftSearchTextRef != null && __state[0] != null)
                _leftSearchTextRef(__instance) = __state[0];

            if (_rightSearchTextRef != null && __state[1] != null)
                _rightSearchTextRef(__instance) = __state[1];
        }
        catch
        {
            // ignored
        }
    }
}
