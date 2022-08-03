using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class KineticExpandedMastery : UnitFactComponentDelegate<KineticExpandedMastery.RuntimeData>, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IInitiatorRulebookHandler<RuleDealDamage>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>
    {
        public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            if (evt.Weapon == null || evt.Weapon.Blueprint.Category != WeaponCategory.KineticBlast)
                return;

            evt.AddModifier(1, this.Fact, ModifierDescriptor.UntypedStackable);
        }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            if (!IsSuitable(evt.SourceAbility))
                return;

            evt.DamageBundle.First?.AddModifier(1, this.Fact);
        }

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (!IsSuitable(evt.Spell))
                return;

            evt.AddBonusDC(1);
            evt.AddBonusCasterLevel(1);
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }

        private bool IsSuitable(BlueprintAbility source)
        {
            if (source == null)
                return False;

            var owner = this.Owner;
            var data = this.Data;
            if (data.BaseBlasts == null)
            {
                var list = new List<BlueprintAbility>();

                var t = KineticistTree.Instance;
                foreach (var focus in t.GetFocus())
                {
                    if (owner.HasFact(focus.Third))
                    {
                        list.Add(focus.Element1.BaseAbility);
                        foreach (var composite in t.GetComposites(focus.Element1, focus.Element2))
                            list.Add(composite.BaseAbility);
                        break;
                    }
                }

                data.BaseBlasts = list.ToArray();
                if (list.Count <= 0)
                    Helper.PrintError("KineticExpandedMastery could not resolve elements");
            }

            return data.BaseBlasts.Contains(source);
        }

        public class RuntimeData
        {
            [JsonProperty]
            public BlueprintAbility[] BaseBlasts;
        }
    }
}
