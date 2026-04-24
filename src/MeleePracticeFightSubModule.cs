using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MeleePracticeFight
{
    public class MeleePracticeFightSubModule : MBSubModuleBase
    {
        private static readonly Harmony _harmony = new("com.meleepracticefight");

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            _harmony.PatchAll();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (gameStarterObject is CampaignGameStarter campaignStarter)
                campaignStarter.AddBehavior(new MeleePracticeFightBehavior());
        }
    }
}
