using CodexLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.ActivatableAbilities;
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
        [PatchInfo(Severity.Fix, "Fix Bloodline: Arcane", "Arcane Apotheosis ignores metamagic casting time penalty", false)]
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
#endif

            const string descAdept = "At 3rd level, you can apply any one metamagic feat you know to a spell you are about to cast without increasing the casting time. You must still expend a higher-level spell slot to cast this spell. You can use this ability once per day at 3rd level and one additional time per day for every four sorcerer levels you possess beyond 3rd, up to five times per day at 19th level. At 20th level, this ability is replaced by arcane apotheosis.";
            var resource = Helper.CreateBlueprintAbilityResource("MetamagicAdeptResource", baseValue: 1, startLevel: 3, levelDiv: 4,
                classes: new BlueprintCharacterClassReference[] { sorcerer, scion });
            var adept = Helper.CreateBlueprintActivatableAbility(
                "MetamagicAdeptActivatable",
                "Metamagic Adept",
                descAdept,
                out var buff,
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
    }
}
