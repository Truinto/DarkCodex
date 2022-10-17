using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Unused.
    /// </summary>
    public class ElementalBarrage : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>
    {
        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            var unit = evt.Target.Descriptor;
            var mix = evt.DamageBundle.ToDamageTypeMix() & DamageTypeMix.EnergyPrimary;

            //if (evt.DamageBundle.Any(a => a.SourceFact == this.Fact)) return;

            if ((mix & DamageTypeMix.Acid) != 0 && unit.HasFact(Acid)
                || (mix & DamageTypeMix.Cold) != 0 && unit.HasFact(Cold)
                || (mix & DamageTypeMix.Electricity) != 0 && unit.HasFact(Electricity)
                || (mix & DamageTypeMix.Fire) != 0 && unit.HasFact(Fire)
                || (mix & DamageTypeMix.Sonic) != 0 && unit.HasFact(Sonic))
            {
                unit.RemoveFact(Acid);
                unit.RemoveFact(Cold);
                unit.RemoveFact(Electricity);
                unit.RemoveFact(Fire);
                unit.RemoveFact(Sonic);

                evt.m_DamageBundle.Add(new EnergyDamage(new DiceFormula(this.Context[AbilityRankType.Default], DiceType.D6), DamageEnergyType.Divine) { SourceFact = this.Fact });
                //this.Context.TriggerRule(new RuleDealDamage(this.Owner, evt.Target, new EnergyDamage(new DiceFormula(this.Context[AbilityRankType.Default], DiceType.D6), DamageEnergyType.Divine) { SourceFact = this.Fact }));
                return;
            }

            if ((mix & DamageTypeMix.Acid) != 0)
                unit.AddBuff(Acid, this.Context, Duration);
            if ((mix & DamageTypeMix.Cold) != 0)
                unit.AddBuff(Cold, this.Context, Duration);
            if ((mix & DamageTypeMix.Electricity) != 0)
                unit.AddBuff(Electricity, this.Context, Duration);
            if ((mix & DamageTypeMix.Fire) != 0)
                unit.AddBuff(Fire, this.Context, Duration);
            if ((mix & DamageTypeMix.Sonic) != 0)
                unit.AddBuff(Sonic, this.Context, Duration);
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
        }

        public static TimeSpan Duration = new Rounds(3).Seconds;

        public static BlueprintBuffReference Acid = Helper.ToRef<BlueprintBuffReference>("823d33bdb23e7c64d9cc1cce9b78fdea");
        public static BlueprintBuffReference Cold = Helper.ToRef<BlueprintBuffReference>("c5e9031099d3e8d4788d3e51f7ffb8a0");
        public static BlueprintBuffReference Electricity = Helper.ToRef<BlueprintBuffReference>("0b8ed343b989bbb4c8d059366a7c2d01");
        public static BlueprintBuffReference Fire = Helper.ToRef<BlueprintBuffReference>("7db8ad7b035c2f244951cbef3c9909df");
        public static BlueprintBuffReference Sonic = Helper.ToRef<BlueprintBuffReference>("49aebc21c7b9406da84c545ed0b8b5b3");
    }
}
