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
        public const ModifierDescriptor Intelligence = (ModifierDescriptor)5380;
        public const ModifierDescriptor Charisma = (ModifierDescriptor)5381;

        public const WeaponCategory ButcheringAxe = (WeaponCategory)5480;

        // use in BlueprintActivatableAblity.WeightInGroup to restrict use of Activatable
        public const int NoManualOn = 788704819;
        public const int NoManualOff = 788704820;
        public const int NoManualAny = 788704821;

        public static CustomDataKey KeyChangeElement = new("ChangeElement");
    }
}
