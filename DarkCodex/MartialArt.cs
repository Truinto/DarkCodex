using CodexLib;
using Kingmaker.Blueprints.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public class MartialArt
    {
        public static void CreatePaladinVirtuousBravo() // TODO: 
        {
            var archetype = Helper.CreateBlueprintArchetype(
                "VirtuousBravoArchetype",
                "Virtuous Bravo",
                "Although no less a beacon of hope and justice than other paladins, virtuous bravos rely on their wit and grace rather than might and strong armor.",
                removeSpellbook: true
                );
            archetype.AddFeatures = new LevelEntry[] { };
        }

        /*
        Bladed Brush (Combat)
        Note: This is associated with a specific deity.
        You know how to balance a polearm perfectly, striking with artful, yet deadly precision.
        Prerequisite(s): Weapon Focus (glaive), must be a worshiper of the associated deity.
        Benefit(s): You can use the Weapon Finesse feat to apply your Dexterity modifier instead of your Strength modifier to attack rolls with a glaive sized for you, even though it isn’t a light weapon. When wielding a glaive, you can treat it as a one-handed piercing or slashing melee weapon and as if you were not making attacks with your off-hand for all feats and class abilities that require such a weapon (such as a duelist’s or swashbuckler’s precise strike).
        As a move action, you can shorten your grip on the glaive, treating it as though it lacked the reach weapon property. You can adjust your grip to grant the weapon the reach property as a move action.
        
        
        
        Virtuous Bravo

        Although no less a beacon of hope and justice than other paladins, virtuous bravos rely on their wit and grace rather than might and strong armor.
        
        Weapon and Armor Proficiency
        Virtuous bravos aren’t proficient with heavy armor or shields (except for bucklers).
        This ability alters the paladin’s armor proficiency.

        Bravo’s Finesse (Ex)
        A virtuous bravo gains Weapon Finesse as a bonus feat. She can use her Charisma score in place of her Intelligence score to meet prerequisites of combat feats.

        Bravo’s Smite (Su)
        When using smite evil, a virtuous bravo doesn’t gain a deflection bonus to AC.
        This ability alters smite evil.

        Nimble (Ex)
        At 3rd level, a virtuous bravo gains a +1 dodge bonus to AC while wearing light armor or no armor.
        Anything that causes the virtuous bravo to lose her Dexterity bonus to AC also causes her to lose this dodge bonus. This bonus increases by 1 for every 4 paladin levels beyond 3rd (to a maximum of +5 at 19th level).
        This ability replaces mercy.

        Panache and Deeds (Ex)
        At 4th level, a virtuous bravo gains the swashbuckler’s panache class feature along with the following swashbuckler deeds: dodging panache, menacing swordplay, opportune parry and riposte, precise strike, and swashbuckler initiative. The virtuous bravo’s paladin levels stack with any swashbuckler levels when using these deeds.
        This ability replaces the paladin’s spellcasting.

        Advanced Deeds (Ex)
        At 11th level, a virtuous bravo gains the following swashbuckler deeds: bleeding wound, evasive, subtle blade, superior feint, swashbuckler’s grace, and targeted strike.
        This ability replaces aura of justice.

        Bravo’s Holy Strike (Su)
        At 20th level, a virtuous bravo becomes a master at dispensing holy justice with her blade.
        When the virtuous bravo confirms a critical hit with a light or one-handed piercing melee weapon, she can choose one of the following three effects in addition to dealing damage: the target is rendered unconscious for 1d4 hours, the target is paralyzed for 2d6 rounds, or the target is slain. Regardless of the effect chosen, the target can attempt a Fortitude save.
        On a success, the target is instead stunned for 1 round (it still takes damage). The DC of this save is equal to 10 + 1/2 the virtuous bravo’s paladin level + her Charisma modifier. Once a creature has been the target of a bravo’s holy strike, regardless of whether or not it succeeds at the save, that creature is immune to that bravo’s holy strike for 24 hours. Creatures that are immune to critical hits are also immune to this ability.
        This ability replaces holy champion.

        */
    }
}
