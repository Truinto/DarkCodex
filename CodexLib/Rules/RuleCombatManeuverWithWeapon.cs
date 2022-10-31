using Kingmaker.UnitLogic.Parts;

namespace CodexLib
{
    /// <summary>
    /// Maneuver rule to use weapon attack bonus.
    /// </summary>
    public class RuleCombatManeuverWithWeapon : RuleCombatManeuver
    {
        public ItemEntityWeapon Weapon;
        public bool TargetAC;

        public RuleCombatManeuverWithWeapon([NotNull] UnitEntityData initiator, [NotNull] UnitEntityData target, CombatManeuver type, ItemEntityWeapon weapon, bool targetAC = false, RuleCalculateAttackBonus attackRule = null) : base(initiator, target, type, attackRule)
        {
            this.Weapon = weapon;
            this.TargetAC = targetAC;
        }

        public override void OnTrigger(RulebookEventContext context)
        {
            if (this.SkipBecauseOfShadow())
                return;

            if (this.AutoFailure || this.Target.Descriptor.State.HasCondition(UnitCondition.ImmuneToCombatManeuvers))
                return;

            if (!this.IgnoreConcealment && !(this.ConcealmentCheck = Rulebook.Trigger(new RuleConcealmentCheck(this.Initiator, this.Target, true))).Success)
                return;

            var weapon = this.Weapon ?? this.Initiator.Body.EmptyHandWeapon;
            if (weapon == null)
                return;

            if (this.OverrideBonus != null)
                this.InitiatorCMB = this.OverrideBonus.Value;
            else
            {
                var cmb = new RuleCalculateCMB(this.Initiator, this.Target, this.Type);
                cmb.ReplaceStrength = this.ReplaceBaseStat ?? cmb.ReplaceStrength;
                cmb.ReplaceBAB = this.ReplaceAttackBonus ?? cmb.ReplaceBAB;

                this.AttackRule ??= Rulebook.Trigger(new RuleCalculateAttackBonus(this.Initiator, this.Target, weapon, 0));
                foreach (Modifier modifier in this.AttackRule.AllBonuses)
                    if (modifier.Stat == StatType.Unknown)
                        AddModifierCopy(modifier);

                cmb.CopyModifiersFrom(this);
                Rulebook.Trigger(cmb);
                this.InitiatorCMB = cmb.Result;
                this.CMBRule = cmb;
            }

            if (this.TargetAC)
                this.TargetCMD = Rulebook.Trigger(new RuleCalculateAC(this.Initiator, this.Target, weapon.Blueprint.AttackType)).Result;
            else
            {
                this.CMDRule = Rulebook.Trigger(new RuleCalculateCMD(this.Initiator, this.Target, this.Type));
                this.TargetCMD = this.CMDRule.Result;
            }
            this.InitiatorRoll = (this.OverrideRoll ?? RulebookEvent.Dice.D20);

            var proj = weapon.WeaponVisualParameters.Projectiles.FirstOrDefault()?.Get();
            if (proj != null)
                Game.Instance.ProjectileController.Launch(this.Initiator, this.Target, proj, AttackResult.Hit, new RuleResolveFunction(this.Initiator, this, Resolve));
            else
                Resolve(this);
        }

        public void Resolve(RulebookEvent rule)
        {
            // check mirror image
            if (!this.Initiator.Descriptor.IsImmuneToVisualEffects)
            {
                bool nearHit = !this.Success && this.TargetCMD - this.InitiatorCMValue <= 5;
                if (this.Success || nearHit)
                {
                    var mirrorImage = this.Target.Get<UnitPartMirrorImage>();
                    if (mirrorImage != null)
                    {
                        int imageNo = mirrorImage.TryAbsorbHit(nearHit);
                        if (imageNo > 0)
                        {
                            mirrorImage.SpendReservedImage(imageNo);
                            this.InitiatorCMB = -99;
                            return;
                        }
                    }
                }
            }

            if (!this.Success)
                return;

            var initiator = this.Initiator;
            var target = this.Target;
            var weapon = this.Weapon;
            var context = this.Reason.Context;

            //if (this.DealDamage) attackroll.Reason.Context.TriggerRule(new RuleDealDamage(initiator, target, new DamageBundle(attackroll.Weapon, attackroll.WeaponStats.DamageDescription[0].CreateDamage())));

            var rounds = this.IncreasedDuration ? (this.SuccessMargin / 5 + RulebookEvent.Dice.D4).Rounds() : (this.SuccessMargin / 5 + 1).Rounds();
            bool trip = false;
            switch (this.Type)
            {
                case CombatManeuver.Trip:
                    trip = target.CanBeKnockedOff();
                    break;

                case CombatManeuver.Overrun:
                    if (!this.IsPartialSuccess)
                        trip = target.CanBeKnockedOff();
                    break;

                case CombatManeuver.Disarm:
                    ItemEntityWeapon maybeWeapon = target.Body.PrimaryHand.MaybeWeapon;
                    ItemEntityWeapon maybeWeapon2 = target.Body.SecondaryHand.MaybeWeapon;
                    if (maybeWeapon != null && !maybeWeapon.Blueprint.IsUnarmed && !maybeWeapon.Blueprint.IsNatural)
                    {
                        target.Descriptor.AddBuff(BlueprintRoot.Instance.SystemMechanics.DisarmMainHandBuff, context, rounds.Seconds);
                    }
                    else if (maybeWeapon2 != null && !maybeWeapon2.Blueprint.IsUnarmed && !maybeWeapon2.Blueprint.IsNatural)
                    {
                        target.Descriptor.AddBuff(BlueprintRoot.Instance.SystemMechanics.DisarmOffHandBuff, context, rounds.Seconds);
                    }
                    break;

                case CombatManeuver.Grapple:
                    break;

                case CombatManeuver.BullRush:
                    Feet feet = 5.Feet() + this.SuccessMargin.Feet();
                    Vector3 normalized = (target.Position - initiator.Position).normalized;
                    target.Ensure<UnitPartForceMove>().Push(normalized, feet.Meters, false);
                    break;

                case CombatManeuver.SunderArmor:
                    target.Descriptor.AddBuff(BlueprintRoot.Instance.SystemMechanics.SunderArmorBuff, context, rounds.Seconds);
                    break;

                case CombatManeuver.DirtyTrickBlind:
                    target.Descriptor.AddBuff(BlueprintRoot.Instance.SystemMechanics.DirtyTrickBlindnessBuff, context, rounds.Seconds);
                    break;

                case CombatManeuver.DirtyTrickEntangle:
                    target.Descriptor.AddBuff(BlueprintRoot.Instance.SystemMechanics.DirtyTrickEntangledBuff, context, rounds.Seconds);
                    break;

                case CombatManeuver.DirtyTrickSickened:
                    target.Descriptor.AddBuff(BlueprintRoot.Instance.SystemMechanics.DirtyTrickSickenedBuff, context, rounds.Seconds);
                    break;

                case CombatManeuver.Pull:
                    Vector3 normalized2 = (initiator.Position - target.Position).normalized;
                    float distance = initiator.DistanceTo(target) - initiator.Corpulence - target.Corpulence;
                    target.Ensure<UnitPartForceMove>().Push(normalized2, distance, false);
                    break;

                default:
                    throw new Exception("Unsupported combat maneuver type: " + this.Type.ToString());
            }

            if (trip)
            {
                target.Descriptor.State.Prone.ShouldBeActive = true;
                EventBus.RaiseEvent<IKnockOffHandler>(h => { h.HandleKnockOff(initiator, target); }, true);
            }
        }
    }
}
