using Kingmaker.Controllers.Units;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Fix limitless activatables", "makes it so activatables with infinite resources start out of combat and stay on after combat", false)]
    [HarmonyPatch]
    public class Patch_LimitlessActivatables
    {
        [HarmonyPatch(typeof(UnitActivatableAbilitiesController), nameof(UnitActivatableAbilitiesController.StopOutOfCombat))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerData(instructions, generator, original);

            data.Seek(typeof(ActivatableAbility), nameof(ActivatableAbility.Stop));
            data.ReplaceCall(Patch1);

            return data.Code;
        }

        [HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.OnNewRound))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler2(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) => Transpiler1(instructions, generator, original);

        [HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.TryStart))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler3(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerData(instructions, generator, original);

            data.Seek(typeof(BlueprintActivatableAbility), nameof(BlueprintActivatableAbility.ActivateOnCombatStarts));
            data.ReplaceNOP();
            data--;
            data.ReplaceCall(Patch3);

            return data.Code;
        }

        public static void Patch1(ActivatableAbility ability, bool forceRemovedBuff)
        {
            if (!IsFree(ability))
                ability.Stop(forceRemovedBuff);
        }

        public static bool Patch3(ActivatableAbility ability)
        {
            //Main.PrintDebug("ActivatableAbility.TryStart");
            return ability.Blueprint.ActivateOnCombatStarts && !IsFree(ability);
        }

        public static bool IsFree(ActivatableAbility ability)
        {
            var free = ability.m_CachedResourceLogic.FirstItem().Component?.FreeBlueprint;
            return free != null && ability.Owner.HasFact(free);
        }



        //[HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.ReapplyBuff))]
        //[HarmonyPostfix]
        //public static void Debug1() => Main.PrintDebug("ActivatableAbility.ReapplyBuff");
        //[HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.Stop))]
        //[HarmonyPostfix]
        //public static void Debug2() => Main.PrintDebug("ActivatableAbility.Stop " + Environment.StackTrace);
    }
}
