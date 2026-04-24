using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SandBox.Missions.MissionLogics.Arena;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MeleePracticeFight
{
    // ======================================================================
    //  Mission behavior – resets the melee-only flag when the mission ends.
    //  Kept as a MissionBehavior so OnEndMission fires reliably.
    // ======================================================================
    public class MeleePracticeWeaponFilter : MissionBehavior
    {
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        protected override void OnEndMission()
        {
            base.OnEndMission();
            MeleePracticeFightBehavior.ResetMeleePracticeFlag();
        }
    }

    // ======================================================================
    //  PATCH 1 – Strip ranged weapons from AgentBuildData before the agent
    //  is built, so they never spawn with them and nothing drops to the ground.
    // ======================================================================
    [HarmonyPatch(typeof(Mission), "SpawnAgent")]
    public static class Patch_Mission_SpawnAgent
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

        public static void Prefix(AgentBuildData agentBuildData)
        {
            if (!MeleePracticeFightBehavior.IsMeleePracticeActive) return;

            Equipment equipment = agentBuildData.AgentOverridenSpawnEquipment;
            if (equipment is null) return;

            for (EquipmentIndex slot = EquipmentIndex.Weapon0; slot <= EquipmentIndex.Weapon3; slot++)
            {
                EquipmentElement elem = equipment[slot];
                if (!elem.IsEmpty &&
                    elem.Item?.HasWeaponComponent is true &&
                    elem.Item.Weapons.Any(w => RangedClasses.Contains(w.WeaponClass)))
                {
                    equipment[slot] = EquipmentElement.Invalid;
                }
            }
        }
    }

    // ======================================================================
    //  PATCH 2 – Inject MeleePracticeWeaponFilter before Mission.AfterStart
    //  iterates behaviors. Must be a PREFIX so AddMissionBehavior runs before
    //  the list enumeration begins (postfix would cause Collection modified).
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
