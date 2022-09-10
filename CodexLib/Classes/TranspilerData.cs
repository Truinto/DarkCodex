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
