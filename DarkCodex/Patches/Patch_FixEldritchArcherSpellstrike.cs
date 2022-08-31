using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using System.CodeDom.Compiler;
using Kingmaker.Armies.TacticalCombat;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Controllers.Projectiles;
using static DarkCodex.Resource;

namespace DarkCodex
{
    [HarmonyPatch]
    [PatchInfo(Severity.Harmony | Severity.Faulty | Severity.DefaultOff, "Fix Eldritch Archer Spellstrike", "fixes spellstrike not working with swift rays", false)]
    public class Patch_FixEldritchArcherSpellstrike
    {
        //[HarmonyPatch(typeof(UnitUseAbility), nameof(UnitUseAbility.Init))]
        //public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions/*, ILGenerator generator, MethodBase original*/)
        //{
        //    var code = instructions as List<CodeInstruction> ?? instructions.ToList();
        //    int index;
        //    var call = AccessTools.PropertyGetter(typeof(UnitCommand), nameof(UnitCommand.Type));
        //    for (index = code.Count - 1; index >= 0; index--)
        //    {
        //        if (code[index].Calls(call))
        //            break;
        //    }
        //    code.NextJumpNever(ref index);
        //    return code;
        //}

        [HarmonyPatch(typeof(UnitUseAbility), nameof(UnitUseAbility.OnAction))]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var code = instructions as List<CodeInstruction> ?? instructions.ToList();

            var call = AccessTools.Method(typeof(EntityDataBase), nameof(EntityDataBase.Get), new Type[] { }, new Type[] { typeof(UnitPartMagus) });
            int index = 0;

            for (; index < code.Count; index++)
            {
                if (code[index].Calls(call))
                    break;
            }
            index -= 2;
            if (code[index].opcode != OpCodes.Ldarg_0)
                throw new Exception();

            code.AddCondition(ref index, Patch, generator, typeof(int));



            return code;
        }

        public static bool Patch(object instance, ref object result)
        {
            Main.PrintDebug("Patch_FixEldritchArcherSpellstrike 1");

            if (instance is not UnitUseAbility __instance)
                return true;

            Main.PrintDebug("Patch_FixEldritchArcherSpellstrike 2");

            if (__instance.Type != UnitCommand.CommandType.Swift || __instance.Target.Unit == null)
                return true;

            Main.PrintDebug("Patch_FixEldritchArcherSpellstrike 3");

            var unitPartMagus = __instance.Executor.Get<UnitPartMagus>();
            if (!unitPartMagus || !unitPartMagus.EldritchArcher || !unitPartMagus.Spellstrike.Active || !unitPartMagus.IsSuitableForEldritchArcherSpellStrike(__instance.Ability))
                return true;

            Main.PrintDebug("Patch_FixEldritchArcherSpellstrike 4");

            var weapon = __instance.Executor.GetFirstWeapon();
            if (weapon == null || !weapon.Blueprint.IsRanged || weapon.Blueprint.VisualParameters.Projectiles.Length == 0)
                return true;

            Main.PrintDebug("Patch_FixEldritchArcherSpellstrike 5");

            //unitPartMagus.SetEldritchArcherSpell(this.Ability, this.SureHandFxList());
            //base.Executor.Commands.AddToQueueFirst(new UnitAttack(this.Target.Unit, default(float?))
            //{
            //    ClearFxOnAttack = this.m_HandFxObjects
            //});

            (__instance.m_AbilityOriginal ?? __instance.Ability).Spend();

            var attackWithWeapon = Rulebook.Trigger(new RuleAttackWithWeapon(__instance.Executor, __instance.TargetUnit, weapon, 0) 
            {
                ExtraAttack = true
            });

            //if (attackWithWeapon.AttackRoll.IsHit)
            {
                var proj = attackWithWeapon.LaunchedProjectiles.FirstOrDefault();
                //if (proj != null && proj.OnHitTrigger is RuleAttackWithWeaponResolve resolve)
                //{
                //    resolve.OnResolve = f => { /* spell here? */ };
                //}

                var castSpell = Rulebook.Trigger(new RuleCastSpell(__instance.Ability, __instance.TargetUnit));
                castSpell.Context.AttackRoll = attackWithWeapon.AttackRoll;
                castSpell.Context.MissTarget = proj?.MissTarget;
            }

            //var stats = Rulebook.Trigger(new RuleCalculateWeaponStats(__instance.Executor, weapon));
            //var attack = Rulebook.Trigger(new RuleAttackRoll(__instance.Executor, __instance.Target.Unit, new RuleCalculateWeaponStats(__instance.Executor, weapon), 0));
            //var projectiles = weapon.Blueprint.VisualParameters.Projectiles.FirstOrDefault();

            result = (int)UnitCommand.ResultType.Success;
            Main.PrintDebug("Patch_FixEldritchArcherSpellstrike 6");
            return false;
        }

        //private void LaunchProjectile(BlueprintProjectile blueprint, bool first, RuleAttackRoll attackRoll, ItemEntityWeapon weapon)
        //{
        //    Projectile projectile;
        //    if (attackRoll.IsHit)
        //    {
        //        var ruleDealDamage = weapon.Blueprint.HasNoDamage ? null : this.CreateRuleDealDamage(TacticalCombatHelper.IsActive || first);
        //        var ruleAttackWithWeaponResolve = new RuleAttackWithWeaponResolve(this, ruleDealDamage);
        //        projectile = Game.Instance.ProjectileController.Launch(attackRoll.Initiator, attackRoll.Target, blueprint, attackRoll, ruleAttackWithWeaponResolve); // replace rule?
        //        if (ruleDealDamage != null)
        //        {
        //            ruleDealDamage.Projectile = projectile;
        //        }
        //    }
        //    else
        //    {
        //        var ruleAttackWithWeaponResolve2 = new RuleAttackWithWeaponResolve(this, null);
        //        projectile = Game.Instance.ProjectileController.Launch(attackRoll.Initiator, attackRoll.Target, blueprint, attackRoll, ruleAttackWithWeaponResolve2);
        //        projectile.CalculateMissTarget();
        //    }
        //    projectile.IsFromWeapon = true;
        //    var unitPartMagus = attackRoll.Initiator.Get<UnitPartMagus>();
        //    if (unitPartMagus && unitPartMagus.EldritchArcherSpell != null)
        //    {
        //        RuleCastSpell ruleCastSpell = Rulebook.Trigger(new RuleCastSpell(unitPartMagus.EldritchArcherSpell, attackRoll.Target));
        //        ruleCastSpell.Context.AttackRoll = attackRoll;
        //        ruleCastSpell.Context.MissTarget = ((projectile != null) ? projectile.MissTarget : null);
        //    }
        //}

    }
}
