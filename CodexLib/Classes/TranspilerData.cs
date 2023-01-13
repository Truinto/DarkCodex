using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class TranspilerData
    {
        #region Fields

        public List<CodeInstruction> Code;
        public int Index;
        public ILGenerator Generator;
        public MethodBase Original;
        public IList<LocalVariableInfo> Locals;

        #endregion

        #region Properties

        public CodeInstruction Current => Code[Index];
        public CodeInstruction Next => Index < Code.Count - 1 ? Code[Index + 1] : null;
        public CodeInstruction Previous => Index > 0 ? Code[Index - 1] : null;
        public bool IsStatic => Original.IsStatic;
        public bool IsFirst => Index == 0;
        public bool IsLast => Index >= Code.Count - 1;

        public CodeInstruction this[int index]
        {
            get => Code[index];
            set => Code[index] = value;
        }

        #endregion

        #region Constructors

        public TranspilerData(IEnumerable<CodeInstruction> code, ILGenerator generator, MethodBase original)
        {
            this.Code = code as List<CodeInstruction> ?? code.ToList();
            this.Index = 0;
            this.Generator = generator;
            this.Original = original;
            this.Locals = original?.GetMethodBody()?.LocalVariables;
        }

        #endregion

        #region Positioning/Search

        public TranspilerData First()
        {
            Index = 0;
            return this;
        }

        public TranspilerData Last()
        {
            Index = Code.Count - 1;
            return this;
        }

        public TranspilerData Offset(int offset)
        {
            Index += offset;
            if (Index < 0)
                Index = 0;
            if (Index >= Code.Count)
                Index = Code.Count - 1;
            return this;
        }

        public TranspilerData Seek(Func<TranspilerData, bool> pred)
        {
            while (true)
            {
                if (Index >= Code.Count - 1)
                    throw new ArgumentException("IsLast");

                ++Index;
                if (pred(this))
                    return this;
            }
        }

        public TranspilerData Rewind(Func<TranspilerData, bool> pred)
        {
            while (true)
            {
                if (Index <= 0)
                    throw new ArgumentException("IsFirst");

                --Index;
                if (pred(this))
                    return this;
            }
        }

        public TranspilerData Seek(Type type, string name)
        {
            var member = Helper.GetMemberInfo(type, name);
            while (true)
            {
                if (Index >= Code.Count - 1)
                    throw new ArgumentException("IsLast");

                ++Index;
                if (Code[Index].Calls(member))
                    return this;
            }
        }

        public TranspilerData Rewind(Type type, string name)
        {
            var member = Helper.GetMemberInfo(type, name);
            while (true)
            {
                if (Index <= 0)
                    throw new ArgumentException("IsFirst");

                --Index;
                if (Code[Index].Calls(member))
                    return this;
            }
        }

        public TranspilerData Seek(OpCode op)
        {
            while (true)
            {
                if (Index >= Code.Count - 1)
                    throw new ArgumentException("IsLast");

                ++Index;
                if (Current.opcode == op)
                    return this;
            }
        }

        public TranspilerData Rewind(OpCode op)
        {
            while (true)
            {
                if (Index <= 0)
                    throw new ArgumentException("IsFirst");

                --Index;
                if (Current.opcode == op)
                    return this;
            }
        }

        public TranspilerData Seek(OpCode op, object operand)
        {
            while (true)
            {
                if (Index >= Code.Count - 1)
                    throw new ArgumentException("IsLast");

                ++Index;
                if (Current.opcode == op && Current.operand == operand)
                    return this;
            }
        }

        public TranspilerData Rewind(OpCode op, object operand)
        {
            while (true)
            {
                if (Index <= 0)
                    throw new ArgumentException("IsFirst");

                --Index;
                if (Current.opcode == op && Current.operand == operand)
                    return this;
            }
        }

        /// <summary>Seeks predicates in order.</summary>
        /// <param name="onStart">True: place at start of match<br/>False: place at end of match</param>
        /// <exception cref="ArgumentException">If no match found.</exception>
        public TranspilerData Seek(bool onStart, params Func<TranspilerData, bool>[] pred)
        {

            while (true)
            {
            lStart:
                Seek(pred[0]);
                int start = Index;
                Helper.PrintDebug($"@{Index} start");

                for (int i = 1; i < pred.Length; i++)
                {
                    Helper.PrintDebug($"@{Index} loop={i}");
                    if (Index >= Code.Count - 1)
                        throw new ArgumentException("IsLast");
                    ++Index;
                    if (!pred[i](this))
                    {
                        Index = start;
                        goto lStart;
                    }
                }

                Helper.PrintDebug($"@{Index} end OnStart={start}");

                if (onStart)
                    Index = start;
                Helper.PrintDebug($"Transpiler Seek:pred[] @{Index} {Current.opcode}");
                return this;
            }
        }

        #endregion

        #region Check

        public bool Is(OpCode op) => Current.opcode == op;

        public bool Is(OpCode op, object operand) => Current.opcode == op && Current.operand == operand;

        public bool Calls(Type type, string name) => Helper.Calls(Current, _memberCache.Get(type, name));

        public bool IsStloc(Type type)
        {
            var code = Current;
            if (!CodeInstructionExtensions.IsStloc(code))
                return false;

            if (code.operand is LocalBuilder lb)
                return lb.LocalType == type;

            if (code.opcode == OpCodes.Stloc_0)
                return Locals[0].LocalType == type;
            if (code.opcode == OpCodes.Stloc_1)
                return Locals[1].LocalType == type;
            if (code.opcode == OpCodes.Stloc_2)
                return Locals[2].LocalType == type;
            if (code.opcode == OpCodes.Stloc_3)
                return Locals[3].LocalType == type;

            return false;
        }

        #endregion

        #region Manipulation

        /// <summary>
        /// Injects IL code.
        /// </summary>
        public void InsertBefore(OpCode opcode, object operand = null)
        {
            Code.Insert(Index++, new CodeInstruction(opcode, operand));
        }

        /// <summary>
        /// Injects call. Return value must equal first argument. Can define __instance and any of the original's parameters in any order. <br/>
        /// This function will inject necessary load OpCodes. Will not validate __result type. Make sure the value on the stack is correct!
        /// </summary>
        /// <param name="func"><b>[T] Function([T __result], [object __instance], [object arg0], [object arg1...])</b></param>
        public void InsertBefore(Delegate func)
        {
            var mi = func.GetMethodInfo();
            var parametersFunc = mi.GetParameters();
            var parametersOriginal = Original.GetParameters();

            // handle parameters
            for (int i = 0; i < parametersFunc.Length; i++)
            {
                // can replace value on stack by returning the same value type
                if (i == 0 && mi.ReturnType != typeof(void))
                {
                    if (mi.ReturnType != parametersFunc[i].ParameterType)
                        throw new ArgumentException("Delegate return value must equal it's first argument (unless it returns void)!");
                    continue;
                }

                // special case for __instance
                if (parametersFunc[i].Name == "__instance")
                {
                    if (IsStatic)
                        throw new ArgumentException($"Delegate can't use __instance because method is static");
                    if (!parametersFunc[i].ParameterType.IsAssignableFrom(Original.DeclaringType))
                        throw new ArgumentException($"Delegate argument {parametersFunc[i].Name} mismatch type '{parametersFunc[i].ParameterType}' - '{Original.DeclaringType}'!");
                    InsertBefore(OpCodes.Ldarg_0);
                    continue;
                }

                // load arguments by name
                int index = parametersOriginal.FindIndex(f => f.Name == parametersFunc[i].Name);
                if (index < 0)
                    throw new ArgumentException($"Delegate unknown parameter name '{parametersFunc[i].Name}'");
                if (!IsStatic) // in non-static methods the first argument is 'this'
                    index++;
                if (!parametersFunc[i].ParameterType.IsAssignableFrom(parametersOriginal[index].ParameterType))
                    throw new ArgumentException($"Delegate argument {parametersFunc[i].Name} mismatch type '{parametersFunc[i].ParameterType}' - '{parametersOriginal[index].ParameterType}'!");
                InsertBefore(OpCodes.Ldarg, index);
            }

            // inject call
            InsertBefore(OpCodes.Call, mi);
        }

        /// <summary>
        /// Injects IL code. Points to this new injection.
        /// </summary>
        public void InsertAfter(OpCode opcode, object operand = null)
        {
            Code.Insert(++Index, new CodeInstruction(opcode, operand));
        }

        /// <summary>
        /// Injects call. Return value must equal first argument. Can define __instance and any of the original's parameters in any order. <br/>
        /// This function will inject necessary load OpCodes. Will not validate __result type. Make sure the value on the stack is correct!
        /// </summary>
        /// <param name="func"><b>[T] Function([T __result], [object __instance], [object arg0], [object arg1...])</b></param>
        public void InsertAfter(Delegate func)
        {
            var mi = func.GetMethodInfo();
            var parametersFunc = mi.GetParameters();
            var parametersOriginal = Original.GetParameters();

            // handle parameters
            for (int i = 0; i < parametersFunc.Length; i++)
            {
                // can replace value on stack by returning the same value type
                if (i == 0 && mi.ReturnType != typeof(void))
                {
                    if (mi.ReturnType != parametersFunc[i].ParameterType)
                        throw new ArgumentException("Delegate return value must equal it's first argument (unless it returns void)!");
                    continue;
                }

                // special case for __instance
                if (parametersFunc[i].Name == "__instance")
                {
                    if (IsStatic)
                        throw new ArgumentException($"Delegate can't use __instance because method is static");
                    if (!parametersFunc[i].ParameterType.IsAssignableFrom(Original.DeclaringType))
                        throw new ArgumentException($"Delegate argument {parametersFunc[i].Name} mismatch type '{parametersFunc[i].ParameterType}' - '{Original.DeclaringType}'!");
                    InsertAfter(OpCodes.Ldarg_0);
                    continue;
                }

                // load arguments by name
                int index = parametersOriginal.FindIndex(f => f.Name == parametersFunc[i].Name);
                if (index < 0)
                    throw new ArgumentException($"Delegate unknown parameter name '{parametersFunc[i].Name}'");
                if (!parametersFunc[i].ParameterType.IsAssignableFrom(parametersOriginal[index].ParameterType))
                    throw new ArgumentException($"Delegate argument {parametersFunc[i].Name} mismatch type '{parametersFunc[i].ParameterType}' - '{parametersOriginal[index].ParameterType}'!");
                if (!IsStatic) // in non-static methods the first argument is 'this'
                    index++;
                InsertAfter(OpCodes.Ldarg, index);
            }

            // inject call
            InsertAfter(OpCodes.Call, mi);
        }

        /// <summary>
        /// Injects function to access local variable. TranspilerData.Current must point to 'Stloc' opcode.
        /// </summary>
        /// <param name="func"><b>void Function(object instance, ref T value)</b></param>
        public void EditLocal(Delegate func)
        {
            // dont't forget ref!
            // Type.GetElementType()
            // Type.MakeByRefType()

            // delegate sanity check
            var mi = func.GetMethodInfo();
            var parameters = mi.GetParameters();
            if (parameters.Length != 2)
                throw new ArgumentException("Delegate must have exactly 2 arguments!");
            if (!IsStatic && !parameters[0].ParameterType.IsAssignableFrom(Original.DeclaringType))
                throw new ArgumentException("Delegate first argument must be object or declaring class!");
            if (!parameters[1].ParameterType.IsByRef)
                throw new ArgumentException("Delegate second argument must be by ref!");
            var line = Current;
            var type = parameters[1].ParameterType.GetElementType();
            if (type != line.GetLocType(Locals))
                throw new ArgumentException("CodeInstruction.operand must match delegate by ref type!");

            if (IsStatic)
                InsertAfter(OpCodes.Ldnull);
            else
                InsertAfter(OpCodes.Ldarg_0);
            InsertAfter(OpCodes.Ldloca, line.operand);
            InsertAfter(OpCodes.Call, mi);
        }

        /// <summary>
        /// Injects function to access stack variable. Call is injected before TranspilerData.Current.
        /// </summary>
        /// <param name="func"><b>void Function(T value, object instance)</b></param>
        public void EditStackBefore(Delegate func)
        {
            // delegate sanity check
            var mi = func.GetMethodInfo();
            var parameters = mi.GetParameters();
            if (parameters.Length != 2)
                throw new ArgumentException("Delegate must have exactly 2 arguments!");
            if (!IsStatic && !parameters[1].ParameterType.IsAssignableFrom(Original.DeclaringType))
                throw new ArgumentException("Delegate second argument must be object or declaring class!");

            if (IsStatic)
                InsertBefore(OpCodes.Ldnull);
            else
                InsertBefore(OpCodes.Ldarg_0);
            InsertBefore(OpCodes.Call, mi);
        }

        /// <summary>
        /// Replaces the current IL line with call. Does not validate arguments.
        /// </summary>
        public void ReplaceCall(Delegate newFunc)
        {
            Code[Index].opcode = OpCodes.Call;
            Code[Index].operand = newFunc.Method;
        }

        public void ReplaceAllCalls(Type type, string name, Delegate newFunc)
        {
            var original = AccessTools.Method(type, name, null, null);

            for (int i = 0; i < Code.Count; i++)
            {
                if (Code[i].Calls(original))
                {
                    Code[i].opcode = OpCodes.Call;
                    Code[i].operand = newFunc.Method;
                }
            }
        }

        public void ReplaceNOP()
        {
            Code[Index].opcode = OpCodes.Nop;
            Code[Index].operand = null;
        }

        public void NextJumpAlways()
        {
            for (; Index < Code.Count; Index++)
            {
                var line = Code[Index];

                if (line.opcode.FlowControl == FlowControl.Branch)
                {
                    Helper.PrintDebug("warning: unexpected branch");
                    return;
                }

                if (line.opcode.FlowControl != FlowControl.Cond_Branch || line.opcode == OpCodes.Switch)
                    continue;

                Helper.PrintDebug($"Transpiler NextJumpAlways @{Index} {line.opcode}");

                if (line.opcode.OperandType == OperandType.InlineBrTarget)
                    Code.Insert(++Index, new CodeInstruction(OpCodes.Br, line.operand));

                else if (line.opcode.OperandType == OperandType.ShortInlineBrTarget)
                    Code.Insert(++Index, new CodeInstruction(OpCodes.Br_S, line.operand));

                else
                    throw new Exception("Did not expect this OpCode");

                return;
            }
        }

        public void NextJumpNever()
        {
            for (; Index < Code.Count; Index++)
            {
                var line = Code[Index];

                if (line.opcode.FlowControl == FlowControl.Branch)
                {
                    Helper.PrintDebug("warning: unexpected branch");
                    return;
                }

                if (line.opcode.FlowControl != FlowControl.Cond_Branch || line.opcode == OpCodes.Switch)
                    continue;

                Helper.PrintDebug($"Transpiler NextJumpNever @{Index} {line.opcode}");

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
                    Code.Insert(Index++, new CodeInstruction(OpCodes.Pop));
                }
                else
                    throw new Exception("Cond_Branch should not pop more than 2");

                return;
            }
        }

        #endregion

        #region Statics

        public static TranspilerData operator ++(TranspilerData a)
        {
            a.Index++;
            return a;
        }

        public static TranspilerData operator --(TranspilerData a)
        {
            a.Index--;
            return a;
        }

        private static CacheData<MemberInfo> _memberCache = new(a => Helper.GetMemberInfo((Type)a[0], (string)a[1]));

        #endregion
    }
}
