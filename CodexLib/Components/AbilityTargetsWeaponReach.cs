using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.UI.AbilityTarget;
using Kingmaker.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    [HarmonyPatch]
    public static class Patch_FixAbilityTargetsWeaponReach
    {
        public static readonly Feet Marker = (-123).Feet();

        //[HarmonyPatch(typeof(BlueprintAbility), nameof(BlueprintAbility.AppendAoE))]
        //[HarmonyPatch(typeof(AbilityTargetsAround), nameof(AbilityTargetsAround.AoERadius), MethodType.Getter)]
        //[HarmonyPostfix]
        //public static void Postfix1b(AbilityTargetsAround __instance, ref Feet __result)
        //{
        //    var context = ContextData<MechanicsContext.Data>.Current?.Context;
        //    _ = context.MaybeCaster;
        //}

        [HarmonyPatch(typeof(AbilityTargetsAround), nameof(AbilityTargetsAround.WouldTargetUnit))]
        [HarmonyPrefix]
        public static bool Prefix1(AbilityData ability, Vector3 targetPos, UnitEntityData unit, AbilityTargetsAround __instance, ref bool __result)
        {
            if (__instance.m_SpreadSpeed != Marker)
                return true;

            __result = unit.IsUnitInRange(targetPos, GetRealRadius(__instance.m_Radius, ability.Caster).Meters);
            return false;
        }

        [HarmonyPatch(typeof(AbilityTargetsAround), nameof(AbilityTargetsAround.Select))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler2(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerTool(instructions, generator, original);

            data.InsertAfterAll(typeof(AbilityTargetsAround), nameof(AbilityTargetsAround.m_Radius), patch);

            return data;

            static Feet patch(Feet __stack, AbilityExecutionContext context, AbilityTargetsAround __instance)
            {
                if (__instance.m_SpreadSpeed != Marker)
                    return __stack;

                return GetRealRadius(__instance.m_Radius, context.MaybeCaster);
            }
        }

        [HarmonyPatch(typeof(AbilityAoERange), nameof(AbilityAoERange.GetRadius))]
        [HarmonyPrefix]
        public static bool Prefix3(AbilityAoERange __instance, ref float __result)
        {
            var targetsAround = __instance.Ability?.Blueprint?.GetComponent<AbilityTargetsAround>();
            if (targetsAround != null || targetsAround.m_SpreadSpeed != Marker)
                return true;

            __result = GetRealRadius(targetsAround.m_Radius, __instance.Ability.Caster).Meters * 2f;
            return false;
        }

        [HarmonyPatch(typeof(CharacterUIDecal), nameof(CharacterUIDecal.HandleCurrentAbilityShow))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler4(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerTool(instructions, generator, original);

            data.InsertAfterAll(typeof(BlueprintAbility), nameof(BlueprintAbility.AoERadius), patch);

            return data;

            static Feet patch(Feet __stack, AbilityData ability, CharacterUIDecal __instance)
            {
                var targetsAround = ability.Blueprint.GetComponent<AbilityTargetsAround>();
                if (targetsAround != null || targetsAround.m_SpreadSpeed != Marker)
                    return __stack;

                return GetRealRadius(targetsAround.m_Radius, ability.Caster);
            }
        }

        [HarmonyPatch(typeof(CharacterUIDecal), nameof(CharacterUIDecal.HandleAbilityTargetHover))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler5(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) => Transpiler4(instructions, generator, original);


        public static Feet GetRealRadius(Feet basevalue, UnitEntityData caster = null, UnitEntityData target = null)
        {
            var weapon = caster?.GetFirstWeapon();
            if (weapon == null)
                return basevalue;

            float corpulence = caster.View.Corpulence + (target?.View.Corpulence ?? 0.5f);
            return weapon.AttackRange + corpulence.Feet();
        }
    }

    public class AbilityTargetsWeaponReach : AbilitySelectTarget, IAbilityAoERadiusProvider
    {
        public Feet BaseRange;
        public Feet SpreadSpeed;
        private bool IncludeDead;
        private ConditionsChecker Condition;

        public AbilityTargetsWeaponReach(Feet baseRange, TargetType targets = TargetType.Enemy, Feet spreadSpeed = default, bool includeDead = false, ConditionsChecker condition = null)
        {
            this.BaseRange = baseRange;
            this.Targets = targets;
            this.SpreadSpeed = spreadSpeed;
            this.IncludeDead = includeDead;
            this.Condition = condition ?? new();
        }

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
