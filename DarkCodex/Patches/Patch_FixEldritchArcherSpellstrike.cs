using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Fix Eldritch Archer Spellstrike", "fixes spellstrike not working with swift rays", false)]
    [HarmonyPatch(typeof(UnitUseAbility), nameof(UnitUseAbility.Init))]
    public class Patch_FixEldritchArcherSpellstrike
    {
        //public static void Postfix(UnitUseAbility __instance)
        //{
        //    if (!__instance.m_IsEldritchArcherSpell && __instance.Target.Unit != null)
        //    {
        //        var unitPartMagus = __instance.Executor.Get<UnitPartMagus>();
        //        if (unitPartMagus != null
        //            && unitPartMagus.EldritchArcher
        //            && unitPartMagus.Spellstrike.Active
        //            && unitPartMagus.IsSuitableForEldritchArcherSpellStrike(__instance.Ability))
        //            __instance.m_IsEldritchArcherSpell = true;
        //    }
        //}

        /// <summary>
        /// This removes the standard action condition on eldritch archer spellstrike.
        /// if (base.Type == UnitCommand.CommandType.Standard)
        /// </summary>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions/*, ILGenerator generator, MethodBase original*/)
        {
            var code = instructions as List<CodeInstruction> ?? instructions.ToList();
            int index;

            var call = AccessTools.PropertyGetter(typeof(UnitCommand), nameof(UnitCommand.Type));

            for (index = code.Count - 1; index >= 0; index--)
            {
                if (code[index].Calls(call))
                    break;
            }

            code.NextJumpNever(ref index);

            return code;
        }
    }
}
