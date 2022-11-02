using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Craft;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.Mechanics.Prerequisites;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.PubSubSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI;
using Kingmaker.UI.Log;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.MVVM._VM.Tooltip.Templates;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Utility;
using Kingmaker.View.Equipment;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityModManagerNet;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using static Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.Controllers.Dialog;

namespace CodexLib
{
    /// <summary>
    /// Extensions for Blueprint handling and other conveniences.
    /// </summary>
    /// <remarks>
    /// BlueprintComponent has a field OwnerBlueprint. When components are shared between blueprints, these may cause weird bugs.
    /// </remarks>
    public static class Helper
    {
        #region Other

        public static int MinMax(this int number, int min, int max)
        {
            return min > number ? min : max < number ? max : number;
        }

        public static void AttackRollPrint(this RuleAttackRoll attackRoll)
        {
            //if (attackRoll != null)
            //{
            //    attackRoll.SuspendCombatLog = false;
            //    if (!attackRoll.Initiator.IsInGame || !attackRoll.Target.IsInGame || attackRoll.AutoHit)
            //    {
            //        return;
            //    }
            //    using (GameLogContext.Scope)
            //    {
            //        CombatLogMessage combatLogMessage = attackRoll.AttackLogEntry ?? ReportLogMessageBuilderAbstract.Strings.Attack.GetData(attackRoll);
            //        CombatLogMessage combatLogMessage2 = attackRoll.ParryLogEntry ?? ReportLogMessageBuilderAbstract.Strings.Attack.GetParryData(attackRoll);
            //        if (combatLogMessage2 != null)
            //        {
            //            //this.ReportCombatLogManager.ManageCombatMessageData(combatLogMessage2, attackRoll.Initiator, attackRoll.Target);
            //        }
            //        if (combatLogMessage != null && attackRoll.Result != AttackResult.Parried)
            //        {
            //            //this.ReportCombatLogManager.ManageCombatMessageData(combatLogMessage, attackRoll.Initiator, attackRoll.Target);
            //        }
            //    }
            //}
            throw new NotImplementedException();
        }

        public static string Clipboard
        {
            get => GUIUtility.systemCopyBuffer;
            set => GUIUtility.systemCopyBuffer = value;
        }

        //https://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static bool HasInterface(this Type type, Type @interface)
        {
            return type != @interface && @interface.IsAssignableFrom(type);
        }

        private static void GetTTT()
        {
            //namespace TabletopTweaks.Core
            //static Main.TTTContext
            //TTTContext.Fixes.MythicFeats.IsDisabled("ExpandedArsenal")
        }

        #endregion

        #region Patching/Harmony

        public const BindingFlags BindingAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        public const BindingFlags BindingInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        public const BindingFlags BindingStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        public static List<MethodInfo> Patch(Type patch)
        {
            try
            {
                Print("Patching " + patch.Name);
                var processor = Scope.Harmony.CreateClassProcessor(patch);
                return processor.Patch();
            }
            catch (Exception e) { PrintException(e); }
            return null;
        }

        /// <summary>
        /// Prints possible patching conflicts.
        /// </summary>
        public static void Check(this PatchClassProcessor processor)
        {
            var patches = GetConflictPatches(processor);
            if (patches != null && patches.Count > 0)
                PrintDebug("warning: potential conflict\n\t" + patches.Join(s => $"{s.owner}, {s.PatchMethod?.Name}", "\n\t"));
        }

        //private static void PatchManual(this Harmony harmony, Type patch)
        //{
        //    /// <summary>Needs ManualPatch attribute.</summary>
        //    /// Leftover.
        //    Print("ManualPatch " + patch.Name);
        //    if (patch.GetCustomAttributes(false).FirstOrDefault(f => f is ManualPatchAttribute) is not ManualPatchAttribute manual)
        //        throw new ArgumentException("Type must have ManualPatchAttribute");
        //    var prefix = patch.GetMethod("Prefix");
        //    var postfix = patch.GetMethod("Postfix");
        //    var attr = new HarmonyPatch(manual.declaringType, manual.methodName, manual.methodType);
        //    harmony.Patch(
        //        original: GetOriginalMethod(attr.info),
        //        prefix: prefix != null ? new HarmonyMethod(prefix) : null,
        //        postfix: postfix != null ? new HarmonyMethod(postfix) : null);
        //    patch.GetField("Patched", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, true);
        //}

        public static void Unpatch(Harmony harmony, Type patch, HarmonyPatchType patchType)
        {
            Print("Unpatch " + patch);
            if (patch.GetCustomAttributes(false).FirstOrDefault(f => f is HarmonyPatch) is not HarmonyPatch attr)
                return;

            MethodBase orignal = attr.info.GetOriginalMethod();
            harmony.Unpatch(orignal, patchType, harmony.Id);
        }

        /// <summary>
        /// Only works if all harmony attributes are on the class. Does not support bulk patches.
        /// </summary>
        public static bool IsPatched(Type patch)
        {
            try
            {
                if (patch.GetCustomAttributes(false).FirstOrDefault(f => f is HarmonyPatch) is not HarmonyPatch attr)
                    throw new ArgumentException("Type must have HarmonyPatch attribute");

                MethodBase orignal = attr.info.GetOriginalMethod();
                if (orignal == null)
                    throw new Exception($"GetOriginalMethod returned null {attr.info}");
                var info = Harmony.GetPatchInfo(orignal);
                return info != null && (info.Prefixes.Any() || info.Postfixes.Any() || info.Transpilers.Any());
            }
            catch (Exception e) { PrintException(e); }
            return true;
        }

        public static List<Patch> GetConflictPatches(PatchClassProcessor processor)
        {
            try
            {
                var harmony = Scope.Harmony;
                var list = new List<Patch>();
                foreach (var patch in processor.patchMethods)
                {
                    var orignal = patch.info.GetOriginalMethod();
                    if (orignal == null)
                        throw new Exception($"GetOriginalMethod returned null {patch.info}");

                    // if unpatched, no conflict
                    var info = Harmony.GetPatchInfo(orignal);
                    if (info == null)
                        continue;

                    // if foreign transpilers, warn conflict
                    list.AddRange(info.Transpilers.Where(a => a.owner != harmony.Id));

                    // if foreign prefixes with return type and identical priority as own prefix, warn conflict
                    var prio = info.Prefixes.Where(w => w.owner == harmony.Id).Select(s => s.priority);
                    list.AddRange(info.Prefixes.Where(w => w.owner != harmony.Id && w.PatchMethod.ReturnType != typeof(void) && prio.Contains(w.priority)));
                }
                return list;
            }
            catch (Exception e) { PrintException(e); }
            return null;
        }

        public static MethodBase GetOriginalMethod(this HarmonyMethod attr)
        {
            try
            {
                switch (attr.methodType)
                {
                    case MethodType.Normal:
                        if (attr.methodName is null)
                            return null;
                        return AccessTools.DeclaredMethod(attr.declaringType, attr.methodName, attr.argumentTypes);

                    case MethodType.Getter:
                        if (attr.methodName is null)
                            return null;
                        return AccessTools.DeclaredProperty(attr.declaringType, attr.methodName).GetGetMethod(true);

                    case MethodType.Setter:
                        if (attr.methodName is null)
                            return null;
                        return AccessTools.DeclaredProperty(attr.declaringType, attr.methodName).GetSetMethod(true);

                    case MethodType.Constructor:
                        return AccessTools.DeclaredConstructor(attr.declaringType, attr.argumentTypes);

                    case MethodType.StaticConstructor:
                        return AccessTools.GetDeclaredConstructors(attr.declaringType)
                            .Where(c => c.IsStatic)
                            .FirstOrDefault();
                }
            }
            catch (AmbiguousMatchException ex)
            {
                throw new Exception("GetOriginalMethod " + ex.ToString());
            }

            return null;
        }

        public static bool Calls(this CodeInstruction code, MemberInfo member)
        {
            if (member is MethodInfo mi)
                return (code.opcode == OpCodes.Call || code.opcode == OpCodes.Callvirt) && object.Equals(code.operand, mi);
            if (member is FieldInfo fi)
                return code.opcode == OpCodes.Ldfld && object.Equals(code.operand, fi);

            throw new ArgumentException($"could not find anything for type={member.GetType()} name={member.Name}");
        }

        public static Type GetLocType(this CodeInstruction code, IList<LocalVariableInfo> locals)
        {
            if (code.operand is LocalBuilder lb)
                return lb.LocalType;
            if (code.operand is Type type)
                return type;

            if (locals == null)
                return null;

            var op = code.opcode;
            if (op == OpCodes.Ldloc_0 || op == OpCodes.Stloc_0)
                return locals[0].LocalType;
            if (op == OpCodes.Ldloc_1 || op == OpCodes.Stloc_1)
                return locals[1].LocalType;
            if (op == OpCodes.Ldloc_2 || op == OpCodes.Stloc_2)
                return locals[2].LocalType;
            if (op == OpCodes.Ldloc_3 || op == OpCodes.Stloc_3)
                return locals[3].LocalType;

            return null;
        }

        public static MemberInfo GetMemberInfo(Type type, string name)
        {
            var mi = type.GetMethod(name, Helper.BindingAll) ?? type.GetProperty(name, Helper.BindingAll).GetMethod;
            if (mi != null)
                return mi;

            var fi = type.GetField(name, Helper.BindingAll);
            if (fi != null)
                return fi;

            throw new ArgumentException($"could not find anything for type={type} name={name}");
        }

        public static IEnumerable<CodeInstruction> _TestTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            foreach (var line in instructions)
            {
                Console.WriteLine($"{line.opcode} : {line.operand?.GetType()}");
            }
            return instructions;
        }

        public static void ReplaceCall(this CodeInstruction code, Type type, string name, Type[] parameters = null, Type[] generics = null)
        {
            var repl = CodeInstruction.Call(type, name, parameters, generics);
            code.opcode = repl.opcode;
            code.operand = repl.operand;
        }

        public static void ReplaceCall(this CodeInstruction code, Delegate newFunc)
        {
            code.opcode = OpCodes.Call;
            code.operand = newFunc.Method;
        }

        public static IEnumerable<CodeInstruction> ReplaceCall(this IEnumerable<CodeInstruction> instr, Type type, string name, Delegate newFunc, Type[] parameters = null, Type[] generics = null)
        {
            var original = AccessTools.Method(type, name, parameters, generics);

            foreach (var line in instr)
            {
                if (line.Calls(original))
                    line.ReplaceCall(newFunc);

                yield return line;
            }
        }

        /// <summary>
        /// Inserts a check and return block. Method returns, if func returns false (like HarmonyPrefix). Original method must return void!
        /// </summary>
        /// <remarks>
        /// End result:
        /// <code>
        /// if (!func(__instance))
        ///     return;
        /// </code>
        /// </remarks>
        public static void AddCondition(this List<CodeInstruction> code, ref int index, ILFunction func, ILGenerator generator)
        {
            PrintDebug($"Transpiler AddCondition @{index}");

            Label label1 = generator.DefineLabel();

            code.Insert(index++, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(index++, new CodeInstruction(OpCodes.Call, func.GetMethodInfo()));
            code.Insert(index++, new CodeInstruction(OpCodes.Brtrue, label1));
            code.Insert(index++, new CodeInstruction(OpCodes.Ret));
            code[index++].labels.Add(label1);
        }

        /// <summary>
        /// Inserts a check and return block. Method returns, if func returns false (like HarmonyPrefix). Original method must return a value. Use unbox, if it's a value type.
        /// </summary>
        /// <param name="unbox">Type of original return type.</param>
        /// <remarks>
        /// End result:
        /// <code>
        /// object __result;
        /// if (!func(__instance, ref __result))
        ///     return __result;
        /// </code>
        /// </remarks>
        public static void AddCondition(this List<CodeInstruction> code, ref int index, ILFunctionRet func, ILGenerator generator, Type unbox = null)
        {
            PrintDebug($"Transpiler AddCondition @{index}");

            var local1 = generator.DeclareLocal(typeof(object));
            Label label1 = generator.DefineLabel();

            code.Insert(index++, new CodeInstruction(OpCodes.Ldarg_0));
            code.Insert(index++, new CodeInstruction(OpCodes.Ldloca, local1));
            code.Insert(index++, new CodeInstruction(OpCodes.Call, func.GetMethodInfo()));
            code.Insert(index++, new CodeInstruction(OpCodes.Brtrue, label1));
            code.Insert(index++, new CodeInstruction(OpCodes.Ldloc, local1));
            if (unbox != null) code.Insert(index++, new CodeInstruction(OpCodes.Unbox_Any, unbox));
            code.Insert(index++, new CodeInstruction(OpCodes.Ret));
            code[index++].labels.Add(label1);
        }

        /// <summary>
        /// Subject to change! Expect signature changes. May not work with ref/out keywords. May not work if return value isn't void.
        /// </summary>
        public static void RemoveMethods(this List<CodeInstruction> code, Type type, string name, Type[] parameters = null, Type[] generics = null)
        {
            var mi = AccessTools.Method(type, name, parameters, generics);
            int count = mi.GetParameters().Length;
            if (!mi.IsStatic) count++;

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].Calls(mi))
                {
                    PrintDebug($"Transpiler RemoveMethods {mi.Name} @{i}");

                    code[i].opcode = OpCodes.Nop;
                    code[i++].operand = null;

                    for (int j = 0; j < count; j++)
                        code.Insert(i++, new CodeInstruction(OpCodes.Pop));

                    if (mi.ReturnType == typeof(void))
                    {
                    }
                    else if (mi.ReturnType.IsValueType)
                    {
                        code.Insert(i++, new CodeInstruction(OpCodes.Ldc_I4_0));
                        if (mi.ReturnType == typeof(long))
                            code.Insert(i++, new CodeInstruction(OpCodes.Conv_I8));
                    }
                    else
                    {
                        code.Insert(i++, new CodeInstruction(OpCodes.Ldnull));
                    }
                }
            }

            /*
             * Notes:
             * - out and ref behave identical
             * - an out/ref caller loads an address instead of the value with 'ldloca.s V_X' where X depends on the address
             * - if a method has a return value, it will load it on the stack; unused return values are discarded with 'pop'
             * - 'box : Type' boxes the valuetype on the stack
             * - 'box.any : Type' unboxes the valuetype and puts in on the stack
             */
        }

        public static void NextJumpAlways(this List<CodeInstruction> code, ref int index)
        {
            for (; index < code.Count; index++)
            {
                var line = code[index];

                if (line.opcode.FlowControl != FlowControl.Cond_Branch)
                    continue;

                PrintDebug($"Transpiler NextJumpAlways {line.opcode} @{index}");

                if (line.opcode.OperandType == OperandType.InlineBrTarget)
                    code.Insert(++index, new CodeInstruction(OpCodes.Br, line.operand));

                else if (line.opcode.OperandType == OperandType.ShortInlineBrTarget)
                    code.Insert(++index, new CodeInstruction(OpCodes.Br_S, line.operand));

                else
                    throw new Exception("Did not expect this OpCode");

                return;
            }
        }

        public static void NextJumpNever(this List<CodeInstruction> code, ref int index)
        {
            for (; index < code.Count; index++)
            {
                var line = code[index];

                if (line.opcode.FlowControl != FlowControl.Cond_Branch)
                    continue;

                PrintDebug($"Transpiler NextJumpNever {line.opcode} @{index}");

                if (line.opcode.StackBehaviourPush != StackBehaviour.Push0)
                    throw new Exception("Cond_Branch should not push onto stack");

                var num = line.opcode.StackBehaviourPop.GetStackChange();
                if (num == 0)
                {
                    line.opcode = OpCodes.Nop;
                    line.operand = null;
                }
                else if (num == -1)
                {
                    line.opcode = OpCodes.Pop;
                    line.operand = null;
                }
                else if (num == -2)
                {
                    line.opcode = OpCodes.Pop;
                    line.operand = null;
                    code.Insert(index++, new CodeInstruction(OpCodes.Pop));
                }
                else
                    throw new Exception("Cond_Branch should not pop more than 2");

                return;
            }
        }

        public static int GetStackChange(this StackBehaviour stack)
        {
            switch (stack)
            {
                case StackBehaviour.Pop0:
                case StackBehaviour.Push0:
                    return 0;
                case StackBehaviour.Pop1:
                case StackBehaviour.Popi:
                case StackBehaviour.Popref:
                case StackBehaviour.Varpop:
                    return -1;
                case StackBehaviour.Push1:
                case StackBehaviour.Pushi:
                case StackBehaviour.Pushi8:
                case StackBehaviour.Pushr4:
                case StackBehaviour.Pushr8:
                case StackBehaviour.Pushref:
                case StackBehaviour.Varpush:
                    return 1;
                case StackBehaviour.Pop1_pop1:
                case StackBehaviour.Popi_pop1:
                case StackBehaviour.Popi_popi:
                case StackBehaviour.Popi_popi8:
                case StackBehaviour.Popi_popr4:
                case StackBehaviour.Popi_popr8:
                case StackBehaviour.Popref_pop1:
                case StackBehaviour.Popref_popi:
                    return -2;
                case StackBehaviour.Push1_push1:
                    return 2;
                case StackBehaviour.Popref_popi_pop1:
                case StackBehaviour.Popref_popi_popi:
                case StackBehaviour.Popref_popi_popi8:
                case StackBehaviour.Popref_popi_popr4:
                case StackBehaviour.Popref_popi_popr8:
                case StackBehaviour.Popref_popi_popref:
                case StackBehaviour.Popi_popi_popi:
                    return -3;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool FakeAlwaysFalse(object obj) => false;
        public static bool FakeAlwaysTrue(object obj) => true;

        #endregion

        #region Arrays

        public static T[] ObjToArray<T>(this T obj)
        {
            if (obj == null) return null;
            return new T[] { obj };
        }

        public static T[] ToArray<T>(params T[] objs)
        {
            return objs;
        }

        public static List<T> AsList<T>(this IEnumerable<T> values)
        {
            return values as List<T> ?? values.ToList();
        }

        /// <summary>Appends objects on array.</summary>
        public static T[] Append<T>(this T[] orig, params T[] objs)
        {
            if (orig == null) orig = new T[0];
            if (objs == null) objs = new T[0];

            int i, j;
            T[] result = new T[orig.Length + objs.Length];
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            for (j = 0; i < result.Length; i++)
                result[i] = objs[j++];
            return result;
        }

        public static List<T> Append<T>(this List<T> orig, List<T> objs)
        {
            var result = new List<T>(orig);
            result.AddRange(objs);
            return result;
        }

        /// <summary>Appends objects on array and overwrites the original.</summary>
        public static void AppendAndReplace<T>(ref T[] orig, params T[] objs)
        {
            if (orig == null) orig = new T[0];
            if (objs == null) objs = new T[0];

            int i, j;
            T[] result = new T[orig.Length + objs.Length];
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            for (j = 0; i < result.Length; i++)
                result[i] = objs[j++];
            orig = result;
        }

        public static void AppendAndReplace<T>(ref T[] orig, List<T> objs)
        {
            if (orig == null) orig = new T[0];

            T[] result = new T[orig.Length + objs.Count];
            int i;
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            foreach (var obj in objs)
                result[i++] = obj;
            orig = result;
        }

        public static void AppendAndReplace<T>(ref T[] orig, IEnumerable<T> objs)
        {
            if (orig == null) orig = new T[0];

            T[] result = new T[orig.Length + objs.Count()];
            int i;
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            foreach (var obj in objs)
                result[i++] = obj;
            orig = result;
        }

        public static void InsertAt<T>(ref T[] orig, T obj, int index = -1)
        {
            if (orig == null) orig = new T[0];
            if (index < 0 || index > orig.Length) index = orig.Length;

            T[] result = new T[orig.Length + 1];
            for (int i = 0, j = 0; i < result.Length; i++)
            {
                if (i == index)
                    result[i] = obj;
                else
                    result[i] = orig[j++];
            }
            orig = result;
        }

        public static void RemoveAt<T>(ref T[] orig, int index)
        {
            if (orig == null) orig = new T[0];
            if (index < 0 || index >= orig.Length) return;

            T[] result = new T[orig.Length - 1];
            for (int i = 0, j = 0; i < result.Length; i++)
            {
                if (i != index)
                    result[i] = orig[j++];
            }
            orig = result;
        }

        public static void RemoveGet<T>(this List<T> list, List<T> result, Func<T, bool> predicate)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate(list[i]))
                {
                    result.Add(list[i]);
                    list.RemoveAt(i);
                    return;
                }
            }
        }

        public static void RemoveGet<T1, T2>(this List<T1> list, List<T2> result, Func<T1, bool> predicate, Func<T1, T2> select)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate(list[i]))
                {
                    result.Add(select(list[i]));
                    list.RemoveAt(i);
                    return;
                }
            }
        }

        //public static void Deconstruct<K, V>(this KeyValuePair<K, V> pair, out K key, out V val) // using Kingmaker.Utility;
        //{
        //    key = pair.Key;
        //    val = pair.Value;
        //}

        /// <summary>
        /// Get dictionary by key and create new value with standard constructor, if it did not exist.
        /// </summary>
        /// <returns>true if new value was created</returns>
        public static bool Ensure<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, out TValue value) where TValue : new()
        {
            if (dic.TryGetValue(key, out value))
                return false;
            dic[key] = value = new();
            return true;
        }

        public static IEnumerable<TResult> SelectNotNull<T, TResult>(this IEnumerable<T> array, Func<T, TResult> func) where TResult : class
        {
            if (array is null)
                yield break;

            foreach (var result in array.Select(func))
                if (result is not null)
                    yield return result;
        }

        public static SpellSchool Merge(this IEnumerable<SpellSchool> array)
        {
            if (array == null)
                return default;

            SpellSchool result = default;
            foreach (var value in array)
                result |= value;
            return result;
        }

        #endregion

        #region Log

        /// <summary>Only prints message, if compiled on DEBUG.</summary>
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void PrintDebug(string msg)
        {
            Scope.Logger.Log(msg);
        }

        internal static void Print(string msg)
        {
            Scope.Logger.Log(msg);
        }

        internal static void PrintError(string msg)
        {
            Scope.Logger.Log("[Exception/Error] " + msg);
        }

        private static int _exceptionCount;
        internal static void PrintException(Exception ex)
        {
            if (_exceptionCount > 1000)
                return;

            Scope.Logger.LogException(ex);

            _exceptionCount++;
            if (_exceptionCount > 1000)
                Scope.Logger.Log("-- too many exceptions, future exceptions are suppressed");
        }

        public static void ShowMessageBox(string messageText, Action onYes = null, int waitTime = 0, string yesLabel = null, string noLabel = null)
        {
            EventBus.RaiseEvent<IMessageModalUIHandler>(w =>
            {
                w.HandleOpen(messageText: messageText,
                    modalType: onYes == null ? MessageModalBase.ModalType.Message : MessageModalBase.ModalType.Dialog,
                    onClose: onYes == null ? null : a => { if (a == MessageModalBase.ButtonType.Yes) try { onYes(); } catch { } },
                    onLinkInvoke: null,
                    yesLabel: yesLabel,
                    noLabel: noLabel,
                    onTextResult: null,
                    inputText: null,
                    inputPlaceholder: null,
                    waitTime: waitTime,
                    maxInputTextLength: uint.MaxValue,
                    items: null);
            }, true);
        }

        public static void ShowInputBox(string messageText, Action<string> onOK = null, Action onCancel = null, int waitTime = 0, string yesLabel = null, string noLabel = null)
        {
            EventBus.RaiseEvent<IMessageModalUIHandler>(w =>
            {
                w.HandleOpen(messageText: messageText,
                    modalType: MessageModalBase.ModalType.TextField,
                    onClose: onCancel == null ? null : a => { if (a == MessageModalBase.ButtonType.No) try { onCancel(); } catch { } },
                    onLinkInvoke: null,
                    yesLabel: yesLabel,
                    noLabel: noLabel,
                    onTextResult: onOK,
                    inputText: null,
                    inputPlaceholder: null,
                    waitTime: waitTime,
                    maxInputTextLength: uint.MaxValue,
                    items: null);
            }, true);
        }

        public static void PrintCombatLog(string text, string header = null, string tooltip = null)
        {
            //var log = LogThreadController.Instance.m_Logs[LogChannelType.AnyCombat];

            var color = GameLogStrings.Instance.DefaultColor;
            var icon = PrefixIcon.RightArrow;
            var tooltipmessage = new TooltipTemplateCombatLogMessage(header ?? text, tooltip);

            var message = new CombatLogMessage(text, color, icon, tooltipmessage, true);
            LogThreadService.Instance.HitDiceRestrictionLogThread.AddMessage(message);
        }

        public static void PrintNotification(string text)
        {
            EventBus.RaiseEvent<IWarningNotificationUIHandler>(h => h.HandleWarning(text, false));
        }

        internal static Exception SetStackTrace(this Exception target) => _SetStackTrace(target, new StackTrace(true));

        /// <summary>Source: https://stackoverflow.com/a/63685720</summary>
        private static readonly Func<Exception, StackTrace, Exception> _SetStackTrace = new Func<Func<Exception, StackTrace, Exception>>(() =>
        {
            // https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees/
            // Exception Set(Exception ex, StackTrace stack)
            // {
            //      ex._stackTraceString = stack.ToString(TraceFormat.Normal);
            // }

            ParameterExpression target = Expression.Parameter(typeof(Exception));
            ParameterExpression stack = Expression.Parameter(typeof(StackTrace));
            Type traceFormatType = typeof(StackTrace).GetNestedType("TraceFormat", BindingFlags.NonPublic);
            MethodInfo toString = typeof(StackTrace).GetMethod("ToString", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { traceFormatType }, null);
            object normalTraceFormat = Enum.GetValues(traceFormatType).GetValue(0); // Enum.ToObject(traceFormatType, 0);
            FieldInfo stackTraceStringField = typeof(Exception).GetField("_stackTraceString", BindingFlags.NonPublic | BindingFlags.Instance);

            MethodCallExpression stackTraceString = Expression.Call(stack, toString, Expression.Constant(normalTraceFormat, traceFormatType));
            BinaryExpression assign = Expression.Assign(Expression.Field(target, stackTraceStringField), stackTraceString);
            return Expression.Lambda<Func<Exception, StackTrace, Exception>>(Expression.Block(assign, target), target, stack).Compile();
        })();

        //private static Exception SetStackTrace2(this Exception target) => _SetStackTrace2(target);
        //private static readonly Func<Exception, Exception> _SetStackTrace2 = new Func<Func<Exception, Exception>>(() =>
        //{
        //    // Exception Set(Exception ex)
        //    // {
        //    //      ex._stackTraceString = new StackTrace(true).ToString(TraceFormat.Normal);
        //    // }
        //    //
        //    ParameterExpression target = Expression.Parameter(typeof(Exception));
        //    Type traceFormatType = typeof(StackTrace).GetNestedType("TraceFormat", BindingFlags.NonPublic);
        //    MethodInfo toString = typeof(StackTrace).GetMethod("ToString", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { traceFormatType }, null);
        //    object normalTraceFormat = Enum.GetValues(traceFormatType).GetValue(0); // Enum.ToObject(traceFormatType, 0);
        //    FieldInfo stackTraceStringField = typeof(Exception).GetField("_stackTraceString", BindingFlags.NonPublic | BindingFlags.Instance);
        //    ConstructorInfo newStackInfo = typeof(StackTrace).GetConstructor(new Type[] { typeof(bool) });
        //    //
        //    NewExpression newStack = Expression.New(newStackInfo, Expression.Constant(false, typeof(bool)));
        //    MethodCallExpression stackTraceString = Expression.Call(newStack, toString, Expression.Constant(normalTraceFormat, traceFormatType));
        //    BinaryExpression assign = Expression.Assign(Expression.Field(target, stackTraceStringField), stackTraceString);
        //    return Expression.Lambda<Func<Exception, Exception>>(Expression.Block(assign, target), target).Compile();
        //})();

        #endregion

        #region Keyboard

        public static void KeyBind()
        {
            //Game.Instance.Keyboard.m_Bindings
        }

        #endregion

        #region JsonSerializer

        public static JsonSerializerSettings _jsetting = new()
        {
            Converters = DefaultJsonSettings.DefaultSettings.Converters,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = DefaultValueHandling.Include,
            TypeNameHandling = TypeNameHandling.Auto,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        };

        public static string Serialize(this object value, bool indent = true, bool type = true, string path = null, bool append = false)
        {
            _jsetting.Formatting = indent ? Formatting.Indented : Formatting.None;
            _jsetting.TypeNameHandling = type ? TypeNameHandling.Auto : TypeNameHandling.None;
            string result = JsonConvert.SerializeObject(value, _jsetting);

            if (path != null)
            {
                //path = Path.Combine(Main.ModPath, path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using var sw = new StreamWriter(path, append);
                sw.WriteLine(result);
                sw.Close();
            }

            return result;
        }

        public static string Serialize<T>(this T value, bool indent = true, bool type = true, string path = null, bool append = false)
        {
            _jsetting.Formatting = indent ? Formatting.Indented : Formatting.None;
            _jsetting.TypeNameHandling = type ? TypeNameHandling.Auto : TypeNameHandling.None;
            string result = JsonConvert.SerializeObject(value, typeof(T), _jsetting);

            if (path != null)
            {
                //path = Path.Combine(Main.ModPath, path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using var sw = new StreamWriter(path, append);
                sw.WriteLine(result);
                sw.Close();
            }

            return result;
        }

        public static object Deserialize(string path = null, string value = null)
        {
            if (path != null)
            {
                //path = Path.Combine(Main.ModPath, path);
                using var sr = new StreamReader(path);
                value = sr.ReadToEnd();
                sr.Close();
            }

            if (value != null)
                return JsonConvert.DeserializeObject(value, _jsetting);
            return null;
        }

        public static T Deserialize<T>(string path = null, string value = null)
        {
            if (path != null)
            {
                //path = Path.Combine(Main.ModPath, path);
                using var sr = new StreamReader(path);
                value = sr.ReadToEnd();
                sr.Close();
            }

            if (value != null)
                return (T)JsonConvert.DeserializeObject(value, typeof(T), _jsetting);
            return default;
        }

        public static string TrySerialize<T>(this T value, bool indent = true, bool type = true, string path = null, bool append = false)
        {
            try
            {
                return Serialize<T>(value: value, indent: indent, type: type, path: path, append: append);
            }
            catch (Exception e) { PrintException(e); }
            return null;
        }

        public static bool TryDeserialize<T>(out T result, string path = null, string value = null)
        {
            try
            {
                result = Deserialize<T>(path, value);
                return true;
            }
            catch (Exception e) { PrintException(e); }
            result = default;
            return false;
        }

        public static void TryPrintFile(string path, string content, bool append = true)
        {
            try
            {
                //path = Path.Combine(Main.ModPath, path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using var sw = new StreamWriter(path, append);
                sw.WriteLine(content);
            }
            catch (Exception e) { PrintException(e); }
        }

        public static void TryPrintBytes(string path, byte[] data)
        {
            try
            {
                //path = Path.Combine(Main.ModPath, path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, data);
            }
            catch (Exception e) { PrintException(e); }
        }

        public static byte[] TryReadBytes(string path)
        {
            try
            {
                //path = Path.Combine(Main.ModPath, path);
                return File.ReadAllBytes(path);
            }
            catch (Exception e) { PrintException(e); }
            return new byte[0];
        }

        public static void TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception e) { PrintException(e); }
        }

        #endregion

        #region GUID

        [Obsolete]
        public static bool Allow_Guid_Generation = false;
        private static string _overwriteGuid;
        //private static List<string> _guidloaded = new();
        //private static Dictionary<string, string> _guids = new();

        private static Dictionary<string, Dictionary<string, string>> _mappedGuids = new();

        private static Dictionary<string, string> GetGuidMap()
        {
            string modPath = Scope.ModPath;

            if (_mappedGuids.Ensure(modPath, out var map))
            {
                load(Path.Combine(modPath, "blueprints.txt"));
                load(Path.Combine(modPath, "blueprints_dynamic.txt"));
            }
            return map;

            void load(string path)
            {
                if (File.Exists(path))
                {
                    foreach (string line in File.ReadAllLines(path))
                    {
                        string[] items = line.Split('\t');
                        if (items.Length >= 2)
                            map[items[0]] = items[1];
                    }
                }
            }
        }

        public static void ForceOverwriteGuid(string guid)
        {
            _overwriteGuid = guid;
        }

        public static string GetGuid(string key)
        {
            if (_overwriteGuid != null)
            {
                string overwrite = _overwriteGuid;
                _overwriteGuid = null;
                return overwrite;
            }

            var map = GetGuidMap();

            map.TryGetValue(key, out string result);
            if (result != null)
                return result;

            if (!Scope.AllowGuidGeneration)
                throw new Exception("Tried to generate a new GUID while not allowed! " + key);

            Print("Warning: Generating new GUID for " + key);
            map[key] = result = Guid.NewGuid().ToString("N");
            using StreamWriter sw = new(Path.Combine(Scope.ModPath, "blueprints.txt"), append: true);
            sw.WriteLine(key + '\t' + result);

            return result;
        }

        public static string RegGuid(string key, string guid)
        {
            var map = GetGuidMap();

            if (!map.ContainsKey(key))
            {
                Print("Warning: Merged new GUID for " + key);
                map.Add(key, guid);
                using StreamWriter sw = new(Path.Combine(Scope.ModPath, "blueprints_dynamic.txt"), append: true);
                sw.WriteLine(key + '\t' + guid);
            }

            return guid;
        }

        public static void DumpLoadedBlueprints()
        {
            using StreamWriter sw = new(Path.Combine(Scope.ModPath, "dump.txt"), append: true);
            foreach (var (_, cache) in ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints)
            {
                if (cache.Offset == 0 && cache.Blueprint is SimpleBlueprint bp)
                {
                    sw.WriteLine($"{bp.name}\t{bp.AssetGuid}\t{bp.GetType().FullName}");
                }
            }
        }

        public static void SanityCheckGuid()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Strings

        private static LocalizedString _empty = new() { Key = "" };

        public static LocalizedString GetString(string key)
        {
            return new LocalizedString { Key = key };
        }

        public static bool IsEmptyKey(this LocalizedString key)
        {
            if (key == null)
                return true;
            if (key == "")
                return true;

            if (LocalizationManager.CurrentPack.GetText(key.Key, false) == "")
                return true;

            return false;
        }

        private static SHA1 _SHA = SHA1.Create();
        private static StringBuilder _sb1 = new();
        private static Dictionary<string, Dictionary<string, string>> _mappedStrings = new();

        private static Dictionary<string, string> GetStringMap()
        {
            string modPath = Scope.ModPath;

            if (_mappedStrings.Ensure(modPath, out var map) && !Scope.AllowGuidGeneration)
            {
                load(Path.Combine(modPath, LocalizationManager.CurrentPack.Locale.ToString() + ".json"));
            }
            return map;

            void load(string path)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        map = Deserialize<Dictionary<string, string>>(path: path);
                        var pack = LocalizationManager.CurrentPack;
                        foreach (var entry in map)
                            pack.PutString(entry.Key, entry.Value);
                    }
                }
                catch (Exception e) { Print($"Could not read lanaguage file for {LocalizationManager.CurrentPack.Locale}: {e.Message}"); }
            }
        }

        public static LocalizedString CreateString(this string value, string key = null)
        {
            if (value == null || value == "")
                return new LocalizedString { Key = "" };

            if (key == null)
            {
                var sha = _SHA.ComputeHash(Encoding.UTF8.GetBytes(value));
                for (int i = 0; i < sha.Length; i++)
                    _sb1.Append(sha[i].ToString("x2"));
                key = _sb1.ToString();
                _sb1.Clear();
            }

            var map = GetStringMap();
            if (!map.ContainsKey(key))
            {
                map.Add(key, value);
                LocalizationManager.CurrentPack.PutString(key, value);
            }

            return new LocalizedString { Key = key };
        }

        /// <summary>
        /// Overwrite existing LocalizedString with new value.
        /// </summary>
        public static void CreateString(this LocalizedString localizedString, string newString)
        {
            if (localizedString.m_Key != null && localizedString.m_Key != "")
                CreateString(newString, localizedString.m_Key);
            else if (localizedString?.Shared?.String?.m_Key != null && localizedString.Shared.String.m_Key != "")
                CreateString(newString, localizedString?.Shared?.String?.m_Key);
            else
                PrintDebug("Warning: CreateString failed since m_Key is empty");
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void ExportStrings()
        {
            var map = GetStringMap();

            try
            {
                string path = Path.Combine(Scope.ModPath, "enGB.json");
                Serialize(map, path: path);
                PrintDebug("Exported language file to " + path);
            }
            catch (Exception e) { PrintException(e); }
        }

        /// <summary>Returns substring. Always excludes char 'c'. Returns null, if index is out of range or char not found.</summary>
        /// <param name="str">source string</param>
        /// <param name="c">char to search for</param>
        /// <param name="start">start index; negative number search last index instead</param>
        /// <param name="tail">get tail instead of head</param>
        public static string TrySubstring(this string str, char c, int start = 0, bool tail = false)
        {
            try
            {
                if (tail)
                {
                    if (start < 0)
                        return str.Substring(str.LastIndexOf(c) + 1);
                    return str.Substring(str.IndexOf(c, start) + 1);
                }

                if (start < 0)
                    return str.Substring(0, str.LastIndexOf(c));
                return str.Substring(start, str.IndexOf(c, start));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool IsNumber(this char c)
        {
            return c >= 48 && c <= 57;
        }

        public static bool IsUppercase(this char c)
        {
            return c >= 65 && c <= 90;
        }

        public static bool IsLowercase(this char c)
        {
            return c >= 97 && c <= 122;
        }

        public static bool IsNotSpaced(this StringBuilder sb)
        {
            if (sb.Length == 0)
                return false;

            return sb[sb.Length - 1] != ' ';
        }

        public static string Red(this string text, bool condition = true)
        {
            if (condition)
                return $"<color=red>{text}</color>";
            return text;
        }

        public static string Grey(this string text, bool condition = true)
        {
            if (condition)
                return $"<color=grey>{text}</color>";
            return text;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWithO(this string source, string value) => source.StartsWith(value, StringComparison.Ordinal);

        #endregion

        #region Components

        public static IEnumerable<T> GetComponents<T>(this BlueprintScriptableObject blueprint, Func<T, bool> pred) where T : BlueprintComponent
        {
            if (blueprint == null)
                yield break;

            foreach (var comp in blueprint.ComponentsArray)
            {
                if (comp is T t && pred(t))
                    yield return t;
            }
        }

        public static T GetComponent<T>(this AnyRef any) where T : BlueprintComponent
        {
            if (any?.GetBlueprint() is BlueprintScriptableObject bso)
                return bso.GetComponent<T>();
            return null;
        }

        public static T AddComponents<T>(this T obj, params BlueprintComponent[] components) where T : BlueprintScriptableObject
        {
            obj.Components = Append(obj.Components, components);

            for (int i = 0; i < obj.Components.Length; i++)
            {
                var comp = obj.Components[i];
                if (comp.name == null)
                    comp.name = $"${comp.GetType().Name}${obj.AssetGuid}${i}";
                if (comp.OwnerBlueprint == null)
                    comp.OwnerBlueprint = obj;
                else if (comp.OwnerBlueprint != obj)
                    PrintDebug($"Warning: reused BlueprintComponent {comp.name} which is attached to {comp.OwnerBlueprint} instead of {obj}");
            }

            return obj;
        }

        public static T AddComponents<T>(this T obj, params object[] components) where T : BlueprintScriptableObject
        {
            var list = new List<BlueprintComponent>();

            foreach (var comp in components)
            {
                if (comp is BlueprintComponent c1)
                    list.Add(c1);
                else if (comp is IEnumerable<BlueprintComponent> c2)
                    list.AddRange(c2);
            }

            return obj.AddComponents(list.ToArray());
        }

        public static T SetComponents<T>(this T obj, params BlueprintComponent[] components) where T : BlueprintScriptableObject
        {
            for (int i = 0; i < components.Length; i++)
            {
                var comp = components[i];
                comp.name = $"${comp.GetType().Name}${obj.AssetGuid}${i}";
                if (comp.OwnerBlueprint == null)
                    comp.OwnerBlueprint = obj;
                else if (comp.OwnerBlueprint != obj)
                    PrintDebug($"Warning: reused BlueprintComponent {comp.name} which is attached to {comp.OwnerBlueprint} instead of {obj}");
            }

            obj.Components = components;
            return obj;
        }

        public static T ReplaceComponent<T, TOrig, TRep>(this T obj, TOrig original, TRep replacement) where T : BlueprintScriptableObject where TOrig : BlueprintComponent where TRep : BlueprintComponent
        {
            for (int i = 0; i < obj.ComponentsArray.Length; i++)
            {
                if (obj.Components[i] is TOrig)
                {
                    obj.Components[i] = replacement;
                    replacement.name = $"${replacement.GetType().Name}${obj.AssetGuid}${i}";
                    if (replacement.OwnerBlueprint == null)
                        replacement.OwnerBlueprint = obj;
                    else if (replacement.OwnerBlueprint != obj)
                    PrintDebug($"Warning: reused BlueprintComponent {replacement.name} which is attached to {replacement.OwnerBlueprint} instead of {obj}");
                    break;
                }
            }

            return obj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T RemoveComponents<T, TRemove>(T obj) where T : BlueprintScriptableObject where TRemove : BlueprintComponent
        {
            var list = obj.ComponentsArray.ToList();
            list.RemoveAll(r => r is TRemove);
            obj.Components = list.ToArray();
            return obj;
        }
        public static BlueprintAbility RemoveComponents<TRemove>(this BlueprintAbility obj) where TRemove : BlueprintComponent
            => RemoveComponents<BlueprintAbility, TRemove>(obj);
        public static BlueprintFeature RemoveComponents<TRemove>(this BlueprintFeature obj) where TRemove : BlueprintComponent
            => RemoveComponents<BlueprintFeature, TRemove>(obj);
        public static BlueprintBuff RemoveComponents<TRemove>(this BlueprintBuff obj) where TRemove : BlueprintComponent
            => RemoveComponents<BlueprintBuff, TRemove>(obj);

        public static T RemoveComponents<T>(this T obj, Predicate<BlueprintComponent> match) where T : BlueprintScriptableObject
        {
            var list = obj.ComponentsArray.ToList();
            list.RemoveAll(match);
            obj.Components = list.ToArray();
            return obj;
        }

        public static List<BlueprintAbilityReference> GetBaseAndVariants(this List<BlueprintAbilityReference> source, Func<BlueprintAbility, bool> predicate = null)
        {
            var result = new List<BlueprintAbilityReference>();

            for (int i = 0; i < source.Count; i++)
            {
                result.Add(source[i]);

                var variants = source[i].Get()?.GetComponent<AbilityVariants>()?.m_Variants;
                if (variants == null)
                    continue;
                foreach (var variant in variants)
                {
                    if (predicate == null || predicate(variant.Get()))
                        result.Add(variant);
                }
            }

            return result;
        }

        public static List<BlueprintAbilityReference> GetVariants(this List<BlueprintAbilityReference> source, Func<BlueprintAbility, bool> predicate = null)
        {
            var result = new List<BlueprintAbilityReference>();

            for (int i = 0; i < source.Count; i++)
            {
                var variants = source[i].Get()?.GetComponent<AbilityVariants>()?.m_Variants;
                if (variants == null)
                    continue;
                foreach (var variant in variants)
                {
                    if (predicate == null || predicate(variant.Get()))
                        result.Add(variant);
                }
            }

            return result;
        }

        public static bool IsRank(this AbilityData ability, int min = -1, int max = int.MaxValue, bool ifEmpty = true)
        {
            var rank = ability.Blueprint.GetComponent<FeatureRank>();
            if (rank == null)
                return ifEmpty;
            return rank.Rank >= min && rank.Rank <= max;
        }

        public static bool IsRank(this BlueprintAbility ability, int min = -1, int max = int.MaxValue, bool ifEmpty = true)
        {
            var rank = ability.GetComponent<FeatureRank>();
            if (rank == null)
                return ifEmpty;
            return rank.Rank >= min && rank.Rank <= max;
        }


        /// <summary>
        /// Get components that are subscribed for a unit.
        /// </summary>
        public static IEnumerable<BlueprintComponent> GetIInitiator<TEvent>(UnitEntityData unit) where TEvent : RulebookEvent
        {
            RulebookEventBus.InitiatorRulebookSubscribers.Get(unit).m_Listeners.TryGetValue(typeof(TEvent), out var iSubscriber);
            if (iSubscriber == null || iSubscriber is not RulebookSubscribersList<TEvent> subscribers)
                yield break;

            foreach (var subscriber in subscribers.List)
            {
                if (subscriber is EntityFactComponent runtimeComp)
                    yield return runtimeComp.SourceBlueprintComponent;
            }
        }

        /// <summary>
        /// Get components that are subscribed for a unit.
        /// </summary>
        public static IEnumerable<BlueprintComponent> GetITarget<TEvent>(UnitEntityData unit) where TEvent : RulebookEvent
        {
            RulebookEventBus.TargetRulebookSubscribers.Get(unit).m_Listeners.TryGetValue(typeof(TEvent), out var iSubscriber);
            if (iSubscriber == null || iSubscriber is not RulebookSubscribersList<TEvent> subscribers)
                yield break;

            foreach (var subscriber in subscribers.List)
            {
                if (subscriber is EntityFactComponent runtimeComp)
                    yield return runtimeComp.SourceBlueprintComponent;
            }
        }

        /// <summary>
        /// Get components that are subscribed.
        /// </summary>
        public static IEnumerable<BlueprintComponent> GetIGlobal<TEvent>(UnitEntityData unit = null) where TEvent : RulebookEvent
        {
            RulebookEventBus.GlobalRulebookSubscribers.m_Listeners.TryGetValue(typeof(TEvent), out var iSubscriber);
            if (iSubscriber == null || iSubscriber is not RulebookSubscribersList<TEvent> subscribers)
                yield break;

            foreach (var subscriber in subscribers.List)
            {
                if (subscriber is EntityFactComponent runtimeComp && (unit == null || runtimeComp.GetSubscribingUnit() == unit))
                    yield return runtimeComp.SourceBlueprintComponent;
            }
        }

        /// <summary>
        /// Get components that are subscribed for a unit. Slow.
        /// </summary>
        public static IEnumerable<TComponent> GetISubscribers<TComponent>(UnitEntityData unit) where TComponent : BlueprintComponent, ISubscriber
        {
            int num;
            Type iHandler;

            // resolve which interfaces are used
            if (typeof(IInitiatorRulebookSubscriber).IsAssignableFrom(typeof(TComponent)))
            {
                iHandler = typeof(IInitiatorRulebookHandler<>);
                num = 0;
            }
            else if (typeof(ITargetRulebookSubscriber).IsAssignableFrom(typeof(TComponent)))
            {
                iHandler = typeof(ITargetRulebookHandler<>);
                num = 1;
            }
            else if (typeof(IGlobalRulebookSubscriber).IsAssignableFrom(typeof(TComponent)))
            {
                iHandler = typeof(IGlobalRulebookHandler<>);
                num = 2;
            }
            else yield break;

            // resolve any one RulebookEvent
            Type iRulebookEvent = null;
            foreach (Type t1 in typeof(TComponent).GetInterfaces())
            {
                if (t1.IsGenericType && t1.GetGenericTypeDefinition() == iHandler)
                {
                    iRulebookEvent = t1.GenericTypeArguments[0];
                    break;
                }
            }
            if (iRulebookEvent == null) yield break;

            // get subscriber list
            List<object> list = null;
            if (num == 0)
            {
                RulebookEventBus.InitiatorRulebookSubscribers.Get(unit).m_Listeners.TryGetValue(iRulebookEvent, out var list1);
                list = list1?.GetType().GetField("List").GetValue(list1) as List<object>;
            }
            else if (num == 1)
            {
                RulebookEventBus.TargetRulebookSubscribers.Get(unit).m_Listeners.TryGetValue(iRulebookEvent, out var list1);
                list = list1?.GetType().GetField("List").GetValue(list1) as List<object>;
            }
            else if (num == 2)
            {
                RulebookEventBus.GlobalRulebookSubscribers.m_Listeners.TryGetValue(iRulebookEvent, out var list1);
                list = list1?.GetType().GetField("List").GetValue(list1) as List<object>;
            }
            if (list == null) yield break;

            // return components
            foreach (var c1 in list)
                if (c1 is EntityFactComponent c2 && c2.SourceBlueprintComponent is TComponent c3 && (unit == null || c2.GetSubscribingUnit() == unit))
                    yield return c3;
        }

        /// <summary>
        /// Get components that are subscribed for a unit.
        /// </summary>
        public static IEnumerable<TComponent> GetISubscribers<TComponent, TEvent>(UnitEntityData unit) where TComponent : BlueprintComponent, ISubscriber where TEvent : RulebookEvent
        {
            if (typeof(IInitiatorRulebookSubscriber).IsAssignableFrom(typeof(TComponent)))
            {
                foreach (var t1 in GetIInitiator<TEvent>(unit))
                    if (t1 is TComponent t2)
                        yield return t2;
            }
            else if (typeof(ITargetRulebookSubscriber).IsAssignableFrom(typeof(TComponent)))
            {
                foreach (var t1 in GetITarget<TEvent>(unit))
                    if (t1 is TComponent t2)
                        yield return t2;
            }
            else if (typeof(IGlobalRulebookSubscriber).IsAssignableFrom(typeof(TComponent)))
            {
                foreach (var t1 in GetIGlobal<TEvent>(unit))
                    if (t1 is TComponent t2)
                        yield return t2;
            }
        }

        /// <summary>
        /// Get last component that is subscribed for a unit.
        /// </summary>
        public static TComponent GetISubscriber<TComponent, TEvent>(UnitEntityData unit) where TComponent : BlueprintComponent, ISubscriber where TEvent : RulebookEvent
        {
            if (typeof(IInitiatorRulebookSubscriber).IsAssignableFrom(typeof(TComponent)))
            {
                RulebookEventBus.InitiatorRulebookSubscribers.Get(unit).m_Listeners.TryGetValue(typeof(TEvent), out var iSubscriber);
                if (iSubscriber != null && iSubscriber is RulebookSubscribersList<TEvent> subscribers)
                    return subscribers.List.LastOrDefault(f => f is TComponent) as TComponent;
            }
            else if (typeof(ITargetRulebookSubscriber).IsAssignableFrom(typeof(TComponent)))
            {
                RulebookEventBus.TargetRulebookSubscribers.Get(unit).m_Listeners.TryGetValue(typeof(TEvent), out var iSubscriber);
                if (iSubscriber != null && iSubscriber is RulebookSubscribersList<TEvent> subscribers)
                    return subscribers.List.LastOrDefault(f => f is TComponent) as TComponent;
            }
            else if (typeof(IGlobalRulebookSubscriber).IsAssignableFrom(typeof(TComponent)))
            {
                RulebookEventBus.GlobalRulebookSubscribers.m_Listeners.TryGetValue(typeof(TEvent), out var iSubscriber);
                if (iSubscriber != null && iSubscriber is RulebookSubscribersList<TEvent> subscribers)
                    return subscribers.List.LastOrDefault(f => f is TComponent) as TComponent;
            }
            return null;
        }

        private static bool FeatureParamEquals(FeatureParam param2, FeatureParam param)
        {
            if (param.Blueprint != null)
                return param.Blueprint == param2;
            if (param.WeaponCategory != null)
                return (param.WeaponCategory & param2.WeaponCategory) != 0;
            if (param.SpellSchool != null)
                return (param.SpellSchool & param2.SpellSchool) != 0;
            if (param.StatType != null)
                return (param.StatType & param2.StatType) != 0;
            return false;
        }

        public static bool HasParam(this UnitEntityData unit, BlueprintFeature blueprintFeature, FeatureParam param)
        {
            foreach (var feature in unit.Facts.GetAll<Feature>())
            {
                if (blueprintFeature != null && blueprintFeature != feature?.Blueprint)
                    continue;

                if (FeatureParamEquals(feature.Param, param))
                    return true;
            }
            return false;
        }

        public static TData GetFactData<TComp, TData>(this UnitEntityData unit, BlueprintFeature blueprintFeature = null) where TComp : UnitFactComponentDelegate<TData> where TData : class, new()
        {
            foreach (var feature in unit.Facts.GetAll<Feature>())
            {
                if (blueprintFeature != null && blueprintFeature != feature?.Blueprint)
                    continue;

                var data = Helper.GetDataExt<TData>(feature);
                if (data != null)
                    return data;
            }
            return null;
        }

        public static TData GetDataExt<TData>(this EntityFact fact) where TData : class, new()
        {
            if (fact == null)
                return null;

            foreach (var component in fact.Components)
            {
                var data = Helper.GetDataExt<TData>(component);
                if (data != null)
                    return data;
            }
            return null;
        }

        /// <summary>
        /// Use this instead of GetData&lt;<typeparamref name="TData"/>&gt;. This returns null instead of an exception.
        /// </summary>
        public static TData GetDataExt<TData>(this EntityFactComponent component) where TData : class, new()
        {
            if (component is EntityFactComponentDelegate<UnitEntityData, TData>.ComponentRuntime runtime)
            {
                return runtime.m_Data;
            }

            PrintDebug("GetData<TData>(EntityFactComponent) is not runtime: " + Environment.StackTrace);
            return null;
        }

        public static TData GetDataExt<TComponent, TData>(this EntityFact fact) where TComponent : class where TData : class, new()
        {
            if (fact == null)
                return null;

            foreach (var component in fact.Components)
            {
                if (component.SourceBlueprintComponent is not TComponent)
                    continue;

                var data = Helper.GetDataExt<TData>(component);
                if (data != null)
                    return data;
            }
            return null;
        }

        /// <param name="blueprintFeature">type: BlueprintFeature</param>
        public static IEnumerable<TData> GetDataAllExt<TData>(this UnitEntityData unit, AnyRef blueprintFeature = null) where TData : class, new()
        {
            foreach (var feature in unit.Facts.GetAll<Feature>())
            {
                if (blueprintFeature != null && blueprintFeature.deserializedGuid != feature?.Blueprint?.AssetGuid)
                    continue;

                foreach (var component in feature.Components)
                {
                    var data = Helper.GetDataExt<TData>(component);
                    if (data != null)
                        yield return data;
                }
            }
        }

        public static IEnumerable<EntityFactComponent> GetRuntimeComponents<TComp>(this UnitEntityData unit) where TComp : class
        {
            //return unit.Progression.Features.SelectFactComponents<TComp>();
            foreach (var fact in unit.Facts.m_Facts)
            {
                foreach (var comp in fact.Components)
                {
                    if (comp.SourceBlueprintComponent is TComp)
                        yield return comp;
                }
            }
        }

        public static EntityFactComponent GetRuntimeComponent<TComp>(this EntityFact fact) where TComp : class
        {
            fact.GetComponent<BlueprintComponent>();
            foreach (var comp in fact.Components)
            {
                if (comp.SourceBlueprintComponent is TComp)
                    return comp;
            }
            return null;
        }

        #endregion

        #region Context Values

        public static ContextStatValue CreateContextStatValue(StatType stat, ModifierDescriptor specificModifier = ModifierDescriptor.None, bool raw = false)
        {
            return new ContextStatValue
            {
                Stat = stat,
                SpecificModifier = specificModifier,
                GetRawValue = raw
            };
        }

        public static ContextValue CreateContextValue(int value)
        {
            return (ContextValue)value;
        }

        public static ContextValue CreateContextValue(AbilityRankType value = AbilityRankType.Default)
        {
            return new ContextValue() { ValueType = ContextValueType.Rank, ValueRank = value };
        }

        public static ContextValue CreateContextValue(AbilitySharedValue value)
        {
            return new ContextValue() { ValueType = ContextValueType.Shared, ValueShared = value };
        }

        public static ContextDurationValue CreateContextDurationValue(ContextValue diceCount = null, DiceType dice = DiceType.Zero, ContextValue bonus = null, DurationRate rate = DurationRate.Rounds)
        {
            return new ContextDurationValue()
            {
                DiceCountValue = diceCount ?? 0,
                DiceType = dice,
                BonusValue = bonus ?? 0,
                Rate = rate
            };
        }

        public static ContextDurationValue CreateContextDurationValue(AbilityRankType diceRank, DiceType dice = DiceType.Zero, int bonus = 0, DurationRate rate = DurationRate.Rounds)
        {
            if ((int)dice <= 1 && bonus == 0)
                return new ContextDurationValue()
                {
                    DiceCountValue = 0,
                    DiceType = DiceType.Zero,
                    BonusValue = CreateContextValue(diceRank),
                    Rate = rate
                };

            return new ContextDurationValue()
            {
                DiceCountValue = CreateContextValue(diceRank),
                DiceType = dice,
                BonusValue = bonus,
                Rate = rate
            };
        }

        #endregion

        #region Create Advanced

        private static MethodInfo _memberwiseClone = AccessTools.Method(typeof(object), "MemberwiseClone");
        /// <summary>
        /// This creates a deep copy of ComponentsArray, but other references might still be shared and morphing can happen.<br/><br/>
        /// Watch out for these fields:<br/>
        /// BlueprintFeature: IsPrerequisiteFor
        /// BlueprintAbility: m_Parent
        /// </summary>
        /// <param name="guid2">guid to merge with, use for dynamic blueprints (unknown list of blueprints), otherwise empty</param>
        public static T Clone<T>(this T obj, string name, string guid2 = null) where T : SimpleBlueprint
        {
            string guid;
            if (guid2 != null)
                guid = MergeIds(name, obj.AssetGuid.ToString(), guid2);
            else
                guid = GetGuid(name);

            var result = (T)_memberwiseClone.Invoke(obj, null);
            result.name = name;
            AddAsset(result, guid);

            if (result is BlueprintScriptableObject bp)
            {
                for (int i = 0; i < bp.ComponentsArray.Length; i++)
                {
                    var comp = bp.ComponentsArray[i] = bp.ComponentsArray[i].Clone();
                    comp.name = $"${comp.GetType().Name}${obj.AssetGuid}${i}";
                    comp.OwnerBlueprint = bp;
                }
            }

            // prevent some morphing
            if (result is BlueprintFeature feature)
            {
                feature.IsPrerequisiteFor = new();
            }
            else if (result is BlueprintAbility ability)
            {
                ability.m_Parent = null;
            }

            return result;
        }

        /// <summary>
        /// Creates deep copy of BlueprintComponent, but shallow copy of anything else.
        /// </summary>
        public static T Clone<T>(this T obj, Action<T> action = null) where T : class
        {
            //if (typeof(T).IsGenericType && typeof(List<>) == typeof(T).GetGenericTypeDefinition())
            //    return Activator.CreateInstance(typeof(T), new object[] { (IEnumerable<object>)obj }) as T;

            var result = (T)_memberwiseClone.Invoke(obj, null);
            if (result is BlueprintComponent comp)
            {
                comp.name = null;
                comp.OwnerBlueprint = null;
            }

            action?.Invoke(result);
            return result;
        }

        /// <summary>
        /// Creates deep copy of IEnumerable<BlueprintComponent>.
        /// </summary>
        public static List<T> Clone<T>(this IEnumerable<T> obj, Action<T> action = null) where T : BlueprintComponent
        {
            var result = obj.ToList();

            for (int i = 0; i < result.Count; i++)
            {
                result[i] = result[i].Clone();
                result[i].name = null;
                result[i].OwnerBlueprint = null;

                action?.Invoke(result[i]);
            }

            return result;
        }

        private static ulong ParseGuidLow(string id) => ulong.Parse(id.Substring(id.Length - 16), System.Globalization.NumberStyles.HexNumber);
        private static ulong ParseGuidHigh(string id) => ulong.Parse(id.Substring(0, id.Length - 16), System.Globalization.NumberStyles.HexNumber);
        public static string MergeIds(string name, string guid1, string guid2, string guid3 = null)
        {
            // Parse into low/high 64-bit numbers, and then xor the two halves.
            ulong low = ParseGuidLow(guid1);
            ulong high = ParseGuidHigh(guid1);

            low ^= ParseGuidLow(guid2);
            high ^= ParseGuidHigh(guid2);

            if (guid3 != null)
            {
                low ^= ParseGuidLow(guid3);
                high ^= ParseGuidHigh(guid3);
            }

            var result = high.ToString("x16") + low.ToString("x16");
            //PrintDebug($"MergeIds {guid1} + {guid2} + {guid3} = {result}");
            RegGuid(name, result);
            return result;
        }

        private static BlueprintFeatureSelection _basicfeats1;
        private static BlueprintFeatureSelection _basicfeats2;
        private static BlueprintFeatureSelection _basicfeats3;
        private static BlueprintFeatureSelection _combatfeats1;
        private static BlueprintFeatureSelection _combatfeats2;
        private static BlueprintFeatureSelection _combatfeats3;
        public static void AddFeats(params BlueprintFeature[] feats)
        {
            _basicfeats1 ??= Get<BlueprintFeatureSelection>("247a4068296e8be42890143f451b4b45"); //BasicFeatSelection
            AppendAndReplace(ref _basicfeats1.m_AllFeatures, feats.ToRef());

            _basicfeats2 ??= Get<BlueprintFeatureSelection>("e10c4f18a6c8b4342afe6954bde0587b"); //ExtraFeatMythicFeat
            AppendAndReplace(ref _basicfeats2.m_AllFeatures, feats.ToRef());

            _basicfeats3 ??= Get<BlueprintFeatureSelection>("a21acdafc0169f5488a9bd3256e2e65b"); //DragonLevel2FeatSelection
            AppendAndReplace(ref _basicfeats3.m_AllFeatures, feats.ToRef());
        }
        public static void AddCombatFeat(BlueprintFeature feats)
        {
            if (_combatfeats1 == null) //FighterFeatSelection
                _combatfeats1 = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("41c8486641f7d6d4283ca9dae4147a9f");
            AppendAndReplace(ref _combatfeats1.m_AllFeatures, feats.ToRef());

            if (_combatfeats2 == null) //CombatTrick
                _combatfeats2 = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("c5158a6622d0b694a99efb1d0025d2c1");
            AppendAndReplace(ref _combatfeats2.m_AllFeatures, feats.ToRef());

            if (_combatfeats3 == null) //ExtraFeatMythicFeat
                _combatfeats3 = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("e10c4f18a6c8b4342afe6954bde0587b");
            AppendAndReplace(ref _combatfeats3.m_AllFeatures, feats.ToRef());

            AddFeats(feats);
        }

        private static BlueprintFeatureSelection _mythicfeats;
        private static BlueprintFeatureSelection _mythictalents;
        private static BlueprintFeatureSelection _mythicextratalents;
        public static void AddMythicTalent(BlueprintFeature feat)
        {
            if (_mythictalents == null)
                _mythictalents = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("ba0e5a900b775be4a99702f1ed08914d");
            if (_mythicextratalents == null)
                _mythicextratalents = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("8a6a511c55e67d04db328cc49aaad2b8");

            AppendAndReplace(ref _mythictalents.m_AllFeatures, feat.ToRef());
            _mythicextratalents.m_AllFeatures = _mythictalents.m_AllFeatures;
        }

        public static void AddMythicFeat(BlueprintFeature feat)
        {
            if (_mythicfeats == null)
                _mythicfeats = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("9ee0f6745f555484299b0a1563b99d81");

            AppendAndReplace(ref _mythicfeats.m_AllFeatures, feat.ToRef());
        }

        private static BlueprintFeatureSelection _roguefeats;
        private static BlueprintFeatureSelection _slayerfeats1;
        private static BlueprintFeatureSelection _slayerfeats2;
        private static BlueprintFeatureSelection _slayerfeats3;
        private static BlueprintFeatureSelection _vivsectionistfeats;
        public static void AddRogueFeat(BlueprintFeature feat)
        {
            if (_roguefeats == null)
                _roguefeats = Get<BlueprintFeatureSelection>("c074a5d615200494b8f2a9c845799d93");
            if (_slayerfeats1 == null)
                _slayerfeats1 = Get<BlueprintFeatureSelection>("04430ad24988baa4daa0bcd4f1c7d118");
            if (_slayerfeats2 == null)
                _slayerfeats2 = Get<BlueprintFeatureSelection>("43d1b15873e926848be2abf0ea3ad9a8");
            if (_slayerfeats3 == null)
                _slayerfeats3 = Get<BlueprintFeatureSelection>("913b9cf25c9536949b43a2651b7ffb66");
            if (_vivsectionistfeats == null)
                _vivsectionistfeats = Get<BlueprintFeatureSelection>("67f499218a0e22944abab6fe1c9eaeee");
            if (_tricksterhexes == null)
                _tricksterhexes = Get<BlueprintFeatureSelection>("290bbcc3c3bb92144b853fd8fb8ff452");

            var reference = feat.ToRef();

            AppendAndReplace(ref _roguefeats.m_AllFeatures, reference);
            AppendAndReplace(ref _slayerfeats1.m_AllFeatures, reference);
            AppendAndReplace(ref _slayerfeats2.m_AllFeatures, reference);
            AppendAndReplace(ref _slayerfeats3.m_AllFeatures, reference);
            AppendAndReplace(ref _vivsectionistfeats.m_AllFeatures, reference);
            AppendAndReplace(ref _tricksterhexes.m_AllFeatures, reference);
        }

        private static BlueprintFeatureSelection _witchhexes;
        private static BlueprintFeatureSelection _shamanhexes;
        private static BlueprintFeatureSelection _hexcrafterhexes;
        private static BlueprintFeatureSelection _tricksterhexes;
        private static BlueprintFeatureSelection _winterhexes;
        public static void AddHex(BlueprintFeature hex, bool allowShaman = true)
        {
            _witchhexes ??= Get<BlueprintFeatureSelection>("9846043cf51251a4897728ed6e24e76f");
            _shamanhexes ??= Get<BlueprintFeatureSelection>("4223fe18c75d4d14787af196a04e14e7");
            _hexcrafterhexes ??= Get<BlueprintFeatureSelection>("ad6b9cecb5286d841a66e23cea3ef7bf");
            _tricksterhexes ??= Get<BlueprintFeatureSelection>("290bbcc3c3bb92144b853fd8fb8ff452");
            _winterhexes ??= Get<BlueprintFeatureSelection>("b921af3627142bd4d9cf3aefb5e2610a");

            var reference = hex.ToRef();

            AppendAndReplace(ref _witchhexes.m_AllFeatures, reference);
            if (allowShaman)
                AppendAndReplace(ref _shamanhexes.m_AllFeatures, reference);
            AppendAndReplace(ref _hexcrafterhexes.m_AllFeatures, reference);
            AppendAndReplace(ref _tricksterhexes.m_AllFeatures, reference);
            AppendAndReplace(ref _winterhexes.m_AllFeatures, reference);
        }

        private static BlueprintFeatureSelection _infusions;
        /// <param name="blueprintFeature">type: BlueprintFeature</param>
        public static void AddInfusion(AnyRef blueprintFeature)
        {
            if (_infusions == null)
                _infusions = Get<BlueprintFeatureSelection>("58d6f8e9eea63f6418b107ce64f315ea");
            AppendAndReplace(ref _infusions.m_AllFeatures, blueprintFeature);
        }

        private static BlueprintFeatureSelection _wildtalents;
        /// <param name="blueprintFeature">type: BlueprintFeature</param>
        public static void AddWildTalent(AnyRef blueprintFeature)
        {
            if (_wildtalents == null)
                _wildtalents = Get<BlueprintFeatureSelection>("5c883ae0cd6d7d5448b7a420f51f8459");
            AppendAndReplace(ref _wildtalents.m_AllFeatures, blueprintFeature);
        }

        public static BlueprintAbility AddToAbilityVariants(this BlueprintAbility parent, IEnumerable<BlueprintAbility> variants) => AddToAbilityVariants(parent, variants.ToArray());

        public static BlueprintAbility AddToAbilityVariants(this BlueprintAbility parent, params BlueprintAbility[] variants)
        {
            var comp = parent.GetComponent<AbilityVariants>();
            if (comp == null)
            {
                comp = new AbilityVariants();
                comp.m_Variants = variants.ToRef();
                parent.AddComponents(comp);
            }
            else
            {
                AppendAndReplace(ref comp.m_Variants, variants.ToRef());
            }

            foreach (var v in variants)
            {
                v.Parent = parent;
            }
            return parent;
        }

        public static BlueprintBuff Flags(this BlueprintBuff buff, bool? hidden = null, bool? stayOnDeath = null, bool? isFromSpell = null, bool? harmful = null)
        {
            if (hidden != null)
            {
                if (hidden.Value)
                    buff.m_Flags |= BlueprintBuff.Flags.HiddenInUi;
                else
                    buff.m_Flags &= ~BlueprintBuff.Flags.HiddenInUi;
            }

            if (stayOnDeath != null)
            {
                if (stayOnDeath.Value)
                    buff.m_Flags |= BlueprintBuff.Flags.StayOnDeath;
                else
                    buff.m_Flags &= ~BlueprintBuff.Flags.StayOnDeath;
            }

            if (isFromSpell != null)
            {
                if (isFromSpell.Value)
                    buff.m_Flags |= BlueprintBuff.Flags.IsFromSpell;
                else
                    buff.m_Flags &= ~BlueprintBuff.Flags.IsFromSpell;
            }

            if (harmful != null)
            {
                if (harmful.Value)
                    buff.m_Flags |= BlueprintBuff.Flags.Harmful;
                else
                    buff.m_Flags &= ~BlueprintBuff.Flags.Harmful;
            }

            return buff;
        }

        public static void AddArcaneVendorItem(BlueprintItemReference item, int amount = 1)
        {
            var vendor_table = ResourcesLibrary.TryGetBlueprint<BlueprintSharedVendorTable>("5450d563aab78134196ee9a932e88671"); //ArcaneScrollsVendorTableI
            vendor_table.AddComponents(new LootItemsPackFixed() { m_Item = new LootItem() { m_Item = item }, m_Count = amount });
        }

        public static void AddExoticVendorItem(BlueprintItemReference item, int amount = 1)
        {
            var vendor_table = ResourcesLibrary.TryGetBlueprint<BlueprintSharedVendorTable>("5f722616e64f41b4f960cd00c0b4896c"); //RE_Chapter3VendorTableExotic
            vendor_table.AddComponents(new LootItemsPackFixed() { m_Item = new LootItem() { m_Item = item }, m_Count = amount });
        }

        public static ContextCondition[] MakeConditionHasNoBuff(params BlueprintBuff[] buffs)
        {
            if (buffs == null || buffs[0] == null) throw new ArgumentNullException();
            var result = new ContextCondition[buffs.Length];

            for (int i = 0; i < result.Length; i++)
            {
                var buff = new ContextConditionHasBuff();
                buff.m_Buff = buffs[i].ToRef();
                buff.Not = true;
                result[i] = buff;
            }

            return result;
        }

        public static ContextActionSavingThrow MakeContextActionSavingThrow(SavingThrowType savingthrow, [CanBeNull] GameAction succeed, [CanBeNull] GameAction failed)
        {
            var result = new ContextActionSavingThrow();
            result.Type = savingthrow;
            result.Actions = CreateActionList(new ContextActionConditionalSaved()
            {
                Succeed = CreateActionList(succeed),
                Failed = CreateActionList(failed)
            });
            return result;
        }

        public static ContextActionSavingThrow Add(this ContextActionSavingThrow b, GameAction addition, bool onSucceed = false, bool onFailed = false)
        {
            if (onSucceed)
            {
                foreach (var action in b.Actions.Actions)
                    if (action is ContextActionConditionalSaved save)
                        AppendAndReplace(ref save.Succeed.Actions, addition);
            }
            else if (onFailed)
            {
                foreach (var action in b.Actions.Actions)
                    if (action is ContextActionConditionalSaved save)
                        AppendAndReplace(ref save.Failed.Actions, addition);
            }
            else
            {
                AppendAndReplace(ref b.Actions.Actions, addition);
            }

            return b;
        }

        public static void MakeStickySpell(BlueprintAbility spell, out BlueprintAbility cast, out BlueprintAbility effect)
        {
            const string guid_cast = "5fc3a01f26584d84b9c2bef04ec6cd8b";
            const string guid_effect = "73749154177c4f8c83231da1c9dc9f6f";

            cast = spell.Clone(spell.name + "_Cast", guid2: guid_cast)
                    .RemoveComponents<AbilityDeliverProjectile>()
                    .RemoveComponents<AbilityEffectRunAction>();
            effect = spell.Clone(spell.name + "_Effect", guid2: guid_effect)
                    .RemoveComponents<AbilityDeliverProjectile>();

            cast.AddComponents(CreateAbilityEffectStickyTouch(effect.ToRef()));
            cast.Range = AbilityRange.Touch;
            cast.Animation = CastAnimationStyle.Self;

            effect.AddComponents(CreateAbilityDeliverTouch());
            effect.Range = AbilityRange.Touch;
            effect.Animation = CastAnimationStyle.Touch;
        }

        /// <param name="buff">type: <b>BlueprintBuff</b></param>
        public static AbilityEffectRunAction MakeRunActionApplyBuff(AnyRef buff, ContextDurationValue duration = null, bool dispellable = false, bool toCaster = false)
        {
            var result = new AbilityEffectRunAction();
            result.Actions = CreateActionList(
                CreateContextActionApplyBuff(buff, duration, dispellable: dispellable, toCaster: toCaster, asChild: toCaster, permanent: duration == null)
                );
            return result;
        }

        public static BlueprintAbility TargetPoint(this BlueprintAbility ability, CastAnimationStyle animation = CastAnimationStyle.Directional, bool self = false)
        {
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            ability.CanTargetEnemies = true;
            ability.CanTargetPoint = true;
            ability.CanTargetFriends = true;
            ability.CanTargetSelf = self;
            ability.Animation = animation;
            return ability;
        }
        public static BlueprintAbility TargetEnemy(this BlueprintAbility ability, CastAnimationStyle animation = CastAnimationStyle.Directional, bool point = false)
        {
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.CanTargetEnemies = true;
            ability.CanTargetPoint = point;
            ability.CanTargetFriends = false;
            ability.CanTargetSelf = false;
            ability.Animation = animation;
            return ability;
        }
        public static BlueprintAbility TargetAlly(this BlueprintAbility ability, CastAnimationStyle animation = CastAnimationStyle.Directional, bool self = true, bool point = false)
        {
            ability.EffectOnEnemy = AbilityEffectOnUnit.None;
            ability.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            ability.CanTargetEnemies = false;
            ability.CanTargetPoint = point;
            ability.CanTargetFriends = true;
            ability.CanTargetSelf = self;
            ability.Animation = animation;
            return ability;
        }
        public static BlueprintAbility TargetAny(this BlueprintAbility ability, CastAnimationStyle animation = CastAnimationStyle.Directional, bool self = true, bool point = false, bool harmful = true)
        {
            ability.EffectOnEnemy = harmful ? AbilityEffectOnUnit.Harmful : AbilityEffectOnUnit.Helpful;
            ability.EffectOnAlly = harmful ? AbilityEffectOnUnit.Harmful : AbilityEffectOnUnit.Helpful;
            ability.CanTargetEnemies = true;
            ability.CanTargetPoint = point;
            ability.CanTargetFriends = true;
            ability.CanTargetSelf = self;
            ability.Animation = animation;
            return ability;
        }
        public static BlueprintAbility TargetSelf(this BlueprintAbility ability, CastAnimationStyle animation = CastAnimationStyle.Omni)
        {
            ability.EffectOnEnemy = AbilityEffectOnUnit.None;
            ability.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            ability.CanTargetEnemies = false;
            ability.CanTargetPoint = false;
            ability.CanTargetFriends = false;
            ability.CanTargetSelf = true;
            ability.Animation = animation;
            return ability;
        }

        /// <summary>Puts a new Conditional infront of existing ActionList.</summary>
        public static ActionList InjectCondition(this ActionList actionList, Condition condition, bool ifTrue = true)
        {
            if (ifTrue)
                actionList.Actions = new GameAction[] { CreateConditional(condition.ObjToArray(), ifTrue: actionList.Actions) };
            else
                actionList.Actions = new GameAction[] { CreateConditional(condition.ObjToArray(), ifFalse: actionList.Actions) };

            return actionList;
        }

        public static Conditional AddCondition(this Conditional conditional, Condition condition)
        {
            InsertAt(ref conditional.ConditionsChecker.Conditions, condition);
            return conditional;
        }

        public static ActionList InsertAt(this ActionList actionList, GameAction gameAction, int index = -1)
        {
            InsertAt(ref actionList.Actions, gameAction, index);
            return actionList;
        }

        #endregion

        #region Metamagic

        public static int GetCost(this Metamagic metamagic, UnitEntityData unit = null)
        {
            int cost = metamagic.DefaultCost();

            if (unit != null)
            {
                var state = unit.State.Features;
                if ((metamagic == Metamagic.Empower && state.FavoriteMetamagicEmpower)
                    || (metamagic == Metamagic.Maximize && state.FavoriteMetamagicMaximize)
                    || (metamagic == Metamagic.Quicken && state.FavoriteMetamagicQuicken)
                    || (metamagic == Metamagic.Reach && state.FavoriteMetamagicReach)
                    || (metamagic == Metamagic.Extend && state.FavoriteMetamagicExtend)
                    || (metamagic == Metamagic.Selective && state.FavoriteMetamagicSelective)
                    || (metamagic == Metamagic.Bolstered && state.FavoriteMetamagicBolstered))
                    cost--;
            }

            return cost;
        }

        #endregion

        #region Blueprint Ability

        public static BlueprintAbilityAreaEffectReference GetArea(this BlueprintAbility ability)
        {
            var runAction = ability.GetComponent<AbilityEffectRunAction>();
            if (runAction == null)
                return null;

            foreach (var action in runAction.Actions.Actions)
                if (action is ContextActionSpawnAreaEffect spawnarea)
                    return spawnarea.m_AreaEffect;
            return null;
        }

        #endregion

        #region Blueprint Weapon

        /// <param name="BlueprintWeaponEnchantment">direct type, reference, or string</param>
        public static BlueprintItemWeapon SetEnchantment(this BlueprintItemWeapon bp, params object[] BlueprintWeaponEnchantment)
        {
            var list = new List<BlueprintWeaponEnchantmentReference>();

            foreach (var obj in BlueprintWeaponEnchantment)
            {
                if (obj is string guid)
                    list.Add(guid.ToRef<BlueprintWeaponEnchantmentReference>());
                else if (obj is BlueprintWeaponEnchantmentReference reference)
                    list.Add(reference);
                else if (obj is BlueprintWeaponEnchantment enchantment)
                    list.Add(enchantment.ToRef());
                else
                    throw new ArgumentException("invalid type: " + obj.GetType().AssemblyQualifiedName);
            }

            bp.m_Enchantments = list.ToArray();
            return bp;
        }

        public static void GenerateEnchantedWeapons(this BlueprintWeaponType weaponType, List<BlueprintItemWeapon> list, BlueprintWeaponEnchantment enchantment, bool full, params object[] BlueprintWeaponEnchantment)
        {
            var plus1 = ToRef<BlueprintWeaponEnchantmentReference>("d42fc23b92c640846ac137dc26e000d4");
            var plus2 = ToRef<BlueprintWeaponEnchantmentReference>("eb2faccc4c9487d43b3575d7e77ff3f5");
            var plus3 = ToRef<BlueprintWeaponEnchantmentReference>("80bb8a737579e35498177e1e3c75899b");
            var plus4 = ToRef<BlueprintWeaponEnchantmentReference>("783d7d496da6ac44f9511011fc5f1979");
            var plus5 = ToRef<BlueprintWeaponEnchantmentReference>("bdba267e951851449af552aa9f9e3992");

            throw new NotImplementedException();
        }

        #endregion

        #region Blueprint Progression

        /// <summary>Merges LevelEntries or appends new ones.</summary>
        public static void AddEntries(this BlueprintProgression progression, List<LevelEntry> entries)
        {
            for (int i = entries.Count - 1; i > 0; i--)
            {
                if (progression.LevelEntries.TryFind(f => f.Level == entries[i].Level, out var entry))
                {
                    entry.m_Features.AddRange(entries[i].m_Features);
                    entries.RemoveAt(i);
                }
            }

            if (entries.Count > 0)
            {
                AppendAndReplace(ref progression.LevelEntries, entries);
            }
        }
        public static void AddEntries(this BlueprintProgression progression, params LevelEntry[] entries) => AddEntries(progression, entries.ToList());

        public static void AddFeature(this BlueprintProgression progression, int level, AnyRef blueprintFeature, AnyRef pairWithGuid = null)
        {
            var levelentry = progression.LevelEntries.FirstOrDefault(f => f.Level == level);
            if (levelentry != null)
                levelentry.m_Features.Add(blueprintFeature.ToRef<BlueprintFeatureBaseReference>());
            else
                AppendAndReplace(ref progression.LevelEntries, CreateLevelEntry(level, blueprintFeature));

            if (pairWithGuid != null)
            {
                var pairGuid = pairWithGuid.deserializedGuid;
                foreach (var ui in progression.UIGroups)
                {
                    if (ui.m_Features.Any(a => a.deserializedGuid == pairGuid))
                    {
                        ui.m_Features.Add(blueprintFeature.ToRef<BlueprintFeatureBaseReference>());
                        break;
                    }
                }
            }

        }

        public static void RemoveFeature(this BlueprintProgression progression, AnyRef blueprintFeature)
        {
            foreach (var levelentry in progression.LevelEntries)
            {
                if (levelentry.m_Features == null)
                    continue;
                if (levelentry.m_Features.Remove(blueprintFeature))
                    return;
            }
        }

        public static void ClearEntries(this BlueprintProgression progression, int level)
        {
            if (level < 0)
                progression.LevelEntries = Array.Empty<LevelEntry>();
            else
            {
                for (int i = progression.LevelEntries.Length - 1; i >= 0; i--)
                {
                    if (progression.LevelEntries[i].Level == level)
                    {
                        RemoveAt(ref progression.LevelEntries, i);
                        break;
                    }
                }
            }
        }

        #endregion

        #region Blueprint Selection

        public static BlueprintFeatureSelection Add(this BlueprintFeatureSelection selection, params BlueprintFeatureReference[] features)
        {
            AppendAndReplace(ref selection.m_AllFeatures, features);
            return selection;
        }

        public static BlueprintFeatureSelection Add(this BlueprintFeatureSelection selection, params AnyRef[] features)
        {
            AppendAndReplace(ref selection.m_AllFeatures, features.ToRef<BlueprintFeatureReference>());
            return selection;
        }

        public static BlueprintFeatureSelection SetSelection(this BlueprintFeatureSelection selection, params AnyRef[] features)
        {
            selection.m_AllFeatures = features.ToRef<BlueprintFeatureReference>();
            return selection;
        }

        #endregion

        #region Blueprint Archetype

        public static BlueprintArchetype SetAddFeatures(this BlueprintArchetype archetype, params LevelEntry[] entries)
        {
            archetype.AddFeatures = entries;
            return archetype;
        }

        public static BlueprintArchetype SetRemoveFeatures(this BlueprintArchetype archetype, params LevelEntry[] entries)
        {
            archetype.RemoveFeatures = entries;
            return archetype;
        }

        public static void AddFeature(this BlueprintArchetype obj, int level, BlueprintFeatureBase feature)
        {
            var levelentry = obj.AddFeatures.FirstOrDefault(f => f.Level == level);
            if (levelentry != null)
                levelentry.m_Features.Add(feature.ToRef());
            else
                AppendAndReplace(ref obj.AddFeatures, CreateLevelEntry(level, feature));
        }

        public static void RemoveFeature(this BlueprintArchetype obj, int level, BlueprintFeatureBase feature)
        {
            var levelentry = obj.RemoveFeatures.FirstOrDefault(f => f.Level == level);
            if (levelentry != null)
                levelentry.m_Features.Add(feature.ToRef());
            else
                AppendAndReplace(ref obj.RemoveFeatures, CreateLevelEntry(level, feature));
        }

        #endregion

        #region Rules

        private static FieldInfo _fieldRuleReasonAbility = AccessTools.Field(typeof(RuleReason), "<Ability>k__BackingField");
        public static void SetRuleAbility(this RuleReason reason, AbilityData abilityData)
        {
            _fieldRuleReasonAbility.SetValue(reason, abilityData);
        }

        public static RuleType ToRuleType(this RulebookEvent rule)
        {
            if (rule is RuleAttackRoll)
                return RuleType.AttackRoll;
            if (rule is RuleSkillCheck)
                return RuleType.SkillCheck;
            if (rule is RuleInitiativeRoll)
                return RuleType.Initiative;
            if (rule is RuleCombatManeuver)
                return RuleType.Maneuver;
            if (rule is RuleSpellResistanceCheck)
                return RuleType.SpellResistance;
            if (rule is RuleDispelMagic)
                return RuleType.DispelMagic;
            if (rule is RuleCheckConcentration)
                return RuleType.Concentration;
            return 0;
        }

        #endregion

        #region Spells

        private static BlueprintParametrizedFeature[] _spells1;
        public static void Add(this BlueprintSpellList spellList, BlueprintAbility spell, int level)
        {
            if (level < 0 || level > 10)
                throw new ArgumentOutOfRangeException();

            // ensure spell list has enough levels
            if (spellList.SpellsByLevel.Length <= level)
            {
                var spells = new SpellLevelList[11];
                for (int i = 0; i < spells.Length; i++)
                    spells[i] = spellList.SpellsByLevel.ElementAtOrDefault(i) ?? new SpellLevelList(i);
                spellList.SpellsByLevel = spells;
            }

            // add new spell to spell list
            spellList.SpellsByLevel[level].m_Spells.Add(spell.ToReference<BlueprintAbilityReference>());

            // attach spell list to new spell
            spell.AddComponents(
                CreateSpellListComponent(spellList.ToReference<BlueprintSpellListReference>(), level)
                );

            // add new spell to feature's spell selections
            if (_spells1 == null)
            {
                _list.Add(Helper.Get<BlueprintParametrizedFeature>("f327a765a4353d04f872482ef3e48c35")); //SpellSpecializationFirst
                foreach (var feature in Helper.Get<BlueprintFeatureSelection>("fe67bc3b04f1cd542b4df6e28b6e0ff5").m_AllFeatures) //SpellSpecializationSelection
                    if (feature.Get() is BlueprintParametrizedFeature specialization)
                        _list.Add(specialization);

                _spells1 = new BlueprintParametrizedFeature[_list.Count];
                _list.CopyTo(_spells1);
                _list.Clear();
            }
            var newAllSpells = Append(_spells1[0].BlueprintParameterVariants, spell.ToReference<AnyBlueprintReference>());
            for (int i = 0; i < _spells1.Length; i++)
                _spells1[i].BlueprintParameterVariants = newAllSpells;
        }

        public static void Add(this BlueprintAbility spell, int level, params AnyRef[] blueprintSpellList)
        {
            foreach (var spellList in blueprintSpellList.Get<BlueprintSpellList>())
            {
                spellList.Add(spell, level);
            }
        }

        public static void PrintSpellDescriptor(SpellDescriptor descriptor)
        {
            string[] names = Enum.GetNames(typeof(SpellDescriptor));
            for (int i = 1; i < names.Length; i++)
            {
                if (((long)descriptor & (1L << i - 1)) != 0)
                    Console.WriteLine(names[i]);
            }
        }

        public static BlueprintAbilityReference GetSticky(this BlueprintAbilityReference spell)
        {
            return spell.Get()?.GetComponent<AbilityEffectStickyTouch>()?.m_TouchDeliveryAbility;
        }

        /// <summary>
        /// Replace references with their sticky variant.
        /// </summary>
        public static BlueprintAbilityReference[] StickyResolve(this BlueprintAbilityReference[] spells)
        {
            var result = new BlueprintAbilityReference[spells.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = spells[i].GetSticky() ?? spells[i];
            return result;
        }

        #endregion

        #region Create

        public static void AddAsset(this SimpleBlueprint bp, string guid) => AddAsset(bp, BlueprintGuid.Parse(guid));
        public static void AddAsset(this SimpleBlueprint bp, Guid guid) => AddAsset(bp, new BlueprintGuid(guid));
        public static void AddAsset(this SimpleBlueprint bp, BlueprintGuid guid)
        {
            if (guid == BlueprintGuid.Empty)
                throw new ArgumentException("GUID must not be empty!");
            bp.AssetGuid = guid;
            ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(guid, bp);
        }

        public static T SetUIData<T>(this T dataprovider, IUIDataProvider other) where T : IUIDataProvider
        {
            return other switch
            {
                BlueprintUnitFact bp => SetUIData(dataprovider, bp.m_DisplayName, bp.m_Description, bp.m_Icon),
                BlueprintAbilityResource bp => SetUIData(dataprovider, bp.LocalizedName, bp.LocalizedDescription, bp.m_Icon),
                BlueprintCharacterClass bp => SetUIData(dataprovider, bp.LocalizedName, bp.LocalizedDescription, bp.m_Icon),
                BlueprintItem bp => SetUIData(dataprovider, bp.m_DisplayNameText, bp.m_DescriptionText, bp.m_Icon),
                BlueprintArchetype bp => SetUIData(dataprovider, bp.LocalizedName, bp.LocalizedDescription, bp.m_Icon),
                BlueprintSpellbook bp => SetUIData(dataprovider, bp.Name, null, null),
                BlueprintWeaponType bp => SetUIData(dataprovider, bp.m_DefaultNameText, bp.m_DescriptionText, bp.m_Icon),
                BlueprintKingdomBuff bp => SetUIData(dataprovider, bp.DisplayName, bp.Description, bp.Icon),
                _ => SetUIData(dataprovider, other.Name, other.Description, other.Icon),
            };
        }

        public static T SetUIData<T>(this T dataprovider, string displayname = null, string description = null, Sprite icon = null) where T : IUIDataProvider
        {
            return SetUIData(dataprovider, displayname?.CreateString(), description?.CreateString(), icon);
        }

        public static T SetUIData<T>(this T dataprovider, LocalizedString displayname = null, LocalizedString description = null, Sprite icon = null) where T : IUIDataProvider
        {
            switch (dataprovider)
            {
                case BlueprintUnitFact bp:
                    bp.m_DisplayName = displayname ?? bp.m_DisplayName;
                    bp.m_Description = description ?? bp.m_Description;
                    bp.m_Icon = icon ?? bp.m_Icon;
                    break;
                case BlueprintAbilityResource bp:
                    bp.LocalizedName = displayname ?? bp.LocalizedName;
                    bp.LocalizedDescription = description ?? bp.LocalizedDescription;
                    bp.m_Icon = icon ?? bp.m_Icon;
                    break;
                case BlueprintCharacterClass bp:
                    bp.LocalizedName = displayname ?? bp.LocalizedName;
                    bp.LocalizedDescription = description ?? bp.LocalizedDescription;
                    bp.m_Icon = icon ?? bp.m_Icon;
                    break;
                case BlueprintItem bp:
                    bp.m_DisplayNameText = displayname ?? bp.m_DisplayNameText;
                    bp.m_DescriptionText = description ?? bp.m_DescriptionText;
                    bp.m_Icon = icon ?? bp.m_Icon;
                    break;
                case BlueprintArchetype bp:
                    bp.LocalizedName = displayname ?? bp.LocalizedName;
                    bp.LocalizedDescription = description ?? bp.LocalizedDescription;
                    bp.m_Icon = icon ?? bp.m_Icon;
                    break;
                case BlueprintSpellbook bp:
                    bp.Name = displayname ?? bp.Name;
                    break;
                case BlueprintWeaponType bp:
                    bp.m_DefaultNameText = displayname ?? bp.m_DefaultNameText;
                    bp.m_DescriptionText = description ?? bp.m_DescriptionText;
                    bp.m_Icon = icon ?? bp.m_Icon;
                    break;
                case BlueprintKingdomBuff bp:
                    bp.DisplayName = displayname ?? bp.DisplayName;
                    bp.Description = description ?? bp.Description;
                    bp.Icon = icon ?? bp.Icon;
                    break;
                default:
                    PrintDebug($"unknown DataProvider: {dataprovider.GetType().FullName}");
                    break;
            }
            return dataprovider;
        }

        public static T SetGroups<T>(this T feature, params FeatureGroup[] groups) where T : BlueprintFeature
        {
            feature.Groups = groups;
            return feature;
        }

        /// <param name="resource">type: <b>BlueprintAbilityResource</b></param>
        public static AbilityResourceLogic CreateAbilityResourceLogic(AnyRef resource, int amount = 1)
        {
            var result = new AbilityResourceLogic();
            result.m_RequiredResource = resource;
            result.Amount = 1;
            return result;
        }

        public static ContextSetAbilityParams CreateContextSetAbilityParams(ContextValue dc = null, ContextValue casterLevel = null, ContextValue spellLevel = null, ContextValue concentration = null, bool add10toDC = true)
        {
            var result = new ContextSetAbilityParams();
            result.Add10ToDC = add10toDC;
            if (dc != null)
                result.DC = dc;
            if (casterLevel != null)
                result.CasterLevel = casterLevel;
            if (spellLevel != null)
                result.SpellLevel = spellLevel;
            if (concentration != null)
                result.Concentration = concentration;
            return result;
        }

        public static ContextCalculateAbilityParams CreateContextCalculateAbilityParams(ContextValue casterLevel = null, ContextValue spellLevel = null, StatType statType = StatType.Constitution, bool kineticist = false)
        {
            var result = new ContextCalculateAbilityParams();
            result.StatType = statType;
            result.ReplaceCasterLevel = casterLevel != null;
            result.CasterLevel = casterLevel;
            result.ReplaceSpellLevel = spellLevel != null;
            result.SpellLevel = spellLevel;
            return result;
        }

        /// <param name="BlueprintAbilityResource">type: <b>BlueprintAbilityResource</b></param>
        public static AddAbilityResources CreateAddAbilityResources(AnyRef BlueprintAbilityResource)
        {
            var result = new AddAbilityResources();
            result.UseThisAsResource = false;
            result.m_Resource = BlueprintAbilityResource;
            result.Amount = 0;
            result.RestoreAmount = true;
            result.RestoreOnLevelUp = false;
            return result;
        }

        public static AddClassSkill CreateAddClassSkill(StatType stat)
        {
            var result = new AddClassSkill();
            result.Skill = stat;
            return result;
        }

        public static RemoveFeatureOnApply CreateRemoveFeatureOnApply(AnyRef blueprintUnitFact)
        {
            var result = new RemoveFeatureOnApply();
            result.m_Feature = blueprintUnitFact;
            return result;
        }

        public static AddProficiencies CreateAddProficiencies(params WeaponCategory[] weaponCategory)
        {
            var result = new AddProficiencies();
            result.WeaponProficiencies = weaponCategory;
            return result;
        }

        public static PrerequisiteNotProficient CreatePrerequisiteNotProficient(WeaponCategory weaponCategory, bool hideInUI = true)
        {
            var result = new PrerequisiteNotProficient();
            result.WeaponProficiencies = weaponCategory.ObjToArray();
            result.Group = Prerequisite.GroupType.All;
            result.CheckInProgression = false;
            result.HideInUI = hideInUI;
            return result;
        }

        public static AddStartingEquipment CreateAddStartingEquipment(params BlueprintItemReference[] items)
        {
            var result = new AddStartingEquipment();
            result.CategoryItems = new WeaponCategory[0];
            result.m_BasicItems = items;
            return result;
        }

        public static ContextConditionIsAlly CreateContextConditionIsAlly()
        {
            return new ContextConditionIsAlly() { Not = false };
        }

        public static ContextConditionIsAlly CreateContextConditionIsEnemy()
        {
            return new ContextConditionIsAlly() { Not = true };
        }

        public static AddContextStatBonus CreateAddContextStatBonus(StatType statType, ModifierDescriptor descriptor = ModifierDescriptor.None, ContextValue value = null, int multiplier = 1)
        {
            var result = new AddContextStatBonus();
            result.Stat = statType;
            result.Descriptor = descriptor;
            result.Value = value ?? CreateContextValue();
            result.Multiplier = multiplier;
            return result;
        }

        public static AbilitySpawnFx CreateAbilitySpawnFx(string asset, AbilitySpawnFxTime time = AbilitySpawnFxTime.OnApplyEffect, AbilitySpawnFxAnchor anchor = AbilitySpawnFxAnchor.SelectedTarget)
        {
            var result = new AbilitySpawnFx();
            result.PrefabLink = GetPrefabLink(asset);
            result.Time = time;
            result.Anchor = anchor;
            result.WeaponTarget = AbilitySpawnFxWeaponTarget.None;
            result.DestroyOnCast = false;
            result.Delay = 0f;
            result.PositionAnchor = anchor;
            result.OrientationAnchor = AbilitySpawnFxAnchor.None;
            result.OrientationMode = AbilitySpawnFxOrientation.Copy;
            return result;
        }

        public static AbilityRequirementHasItemInHands CreateAbilityRequirementHasItemInHands(bool checkMelee = true)
        {
            var result = new AbilityRequirementHasItemInHands();
            result.m_Type = checkMelee ? AbilityRequirementHasItemInHands.RequirementType.HasMeleeWeapon : AbilityRequirementHasItemInHands.RequirementType.HasShield;
            return result;
        }

        public static AbilityEffectStickyTouch CreateAbilityEffectStickyTouch(BlueprintAbilityReference ability)
        {
            var result = new AbilityEffectStickyTouch();
            result.m_TouchDeliveryAbility = ability;
            return result;
        }

        public static AbilityDeliverTouch CreateAbilityDeliverTouch()
        {
            var result = new AbilityDeliverTouch();
            result.m_TouchWeapon = ToRef<BlueprintItemWeaponReference>("bb337517547de1a4189518d404ec49d4");
            return result;
        }

        public static AddAreaEffect CreateAddAreaEffect(this BlueprintAbilityAreaEffect area)
        {
            var result = new AddAreaEffect();
            result.m_AreaEffect = area.ToReference<BlueprintAbilityAreaEffectReference>();
            return result;
        }

        public static CombatStateTrigger CreateCombatStateTrigger(GameAction start = null, GameAction end = null)
        {
            var result = new CombatStateTrigger();
            result.CombatStartActions = CreateActionList(start);
            result.CombatEndActions = CreateActionList(start);
            return result;
        }

        public static AutoMetamagic CreateAutoMetamagic(Metamagic metamagic, List<BlueprintAbilityReference> abilities = null, AutoMetamagic.AllowedType type = AutoMetamagic.AllowedType.Any)
        {
            var result = new AutoMetamagic();
            result.m_AllowedAbilities = type;
            result.Metamagic = metamagic;
            result.Abilities = abilities ?? new List<BlueprintAbilityReference>();
            return result;
        }

        public static AddFactContextActions CreateAddFactContextActions(GameAction[] on = null, GameAction[] off = null, GameAction[] round = null)
        {
            var result = new AddFactContextActions();
            result.Activated = CreateActionList(on);
            result.Deactivated = CreateActionList(off);
            result.NewRound = CreateActionList(round);
            return result;
        }

        public static ContextActionCastSpell CreateContextActionCastSpell(BlueprintAbilityReference spell, bool castByTarget = false)
        {
            var result = new ContextActionCastSpell();
            result.m_Spell = spell;
            result.CastByTarget = castByTarget;
            return result;
        }

        public static ContextActionMeleeAttack CreateContextActionMeleeAttack(bool isPoint = false)
        {
            var result = new ContextActionMeleeAttack();
            result.SelectNewTarget = isPoint;
            result.AutoHit = false;
            result.IgnoreStatBonus = false;
            result.AutoCritThreat = false;
            result.AutoCritConfirmation = false;
            return result;
        }

        public static ContextActionAddFeature CreateContextActionAddFeature(BlueprintFeatureReference permanent)
        {
            var result = new ContextActionAddFeature();
            result.m_PermanentFeature = permanent;
            return result;
        }

        /// <summary>Adds a fact, but only fact not already granted through other means.</summary>
        public static AddFeatureIfHasFact CreateAddFeatureIfHasFact(AnyRef feature)
        {
            var result = new AddFeatureIfHasFact();
            result.m_CheckedFact = feature;
            result.m_Feature = feature;
            result.Not = true;
            return result;
        }

        public static AddFeatureIfHasFact CreateAddFeatureIfHasFact(AnyRef check_BlueprintUnitFact, AnyRef blueprintUnitFact, bool not = false)
        {
            var result = new AddFeatureIfHasFact();
            result.m_CheckedFact = check_BlueprintUnitFact;
            result.m_Feature = blueprintUnitFact;
            result.Not = not;
            return result;
        }

        public static AddStatBonus CreateAddStatBonus(int value, StatType stat, ModifierDescriptor descriptor = ModifierDescriptor.UntypedStackable)
        {
            var result = new AddStatBonus();
            result.Value = value;
            result.Stat = stat;
            result.Descriptor = descriptor;
            return result;
        }

        public static AddContextStatBonus CreateAddContextStatBonus(ContextValue value, StatType stat, ModifierDescriptor descriptor = ModifierDescriptor.UntypedStackable)
        {
            var result = new AddContextStatBonus();
            result.Value = value;
            result.Stat = stat;
            result.Descriptor = descriptor;
            return result;
        }

        public static AddInitiatorAttackRollTrigger CreateAddInitiatorAttackRollTrigger(ActionList Action, bool OnOwner = false, bool SneakAttack = false, bool OnlyHit = true, bool CriticalHit = false, bool CheckWeapon = false, WeaponCategory WeaponCategory = 0)
        {
            var result = new AddInitiatorAttackRollTrigger();
            result.Action = Action;
            result.OnOwner = OnOwner;
            result.SneakAttack = SneakAttack;
            result.OnlyHit = OnlyHit;
            result.CriticalHit = CriticalHit;
            result.CheckWeapon = CheckWeapon;
            result.WeaponCategory = WeaponCategory;
            return result;
        }

        public static AddInitiatorAttackWithWeaponTrigger CreateAddInitiatorAttackWithWeaponTrigger(ActionList Action, bool OnlyHit = true, bool OnlySneakAttack = false, bool OnMiss = false, bool OnlyOnFullAttack = false, bool OnlyOnFirstAttack = false, bool OnlyOnFirstHit = false, bool CriticalHit = false, bool OnAttackOfOpportunity = false, bool NotCriticalHit = false, bool NotSneakAttack = false, bool ActionsOnInitiator = false, bool ReduceHPToZero = false, bool DamageMoreTargetMaxHP = false, bool AllNaturalAndUnarmed = false, bool DuelistWeapon = false, bool NotExtraAttack = false, bool OnCharge = false, bool IgnoreAutoHit = false, BlueprintWeaponTypeReference m_WeaponType = null, WeaponCategory? WeaponCategory = null, WeaponFighterGroup? WeaponFighterGroup = null, WeaponRangeType? WeaponRangeType = null, Feet? DistanceLessEqual = null)
        {
            var result = new AddInitiatorAttackWithWeaponTrigger();
            result.Action = Action;
            result.OnlyHit = OnlyHit;
            result.OnlySneakAttack = OnlySneakAttack;
            result.OnMiss = OnMiss;
            result.OnlyOnFullAttack = OnlyOnFullAttack;
            result.OnlyOnFirstAttack = OnlyOnFirstAttack;
            result.OnlyOnFirstHit = OnlyOnFirstHit;
            result.CriticalHit = CriticalHit;
            result.OnAttackOfOpportunity = OnAttackOfOpportunity;
            result.NotCriticalHit = NotCriticalHit;
            result.NotSneakAttack = NotSneakAttack;
            result.ActionsOnInitiator = ActionsOnInitiator;
            result.ReduceHPToZero = ReduceHPToZero;
            result.DamageMoreTargetMaxHP = DamageMoreTargetMaxHP;
            result.AllNaturalAndUnarmed = AllNaturalAndUnarmed;
            result.DuelistWeapon = DuelistWeapon;
            result.NotExtraAttack = NotExtraAttack;
            result.OnCharge = OnCharge;
            result.IgnoreAutoHit = IgnoreAutoHit;
            result.m_WeaponType = m_WeaponType;
            result.CheckWeaponCategory = WeaponCategory != null;
            result.Category = WeaponCategory.GetValueOrDefault();
            result.CheckWeaponGroup = WeaponFighterGroup != null;
            result.Group = WeaponFighterGroup.GetValueOrDefault();
            result.CheckWeaponRangeType = WeaponRangeType != null;
            result.RangeType = WeaponRangeType.GetValueOrDefault();
            result.CheckDistance = DistanceLessEqual != null;
            result.DistanceLessEqual = DistanceLessEqual.GetValueOrDefault();

            return result;
        }

        public static PrerequisiteFullStatValue CreatePrerequisiteFullStatValue(StatType stat, int value = 0)
        {
            var result = new PrerequisiteFullStatValue();
            result.Stat = stat;
            result.Value = value;
            return result;
        }

        public static AddCondition CreateAddCondition(UnitCondition condition)
        {
            return new AddCondition
            {
                Condition = condition
            };
        }

        public static AddConditionExceptions CreateAddConditionExceptions(UnitCondition condition, params BlueprintBuffReference[] buffs)
        {
            var result = new AddConditionExceptions();
            result.Condition = condition;
            result.Exception = new UnitConditionExceptionsFromBuff { Exceptions = buffs };
            return result;
        }

        public static SpellDescriptorComponent CreateSpellDescriptorComponent(SpellDescriptor descriptor)
        {
            return new SpellDescriptorComponent { Descriptor = descriptor };
        }

        public static SpellImmunityToSpellDescriptor CreateSpellImmunityToSpellDescriptor(SpellDescriptor descriptor, BlueprintUnitFactReference ignoreFact = null)
        {
            var result = new SpellImmunityToSpellDescriptor();
            result.Descriptor = descriptor;
            result.m_CasterIgnoreImmunityFact = ignoreFact;
            return result;
        }

        public static AddMechanicsFeature CreateAddMechanicsFeature(AddMechanicsFeature.MechanicsFeatureType feature)
        {
            var result = new AddMechanicsFeature();
            result.m_Feature = feature;
            return result;
        }

        public static AddMechanicFeatureCustom CreateAddMechanicsFeature(MechanicFeature feature)
        {
            return new AddMechanicFeatureCustom(feature);
        }

        public static AbilityTargetHasFact CreateAbilityTargetHasFact(bool inverted, params BlueprintUnitFactReference[] facts)
        {
            var result = new AbilityTargetHasFact();
            result.Inverted = inverted;
            result.m_CheckedFacts = facts;
            return result;
        }

        public static ContextRankConfig CreateContextRankConfig(ContextRankBaseValueType baseValueType, ContextRankProgression progression = ContextRankProgression.AsIs, AbilityRankType type = AbilityRankType.Default, int? min = null, int? max = null, int startLevel = 0, int stepLevel = 0, bool exceptClasses = false, StatType stat = StatType.Unknown, BlueprintUnitPropertyReference customProperty = null, BlueprintCharacterClassReference[] classes = null, BlueprintArchetypeReference[] archetypes = null, BlueprintFeatureReference feature = null, BlueprintFeatureReference[] featureList = null/*, (int, int)[] customProgression = null*/)
        {
            var result = new ContextRankConfig();
            result.m_Type = type;
            result.m_BaseValueType = baseValueType;
            result.m_Progression = progression;
            result.m_UseMin = min.HasValue;
            result.m_Min = min ?? 0;
            result.m_UseMax = max.HasValue;
            result.m_Max = max ?? 20;
            result.m_StartLevel = startLevel;
            result.m_StepLevel = stepLevel;
            result.m_Feature = feature;
            result.m_CustomProperty = customProperty;
            result.m_Stat = stat;
            result.m_Class = classes ?? Array.Empty<BlueprintCharacterClassReference>();
            result.Archetype = ToRef<BlueprintArchetypeReference>("");
            result.m_AdditionalArchetypes = archetypes ?? Array.Empty<BlueprintArchetypeReference>();
            result.m_FeatureList = featureList ?? Array.Empty<BlueprintFeatureReference>();

            return result;
        }

        public static ContextCalculateSharedValue CreateContextCalculateSharedValue(AbilitySharedValue ValueType = AbilitySharedValue.Damage, ContextDiceValue Value = null, double Modifier = 1.0)
        {
            if (Value == null)
                Value = CreateContextDiceValue(DiceType.One, AbilityRankType.DamageDice, AbilityRankType.DamageBonus);

            var result = new ContextCalculateSharedValue();
            result.ValueType = ValueType;
            result.Value = Value;
            result.Modifier = Modifier;
            return result;
        }

        public static ContextDiceValue CreateContextDiceValue(DiceType dice, ContextValue diceCount = null, ContextValue bonus = null)
        {
            return new ContextDiceValue()
            {
                DiceType = dice,
                DiceCountValue = diceCount ?? CreateContextValue(),
                BonusValue = bonus ?? 0
            };
        }

        public static ContextDiceValue CreateContextDiceValue(DiceType dice, AbilityRankType dicecount, AbilityRankType bonus)
        {
            return new ContextDiceValue()
            {
                DiceType = dice,
                DiceCountValue = CreateContextValue(dicecount),
                BonusValue = CreateContextValue(bonus)
            };
        }

        public static ContextActionDealDamage CreateContextActionDealDamage(PhysicalDamageForm physical, ContextDiceValue damage, bool isAoE = false, bool halfIfSaved = false, bool IgnoreCritical = false, bool half = false, bool alreadyHalved = false, AbilitySharedValue sharedValue = 0, bool readShare = false, bool writeShare = false)
        {
            // physical damage
            var c = new ContextActionDealDamage();
            c.DamageType = new DamageTypeDescription()
            {
                Type = DamageType.Physical,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData() { Form = physical }
            };
            c.Duration = CreateContextDurationValue();
            c.Value = damage;
            c.IsAoE = isAoE;
            c.HalfIfSaved = halfIfSaved;
            c.IgnoreCritical = IgnoreCritical;
            c.Half = half;
            c.AlreadyHalved = alreadyHalved;
            c.ReadPreRolledFromSharedValue = readShare;
            c.PreRolledSharedValue = readShare ? sharedValue : 0;
            c.WriteResultToSharedValue = writeShare;
            c.ResultSharedValue = writeShare ? sharedValue : 0;
            return c;
        }

        public static ContextActionDealDamage CreateContextActionDealDamage(DamageEnergyType energy, ContextDiceValue damage, bool isAoE = false, bool halfIfSaved = false, bool IgnoreCritical = false, bool half = false, bool alreadyHalved = false, AbilitySharedValue sharedValue = 0, bool readShare = false, bool writeShare = false)
        {
            // energy damage
            var c = new ContextActionDealDamage();
            c.DamageType = new DamageTypeDescription()
            {
                Type = DamageType.Energy,
                Energy = energy,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData()
            };
            c.Duration = CreateContextDurationValue();
            c.Value = damage;
            c.IsAoE = isAoE;
            c.HalfIfSaved = halfIfSaved;
            c.IgnoreCritical = IgnoreCritical;
            c.Half = half;
            c.AlreadyHalved = alreadyHalved;
            c.ReadPreRolledFromSharedValue = readShare;
            c.PreRolledSharedValue = readShare ? sharedValue : 0;
            c.WriteResultToSharedValue = writeShare;
            c.ResultSharedValue = writeShare ? sharedValue : 0;
            return c;
        }

        public static ContextActionDealDamage CreateContextActionDealForceDamage(ContextDiceValue damage, bool isAoE = false, bool halfIfSaved = false, bool IgnoreCritical = false, bool half = false, bool alreadyHalved = false, AbilitySharedValue sharedValue = 0, bool readShare = false, bool writeShare = false)
        {
            // Force damage
            var c = new ContextActionDealDamage();
            c.DamageType = new DamageTypeDescription()
            {
                Type = DamageType.Force,
                Energy = DamageEnergyType.Fire,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData()
            };
            c.Duration = CreateContextDurationValue();
            c.Value = damage;
            c.IsAoE = isAoE;
            c.HalfIfSaved = halfIfSaved;
            c.IgnoreCritical = IgnoreCritical;
            c.Half = half;
            c.AlreadyHalved = alreadyHalved;
            c.ReadPreRolledFromSharedValue = readShare;
            c.PreRolledSharedValue = readShare ? sharedValue : 0;
            c.WriteResultToSharedValue = writeShare;
            c.ResultSharedValue = writeShare ? sharedValue : 0;
            return c;
        }

        public static ContextActionChangeSharedValue CreateContextActionChangeSharedValue(AbilitySharedValue sharedValue, ContextValue add = null, ContextValue set = null, ContextValue mult = null)
        {
            var result = new ContextActionChangeSharedValue();

            result.SharedValue = sharedValue;

            if (add != null)
            {
                result.AddValue = add;
                result.Type = SharedValueChangeType.Add;
            }

            if (set != null)
            {
                if (result.Type != 0) throw new ArgumentException();
                result.AddValue = add;
                result.Type = SharedValueChangeType.Set;
            }

            if (mult != null)
            {
                if (result.Type != 0) throw new ArgumentException();
                result.MultiplyValue = add;
                result.Type = SharedValueChangeType.Multiply;
            }

            return result;
        }

        public static ContextConditionSharedValueHigher CreateContextConditionSharedValueHigher(AbilitySharedValue sharedValue, int value, Equality equality = Equality.HigherOrEqual)
        {
            var result = new ContextConditionSharedValueHigher();

            result.SharedValue = sharedValue;

            switch (equality)
            {
                case Equality.HigherThan:
                    result.HigherOrEqual = value + 1;
                    break;
                case Equality.HigherOrEqual:
                    result.HigherOrEqual = value;
                    break;
                case Equality.LowerThan:
                    result.HigherOrEqual = value - 1;
                    result.Inverted = true;
                    break;
                case Equality.LowerOrEqual:
                    result.HigherOrEqual = value;
                    result.Inverted = true;
                    break;
                default:
                    throw new ArgumentException();
            }

            return result;
        }

        public static SpellComponent CreateSpellComponent(SpellSchool school)
        {
            var result = new SpellComponent();
            result.School = school;
            return result;
        }

        public static CraftInfoComponent CreateCraftInfoComponent()
        {
            var result = new CraftInfoComponent();
            result.SpellType = CraftSpellType.Other;
            result.SavingThrow = CraftSavingThrow.None;
            result.AOEType = CraftAOE.None;
            return result;
        }

        public static SpellListComponent CreateSpellListComponent(BlueprintSpellListReference spellList, int spellLevel)
        {
            var result = new SpellListComponent();
            result.m_SpellList = spellList;
            result.SpellLevel = spellLevel;
            return result;
        }

        public static AbilityCasterHasFacts CreateAbilityCasterHasFacts(bool NeedsAll = false, params BlueprintUnitFactReference[] Facts)
        {
            var result = new AbilityCasterHasFacts();
            result.m_Facts = Facts;
            result.NeedsAll = NeedsAll;
            return result;
        }

        public static AbilityDeliverProjectile CreateAbilityDeliverProjectile(BlueprintProjectileReference projectile, AbilityProjectileType type = AbilityProjectileType.Simple, BlueprintItemWeaponReference weapon = null, Feet length = default, Feet width = default)
        {
            var result = new AbilityDeliverProjectile();
            result.m_Projectiles = projectile.ObjToArray();
            result.Type = type;
            result.m_Length = length;
            result.m_LineWidth = width;
            result.m_Weapon = weapon;
            result.Type = AbilityProjectileType.Line;
            result.NeedAttackRoll = weapon != null;
            return result;
        }

        public static AbilityShowIfCasterHasFact CreateAbilityShowIfCasterHasFact(BlueprintUnitFactReference UnitFact)
        {
            var result = new AbilityShowIfCasterHasFact();
            result.m_UnitFact = UnitFact;
            return result;
        }

        /// <param name="UnitFact">type: <b>BlueprintUnitFact</b></param>
        public static AbilityShowIfCasterHasFact CreateAbilityShowIfCasterHasFact(AnyRef UnitFact, bool not = false)
        {
            var result = new AbilityShowIfCasterHasFact();
            result.m_UnitFact = UnitFact;
            result.Not = not;
            return result;
        }

        public static AbilityRequirementActionAvailable CreateAbilityRequirementActionAvailable(bool Not, ActionType Action, float Amount = 3f)
        {
            var result = new AbilityRequirementActionAvailable();
            result.Not = Not;
            result.Action = Action;
            result.Amount = Amount;
            return result;
        }

        public static AbilityEffectRunAction CreateAbilityEffectRunAction(SavingThrowType save = SavingThrowType.Unknown, params GameAction[] actions)
        {
            if (actions == null || actions[0] == null) throw new ArgumentNullException();
            var result = new AbilityEffectRunAction();
            result.SavingThrowType = save;
            if (save == SavingThrowType.Unknown)
                result.Actions = CreateActionList(actions);
            else
                result.Actions = CreateActionList(CreateContextActionConditionalSaved(failed: actions));
            return result;
        }

        public static AbilityEffectRunAction CreateAbilityEffectRunAction(SavingThrowType save = SavingThrowType.Unknown, Condition[] condition = null, GameAction[] ifTrue = null, GameAction[] ifFalse = null)
        {
            var result = new AbilityEffectRunAction();
            result.SavingThrowType = save;
            if (condition != null)
                result.Actions = CreateActionList(CreateConditional(condition, ifTrue, ifFalse));
            else if (save != SavingThrowType.Unknown)
                result.Actions = CreateActionList(CreateContextActionConditionalSaved(succeed: ifTrue, failed: ifFalse));
            else
                throw new ArgumentNullException();

            return result;
        }

        public static AbilityExecuteActionOnCast CreateAbilityExecuteActionOnCast(params GameAction[] actions)
        {
            var result = new AbilityExecuteActionOnCast();
            result.Actions = CreateActionList(actions);
            result.Conditions = new ConditionsChecker() { Operation = Operation.And, Conditions = Array.Empty<Condition>() };
            return result;
        }

        public static AbilityExecuteActionOnCast CreateAbilityExecuteActionOnCast(GameAction[] actions, Condition[] conditions = null, Operation operation = Operation.And)
        {
            var result = new AbilityExecuteActionOnCast();
            result.Actions = CreateActionList(actions);
            result.Conditions = new ConditionsChecker() { Operation = operation, Conditions = conditions ?? Array.Empty<Condition>() };
            return result;
        }

        public static ConditionsChecker CreateConditionsChecker(Operation operation = Operation.And, params Condition[] conditions)
        {
            var result = new ConditionsChecker() { Operation = operation, Conditions = conditions ?? Array.Empty<Condition>() };
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Events.ActivateTrigger CreateActivateTrigger(ConditionsChecker conditions, GameAction[] actions, bool once = false, bool onAreaLoad = false)
        {
            var result = new Kingmaker.Designers.EventConditionActionSystem.Events.ActivateTrigger();
            result.Conditions = conditions;
            result.Actions = CreateActionList(actions);
            result.m_Once = once;
            result.m_AlsoOnAreaLoad = onAreaLoad;
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.HasFact CreateHasFact(UnitEvaluator unit, BlueprintUnitFactReference fact)
        {
            var result = new Kingmaker.Designers.EventConditionActionSystem.Conditions.HasFact();
            result.Unit = unit;
            result.m_Fact = fact;
            return result;
        }

        public static AddFact CreateAddFact(UnitEvaluator unit, BlueprintUnitFactReference fact)
        {
            var result = new AddFact();
            result.Unit = unit;
            result.m_Fact = fact;
            return result;
        }

        public static ContextActionRemoveBuff CreateContextActionRemoveBuff(BlueprintBuff buff, bool toCaster = false)
        {
            var result = new ContextActionRemoveBuff();
            result.m_Buff = buff.ToRef();
            result.ToCaster = toCaster;
            return result;
        }

        public static ContextConditionHasBuff CreateContextConditionHasBuff(this BlueprintBuff buff)
        {
            var hasBuff = new ContextConditionHasBuff();
            hasBuff.m_Buff = buff.ToRef();
            return hasBuff;
        }

        public static AbilityAcceptBurnOnCast CreateAbilityAcceptBurnOnCast(int burnValue)
        {
            var result = new AbilityAcceptBurnOnCast();
            result.BurnValue = burnValue;
            return result;
        }

        public static RecalculateOnFactsChange CreateRecalculateOnFactsChange(params BlueprintUnitFactReference[] facts)
        {
            var result = new RecalculateOnFactsChange();
            result.m_CheckedFacts = facts;
            return result;
        }

        public static ContextConditionHasFact CreateContextConditionHasFact(AnyRef fact, bool not = false)
        {
            var result = new ContextConditionHasFact();
            result.m_Fact = fact;
            result.Not = not;
            return result;
        }

        public static ContextConditionHasFactRank CreateContextConditionHasFactRank(BlueprintUnitFactReference fact, ContextValue rank)
        {
            var result = new ContextConditionHasFactRank();
            result.Fact = fact;
            result.RankValue = rank;
            return result;
        }

        public static ContextConditionCasterHasFact CreateContextConditionCasterHasFact(BlueprintUnitFactReference fact)
        {
            var result = new ContextConditionCasterHasFact();
            result.m_Fact = fact;
            return result;
        }

        public static ActionList CreateActionList(params GameAction[] actions)
        {
            if (actions == null || actions.Length == 1 && actions[0] == null) actions = Array.Empty<GameAction>();
            return new ActionList() { Actions = actions };
        }

        public static Conditional CreateConditional(Condition condition, GameAction ifTrue = null, GameAction ifFalse = null, bool OperationAnd = true)
        {
            var c = new Conditional();
            c.ConditionsChecker = new ConditionsChecker() { Conditions = condition.ObjToArray(), Operation = OperationAnd ? Operation.And : Operation.Or };
            c.IfTrue = CreateActionList(ifTrue);
            c.IfFalse = CreateActionList(ifFalse);
            return c;
        }

        public static Conditional CreateConditional(Condition[] condition, GameAction[] ifTrue = null, GameAction[] ifFalse = null, bool OperationAnd = true)
        {
            var c = new Conditional();
            c.ConditionsChecker = new ConditionsChecker() { Conditions = condition, Operation = OperationAnd ? Operation.And : Operation.Or };
            c.IfTrue = CreateActionList(ifTrue);
            c.IfFalse = CreateActionList(ifFalse);
            return c;
        }

        public static ContextActionConditionalSaved CreateContextActionConditionalSaved(GameAction succeed = null, GameAction failed = null)
        {
            var result = new ContextActionConditionalSaved();
            result.Succeed = CreateActionList(succeed);
            result.Failed = CreateActionList(failed);
            return result;
        }

        public static ContextActionConditionalSaved CreateContextActionConditionalSaved(GameAction[] succeed = null, GameAction[] failed = null)
        {
            var result = new ContextActionConditionalSaved();
            result.Succeed = CreateActionList(succeed);
            result.Failed = CreateActionList(failed);
            return result;
        }

        public static AbilityRequirementHasBuffs CreateAbilityRequirementHasBuffs(bool Not, params BlueprintBuff[] Buffs)
        {
            var result = new AbilityRequirementHasBuffs();
            result.Not = Not;
            result.Buffs = Buffs;
            return result;
        }

        public static AbilityRequirementHasBuffTimed CreateAbilityRequirementHasBuffTimed(CompareType Compare, TimeSpan TimeLeft, params BlueprintBuff[] Buffs)
        {
            var result = new AbilityRequirementHasBuffTimed();
            result.Compare = Compare;
            result.Buffs = Buffs;
            result.TimeLeft = TimeLeft;
            return result;
        }

        public static ContextActionApplyBuff CreateContextActionApplyBuff(this BlueprintBuff buff, float duration, DurationRate rate = DurationRate.Rounds, bool fromSpell = false, bool dispellable = false, bool toCaster = false, bool asChild = false, bool permanent = false)
        {
            var result = new ContextActionApplyBuff();
            result.m_Buff = buff.ToRef();
            result.UseDurationSeconds = true;
            result.DurationSeconds = duration;
            result.IsFromSpell = fromSpell;
            result.IsNotDispelable = !dispellable;
            result.ToCaster = toCaster;
            result.AsChild = asChild;
            result.Permanent = permanent;
            return result;
        }

        public static ContextActionApplyBuff CreateContextActionApplyBuff(this BlueprintBuff buff, int duration = 0, DurationRate rate = DurationRate.Rounds, bool fromSpell = false, bool dispellable = false, bool toCaster = false, bool asChild = false, bool permanent = false)
        {
            return CreateContextActionApplyBuff(buff, CreateContextDurationValue(bonus: duration, rate: rate), fromSpell: fromSpell, toCaster: toCaster, asChild: asChild, dispellable: dispellable, permanent: permanent);
        }

        public static ContextActionApplyBuff CreateContextActionApplyBuff(this BlueprintBuff buff, ContextDurationValue duration, bool fromSpell = false, bool dispellable = true, bool toCaster = false, bool asChild = false, bool permanent = false)
        {
            var result = new ContextActionApplyBuff();
            result.m_Buff = buff.ToRef();
            result.DurationValue = duration;
            result.IsFromSpell = fromSpell;
            result.IsNotDispelable = !dispellable;
            result.ToCaster = toCaster;
            result.AsChild = asChild;
            result.Permanent = permanent;
            return result;
        }

        /// <param name="buff">type: <b>BlueprintBuff</b></param>
        public static ContextActionApplyBuff CreateContextActionApplyBuff(AnyRef buff, ContextDurationValue duration = null, bool fromSpell = false, bool dispellable = true, bool toCaster = false, bool asChild = false)
        {
            var result = new ContextActionApplyBuff();
            result.m_Buff = buff;
            result.DurationValue = duration;
            result.IsFromSpell = fromSpell;
            result.IsNotDispelable = !dispellable;
            result.ToCaster = toCaster;
            result.AsChild = asChild;
            result.Permanent = duration == null;
            return result;
        }

        public static ContextActionApplyBuff CreateContextActionApplyBuff(this BlueprintBuff buff, float durationInSeconds, bool fromSpell = false, bool dispellable = true, bool toCaster = false, bool asChild = false, bool permanent = false)
        {
            var result = new ContextActionApplyBuff();
            result.m_Buff = buff.ToRef();
            result.UseDurationSeconds = true;
            result.DurationSeconds = durationInSeconds;
            result.IsFromSpell = fromSpell;
            result.IsNotDispelable = !dispellable;
            result.ToCaster = toCaster;
            result.AsChild = asChild;
            result.Permanent = permanent;
            return result;
        }

        public static LevelEntry CreateLevelEntry(int level, params AnyRef[] blueprintFeatureBase)
        {
            var result = new LevelEntry();
            result.Level = level;
            result.m_Features = blueprintFeatureBase.ToRef<BlueprintFeatureBaseReference>().ToList();
            return result;
        }

        public static PrerequisiteMythicLevel CreatePrerequisiteMythicLevel(int level)
        {
            var result = new PrerequisiteMythicLevel();
            result.Level = level;
            return result;
        }

        public static ClassLevelsForPrerequisites CreateClassLevelsForPrerequisites(BlueprintCharacterClassReference target_class, int bonus = 0, BlueprintCharacterClassReference source_class = null, double multiplier = 0d)
        {
            var result = new ClassLevelsForPrerequisites();
            result.m_FakeClass = target_class;
            result.m_ActualClass = source_class ?? target_class;
            result.Summand = bonus;
            result.Modifier = multiplier;
            return result;
        }

        public static PrerequisiteFeaturesFromList CreatePrerequisiteFeaturesFromList(bool any, params BlueprintFeatureReference[] features)
        {
            var result = new PrerequisiteFeaturesFromList();
            result.m_Features = features;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            result.Amount = 1;
            return result;
        }

        public static PrerequisiteFeaturesFromList CreatePrerequisiteFeaturesFromList(IEnumerable<AnyRef> blueprintFeature = null, int amount = 1, bool any = false)
        {
            var result = new PrerequisiteFeaturesFromList();
            result.m_Features = blueprintFeature?.ToRef<BlueprintFeatureReference>();
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            result.Amount = blueprintFeature == null ? 0 : amount;
            return result;
        }

        public static PrerequisiteStatValue CreatePrerequisiteStatValue(StatType statType, int value, bool any = false)
        {
            var result = new PrerequisiteStatValue();
            result.Stat = statType;
            result.Value = value;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }

        /// <param name="feat">type: <b>BlueprintFeature</b></param>
        public static PrerequisiteFeature CreatePrerequisiteFeature(this AnyRef feat, bool any = false)
        {
            var result = new PrerequisiteFeature();
            result.m_Feature = feat;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }

        public static PrerequisiteNoFeature CreatePrerequisiteNoFeature(this BlueprintFeatureReference feat, bool any = false)
        {
            var result = new PrerequisiteNoFeature();
            result.m_Feature = feat;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }

        /// <param name="class">type: <b>BlueprintCharacterClass</b></param>
        public static PrerequisiteClassLevel CreatePrerequisiteClassLevel(AnyRef @class, int level, bool any = false) => CreatePrerequisiteClassLevel((BlueprintCharacterClassReference)@class, level, any);
        public static PrerequisiteClassLevel CreatePrerequisiteClassLevel(BlueprintCharacterClassReference @class, int level, bool any = false)
        {
            var result = new PrerequisiteClassLevel();
            result.m_CharacterClass = @class;
            result.Level = level;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }

        public static PrerequisiteArchetypeLevel CreatePrerequisiteArchetypeLevel(BlueprintArchetypeReference archetype, int level = 1, bool any = false, BlueprintCharacterClassReference characterClass = null)
        {
            var result = new PrerequisiteArchetypeLevel();
            result.m_Archetype = archetype;
            result.m_CharacterClass = characterClass ?? archetype.Get().GetParentClass().ToRef();
            result.Level = level;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }

        public static PrerequisiteNoArchetype CreatePrerequisiteNoArchetype(BlueprintArchetypeReference m_Archetype, BlueprintCharacterClassReference m_CharacterClass = null)
        {
            var result = new PrerequisiteNoArchetype();
            result.m_Archetype = m_Archetype;
            result.m_CharacterClass = m_CharacterClass ?? m_Archetype.Get().GetParentClass().ToRef();
            return result;
        }

        /// <param name="feature">type: <b>BlueprintFeature</b></param>
        /// <param name="spell">type: <b>BlueprintAbility</b></param>
        public static PrerequisiteParametrizedFeature CreatePrerequisiteParametrizedFeature(AnyRef feature, WeaponCategory weaponCategory = 0, SpellSchool spellSchool = 0, AnyRef spell = null, bool any = false)
        {
            var result = new PrerequisiteParametrizedFeature();
            result.m_Feature = feature;
            result.WeaponCategory = weaponCategory;
            result.SpellSchool = spellSchool;
            result.m_Spell = spell;
            result.ParameterType = spell != null ? FeatureParameterType.SpellSpecialization : spellSchool != 0 ? FeatureParameterType.SpellSpecialization : FeatureParameterType.WeaponCategory;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }

        public static AddFacts CreateAddFacts(params BlueprintUnitFactReference[] facts)
        {
            var result = new AddFacts();
            result.m_Facts = facts;
            return result;
        }

        public static AddFacts CreateAddFacts(params AnyRef[] facts)
        {
            var result = new AddFacts();
            result.m_Facts = facts.ToRef<BlueprintUnitFactReference>();
            return result;
        }

        public static AddFacts CreateAddFacts(IEnumerable<BlueprintUnitFactReference> facts) => CreateAddFacts(facts.ToArray());

        /// <param name="blueprintFeature">type: <b>BlueprintFeature</b></param>
        public static AddFeatureOnApply CreateAddFeatureOnApply(AnyRef blueprintFeature)
        {
            var result = new AddFeatureOnApply();
            result.m_Feature = blueprintFeature;
            return result;
        }

        public static DuplicateSpell CreateDuplicateSpell(Func<AbilityData, bool> abilityCheck, int radius = 30)
        {
            return new DuplicateSpell
            {
                Radius = radius,
                AbilityCheck = abilityCheck
            };
        }

        public static LevelUpAddSelectionHasFact CreateLevelUpAddSelectionHasFact(BlueprintUnitFact[] facts, IFeatureSelection selection)
        {
            var result = new LevelUpAddSelectionHasFact();
            result.Facts = facts;
            result.Selection = selection; // takes BlueprintFeatureSelection or BlueprintParametrizedFeature
            return result;
        }

        public static LearnSpellList CreateLearnSpellList(BlueprintSpellListReference spellList, BlueprintCharacterClassReference characterClass = null, BlueprintArchetypeReference archetype = null)
        {
            var result = new LearnSpellList();
            result.m_SpellList = spellList;
            result.m_CharacterClass = characterClass ?? archetype.Get().GetParentClass().ToRef();
            result.m_Archetype = archetype;
            return result;
        }

        public static AddKnownSpell CreateAddKnownSpell(BlueprintAbilityReference spell, int SpellLevel, BlueprintCharacterClassReference characterClass = null, BlueprintArchetypeReference archetype = null)
        {
            var result = new AddKnownSpell(); //AddSpellKnownTemporary
            result.m_Spell = spell;
            result.SpellLevel = SpellLevel;
            result.m_CharacterClass = characterClass ?? archetype.Get().GetParentClass().ToRef();
            result.m_Archetype = archetype;
            return result;
        }

        public static BlueprintSpellList CreateBlueprintSpellList()
        {
            throw new NotImplementedException();
            //var result = new BlueprintSpellList();
            //return result;
        }

        public static BlueprintBuff CreateBlueprintBuff(string name, string displayName = null, string description = null, Sprite icon = null, PrefabLink fxOnStart = null)
        {
            string guid = GetGuid(name);

            var result = new BlueprintBuff();
            result.name = name;
            result.m_DisplayName = displayName.CreateString();
            result.m_Description = description.CreateString();
            result.m_Icon = icon;
            result.FxOnStart = fxOnStart ?? new PrefabLink();
            result.FxOnRemove = new PrefabLink();
            result.IsClassFeature = true;

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintFeature CreateBlueprintFeature(string name, string displayName = null, string description = null, Sprite icon = null, FeatureGroup group = 0)
        {
            string guid = GetGuid(name);

            var result = new BlueprintFeature();
            result.IsClassFeature = true;
            result.name = name;
            result.m_DisplayName = displayName.CreateString();
            result.m_Description = description.CreateString();
            result.m_Icon = icon;

            if (group == 0)
                result.Groups = Array.Empty<FeatureGroup>();
            else if (group == FeatureGroup.WizardFeat)
                result.Groups = ToArray(FeatureGroup.WizardFeat, FeatureGroup.Feat);
            else if (group == FeatureGroup.CombatFeat)
                result.Groups = ToArray(FeatureGroup.CombatFeat, FeatureGroup.Feat);
            else
                result.Groups = ToArray(group);

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintAbility CreateBlueprintAbility(string name, string displayName = null, string description = null, Sprite icon = null, AbilityType type = AbilityType.Supernatural, CommandType actionType = CommandType.Standard, AbilityRange range = AbilityRange.Close, LocalizedString duration = null, LocalizedString savingThrow = null)
        {
            string guid = GetGuid(name);

            var result = new BlueprintAbility();
            result.name = name;
            result.m_DisplayName = displayName.CreateString();
            result.m_Description = description.CreateString();
            result.m_Icon = icon;
            result.ResourceAssetIds = Array.Empty<string>();
            result.Type = type;
            result.ActionType = actionType;
            result.Range = range;
            result.LocalizedDuration = duration ?? _empty;
            result.LocalizedSavingThrow = savingThrow ?? _empty;
            result.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Extend | Metamagic.Heighten | Metamagic.Reach
                                      | Metamagic.CompletelyNormal | Metamagic.Persistent | Metamagic.Selective | Metamagic.Bolstered;
            AddAsset(result, guid);
            return result;
        }

        public static BlueprintFeatureSelection CreateBlueprintFeatureSelection(string name, string displayName = null, string description = null, Sprite icon = null, FeatureGroup group = 0, SelectionMode mode = SelectionMode.Default)
        {
            string guid = GetGuid(name);

            var result = new BlueprintFeatureSelection();
            result.IsClassFeature = true;
            result.name = name;
            result.m_DisplayName = displayName.CreateString();
            result.m_Description = description.CreateString();
            result.Groups = group == 0 ? Array.Empty<FeatureGroup>() : ToArray(group);
            result.m_Icon = icon;
            result.Mode = mode;

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintParametrizedFeature CreateBlueprintParametrizedFeature(string name, string displayName = null, string description = null, Sprite icon = null, FeatureGroup group = 0, FeatureParameterType parameterType = FeatureParameterType.Custom, bool requireKnown = false, bool requireUnknown = false, AnyBlueprintReference[] blueprints = null)
        {
            string guid = GetGuid(name);

            var result = new BlueprintParametrizedFeature();
            result.IsClassFeature = true;
            result.name = name;
            result.m_DisplayName = displayName.CreateString();
            result.m_Description = description.CreateString();
            result.m_Icon = icon;
            result.Groups = group == 0 ? Array.Empty<FeatureGroup>() : ToArray(group);
            result.ParameterType = parameterType; //FeatureParameterType.FeatureSelection
            result.BlueprintParameterVariants = blueprints ?? Array.Empty<AnyBlueprintReference>();

            result.RequireProficiency = requireKnown; // use this to require the spell to be known?
            result.HasNoSuchFeature = requireUnknown; // use this to require the spell to be unkonwn?

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintActivatableAbility CreateBlueprintActivatableAbility(string name, out BlueprintBuff buff, string displayName = null, string description = null, Sprite icon = null, CommandType commandType = CommandType.Free, AbilityActivationType activationType = AbilityActivationType.Immediately, ActivatableAbilityGroup group = ActivatableAbilityGroup.None, bool deactivateImmediately = true, bool onByDefault = false, bool onlyInCombat = false, bool deactivateEndOfCombat = false, bool deactivateAfterRound = false, bool deactivateWhenStunned = false, bool deactivateWhenDead = false, bool deactivateOnRest = false, bool useWithSpell = false, int groupWeight = 1)
        {
            string guid = GetGuid(name);

            var result = new BlueprintActivatableAbility();
            result.name = name;
            result.m_DisplayName = displayName.CreateString();
            result.m_Description = description.CreateString();
            result.m_Icon = icon;
            result.ResourceAssetIds = Array.Empty<string>();

            //result.m_Buff = buff;
            result.m_ActivateWithUnitCommand = commandType;
            result.ActivationType = activationType;
            result.DeactivateImmediately = deactivateImmediately;
            result.IsOnByDefault = onByDefault;
            result.OnlyInCombat = onlyInCombat;

            result.DeactivateIfCombatEnded = deactivateEndOfCombat;
            result.DeactivateAfterFirstRound = deactivateAfterRound;
            result.DeactivateIfOwnerDisabled = deactivateWhenStunned;
            result.DeactivateIfOwnerUnconscious = deactivateWhenDead;
            result.DoNotTurnOffOnRest = deactivateOnRest;

            result.Group = group;
            result.WeightInGroup = groupWeight; // how many resources one activation costs
            result.m_ActivateOnUnitAction = useWithSpell ? AbilityActivateOnUnitActionType.CastSpell : 0; // when spell casts costs resources

            //result.IsTargeted = ;
            //result.m_SelectTargetAbility = ;

            AddAsset(result, guid);

            // make activatable buff
            buff = new BlueprintBuff();
            buff.name = name + "_Buff";
            buff.m_DisplayName = result.m_DisplayName;
            buff.m_Description = result.m_Description;
            buff.m_Icon = icon;
            buff.FxOnStart = new PrefabLink();
            buff.FxOnRemove = new PrefabLink();
            buff.IsClassFeature = true;
            AddAsset(buff, GetGuid(buff.name));

            result.m_Buff = buff.ToRef();
            return result;
        }

        [Obsolete]
        public static BlueprintActivatableAbility CreateBlueprintActivatableAbility(string name, string displayName, string description, out BlueprintBuff buff, Sprite icon = null, CommandType commandType = CommandType.Free, AbilityActivationType activationType = AbilityActivationType.Immediately, ActivatableAbilityGroup group = ActivatableAbilityGroup.None, bool deactivateImmediately = true, bool onByDefault = false, bool onlyInCombat = false, bool deactivateEndOfCombat = false, bool deactivateAfterRound = false, bool deactivateWhenStunned = false, bool deactivateWhenDead = false, bool deactivateOnRest = false, bool useWithSpell = false, int groupWeight = 1)
        {
            return CreateBlueprintActivatableAbility(name, out buff, displayName, description, icon, commandType, activationType, group, deactivateImmediately, onByDefault, onlyInCombat, deactivateEndOfCombat, deactivateAfterRound, deactivateWhenStunned, deactivateWhenDead, deactivateOnRest, useWithSpell, groupWeight);
        }

        public static BlueprintAbilityAreaEffect CreateBlueprintAbilityAreaEffect(string name, bool applyEnemy = false, bool applyAlly = false, AreaEffectShape shape = AreaEffectShape.Cylinder, Feet size = default, PrefabLink sfx = null, BlueprintBuffReference buffWhileInside = null, ActionList unitEnter = null, ActionList unitExit = null, ActionList unitMove = null, ActionList unitRound = null)
        {
            if (!applyAlly && !applyEnemy)
                throw new ArgumentException("area must effect either allies or enemies");

            string guid = GetGuid(name);

            var result = new BlueprintAbilityAreaEffect();
            result.name = name;
            result.Shape = shape;
            result.Size = size;
            result.Fx = sfx ?? new PrefabLink();

            if (applyEnemy && applyAlly)
                result.m_TargetType = BlueprintAbilityAreaEffect.TargetType.Any;
            else if (applyEnemy)
                result.m_TargetType = BlueprintAbilityAreaEffect.TargetType.Enemy;
            else if (applyAlly)
                result.m_TargetType = BlueprintAbilityAreaEffect.TargetType.Ally;

            if (buffWhileInside != null)
            {
                AbilityAreaEffectBuff areabuff = new(); // applies buff while inside
                areabuff.Condition = new ConditionsChecker();
                areabuff.CheckConditionEveryRound = false;
                areabuff.m_Buff = buffWhileInside;

                result.SetComponents(areabuff);
            }

            if (unitEnter != null || unitExit != null || unitMove != null || unitRound != null)
            {
                AbilityAreaEffectRunAction runaction = new(); // runs actions that persist even when leaving
                runaction.UnitEnter = unitEnter ?? new ActionList();
                runaction.UnitExit = unitExit ?? new ActionList();
                runaction.UnitMove = unitMove ?? new ActionList();
                runaction.Round = unitRound ?? new ActionList();

                result.AddComponents(runaction);
            }

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintWeaponEnchantment CreateBlueprintWeaponEnchantment(string name, string enchantName = null, string description = null, string prefix = null, string suffix = null, int enchantValue = 0)
        {
            string guid = GetGuid(name);

            var result = new BlueprintWeaponEnchantment();
            result.name = name;
            result.m_EnchantName = enchantName.CreateString();
            result.m_Description = description.CreateString();
            result.m_Prefix = prefix.CreateString();
            result.m_Suffix = suffix.CreateString();
            result.m_EnchantmentCost = enchantValue;

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintUnitProperty CreateBlueprintUnitProperty(string name)
        {
            var guid = GetGuid(name);
            var result = new BlueprintUnitProperty();
            result.name = name;

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintWeaponType CreateBlueprintWeaponType(string name, string displayName, string cloneFromWeaponType, Feet range = default, DiceFormula damage = default, PhysicalDamageForm form = PhysicalDamageForm.Bludgeoning, int critRange = 20, DamageCriticalModifierType critMod = DamageCriticalModifierType.X2, WeaponFighterGroupFlags? fighterGroup = null, WeaponCategory? category = null, float? weight = null)
        {
            // note: take care to not mutate m_VisualParameters!
            string guid = GetGuid(name);
            var clone = ResourcesLibrary.TryGetBlueprint<BlueprintWeaponType>(cloneFromWeaponType);
            if (range == default)
                range = 5.Feet();
            if (damage == default)
                damage = new DiceFormula(1, DiceType.D6);

            var result = new BlueprintWeaponType();
            result.name = name;

            result.m_TypeNameText = displayName.CreateString();
            result.m_DefaultNameText = result.m_TypeNameText;
            result.m_DescriptionText = "".CreateString();
            result.m_MasterworkDescriptionText = "".CreateString();
            result.m_MagicDescriptionText = "".CreateString();

            result.m_AttackRange = range;
            result.m_BaseDamage = damage;
            result.m_DamageType = new DamageTypeDescription();
            result.m_DamageType.Physical.Form = form;
            result.m_CriticalRollEdge = critRange;
            result.m_CriticalModifier = critMod;
            result.m_FighterGroupFlags = fighterGroup ?? clone.m_FighterGroupFlags;
            result.Category = category ?? clone.Category;
            result.m_Weight = weight ?? clone.m_Weight;

            result.m_VisualParameters = clone.m_VisualParameters;
            result.m_Icon = clone.Icon;
            result.m_IsTwoHanded = clone.m_IsTwoHanded;
            result.m_IsLight = clone.m_IsLight;
            result.m_IsMonk = clone.m_IsMonk;
            result.m_IsNatural = clone.m_IsNatural;
            result.m_IsUnarmed = clone.m_IsUnarmed;
            result.m_OverrideAttackBonusStat = clone.m_OverrideAttackBonusStat;
            result.m_AttackBonusStatOverride = clone.m_AttackBonusStatOverride;
            result.m_ShardItem = clone.m_ShardItem;

            result.m_Enchantments = clone.m_Enchantments;

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintItemWeapon CreateBlueprintItemWeapon(string name, string displayName = null, string description = null, BlueprintWeaponTypeReference weaponType = null, DiceFormula? damageOverride = null, DamageTypeDescription form = null, BlueprintItemWeaponReference secondWeapon = null, bool primaryNatural = false, int price = 1000)
        {
            string guid = GetGuid(name);

            var result = new BlueprintItemWeapon();
            result.name = name;

            result.m_DisplayNameText = displayName.CreateString();
            result.m_DescriptionText = description.CreateString();
            result.m_Type = weaponType;
            result.m_Size = Size.Medium;
            result.m_OverrideDamageDice = damageOverride != null;
            result.m_DamageDice = damageOverride.GetValueOrDefault();
            result.m_OverrideDamageType = form != null;
            result.m_DamageType = form;
            result.Double = secondWeapon != null;
            result.m_SecondWeapon = secondWeapon;
            result.KeepInPolymorph = false;
            result.m_AlwaysPrimary = primaryNatural;
            result.m_Cost = price;

            // didn't needs these values so far
            result.CR = 0;
            result.m_Ability = null;
            result.m_ActivatableAbility = null;
            result.SpendCharges = false;
            result.Charges = 1;
            result.RestoreChargesOnRest = result.SpendCharges;
            result.CasterLevel = 1;
            result.SpellLevel = 1;
            result.DC = 11;
            result.m_Weight = 0;
            result.m_FlavorText = "".CreateString();
            result.m_NonIdentifiedNameText = "".CreateString();
            result.m_NonIdentifiedDescriptionText = "".CreateString();
            result.m_Icon = null; //can be null

            result.m_Enchantments = Array.Empty<BlueprintWeaponEnchantmentReference>();

            // not sure
            result.OnEnableWithLibrary();
            result.m_VisualParameters.m_Projectiles = Array.Empty<BlueprintProjectileReference>();
            result.m_VisualParameters.m_PossibleAttachSlots = Array.Empty<UnitEquipmentVisualSlotType>();

            AddAsset(result, guid);
            return result;
        }

        public static ActivatableAbilityResourceLogic CreateActivatableAbilityResourceLogic(this BlueprintAbilityResource resource, ActivatableAbilityResourceLogic.ResourceSpendType spendType = ActivatableAbilityResourceLogic.ResourceSpendType.None, AnyRef free_BlueprintUnitFact = null)
        {
            var result = new ActivatableAbilityResourceLogic();
            result.SpendType = spendType;
            result.m_FreeBlueprint = free_BlueprintUnitFact;
            result.m_RequiredResource = resource.ToReference<BlueprintAbilityResourceReference>();

            return result;
        }

        public static BlueprintAbilityResource CreateBlueprintAbilityResource(string name, string displayName = null, string description = null, Sprite icon = null, int min = 1, int max = 0,
            int baseValue = 0, int startLevel = -1, int levelMult = 1, int levelDiv = 1, float otherClasses = 0f,
            StatType stat = StatType.Unknown, int altResourcePerLevel = 0, BlueprintCharacterClassReference[] classes = null, BlueprintArchetypeReference[] archetypes = null)
        {
            string guid = GetGuid(name);

            var result = new BlueprintAbilityResource();
            result.name = name;

            result.LocalizedName = displayName.CreateString();
            result.LocalizedDescription = description.CreateString();
            result.m_Icon = icon;
            result.m_UseMax = max > 0;
            result.m_Max = max;
            result.m_Min = min;

            result.m_MaxAmount.BaseValue = baseValue;

            result.m_MaxAmount.IncreasedByLevel = altResourcePerLevel > 0;
            result.m_MaxAmount.m_Class = classes;
            result.m_MaxAmount.m_Archetypes = archetypes;
            result.m_MaxAmount.LevelIncrease = altResourcePerLevel; // simple flat bonus resource per level

            result.m_MaxAmount.IncreasedByLevelStartPlusDivStep = startLevel >= 0;
            result.m_MaxAmount.StartingLevel = startLevel;   // level the increase starts at
            result.m_MaxAmount.StartingIncrease = 0; // base value; redundant with BaseValue
            result.m_MaxAmount.LevelStep = levelDiv; // divisor
            result.m_MaxAmount.PerStepIncrease = levelMult; // level multiplicator
            result.m_MaxAmount.MinClassLevelIncrease = 0; // redundant with m_Min
            result.m_MaxAmount.m_ClassDiv = classes;
            result.m_MaxAmount.m_ArchetypesDiv = archetypes;
            result.m_MaxAmount.OtherClassesModifier = otherClasses; // amount granted from non-listed classes

            result.m_MaxAmount.IncreasedByStat = stat != StatType.Unknown;
            result.m_MaxAmount.ResourceBonusStat = stat;

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintArchetype CreateBlueprintArchetype(string name, string displayName = null, string description = null, Sprite icon = null, BlueprintSpellbookReference replaceSpellbook = null, bool removeSpellbook = false, bool isArcane = false, bool isDivine = false, int addSkillPoints = 0, BlueprintStatProgressionReference BAB = null, BlueprintStatProgressionReference Fortitute = null, BlueprintStatProgressionReference Reflex = null, BlueprintStatProgressionReference Will = null,
            StatType[] classSkills = null, StatType[] recommendStat = null, StatType[] notRecommendStat = null, AnyRef[] blueprintItem = null)
        {
            string guid = GetGuid(name);

            var result = new BlueprintArchetype();
            result.name = name;

            result.LocalizedName = displayName.CreateString();
            result.LocalizedDescription = description.CreateString();
            result.m_Icon = icon;
            result.m_ReplaceSpellbook = replaceSpellbook;
            result.RemoveSpellbook = removeSpellbook;
            result.BuildChanging = false;
            result.ChangeCasterType = isArcane || isDivine; //IsDivineCaster //IsArcaneCaster
            result.IsArcaneCaster = isArcane;
            result.IsDivineCaster = isDivine;
            result.AddSkillPoints = addSkillPoints;
            result.m_BaseAttackBonus = BAB;
            result.m_FortitudeSave = Fortitute;
            result.m_ReflexSave = Reflex;
            result.m_WillSave = Will;
            result.m_ParentClass = default;
            result.m_Difficulty = 3;

            result.ReplaceClassSkills = classSkills != null; //ClassSkills
            if (classSkills != null) result.ClassSkills = classSkills;

            result.ReplaceStartingEquipment = blueprintItem != null; //m_StartingItems //StartingGold
            result.StartingGold = 500;
            if (blueprintItem != null) result.m_StartingItems = blueprintItem.ToRef<BlueprintItemReference>();

            result.OverrideAttributeRecommendations = recommendStat != null || notRecommendStat != null; //RecommendedAttributes //NotRecommendedAttributes
            if (recommendStat != null) result.RecommendedAttributes = recommendStat;
            if (notRecommendStat != null) result.NotRecommendedAttributes = notRecommendStat;

            //result.m_SignatureAbilities = ;

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintProgression CreateBlueprintProgression(string name, string displayname = null, string description = null, Sprite icon = null, FeatureGroup group = 0)
        {
            string guid = GetGuid(name);

            var result = new BlueprintProgression();
            result.name = name;

            result.m_DisplayName = displayname.CreateString();
            result.m_Description = description.CreateString();
            result.m_Icon = icon;
            result.Groups = group == 0 ? Array.Empty<FeatureGroup>() : ToArray(group);

            result.IsClassFeature = true;

            AddAsset(result, guid);
            return result;
        }

        #endregion

        #region Library

        public static PrefabLink GetPrefabLink(string assetId)
        {
            return new PrefabLink() { AssetId = assetId };
        }

        public static T Get<T>(string guid) where T : SimpleBlueprint
        {
            var sb = ResourcesLibrary.TryGetBlueprint(BlueprintGuid.Parse(guid));

            if (sb is T bp)
                return bp;
            else if (sb == null)
                Print("ERROR: could not resolve " + guid);
            else
                Print($"ERROR: invalid conversion {sb.name} : {guid}");

            return null;
        }

        public static SimpleBlueprint Get(string guid)
        {
            return ResourcesLibrary.TryGetBlueprint(BlueprintGuid.Parse(guid));
        }

        #endregion

        #region GameActions

        // recursively search for GameActions
        public static List<T> Get<T>(this BlueprintAbility ability, Action<T> func = null, Func<T, bool> match = null) where T : GameAction
        {
            var list = ability.GetComponent<AbilityEffectRunAction>()?.Actions?.Actions.Get<T>();

            if (match != null)
                for (int i = list.Count - 1; i > 0; i--)
                    if (match.Invoke(list[i]))
                        list.RemoveAt(i);

            if (func != null)
                list.ForEach(func);

            return list;
        }
        public static List<T> Get<T>(this GameAction[] thisActions) where T : GameAction
        {
            var list = new List<T>();
            thisActions.Get<T>(ref list);
            return list;
        }
        public static void Get<T>(this GameAction[] thisActions, ref List<T> list) where T : GameAction
        {
            if (thisActions == null)
                return;

            foreach (var action in thisActions)
            {
                if (action is T x)
                    list.Add(x);

                foreach (var field in action.GetType().GetFields())
                {
                    if (field.FieldType == typeof(ActionList))
                        ((ActionList)field.GetValue(thisActions))?.Actions.Get<T>(ref list);
                }
            }
        }

        #endregion

        #region Enums

        public static void EnumCreateModifierDescriptor(ModifierDescriptor num, string name, string description)
        {
            var root = Game.Instance.BlueprintRoot.LocalizedTexts.AbilityModifiers;
            if (root.Entries.FindOrDefault(f => f.Key == num) != null)
                PrintError($"Enum ModifierDescriptor {num} already occupied!");

            var entry = new AbilityModifierEntry()
            {
                Key = num,
                Name = name.CreateString(),
                Description = description.CreateString()
            };

            AppendAndReplace(ref root.Entries, entry);
        }

        public static void EnumCreateWeaponCategory(WeaponCategory num, string name, Sprite icon = null)
        {
            //var assembly = Assembly.GetExecutingAssembly();
            //var name = assembly.GetName();
            //var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
            //var mb = ab.DefineDynamicModule(name.Name, name.Name + ".dll");
            //var eb = mb.DefineEnum("WeaponCategory", TypeAttributes.Public, typeof(int));
            //eb.DefineLiteral("ButcheringAxe", 5480);
            //var type = eb.CreateType();

            LocalizedString localized = name.CreateString();
            Patches.Patch_WeaponCategory.Extention.Add((num, localized, icon));

            var stats = LocalizedTexts.Instance.Stats;
            if (stats.m_WeaponCache == null && stats.WeaponEntries != null)
            {
                stats.m_WeaponCache = new();
                foreach (var entry in stats.WeaponEntries)
                    stats.m_WeaponCache[entry.Proficiency] = entry.Text;
            }
            else if (stats.m_WeaponCache == null)
            {
                PrintError("m_WeaponCache is null");
                return;
            }
            stats.m_WeaponCache[num] = localized;
        }

        public static DiceType ToDiceType(this int value)
        {
            switch (value)
            {
                case <= 0: return DiceType.Zero;
                case 1: return DiceType.One;
                case 2: return DiceType.D2;
                case 3: return DiceType.D3;
                case <= 5: return DiceType.D4;
                case <= 7: return DiceType.D6;
                case <= 9: return DiceType.D8;
                case <= 11: return DiceType.D10;
                case <= 19: return DiceType.D12;
                case <= 50: return DiceType.D20;
                default: return DiceType.D100;
            }
        }

        public static DamageTypeMix ToDamageTypeMix(this IEnumerable<BaseDamage> bundle)
        {
            var result = DamageTypeMix.None;
            foreach (var dmg in bundle)
            {
                if (dmg is PhysicalDamage physical)
                {
                    result |= (DamageTypeMix)(int)physical.Form;
                    result |= (DamageTypeMix)((int)physical.m_Materials << 22);
                }
                else if (dmg is EnergyDamage energy)
                    result |= (DamageTypeMix)(1 << ((int)energy.EnergyType + 3));
                else if (dmg is ForceDamage)
                    result |= DamageTypeMix.Force;
                else if (dmg is DirectDamage)
                    result |= DamageTypeMix.Direct;
                else if (dmg is UntypedDamage)
                    result |= DamageTypeMix.Untyped;
            }

            return result;
        }

        #endregion

        #region ToReference

        public static bool NotEmpty(this BlueprintReferenceBase reference)
        {
            return reference != null && reference.deserializedGuid != BlueprintGuid.Empty && reference.GetBlueprint() != null;
        }

        /// <summary>
        /// Set a specific BlueprintReference to a specific Blueprint. The main purpose is to ensure guids match.
        /// </summary>
        public static void SetReference(this BlueprintReferenceBase reference, SimpleBlueprint bp)
        {
            if (reference is null || bp is null)
            {
                var guid = bp?.AssetGuid ?? reference?.deserializedGuid;
                PrintError($"SetReference argument is null for {guid}");
                return;
            }

            if (reference.deserializedGuid != null && reference.deserializedGuid != bp.AssetGuid)
                throw new InvalidOperationException($"tried to set blueprint reference to a new guid old={reference.deserializedGuid} new={bp.AssetGuid}");

            reference.deserializedGuid = bp.AssetGuid;
            reference.Cached = bp;
        }

        private static List<object> _list = new();

        public static T ToRef<T>(this string guid) where T : BlueprintReferenceBase, new()
        {
            return new T { deserializedGuid = BlueprintGuid.Parse(guid) };
        }

        public static T ToRef<T>(this BlueprintReferenceBase reference) where T : BlueprintReferenceBase, new()
        {
            return new T { deserializedGuid = reference.deserializedGuid };
        }

        public static T ToRef<T>(this object obj) where T : BlueprintReferenceBase, new()
        {
            if (obj is T t)
                return t;
            if (obj is string str)
                return new T { deserializedGuid = BlueprintGuid.Parse(str) };
            if (obj is BlueprintReferenceBase bp)
                return new T { deserializedGuid = bp.deserializedGuid, Cached = bp.Cached };
            if (obj is SimpleBlueprint sb)
                return new T { deserializedGuid = sb.AssetGuid, Cached = sb };
            Helper.PrintError($"ToRef could not resolve type '{obj?.GetType()}'");
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ToRef<T>(params object[] source) where T : BlueprintReferenceBase, new() => ToRef<T>((IEnumerable<object>)source);

        /// <summary>
        /// Converts a collection of strings, references, and blueprints into a specified reference type.<br/>
        /// Can handle multiple different types and will recursively resolve collections.
        /// </summary>
        public static T[] ToRef<T>(this IEnumerable<object> source) where T : BlueprintReferenceBase, new()
        {
            if (source is null)
                return new T[0];

            recursiveToRef(source);

            var result = new T[_list.Count];
            _list.CopyTo(result);
            _list.Clear();
            return result;

            void recursiveToRef(IEnumerable<object> range)
            {
                T t;
                foreach (var sub in range)
                {
                    if (sub is IEnumerable<object> range2)
                        recursiveToRef(range2);
                    else if ((t = ToRef<T>(sub)) != null)
                        _list.Add(t);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnyRef ToAny(this object obj) => AnyRef.ToAny(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnyRef[] ToAny(params object[] source) => ToAny((IEnumerable<object>)source);

        /// <summary>
        /// Converts a collection of strings, references, and blueprints into a AnyRef reference type.<br/>
        /// Can handle multiple different types and will recursively resolve collections.
        /// </summary>
        public static AnyRef[] ToAny(this IEnumerable<object> source)
        {
            if (source is null)
                return new AnyRef[0];

            recursiveToAny(source);

            var result = new AnyRef[_list.Count];
            _list.CopyTo(result);
            _list.Clear();
            return result;

            void recursiveToAny(IEnumerable<object> range)
            {
                AnyRef any;
                foreach (var sub in range)
                {
                    if (sub is IEnumerable<object> range2)
                        recursiveToAny(range2);
                    else if ((any = AnyRef.ToAny(sub)) != null)
                        _list.Add(any);
                }
            }
        }

        public static IEnumerable<T> Get<T>(this IEnumerable<AnyRef> bpRef) where T : SimpleBlueprint
        {
            foreach (var reference in bpRef)
                yield return reference.Get<T>();
        }

        // note: methods below should be phased out in favor of AnyRef

        public static BlueprintUnitFactReference ToRef2(this BlueprintAbility feature)
        {
            if (feature == null) return null;
            var result = new BlueprintUnitFactReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintUnitFactReference ToRef2(this BlueprintFeature feature)
        {
            if (feature == null) return null;
            var result = new BlueprintUnitFactReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintUnitFactReference ToRef2(this BlueprintBuff feature)
        {
            if (feature == null) return null;
            var result = new BlueprintUnitFactReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }

        public static BlueprintFeatureReference ToRef(this BlueprintFeature feature)
        {
            if (feature == null) return null;
            //feature.ToReference<BlueprintFeatureReference>();
            var result = new BlueprintFeatureReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintFeatureReference[] ToRef(this BlueprintFeature[] feature)
        {
            if (feature == null) return null;
            var result = new BlueprintFeatureReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new BlueprintFeatureReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        public static BlueprintAbilityReference ToRef(this BlueprintAbility feature)
        {
            if (feature == null) return null;
            var result = new BlueprintAbilityReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintAbilityReference[] ToRef(this BlueprintAbility[] feature)
        {
            if (feature == null) return null;
            var result = new BlueprintAbilityReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new BlueprintAbilityReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        public static BlueprintCharacterClassReference ToRef(this BlueprintCharacterClass feature)
        {
            if (feature == null) return null;
            var result = new BlueprintCharacterClassReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }

        public static BlueprintFeatureBaseReference ToRef(this BlueprintFeatureBase feature)
        {
            if (feature == null) return null;
            var result = new BlueprintFeatureBaseReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }

        public static BlueprintBuffReference ToRef(this BlueprintBuff feature)
        {
            if (feature == null) return null;
            var result = new BlueprintBuffReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }

        public static BlueprintUnitFactReference ToRef(this BlueprintUnitFact feature)
        {
            if (feature == null) return null;
            var result = new BlueprintUnitFactReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }

        public static BlueprintUnitPropertyReference ToRef(this BlueprintUnitProperty feature)
        {
            if (feature == null) return null;
            var result = new BlueprintUnitPropertyReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }

        public static BlueprintArchetypeReference ToRef(this BlueprintArchetype feature)
        {
            if (feature == null) return null;
            var result = new BlueprintArchetypeReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }

        public static BlueprintWeaponEnchantmentReference ToRef(this BlueprintWeaponEnchantment feature)
        {
            if (feature == null) return null;
            var result = new BlueprintWeaponEnchantmentReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }


        #endregion

        #region Image

        public static Sprite StealIcon(string guid)
        {
            //var res = ResourcesLibrary.TryGetBlueprint(BlueprintGuid.Parse(guid));            
            //if (res is BlueprintUnitFact res1)
            try
            {
                var bp = ResourcesLibrary.TryGetBlueprint(BlueprintGuid.Parse(guid));
                if (bp is BlueprintUnitFact fact)
                    return fact.m_Icon;
                if (bp is BlueprintWeaponType weapontype)
                    return weapontype.m_Icon;
                if (bp is BlueprintBuff buff)
                    return buff.Icon;
            }
            catch (Exception)
            {
            }
            Print("Could not import icon from " + guid);
            return null;
        }

        public static Sprite CreateSprite(string path, int width = 64, int height = 64)
        {
            try
            {
                var bytes = File.ReadAllBytes(path);
                var texture = new Texture2D(width, height);
                texture.LoadImage(bytes);
                return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0, 0));
            }
            catch (Exception e)
            {
                PrintException(e);
                return null;
            }
        }

        public static Texture CreateTexture(string path, int width = 64, int height = 64)
        {
            try
            {
                var bytes = File.ReadAllBytes(path);
                var texture = new Texture2D(width, height);
                texture.LoadImage(bytes);
                return texture;
            }
            catch (Exception e)
            {
                PrintException(e);
                return null;
            }
        }

        public static SpriteLink GetSprite(string guid)
        {
            return new SpriteLink { AssetId = guid };
        }

        public static Sprite LoadSprite(string guid)
        {
            try
            {
                return new SpriteLink { AssetId = guid }.Load();
            }
            catch (Exception e)
            {
                Helper.PrintDebug($"Could not load '{guid}' {e.Message}");
                return null;
            }
        }

        #endregion
    }
}
