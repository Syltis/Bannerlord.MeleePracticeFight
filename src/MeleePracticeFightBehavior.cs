using System;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;

namespace MeleePracticeFight
{
    public class MeleePracticeFightBehavior : CampaignBehaviorBase
    {
        internal static bool IsMeleePracticeActive { get; private set; } = false;

        // Cached via reflection since ArenaMasterCampaignBehavior is internal to SandBox.dll
        private static MethodInfo? _enterPracticeConsequence;

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
                optionText:  "{=mpf_menu}Practice Fight (Melee Only)",
                condition:   MenuCondition,
                consequence: MenuConsequence,
                isLeave:     false,
                index:       3);
        }

        private bool MenuCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Mission;
            Settlement? s = Settlement.CurrentSettlement;
            return s != null && s.IsTown;
        }

        private static void Msg(string text) =>
            System.Diagnostics.Debug.WriteLine($"[MPF] {text}");

        private void MenuConsequence(MenuCallbackArgs args)
        {
            IsMeleePracticeActive = true;

            // Resolve ArenaMasterCampaignBehavior.game_menu_enter_practice_fight_on_consequence once and cache it.
            // This is the exact code path the vanilla "Enter Practice Fight" button uses.
            object? behaviorInstance = null;
            if (_enterPracticeConsequence == null)
            {
                var sandboxAsm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "SandBox");
                if (sandboxAsm == null) { Msg("ERROR: SandBox assembly not found"); ResetMeleePracticeFlag(); return; }

                var behaviorType = sandboxAsm.GetType("SandBox.CampaignBehaviors.ArenaMasterCampaignBehavior");
                if (behaviorType == null) { Msg("ERROR: ArenaMasterCampaignBehavior not found"); ResetMeleePracticeFlag(); return; }

                _enterPracticeConsequence = behaviorType.GetMethod(
                    "game_menu_enter_practice_fight_on_consequence",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (_enterPracticeConsequence == null) { Msg("ERROR: consequence method not found"); ResetMeleePracticeFlag(); return; }

                Msg("Resolved consequence method");
            }

            // Get the live behavior instance from Campaign
            behaviorInstance = Campaign.Current.CampaignBehaviorManager
                .GetBehaviors<CampaignBehaviorBase>()
                .FirstOrDefault(b => b.GetType().Name == "ArenaMasterCampaignBehavior");
            if (behaviorInstance == null) { Msg("ERROR: behavior instance not found"); ResetMeleePracticeFlag(); return; }

            try
            {
                _enterPracticeConsequence.Invoke(behaviorInstance, new object[] { args });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException ?? ex;
                Msg($"ERROR invoking: {inner.GetType().Name}: {inner.Message}");
                ResetMeleePracticeFlag();
            }
        }

        internal static void ResetMeleePracticeFlag() => IsMeleePracticeActive = false;
    }
}
