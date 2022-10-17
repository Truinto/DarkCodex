using Kingmaker.RuleSystem.Rules.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class IncreaseModifierBonus : UnitFactComponentDelegate, IAfterRule, IInitiatorRulebookHandler<RuleSkillCheck>, IUnitGainFactHandler, IUnitLostFactHandler
    {
        public int Bonus;
        public ModifierDescriptor Descriptor;

        public IncreaseModifierBonus(int bonus = 1, ModifierDescriptor descriptor = ModifierDescriptor.Luck)
        {
            this.Bonus = bonus;
            this.Descriptor = descriptor;
        }

        public override void OnTurnOn()
        {
            Update();
        }

        public override void OnTurnOff()
        {
            foreach (var stat in this.Owner.Stats.AllStats)
                stat.RemoveModifiersFrom(this.Runtime);
        }

        public void HandleUnitGainFact(EntityFact fact)
        {
            Update();
        }

        public void HandleUnitLostFact(EntityFact fact)
        {
            Update();
        }

        public void Update()
        {
            foreach (var stat in this.Owner.Stats.AllStats)
            {
                stat.ModifierList.TryGetValue(this.Descriptor, out var list);

                if (list == null || !list.Any(a => a.StackMode == ModifiableValue.StackMode.Default))
                {
                    stat.RemoveModifiersFrom(this.Runtime);
                }
                else if (!list.Any(a => a.StackMode == ModifiableValue.StackMode.ForceStack))
                {
                    stat.AddModifier(new ModifiableValue.Modifier
                    {
                        ModValue = this.Bonus,
                        ModDescriptor = this.Descriptor,
                        StackMode = ModifiableValue.StackMode.ForceStack,
                        AppliedTo = stat,
                        Source = this.Fact,
                        SourceComponent = this.name
                    });
                }
            }
        }

        public void OnEventAboutToTrigger(RuleSkillCheck evt)
        {
            // check for ArchaeologistLuckBuff : BuffAbilityRollsBonus
            evt.Bonus.ModifierList.TryGetValue(this.Descriptor, out var list);
            if (list != null && list.Any(a => a.StackMode == ModifiableValue.StackMode.Default) && !list.Any(a => a.StackMode == ModifiableValue.StackMode.ForceStack))
            {
                evt.Bonus.AddModifier(new ModifiableValue.Modifier
                {
                    ModValue = this.Bonus,
                    ModDescriptor = this.Descriptor,
                    StackMode = ModifiableValue.StackMode.ForceStack,
                    AppliedTo = evt.Bonus,
                    Source = this.Fact,
                    SourceComponent = this.name
                });
            }
        }

        public void OnEventDidTrigger(RuleSkillCheck evt)
        {
        }
    }
}
