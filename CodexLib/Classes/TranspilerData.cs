﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class TranspilerData
    {
        public List<CodeInstruction> Code;
        public int Index;
        public ILGenerator Generator;
        public MethodBase Original;
        public IList<LocalVariableInfo> Locals;

        public TranspilerData(IEnumerable<CodeInstruction> code, ILGenerator generator, MethodBase original) : this(code as List<CodeInstruction> ?? code.ToList(), generator, original)
        {
        }

        public TranspilerData(List<CodeInstruction> code, ILGenerator generator, MethodBase original)
        {
            this.Code = code;
            this.Index = 0;
            this.Generator = generator;
            this.Original = original;
            this.Locals = original.GetMethodBody()?.LocalVariables;
        }

        public void InsertBefore(OpCode opcode, object operand = null)
        {
            Code.Insert(Index++, new CodeInstruction(opcode, operand));
        }

        public void InsertAfter(OpCode opcode, object operand = null)
        {
            Code.Insert(++Index, new CodeInstruction(opcode, operand));
        }

        public TranspilerData Seek(Func<TranspilerData, bool> pred)
        {
            while (Index < Code.Count)
            {
                ++Index;
                if (pred(this))
                    break;
            }
            return this;
        }

        public TranspilerData Rewind(Func<TranspilerData, bool> pred)
        {
            while (Index >= 0)
            {
                --Index;
                if (pred(this))
                    break;
            }
            return this;
        }

        public TranspilerData Seek(Type type, string name)
        {
            var mi = type.GetMethod(name, Helper.BindingAll) ?? type.GetProperty(name, Helper.BindingAll).GetMethod;

            if (mi != null)
            {
                while (Index < Code.Count)
                {
                    ++Index;
                    if (Code[Index].Calls(mi))
                        return this;
                }
            }

            var fi = type.GetField(name, Helper.BindingAll);
            if (fi != null)
            {
                while (Index < Code.Count)
                {
                    ++Index;
                    if (Code[Index].Calls(fi))
                        return this;
                }
            }

            return this;
        }

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

        public bool IsStloc(Type type)
        {
            var code = Current;
            if (!code.IsStloc())
                return false;

            if (code.operand is LocalBuilder lb)
                return lb.LocalType == type;

            if (code.opcode == OpCodes.Stloc_0)
                return Locals[0].LocalType == type;
            if (code.opcode == OpCodes.Stloc_0)
                return Locals[1].LocalType == type;
            if (code.opcode == OpCodes.Stloc_0)
                return Locals[2].LocalType == type;
            if (code.opcode == OpCodes.Stloc_0)
                return Locals[3].LocalType == type;

            return false;
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
            if (parameters.Length != 2 || !parameters[1].ParameterType.IsByRef)
                throw new ArgumentException("Delegate must have exactly 2 arguments and the second argument must be by ref!");

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

        public void NextJumpAlways()
        {
            for (; Index < Code.Count; Index++)
            {
                var line = Code[Index];

                if (line.opcode.FlowControl != FlowControl.Cond_Branch)
                    continue;

                Helper.PrintDebug($"Transpiler NextJumpAlways {line.opcode} @{Index}");

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

                if (line.opcode.FlowControl != FlowControl.Cond_Branch)
                    continue;

                Helper.PrintDebug($"Transpiler NextJumpNever {line.opcode} @{Index}");

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
        public CodeInstruction Current => Code[Index];
        public CodeInstruction Next => Index + 1 < Code.Count ? Code[Index + 1] : null;
        public CodeInstruction Previous => Index > 0 ? Code[Index - 1] : null;
        public bool IsStatic => Original.IsStatic;

        public CodeInstruction this[int index]
        {
            get => Code[index];
            set => Code[index] = value;
        }

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
    }
}
