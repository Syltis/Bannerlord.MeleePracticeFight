using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SandBox.Missions.MissionLogics.Arena;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MeleePracticeFight
{
    // ======================================================================
    //  Mission behavior – drops ranged weapons from every spawning agent
    // ======================================================================
    public class MeleePracticeWeaponFilter : MissionBehavior
    {
        private static readonly HashSet<WeaponClass> RangedClasses = new HashSet<WeaponClass>
        {
            WeaponClass.Bow,
            WeaponClass.Crossbow,
            WeaponClass.Stone,
            WeaponClass.ThrowingAxe,
            WeaponClass.ThrowingKnife,
            WeaponClass.Javelin,
            WeaponClass.Arrow,
            WeaponClass.Bolt,
        };

        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            base.OnAgentBuild(agent, banner);
            for (EquipmentIndex slot = EquipmentIndex.Weapon0; slot <= EquipmentIndex.Weapon3; slot++)
            {
                MissionWeapon weapon = agent.Equipment[slot];
                if (weapon.IsEmpty) continue;
                if (weapon.Item?.HasWeaponComponent == true &&
                    weapon.Item.Weapons.Any(w => RangedClasses.Contains(w.WeaponClass)))
                    agent.DropItem(slot);
            }
        }

        protected override void OnEndMission()
        {
            base.OnEndMission();
            MeleePracticeFightBehavior.ResetMeleePracticeFlag();
        }
    }

    // ======================================================================
    //  PATCH 1 – Inject weapon filter before Mission.AfterStart iterates behaviors.
    //  Must be a PREFIX on Mission (not a postfix on a behavior inside it) so that
    //  AddMissionBehavior runs before the list enumeration begins.
    // ======================================================================
    [HarmonyPatch(typeof(Mission), "AfterStart")]
    public static class Patch_Mission_AfterStart
    {
        public static void Prefix()
        {
            if (!MeleePracticeFightBehavior.IsMeleePracticeActive) return;
            if (Mission.Current?.GetMissionBehavior<ArenaPracticeFightMissionController>() == null) return;
            Mission.Current.AddMissionBehavior(new MeleePracticeWeaponFilter());
        }
    }
}
