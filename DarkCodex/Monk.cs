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
                Helper.CreatePrerequisiteNoArchetype(zenarcher, Helper.ToRef<BlueprintCharacterClassReference>("e8f21e5b58e0569468e420ebea456124"))
                );

            Resource.Cache.FeatureFeralCombat.SetReference(feature);

            Helper.AddFeats(Resource.Cache.FeatureFeralCombat);
        }

        public static void CreateMasterOfManyStyles()
        {
            /*
            Master of Many Styles

            The master of many styles is a collector. For every move, he seeks a counter. For every style, he has a riposte. Ultimately, he seeks perfection through the fusion of styles.
            Bonus Feat

            At 1st level, 2nd level, and every four levels thereafter, a master of many styles may select a bonus style feat or the Elemental Fist feat. He does not need to meet the prerequisites of that feat, except the Elemental Fist feat. Starting at 6th level, a master of many styles can choose to instead gain a wildcard style slot. Whenever he enters one or more styles, he can spend his wildcard style slots to gain feats in those styles’ feat paths (such as Earth Child Topple) as long as he meets the prerequisites. Each time he changes styles, he can also change these wildcard style slots.
            This ability replaces a monk’s standard bonus feats.

            Fuse Style (Ex)
            At 1st level, a master of many styles can fuse two of the styles he knows into a more perfect style. The master of many styles can have two style feat stances active at once. Starting a stance provided by a style feat is still a swift action, but when the master of many styles switches to another style feat, he can choose one style whose stance is already active to persist. He may only have two style feat stances active at a time.
            At 8th level, the master of many styles can fuse three styles at once. He can have the stances of three style feats active at the same time. He gains a bonus on attack rolls equal to the number of styles whose stances he currently has active. Furthermore, he can enter up to three stances as a swift action.
            At 15th level, the master of many styles can fuse four styles at once. He can have the stances of four style feats active at the same time. Furthermore, he can enter up to four stances as a free action by spending 1 point from his ki pool.
            This ability replaces flurry of blows.

            Perfect Style (Ex)
            At 20th level, a master of many styles can have the stances of five style feats active at once, and can change those stances as a free action.
            This ability replaces perfect self.
            */

            new AddFactsSafe();
        }

        public static void CreateWeaponSelection()
        {
            Main.Patch(typeof(Patch_UnitCreateFullAttack));

            var act = Helper.CreateBlueprintActivatableAbility(
                "BasicSelectWeapon",
                out var _
                );

            // idea: have an ability to select which weapon to use in a full attack
            // this is most relevant when dealing with unarmed strikes and natural attacks

            // fix Flurry of Blows natural attack stacking

            // modes:
            // - off (use vanilla logic)
            // - Flurry, mainhand only
            // - Flurry, offhand only
            // - Flurry, unarmed only
            // - Flurry, natural attack only (req. Feral Combat Training)
            // - Unarmed, then natural attacks

            // CloakOfTheWinterWolfFeature
            // ManeuverOnAttack / AddInitiatorAttackWithWeaponTrigger
        }

        public static void CreateStyleMaster()
        {
            /*
            Combat Style Master
            You shift between combat styles, combining them to increased effect.
            Prerequisites: Improved Unarmed Strike, two or more style feats, base attack bonus +6 or monk level 5th.
            Benefit: You can have two style feat stances active at once.
            */

            var preqStyles = Helper.CreatePrerequisiteFeaturesFromList();

            var feat = Helper.CreateBlueprintFeature(
                "CombatStyleMaster",
                "Combat Style Master",
                "You shift between combat styles, combining them to increased effect.\nBenefit: You can have two style feat stances active at once."
                ).SetComponents(
                Helper.CreatePrerequisiteFeature("7812ad3672a4b9a4fb894ea402095167"), //ImprovedUnarmedStrike
                preqStyles,
                Helper.CreatePrerequisiteFullStatValue(StatType.BaseAttackBonus, 5),
                new IncreaseActivatableAbilityGroupSize { Group = ActivatableAbilityGroup.CombatStyle }
                );

            Helper.AddCombatFeat(feat);

            Main.RunLast("Combat Style Master", () => 
            {
                preqStyles.Amount = 2;
                preqStyles.m_Features = Resource.Cache.Feature.Where(w => w.Groups.Any(FeatureGroup.StyleFeat)).ToRef<BlueprintFeatureReference>();
            });
        }

        public static void CreateSnakeStyle()
        {
            /*
            Snake Style (Combat, Style)
            You watch your foe’s every movement and then punch through its defense.
            Prerequisite: Improved Unarmed Strike, Acrobatics 1 rank, Sense Motive 3 ranks.
            Benefit: You gain a +2 bonus on Sense Motive checks, and you can deal piercing damage with your unarmed strikes. While using the Snake Style feat, when an opponent targets you with a melee or ranged attack, you can spend an immediate action to make a Sense Motive check. You can use the result as your AC or touch AC against that attack. You must be aware of the attack and not flat-footed.
            Normal: An unarmed strike deals bludgeoning damage.

            Snake Sidewind (Combat)
            Your sensitive twisting movements make you difficult to anticipate combat.
            Prerequisite: Improved Unarmed Strike, Snake Style, Acrobatics 3 ranks, Sense Motive 6 ranks.
            Benefit: You gain a +4 bonus to CMD against trip combat maneuvers and on Acrobatics checks and saving throws to avoid being knocked prone. While using the Snake Style feat, whenever you score a critical threat with your unarmed strike, you can make a Sense Motive check in place of the attack roll to confirm the critical hit. Whenever you score a critical hit with your unarmed strike, you can spend an immediate action to take a 5-foot step even if you have otherwise moved this round.
            Normal: You can take a 5-foot step only if you have not otherwise moved this round.

            Snake Fang (Combat)
            You can unleash attacks against an opponent that has dropped its guard.
            Prerequisite: Combat Reflexes, Improved Unarmed Strike, Snake Sidewind, Snake Style, Acrobatics 6 ranks, Sense Motive 9 ranks.
            Benefit: While using the Snake Style feat, when an opponent’s attack misses you, you can make an unarmed strike against that opponent as an attack of opportunity. If this attack of opportunity hits, you can spend an immediate action to make another unarmed strike against the same opponent.
            */
        }

        public static void CreateBoarStyle()
        {
            /*
            Boar Style (Combat, Style)
            Your sharp teeth and nails rip your foes open.
            Prerequisites: Improved Unarmed Strike, Intimidate 3 ranks.
            Benefit: You can deal bludgeoning damage or slashing damage with your unarmed strikes—changing damage type is a free action. While using this style, once per round when you hit a single foe with two or more unarmed strikes, you can tear flesh. When you do, you deal 2d6 extra points of damage with the attack.

            Boar Ferocity (Combat)
            Your flesh-ripping unarmed strikes terrify your victims.
            Prerequisites: Improved Unarmed Strike, Boar Style, Intimidate 6 ranks.
            Benefit: You add piercing damage to the damage types you can deal with your unarmed strikes. Further, you gain a +2 bonus on Intimidate checks to demoralize opponents. While using Boar Style, whenever you tear an opponent’s flesh, you can spend a free action to make an Intimidate check to demoralize that opponent.

            Boar Shred (Combat)
            The wounds you inflict with your unarmed strikes bleed, giving you renewed vigor.
            Prerequisites: Improved Unarmed Strike, Boar Ferocity, Boar Style, Intimidate 9 ranks.
            Benefit: You can make an Intimidate check to demoralize an opponent as a move action. While using Boar Style, whenever you tear an opponent’s flesh, once per round at the start of that opponent’s turn he takes 1d6 bleed damage. The bleed damage dealt while using Boar Style persist even if you later switch to a different style.
            */
        }

        public static void CreateWolfStyle()
        {
            /*
            Wolf Style (Combat, Style)
            While in this style, you hamper foes that turn their backs on you.
            Prerequisites: Wis 13, Improved Unarmed Strike, Knowledge (nature) 3 ranks.
            Benefit: While using this style, whenever you deal at least 10 points of damage to a foe with an attack of opportunity, that foe’s base speed decreases by 5 feet until the end of its next turn. For every 10 points of damage your attack deals beyond 10, the foe’s base speed decreases by an additional 5 feet. If the penalty meets or exceeds the total base speed of the foe, you can attempt to trip the foe as a free action after the attack of opportunity is resolved.

            Wolf Trip (Combat, Style)
            You have studied the manner in which wolves bring down their prey.
            Prerequisites: Wis 15, Improved Unarmed Strike, Wolf Style, Knowledge (nature) 6 ranks.
            Benefit: While using Wolf Style, you gain a +2 bonus when you attempt a trip combat maneuver as part of an attack of opportunity. Whenever you successfully trip a creature, as a free action you can choose an available space that is both adjacent to you and the creature’s original space for the tripped creature to land prone in.

            Wolf Savage (Combat, Style)
            You savage your foes so badly that they can become supernaturally disfigured.
            Prerequisites: Wis 17, Improved Unarmed Strike, Wolf Style, Wolf Trip, Knowledge (nature) 9 ranks.
            Benefit: While using Wolf Style, when you deal at least 10 points of damage to a prone opponent with a natural weapon or an unarmed strike, as a swift action you can savage that creature. When you do, your opponent must succeed at a Fortitude save (DC = 10 + half your character level + your Wisdom modifier). If the target fails the saving throw, it takes either 1d4 Charisma damage or 1d4 Constitution damage, or it becomes fatigued (your choice). Ability score damage dealt with this ability cannot equal or exceed the victim’s actual ability score total.
            */
        }
    }
}
