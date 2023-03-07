using HarmonyLib;
using Kingmaker;
using Kingmaker.Armies.TacticalCombat;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.Critters;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Kingmaker.Controllers.Clicks.Handlers;

namespace CodexLib
{
    /// <summary>
    /// Ability logic to teleport caster to point and return targets inbetween plus reach
    /// </summary>
    public class AbilityDeliverTeleportTrample : AbilityDeliverProjectile, IAbilityTargetRestriction //AbilityDeliverEffect
    {
        public BlueprintProjectileReference Projectile;
        public PrefabLink DisappearFx;
        public float DisappearDuration;
        public PrefabLink AppearFx;
        public int TargetLimit = int.MaxValue;
        public bool UseReach;

        public AbilityDeliverTeleportTrample()
        {
            //m_ControlledProjectileHolderBuff = BlueprintRoot.Instance.SystemMechanics.m_PetrifiedBuff;
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper spellTarget)
        {
            yield return null;

            bool hasMainTarget = false;
            int targetLimit = TargetLimit;
            var caster = context.Caster;
            var units = new List<UnitEntityData>() { caster };
            AbilityCustomDimensionDoor.AddMounts(units);

            var targetPoint = spellTarget.Point;
            Helper.PrintDebug($"AbilityDeliverTeleportTrample unit={spellTarget.Unit} point={spellTarget.m_Point} orientation={spellTarget.Orientation}");
            if (spellTarget.Unit != null) // move target, if would land on a unit
            {
                targetPoint += Quaternion.Euler(0f, spellTarget.Unit.Orientation, 0f)
                    * Vector3.forward
                    * (caster.View.Corpulence + spellTarget.Unit.View.Corpulence + GameConsts.MinWeaponRange.Meters);
                hasMainTarget = true;
                targetLimit--;
            }

            var targetPoints = new Vector3[units.Count];
            for (int i = 0; i < units.Count; i++)
                targetPoints[i] = units[i].Position - caster.Position + targetPoint;

            var routine = CreateTeleportationRoutine(context, units, caster.Position, targetPoints);
            while (routine.MoveNext())
            {
                if (routine.Current is not null && targetLimit-- > 0)
                {
                    yield return routine.Current;
                    if (hasMainTarget && routine.Current.Target.Unit == spellTarget.Unit)
                        hasMainTarget = false;
                }
                else
                    yield return null;
            }

            if (hasMainTarget)
                yield return new AbilityDeliveryTarget(spellTarget.Unit);

            yield break;
        }

        public IEnumerator<AbilityDeliveryTarget> CreateTeleportationRoutine(AbilityExecutionContext context, List<UnitEntityData> units, Vector3 sourcePosition, Vector3[] targetPosition)
        {
            var sfxDisappear = this.DisappearFx?.Load();
            var sfxAppear = this.AppearFx?.Load();
            var projTeleportation = this.Projectile?.Get();
            var startTime = Game.Instance.TimeController.GameTime;

            // disappear
            foreach (var unit in units)
            {
                unit.View.StopMoving();
                unit.AddBuff(BlueprintRoot.Instance.SystemMechanics.DimensionDoorBuff, null, new TimeSpan?(AbilityCustomDimensionDoor.MaxTeleportationDuration));
                FxHelper.SpawnFxOnUnit(sfxDisappear, unit.View);
            }

            // transport
            if (projTeleportation != null)
            {
                var projectileRoutine = CreateProjectileRoutine(context, projTeleportation, units, sourcePosition, targetPosition[0]);
                while (projectileRoutine.MoveNext())
                {
                    yield return projectileRoutine.Current;
                    if (Game.Instance.TimeController.GameTime - startTime > AbilityCustomDimensionDoor.MaxTeleportationDuration)
                        break;
                }
            }

            // appear
            for (int i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                unit.CombatState.PreventAttacksOfOpporunityNextFrame = true;
                unit.Position = targetPosition[i];
                FxHelper.SpawnFxOnUnit(sfxAppear, unit.View);
                unit.RemoveFact(BlueprintRoot.Instance.SystemMechanics.DimensionDoorBuff);
                foreach (var familiar in unit.Familiars)
                    familiar.TeleportToMaster();
            }

            yield break;
        }

        public IEnumerator<AbilityDeliveryTarget> CreateProjectileRoutine(AbilityExecutionContext context, BlueprintProjectile blueprint, List<UnitEntityData> units, Vector3 sourcePosition, Vector3 targetPosition)
        {
            var usedUnits = units.ToList();
            Vector3 direction3d = (targetPosition - sourcePosition).ToXZ().normalized;
            Vector2 direction2d = direction3d.To2D().normalized;

            //ItemEntityWeapon itemEntityWeapon = unit.GetThreatHandMelee()?.MaybeWeapon ?? this.Weapon.CreateEntity<ItemEntityWeapon>();

            var proj = Game.Instance.ProjectileController.Launch(units[0], targetPosition, blueprint);
            while (true)
            {
                yield return null;
                if (proj.Cleared)
                    break;

                float passedDistance = proj.PassedDistance;
                foreach (var bystander in Game.Instance.State.Units)
                {
                    if (bystander.Descriptor.State.IsDead)
                        continue;
                    if (usedUnits.Contains(bystander))
                        continue;

                    if (WouldTargetUnitLine(context.Ability, bystander, sourcePosition, direction2d, passedDistance))
                    {
                        usedUnits.Add(bystander);
                        yield return new AbilityDeliveryTarget(bystander)
                        {
                            Projectile = proj
                        };
                        passedDistance = proj.PassedDistance;
                    }
                }

                if (proj.IsHit)
                    break;
            }

            yield break;
        }

        public override bool WouldTargetUnit(AbilityData ability, Vector3 targetPos, UnitEntityData unit, AbilityParams cachedParams = null)
        {
            var caster = ability.Caster.Unit;
            Vector3 normalized = (targetPos - caster.Position).normalized;
            Vector3 launchPos = caster.Position + normalized * caster.Corpulence;
            float meters = ability.Blueprint.GetRange(ability.HasMetamagic(Metamagic.Reach), ability).Meters;
            return WouldTargetUnitLine(ability, unit, launchPos, normalized.To2D(), meters);
        }

        public bool WouldTargetUnitLine(AbilityData ability, UnitEntityData unit, Vector3 launchPos, Vector2 castDir, float distance)
        {
            if (!CanTargetUnit(ability, unit))
            {
                return false;
            }

            float reach = this.UseReach ? ability.Caster.Unit.GetThreatHandMelee()?.Weapon?.AttackRange.Meters ?? 3f : 0;
            float width = this.UseReach ? reach * 2f : this.LineWidth.Meters * 0.5f + unit.Corpulence;
            float a = Vector2.Dot((unit.Position - launchPos).To2D(), castDir);
            if (a <= 0f || a >= distance + unit.Corpulence + reach) // area will extend forward by reach as well
            {
                return false;
            }
            Vector2 b = castDir * a;
            return ((unit.Position - launchPos).To2D() - b).sqrMagnitude <= width * width;
        }

        public bool CanTargetUnit(AbilityData ability, UnitEntityData unit)
        {
            bool isAlly = ability.Caster.Unit.IsAlly(unit);
            if (!isAlly && ability.Blueprint.CanTargetEnemies)
                return true;
            if (isAlly && ability.Blueprint.CanTargetFriends && (ability.Blueprint.EffectOnAlly == AbilityEffectOnUnit.Helpful || !ability.HasMetamagic(Metamagic.Selective)))
                return true;

            return false;
        }

        public bool IsTargetRestrictionPassed(UnitEntityData caster, TargetWrapper target)
        {
            return !AbilityCustomDimensionDoor.CheckTargetIsOnDisabledIsland(target) && ObstacleAnalyzer.IsPointInsideNavMesh(target.Point);// && !FogOfWarController.IsInFogOfWar(target.Point);
        }

        public string GetAbilityTargetRestrictionUIText(UnitEntityData caster, TargetWrapper target)
        {
            return LocalizedTexts.Instance.Reasons.TargetIsInvalid;
        }
    }
}
