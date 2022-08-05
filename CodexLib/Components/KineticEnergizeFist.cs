using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Class.Kineticist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    [AllowedOn(typeof(BlueprintActivatableAbility))]
    public class KineticEnergizeFist : UnitFactComponentDelegate<VariantSelectionData>, IActionBarConvert, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookHandler<RuleAttackWithWeaponResolve>
    {
        public bool IsFist;

        public KineticEnergizeFist(bool isFist)
        {
            this.IsFist = isFist;
        }

        public List<MechanicActionBarSlot> GetConverts()
        {
            var data = this.Data;

            var list = new List<MechanicActionBarSlot>();
            foreach (var element in KineticistTree.Instance.GetAll(true, true, archetype: true))
            {
                if (this.Owner.HasFact(element.BlastFeature))
                    list.Add(new MechanicActionBarSlotVariantSelection(this.Owner, element, data));
            }

            return list;
        }

        public Sprite GetIcon()
        {
            return this.Data.Wrapper.Selected?.Icon;
        }

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (this.Fact is not ActivatableAbility act || !act.IsOn)
                return;

            if (this.Data.Wrapper.Selected is not KineticistTree.Element element)
                return;

            var wbp = evt.Weapon.Blueprint;
            if ((wbp.IsNatural || wbp.IsUnarmed) ^ this.IsFist)
                return;

            if (wbp.FighterGroup == 0) // exclude touch/ray attacks and kinetic blades
                return;

            // add damage to weapon stats (does not include metakinesis)
            var rank = evt.Initiator.Descriptor.Progression.Features.GetRank(KineticistTree.Instance.KineticBlast);
            if (IsFist)
            {
                if (element.IsComposite && !element.IsMixType)
                    rank *= 2;
                rank = Math.Max(1, rank / 3);
            }
            else
            {
                rank += 2;
                rank /= 3;
                rank = Math.Max(1, rank);
                if (element.IsComposite && !element.IsMixType)
                    rank *= 2;
            }

            var dice = new DiceFormula(rank, DiceType.D6);
            foreach (var descript in element.DamageType)
            {
                var damage = new DamageDescription
                {
                    TypeDescription = descript,
                    Dice = dice,
                    SourceFact = act
                };
                evt.DamageDescription.Add(damage);
            }
        }

        public void OnEventAboutToTrigger(RuleAttackWithWeaponResolve evt)
        {
            if (this.Fact is not ActivatableAbility act || !act.IsOn)
                return;

            var kineticist = evt.Initiator.Get<UnitPartKineticist>();
            if (kineticist == null)
                return;

            if (evt.AttackWithWeapon.IsFirstAttack)
                kineticist.RemoveBladeActivatedBuff();

            if (this.Data.Wrapper.Selected is not KineticistTree.Element element)
                return;

            var wbp = evt.AttackWithWeapon.Weapon.Blueprint;
            if ((wbp.IsNatural || wbp.IsUnarmed) ^ this.IsFist)
                return;

            if (wbp.FighterGroup == 0)
                return;

            var abilityData = new AbilityData(element.BaseAbility, evt.Initiator);

            // handle burn
            if (!kineticist.IsBladeActivated)
            {
                var cost = new KineticistAbilityBurnCost(blast: element.IsComposite ? 2 : 0, infusion: 1, metakinesis: 0, wildTalent: 0);
                EventBus.RaiseEvent<IKineticistCalculateAbilityCostHandler>(h => h.HandleKineticistCalculateAbilityCost(evt.Initiator, element.BaseAbility, ref cost), true);
                int total = cost.Total;

                // if burn cannot be paid, remove damage and return
                if (kineticist.LeftBurnThisRound < total)
                {
                    var chunks = evt.Damage.m_DamageBundle.m_Chunks;
                    for (int i = chunks.Count - 1; i >= 1; i--)
                    {
                        if (chunks[i].SourceFact == act)
                            chunks.RemoveAt(i);
                    }
                    return;
                }

                total = kineticist.AcceptBurn(total, abilityData);
                EventBus.RaiseEvent<IKineticistAcceptBurnHandler>(evt.Initiator, h => h.HandleKineticistAcceptBurn(kineticist, total, abilityData));

                kineticist.AddBladeActivatedBuff();
            }

            // check metakinesis
            bool empower = abilityData.HasMetamagic(Metamagic.Empower);
            bool maximize = abilityData.HasMetamagic(Metamagic.Maximize);
            if (empower || maximize)
            {
                var chunks = evt.Damage.m_DamageBundle.m_Chunks;
                for (int i = 1; i < chunks.Count; i++)
                {
                    if (chunks[i].SourceFact != act)
                        continue;

                    if (empower)
                        chunks[i].EmpowerBonus = 1.5f;
                    if (maximize)
                        chunks[i].CalculationType = DamageCalculationType.Maximized;
                }
            }

            // invoke fake damage for substance infusions
            var reason = new RuleReason(evt);
            reason.SetRuleAbility(abilityData);
            var rule = new RuleDealDamage(evt.Initiator, evt.Target, new DirectDamage(DiceFormula.Zero, 0))
            {
                IsFake = true,
                DisableBattleLog = true,
                Reason = reason,
            };

            Rulebook.Trigger(rule);
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
        }

        public void OnEventDidTrigger(RuleAttackWithWeaponResolve evt)
        {
        }
    }
}
