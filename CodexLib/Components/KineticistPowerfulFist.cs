using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.Kineticist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class KineticistPowerfulFist : UnitFactComponentDelegate<VariantSelectionData>, IInitiatorRulebookHandler<RuleDealDamage>, IKineticistCalculateAbilityCostHandler, IActionBarConvert
    {
        public List<MechanicActionBarSlot> GetConverts()
        {
            var data = this.Data;
            var list = new List<MechanicActionBarSlot>();
            int lvl = this.Owner.Progression.GetClassLevel(KineticistTree.Instance.Class);

            list.Add(new MechanicActionBarSlotVariantSelection(this.Owner, UINumber.Get(1), data));
            if (lvl >= 9)
                list.Add(new MechanicActionBarSlotVariantSelection(this.Owner, UINumber.Get(2), data));
            if (lvl >= 13)
                list.Add(new MechanicActionBarSlotVariantSelection(this.Owner, UINumber.Get(3), data));

            return list;
        }

        public Sprite GetIcon()
        {
            return this.Data.Selected?.Icon;
        }

        public void HandleKineticistCalculateAbilityCost(UnitDescriptor caster, BlueprintAbility abilityBlueprint, ref KineticistAbilityBurnCost cost)
        {
            if (this.Fact is not ActivatableAbility act || !act.IsOn)
                return;

            if (this.Data.Selected is not UINumber num)
                return;

            cost.Increase(num.Value + 1, KineticistBurnType.Infusion);
        }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            if (this.Fact is not ActivatableAbility act || !act.IsOn)
                return;

            if (this.Data.Selected is not UINumber num)
                return;

            var kin = evt.SourceAbility?.GetComponent<AbilityKineticist>();
            if (kin == null)
                return;

            var bundles = evt.m_DamageBundle.m_Chunks;
            for (int i = 0; i < bundles.Count; i++)
            {
                var dmg = bundles[i].Dice;
                var dice = dmg.m_Dice;

                for (int j = 0; i < num.Value; j++)
                {
                    if (dice == DiceType.D6)
                        dice = DiceType.D8;
                    else if (dice == DiceType.D8)
                        dice = DiceType.D10;
                    else if (dice == DiceType.D10)
                        dice = DiceType.D12;
                    else
                        break;
                }

                bundles[i].ReplaceDice(dmg);
            }
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
        }
    }
}
