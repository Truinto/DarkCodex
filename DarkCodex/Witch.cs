using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using DarkCodex.Components;
using Kingmaker.Designers.Mechanics.Facts;

namespace DarkCodex
{
    public class Witch
    {
        public static void createExtraHex()
        {
            var witch_class = Helper.ToRef<BlueprintCharacterClassReference>("1b9873f1e7bfe5449bc84d03e9c8e3cc"); //WitchClass
            var hexcrafter_class = Helper.ToRef<BlueprintArchetypeReference>("79ccf7a306a5d5547bebd97299f6fc89"); //HexcrafterArchetype
            var witch_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("9846043cf51251a4897728ed6e24e76f"); //WitchHexSelection
            var shaman_class = Helper.ToRef<BlueprintCharacterClassReference>("145f1d3d360a7ad48bd95d392c81b38e"); //ShamanClass
            var shaman_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("4223fe18c75d4d14787af196a04e14e7"); //ShamanHexSelection

            var witch_extra = Helper.CreateBlueprintFeatureSelection(
                "WitchHexExtra",
                "Extra Hex",
                "You gain one additional hex. You must meet the prerequisites for this hex. You can take this feat multiple times. Each time you do, you gain another hex.",
                group: FeatureGroup.WitchHex
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(witch_class, 1, true),
                Helper.CreatePrerequisiteArchetypeLevel(hexcrafter_class, 1, true)
                );
            witch_extra.Ranks = 10;
            witch_extra.m_AllFeatures = witch_selection.m_AllFeatures;

            var shaman_extra = Helper.CreateBlueprintFeatureSelection(
                "ShamanhHexExtra",
                "Extra Hex",
                "You gain one additional hex. You must meet the prerequisites for this hex. You can take this feat multiple times. Each time you do, you gain another hex.",
                group: FeatureGroup.ShamanHex
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(shaman_class, 1)
                );
            shaman_extra.Ranks = 10;
            shaman_extra.m_AllFeatures = shaman_selection.m_AllFeatures;

            Helper.AddFeats(witch_extra, shaman_extra);
        }

        public static void createHexStrike()
        {

        }

        public static void createSplitHex()
        {
            //DublicateSpellComponent
        }

        public static void createCackleActivatable()
        {
            var cackle_feat = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("36f2467103d4635459d412fb418276f4");
            var cackle = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("4bd01292a9bc4304f861a6a07f03b855");
            var chant_feat = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("3f776576b5f27604a9dad54d361153af");
            var chant = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("6cd07c80aabf2b248a11921090de9c17");
            var sfx = new PrefabLink() { AssetId = "79665f3d500fdf44083feccf4cbfc00a" };

            var consume_move = new ActivatableAbilityUnitCommand { Type = CommandType.Move };

            var cackle_addarea = Helper.CreateBlueprintAbilityAreaEffect(
                "CacklePassiveArea",
                shape: AreaEffectShape.Cylinder,
                applyAlly: true,
                applyEnemy: true,
                size: 30.Feet(),
                sfx: sfx,
                unitRound: cackle.GetComponent<AbilityEffectRunAction>().Actions
                ).CreateAddAreaEffect();
            var cackle_passiv = Helper.CreateBlueprintActivatableAbility(
                "WitchHexCacklePassive",
                "Cackle (passive)",
                cackle.m_Description, 
                out BlueprintBuff cackle_buff,
                icon: cackle.Icon,
                commandType: CommandType.Move,
                deactivateWhenStunned: true,
                deactivateWhenDead: true
                ).SetComponents(consume_move);
            cackle_buff.SetComponents(cackle_addarea);

            var chant_addarea = Helper.CreateBlueprintAbilityAreaEffect(
                "ChantPassiveArea",
                shape: AreaEffectShape.Cylinder,
                applyAlly: true,
                applyEnemy: true,
                size: 30.Feet(),
                sfx: sfx,
                unitRound: chant.GetComponent<AbilityEffectRunAction>().Actions
                ).CreateAddAreaEffect();
            var chant_passiv = Helper.CreateBlueprintActivatableAbility(
                "ShamanHexChantPassive",
                "Chant (passive)",
                chant.m_Description,
                out BlueprintBuff chant_buff,
                icon: chant.Icon,
                commandType: CommandType.Move,
                deactivateWhenStunned: true,
                deactivateWhenDead: true
                ).SetComponents(consume_move);
            chant_buff.SetComponents(chant_addarea);

            Helper.AppendAndReplace(ref cackle_feat.GetComponent<AddFacts>().m_Facts, cackle_passiv.ToRef());
            Helper.AppendAndReplace(ref chant_feat.GetComponent<AddFacts>().m_Facts, chant_passiv.ToRef());
        }

        public static void createIceTomb()
        {
            var IcyPrison = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("65e8d23aef5e7784dbeb27b1fca40931");
            var WitchMajorHex = Helper.ToRef<BlueprintFeatureReference>("8ac781b33e380c84aa578f1b006dd6c5");
            var Staggered = Helper.ToRef<BlueprintBuffReference>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var IcyPrisonParalyzedBuff = Helper.ToRef<BlueprintBuffReference>("6f0e450771cc7d446aea798e1fef1c7a");
            var WitchHexSelection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("9846043cf51251a4897728ed6e24e76f");


            var icetomb_cooldown = Helper.CreateBlueprintBuff("WitchHexIceTombCooldownBuff", "", "").Flags(hidden: true);

            var icetomb_debuff = Helper.CreateBlueprintBuff(
                "WitchHexIceTombBuff",
                "Frozen",
                "A storm of ice and freezing wind enveloped the creature.",
                fxOnStart: Helper.GetPrefabLink("21b65d177b9db1d4ca4961de15645d95") //IcyPrisonParalyzedBuff
                ).SetComponents(
                Helper.CreateAddCondition(UnitCondition.Paralyzed),
                Helper.CreateAddCondition(UnitCondition.Unconscious)
                ).Flags(isFromSpell: true, harmful: true);

            var runaction = Helper.CreateAbilityEffectRunAction(
                SavingThrowType.Fortitude,
                Helper.CreateContextActionApplyBuff(icetomb_cooldown, 1, DurationRate.Days),
                Helper.CreateContextActionDealDamage(DamageEnergyType.Cold, Helper.CreateContextDiceValue(dice: DiceType.D6, diceCount: 3, bonus: 0), halfIfSaved: true),
                Helper.CreateContextActionConditionalSaved(
                    failed: Helper.CreateContextActionApplyBuff(icetomb_debuff, Helper.CreateContextDurationValue(diceRank: AbilityRankType.Default, rate: DurationRate.Minutes), false, false),
                    succeed: Helper.CreateContextActionApplyBuff(Staggered, Helper.CreateContextDurationValue(diceCount: 1, dice: DiceType.D4), false, false)
                    )
                );

            var icetomb_ab = Helper.CreateBlueprintAbility(
                "WitchHexIceTombAbility",
                "Ice Tomb",
                "A storm of ice and freezing wind envelops the creature, which takes 3d8 points of cold damage (Fortitude half). If the target fails its save, it is paralyzed and unconscious for 1 minute/level. A successful save destroys the ice freeing the creature, which is staggered for 1d4 rounds. Whether or not the target’s saving throw is successful, it cannot be the target of this hex again for 1 day.",
                null,
                IcyPrison.Icon,
                AbilityType.Supernatural,
                CommandType.Standard,
                AbilityRange.Medium,
                Resource.Strings.MinutesPerLevel,
                Resource.Strings.FortitudePartial
                ).SetComponents(
                runaction,
                Helper.CreateAbilityTargetHasFact(true, icetomb_cooldown.ToRef2()),
                Helper.CreateSpellDescriptorComponent(SpellDescriptor.Hex | SpellDescriptor.Cold)
                ).TargetEnemy();

            var icetomb = Helper.CreateBlueprintFeature(
                "WitchHexIceTombFeature",
                icetomb_ab.m_DisplayName,
                icetomb_ab.m_Description,
                null,
                IcyPrison.Icon,
                FeatureGroup.WitchHex
                ).SetComponents(
                Helper.CreateAddFacts(icetomb_ab.ToRef2()),
                Helper.CreatePrerequisiteFeature(WitchMajorHex)
                );

            Helper.AppendAndReplace(ref WitchHexSelection.m_AllFeatures, icetomb.ToRef());
        }
    
        public static void fixBoundlessHealing()
        {
            var boundless = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c8bbb330aaecaf54dbc7570200653f8c"); //BoundlessHealing
            var heal1 = Helper.ToRef<BlueprintAbilityReference>("ed4fbfcdb0f5dcb41b76d27ed00701af"); //WitchHexHealingAbility
            var heal2 = Helper.ToRef<BlueprintAbilityReference>("3408c351753aa9049af25af31ebef624"); //WitchHexMajorHealingAbility

            var auto = boundless.GetComponent<AutoMetamagic>().Abilities;
            auto.Add(heal1);
            auto.Add(heal2);
            Helper.AppendAndReplace(ref boundless.GetComponent<AddUnlimitedSpell>().m_Abilities, heal1, heal2);
        }
    }
}
