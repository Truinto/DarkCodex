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

namespace DarkCodex
{
    public class Hexcrafter
    {
        [PatchInfo(Severity.Create | Severity.Faulty, "Accursed Strike", "hexcrafter arcana: Accursed Strike", false)]
        public static void CreateAccursedStrike() // todo finish accursed strike
        {
            var hexes = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("9846043cf51251a4897728ed6e24e76f"); //WitchHexSelection

            Resource.Cache.Ensure();
            var hexes_harmful = Resource.Cache.Ability.Where(w => w.EffectOnEnemy == AbilityEffectOnUnit.Harmful && (w.name.StartsWith("WitchHex") || w.name.StartsWith("ShamanHex")));

            var accursed_strike_variants = new List<BlueprintAbility>();
            foreach (var hex in hexes_harmful)
            {
                Helper.MakeStickySpell(hex.AssetGuidThreadSafe, out var hex_accursed, out _);

                accursed_strike_variants.Add(hex_accursed);
            }

            //BalefulPolymorph
            //cast needs: AbilityEffectStickyTouch ->
            //effect needs: AbilityDeliverTouch -> bb337517547de1a4189518d404ec49d4:TouchItem

            //var baleful = library.Get<BlueprintAbility>("3105d6e9febdc3f41a08d2b7dda1fe74");//BalefulPolymorph
            //var baleful_touch = library.CopyAndAdd(baleful, "BalefulPolymorphCast", Guid.i.Reg("32e0d31f45484a64b78bea82f80d42af"));
            //baleful.AddComponent(HelperEA.CreateDeliverTouch());
            //baleful_touch.Range = AbilityRange.Touch;
            //baleful_touch.Animation = CastAnimationStyle.Touch;
            //baleful_touch.ReplaceComponent<AbilityEffectRunAction>(HelperEA.CreateStickyTouch(baleful));
            //baleful_touch.RemoveComponents<AbilitySpawnFx>();

            //var accursed_glare_touch = library.CopyAndAdd(NewSpells.accursed_glare, "AccursedGlareCast", Guid.i.Reg("419f9b711f4f47baa3844f8cbe8eac86"));
            //NewSpells.accursed_glare.AddComponent(HelperEA.CreateDeliverTouch()); // TODO: check this doesn't morph the original
            //accursed_glare_touch.Range = AbilityRange.Touch;
            //accursed_glare_touch.Animation = CastAnimationStyle.Touch;
            //accursed_glare_touch.ReplaceComponent<AbilityEffectRunAction>(HelperEA.CreateStickyTouch(NewSpells.accursed_glare));
            //accursed_glare_touch.RemoveComponents<AbilitySpawnFx>();

            //var bonus_spelllist = new ExtraSpellList(
            //        new ExtraSpellList.SpellId(baleful_touch.AssetGuid, 5),
            //        new ExtraSpellList.SpellId(accursed_glare_touch.AssetGuid, 3)
            //    ).createLearnSpellList("MagusBonusSpellList", Guid.i.Reg("d1797a72a3c843459be0c44d22aa7296"), magus, archetype);


            //foreach (var hex in hexes_harmful)
            //{
            //    var hex_accursed = library.CopyAndAdd(hex, hex.name + "Accursed", hex.AssetGuid, "d0bf27aea4234946bd065710e759b1b5");
            //    hex_accursed.AddComponent(HelperEA.CreateDeliverTouch());
            //    hex_accursed.Range = AbilityRange.Touch;
            //    hexes_ability.Add(hex_accursed);
            //    Main.DebugLog("Generated " + hex_accursed.name + ":" + hex_accursed.AssetGuid);

            //    accursed_strike_variants[i] = hex_accursed.CreateTouchSpellCast();
            //    accursed_strike_variants[i].SetName("Accursed Strike: " + hex.Name);
            //    accursed_strike_variants[i].ActionType = CommandType.Standard;
            //    accursed_strike_variants[i++].AddComponent(Helper.CreateAbilityShowIfCasterHasAnyFacts(hex.Parent ? hex.Parent : hex));
            //}

            //var accursed_strike_ab = HelperEA.CreateAbility(
            //    "AccursedStrikeAbility",
            //    "Accursed Strike",
            //    "Any prepared spell or hex with the curse descriptor can be delivered using the spellstrike ability, even if the spells are not touch attack spells.",
            //    Guid.i.Reg("5dcaf35eddf74885bbbdc9c045fdd3ff"),
            //    accursed_strike_variants[0].Icon,
            //    AbilityType.Supernatural,
            //    CommandType.Standard,
            //    AbilityRange.Touch,
            //    "",
            //    ""
            //);
            //HelperEA.SetMiscAbilityParametersTouchHarmful(accursed_strike_ab);
            //accursed_strike_ab.SetComponents(HelperEA.CreateAbilityVariants(accursed_strike_ab, accursed_strike_variants));

            //accursed_strike = HelperEA.CreateFeature(
            //    "AccursedStrikeFeature",
            //    "Accursed Strike",
            //    "Any prepared spell or hex with the curse descriptor can be delivered using the spellstrike ability, even if the spells are not touch attack spells.",
            //    Guid.i.Reg("842537d10e1a47e7a87d050613b6e85b"),
            //    accursed_strike_variants[0].Icon,
            //    FeatureGroup.None,
            //    HelperEA.CreatePrerequisiteArchetypeLevel(magus, archetype, 1, true),
            //    HelperEA.CreateAddFact(accursed_strike_ab),
            //    bonus_spelllist
            //);

            ////hex_arcana_selection.AllFeatures = hex_arcana_selection.AllFeatures.AddToArray(accursed_strike);
            //hexes.Add(accursed_strike);
        }

        [PatchInfo(Severity.Fix, "Fix Hexcrafter", "allows hex selection with any arcana, add missing spell recall at level 11", false)]
        public static void FixProgression()
        {
            var HexcrafterArchetype = ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("79ccf7a306a5d5547bebd97299f6fc89");
            var MagusArcanaSelection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("e9dc4dfc73eaaf94aae27e0ed6cc9ada");
            var HexcrafterMagusHexArcanaSelection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("ad6b9cecb5286d841a66e23cea3ef7bf");
            var MagusSpellRecallFeature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("61fc0521e9992624e9c518060bf89c0f");

            HexcrafterArchetype.RemoveFeature(3, MagusArcanaSelection);
            HexcrafterArchetype.AddFeature(3, HexcrafterMagusHexArcanaSelection);
            HexcrafterArchetype.AddFeature(11, MagusSpellRecallFeature);
        }
    }
}
