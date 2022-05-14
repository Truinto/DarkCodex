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
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            List<CodeInstruction> list = instr.ToList();
            var original = AccessTools.Method(typeof(EntityFact), nameof(EntityFact.GetComponent), null, typeof(ActivatableAbilityUnitCommand).ObjToArray());

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Calls(original))
                {
                    Main.PrintDebug("Patched at " + i);
                    list[i] = CodeInstruction.Call(typeof(Patch_ActivatableOnTurnOn), nameof(NullReplacement));
                }
            }

            return list;
        }

        public static object NullReplacement(ActivatableAbility something)
        {
            return null;
        }
    }
}
