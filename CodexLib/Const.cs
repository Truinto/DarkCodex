using Kingmaker.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.RuleSystem.RulebookEvent;

namespace CodexLib
{
    /// <summary>
    /// Collection of const values. Mostly custom Enums or key strings.
    /// </summary>
    public class Const
    {
        public const ModifierDescriptor TTT_NaturalArmorSize = (ModifierDescriptor)1717;
        public const ModifierDescriptor TTT_NaturalArmorStackable = (ModifierDescriptor)1718;
        public const ModifierDescriptor TTT_DodgeStrength = (ModifierDescriptor)2121;
        public const ModifierDescriptor TTT_DodgeDexterity = (ModifierDescriptor)2122;
        public const ModifierDescriptor TTT_DodgeConstitution = (ModifierDescriptor)2123;
        public const ModifierDescriptor TTT_DodgeIntelligence = (ModifierDescriptor)2124;
        public const ModifierDescriptor TTT_DodgeWisdom = (ModifierDescriptor)2125;
        public const ModifierDescriptor TTT_DodgeCharisma = (ModifierDescriptor)2126;
        public const ModifierDescriptor TTT_UntypedStrength = (ModifierDescriptor)3121;
        public const ModifierDescriptor TTT_UntypedDexterity = (ModifierDescriptor)3122;
        public const ModifierDescriptor TTT_UntypedConstitution = (ModifierDescriptor)3123;
        public const ModifierDescriptor TTT_UntypedIntelligence = (ModifierDescriptor)3124;
        public const ModifierDescriptor TTT_UntypedWisdom = (ModifierDescriptor)3125;
        public const ModifierDescriptor TTT_UntypedCharisma = (ModifierDescriptor)3126;
        public const ModifierDescriptor TTT_UntypedWeaponTraining = (ModifierDescriptor)3127;
        public const ModifierDescriptor TTT_UntypedWeaponFocus = (ModifierDescriptor)3128;
        public const ModifierDescriptor TTT_UntypedWeaponFocusGreater = (ModifierDescriptor)3129;
        public const ModifierDescriptor TTT_UntypedSpellFocus = (ModifierDescriptor)3130;
        public const ModifierDescriptor TTT_UntypedSpellFocusGreater = (ModifierDescriptor)3131;
        public const ModifierDescriptor TTT_UntypedSchoolMastery = (ModifierDescriptor)3132;
        public const ModifierDescriptor TTT_UntypedVarisianTattoo = (ModifierDescriptor)3133;
        public const ModifierDescriptor TTT_EnhancementWeapon = (ModifierDescriptor)4121;

        public const ModifierDescriptor Intelligence = (ModifierDescriptor)5380;
        public const ModifierDescriptor Charisma = (ModifierDescriptor)5381;

        public const WeaponCategory ButcheringAxe = (WeaponCategory)5480;

        // use in BlueprintActivatableAblity.WeightInGroup to restrict use of Activatable
        public const int NoManualOn = 788704819;
        public const int NoManualOff = 788704820;
        public const int NoManualAny = 788704821;

        public const PetType PetUndead = (PetType)5580;

        public const string ObsoleteNotice = "see overload; use named parameters";

        public static CustomDataKey KeyChangeElement = new("ChangeElement");
    }
}
