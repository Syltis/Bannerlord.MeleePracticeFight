using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;

namespace MeleePracticeFight
{
    public class MeleePracticeFightBehavior : CampaignBehaviorBase
    {
        internal static bool IsMeleePracticeActive { get; private set; }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            starter.AddGameMenuOption(
                menuId:      "town_arena",
                optionId:    "town_arena_melee_practice",
                optionText:  "{=mpf_menu}Practice Fight (Melee)",
                condition:   MenuCondition,
                consequence: MenuConsequence,
                isLeave:     false);
        }

        private static bool MenuCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.PracticeFight;
            return Settlement.CurrentSettlement?.IsTown is true;
        }

        private static void MenuConsequence(MenuCallbackArgs args)
        {
            IsMeleePracticeActive = true;

            PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(
                LocationComplex.Current.GetLocationWithId("arena"), null, null, null);

            // Mirror _enteredPracticeFightFromMenu = true from the vanilla consequence
            // so that rewards and end-of-fight logic behave identically
            var instance = Campaign.Current.CampaignBehaviorManager
                .GetBehaviors<CampaignBehaviorBase>()
                .FirstOrDefault(b => b.GetType().Name == "ArenaMasterCampaignBehavior");
            instance?.GetType()
                .GetField("_enteredPracticeFightFromMenu", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(instance, true);
        }

        internal static void ResetMeleePracticeFlag() => IsMeleePracticeActive = false;
    }
}
