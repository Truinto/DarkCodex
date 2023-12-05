using HarmonyLib;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.ActivatableAbilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniRx;
using Shared;
using CodexLib;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Activatable OnTurnOn", "fixes activatable not being allowed to be active when they have the same action (like 2 move actions)", false)]
    [HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.OnDidTurnOn))]
    public static class Patch_ActivatableOnTurnOn
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerTool(instructions, generator, original);
            data.ReplaceAllCalls(typeof(EntityFact), nameof(EntityFact.GetComponent), Patch, null, [typeof(ActivatableAbilityUnitCommand)]);
            return data;
        }

        public static ActivatableAbilityUnitCommand Patch(EntityFact instance)
        {
            return null;
        }
    }
}
