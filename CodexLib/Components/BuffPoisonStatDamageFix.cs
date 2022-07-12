using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    [AllowedOn(typeof(BlueprintBuff), false)]
    public class BuffPoisonStatDamageFix : UnitBuffComponentDelegate<BuffPoisonStatDamageData>, ITickEachRound, IPoisonStack
    {
        public StatType Stat;
        public DiceFormula Value;
        public int Bonus = 0;
        public int SuccesfullSaves = 1;
        public int Ticks = 6;
        public SavingThrowType SaveType = SavingThrowType.Fortitude;
        public bool ConsecutiveSaves = false;
        public bool NoEffectOnFirstTick = false;
        public StatType AltStat;

        public override void OnActivate()
        {
            if (this.Owner.Stats.GetStat(this.Stat) != null)
            {
                var rule = new RuleDealStatDamage(this.Context.MaybeCaster, this.Owner, this.Stat, this.Value, this.Bonus)
                {
                    Reason = this.Fact
                };
                this.Context.TriggerRule(rule);
            }
            this.Data.TicksPassed++;
        }

        void ITickEachRound.OnNewRound()
        {
            var data = this.Data;
            if (this.NoEffectOnFirstTick && data.TicksPassed == 0)
                return;

            if (this.Ticks <= data.TicksPassed++ || this.SaveType == SavingThrowType.Unknown)
            {
                this.Buff.Remove();
                return;
            }

            var stat = this.Owner.Stats.GetStat(this.Stat) ?? this.Owner.Stats.GetStat(this.AltStat);
            if (stat == null)
            {
                this.Buff.Remove();
                return;
            }

            var save = new RuleSavingThrow(this.Owner, this.SaveType, this.Context.Params.DC + data.BonusDC) { Reason = this.Fact };
            Game.Instance.Rulebook.TriggerEvent(save);
            if (!save.IsPassed)
            {
                var damage = new RuleDealStatDamage(base.Owner, this.Owner, stat.Type, this.Value, this.Bonus) { Reason = this.Fact };
                Game.Instance.Rulebook.TriggerEvent(damage);
                if (this.ConsecutiveSaves)
                    data.SavesSucceeded = 0;
            }
            else if (++data.SavesSucceeded >= this.SuccesfullSaves)
            {
                this.Buff.Remove();
            }
        }

        void IPoisonStack.Stack()
        {
            var data = this.Data;
            data.TicksPassed -= this.Ticks / 2;
            data.SavesSucceeded = Math.Max(0, data.SavesSucceeded - 1);
            data.BonusDC += 2;
        }
    }
}
