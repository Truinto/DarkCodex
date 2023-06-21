using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

#pragma warning disable IDE0009 // ignore missing 'this.'

namespace Shared
{
    /// <summary>
    /// Tool to automate transpiler modifications.
    /// </summary>
    public class TranspilerTool : IList<CodeInstruction>
    {
        #region Constants

        /// <summary></summary>
        public const BindingFlags BindingAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        /// <summary></summary>
        public const BindingFlags BindingInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        /// <summary></summary>
        public const BindingFlags BindingStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        #endregion

        #region Fields

        /// <summary>Index of the currently selected line. 'Index' usually refers to this variable.</summary>
        public int Index;
        /// <summary>Collection of all CodeInstructions.</summary>
        public readonly IList<CodeInstruction> Code;
        /// <summary>ILGenerator. Required for creating new labels or locals.</summary>
        public readonly ILGenerator Generator;
        /// <summary>MethodInfo of Original. 'Original' usually refers to the method being patched by the transpiler.</summary>
        public readonly MethodInfo Original;
        /// <summary>Collection of locals used by the original method.</summary>
        public readonly IList<LocalVariableInfo> Locals;
        /// <summary>Full collection of locals, original or created by this tool.</summary>
        public readonly List<LocalInfo> LocalsExtended;
        /// <summary>Full collection of labels, original or created by this tool.</summary>
        public readonly List<LabelInfo> LabelsExtended;

        #endregion

        #region Properties

        /// <summary>Currently selected line of code.</summary>
        public CodeInstruction Current => Code[Index];
        /// <summary>Line of code after selection. Does not move index. Returns null, if end of code.</summary>
        public CodeInstruction Next => !IsLast ? Code[Index + 1] : null;
        /// <summary>Line of code before selection. Does not move index. Returns null, if index is 0.</summary>
        public CodeInstruction Previous => !IsFirst ? Code[Index - 1] : null;
        /// <summary>True if original method is static. False if it's an instance object.</summary>
        public bool IsStatic => Original.IsStatic;
        /// <summary>True if index is 0.</summary>
        public bool IsFirst => Index <= 0;
        /// <summary>True if index is beyond the collection of lines.</summary>
        public bool IsLast => Index >= Code.Count - 1;
        /// <summary>Total count of lines.</summary>
        public int Count => Code.Count;
        /// <summary></summary>
        public bool IsReadOnly => Code.IsReadOnly;
        /// <summary>Code of a specific index. Does not move selected index.</summary>
        public CodeInstruction this[int index]
        {
            get => Code[index];
            set => Code[index] = value;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Requires all optional objects from HarmonyTranspiler. Throws if any is null or original is not of type MethodInfo.
        /// </summary>
        public TranspilerTool(IEnumerable<CodeInstruction> code, ILGenerator generator, MethodBase original)
        {
            if (code is TranspilerTool tool)
            {
                this.Code = tool.Code;
                this.Generator = tool.Generator;
                this.Original = tool.Original;
                this.Locals = tool.Locals;
                this.LocalsExtended = tool.LocalsExtended;
                this.LabelsExtended = tool.LabelsExtended;
                return;
            }

            this.Code = code as IList<CodeInstruction> ?? code.ToList();
            this.Index = 0;
            this.Generator = generator ?? throw new ArgumentNullException();
            this.Original = (MethodInfo)original ?? throw new ArgumentNullException();
            Locals = original.GetMethodBody().LocalVariables;
            LocalsExtended = new();
            LabelsExtended = new();

            foreach (var local in Locals)
                LocalsExtended.Add(new(local, "V_" + local.LocalIndex));

            foreach (var line in code)
                LabelsExtended.AddUnique(line.labels, a => new(a));
        }

        #endregion

        #region Positioning/Search

        /// <summary>
        /// Moves index to first line.
        /// </summary>
        public TranspilerTool First()
        {
            Index = 0;
            return this;
        }

        /// <summary>
        /// Moves index to last line.
        /// </summary>
        public TranspilerTool Last()
        {
            Index = Code.Count - 1;
            return this;
        }

        /// <summary>
        /// Moves index by <paramref name="offset"/>. Negative values are allowed. Does not go past boundaries.
        /// </summary>
        public TranspilerTool Offset(int offset)
        {
            Index += offset;
            if (Index < 0)
                Index = 0;
            if (Index >= Code.Count)
                Index = Code.Count - 1;
            return this;
        }

        /// <summary>
        /// Moves index forward until predicate matches. Throws if no match found.
        /// </summary>
        public TranspilerTool Seek(Func<TranspilerTool, bool> pred)
        {
            while (true)
            {
                if (IsLast)
                    throw new IndexOutOfRangeException("IsLast");

                ++Index;
                if (pred(this))
                    return this;
            }
        }

        /// <summary>
        /// Moves index backward until predicate matches. Throws if no match found.
        /// </summary>
        public TranspilerTool Rewind(Func<TranspilerTool, bool> pred)
        {
            while (true)
            {
                if (IsFirst)
                    throw new IndexOutOfRangeException("IsFirst");

                --Index;
                if (pred(this))
                    return this;
            }
        }

        /// <summary>
        /// Moves index forward until calling a matching method or accessing a matching field. <br/>
        /// Also resolves property getter, but their setter is ignored.
        /// </summary>
        /// <seealso cref="GetMemberInfo(Type, string, Type[], Type[])"/>
        public TranspilerTool Seek(Type type, string name, Type[] parameters = null, Type[] generics = null)
        {
            while (true)
            {
                if (IsLast)
                    throw new IndexOutOfRangeException("IsLast");

                ++Index;
                if (Calls(type, name, parameters, generics))
                    return this;
            }
        }

        /// <summary>
        /// Moves index backward until calling a matching method or accessing a matching field. <br/>
        /// Also resolves property getter, but their setting is ignored.
        /// </summary>
        /// <seealso cref="GetMemberInfo(Type, string, Type[], Type[])"/>
        public TranspilerTool Rewind(Type type, string name, Type[] parameters = null, Type[] generics = null)
        {
            while (true)
            {
                if (IsFirst)
                    throw new IndexOutOfRangeException("IsFirst");

                --Index;
                if (Calls(type, name, parameters, generics))
                    return this;
            }
        }

        /// <summary>
        /// Moves index forward until a specified OpCode is used. Can optionally compare the operand as well.
        /// </summary>
        public TranspilerTool Seek(OpCode op, object operand = null)
        {
            while (true)
            {
                if (IsLast)
                    throw new IndexOutOfRangeException("IsLast");

                ++Index;
                if (Current.opcode == op && (operand == null || object.Equals(Current.operand, operand)))
                    return this;
            }
        }

        /// <summary>
        /// Moves index backward until a specified OpCode is used. Can optionally compare the operand as well.
        /// </summary>
        public TranspilerTool Rewind(OpCode op, object operand = null)
        {
            while (true)
            {
                if (IsFirst)
                    throw new IndexOutOfRangeException("IsFirst");

                --Index;
                if (Current.opcode == op && (operand == null || Current.operand == operand))
                    return this;
            }
        }

        /// <summary>
        /// Moves index forward until predicates match in order. <br/>
        /// If <paramref name="onStart"/> is true, will place index to the start of the match. Otherwise at the end.
        /// </summary>
        public TranspilerTool Seek(bool onStart, params Func<TranspilerTool, bool>[] pred)
        {
            while (true)
            {
            lStart:
                Seek(pred[0]);
                int start = Index;
                Logger.PrintDebug($"@{Index} start");

                for (int i = 1; i < pred.Length; i++)
                {
                    Logger.PrintDebug($"@{Index} loop={i}");
                    if (IsLast)
                        throw new IndexOutOfRangeException("IsLast");
                    ++Index;
                    if (!pred[i](this))
                    {
                        Index = start;
                        goto lStart;
                    }
                }

                Logger.PrintDebug($"@{Index} end OnStart={start}");

                if (onStart)
                    Index = start;
                Logger.PrintDebug($"Transpiler Seek:pred[] @{Index} {Current.opcode}");
                return this;
            }
        }

        #endregion

        #region Check

        /// <summary>Checks if current line matches OpCode.</summary>
        public bool Is(OpCode op) => Current.opcode == op;

        /// <summary>Checks if current line matches OpCode and operand.</summary>
        public bool Is(OpCode op, object operand) => Current.opcode == op && object.Equals(Current.operand, operand);

        /// <summary>
        /// Checks if current line calls or accesses given member.
        /// </summary>
        /// <seealso cref="GetMemberInfo(Type, string, Type[], Type[])"/>
        public bool Calls(Type type, string name, Type[] parameters = null, Type[] generics = null)
        {
            return OpCode_Member.Contains(Current.opcode) && _memberCache.Get(type, name, parameters, generics).Equals(Current.operand);
        }

        /// <summary>
        /// Checks if current line calls or accesses given member.
        /// </summary>
        /// <seealso cref="GetMemberInfo(Type, string, Type[], Type[])"/>
        public bool Calls(MemberInfo memberinfo)
        {
            return OpCode_Member.Contains(Current.opcode) && memberinfo.Equals(Current.operand);
        }

        /// <summary>Checks if current line stores local and has matching index.</summary>
        public bool IsStloc(int localIndex) => Current.IsStloc() && localIndex == GetLocalIndex();

        /// <summary>Checks if current line stores local and has matching name.</summary>
        public bool IsStloc(string localName) => Current.IsStloc() && GetLocal(localName).Equals(Current.operand);

        /// <summary>Checks if current line stores local and has exactly matching type.</summary>
        public bool IsStloc(Type type) => Current.IsStloc() && GetLocal().Type == type;

        /// <summary>Checks if current line loads local and has matching index.</summary>
        public bool IsLdloc(int localIndex) => Current.IsLdloc() && localIndex == GetLocalIndex();

        /// <summary>Checks if current line loads local and has matching name.</summary>
        public bool IsLdloc(string localName) => Current.IsLdloc() && GetLocal(localName).Equals(Current.operand);

        /// <summary>Checks if current line loads local and has exactly matching type.</summary>
        public bool IsLdloc(Type type) => Current.IsLdloc() && GetLocal().Type == type;

        /// <summary>Checks if current line loads a constant.</summary>
        public bool IsLoadConstant(long number) => Current.LoadsConstant(number);

        /// <summary>Checks if current line loads a constant.</summary>
        public bool IsLoadConstant(Enum e) => Current.LoadsConstant(e);

        /// <summary>Checks if current line loads a constant.</summary>
        public bool IsLoadConstant(double number) => Current.LoadsConstant(number);

        #endregion

        #region Manipulation

        /// <summary>
        /// Inserts call to a delegate. Either before or after current line. Increments index.
        /// </summary>
        protected void InsertCall(bool before, Delegate func)
        {
            var mi = func.GetMethodInfo();
            var parametersFunc = mi.GetParameters();
            bool hasReturn = mi.ReturnType != typeof(void);

            if (!mi.IsStatic)
                throw new ArgumentException("Delegate must be static");

            if (hasReturn && (parametersFunc.Length == 0 || parametersFunc[0].ParameterType != mi.ReturnType))
                throw new ExceptionInvalidReturn("Delegate return value must equal it's first argument (unless it returns void)!");

            // handle parameters
            for (int i = hasReturn ? 1 : 0; i < parametersFunc.Length; i++)
            {
                var parameterFunc = parametersFunc[i];

                if (InsertPushOriginal(before, parameterFunc))
                    continue;

                if (InsertPushLocal(before, parameterFunc))
                    continue;

                throw new ArgumentException($"Delegate unknown parameter name '{parameterFunc.Name}'!");
            }

            // inject call
            Code.Insert(before ? Index++ : ++Index, new CodeInstruction(OpCodes.Call, mi));
        }

        /// <summary>
        /// Tries to push local onto stack. Returns true if successfull.
        /// </summary>
        protected bool InsertPushLocal(bool before, ParameterInfo parameterFunc)
        {
            Type targetType = !parameterFunc.ParameterType.IsByRef ? parameterFunc.ParameterType : parameterFunc.ParameterType.GetElementType();
            LocalInfo local;
            int counter = 0;

            var targetLocal = parameterFunc.GetCustomAttribute<LocalParameterAttribute>();
            if (targetLocal == null)
            {
                local = GetLocal(parameterFunc.Name, canMake: false);
                if (local.IsEmpty)
                    return false;
            }
            else if (targetLocal.Name != null)
                local = GetLocal(targetLocal.Name, targetLocal.Type ??= targetType); // this may generate a new local           
            else if (targetLocal.Index >= 0)
                local = LocalsExtended.Find(f => f.Index == targetLocal.Index);
            else if (targetLocal.IndexByType >= 0)
                local = LocalsExtended.Find(a => a.Type == targetType && counter++ >= targetLocal.IndexByType);
            else
                local = LocalsExtended.Find(a => a.Index == GetLocalIndex());

            if (local.IsEmpty)
                throw new ExceptionInvalidParameter($"no matching local for {parameterFunc.Name}");

            bool deref = ThrowParameterIncompatible(parameterFunc, local.Type);

            Code.Insert(before ? Index++ : ++Index, new CodeInstruction(deref ? OpCodes.Ldloca : OpCodes.Ldloc, local.Index));
            return true;
        }

        /// <summary>
        /// Tries to push original parameter onto stack. Returns true if successfull.
        /// </summary>
        protected bool InsertPushOriginal(bool before, ParameterInfo parameterFunc)
        {
            int index;
            Type parameterTypeOriginal;
            var original = parameterFunc.GetCustomAttribute<OriginalParameterAttribute>();
            string name = original?.Name ?? parameterFunc.Name;

            if (name == "__instance")
            {
                if (IsStatic)
                    throw new ExceptionInvalidInstance("Delegate can not define __instance for a static method!");
                index = -1;
                parameterTypeOriginal = Original.DeclaringType;
            }
            else
            {
                var parametersOriginal = this.Original.GetParameters();
                index = parametersOriginal.FindIndex(f => f.Name == name);
                if (index < 0)
                    if (original == null)
                        return false;
                    else
                        throw new ExceptionInvalidParameter($"no matching original for {parameterFunc.Name}={name}");
                parameterTypeOriginal = parametersOriginal[index].ParameterType;
            }

            bool deref = ThrowParameterIncompatible(parameterFunc, parameterTypeOriginal);

            Code.Insert(before ? Index++ : ++Index, new CodeInstruction(deref ? OpCodes.Ldarga : OpCodes.Ldarg, IsStatic ? index : index + 1));
            return true;
        }

        /// <summary>
        /// Injects IL code. Increments index (still pointing to the same code).
        /// </summary>
        public void InsertBefore(OpCode opcode, object operand = null)
        {
            if (operand is LabelInfo label)
                operand = (Label)label;
            Code.Insert(Index++, new CodeInstruction(opcode, operand));
        }

        /// <summary>
        /// Injects IL code. Increments index (pointing to the new code).
        /// </summary>
        public void InsertAfter(OpCode opcode, object operand = null)
        {
            if (operand is LabelInfo label)
                operand = (Label)label;
            Code.Insert(++Index, new CodeInstruction(opcode, operand));
        }

        /// <summary>
        /// Injects call. Increments index (still pointing to the same code). <br/>
        /// Delegate may return void. Otherwise the <b>first parameter is taken from the stack and replaced with the return value</b>.<br/>
        /// <b>[T] Function([T __stack], [object __instance], [object arg0], [object arg1...])</b>
        /// </summary>
        /// <param name="func"><b>[T] Function([T __stack], [object __instance], [object arg0], [object arg1...])</b></param>
        /// <remarks>
        /// The delegate may define any of the original parameters, locals, and __instance (if non-static).<br/>
        /// These optional parameters are matched by name, with <see cref="LocalParameterAttribute"/>, or with <see cref="OriginalParameterAttribute"/>.<br/>
        /// If <see cref="LocalParameterAttribute"/> doesn't match any existing locals, then a new one is generated. Non matching parameters will throw.<br/>
        /// If the optional parameter's source is ref (or out), then the delegate must also use ref. Otherwise ref is optional.
        /// </remarks>
        public void InsertBefore(Delegate func)
        {
            InsertCall(true, func);
        }

        /// <summary>
        /// Injects call. Increments index (pointing to the new code). <br/>
        /// Delegate may return void. Otherwise the <b>first parameter is taken from the stack and replaced with the return value</b>.<br/>
        /// <b>[T] Function([T __stack], [object __instance], [object arg0], [object arg1...])</b>
        /// </summary>
        /// <param name="func"><b>[T] Function([T __stack], [object __instance], [object arg0], [object arg1...])</b></param>
        /// <remarks>
        /// The delegate may define any of the original parameters, locals, and __instance (if non-static).<br/>
        /// These optional parameters are matched by name, with <see cref="LocalParameterAttribute"/>, or with <see cref="OriginalParameterAttribute"/>.<br/>
        /// If <see cref="LocalParameterAttribute"/> doesn't match any existing locals, then a new one is generated. Non matching parameters will throw.<br/>
        /// If the optional parameter's source is ref (or out), then the delegate must also use ref. Otherwise ref is optional.
        /// </remarks>
        public void InsertAfter(Delegate func)
        {
            InsertCall(false, func);
        }

        /// <summary>
        /// Same as <see cref="InsertAfter(Delegate)"/>, but iterates all lines of code.
        /// </summary>
        /// <param name="type">Declaring type of the target member.</param>
        /// <param name="name">Name of the target member.</param>
        /// <param name="func">Delegate to run after target.</param>
        /// <param name="parameters">Optional parameter definition, if target method is overloaded.</param>
        /// <param name="generics">Optional generics definition, if target method has generics.</param>
        public int InsertAfterAll(Type type, string name, Delegate func, Type[] parameters = null, Type[] generics = null)
        {
            var original = _memberCache.Get(type, name, parameters, generics);
            return InsertAfterAll(original, func);
        }

        /// <summary>
        /// Same as <see cref="InsertAfter(Delegate)"/>, but iterates all lines of code.
        /// </summary>
        /// <param name="original">Target member.</param>
        /// <param name="func">Delegate to run after target.</param>
        public int InsertAfterAll(MemberInfo original, Delegate func)
        {
            int counter = 0;
            int index = Index;

            First();
            while (!IsLast)
            {
                if (Calls(original))
                {
                    InsertAfter(func);
                    counter++;
                }
                Index++;
            }

            Index = index;
            return counter;
        }

        /// <summary>
        /// Injects return value. Returns original method, if returning false or void. If the original has a return value, then it must be supplied with __result.<br/>
        /// This function will inject necessary load OpCodes.<br/>
        /// <b>[bool] Function(T out __result, [object __instance], [object arg0], [object arg1...])</b>
        /// </summary>
        /// <param name="func"><b>[bool] Function(T out __result, [object __instance], [object arg0], [object arg1...])</b></param>
        /// <param name="before">If true will inject delegate before current line (still pointing to the same code). Otherwise after (pointing to the new code).</param>
        /// <remarks>
        /// The delegate may define any of the original parameters, locals, and __instance (if non-static).<br/>
        /// These optional parameters are matched by name, with <see cref="LocalParameterAttribute"/>, or with <see cref="OriginalParameterAttribute"/>.<br/>
        /// If <see cref="LocalParameterAttribute"/> doesn't match any existing locals, then a new one is generated. Non matching parameters will throw.<br/>
        /// If the optional parameter's source is ref (or out), then the delegate must also use ref. Otherwise ref is optional.
        /// </remarks>
        public void InsertReturn(Delegate func, bool before = true)
        {
            var mi = func.GetMethodInfo();
            var parametersFunc = mi.GetParameters();
            bool hasCondition = false;
            LocalInfo returnLocal = default;
            Type returnType = Original.ReturnType;
            Label label1 = default;

            if (!mi.IsStatic)
                throw new ArgumentException("Delegate must be static");

            // check func return type
            if (mi.ReturnType == typeof(bool))
                hasCondition = true;
            else if (mi.ReturnType != typeof(void))
                throw new ExceptionInvalidReturn("Delegate can only return void or bool!");

            // check original return type
            if (returnType != typeof(void) && !parametersFunc.Any(a => a.Name == "__result"))
                throw new ExceptionMissingResult("Original has a return value; Delegate must have __result!");
            if (returnType == typeof(void) && parametersFunc.Any(a => a.Name == "__result"))
                throw new ExceptionInvalidResult("Original has no return value; Delegate must not have __result!");

            for (int i = 0; i < parametersFunc.Length; i++)
            {
                var parameterFunc = parametersFunc[i];

                if (parameterFunc.Name == "__result")
                {
                    if (!parameterFunc.ParameterType.IsByRef)
                        throw new ExceptionMissingRef("Delegate __result must be by ref!");
                    var returnTypeFunc = parameterFunc.ParameterType.GetElementType();
                    if (!returnType.IsTypeCompatible(returnTypeFunc))
                        throw new InvalidCastException($"Delegate parameter {parameterFunc.Name} mismatch type '{returnType}' - '{returnTypeFunc}'!");

                    returnLocal = GetLocal("__result", returnType);
                    Code.Insert(before ? Index++ : ++Index, new CodeInstruction(OpCodes.Ldloca, returnLocal.Local));
                    continue;
                }

                if (InsertPushOriginal(before, parameterFunc))
                    continue;

                if (InsertPushLocal(before, parameterFunc))
                    continue;

                throw new ArgumentException($"Delegate unknown parameter name '{parameterFunc.Name}'!");
            }

            Code.Insert(before ? Index++ : ++Index, new CodeInstruction(OpCodes.Call, mi));

            if (hasCondition)
                Code.Insert(before ? Index++ : ++Index, new CodeInstruction(OpCodes.Brtrue, label1 = GetLabel()));

            if (returnType != typeof(void))
                Code.Insert(before ? Index++ : ++Index, new CodeInstruction(OpCodes.Ldloc, returnLocal.Local));

            Code.Insert(before ? Index++ : ++Index, new CodeInstruction(OpCodes.Ret, null));

            if (hasCondition)
                (before ? Current : Next).labels.Add(label1);
        }

        /// <summary>
        /// Injects jump to given label.<br/>
        /// This function will inject necessary load OpCodes.<br/>
        /// Use <see cref="GetLabel(CodeInstruction, string, bool)"/> to jump to a specific CodeInstruction.<br/>
        /// <b>bool Function([object __instance], [object arg0], [object arg1...])</b>
        /// </summary>
        /// <param name="func"><b>bool Function([object __instance], [object arg0], [object arg1...])</b></param>
        /// <param name="label">Label to jump to, if func returns true.</param>
        /// <param name="before">If true will inject delegate before current line (still pointing to the same code). Otherwise after (pointing to the new code).</param>
        /// <remarks>
        /// The delegate may define any of the original parameters, locals, and __instance (if non-static).<br/>
        /// These optional parameters are matched by name, with <see cref="LocalParameterAttribute"/>, or with <see cref="OriginalParameterAttribute"/>.<br/>
        /// If <see cref="LocalParameterAttribute"/> doesn't match any existing locals, then a new one is generated. Non matching parameters will throw.<br/>
        /// If the optional parameter's source is ref (or out), then the delegate must also use ref. Otherwise ref is optional.
        /// </remarks>
        public void InsertJump(Delegate func, LabelInfo label, bool before = false)
        {
            var mi = func.GetMethodInfo();
            var parametersFunc = mi.GetParameters();

            if (!mi.IsStatic)
                throw new ArgumentException("Delegate must be static");
            if (mi.ReturnType != typeof(bool))
                throw new ArgumentException("Delegate must return bool");
            if (label.IsEmpty)
                throw new ArgumentException("LabelInfo must not be empty");

            for (int i = 0; i < parametersFunc.Length; i++)
            {
                var parameterFunc = parametersFunc[i];

                if (InsertPushOriginal(before, parameterFunc))
                    continue;

                if (InsertPushLocal(before, parameterFunc))
                    continue;

                throw new ArgumentException($"Delegate unknown parameter name '{parameterFunc.Name}'!");
            }

            Code.Insert(before ? Index++ : ++Index, new CodeInstruction(OpCodes.Call, mi));
            Code.Insert(before ? Index++ : ++Index, new CodeInstruction(OpCodes.Brtrue, (Label)label));
        }

        /// <summary>
        /// <b>Instance methods need their instance as the first parameter!</b><br/>
        /// Replaces the current IL line with call. The delegate must return the replacement return type and <b>must have the same parameters with identical names and order</b>.<br/>
        /// <b>object Function(object object_instance, object parameter0, object parameter1..., [object __instance], [object arg0], [object arg1...])</b>
        /// </summary>
        /// <param name="func"><b>object Function(object object_instance, object parameter0, object parameter1..., [object __instance], [object arg0], [object arg1...])</b></param>
        /// <remarks>
        /// The current line may contain a method call or field access.<br/>
        /// The delegate may define any of the original parameters, locals, and __instance (if non-static), but only after the replacement parameters.<br/>
        /// These optional parameters are matched by name, with <see cref="LocalParameterAttribute"/>, or with <see cref="OriginalParameterAttribute"/>.<br/>
        /// If <see cref="LocalParameterAttribute"/> doesn't match any existing locals, then a new one is generated. Non matching parameters will throw.<br/>
        /// If the optional parameter's source is ref (or out), then the delegate must also use ref. Otherwise ref is optional.
        /// </remarks>
        public void ReplaceCall(Delegate func)
        {
            //if (Current.opcode != OpCodes.Call && Current.opcode != OpCodes.Callvirt
            //    || Current.operand is not MethodInfo replace)
            //    throw new InvalidOperationException("Current must be a call");

            var replace = Current.operand as MemberInfo;
            var mi = func.GetMethodInfo();
            var parametersFunc = mi.GetParameters();
            ParameterInfo[] parametersReplace;
            bool isReplaceStatic;
            LocalInfo local;

            if (!mi.IsStatic)
                throw new ArgumentException("Delegate must be static");

            if (replace is MethodInfo replaceMethod) // if a method is overwritten
            {
                Logger.PrintDebug($"ReplaceCall \n\t{mi.FullDescription()} -> \n\t{replaceMethod.FullDescription()}");
                parametersReplace = replaceMethod.GetParameters();
                isReplaceStatic = replaceMethod.IsStatic;

                if (!replaceMethod.ReturnType.IsTypeCompatible(mi.ReturnType))
                    throw new ExceptionMissingReturn("Delegate must return identical type!");
            }
            else if (replace is FieldInfo replaceField) // if a field call is overwritten
            {
                Logger.PrintDebug($"ReplaceCall \n\t{mi.FullDescription()} -> \n\t{replace}");
                parametersReplace = new ParameterInfo[0];
                isReplaceStatic = Current.opcode != OpCodes.Ldfld;

                if (!replaceField.FieldType.IsTypeCompatible(mi.ReturnType))
                    throw new ExceptionMissingReturn("Delegate must return identical type!");
            }
            else if (Current.opcode == OpCodes.Ldc_I8)
            {
                Logger.PrintDebug($"ReplaceCall \n\t{mi.FullDescription()} -> \n\t{Current.opcode}");
                parametersReplace = new ParameterInfo[0];
                isReplaceStatic = true;

                if (!typeof(long).IsTypeCompatible(mi.ReturnType))
                    throw new ExceptionMissingReturn("Delegate must return identical type!");
            }
            else if (OpCode_Ldc_i.Contains(Current.opcode))
            {
                Logger.PrintDebug($"ReplaceCall \n\t{mi.FullDescription()} -> \n\t{Current.opcode}");
                parametersReplace = new ParameterInfo[0];
                isReplaceStatic = true;

                if (!typeof(int).IsTypeCompatible(mi.ReturnType))
                    throw new ExceptionMissingReturn("Delegate must return identical type!");
            }
            else if (!(local = GetLocal()).IsEmpty)
            {
                Logger.PrintDebug($"ReplaceCall \n\t{mi.FullDescription()} -> \n\t{local}");
                parametersReplace = new ParameterInfo[0];
                isReplaceStatic = true;

                if (!local.Type.IsTypeCompatible(mi.ReturnType))
                    throw new ExceptionMissingReturn("Delegate must return identical type!");
            }
            else
                throw new InvalidOperationException("Current must call a member");

            if (!isReplaceStatic && (parametersFunc.Length == 0 || parametersFunc[0].Name == parametersReplace.AtIndex(0)?.Name))
                throw new ExceptionMissingInstance("Delegate is missing instance!");

            if (parametersFunc.Length < parametersReplace.Length + (isReplaceStatic ? 0 : 1))
                throw new ExceptionMissingParameter("Delegate has less parameters than replaced method!");

            for (int i = 0, j = 0; i < parametersFunc.Length; i++, j++)
            {
                var parameterFunc = parametersFunc[i];

                if (i == 0 && !isReplaceStatic)
                {
                    if (!parameterFunc.ParameterType.IsTypeCompatible(replace.DeclaringType))
                        throw new InvalidCastException($"Delegate parameter {parameterFunc.Name} mismatch type '{parameterFunc.ParameterType}' - '{replace.DeclaringType}'!");
                    j--;
                    continue;
                }

                var parameterReplace = parametersReplace.AtIndex(j);
                if (parameterReplace != null)
                {
                    if (parameterFunc.Name != parameterReplace.Name)
                        throw new ExceptionInvalidParameterOrder($"Delegate must implement all replacement parameters in the same order and the same name '{parameterFunc.Name}' - '{parameterReplace.Name}'!");
                    if (!parameterFunc.ParameterType.IsTypeCompatible(parameterReplace.ParameterType))
                        throw new InvalidCastException($"Delegate parameter {parameterFunc.Name} mismatch type '{parameterFunc.ParameterType}' - '{parameterReplace.ParameterType}'!");
                    continue;
                }

                if (InsertPushOriginal(true, parameterFunc))
                    continue;

                if (InsertPushLocal(true, parameterFunc))
                    continue;

                throw new ArgumentException($"Delegate unknown parameter name '{parameterFunc.Name}'!");
            }

            Current.opcode = OpCodes.Call;
            Current.operand = mi;
        }

        /// <summary>
        /// Same as <see cref="ReplaceCall(Delegate)"/>, but iterates all lines of code.
        /// </summary>
        /// <param name="type">Declaring type of the member to be replaced.</param>
        /// <param name="name">Name of the member to be replaced.</param>
        /// <param name="func">Delegate the member should be replaced with.</param>
        /// <param name="parameters">Optional parameter definition, if target method is overloaded.</param>
        /// <param name="generics">Optional generics definition, if target method has generics.</param>
        public int ReplaceAllCalls(Type type, string name, Delegate func, Type[] parameters = null, Type[] generics = null)
        {
            var original = _memberCache.Get(type, name, parameters, generics);
            return ReplaceAllCalls(original, func);
        }

        /// <summary>
        /// Same as <see cref="ReplaceCall(Delegate)"/>, but iterates all lines of code.
        /// </summary>
        /// <param name="original">Target member.</param>
        /// <param name="func">Delegate the member should be replaced with.</param>
        public int ReplaceAllCalls(MemberInfo original, Delegate func)
        {
            int counter = 0;
            int index = Index;

            First();
            while (!IsLast)
            {
                if (Calls(original))
                {
                    ReplaceCall(func);
                    counter++;
                }
                Index++;
            }

            Index = index;
            return counter;
        }

        /// <summary>
        /// Change current line's load to a new value. Can be any integer type. Throws if current isn't OpCode.Ldc_i.
        /// </summary>
        public void ReplaceConstant(Enum newValue)
        {
            if (!OpCode_Ldc_i.Contains(Current.opcode))
                throw new InvalidOperationException("IL must load constant");

            if (Current.opcode.OperandType == OperandType.InlineNone)
                Current.opcode = OpCodes.Ldc_I4;

            if (Current.opcode.OperandType == OperandType.InlineI8)
                Current.operand = Convert.ToInt64(newValue);
            else
                Current.operand = Convert.ToInt32(newValue);
        }

        /// <summary>
        /// Change current line's load to a new value. Can be any integer type. Throws if current isn't OpCode.Ldc_i.
        /// </summary>
        public void ReplaceConstant(long newValue)
        {
            if (!OpCode_Ldc_i.Contains(Current.opcode))
                throw new InvalidOperationException("IL must load constant");

            if (Current.opcode.OperandType == OperandType.InlineNone)
                Current.opcode = OpCodes.Ldc_I4;

            if (Current.opcode.OperandType == OperandType.InlineI8)
                Current.operand = newValue;
            else
                Current.operand = (int)newValue;
        }

        /// <summary>
        /// Change current line's load to a new value. Can be any float-point type. Throws if current isn't OpCode.Ldc_r.
        /// </summary>
        public void ReplaceConstant(double newValue)
        {
            if (!OpCode_Ldc_r.Contains(Current.opcode))
                throw new InvalidOperationException("IL must load constant");

            if (Current.opcode.OperandType == OperandType.ShortInlineR)
                Current.operand = (float)newValue;
            else
                Current.operand = newValue;
        }

        /// <summary>
        /// Change load to a new value. Can be any integer type. Throws if current isn't OpCode.Ldc_i.<br/>
        /// Optionally define start and end index.<br/>
        /// <b>0 and 1 may change boolean values too.</b>
        /// </summary>
        public int ReplaceAllConstant(Enum oldValue, Enum newValue, int start = 0, int end = -1)
        {
            int counter = 0;
            int startedAt = Index;
            if (end < 0 || end > Count)
                end = Count;

            Index = start;
            while (Index < end)
            {
                if (Current.LoadsConstant(oldValue))
                {
                    ReplaceConstant(newValue);
                    counter++;
                }
                Index++;
            }

            Index = startedAt;
            return counter;
        }

        /// <summary>
        /// Change load to a new value. Can be any integer type. Throws if current isn't OpCode.Ldc_i.<br/>
        /// Optionally define start and end index.<br/>
        /// <b>0 and 1 may change boolean values too.</b>
        /// </summary>
        public int ReplaceAllConstant(long oldValue, long newValue, int start = 0, int end = -1)
        {
            int counter = 0;
            int startedAt = Index;
            if (end < 0 || end > Count)
                end = Count;

            Index = start;
            while (Index < end)
            {
                if (Current.LoadsConstant(oldValue))
                {
                    ReplaceConstant(newValue);
                    counter++;
                }
                Index++;
            }

            Index = startedAt;
            return counter;
        }

        /// <summary>
        /// Change load to a new value. Can be any float-point type. Throws if current isn't OpCode.Ldc_r.<br/>
        /// Optionally define start and end index.
        /// </summary>
        public int ReplaceAllConstant(double oldValue, double newValue, int start = 0, int end = -1)
        {
            int counter = 0;
            int startedAt = Index;
            if (end < 0 || end > Count)
                end = Count;

            Index = start;
            while (Index < end)
            {
                if (Current.LoadsConstant(oldValue))
                {
                    ReplaceConstant(newValue);
                    counter++;
                }
                Index++;
            }

            Index = startedAt;
            return counter;
        }

        /// <summary>
        /// Change current line to NOP.
        /// </summary>
        public void ReplaceNOP()
        {
            Current.opcode = OpCodes.Nop;
            Current.operand = null;
        }

        /// <summary>
        /// Moves index forward until a branched jump occures. Then injects jump to the same label. <br/>
        /// Throws if a different type of jump is found first.
        /// </summary>
        public void NextJumpAlways()
        {
            for (; Index < Code.Count; Index++)
            {
                var line = Current;

                if (line.opcode.FlowControl == FlowControl.Branch)
                    throw new InvalidOperationException("unexpected branch");
                if (line.opcode == OpCodes.Switch)
                    throw new InvalidOperationException("unexpected switch");

                if (line.opcode.FlowControl != FlowControl.Cond_Branch)
                    continue;

                Logger.PrintDebug($"Transpiler NextJumpAlways @{Index} {line.opcode}");

                if (line.opcode.OperandType == OperandType.InlineBrTarget)
                    Code.Insert(++Index, new CodeInstruction(OpCodes.Br, line.operand));
                else if (line.opcode.OperandType == OperandType.ShortInlineBrTarget)
                    Code.Insert(++Index, new CodeInstruction(OpCodes.Br_S, line.operand));
                else
                    throw new Exception($"Did not expect this OpCode {line.opcode}");

                return;
            }
            throw new IndexOutOfRangeException("IsLast");
        }

        /// <summary>
        /// Moves index forward until a branched jump occures. Then replaces its label with a label to the next line.<br/>
        /// Throws if a different type of jump is found first.
        /// </summary>
        public void NextJumpNever()
        {
            for (; Index < Code.Count; Index++)
            {
                var line = Current;

                if (line.opcode.FlowControl == FlowControl.Branch)
                    throw new InvalidOperationException("unexpected branch");
                if (line.opcode == OpCodes.Switch)
                    throw new InvalidOperationException("unexpected switch");

                if (line.opcode.FlowControl != FlowControl.Cond_Branch)
                    continue;

                Logger.PrintDebug($"Transpiler NextJumpNever @{Index} {line.opcode}");

                line.operand = (Label)GetLabel(Next);

                //if (line.opcode.StackBehaviourPush != StackBehaviour.Push0)
                //    throw new Exception($"Cond_Branch should not push onto stack {line.opcode}");
                //var num = GetStackChange(line.opcode.StackBehaviourPop);
                //if (num == 0)
                //{
                //    line.opcode = OpCodes.Nop;
                //    line.operand = null;
                //}
                //else if (num == -1)
                //{
                //    line.opcode = OpCodes.Pop;
                //    line.operand = null;
                //}
                //else if (num == -2)
                //{
                //    line.opcode = OpCodes.Pop;
                //    line.operand = null;
                //    Code.Insert(Index++, new CodeInstruction(OpCodes.Pop));
                //}
                //else
                //    throw new Exception($"Cond_Branch should not pop more than 2 {line.opcode}");

                return;
            }
            throw new IndexOutOfRangeException("IsLast");
        }

        #endregion

        #region Labels

        /// <summary>
        /// Get a label to a specific CodeInstruction. Name must be attached to this CodeInstruction or be null.
        /// </summary>
        public LabelInfo GetLabel(CodeInstruction line, string name = null, bool canMake = true)
        {
            LabelInfo label;
            if (name != null)
            {
                label = LabelsExtended.Find(f => f.Name == name);
                if (!label.IsEmpty)
                {
                    if (!line.labels.Contains(label))
                        throw new Exception($"Label not attached to correct CodeInstruction: {name}");
                    return label;
                }
            }

            if (line.labels.Count == 0)
            {
                if (!canMake)
                    throw new Exception($"CodeInstruction has no label and canMake is false");
                line.labels.Add(label = GetLabel(name, true));
                return label;
            }

            label = LabelsExtended.Find(f => f.Equals(line.labels.First()));
            if (label.IsEmpty)
                throw new Exception($"Missing label in LabelsExtended: {label}");

            return label;
        }

        /// <summary>
        /// Get label with a specific name or create a new label, if name is null or non-existent.
        /// </summary>
        public LabelInfo GetLabel(string name = null, bool canMake = true)
        {
            var info = name == null ? default : LabelsExtended.Find(f => f.Name == name);
            if (info.IsEmpty && canMake)
            {
                info = new(Generator.DefineLabel(), name);
                if (LabelsExtended.Any(a => a.Equals(info)))
                    throw new Exception($"Label with the same index/name already exists! {info}");
                LabelsExtended.Add(info);
            }
            return info;
        }

        /// <summary>
        /// Get label by label index (not line index).
        /// </summary>
        public LabelInfo GetLabel(int number)
        {
            var info = LabelsExtended.Find(f => f.Index == number);
            return info;
        }

        /// <summary>
        /// Set name of label by index.
        /// </summary>
        public void NameLabel(int labelIndex, string name)
        {
            if (LabelsExtended.Any(a => a.Name == name))
                throw new Exception($"Label with the same index/name already exists! {name}");

            int index = LabelsExtended.FindIndex(f => f.Index == labelIndex);
            if (index < 0)
                throw new Exception($"Label with index {labelIndex} doesn't exists! {name}");
            LabelsExtended[index] = new(LabelsExtended[index].Label, name);
        }

        /// <summary>
        /// Set name of label.
        /// </summary>
        public void NameLabel(Label label, string name)
        {
            if (LabelsExtended.Any(a => a.Name == name))
                throw new Exception($"Label with the same index/name already exists! {name}");

            int index = LabelsExtended.FindIndex(f => f.Label == label);
            if (index < 0)
                throw new Exception($"Label with index {label} doesn't exists! {name}");
            LabelsExtended[index] = new(label, name);
        }

        #endregion

        #region Locals

        /// <summary>
        /// Get local with a specific name or create a new local, if name is null or non-existent and type is given.
        /// </summary>
        public LocalInfo GetLocal(string name = null, Type type = null, bool canMake = true)
        {
            var info = name == null ? default : LocalsExtended.Find(f => f.Name == name);
            if (info.IsEmpty && type != null && canMake)
            {
                info = new(Generator.DeclareLocal(type), name);
                LocalsExtended.Add(info);
            }

            if (type != null && !info.IsEmpty && !info.Type.IsTypeCompatible(type))
                throw new InvalidCastException("local type incompatible");

            return info;
        }

        /// <summary>
        /// Get local by index.
        /// </summary>
        public LocalInfo GetLocal(int index, Type type = null)
        {
            var info = LocalsExtended.Find(f => f.Index == index);

            if (type != null && !info.IsEmpty && !info.Type.IsTypeCompatible(type))
                throw new InvalidCastException("local type incompatible");

            return info;
        }

        /// <summary>
        /// Set name of local by index.
        /// </summary>
        public void NameLocal(int localIndex, string name)
        {
            if (LocalsExtended.Any(a => a.Name == name))
                throw new Exception($"Label with the same index/name already exists! {name}");

            int index = LocalsExtended.FindIndex(f => f.Index == localIndex);
            if (index < 0)
                throw new Exception($"Local with index {localIndex} doesn't exists! {name}");
            LocalsExtended[index] = new(LocalsExtended[index].Local, name);
        }

        /// <summary>
        /// Set name of local.
        /// </summary>
        public void NameLocal(LocalVariableInfo local, string name)
        {
            if (LocalsExtended.Any(a => a.Name == name))
                throw new Exception($"Label with the same index/name already exists! {name}");

            int index = LocalsExtended.FindIndex(f => f.Local.LocalIndex == local.LocalIndex);
            if (index < 0)
                throw new Exception($"Local with index {local.LocalIndex} doesn't exists! {name}");
            LocalsExtended[index] = new(local, name);
        }

        /// <summary>
        /// Get local of current line. May return empty LocalInfo.
        /// </summary>
        public LocalInfo GetLocal()
        {
            int index = GetLocalIndex();
            if (index < 0)
                return default;
            return LocalsExtended.Find(a => a.Index == index);
        }

        /// <summary>
        /// Get index of local of current line. May return -1.
        /// </summary>
        public int GetLocalIndex()
        {
            if (!Current.IsLdloc() && !Current.IsStloc())
                return -1;
            if (Current.operand is LocalVariableInfo lv)
                return lv.LocalIndex;
            if (Current.operand is int integer)
                return integer;
            if (int.TryParse(Current.opcode.Name.Last().ToString(), out int value))
                return value;
            return -1;
        }

        #endregion

        #region IList<T>

        /// <summary></summary>
        public int IndexOf(CodeInstruction item)
        {
            return Code.IndexOf(item);
        }

        /// <summary></summary>
        public void Insert(int index, CodeInstruction item)
        {
            if (item.operand is LabelInfo label)
                item.operand = (Label)label;
            Code.Insert(index, item);
        }

        /// <summary></summary>
        public void RemoveAt(int index)
        {
            Code.RemoveAt(index);
        }

        /// <summary></summary>
        public void Add(CodeInstruction item)
        {
            if (item.operand is LabelInfo label)
                item.operand = (Label)label;
            Code.Add(item);
        }

        /// <summary></summary>
        public void Clear()
        {
            Code.Clear();
        }

        /// <summary></summary>
        public bool Contains(CodeInstruction item)
        {
            return Code.Contains(item);
        }

        /// <summary></summary>
        public void CopyTo(CodeInstruction[] array, int arrayIndex)
        {
            Code.CopyTo(array, arrayIndex);
        }

        /// <summary></summary>
        public bool Remove(CodeInstruction item)
        {
            return Code.Remove(item);
        }

        /// <summary></summary>
        public IEnumerator<CodeInstruction> GetEnumerator()
        {
            return Code.GetEnumerator();
        }

        /// <summary></summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Code).GetEnumerator();
        }

        #endregion

        #region Statics

        /// <summary>Collection of Ldc_i opcodes.</summary>
        public static OpCode[] OpCode_Ldc_i = new OpCode[]
        {
            OpCodes.Ldc_I4_M1,
            OpCodes.Ldc_I4_0,
            OpCodes.Ldc_I4_1,
            OpCodes.Ldc_I4_2,
            OpCodes.Ldc_I4_3,
            OpCodes.Ldc_I4_4,
            OpCodes.Ldc_I4_5,
            OpCodes.Ldc_I4_6,
            OpCodes.Ldc_I4_7,
            OpCodes.Ldc_I4_8,
            OpCodes.Ldc_I4,
            OpCodes.Ldc_I4_S,
            OpCodes.Ldc_I8,
        };

        /// <summary>Collection of Ldc_r opcodes.</summary>
        public static OpCode[] OpCode_Ldc_r = new OpCode[]
        {
            OpCodes.Ldc_R4,
            OpCodes.Ldc_R8,
        };

        /// <summary>Collection of member opcodes.</summary>
        public static OpCode[] OpCode_Member = new OpCode[]
        {
            OpCodes.Call,
            OpCodes.Callvirt,
            OpCodes.Ldsfld,
            OpCodes.Ldfld,
        };

        /// <summary>
        /// Increment index by 1.
        /// </summary>
        public static TranspilerTool operator ++(TranspilerTool a)
        {
            a.Index++;
            return a;
        }

        /// <summary>
        /// Decrement index by 1.
        /// </summary>
        public static TranspilerTool operator --(TranspilerTool a)
        {
            a.Index--;
            return a;
        }

        /// <summary>
        /// Cache of MemberInfo.
        /// </summary>
        protected static readonly CacheData<MemberInfo> _memberCache = new(a => GetMemberInfo((Type)a[0], (string)a[1], (Type[])a[2], (Type[])a[3]));

        /// <summary>
        /// Get MemberInfo for a specific method, field, or get-property.
        /// </summary>
        /// <param name="type">Declaring type.</param>
        /// <param name="name">Method or field name.</param>
        /// <param name="parameters">Only for methods.</param>
        /// <param name="generics">Only for methods.</param>
        public static MemberInfo GetMemberInfo(Type type, string name, Type[] parameters = null, Type[] generics = null)
        {
            return (MemberInfo)AccessTools.Method(type, name, parameters, generics)
                ?? (MemberInfo)AccessTools.PropertyGetter(type, name)
                ?? (MemberInfo)AccessTools.Field(type, name)
                ?? throw new ArgumentException($"could not find anything for type={type} name={name} parameters={parameters.Join()} generics={generics.Join()}");
        }

        /// <summary>
        /// Returns value of stack change. Zero has no change. Negative values pop from stack. Positive values push to stack.
        /// </summary>
        public static int GetStackChange(StackBehaviour stack)
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

        /// <summary>
        /// Throws if type cannot be assigned to parameter. Handles by-ref resolution.<br/>
        /// Returns true, if operation must use dereference.
        /// </summary>
        public static bool ThrowParameterIncompatible(ParameterInfo parameter, Type right) => ThrowParameterIncompatible(parameter?.ParameterType, right);

        /// <summary>
        /// Throws if type cannot be assigned to parameter. Handles by-ref resolution.<br/>
        /// Returns true, if operation must use dereference.
        /// </summary>
        public static bool ThrowParameterIncompatible(Type left, Type right)
        {
            if (left == null)
                throw new ArgumentNullException("left is null");
            if (right == null)
                throw new ArgumentNullException("right is null");

            if (!left.IsByRef && right.IsByRef)
                throw new ExceptionMissingRef($"parameter must be by ref, if type is by ref '{left}' - '{right}'");

            if (left == right)
                return false;

            if (left.IsByRef)
            {
                if (left.GetElementType() == right)
                    return true;
                throw new InvalidCastException($"by ref parameters must match type exactly '{left}' - '{right}'");
            }

            if (left.IsValueType || right.IsValueType)
                throw new InvalidCastException($"value-type parameters must match type exactly '{left}' - '{right}'");

            if (!left.IsAssignableFrom(right))
                throw new InvalidCastException($"cannot cast '{left}' - '{right}'");

            return false;
        }

        /// <summary>
        /// Untested. Checks if a particular method has calls to another method.<br/>
        /// Does not resolve wrapped method calls (e.g. lambda expressions).
        /// </summary>
        public static bool HasMethodCall(MethodInfo parentMethod, MethodInfo lookingforMethod)
        {
            foreach (var line in PatchProcessor.ReadMethodBody(parentMethod))
            {
                if (lookingforMethod.Equals(line.Value))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Untested. Returns all methods in all assemblies calling a particular method.
        /// </summary>
        public static IEnumerable<MethodInfo> GetCallers(MethodInfo lookingforMethod, string[] includeAssemblies = null, string[] excludeAssemblies = null)
        {
            //var patches = Harmony.GetPatchInfo(info);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.FullName;
                if (includeAssemblies != null && !includeAssemblies.Any(name.StartsWith))
                    continue;
                if (excludeAssemblies != null && excludeAssemblies.Any(name.StartsWith))
                    continue;

                foreach (var @class in AccessTools.GetTypesFromAssembly(assembly))
                {
                    foreach (var parentMethod in @class.GetMethods(BindingAll))
                    {
                        if (HasMethodCall(parentMethod, lookingforMethod))
                            yield return parentMethod;
                    }
                }
            }
        }

        /// <summary>
        /// Untested. Returns <i><paramref name="parentMethod"/></i> and embedded methods which call <i><paramref name="lookingforMethod"/></i>.
        /// Does resolve some wrapped method calls (e.g. lambda expressions).
        /// </summary>
        public static IEnumerable<MethodInfo> GetCallersR(MethodInfo parentMethod, MethodInfo lookingforMethod, bool onlyParent = false)
        {
            bool didReturn = false;
            foreach (var line in PatchProcessor.ReadMethodBody(parentMethod))
            {
                if (!didReturn && lookingforMethod.Equals(line.Value))
                {
                    didReturn = true;
                    yield return parentMethod;
                }

                // ldftn - MethodInfo
                // newobj - System.Action or System.Func
                // if buffered: dup + stsfld
                if (line.Key == OpCodes.Ldftn && line.Value is MethodInfo lambda
                    && (!onlyParent || lambda.DeclaringType == parentMethod.DeclaringType || lambda.DeclaringType?.DeclaringType == parentMethod.DeclaringType)
                    && HasMethodCall(lambda, lookingforMethod))
                {
                    yield return lambda;
                }
            }
        }

        #endregion
    }

    #region Structs

    /// <summary>
    /// Struct to hold Label and string.<br/>
    /// Use <see cref="TranspilerTool.GetLabel(string, bool)"/> to get or create new labels.
    /// </summary>
    public readonly struct LabelInfo
    {
        /// <summary></summary>
        public readonly Label Label;
        /// <summary></summary>
        public readonly string Name;

        /// <summary></summary>
        public int Index => !IsEmpty ? Label.GetHashCode() : -1;
        /// <summary></summary>
        public bool IsEmpty => Name == null;

        /// <summary></summary>
        internal LabelInfo(Label label, string name = null)
        {
            this.Label = label;
            this.Name = name ?? "L_" + label.GetHashCode();
        }

        /// <summary></summary>
        internal LabelInfo(int label, string name)
        {
            if (label < 0)
                throw new IndexOutOfRangeException();
            this.Label = (Label)typeof(Label).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0].Invoke(new object[] { label });
            this.Name = name ?? throw new ArgumentNullException();
        }

        /// <summary></summary>
        public override string ToString()
        {
            return $"[Label:{Index}:{Name}]";
        }

        /// <summary>
        /// Compares LabelInfo, int, Label, or string. Always returns false if IsEmpty.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is LabelInfo info)
                return this.Index == info.Index || this.Name == info.Name;
            if (obj is int integer)
                return this.Index == integer;
            if (obj is Label label)
                return this.Index == label.GetHashCode();
            if (obj is string name)
                return this.Name == name;
            return false;
        }

        /// <summary></summary>
        public override int GetHashCode()
        {
            return Index;
        }

        /// <summary></summary>
        public static implicit operator Label(LabelInfo info) => info.Label;

        /// <summary></summary>
        public static implicit operator int(LabelInfo info) => info.Index;
    }

    /// <summary>
    /// Struct to hold LocalVariableInfo and string.
    /// </summary>
    public readonly struct LocalInfo
    {
        /// <summary></summary>
        public readonly LocalVariableInfo Local;
        /// <summary></summary>
        public readonly string Name;

        /// <summary></summary>
        public int Index => Local?.LocalIndex ?? -1;
        /// <summary></summary>
        public Type Type => Local?.LocalType;
        /// <summary></summary>
        public bool IsEmpty => Name == null;

        /// <summary></summary>
        internal LocalInfo(LocalVariableInfo local, string name)
        {
            this.Local = local ?? throw new ArgumentNullException();
            this.Name = name ?? throw new ArgumentNullException();
        }

        /// <summary></summary>
        public override string ToString()
        {
            return $"[Local:{Index}:{Name}:{Type?.Name}]";
        }

        /// <summary>
        /// Compares LocalInfo, int, LocalVariableInfo, or string. Always returns false if IsEmpty.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is LocalInfo info)
                return this.Index == info.Index || this.Name == info.Name;
            if (obj is int integer)
                return this.Index == integer;
            if (obj is LocalVariableInfo info2)
                return this.Index == info2.LocalIndex;
            if (obj is string name)
                return this.Name == name;
            return false;
        }

        /// <summary></summary>
        public override int GetHashCode()
        {
            return Index;
        }

        /// <summary></summary>
        public static implicit operator LocalVariableInfo(LocalInfo info) => info.Local;

        /// <summary></summary>
        public static implicit operator int(LocalInfo info) => info.Index;
    }

    #endregion
}
