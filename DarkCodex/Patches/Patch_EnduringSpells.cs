using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.Blueprints;
using Kingmaker;
using Kingmaker.Controllers;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Enduring Spells", "allows Enduring Spell to apply to spells from any source; fix for Magic Weapon", false)]
    [HarmonyPatch]
    public class Patch_EnduringSpells
    {
        [HarmonyPatch(typeof(EnduringSpells), nameof(EnduringSpells.HandleBuffDidAdded))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            bool flag = true;

            foreach (var line in instr)
            {
                if (flag && line.opcode == OpCodes.Ret)
                {
                    line.opcode = OpCodes.Nop;
                    flag = false;
                }
                yield return line;
            }

            if (flag)
                throw new Exception("Transpiler patch illegal state.");
        }

        public static BlueprintFeatureReference EnduringSpellsGreater = Helper.ToRef<BlueprintFeatureReference>("13f9269b3b48ae94c896f0371ce5e23c");

        [HarmonyPatch(typeof(ItemEntity), nameof(ItemEntity.AddEnchantment))]
        [HarmonyPostfix]
        public static void AddEnchantment(BlueprintItemEnchantment blueprint, MechanicsContext parentContext, Rounds? duration, ItemEnchantment __result)
        {
            if (parentContext?.MaybeCaster != null && __result != null && __result.EndTime != TimeSpan.Zero)
            {
                if (parentContext.MaybeCaster.HasFact(EnduringSpellsGreater))
                {
                    __result.EndTime = Game.Instance.TimeController.GameTime + 24.Hours();
                }
            }
        }
    }
}
