using CodexLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public class Spellcasters
    {
        /// <summary>
        /// Known issues:
        /// - Seeker archetype gets too many charges of arcane adept (won't fix)
        /// </summary>
        [PatchInfo(Severity.Create, "Fix Bloodline: Arcane", "Arcane Apotheosis ignores metamagic casting time penalty", false)]
        public static void FixBloodlineArcane()
        {
            var sorcerer = Helper.ToRef<BlueprintCharacterClassReference>("b3a505fb61437dc4097f43c3f8f9a4cf"); //SorcererClass
            var scion = Helper.ToRef<BlueprintCharacterClassReference>("f5b8c63b141b2f44cbb8c2d7579c34f5"); //EldritchScionClass
            var bloodline = Helper.Get<BlueprintProgression>("4d491cf9631f7e9429444f4aed629791"); //BloodlineArcaneProgression
            var sage = Helper.Get<BlueprintProgression>("7d990675841a7354c957689a6707c6c2"); //SageBloodlineProgression
            var seeker = Helper.Get<BlueprintProgression>("562c5e4031d268244a39e01cc4b834bb"); //SeekerBloodlineArcaneProgression
            var seekerFeat = Helper.Get<BlueprintFeature>("e407e1a130324fa8866dcbfd21ba1fa7"); //SeekerBloodlineArcaneCombatCastingAdeptFeatureAddLevel9
            var adeptFeat = Helper.Get<BlueprintFeature>("82a966061553ea442b0ce0cdb4e1d49c"); //BloodlineArcaneCombatCastingAdeptFeatureLevel1
            var apotheosis = Helper.Get<BlueprintFeature>("2086d8c0d40e35b40b86d47e47fb17e4"); //BloodlineArcaneArcaneApotheosisFeature

            apotheosis.AddComponents(
                Helper.CreateRemoveFeatureOnApply(adeptFeat),
                new AddMechanicFeatureCustom(MechanicFeature.SpontaneousMetamagicNoFullRound)
                );
            apotheosis.m_Description.CreateString(apotheosis.m_Description + "\nYou can add any metamagic feats that you know to your spells without increasing their casting time, although you must still expend higher-level spell slots.");

#if false
            var feat = Helper.CreateBlueprintActivatableAbility(
                "BloodlineArcaneApotheosisActivatable",
                "Arcane Apotheosis",
                "Whenever you use magic items that require charges, you can instead expend spell slots to power the item. For every three levels of spell slots that you expend, you consume one less charge when using a magic item that expends charges.",
                out var buff
                );
            //RuleSpendCharge
#endif

            const string descAdept = "At 3rd level, you can apply any one metamagic feat you know to a spell you are about to cast without increasing the casting time. You must still expend a higher-level spell slot to cast this spell. You can use this ability once per day at 3rd level and one additional time per day for every four sorcerer levels you possess beyond 3rd, up to five times per day at 19th level. At 20th level, this ability is replaced by arcane apotheosis.";
            var resource = Helper.CreateBlueprintAbilityResource("MetamagicAdeptResource", baseValue: 1, startLevel: 3, levelDiv: 4,
                classes: new BlueprintCharacterClassReference[] { sorcerer, scion });
            var adept = Helper.CreateBlueprintActivatableAbility(
                "MetamagicAdeptActivatable",
                out var buff,
                "Metamagic Adept",
                descAdept,
                icon: adeptFeat.m_Icon
                ).SetComponents(
                Helper.CreateActivatableAbilityResourceLogic(resource, ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                );
            buff.SetComponents(new MetamagicAdeptFix(resource.ToReference<BlueprintAbilityResourceReference>()));
            adeptFeat.SetComponents(Helper.CreateAddFacts(adept), Helper.CreateAddAbilityResources(resource));
            adeptFeat.m_Description.CreateString(descAdept);
            seekerFeat.m_Description.CreateString(descAdept);

            bloodline.RemoveFeature("3d7b19c8a1d03464aafeb306342be000"); //BloodlineArcaneCombatCastingAdeptFeatureAddLevel2
            sage.RemoveFeature("3d7b19c8a1d03464aafeb306342be000");
            seeker.RemoveFeature("9cb584629d604325a1141c72ad751e17"); //SeekerBloodlineArcaneCombatCastingAdeptFeatureAddLevel15
        }

        [PatchInfo(Severity.Extend, "Arcanist Brown-Fur", "allows Share Transmutation to affect any spell", true)]
        public static void PatchArcanistBrownFur()
        {
            Main.Patch(typeof(Patch_ArcanistBrownFur));

            var trigger1 = Helper.Get<BlueprintBuff>("2231eb5d1a5a48d499a20fa5bde7a4e2").GetComponent<AddAbilityUseTrigger>(); //ShareTransmutationBuff
            var trigger2 = Helper.Get<BlueprintBuff>("e0d4e42a41a0a24459a1bfc4f0a3ae4c").GetComponent<AddAbilityUseTrigger>(); //ShareTransmutationBuffGreater

            trigger1.FromSpellbook = false;
            trigger1.CheckRange = true;
            trigger1.Range = AbilityRange.Personal;
            trigger1.CheckAbilityType = true;
            trigger1.Type = AbilityType.Spell;

            trigger2.FromSpellbook = false;
            trigger2.CheckRange = true;
            trigger2.Range = AbilityRange.Personal;
            trigger2.CheckAbilityType = true;
            trigger2.Type = AbilityType.Spell;
        }

        [PatchInfo(Severity.Create, "Purifying Channel", "basic feat: channel positive energy deals fire damage", false)]
        public static void CreatePurifyingChannel()
        {
            AnyRef feat = Helper.CreateBlueprintFeature(
                "PurifyingChannel",
                "Purifying Channel",
                "When you channel positive energy to heal, one creature that you exclude from your channeling takes an amount of fire damage equal to the die result you roll for healing, and is dazzled for 1 round by the light of these flames. A successful saving throw against your channel energy halves the fire damage and negates the dazzled effect."
                ).SetComponents(
                Helper.CreatePrerequisiteFeature("fd30c69417b434d47b6b03b9c1f568ff"), //SelectiveChannel
                Helper.CreatePrerequisiteStatValue(StatType.Charisma, 15)
                );

            var action = Helper.CreateConditional(
                new Condition[] {
                    new ContextConditionCasterHasFact { m_Fact = feat },
                    new ContextConditionIsEnemy(),
                    Helper.CreateContextConditionSharedValueHigher(AbilitySharedValue.DurationSecond, 0, Equality.LowerOrEqual)
                },
                ifTrue: new GameAction[] {
                    Helper.CreateContextActionChangeSharedValue(AbilitySharedValue.DurationSecond, add: 1),
                                        new ContextActionSavingThrow {
                        Type = SavingThrowType.Will,
                        Actions = Helper.CreateActionList(
                            Helper.CreateContextActionDealDamage(DamageEnergyType.Fire, Helper.CreateContextDiceValue(DiceType.Zero, 0, Helper.CreateContextValue(AbilitySharedValue.Heal)), false, true),
                            Helper.CreateContextActionConditionalSaved(failed: Helper.CreateContextActionApplyBuff("df6d1025da07524429afbae248845ecc", Helper.CreateContextDurationValue(bonus: 1))), //DazzledBuff
                            new ContextActionSpawnFx { PrefabLink = Helper.GetPrefabLink("61602c5b0ac793d489c008e9cb58f631") })
                    },
                });

            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("f5fc9a1a2a3c1a946a31b320d1dd31b2").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //ChannelEnergy
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("6670f0f21a1d7f04db2b8b115e8e6abf").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //ChannelEnergyPaladinHeal
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("574cf074e8b65e84d9b69a8c6f1af27b").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //ChannelEnergyEmpyrealHeal
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("0c0cf7fcb356d2448b7d57f2c4db3c0c").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //ChannelEnergyHospitalerHeal
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("b5cf6b80e65ea724d99dc9f4f8874fc3").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //WarpriestChannelEnergy
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("0fb4bb4eae14fe84e8b45d8ea207c4e1").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //ShamanLifeSpiritChannelEnergy
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("6bcaf7636388f2a40bce263372735eef").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //WarpriestShieldbearerChannelEnergy
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("d470eb6b3b31fde4bb44ec753de0b862").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //WitchDoctorChannelEnergy
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("75edd403e824aa048ab5d4827b803b08").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //HexChannelerChannelEnergy
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("b9eca127dd82f554fb2ccd804de86cf6").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //OracleRevelationChannelAbility

            Helper.AddFeats(feat);
        }

        [PatchInfo(Severity.Create, "Bestow Hope", "basic feat: channel energy reduces fear", false)]
        public static void CreateBestowHope()
        {
            var shaken = Helper.ToRef<BlueprintBuffReference>("25ec6cb6ab1845c48a95f9c20b034220"); //Shaken
            var frightend = Helper.ToRef<BlueprintBuffReference>("f08a7239aa961f34c8301518e71d4cdf"); //Frightened

            var deities = Helper.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4").m_AllFeatures.Where(w => //DeitySelection
            {
                var preq = w.Get()?.GetComponent<PrerequisiteAlignment>();
                if (preq == null)
                    return false;

                return (preq.Alignment & AlignmentMaskType.Evil) == 0;
            }).Select(s => s.ToRef<BlueprintFeatureReference>()).ToArray();

            AnyRef feat = Helper.CreateBlueprintFeature(
                "BestowHope",
                "Bestow Hope",
                "When you heal a creature by channeling positive energy, you also relieve its fear. If a creature you heal is shaken, that condition ends. If the creature is frightened, it becomes shaken instead. If the creature is panicked, it becomes frightened instead."
                ).SetComponents(
                Helper.CreatePrerequisiteFeature("a79013ff4bcd4864cb669622a29ddafb"), //ChannelEnergyFeature
                Helper.CreatePrerequisiteFeaturesFromList(false, deities)
                );

            var action = Helper.CreateConditional(
                new Condition[] {
                    new ContextConditionCasterHasFact { m_Fact = feat },
                    new ContextConditionIsAlly()
                },
                ifTrue: new GameAction[] {
                    Helper.CreateContextActionRemoveBuff(shaken),
                    new ContextActionSubstituteBuff(frightend, shaken, true)
                });

            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("f5fc9a1a2a3c1a946a31b320d1dd31b2").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //ChannelEnergy
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("6670f0f21a1d7f04db2b8b115e8e6abf").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //ChannelEnergyPaladinHeal
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("574cf074e8b65e84d9b69a8c6f1af27b").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //ChannelEnergyEmpyrealHeal
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("0c0cf7fcb356d2448b7d57f2c4db3c0c").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //ChannelEnergyHospitalerHeal
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("b5cf6b80e65ea724d99dc9f4f8874fc3").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //WarpriestChannelEnergy
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("0fb4bb4eae14fe84e8b45d8ea207c4e1").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //ShamanLifeSpiritChannelEnergy
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("6bcaf7636388f2a40bce263372735eef").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //WarpriestShieldbearerChannelEnergy
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("d470eb6b3b31fde4bb44ec753de0b862").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //WitchDoctorChannelEnergy
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("75edd403e824aa048ab5d4827b803b08").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //HexChannelerChannelEnergy
            Helper.AppendAndReplace(ref Helper.Get<BlueprintAbility>("b9eca127dd82f554fb2ccd804de86cf6").GetComponent<AbilityEffectRunAction>().Actions.Actions, action); //OracleRevelationChannelAbility

            Helper.AddFeats(feat);
        }
    }
}
