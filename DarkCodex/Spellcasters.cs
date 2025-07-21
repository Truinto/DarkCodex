﻿using CodexLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Recommendations;
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
            var adeptFeat = Helper.Get<BlueprintFeature>("82a966061553ea442b0ce0cdb4e1d49c"); //BloodlineArcaneCombatCastingAdeptFeatureLevel1
            var apotheosis = Helper.Get<BlueprintFeature>("2086d8c0d40e35b40b86d47e47fb17e4"); //BloodlineArcaneArcaneApotheosisFeature

            apotheosis.AddComponents(
                //Helper.CreateRemoveFeatureOnApply(adeptFeat),
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
                classes: [sorcerer, scion]);
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

            // remove the staged level depending bonus and replace it with the Metamagic Adept ability
            Helper.Get<BlueprintFeature>("7aa83ee3526a946419561d8d1aa09e75") //BloodlineArcaneCombatCastingAdeptFeatureAddLevel1
                .SetComponents(Helper.CreateAddFacts(adeptFeat));
            bloodline.RemoveFeature("3d7b19c8a1d03464aafeb306342be000"); //BloodlineArcaneCombatCastingAdeptFeatureAddLevel2
            sage.RemoveFeature("3d7b19c8a1d03464aafeb306342be000"); //BloodlineArcaneCombatCastingAdeptFeatureAddLevel2

            Helper.Get<BlueprintFeature>("e407e1a130324fa8866dcbfd21ba1fa7") //SeekerBloodlineArcaneCombatCastingAdeptFeatureAddLevel9
                .SetComponents(Helper.CreateAddFacts(adeptFeat));
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
                [
                    new ContextConditionCasterHasFact() { m_Fact = feat },
                    new ContextConditionIsEnemy(),
                    Helper.CreateContextConditionSharedValueHigher(AbilitySharedValue.DurationSecond, 0, Equality.LowerOrEqual)
                ],
                ifTrue: [
                    Helper.CreateContextActionChangeSharedValue(AbilitySharedValue.DurationSecond, add: 1),
                    new ContextActionSavingThrow() {
                        Type = SavingThrowType.Will,
                        Actions = Helper.CreateActionList(
                            Helper.CreateContextActionDealDamage(DamageEnergyType.Fire, Helper.CreateContextDiceValue(DiceType.Zero, 0, Helper.CreateContextValue(AbilitySharedValue.Heal)), false, true),
                            Helper.CreateContextActionConditionalSaved(failed: Helper.CreateContextActionApplyBuff("df6d1025da07524429afbae248845ecc", Helper.CreateContextDurationValue(bonus: 1))), //DazzledBuff
                            new ContextActionSpawnFx() { PrefabLink = Helper.GetPrefabLink("61602c5b0ac793d489c008e9cb58f631") })
                    },
                ]);

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
                Helper.Get<BlueprintFeature>("fd30c69417b434d47b6b03b9c1f568ff").GetComponents<PrerequisiteFeature>(f => f.Group == Prerequisite.GroupType.Any), //SelectiveChannel
                Helper.CreatePrerequisiteFeaturesFromList(false, deities)
                );

            var action = Helper.CreateConditional(
                [
                    new ContextConditionCasterHasFact() { m_Fact = feat },
                    new ContextConditionIsAlly()
                ],
                ifTrue: [
                    Helper.CreateContextActionRemoveBuff(shaken),
                    new ContextActionSubstituteBuff(frightend, shaken, true)
                ]);

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

        [PatchInfo(Severity.Create, "Energy Channel", "basic feat: channel energy through weapon attacks", false)]
        public static void CreateEnergyChannel()
        {
            AnyRef buff = Helper.CreateBlueprintBuff(
                "EnergyChannelBuff"
                ).SetComponents(
                new EnergyChannel()
                );

            var acid = Helper.CreateBlueprintAbility(
                "EnergyChannelAbilityAcid",
                "Energy Channel – Acid",
                "As a swift action, you can expend one use of channel energy to grant your weapon attacks a bonus on damage rolls equal to twice the number of dice rolled for your channel energy. This additional damage is of an energy type determined by your domain or blessing: acid (Earth), cold (Water), electricity (Air), or fire (Fire).",
                icon: Helper.StealIcon("0c852a2405dd9f14a8bbcfaf245ff823"),
                type: AbilityType.Supernatural,
                actionType: UnitCommand.CommandType.Swift,
                range: AbilityRange.Personal
                ).TargetSelf(
                ).SetComponents(
                new ActivatableVariants(
                    "279447a6bf2d3544d93a0a39c3b8e91d", //ChannelPositiveHarm
                    "89df18039ef22174b81052e2e419c728", //ChannelNegativeEnergy
                    "d899a34f7d39170449120d155199ab2d", //PurifierSacredScourgeAbility
                    "ab0635df6b4674e4e96809bd718cab89", //OracleRevelationChannelHarmAbility
                    "cc17243b2185f814aa909ac6b6599eaa", //ChannelEnergyHospitalerHarm
                    "4937473d1cfd7774a979b625fb833b47", //ChannelEnergyPaladinHarm
                    "17e5f3fa4754bba4391c02fba1142465", //SpiritWardenChannelPositiveHarm
                    "193cef80f7287e648874d3df1eb81159", //WitchDoctorChannelPositiveHarm
                    "0fb4bb4eae14fe84e8b45d8ea207c4e1", //ShamanLifeSpiritChannelEnergy
                    "e1536ee240c5d4141bf9f9485a665128", //ChannelEnergyEmpyrealHarm
                    "023bd78a3b068e84aad2b1fd273daae1", //WarpriestChannelNegativeEnergy
                    "fd5d66b9b64cca6499d4d17af0f5577c", //WarpriestChannelPositiveHarm
                    "894e20539c353c74ab2733a056351947", //WarpriestShieldbearerChannelPositiveHarm
                    "d5f525a9c5efa634d843d5c35f074f02", //WarpriestShieldbearerChannelNegativeEnergy
                    "fb917ad147d846e42ad22c8e14f44b79", //HexChannelerChannelPositiveHarm
                    "fb2df4978dd4fd745a7aaecfd1068512", //HexChannelerChannelNegativeEnergy
                    "a087951516895d64dbae77434f4967bd"),//LichChannelNegativeHarm
                new EnergyChannelApplyEffect(buff, DamageEnergyType.Acid),
                Helper.CreateAbilityShowIfCasterHasFact("08bbcbfe5eb84604481f384c517ac800") //EarthDomain
                );
            var cold = acid.Clone("EnergyChannelAbilityCold").SetUIData(displayname: "Energy Channel – Cold", icon: Helper.StealIcon("403bcf42f08ca70498432cf62abee434"));
            cold.GetComponent<EnergyChannelApplyEffect>().Element = DamageEnergyType.Cold;
            cold.GetComponent<AbilityShowIfCasterHasFact>().m_UnitFact = Helper.ToRef<BlueprintUnitFactReference>("e63d9133cebf2cf4788e61432a939084"); //WaterDomain
            var electricity = acid.Clone("EnergyChannelAbilityElectricity").SetUIData(displayname: "Energy Channel – Electricity", icon: Helper.StealIcon("d2cff9243a7ee804cb6d5be47af30c73"));
            electricity.GetComponent<EnergyChannelApplyEffect>().Element = DamageEnergyType.Electricity;
            electricity.GetComponent<AbilityShowIfCasterHasFact>().m_UnitFact = Helper.ToRef<BlueprintUnitFactReference>("750bfcd133cd52f42acbd4f7bc9cc365"); //AirDomain
            var fire = acid.Clone("EnergyChannelAbilityFire").SetUIData(displayname: "Energy Channel – Fire", icon: Helper.StealIcon("cdb106d53c65bbc4086183d54c3b97c7"));
            fire.GetComponent<EnergyChannelApplyEffect>().Element = DamageEnergyType.Fire;
            fire.GetComponent<AbilityShowIfCasterHasFact>().m_UnitFact = Helper.ToRef<BlueprintUnitFactReference>("881b2137a1779294c8956fe5b497cc35"); //FireDomain

            var feat = Helper.CreateBlueprintFeature(
                "EnergyChannelFeature",
                "Energy Channel",
                "You distill your channeled energy into your weapon to empower it.\nBenefit: As a swift action, you can expend one use of channel energy to grant your weapon attacks a bonus on damage rolls equal to twice the number of dice rolled for your channel energy. This additional damage is of an energy type determined by your domain or blessing: acid (Earth), cold (Water), electricity (Air), or fire (Fire). This effect lasts for 3 minutes, but each time you use this effect, reduce the remaining duration by 1 minute."
                ).SetComponents(
                Helper.CreateAddFacts(acid, cold, electricity, fire),
                Helper.Get<BlueprintFeature>("fd30c69417b434d47b6b03b9c1f568ff").GetComponents<PrerequisiteFeature>(f => f.Group == Prerequisite.GroupType.Any), //SelectiveChannel
                Helper.CreatePrerequisiteFeaturesFromList(new AnyRef[] {
                    "08bbcbfe5eb84604481f384c517ac800", //EarthDomain
                    "e63d9133cebf2cf4788e61432a939084", //WaterDomain
                    "750bfcd133cd52f42acbd4f7bc9cc365", //AirDomain
                    "881b2137a1779294c8956fe5b497cc35", //FireDomain
                    })
                );

            Helper.AddFeats(feat);
        }

        [PatchInfo(Severity.Create, "Channel Form", "basic feat: collection of abilities to shape channel energy into new forms", true)]
        public static void CreateChannelForm()
        {
            /*
            Malleable Symbol
            Aura moderate conjuration; CL 9th; Weight 1 lb.; Slot none; Price 10,000 gp
            This undistinguished metal lump has no powers in its default state. By concentrating on it for 1 minute, a creature who worships a deity may transform it into a masterwork holy symbol of that deity. Whenever this holy symbol is used as a focus for channeled energy, as a free action the bearer can alter the channel area to one of the following options:
            - 10-foot burst centered anywhere within 30 feet
            - 60-foot line
            - 30-foot cone
            */

            AnyRef[] channels = BpCache.Get<BlueprintAbility>().Where(w => w.Type != AbilityType.Spell && w.Type != AbilityType.SpellLike && w.SpellDescriptor.HasAnyFlag(
                SpellDescriptor.ChannelNegativeHarm
                | SpellDescriptor.ChannelNegativeHeal
                | SpellDescriptor.ChannelPositiveHarm
                | SpellDescriptor.ChannelPositiveHeal)).ToAny();

            var ray = Helper.CreateBlueprintAbility(
                "ChannelFormRay",
                "Channel Ray",
                "Alter your channel energy area into a ray attack.",
                icon: Helper.StealIcon("8eead52509987034ea9025d60cc05985"), //RayOfExhaustion
                range: AbilityRange.Long
                ).TargetEnemy(
                ).SetComponents(
                new ActivatableVariants(channels),
                new VariantSelectionApplyEffect(),
                Helper.CreateAbilityDeliverProjectile(
                    projectile: Resource.Projectile.RayOfExhaustion00,
                    type: AbilityProjectileType.Simple,
                    length: 0.Feet(),
                    width: 5.Feet(),
                    weapon: "f6ef95b1f7bb52b408a5b345a330ffe8") //RayItem
                );

            var burst = Helper.CreateBlueprintAbility(
                "ChannelFormBurst",
                "Channel Burst",
                "Alter your channel energy area into a 10 feet radius burst.",
                icon: channels.First().Get<BlueprintAbility>().Icon,
                range: AbilityRange.Close
                ).TargetPoint(
                ).SetComponents(
                new ActivatableVariants(channels),
                new VariantSelectionApplyEffect(),
                Helper.CreateAbilityDeliverProjectile(
                    projectile: Resource.Projectile.AlchemistHolyBomb00,
                    type: AbilityProjectileType.Simple,
                    length: 0.Feet(),
                    width: 5.Feet()),
                Helper.CreateAbilityTargetsAround(10.Feet())
                );

            var line = Helper.CreateBlueprintAbility(
                "ChannelFormLine",
                "Channel Line",
                "Alter your channel energy area into a 60 feet line.",
                icon: Helper.StealIcon("2aad85320d0751340a0786de073ee3d5"),
                range: AbilityRange.Projectile
                ).TargetPoint(
                ).SetComponents(
                new ActivatableVariants(channels),
                new VariantSelectionApplyEffect(),
                Helper.CreateAbilityDeliverProjectile(
                    projectile: Resource.Projectile.Kinetic_BlueFlameLine00,
                    type: AbilityProjectileType.Line,
                    length: 60.Feet(),
                    width: 5.Feet())
                );

            var cone = Helper.CreateBlueprintAbility(
                "ChannelFormCone",
                "Channel Cone",
                "Alter your channel energy area into a 30 feet cone.",
                icon: Helper.StealIcon("fde466d2c24705641bcd97d04a323566"),
                range: AbilityRange.Projectile
                ).TargetPoint(
                ).SetComponents(
                new ActivatableVariants(channels),
                new VariantSelectionApplyEffect(),
                Helper.CreateAbilityDeliverProjectile(
                    projectile: Resource.Projectile.ChannelNegativeEnergyCone30Feet00,
                    type: AbilityProjectileType.Cone,
                    length: 30.Feet(),
                    width: 5.Feet())
                );

            var feat = Helper.CreateBlueprintFeature(
                "ChannelFormFeat",
                "Channel Form",
                "As a free action you may alter your channel energy area to one of the following options:\n- single target ray\n- 10-foot burst centered anywhere within 30 feet\n- 60-foot line\n- 30-foot cone"
                ).SetComponents(
                Helper.CreatePureRecommendation(),
                Helper.CreateAddFacts(ray, burst, line, cone),
                Helper.Get<BlueprintFeature>("fd30c69417b434d47b6b03b9c1f568ff").GetComponents<PrerequisiteFeature>(f => f.Group == Prerequisite.GroupType.Any) //SelectiveChannel
                );

            Helper.AddFeats(feat);
        }

        [PatchInfo(Severity.Create | Severity.Hidden | Severity.WIP, "Dazing Spell (Metamagic)", "spell damage also daze the target", false)]
        public static void CreateDazingMetamagic()
        {
            /*
            Dazing Spell (Metamagic)
            You can daze creatures with the power of your spells.
            Benefit: You can modify a spell to daze a creature damaged by the spell. When a creature takes damage from this spell, they become dazed for a number of rounds equal to the original level of the spell. If the spell allows a saving throw, a successful save negates the daze effect. If the spell does not allow a save, the target can make a Will save to negate the daze effect. If the spell effect also causes the creature to become dazed, the duration of this metamagic effect is added to the duration of the spell.
            Level Increase: +3 (a dazing spell uses up a spell slot three levels higher than the spell’s actual level.
            Spells that do not inflict damage do not benefit from this feat. 
            */

            var feat = Helper.CreateBlueprintFeature(
                "DazingSpellFeat",
                "Dazing Spell (Metamagic)",
                "You can daze creatures with the power of your spells.\nBenefit: You can modify a spell to daze a creature damaged by the spell. When a creature takes damage from this spell, they become dazed for a number of rounds equal to the original level of the spell. If the spell allows a saving throw, a successful save negates the daze effect. If the spell does not allow a save, the target can make a Will save to negate the daze effect.\nLevel Increase: +3 (a dazing spell uses up a spell slot three levels higher than the spell’s actual level.)\nSpells that do not inflict damage do not benefit from this feat.",
                group: FeatureGroup.WizardFeat
                ).SetComponents(
                new AddMetamagicFeat() { Metamagic = Const.Dazing },
                Helper.CreatePrerequisiteStatValue(StatType.Intelligence, 3),
                Helper.CreateFeatureTagsComponent(FeatureTag.Magic | FeatureTag.Metamagic)
                );

            // must add name
            var names = Game.Instance.BlueprintRoot.LocalizedTexts.MetamagicNames;
            if (names.Entries.Any(a => a.Value == Const.Dazing))
                Helper.AppendAndReplace(ref names.Entries, new Kingmaker.Blueprints.Root.Strings.MetamagicString.MyEntry() { Value = Const.Dazing, Text = Helper.CreateString("Dazing") });

            var favorite = Helper.CreateBlueprintFeature(
                "FavoriteMetamagicDazing",
                "Favorite Metamagic — Dazing"
                ).SetUIData(
                description: Helper.GetString("db1fbb2d-e684-43a1-a9d3-8a79a5b23761")
                ).SetComponents(
                //Helper.CreateAddMechanicsFeature(AddMechanicsFeature.MechanicsFeatureType.FavoriteMetamagicBolstered),
                new MetamagicReduceCostParametrized(metamagic: Const.Dazing),
                Helper.CreatePrerequisiteFeature(feat)
                );

            Helper.AddWizardFeat(feat);
            Helper.Get<BlueprintFeatureSelection>("503fb196aa222b24cb6cfdc9a284e838") //FavoriteMetamagicSelection
                .Add(favorite);

            EventBus.Subscribe(new Event_DazingSpell());

            Main.RunLast("DazingSpellFeat", () =>
            {
                foreach (var spell in BpCache.Get<BlueprintAbility>())
                    if (spell.Type == AbilityType.Spell)
                        spell.AvailableMetamagic |= Const.Dazing;
            });
        }
    }
}
