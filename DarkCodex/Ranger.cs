using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public class Ranger
    {
        [PatchInfo(Severity.Create, "Improved Hunters Bond", "combat feat: Improved Hunter's Bond", false)]
        public static void CreateImprovedHuntersBond()
        {
            var buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("2f93cad6b132aac4e80728d7fa03a8aa"); //HuntersBondBuff
            var ability = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("cd80ea8a7a07a9d4cb1a54e67a9390a5"); //HuntersBondAbility
            var ranger = Helper.ToRef<BlueprintCharacterClassReference>("cda0615668a6df14eb36ba19ee881af6"); //RangerClass
            var huntersbond = Helper.ToRef<BlueprintFeatureReference>("6dddf5ba2291f41498df2df7f8fa2b35"); //HuntersBondFeature

            var improvedFeat = Helper.CreateBlueprintFeature(
                "ImprovedHuntersBond",
                "Improved Hunter’s Bond",
                "When you activate hunter’s bond, you can grant your allies your full favored enemy bonus against a single target."
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(ranger, 9),
                Helper.CreatePrerequisiteFeature(huntersbond)
                );
            improvedFeat.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.CombatFeat };

            var improvedBuff = Helper.CreateBlueprintBuff(
                "ImprovedHuntersBondBuff", "", "",
                icon: buff.m_Icon
                ).SetComponents(
                new ShareFavoredEnemies()
                );
            improvedBuff.m_DisplayName = buff.m_DisplayName;
            improvedBuff.m_Description = buff.m_Description;
            improvedBuff.Frequency = DurationRate.TenMinutes;

            ability.GetComponent<AbilityEffectRunAction>().Actions = Helper.CreateActionList(
                Helper.CreateConditional(
                    Helper.CreateContextConditionCasterHasFact(improvedFeat.ToRef2()),
                    Helper.CreateContextActionApplyBuff(improvedBuff, Helper.CreateContextDurationValue(AbilityRankType.Default), asChild: true),
                    Helper.CreateContextActionApplyBuff(buff, Helper.CreateContextDurationValue(AbilityRankType.Default), asChild: true)
                ));

            Helper.AddCombatFeat(improvedFeat);
        }

        public static void PatchFavoredEnemy()
        {
            // upgrade all ranks
            // RangedCleave not stacking with favored enemy
        }
    }
}
