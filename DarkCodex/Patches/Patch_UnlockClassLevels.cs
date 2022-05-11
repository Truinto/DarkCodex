using HarmonyLib;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_UnlockClassLevels
    {
        [HarmonyPatch(typeof(BlueprintCharacterClass), nameof(BlueprintCharacterClass.MeetsPrerequisites))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> MeetsPrerequisites(IEnumerable<CodeInstruction> instr)
        {
            var lines = instr.ToList();
            int count = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].LoadsConstant(20))// || lines[i].LoadsConstant(10))
                {
                    Helper.PrintDebug("Patched at " + i);
                    lines[i] = CodeInstruction.Call(typeof(Patch_UnlockClassLevels), nameof(GetMaxLevel));
                    lines.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));

                    count++;
                }
            }

            if (count != 1)
                Helper.PrintDebug("unexcepted count: " + count);

            return lines;
        }
        public static int GetMaxLevel(BlueprintCharacterClass instance)
        {
            return 40; // instance.Progression.LevelEntries.Length;
        }

        //[HarmonyPatch(typeof(BlueprintStatProgression), nameof(BlueprintStatProgression.HasBonusForLevel))]
        //[HarmonyPostfix]
        public static void HasBonusForLevel(ref bool __result)
        {
            __result = true;
        }

        //[HarmonyPatch(typeof(BlueprintStatProgression), nameof(BlueprintStatProgression.GetBonus))]
        //[HarmonyPrefix]
        public static bool GetStatBonus(int level, BlueprintStatProgression __instance, ref int __result)
        {
            level = level.MinMax(0, __instance.Bonuses.Length - 1);
            __result = __instance.Bonuses[level];
            return false;
        }

        [HarmonyPatch(typeof(Spellbook), nameof(Spellbook.BaseLevel), MethodType.Getter)]
        [HarmonyPriority(410)]
        [HarmonyPrefix]
        public static bool GetSpellLevel(Spellbook __instance, ref int __result)
        {
            __result = __instance.m_BaseLevelInternal + __instance.Blueprint.CasterLevelModifier;
            if (__result < 0)
                __result = 0;
            return false;
        }

    }
}
