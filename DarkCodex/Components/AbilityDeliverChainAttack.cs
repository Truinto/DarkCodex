using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkCodex.Components
{
    public class AbilityDeliverChainAttack : AbilityDeliverEffect
    {
        public bool TargetDead;
        public float DelayBetweenChain; 
        public ContextValue TargetsCount;
        public TargetType TargetType;
        public ConditionsChecker Condition;
        public BlueprintItemWeaponReference Weapon;
        public BlueprintProjectileReference ProjectileFirst;
        public BlueprintProjectileReference Projectile;
        public bool NeedAttackRoll => Weapon != null;



        public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
        {
            //var applyEffect = context.AbilityBlueprint.GetComponent<AbilityApplyEffect>();

            var currentLauncher = context.MaybeCaster;
            var currentTarget = target.Unit;
            var usedTargets = new HashSet<UnitEntityData>();
            int targetsCount = this.TargetsCount.Calculate(context);
            float radius = context.AbilityBlueprint.GetRange(context.HasMetamagic(Metamagic.Reach), context.Ability).Meters;

            if (currentLauncher == null || currentTarget == null)
                yield break;

            int targetIndex = 0;
            while (targetIndex < targetsCount)
            {
                var delivery = DeliverInternal(context, currentLauncher, currentTarget, targetIndex == 0); // start projectile
                while (delivery.MoveNext()) // cast projectile and wait until resolved
                    yield return delivery.Current; // returning a target will immediately trigger AbilityExecutionProcess.ApplyEffect(context, delivery.Current, applyEffect, null);

                if (delivery.Current == null || delivery.Current.AttackRoll?.IsHit == false) // if process was canceled or missed stop all
                    yield break;

                targetIndex++;

                if (targetIndex < targetsCount) // get next target, unless last projectile
                {
                    if (this.DelayBetweenChain > 0f) // wait delay, if any
                    {
                        var startTime = Game.Instance.TimeController.GameTime;
                        while (Game.Instance.TimeController.GameTime - startTime < this.DelayBetweenChain.Seconds())
                            yield return null;
                    }

                    usedTargets.Add(currentTarget);
                    currentLauncher = currentTarget;
                    currentTarget = SelectNextTarget(context, currentLauncher, usedTargets, radius);
                    if (currentTarget == null) // stop if no target found
                        yield break;
                }
            }
        }

        private IEnumerator<AbilityDeliveryTarget> DeliverInternal(AbilityExecutionContext context, UnitEntityData launcher, UnitEntityData target, bool isFirst)
        {
            Projectile proj = Game.Instance.ProjectileController.Launch(launcher, target, isFirst ? this.ProjectileFirst?.Get() ?? this.Projectile.Get() : this.Projectile.Get());
            proj.IsFirstProjectile = isFirst;
            RuleAttackRoll attackRoll = null;

            if (this.NeedAttackRoll) // decide whenever the attack hit or not
            {
                var weapon = this.Weapon.Get().CreateEntity<ItemEntityWeapon>();
                attackRoll = new RuleAttackRoll(context.MaybeCaster, target, weapon, 0) { SuspendCombatLog = true };
                context.TriggerRule(attackRoll);
                if (context.ForceAlwaysHit)
                    attackRoll.SetFake(AttackResult.Hit);

                proj.AttackRoll = attackRoll;
                proj.MissTarget = context.MissTarget;
            }

            while (!proj.IsHit) // wait until projectile hit
            {
                if (proj.Cleared) // stop if projectile controller cleared projectiles
                    yield break;
                yield return null;
            }

            attackRoll?.ConsumeMirrorImageIfNecessary();

            yield return new AbilityDeliveryTarget(proj.Target)
            {
                AttackRoll = proj.AttackRoll,
                Projectile = proj
            };
        }

        private UnitEntityData SelectNextTarget(AbilityExecutionContext context, TargetWrapper center, HashSet<UnitEntityData> usedTargets, float radius)
        {
            var point = center.Point;
            float min = float.MaxValue;
            UnitEntityData result = null;
            foreach (UnitEntityData unitEntityData in Game.Instance.State.Units)
            {
                float distance = (unitEntityData.Position - point).magnitude;
                if (this.CheckTarget(context, unitEntityData) && distance <= radius && !usedTargets.Contains(unitEntityData) && distance < min)
                {
                    min = distance;
                    result = unitEntityData;
                }
            }
            return result;
        }

        private bool CheckTarget(AbilityExecutionContext context, UnitEntityData unit)
        {
            if (unit.Descriptor.State.IsDead && !this.TargetDead)
                return false;

            if ((this.TargetType == TargetType.Enemy && !context.MaybeCaster.IsEnemy(unit)) || (this.TargetType == TargetType.Ally && context.MaybeCaster.IsEnemy(unit)))
                return false;

            if (this.TargetType == TargetType.Any && context.HasMetamagic(Metamagic.Selective) && !this.Condition.HasIsAllyCondition() && !context.MaybeCaster.IsEnemy(unit))
                return false;

            if (unit.State.Features.Blinking && !context.MaybeOwner.Descriptor.IsSeeInvisibility)
            {
                var ruleRollD = new RuleRollD100(context.MaybeOwner);
                Rulebook.Trigger(ruleRollD);
                if (ruleRollD < 50)
                    return false;
            }

            if (this.Condition?.HasConditions == true)
            {
                using (context.GetDataScope(unit))
                {
                    return this.Condition.Check();
                }
            }

            return true;
        }
    }
}
