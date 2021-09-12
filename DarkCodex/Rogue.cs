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
    public class Rogue
    {
        public static void createExtraRogueTalent()
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
