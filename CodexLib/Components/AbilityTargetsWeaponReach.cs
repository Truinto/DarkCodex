using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class AbilityTargetsWeaponReach : AbilitySelectTarget, IAbilityAoERadiusProvider // TODO: use for BladeWhirlwind
    {
        public Feet BaseRange;
        public Feet SpreadSpeed;
        private bool IncludeDead;
        private ConditionsChecker Condition;

        public TargetType Targets { get; set; }

        public Feet AoERadius => GetRealRadius();

        public Feet GetRealRadius(UnitEntityData caster = null, UnitEntityData target = null)
        {
            var weapon = caster?.GetFirstWeapon();
            if (weapon == null)
                return BaseRange;

            float corpulence = caster.View.Corpulence + (target?.View.Corpulence ?? 0.5f);
            return weapon.AttackRange + corpulence.Feet();
        }

        public override Feet GetSpreadSpeed() => SpreadSpeed;

        public override IEnumerable<TargetWrapper> Select(AbilityExecutionContext context, TargetWrapper anchor)
        { 
            if (context.MaybeCaster == null)
            {
                Helper.PrintDebug("Caster is missing");
                return Enumerable.Empty<TargetWrapper>();
            }
            var source = GameHelper.GetTargetsAround(anchor.IsUnit ? anchor.Unit.EyePosition : anchor.Point, GetRealRadius(context.MaybeCaster, anchor.Unit), true, this.IncludeDead);

            switch (this.Targets)
            {
                case TargetType.Enemy:
                    source = source.Where(new Func<UnitEntityData, bool>(context.MaybeCaster.IsEnemy));
                    break;
                case TargetType.Ally:
                    source = source.Where(new Func<UnitEntityData, bool>(context.MaybeCaster.IsAlly));
                    break;
                case TargetType.Any:
                    if (context.HasMetamagic(Metamagic.Selective) && !this.Condition.HasIsAllyCondition())
                        source = source.Where(new Func<UnitEntityData, bool>(context.MaybeCaster.IsEnemy));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (!context.AbilityBlueprint.CanTargetFriends)
            {
                source = from t in source
                         where t.IsPlayerFaction || context.Caster.CanAttack(t)
                         select t;
            }
            if (this.Condition.HasConditions)
            {
                source = source.Where(delegate (UnitEntityData u)
                {
                    bool result;
                    using (context.GetDataScope(u))
                    {
                        result = this.Condition.Check();
                    }
                    return result;
                }).ToList<UnitEntityData>();
            }
            return from u in source
                   select new TargetWrapper(u);
        }

        public bool WouldTargetUnit(AbilityData ability, Vector3 targetPos, UnitEntityData unit)
        {
            return unit.IsUnitInRange(targetPos, GetRealRadius(ability.Caster, unit).Meters, true);
        }
    }
}
