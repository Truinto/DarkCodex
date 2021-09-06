using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public class Hexcrafter
    {
        public static void fixProgression()
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
