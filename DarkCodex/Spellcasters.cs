using CodexLib;
using Kingmaker.Blueprints.Classes;
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
        [PatchInfo(Severity.Fix, "Fix Bloodline: Arcane", "Arcane Apotheosis ignores metamagic casting time penalty", false)]
        public static void FixBloodlineArcane()
        {
            var bloodline = Helper.Get<BlueprintFeature>("2086d8c0d40e35b40b86d47e47fb17e4"); //BloodlineArcaneArcaneApotheosisFeature
            bloodline.AddComponents(new AddMechanicFeatureCustom(MechanicFeature.SpontaneousMetamagicNoFullRound));
            bloodline.m_Description.CreateString(bloodline.m_Description + "\nYou can add any metamagic feats that you know to your spells without increasing their casting time, although you must still expend higher-level spell slots.");
#if false
            var feat = Helper.CreateBlueprintActivatableAbility(
                "BloodlineArcaneApotheosisActivatable",
                "Arcane Apotheosis",
                "Whenever you use magic items that require charges, you can instead expend spell slots to power the item. For every three levels of spell slots that you expend, you consume one less charge when using a magic item that expends charges.",
                out var buff
                );
#endif
        }
    }
}
