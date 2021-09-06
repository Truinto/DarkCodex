using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using DarkCodex.Components;
using Kingmaker.Utility;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using static Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Designers.EventConditionActionSystem.Actions;

namespace DarkCodex
{
    public class Kineticist
    {
        public static void createKineticistBackground()
        {
            var x = Helper.CreateBlueprintFeature(
                "BackgroundElementalist",
                "Elemental Plane Outsider",
                "Elemental Plane Outsider count as 1 level higher for determining Kineticist levels.",
                null,
                null,
                0,
                Helper.CreateClassLevelsForPrerequisites("42a455d9ec1ad924d889272429eb8391", 1)); //kineticist class

            Helper.AppendAndReplace(ref ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("fa621a249cc836f4382ca413b976e65e").m_AllFeatures, x.ToRef());
        }

        // known issue:
        // - gathering long consumes the remaining move range (cannot fix)
        // - gathering long works while weapon is equiped
        public static void createMobileGatheringFeat()
        {
            // --- base game stuff ---
            var buff1 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("e6b8b31e1f8c524458dc62e8a763cfb1");   //GatherPowerBuffI
            var buff2 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("3a2bfdc8bf74c5c4aafb97591f6e4282");   //GatherPowerBuffII
            var buff3 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("82eb0c274eddd8849bb89a8e6dbc65f8");   //GatherPowerBuffIII
            var gather_original_ab = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("6dcbffb8012ba2a4cb4ac374a33e2d9a");    //GatherPower
            var kineticist_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("42a455d9ec1ad924d889272429eb8391");

            // rename buffs, so it's easier to tell them apart
            buff1.m_Icon = gather_original_ab.Icon;
            buff1.m_DisplayName = Helper.CreateString(buff1.m_DisplayName + " Lv1");
            buff2.m_Icon = gather_original_ab.Icon;
            buff2.m_DisplayName = Helper.CreateString(buff2.m_DisplayName + " Lv2");
            buff3.m_Icon = gather_original_ab.Icon;
            buff3.m_DisplayName = Helper.CreateString(buff3.m_DisplayName + " Lv3");

            // new buff that halves movement speed, disallows normal gathering
            var mobile_debuff = Helper.CreateBlueprintBuff(
                "MobileGatheringDebuff",
                "Mobile Gathering Debuff",
                "Your movement speed is halved after gathering power.",
                null,
                Helper.CreateSprite("GatherMobileHigh.png"),
                null,
                new TurnBasedBuffMovementSpeed(multiplier: 0.5f));

            var apply_debuff = Helper.CreateContextActionApplyBuff(mobile_debuff, 1);
            var can_gather = Helper.CreateAbilityRequirementHasBuffTimed(CompareType.LessOrEqual, 1.Rounds().Seconds, buff1, buff2, buff3);

            // cannot use usual gathering after used mobile gathering
            gather_original_ab.AddComponents(Helper.CreateAbilityRequirementHasBuffs(true, mobile_debuff));

            // ability as free action that applies buff and 1 level of gatherpower
            // - is free action
            // - increases gather power by 1 level, similiar to GatherPower:6dcbffb8012ba2a4cb4ac374a33e2d9a
            // - applies debuff
            // - get same restriction as usual gathering
            var three2three = Helper.CreateConditional(Helper.CreateContextConditionHasBuff(buff3), Helper.CreateContextActionApplyBuff(buff3, 2));
            var two2three = Helper.CreateConditional(Helper.CreateContextConditionHasBuff(buff2).ObjToArray(), new GameAction[] { Helper.CreateContextActionRemoveBuff(buff2), Helper.CreateContextActionApplyBuff(buff3, 2) });
            var one2two = Helper.CreateConditional(Helper.CreateContextConditionHasBuff(buff1).ObjToArray(), new GameAction[] { Helper.CreateContextActionRemoveBuff(buff1), Helper.CreateContextActionApplyBuff(buff2, 2) });
            var zero2one = Helper.CreateConditional(Helper.CreateConditionHasNoBuff(buff1, buff2, buff3), new GameAction[] { Helper.CreateContextActionApplyBuff(buff1, 2) });
            var regain_halfmove = new ContextActionUndoAction(command: UnitCommand.CommandType.Move);
            var mobile_gathering_short_ab = Helper.CreateBlueprintAbility(
                "MobileGatheringShort",
                "Mobile Gathering (Move Action)",
                "You may move up to half your normal speed while gathering power.",
                null,
                Helper.CreateSprite("GatherMobileLow.png"),
                AbilityType.Special,
                UnitCommand.CommandType.Move,
                AbilityRange.Personal,
                "",
                "",
                can_gather,
                Helper.CreateAbilityEffectRunAction(0, regain_halfmove, apply_debuff, three2three, two2three, one2two, zero2one));
            mobile_gathering_short_ab.CanTargetSelf = true;
            mobile_gathering_short_ab.Animation = CastAnimationStyle.Self;//UnitAnimationActionCastSpell.CastAnimationStyle.Kineticist;
            mobile_gathering_short_ab.HasFastAnimation = true;

            // same as above but standard action and 2 levels of gatherpower
            var one2three = Helper.CreateConditional(Helper.CreateContextConditionHasBuff(buff1).ObjToArray(), new GameAction[] { Helper.CreateContextActionRemoveBuff(buff1), Helper.CreateContextActionApplyBuff(buff3, 2) });
            var zero2two = Helper.CreateConditional(Helper.CreateConditionHasNoBuff(buff1, buff2, buff3), new GameAction[] { Helper.CreateContextActionApplyBuff(buff2, 2) });
            var hasMoveAction = Helper.CreateAbilityRequirementActionAvailable(false, ActionType.Move, 4.5f);
            var lose_halfmove = new ContextActionUndoAction(command: UnitCommand.CommandType.Move, amount: -1.5f);
            var mobile_gathering_long_ab = Helper.CreateBlueprintAbility(
                "MobileGatheringLong",
                "Mobile Gathering (Full Round)",
                "You may move up to half your normal speed while gathering power.\nNote: Use half your speed before using this.",
                null,
                Helper.CreateSprite("GatherMobileMedium.png"),
                AbilityType.Special,
                UnitCommand.CommandType.Standard,
                AbilityRange.Personal,
                "",
                "",
                can_gather,
                hasMoveAction,
                Helper.CreateAbilityEffectRunAction(0, lose_halfmove, apply_debuff, three2three, two2three, one2three, zero2two));
            mobile_gathering_long_ab.CanTargetSelf = true;
            mobile_gathering_long_ab.Animation = CastAnimationStyle.Self;
            mobile_gathering_long_ab.HasFastAnimation = true;

            var mobile_gathering_feat = Helper.CreateBlueprintFeature(
                "MobileGatheringFeat",
                "Mobile Gathering",
                "While gathering power, you can move up to half your normal speed. This movement provokes attacks of opportunity as normal.",
                null,
                mobile_debuff.Icon,
                FeatureGroup.Feat,
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 7, true),
                Helper.CreateAddFacts(mobile_gathering_short_ab, mobile_gathering_long_ab));
            mobile_gathering_feat.Ranks = 1;
            mobile_gathering_feat.IsClassFeature = true;
            Helper.AddFeats(mobile_gathering_feat);

            // make original gather ability visible for manual gathering and allow to extend buff3
            gather_original_ab.Hidden = false;
            var hasBuff2 = gather_original_ab.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as Conditional;
            Helper.AppendAndReplace(ref hasBuff2.ConditionsChecker.Conditions, Helper.CreateContextConditionHasBuff(buff3));
        }
    }

    #region Patches

    /// <summary>
    /// Normal: Healing burn turns its non-lethal damage into actual damage.
    /// Patched: Healing burn heals non-lethal damage completely.
    /// </summary>
    [HarmonyPatch(typeof(UnitPartKineticist), nameof(UnitPartKineticist.HealBurn))]
    public class Patch_HealBurnDamage
    {
        public static void Postfix(int value, UnitPartKineticist __instance)
        {
            __instance.Owner.Damage -= value * __instance.Owner.Progression.CharacterLevel;
        }
    }

    /// <summary>
    /// Normal: The level of gathering power is determined by the mode (none, low, medium, high) selected. If the mode is lower than the already accumulated gather level, than levels are lost.
    /// Patched: The level of gathering is true to the accumulated level or the selected mode, whatever is higher.
    /// </summary>
    [HarmonyPatch(typeof(KineticistController), nameof(KineticistController.TryApplyGatherPower))]
    public class Patch_TrueGatherPowerLevel
    {
        public static BlueprintBuff buff1 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("e6b8b31e1f8c524458dc62e8a763cfb1");
        public static BlueprintBuff buff2 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("3a2bfdc8bf74c5c4aafb97591f6e4282");
        public static BlueprintBuff buff3 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("82eb0c274eddd8849bb89a8e6dbc65f8");

        public static bool Prefix(UnitPartKineticist kineticist, BlueprintAbility abilityBlueprint, ref KineticistAbilityBurnCost cost)
        {
            if (kineticist == null || abilityBlueprint.GetComponent<AbilityKineticist>() == null || kineticist.GatherPowerAbility == null)
                return false;

            int buffRank = kineticist.TargetGatherPowerRank; // get the target power rank

            // check if stronger buff exists and if so apply it instead
            if (buffRank < 1 && kineticist.Owner.Buffs.GetBuff(buff1) != null)
                buffRank = 1;
            else if (buffRank < 2 && kineticist.Owner.Buffs.GetBuff(buff2) != null)
                buffRank = 2;
            else if (buffRank < 3 && kineticist.Owner.Buffs.GetBuff(buff3) != null)
                buffRank = 3;

            int value = KineticistUtils.CalculateGatherPowerBonus(kineticist.GatherPowerBaseValue, buffRank); // add increase from Supercharge
            
            cost.IncreaseGatherPower(value); // apply value

            return false;
        }
    }

    #endregion
}
