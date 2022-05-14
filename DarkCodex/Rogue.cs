using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Blueprints.Classes.Spells;
using Shared;
using CodexLib;

namespace DarkCodex
{
    public class Rogue
    {
        [PatchInfo(Severity.Create, "Bleeding Attack", "rogue talent: Bleeding Attack; basic talent: Flensing Strike", false)]
        public static void CreateBleedingAttack()
        {
            var RogueTalentSelection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("c074a5d615200494b8f2a9c845799d93");
            var SneakAttack = Helper.ToRef<BlueprintFeatureReference>("9b9eac6709e1c084cb18c3a366e0ec87");

            BlueprintFeature flensing = Helper.CreateBlueprintFeature(
                "FlensingStrike",
                "Flensing Strike",
                "When you successfully inflict sneak attack damage on a foe with a slashing weapon, your attack doesn’t go particularly deep, but you do carve away a significant portion of skin and flesh. If this sneak attack inflicts bleed damage, the victim of the sneak attack is sickened by the pain and has its natural armor bonus (if any) reduced by a number of points equal to the number of sneak attack dice you possess. These penalties persist as long as the bleed damage persists. Multiple strikes on the same foe do not stack the bleed damage, but the penalty to natural armor does stack, to a maximum penalty equal to the target’s normal full natural armor score."
                );

            var bleeding = Helper.CreateBlueprintFeature(
                "RogueBleedingAttack",
                "Bleeding Attack",
                "A rogue with this ability can cause living opponents to bleed by hitting them with a sneak attack. This attack causes the target to take 1 additional point of damage each round for each die of the rogue’s sneak attack (e.g., 4d6 equals 4 points of bleed). Bleeding creatures take that amount of damage every round at the start of each of their turns. The bleeding can be stopped by a DC 15 Heal check or the application of any effect that heals hit point damage.\nSpecial: Bleeding damage from this ability does not stack with itself. Bleeding damage bypasses any damage reduction the creature might possess.",
                ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("75039846c3d85d940aa96c249b97e562").Icon
                ).SetComponents(
                Helper.CreatePrerequisiteFeature(SneakAttack),
                Helper.CreateAddInitiatorAttackWithWeaponTrigger(
                    Helper.CreateActionList(Helper.CreateConditional(
                        condition: new ContextConditionCasterHasFact { m_Fact = flensing.ToRef2() },
                        ifTrue: new ContextActionIncreaseBleed(true),
                        ifFalse: new ContextActionIncreaseBleed(false))),
                    OnlySneakAttack: true),
                Helper.CreateSpellDescriptorComponent(SpellDescriptor.Bleed),
                Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, customProperty: Resource.Cache.PropertySneakAttackDice)
                );
            bleeding.Groups = new FeatureGroup[] { FeatureGroup.RogueTalent, FeatureGroup.SlayerTalent, FeatureGroup.VivisectionistDiscovery };

            flensing.SetComponents(
                Helper.CreatePrerequisiteFullStatValue(StatType.SneakAttack, 3),
                Helper.CreatePrerequisiteFeature(bleeding.ToRef())
                );

            Helper.AddRogueFeat(bleeding);
            Helper.AddFeats(flensing);
        }

        [PatchInfo(Severity.Create, "Extra Rogue Talent", "basic feat: Extra Rogue Talent", false)]
        public static void CreateExtraRogueTalent()
        {
            var RogueTalentSelection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("c074a5d615200494b8f2a9c845799d93");
            var RogueClass = Helper.ToRef<BlueprintCharacterClassReference>("299aa766dee3cbf4790da4efb8c72484");

            var extrarogue = Helper.CreateBlueprintFeatureSelection(
                "ExtraRogueTalent",
                "Extra Rogue Talent",
                "You gain one additional rogue talent. You must meet all of the prerequisites for this rogue talent.",
                group: FeatureGroup.Feat
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(RogueClass, 1)
                );
            extrarogue.Ranks = 10;
            extrarogue.m_AllFeatures = RogueTalentSelection.m_AllFeatures;

            Helper.AddFeats(extrarogue);
        }
    }
}
