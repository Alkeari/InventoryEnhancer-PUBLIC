using System;
using InventoryEnhancer.Behaviors;
using InventoryEnhancer.Helpers;
using InventoryEnhancer.Patches;
using InventoryEnhancer.UI;
using Bannerlord.UIExtenderEx;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Localization;

namespace InventoryEnhancer;

public sealed class SubModule : MBSubModuleBase
{
    private Harmony? _harmony;
    private readonly UIExtender _uiExtender = UIExtender.Create("InventoryEnhancer");

    protected override void OnSubModuleLoad()
    {
        base.OnSubModuleLoad();
        InventoryEnhancerLogger.Log(">>> OnSubModuleLoad");

        try
        {
            _harmony ??= new Harmony("InventoryEnhancer");

            SearchEnablerPatch.Apply(_harmony);
            InventoryEnhancerLogger.Log("  SearchEnablerPatch applied.");
            SearchHelperPatch.Apply(_harmony);
            InventoryEnhancerLogger.Log("  SearchHelperPatch applied.");
            FilterPatch.Apply(_harmony);
            InventoryEnhancerLogger.Log("  FilterPatch applied.");
            SortPatch.Apply(_harmony);
            InventoryEnhancerLogger.Log("  SortPatch applied.");

            _uiExtender.Register(typeof(SubModule).Assembly);
            InventoryEnhancerLogger.Log("  UIExtenderEx registered.");
            _uiExtender.Enable();
            InventoryEnhancerLogger.Log("  UIExtenderEx enabled.");

            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject("{=InvEnh_Msg_Loaded}Inventory Enhancer: Loaded.").ToString(), Colors.Green));

            InventoryEnhancerLogger.Log("<<< OnSubModuleLoad [SUCCESS]");
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("OnSubModuleLoad FAILED", ex);
            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject("{=InvEnh_Msg_FailedInit}Inventory Enhancer: Failed to initialize. Check the log for details.").ToString(), Colors.Red));
        }
    }

    protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
    {
        InventoryEnhancerLogger.Log($">>> OnGameStart (GameType={game?.GameType?.GetType().Name})");
        try
        {
            base.OnGameStart(game, gameStarterObject);

            if (gameStarterObject is CampaignGameStarter campaignStarter)
            {
                campaignStarter.AddBehavior(new Behavior());
                InventoryEnhancerLogger.Log("  Behavior registered.");
            }
            else
            {
                InventoryEnhancerLogger.Log("  Not a campaign - Behavior not registered.");
            }

            InventoryEnhancerLogger.Log("<<< OnGameStart [SUCCESS]");
        }
        catch (Exception ex)
        {
            InventoryEnhancerLogger.LogError("OnGameStart FAILED", ex);
        }
    }
}
