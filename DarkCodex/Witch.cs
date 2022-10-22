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
using Kingmaker.Designers.Mechanics.Facts;
using Shared;
using CodexLib;
using Kingmaker.Blueprints.Classes.Prerequisites;

namespace DarkCodex
{
    public class Witch
    {
        [PatchInfo(Severity.Create, "Extra Hex", "basic feat: Extra Hex", false)]
        public static void CreateExtraHex()
        {
            var witch_class = Helper.ToRef<BlueprintCharacterClassReference>("1b9873f1e7bfe5449bc84d03e9c8e3cc"); //WitchClass
            var hexcrafter_class = Helper.ToRef<BlueprintArchetypeReference>("79ccf7a306a5d5547bebd97299f6fc89"); //HexcrafterArchetype
            var witch_selection = Helper.Get<BlueprintFeatureSelection>("9846043cf51251a4897728ed6e24e76f"); //WitchHexSelection
            var shaman_class = Helper.ToRef<BlueprintCharacterClassReference>("145f1d3d360a7ad48bd95d392c81b38e"); //ShamanClass
            var shaman_selection = Helper.Get<BlueprintFeatureSelection>("4223fe18c75d4d14787af196a04e14e7"); //ShamanHexSelection

            var witch_extra = Helper.CreateBlueprintFeatureSelection(
                "WitchHexExtra",
                "Extra Hex",
                "You gain one additional hex. You must meet the prerequisites for this hex. You can take this feat multiple times. Each time you do, you gain another hex.",
                group: FeatureGroup.WitchHex
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(witch_class, 1, true),
                Helper.CreatePrerequisiteArchetypeLevel(hexcrafter_class, 1, true, characterClass: Helper.ToRef<BlueprintCharacterClassReference>("45a4607686d96a1498891b3286121780"))
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

        [PatchInfo(Severity.Create | Severity.Faulty, "Hex Strike", "", false)]
        public static void CreateHexStrike()
        {
            /*
            Chanting and cursing, you put a hex on your enemy as part of your unarmed strike.
            Prerequisite: Hex class feature, Improved Unarmed Strike.
            Benefit: Choose one hex that you can use to affect no more than one opponent. If you make a successful unarmed strike against an opponent, in addition to dealing your unarmed strike damage, you can use a swift action to deliver the effects of the chosen hex to that opponent. Doing so does not provoke attacks of opportunity.
             */

            var act = Helper.CreateBlueprintActivatableAbility(
                "HexStrikeActivatable",
                out var _, 
                "Hex Strike",
                "Chanting and cursing, you put a hex on your enemy as part of your unarmed strike. Benefit: Choose one hex that you can use to affect no more than one opponent. If you make a successful unarmed strike against an opponent, in addition to dealing your unarmed strike damage, you can use a swift action to deliver the effects of the chosen hex to that opponent. Doing so does not provoke attacks of opportunity.",
                icon: Helper.StealIcon("85067a04a97416949b5d1dbf986d93f3")
                ).SetComponents(
                new HexStrike()
                );

            var feat = Helper.CreateBlueprintFeature(
                "HexStrike"
                ).SetUIData(act);
        }

        [PatchInfo(Severity.Create | Severity.WIP, "Split Hex", "basic feat: Split Hex, Split Major Hex", false)]
        public static void CreateSplitHex()
        {
            var major = Helper.ToRef<BlueprintFeatureReference>("8ac781b33e380c84aa578f1b006dd6c5"); //WitchMajorHex
            var grand = Helper.ToRef<BlueprintFeatureReference>("d24c2467804ce0e4497d9978bafec1f9"); //WitchGrandHex

            var splitHex = Helper.CreateBlueprintFeature(
                "SplitHex",
                "Split Hex",
                "When you use one of your hexes (not a major hex or a grand hex) that targets a single creature, you can choose another creature within 30 feet of the first target to also be targeted by the hex.",
                group: FeatureGroup.Feat
                ).SetComponents(
                Helper.CreateDuplicateSpell(f => !f.IsAOE && f.Blueprint.SpellDescriptor.HasFlag(SpellDescriptor.Hex) && f.IsRank(max: 0)),
                Helper.CreatePrerequisiteFeature(major)
                );

            var splitMajorHex = Helper.CreateBlueprintFeature(
                "SplitMajorHex",
                "Split Major Hex",
                "When you use one of your major hexes (not a grand hex) that targets a creature, you can choose another creature within 30 feet of the first target to also be targeted by the major hex.",
                group: FeatureGroup.Feat
                ).SetComponents(
                Helper.CreateDuplicateSpell(f => !f.IsAOE && f.Blueprint.SpellDescriptor.HasFlag(SpellDescriptor.Hex) && f.IsRank(min: 1, max: 1, ifEmpty: false)),
                Helper.CreatePrerequisiteFeature(grand),
                Helper.CreatePrerequisiteFeature(splitHex)
                );

            Helper.AddFeats(splitHex, splitMajorHex);

            Main.RunLast("SplitHex", () =>
            {
                foreach (var feat in Resource.Cache.Feature)
                {
                    var preq = feat.GetComponents<PrerequisiteFeature>();
                    int rank = preq.Any(a => a.m_Feature.Equals(major)) ? 1 : preq.Any(a => a.m_Feature.Equals(grand)) ? 2 : 0;
                    if (rank == 0)
                        continue;

                    Main.PrintDebug($"{feat.name} has rank {rank}");

                    var addfacts = feat.GetComponent<AddFacts>();
                    if (addfacts == null)
                        continue;

                    foreach (var fact in addfacts.m_Facts)
                    {
                        if (fact.Get() is BlueprintAbility ab)
                        {
                            ab.AddComponents(new FeatureRank(rank));
                            Main.PrintDebug($"adding rank {rank} to {ab.name}");
                        }
                    }
                }
            });
        }

        [PatchInfo(Severity.Create, "Cackle Activatable", "Cackle/Chant can be toggled to use move action passively", Requirement: typeof(Patch_ActivatableOnNewRound))]
        public static void CreateCackleActivatable()
        {
            var cackle_feat = Helper.Get<BlueprintFeature>("36f2467103d4635459d412fb418276f4");
            var cackle = Helper.Get<BlueprintAbility>("4bd01292a9bc4304f861a6a07f03b855");
            var chant_feat = Helper.Get<BlueprintFeature>("3f776576b5f27604a9dad54d361153af");
            var chant = Helper.Get<BlueprintAbility>("6cd07c80aabf2b248a11921090de9c17");
            var sfx = new PrefabLink() { AssetId = "79665f3d500fdf44083feccf4cbfc00a" };

            var consume_move = new ActivatableAbilityUnitCommand { Type = CommandType.Move };

            var runCackle = cackle.GetComponent<AbilityEffectRunAction>().Actions;
            var cackle_addarea = Helper.CreateBlueprintAbilityAreaEffect(
                "CacklePassiveArea",
                shape: AreaEffectShape.Cylinder,
                applyAlly: true,
                applyEnemy: true,
                size: 30.Feet(),
                sfx: sfx,
                unitEnter: null,
                unitRound: runCackle
                ).CreateAddAreaEffect();
            var cackle_passiv = Helper.CreateBlueprintActivatableAbility(
                "WitchHexCacklePassive",
                out BlueprintBuff cackle_buff,
                "Cackle (passive)",
                cackle.m_Description,
                icon: cackle.Icon,
                //activationType: AbilityActivationType.WithUnitCommand,
                //commandType: CommandType.Move,
                deactivateWhenStunned: true,
                deactivateWhenDead: true,
                deactivateImmediately: false
                ).SetComponents(
                consume_move);
            cackle_buff.SetComponents(cackle_addarea);

            var runChant = chant.GetComponent<AbilityEffectRunAction>().Actions;
            var chant_addarea = Helper.CreateBlueprintAbilityAreaEffect(
                "ChantPassiveArea",
                shape: AreaEffectShape.Cylinder,
                applyAlly: true,
                applyEnemy: true,
                size: 30.Feet(),
                sfx: sfx,
                unitEnter: null,
                unitRound: runChant
                ).CreateAddAreaEffect();
            var chant_passiv = Helper.CreateBlueprintActivatableAbility(
                "ShamanHexChantPassive",
                out BlueprintBuff chant_buff,
                "Chant (passive)",
                chant.m_Description,
                icon: chant.Icon,
                //activationType: AbilityActivationType.WithUnitCommand,
                //commandType: CommandType.Move,
                deactivateWhenStunned: true,
                deactivateWhenDead: true,
                deactivateImmediately: false
                ).SetComponents(
                consume_move);
            chant_buff.SetComponents(chant_addarea);

            Helper.AppendAndReplace(ref cackle_feat.GetComponent<AddFacts>().m_Facts, cackle_passiv.ToRef());
            Helper.AppendAndReplace(ref chant_feat.GetComponent<AddFacts>().m_Facts, chant_passiv.ToRef());
        }

        [PatchInfo(Severity.Create, "Ice Tomb", "Hex: Ice Tomb", false)]
        public static void CreateIceTomb()
        {
            var IcyPrison = Helper.Get<BlueprintAbility>("65e8d23aef5e7784dbeb27b1fca40931");
            var WitchMajorHex = Helper.ToRef<BlueprintFeatureReference>("8ac781b33e380c84aa578f1b006dd6c5");
            var Staggered = Helper.ToRef<BlueprintBuffReference>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var IcyPrisonParalyzedBuff = Helper.ToRef<BlueprintBuffReference>("6f0e450771cc7d446aea798e1fef1c7a");

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
                    succeed: Helper.CreateContextActionApplyBuff(Staggered.Get(), Helper.CreateContextDurationValue(diceCount: 1, dice: DiceType.D4), false, false)
                    )
                );

            var icetomb_ab = Helper.CreateBlueprintAbility(
                "WitchHexIceTombAbility",
                "Ice Tomb",
                "A storm of ice and freezing wind envelops the creature, which takes 3d8 points of cold damage (Fortitude half). If the target fails its save, it is paralyzed and unconscious for 1 minute/level. A successful save destroys the ice freeing the creature, which is staggered for 1d4 rounds. Whether or not the target’s saving throw is successful, it cannot be the target of this hex again for 1 day.",
                IcyPrison.Icon,
                AbilityType.Supernatural,
                CommandType.Standard,
                AbilityRange.Medium,
                Resource.Strings.MinutesPerLevel,
                Resource.Strings.FortitudePartial
                ).SetComponents(
                runaction,
                Helper.CreateAbilityTargetHasFact(true, icetomb_cooldown.ToRef2()),
                Helper.CreateSpellDescriptorComponent(SpellDescriptor.Hex | SpellDescriptor.Cold | SpellDescriptor.Paralysis)
                ).TargetEnemy();
            SetHexParams(icetomb_ab);

            var icetomb = Helper.CreateBlueprintFeature(
                "WitchHexIceTombFeature",
                icetomb_ab.m_DisplayName,
                icetomb_ab.m_Description,
                IcyPrison.Icon,
                FeatureGroup.WitchHex
                ).SetComponents(
                Helper.CreateAddFacts(icetomb_ab.ToRef2()),
                Helper.CreatePrerequisiteFeature(WitchMajorHex)
                );

            Helper.AddHex(icetomb, false);
        }

        [PatchInfo(Severity.Fix, "Boundless Healing Hex", "boundless healing applies to healing hex", false)]
        public static void FixBoundlessHealing()
        {
            var boundless = Helper.Get<BlueprintFeature>("c8bbb330aaecaf54dbc7570200653f8c"); //BoundlessHealing
            var heal1 = Helper.ToRef<BlueprintAbilityReference>("ed4fbfcdb0f5dcb41b76d27ed00701af"); //WitchHexHealingAbility
            var heal2 = Helper.ToRef<BlueprintAbilityReference>("3408c351753aa9049af25af31ebef624"); //WitchHexMajorHealingAbility

            var auto = boundless.GetComponent<AutoMetamagic>().Abilities;
            auto.Add(heal1);
            auto.Add(heal2);
            Helper.AppendAndReplace(ref boundless.GetComponent<AddUnlimitedSpell>().m_Abilities, heal1, heal2);
        }

        [PatchInfo(Severity.Fix, "Fix Fortune Hex", "Fortune hex will only trigger once per type of roll and per round", false)]
        public static void FixFortuneHex()
        {
            // note: Normally this triggers with every check. RAW it should trigger only once per round.
            // But since selecting which roll is most important is near impossible during gameplay, a compromise is made.
            // With this it triggeres once per round for each of the different types of rolls. For a maximum of 4 times:
            // AttackRoll, Maneuver, SavingThrow, SkillCheck

            var buff = Helper.Get<BlueprintBuff>("9121b87785f37f94e861b03a103d03e2"); //WitchHexFortuneBuff
            buff.SetComponents(
                new ModifyD20Once(buff.GetComponent<ModifyD20>())
                );
            buff.m_Description.CreateString("The witch can grant a creature within 30 feet a bit of good luck for 1 {g|Encyclopedia:Combat_Round}round{/g}. The target can call upon this good luck, allowing him to {g|Encyclopedia:Dice}reroll{/g}, taking the better result.\nThis works once per round for each of these types of checks:\n- {g|Encyclopedia:Attack}attack rolls{/g}\n- {g|Encyclopedia:Combat_Maneuvers}combat maneuvers{/g}\n- {g|Encyclopedia:Skills}skill{/g} or {g|Encyclopedia:Ability_Scores}ability checks{/g}\n- {g|Encyclopedia:Saving_Throw}saving throws{/g}\nAt 8th level and 16th level, the duration of this hex is extended by 1 round. Once a creature has benefited from the fortune hex, it cannot benefit from it again for 24 hours.");

            buff = Helper.Get<BlueprintBuff>("32ba2d292233b7a4cbcc3884496a68f4"); //ShamanHexFortuneBuff
            buff.SetComponents(
                new ModifyD20Once(buff.GetComponent<ModifyD20>())
                );
            buff.m_Description.CreateString("The shaman can grant a creature within 30 feet a bit of good luck for 1 {g|Encyclopedia:Combat_Round}round{/g}. The target can call upon this good luck, allowing him to {g|Encyclopedia:Dice}reroll{/g}, taking the better result.\nThis works once per round for each of these types of checks:\n- {g|Encyclopedia:Attack}attack rolls{/g}\n- {g|Encyclopedia:Combat_Maneuvers}combat maneuvers{/g}\n- {g|Encyclopedia:Skills}skill{/g} or {g|Encyclopedia:Ability_Scores}ability checks{/g}\n- {g|Encyclopedia:Saving_Throw}saving throws{/g}\nAt 8th level and 16th level, the duration of this hex is extended by 1 round. Once a creature has benefited from the fortune hex, it cannot benefit from it again for 24 hours.");
        }

        #region Helper

        private static void SetHexParams(BlueprintAbility hex)
        {
            hex.AddComponents(new ContextSetAbilityParams
            {
                DC = new ContextValue
                {
                    ValueType = ContextValueType.CasterCustomProperty,
                    m_CustomProperty = Helper.ToRef<BlueprintUnitPropertyReference>("bdc230ce338f427ba74de65597b0d57a")
                },
                CasterLevel = new ContextValue
                {
                    ValueType = ContextValueType.CasterCustomProperty,
                    m_CustomProperty = Helper.ToRef<BlueprintUnitPropertyReference>("2d2243f4f3654512bdda92e80ef65b6d")
                },
                SpellLevel = new ContextValue
                {
                    ValueType = ContextValueType.CasterCustomProperty,
                    m_CustomProperty = Helper.ToRef<BlueprintUnitPropertyReference>("75efe8b64a3a4cd09dda28cef156cfb5")
                },
                Concentration = -1,
            });
        }

        #endregion

    }
}
