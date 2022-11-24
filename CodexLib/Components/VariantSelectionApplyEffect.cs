using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Mimic properties of selected VariantSelection:<br/>
    /// - ability restriction (only AbilityResourceLogic)<br/>
    /// - ContextRankConfig, ContextCalculateSharedValue, ContextAbilityParamsCalculator<br/>
    /// - AbilityApplyEffect
    /// </summary>
    public class VariantSelectionApplyEffect : AbilityApplyEffect, IAbilityRestriction, IMechanicRecalculate
    {
        public int Priority => 0;

        public string GetAbilityRestrictionUIText()
        {
            return LocalizedTexts.Instance.Reasons.NoResources;
        }

        public bool IsAbilityRestrictionPassed(AbilityData ability)
        {
            if (ability.Caster.GetFact(ability.Blueprint)?.GetDataExt<IActionBarConvert, VariantSelectionData>()?.Selected is not BlueprintScriptableObject bp)
                return false;
            var resource = bp.GetComponent<AbilityResourceLogic>();
            if (resource == null)
                return true;
            return resource.IsAbilityRestrictionPassed(ability);
        }

        public void PreCalculate(MechanicsContext context)
        {
            var data = context.MaybeCaster.GetFact(context.SourceAbility)?.GetDataExt<IActionBarConvert, VariantSelectionData>();
            if (data?.Selected is not BlueprintAbility sourceAbility)
                return;

            if (context.m_RankSources == null)
            {
                context.m_ParamsCalculator = sourceAbility.GetComponent<ContextAbilityParamsCalculator>();
                context.m_RankSources = sourceAbility.GetComponents<ContextRankConfig>().ToList();
                context.m_ValueSources = sourceAbility.GetComponents<ContextCalculateSharedValue>().ToList();
                return;
            }

            BlueprintScriptableObject oldAbility;
            if (context.m_RankSources.Count > 0)
                oldAbility = context.m_RankSources[0].OwnerBlueprint;
            else if (context.m_ValueSources.Count > 0)
                oldAbility = context.m_ValueSources[0].OwnerBlueprint;
            else
                return;

            if (oldAbility != sourceAbility)
            {
                context.m_ParamsCalculator = sourceAbility.GetComponent<ContextAbilityParamsCalculator>();
                context.m_RankSources.Clear();
                context.m_RankSources.AddRange(sourceAbility.GetComponents<ContextRankConfig>());
                context.m_ValueSources.Clear();
                context.m_ValueSources.AddRange(sourceAbility.GetComponents<ContextCalculateSharedValue>());
            }
        }

        public void PostCalculate(MechanicsContext context)
        {
        }

        public override void Apply(AbilityExecutionContext context, TargetWrapper target)
        {
            if (context.SourceAbility == null
                || context.MaybeCaster == null)
                return;

            if (context.MaybeCaster.GetFact(context.SourceAbility)?.GetDataExt<IActionBarConvert, VariantSelectionData>()?.Selected is not BlueprintAbility sourceAbility)
                return;

            // reduce resource
            sourceAbility.GetComponent<AbilityResourceLogic>()?.Spend(context.Ability);

            // run AbilityEffectRunAction
            sourceAbility.GetComponent<AbilityApplyEffect>()?.Apply(context, target);
        }
    }
}
