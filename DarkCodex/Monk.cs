using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Items;
using System.Text;
using System.Threading.Tasks;
using Shared;
using CodexLib;

namespace DarkCodex
{
    public class Monk
    {
        [PatchInfo(Severity.Create, "Feral Combat Training", "basic feat: Feral Combat Training", true, Requirement: typeof(Patch_FeralCombat))]
        public static void CreateFeralCombatTraining()
        {
            Main.Patch(typeof(Patch_FeralCombat));

            /*
             Feral Combat Training (Combat)
            You were taught a style of martial arts that relies on the natural weapons from your racial ability or class feature.
            Prerequisite: Improved Unarmed Strike, Weapon Focus with selected natural weapon.
            Benefit: Choose one of your natural weapons. While using the selected natural weapon, you can apply the effects of feats that have Improved Unarmed Strike as a prerequisite.
            Special: If you are a monk, you can use the selected natural weapon with your flurry of blows class feature.
             */
            var unarmedstrike = Helper.ToRef<BlueprintFeatureReference>("7812ad3672a4b9a4fb894ea402095167"); //ImprovedUnarmedStrike
            var weaponfocus = Helper.ToRef<BlueprintFeatureReference>("1e1f627d26ad36f43bbd26cc2bf8ac7e"); //WeaponFocus
            var zenarcher = Helper.ToRef<BlueprintArchetypeReference>("2b1a58a7917084f49b097e86271df21c"); //ZenArcherArchetype

            string name = "Feral Combat Training";
            string description = "While using any natural weapon, you can apply the effects of feats that have Improved Unarmed Strike as a prerequisite. Special: If you are a monk, you can use natural weapons with your flurry of blows class feature.";

            var feature = Helper.CreateBlueprintFeature(
                "FeralCombatTrainingFeature",
                name,
                description,
                group: FeatureGroup.Feat
                ).SetComponents(
                Helper.CreatePrerequisiteFeature(unarmedstrike),
                Helper.CreatePrerequisiteFeature(weaponfocus),
                Helper.CreatePrerequisiteNoArchetype(zenarcher)
                );

            Resource.Cache.FeatureFeralCombat.SetReference(feature);

            Helper.AddFeats(Resource.Cache.FeatureFeralCombat);
        }
    }
}
