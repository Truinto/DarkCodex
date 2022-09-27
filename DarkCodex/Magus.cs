using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Shared;
using CodexLib;
using Kingmaker.Enums;

namespace DarkCodex
{
    public class Magus
    {
        [PatchInfo(Severity.Create | Severity.Faulty, "Accursed Strike", "hexcrafter arcana: Accursed Strike", false)]
        public static void CreateAccursedStrike()
        {
            var hexes = Helper.Get<BlueprintFeatureSelection>("9846043cf51251a4897728ed6e24e76f"); //WitchHexSelection
            var hexcrafter = Helper.ToRef<BlueprintArchetypeReference>("79ccf7a306a5d5547bebd97299f6fc89");

            // touch spells
            Helper.MakeStickySpell(Helper.Get<BlueprintAbility>("3105d6e9febdc3f41a08d2b7dda1fe74"), out var baleful, out _);

            // touch hexes
            Resource.Cache.Ensure();
            var hexes_harmful = Resource.Cache.Ability.Where(w => w.EffectOnEnemy == AbilityEffectOnUnit.Harmful && (w.name.StartsWith("WitchHex") || w.name.StartsWith("ShamanHex"))).ToArray();
            var accursed_strike_variants = new List<BlueprintAbility>();
            foreach (var hex in hexes_harmful)
            {
                Helper.MakeStickySpell(hex, out var hex_accursed, out var hex_effect);

                hex_effect.ActionType = CommandType.Standard;
                hex_accursed.ActionType = CommandType.Standard;
                hex_accursed.AddComponents(Helper.CreateAbilityShowIfCasterHasFact(hex.Parent ? hex.Parent.ToRef2() : hex.ToRef2()));

                accursed_strike_variants.Add(hex_accursed);
            }

            var accursed_strike_ab = Helper.CreateBlueprintAbility(
                "AccursedStrikeAbility",
                "Accursed Strike",
                "Any prepared spell or hex with the curse descriptor can be delivered using the spellstrike ability, even if the spells are not touch attack spells.",
                accursed_strike_variants[0].m_Icon,
                AbilityType.Supernatural,
                CommandType.Standard,
                AbilityRange.Touch
                ).TargetEnemy(
                ).AddToAbilityVariants(accursed_strike_variants.ToArray());

            // feature
            var feat = Helper.CreateBlueprintFeature(
                "AccursedStrikeFeature",
                "Accursed Strike",
                "Any prepared spell or hex with the curse descriptor can be delivered using the spellstrike ability, even if the spells are not touch attack spells.",
                accursed_strike_variants[0].m_Icon,
                FeatureGroup.WitchHex
                ).SetComponents(
                Helper.CreatePrerequisiteArchetypeLevel(hexcrafter, 1, characterClass: Helper.ToRef<BlueprintCharacterClassReference>("45a4607686d96a1498891b3286121780")),
                Helper.CreateAddFacts(accursed_strike_ab.ToRef2()),
                Helper.CreateAddKnownSpell(baleful.ToRef(), 5, archetype: hexcrafter)
                );

            Resource.Cache.AccursedStrike.AddRange(accursed_strike_variants);
#if DEBUG
            Helper.AddHex(feat, false); // TODO: bugfix accursed strike
#endif
        }

        [PatchInfo(Severity.Fix, "Fix Hexcrafter", "allows hex selection with any arcana, add missing spell recall at level 11", false)]
        public static void FixHexcrafterProgression()
        {
            var HexcrafterArchetype = Helper.Get<BlueprintArchetype>("79ccf7a306a5d5547bebd97299f6fc89");
            var MagusArcanaSelection = Helper.Get<BlueprintFeatureSelection>("e9dc4dfc73eaaf94aae27e0ed6cc9ada");
            var HexcrafterMagusHexArcanaSelection = Helper.Get<BlueprintFeatureSelection>("ad6b9cecb5286d841a66e23cea3ef7bf");
            var MagusSpellRecallFeature = Helper.Get<BlueprintFeature>("61fc0521e9992624e9c518060bf89c0f");

            HexcrafterArchetype.RemoveFeature(3, MagusArcanaSelection);
            HexcrafterArchetype.AddFeature(3, HexcrafterMagusHexArcanaSelection);
            HexcrafterArchetype.AddFeature(11, MagusSpellRecallFeature);
        }

        [PatchInfo(Severity.Fix | Severity.DefaultOff, "Sword Saint Any Weapon", "allow Sword Saint to pick any weapon focus", true)]
        public static void PatchSwordSaint()
        {
            var chosenweapon = Helper.Get<BlueprintParametrizedFeature>("c0b4ec0175e3ff940a45fc21f318a39a"); //SwordSaintChosenWeaponFeature
            chosenweapon.WeaponSubCategory = WeaponSubCategory.None;
        }
    }
}
