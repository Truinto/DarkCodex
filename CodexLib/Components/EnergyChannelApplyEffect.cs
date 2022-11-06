using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class EnergyChannelApplyEffect : AbilityApplyEffect
    {
        public BlueprintBuffReference Buff;
        public DamageEnergyType Element;

        /// <param name="buff">type: <b>BlueprintBuff</b></param>
        public EnergyChannelApplyEffect(AnyRef buff, DamageEnergyType element)
        {
            this.Buff = buff;
            this.Element = element;
        }

        public override void Apply(AbilityExecutionContext context, TargetWrapper target)
        {
            if (context.SourceAbility == null
                || context.MaybeCaster == null
                || target.Unit == null)
                return;

            var data = context.MaybeCaster.GetFact(context.SourceAbility).GetDataExt<IActionBarConvert, VariantSelectionData>();
            if (data?.Selected is not BlueprintAbility sourceAbility)
                return;

            // reduce resource
            var resource = sourceAbility?.GetComponent<AbilityResourceLogic>()?.RequiredResource;
            if (resource == null || !context.MaybeCaster.Resources.HasEnoughResource(resource, 1))
                return;
            context.MaybeCaster.Resources.Spend(resource, 1);

            // set context values equal to source ability
            context[AbilitySharedValue.Damage] = sourceAbility.GetComponent<ContextRankConfig>(f => f.Type == AbilityRankType.Default).GetValue(context);
            context[AbilitySharedValue.DamageBonus] = sourceAbility.GetComponent<ContextRankConfig>(f => f.Type == AbilityRankType.DamageBonus).GetValue(context);
            context[AbilitySharedValue.StatBonus] = (int)this.Element;

            // apply buff
            var buff = target.Unit.Descriptor.AddBuff(this.Buff, context, 30.Rounds().Seconds);
        }
    }
}
