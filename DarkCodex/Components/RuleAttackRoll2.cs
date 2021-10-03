using JetBrains.Annotations;
using Kingmaker.Armies.TacticalCombat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Settings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    public class RuleAttackRoll2 : RuleAttackRoll
    {
		public RuleAttackRoll2([NotNull] UnitEntityData initiator, [NotNull] UnitEntityData target, [NotNull] RuleCalculateWeaponStats weaponStats, int attackBonusPenalty) 
			: base(initiator, target, weaponStats, attackBonusPenalty)
		{
		}

		public RuleAttackRoll2([NotNull] UnitEntityData initiator, [NotNull] UnitEntityData target, [NotNull] ItemEntityWeapon weapon, int attackBonusPenalty) 
			: base(initiator, target, weapon, attackBonusPenalty)
		{
		}

		public override void OnTrigger(RulebookEventContext context)
        {
			if (!this.WeaponStats.IsTriggered)
			{
				Rulebook.Trigger<RuleCalculateWeaponStats>(this.WeaponStats);
			}
			bool flag = this.Target.Descriptor.State.HasCondition(UnitCondition.Confusion);
			bool flag2 = !this.Target.IsEnemy(this.Initiator) && !this.Target.Faction.Neutral && !flag;
			if (this.Initiator == this.Target || (this.AttackType.IsTouch() && flag2))
			{
				this.AutoHit = true;
			}
			if (this.AutoHit)
			{
				this.Result = AttackResult.Hit;
				this.IsCriticalConfirmed = (this.AutoCriticalThreat && this.AutoCriticalConfirmation);
			}
			else if (this.AutoMiss)
			{
				this.Result = AttackResult.Miss;
			}
			else if (this.TryOvercomeTargetConcealmentAndMissChance() || TacticalCombatHelper.IsActive)
			{
				this.ACRule = Rulebook.Trigger<RuleCalculateAC>(new RuleCalculateAC(this.Initiator, this.Target, this.AttackType)
				{
					ForceFlatFooted = this.ForceFlatFooted
				});
				this.IsTargetFlatFooted = this.ACRule.IsTargetFlatFooted;
				this.TargetAC = this.ACRule.Result;
				this.AttackBonusRule = new RuleCalculateAttackBonus(this.Initiator, this.Target, this.WeaponStats, this.AttackBonusPenalty)
				{
					TargetIsFlanked = this.TargetIsFlanked
				};
				this.AttackBonusRule.CopyModifiersFrom(this);
				Rulebook.Trigger<RuleCalculateAttackBonus>(this.AttackBonusRule);
				this.AttackBonus = this.AttackBonusRule.Result;
				this.D20 = (TacticalCombatHelper.IsActive ? new RuleRollD20(this.Initiator, 21) : RulebookEvent.Dice.D20);
				bool flag3 = this.IsSuccessRoll(this.D20);
				this.Result = (flag3 ? AttackResult.Hit : this.Target.Stats.AC.SelectMissReason(this.IsTargetFlatFooted, this.AttackType.IsTouch()));
				this.IsSneakAttack = (this.IsHit && !this.ImmuneToSneakAttack && this.IsAttackRollSuitableForSneakAttack() && (this.IsTargetFlatFooted || this.Target.CombatState.IsFlanked) && this.Initiator.Stats.SneakAttack > 0);
				SettingsEntityEnum<CriticalHitPower> enemyCriticalHits = SettingsRoot.Difficulty.EnemyCriticalHits;
				this.IsCriticalRoll = (TacticalCombatHelper.IsActive ? (!this.ImmuneToCriticalHit && this.AutoCriticalThreat) : (flag3 && !this.ImmuneToCriticalHit && (this.D20 >= this.WeaponStats.CriticalEdge || this.AutoCriticalThreat) && (!this.Target.IsPlayerFaction || enemyCriticalHits == CriticalHitPower.Weak || enemyCriticalHits == CriticalHitPower.Normal)));
				if (this.IsCriticalRoll)
				{
					RuleCalculateAC ruleCalculateAC = Rulebook.Trigger<RuleCalculateAC>(new RuleCalculateAC(this.Initiator, this.Target, this.AttackType)
					{
						IsCritical = true
					});
					this.TargetCriticalAC = ruleCalculateAC.Result;
					this.CriticalConfirmationD20 = (this.AutoCriticalConfirmation ? new RuleRollD20(this.Initiator, 20) : RulebookEvent.Dice.D20);
					this.IsCriticalConfirmed = (this.AutoCriticalConfirmation || this.CriticalConfirmationRoll >= this.TargetCriticalAC);
				}
				if (this.IsSneakAttack || this.IsCriticalConfirmed || this.PreciseStrike > 0)
				{
					UnitPartFortification unitPartFortification = this.Target.Get<UnitPartFortification>();
					this.FortificationChance = ((unitPartFortification != null) ? unitPartFortification.Value : 0);
					if (this.TargetUseFortification)
					{
						this.FortificationRoll = RulebookEvent.Dice.D100;
						if (!this.FortificationOvercomed)
						{
							this.FortificationNegatesSneakAttack = this.IsSneakAttack;
							this.FortificationNegatesCriticalHit = this.IsCriticalConfirmed;
							this.IsSneakAttack = false;
							this.IsCriticalConfirmed = false;
							this.PreciseStrike = 0;
						}
					}
				}
				if (this.IsCriticalConfirmed)
				{
					this.Result = AttackResult.CriticalHit;
				}
				bool flag4 = !flag3 && this.TargetAC - this.D20 - this.AttackBonus <= 5;
				if ((flag3 || flag4) && !this.Initiator.Descriptor.IsImmuneToVisualEffects)
				{
					UnitPartMirrorImage unitPartMirrorImage = this.Target.Get<UnitPartMirrorImage>();
					if (unitPartMirrorImage != null)
					{
						this.HitMirrorImageIndex = unitPartMirrorImage.TryAbsorbHit(flag4);
						if (this.HitMirrorImageIndex > 0)
						{
							this.Result = AttackResult.MirrorImage;
						}
					}
				}
			}
			else
			{
				this.Result = AttackResult.Concealment;
			}
			if (TacticalCombatHelper.IsActive)
			{
				this.Result = AttackResult.Hit;
			}
			else if (this.IsHit && !this.AutoHit && this.Parry != null)
			{
				this.Parry.Trigger(context);
				if (this.Parry.Roll + this.Parry.AttackBonus > this.D20 + this.AttackBonus)
				{
					this.Result = AttackResult.Parried;
				}
			}
			this.IsSneakAttack &= this.IsHit;
			EventBus.RaiseEvent<IAttackHandler>(delegate (IAttackHandler h)
			{
				h.HandleAttackHitRoll(this);
			}, true);
		}
    }
}
