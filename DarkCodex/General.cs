using DarkCodex.Components;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public class General
    {
        public static void createAbilityFocus()
        {
            Resource.Cache.Ensure();

            var abilities = Resource.Cache.Ability.Where(ability =>
            {
                if (ability.Type == AbilityType.Spell)
                    return false;

                if (ability.m_Parent != null && !ability.m_Parent.IsEmpty())
                    return false;

                var run = ability.GetComponent<AbilityEffectRunAction>();
                if (run == null)
                    return false;

                return run.SavingThrowType != SavingThrowType.Unknown;
            }).ToArray();

            var feat = Helper.CreateBlueprintParametrizedFeature(
                "AbilityFocus",
                "Ability Focus",
                "Choose one special attack. Add +2 to the DC for all saving throws against the special attack on which you focus.",
                blueprints: abilities.ToRef3()
                ).SetComponents(
                new AbilityFocusParametrized()
                );
            feat.Ranks = 10;

            Helper.AddFeats(feat);
        }

        public static void patchAngelsLight()
        {
            var angelbuff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("e173dc1eedf4e344da226ffbd4d76c60"); // AngelMinorAbilityEffectBuff

            var temphp = angelbuff.GetComponent<TemporaryHitPointsFromAbilityValue>();
            temphp.Value = Helper.CreateContextValue(AbilityRankType.Default);
            angelbuff.AddComponents(Helper.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, type: AbilityRankType.Default)); // see FalseLifeBuff
        }
    }
}
