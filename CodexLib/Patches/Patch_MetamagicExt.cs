using HarmonyLib;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._VM.ServiceWindows.Spellbook.Metamagic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib.Patches
{
    /// <summary>
    /// Patch to extend metamagic.
    /// </summary>
    [HarmonyPatch]
    public class Patch_MetamagicExt
    {
        private static Metamagic[] _buffer;

        //[HarmonyPatch(typeof(RuleApplyMetamagic), nameof(RuleApplyMetamagic.OnTrigger))] // this is obsolete with MetamagicReduceCostParametrized
        //[HarmonyPatch(typeof(RuleCollectMetamagic), nameof(RuleCollectMetamagic.AddMetamagic))] // nothing to do

        /// <summary>
        /// Forces Metamagic list to iterate through all bits, instead of just the defined Enums.<br/>
        /// This will fix all metamagic names for all mods, as long as <see cref="UIUtilityTexts.GetMetamagicName"/> returns their text.
        /// </summary>
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerTool(instructions, generator, original);
            data.ReplaceAllCalls(typeof(Enum), nameof(Enum.GetValues), patch);
            return data;

            //call class [mscorlib]System.Array [mscorlib]System.Enum::GetValues(class [mscorlib]System.Type)
            Metamagic[] patch(Type type)
            {
                if (_buffer == null)
                {
                    _buffer = new Metamagic[62];
                    for (int i = 0; i < 62; i++)
                        _buffer[i] = (Metamagic)(1 << i);
                }
                return _buffer;
            }
        }

        /// <summary>
        /// Forces Metamagic list to iterate through all bits, instead of just the defined Enums.<br/>
        /// This will fix all metamagic names for all mods, as long as <see cref="UIUtilityTexts.GetMetamagicName"/> returns their text.
        /// </summary>
        [HarmonyPatch(typeof(UIUtilityTexts), nameof(UIUtilityTexts.GetMetamagicList))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler1Alt(IEnumerable<CodeInstruction> instructions)
        {
            // this replaces the call to Enum.GetValues with a call to 'patch'
            var org = AccessTools.Method(typeof(Enum), nameof(Enum.GetValues));
            foreach (var line in instructions)
            {
                if (line.Calls(org))
                    line.operand = ((Delegate)patch).Method;
                yield return line;
            }

            Metamagic[] patch(Type type)
            {
                if (_buffer == null)
                {
                    _buffer = new Metamagic[62];
                    for (int i = 0; i < 62; i++)
                        _buffer[i] = (Metamagic)(1 << i);
                }
                return _buffer;
            }
        }

        [HarmonyPatch(typeof(MetamagicHelper), nameof(MetamagicHelper.DefaultCost))]
        [HarmonyPrefix]
        public static bool Prefix2(Metamagic metamagic, ref int __result)
        {
            switch (metamagic)
            {
                case Const.Dazing:
                    __result = 3;
                    return false;
                default:
                    return true;
            }
        }

        [HarmonyPatch(typeof(MetamagicHelper), nameof(MetamagicHelper.SpellIcon))]
        [HarmonyPrefix]
        public static bool Prefix3(Metamagic metamagic, ref Sprite __result)
        {
            switch (metamagic)
            {
                case Const.Dazing:
                    __result = UIRoot.Instance.SpellBookColors.MetamagicEmpower; // TODO make new icon
                    return false;
                default:
                    return true;
            }
        }

        [HarmonyPatch(typeof(SpellbookMetamagicSelectorVM), nameof(SpellbookMetamagicSelectorVM.GetCost))]
        [HarmonyPostfix]
        public static void Postfix4(Metamagic metamagic, SpellbookMetamagicSelectorVM __instance, ref int __result) // TODO remove test logic
        {
            //Helper.PrintDebug($"SpellbookMetamagicSelectorVM meta={metamagic}");
            var unit = __instance.m_Unit.Value.Unit;
            foreach (var fact in unit.Facts.m_Facts)
            {
                foreach (var comp in fact.BlueprintComponents)
                {
                    if (comp is MetamagicReduceCostParametrized reduction && (reduction.Metamagic & metamagic) != 0)
                        __result -= reduction.Reduction;
                }
            }
        }
    }
}
