namespace CodexLib
{
    /// <summary>
    /// Fix for overriding 'AbilityData.OverridenResourceLogic', when the logic is also IAbilityRestriction.
    /// </summary>
    [HarmonyPatch]
    public class Patch_ResourceOverride
    {
        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.CanBeCastByCaster), MethodType.Getter)]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerTool(instructions, generator, original);

            data.Seek(typeof(IAbilityRestriction), nameof(IAbilityRestriction.IsAbilityRestrictionPassed));
            data.ReplaceCall(patch);

            return data.Code;

            static bool patch(IAbilityRestriction instance, AbilityData ability)
            {
                if (instance is IAbilityResourceLogic
                    && ability.OverridenResourceLogic is IAbilityRestriction replace)
                {
                    return replace.IsAbilityRestrictionPassed(ability);
                }

                return instance.IsAbilityRestrictionPassed(ability);
            }
        }
    }
}
