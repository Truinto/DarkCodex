using HarmonyLib;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace TestUnity
{
    [TestClass]
    public class TranspilerTests
    {
        private static readonly Harmony harmony = new("TranspilerTests");
        const int TestValue = 2;

        [TestMethod]
        public void TestReplaceCallFail()
        {
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchStatic)), transpiler: new(((Delegate)ReplaceTestFails).Method));
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchInstance)), transpiler: new(((Delegate)ReplaceTestFails).Method));
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchReplaceField)), transpiler: new(((Delegate)ReplaceTestFails).Method));
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchReplaceField2)), transpiler: new(((Delegate)ReplaceTestFails).Method));
            harmony.UnpatchAll();
        }
        public static IEnumerable<CodeInstruction> ReplaceTestFails(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            TranspilerTool tools;

            if (original.Name == nameof(TestTarget.PatchReplaceField))
            {
                tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
                Assert.ThrowsException<ArgumentException>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.FieldStatic), FailFieldInvalidInstance));

                tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
                Assert.ThrowsException<ExceptionMissingReturn>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.FieldStatic), FailFieldMissingReturn));

                tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
                Assert.ThrowsException<ExceptionMissingReturn>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.FieldStatic), FailFieldInvalidReturn));

                return instr;
            }

            if (original.Name == nameof(TestTarget.PatchReplaceField2))
            {
                tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
                Assert.ThrowsException<ExceptionMissingInstance>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.FieldInstance), FailFieldMissingInstance));

                return instr;
            }

            // PatchStatic
            if (original.IsStatic)
            {
                tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
                Assert.ThrowsException<ExceptionInvalidInstance>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Static), FailStatic));

                tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
                Assert.ThrowsException<ExceptionMissingRef>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Static), FailMissingRef));

                tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
                Assert.ThrowsException<InvalidCastException>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Static), FailWrongOriginalType));

                // different target
                tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
                Assert.ThrowsException<InvalidCastException>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Static), FailWrongOriginalType));

                return instr;
            }

            //PatchInstance
            tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
            Assert.ThrowsException<ExceptionMissingReturn>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Instance), FailMissingReturn));

            tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
            Assert.ThrowsException<ExceptionMissingInstance>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Instance), FailMissingInstance));

            tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
            Assert.ThrowsException<ExceptionMissingParameter>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Instance), FailMissingParameter));

            tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
            Assert.ThrowsException<ExceptionInvalidParameterOrder>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Instance), FailWrongOrder));

            tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
            Assert.ThrowsException<ArgumentException>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Instance), FailWrongParameter));

            tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
            Assert.ThrowsException<InvalidCastException>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Instance), FailWrongType));

            tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
            Assert.ThrowsException<InvalidCastException>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Instance), FailWrongType2));

            tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
            Assert.ThrowsException<InvalidCastException>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Instance), FailTypeIncompatible));

            tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
            Assert.ThrowsException<InvalidCastException>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Instance), FailTypeIncompatible2));

            tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
            Assert.ThrowsException<InvalidCastException>(() => tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Instance), FailTypeIncompatible3));

            return instr;
        }
        public static int FailStatic(int number1, int number2, TestTarget __instance)
        {
            return 2;
        }
        public static int FailMissingRef(int number1, int number2, int numberByRef)
        {
            return 2;
        }
        public static int FailWrongOriginalType(int number1, int number2, long number)
        {
            return 2;
        }
        public static void FailMissingReturn(TestRemote remote, int number1, int number2)
        {
        }
        public static int FailMissingInstance(int number1, int number2)
        {
            return 2;
        }
        public static int FailMissingParameter(TestRemote remote, int number1)
        {
            return 2;
        }
        public static int FailWrongOrder(TestRemote remote, int number2, int number1)
        {
            return 2;
        }
        public static int FailWrongParameter(TestRemote remote, int number1, int number2, string invalid)
        {
            return 2;
        }
        public static int FailWrongType(TestRemote remote, int number1, long number2)
        {
            return 2;
        }
        public static int FailWrongType2(TestRemote remote, int number1, ref int number2)
        {
            return 2;
        }
        public static int FailTypeIncompatible(TestRemote remote, int number1, int number2, ref IList<int> V_0)
        {
            return 2;
        }
        public static int FailTypeIncompatible2(TestRemote remote, int number1, int number2, ref SuperList<int> V_0)
        {
            return 2;
        }
        public static int FailTypeIncompatible3(TestRemote remote, int number1, int number2, SuperList<int> V_0)
        {
            return 2;
        }
        public static int FailFieldInvalidInstance(TestRemote instance)
        {
            return 2;
        }
        public static int FailFieldMissingInstance()
        {
            return 2;
        }
        public static void FailFieldMissingReturn()
        {
        }
        public static long FailFieldInvalidReturn()
        {
            return 2L;
        }


        [TestMethod]
        public void TestReplaceCallInstance()
        {
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchInstance)), transpiler: new(((Delegate)ReplaceCallInstance).Method));
            new TestTarget().PatchInstance();
            harmony.UnpatchAll();
        }
        public static IEnumerable<CodeInstruction> ReplaceCallInstance(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            var tools = new TranspilerTool(instr, generator, original);
            Assert.IsTrue(1 == tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Instance), PatchReplaceCallInstance));
            return tools;
        }
        public static int PatchReplaceCallInstance(object remote, int number1, int number2)
        {
            return 2;
        }



        [TestMethod]
        public void TestReplaceCallInstanceL()
        {
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchInstance)), transpiler: new(((Delegate)ReplaceCallInstanceL).Method));
            new TestTarget().PatchInstance();
            harmony.UnpatchAll();
        }
        public static IEnumerable<CodeInstruction> ReplaceCallInstanceL(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            var tools = new TranspilerTool(instr, generator, original);
            Assert.IsTrue(1 == tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Instance), PatchReplaceCallInstanceL));
            return tools;
        }
        public static int PatchReplaceCallInstanceL(TestRemote remote, int number1, int number2, [LocalParameter(index: 0)] IList<int> local, [LocalParameter(IndexByType = 0)] ref int localByRef, ref List<int> V_0)
        {
            Assert.AreSame(local, V_0);
            Assert.AreEqual(8, local.First());
            Assert.AreEqual(8, localByRef);

            local[0] = 2;
            localByRef = 2;

            return remote.Instance(number1, number2);
        }



        [TestMethod]
        public void TestReplaceCallStatic()
        {
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchStatic)), transpiler: new(((Delegate)ReplaceCallStatic).Method));
            int local0 = 8;
            TestTarget.PatchStatic(8, ref local0);
            harmony.UnpatchAll();
        }
        public static IEnumerable<CodeInstruction> ReplaceCallStatic(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            var tools = new TranspilerTool(instr, generator, original);
            Assert.IsTrue(1 == tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Static), PatchReplaceCallStatic));
            return tools;
        }
        public static int PatchReplaceCallStatic(int number1, int number2, ref int number, ref int numberByRef)
        {
            Assert.AreEqual(8, number);
            Assert.AreEqual(8, numberByRef);
            number = 2;
            numberByRef = 2;
            return 2;
        }



        [TestMethod]
        public void TestReplaceCallReference()
        {
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchStatic)), transpiler: new(((Delegate)ReplaceCallReference).Method));
            int local0 = 2;
            TestTarget.PatchStatic(2, ref local0);
            harmony.UnpatchAll();
        }
        public static IEnumerable<CodeInstruction> ReplaceCallReference(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            var tools = new TranspilerTool(instr, generator, original);
            Assert.IsTrue(1 == tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.ReferenceTest), PatchReplaceCallReference));
            return tools;
        }
        public static void PatchReplaceCallReference(int number_noref, ref int number_ref, int number, ref int numberByRef)
        {
            Assert.AreEqual(8, number_noref);
            Assert.AreEqual(8, number_ref);
            Assert.AreEqual(2, number);
            Assert.AreEqual(2, numberByRef);
            number_ref = 2;
        }



        [TestMethod]
        public void TestReplaceCallParameter()
        {
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchParameter)), transpiler: new(((Delegate)ReplaceCallParameter).Method));
            int local0 = 8;
            List<int> local1 = new List<int>() { 8 };
            new TestTarget().PatchParameter(8, ref local0, new List<int>() { 8 }, ref local1);
            harmony.UnpatchAll();
        }
        public static IEnumerable<CodeInstruction> ReplaceCallParameter(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            var tools = new TranspilerTool(instr, generator, original);
            Assert.IsTrue(1 == tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.Instance), PatchReplaceCallParameter));
            return tools;
        }
        public static int PatchReplaceCallParameter(TestRemote remote, int number1, int number2, ref int number, ref int numberByRef, [OriginalParameter("numbers")] List<int> numbersRename, TestTarget __instance, ref List<int> numbersByRef)
        {
            Assert.IsInstanceOfType(__instance, typeof(TestTarget));
            Assert.AreEqual(8, number);
            Assert.AreEqual(8, numberByRef);
            Assert.AreEqual(8, numbersRename.First());
            Assert.AreEqual(8, numbersByRef.First());

            number = 2;
            numberByRef = 2;
            numbersRename[0] = 2;
            numbersByRef[0] = 2;

            return remote.Instance(number1, number2);
        }



        [TestMethod]
        public void TestReplaceCallFieldStatic()
        {
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchReplaceField)), transpiler: new(((Delegate)ReplaceCallFieldStatic).Method));
            TestTarget.PatchReplaceField();
            harmony.UnpatchAll();
        }
        public static IEnumerable<CodeInstruction> ReplaceCallFieldStatic(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            var tools = new TranspilerTool(instr, generator, original);
            Assert.IsTrue(1 == tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.FieldStatic), PatchReplaceCallFieldStatic));
            return tools;
        }
        public static int PatchReplaceCallFieldStatic()
        {
            return 2;
        }



        [TestMethod]
        public void TestReplaceCallFieldInstance()
        {
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchReplaceField2)), transpiler: new(((Delegate)ReplaceCallFieldInstance).Method));
            new TestTarget().PatchReplaceField2();
            harmony.UnpatchAll();
        }
        public static IEnumerable<CodeInstruction> ReplaceCallFieldInstance(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            var tools = new TranspilerTool(instr, generator, original);
            Assert.IsTrue(1 == tools.ReplaceAllCalls(typeof(TestRemote), nameof(TestRemote.FieldInstance), PatchReplaceCallFieldInstance));
            return tools;
        }
        public static int PatchReplaceCallFieldInstance(TestRemote instance)
        {
            return 2;
        }



        [TestMethod]
        public void TestInsertCallFail()
        {
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchStatic)), transpiler: new(((Delegate)InsertCallFails).Method));
            harmony.UnpatchAll();
        }
        public static IEnumerable<CodeInstruction> InsertCallFails(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            TranspilerTool tools;

            tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
            Assert.ThrowsException<ArgumentException>(() => tools.InsertAfter(Fail2MissingReturn));

            tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
            Assert.ThrowsException<ExceptionInvalidReturn>(() => tools.InsertAfter(Fail2HasReturn));

            tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
            Assert.ThrowsException<ExceptionInvalidReturn>(() => tools.InsertAfter(Fail2HasReturn));

            return instr;
        }
        public static void Fail2MissingReturn(int __stack)
        {
        }
        public static int Fail2HasReturn()
        {
            return 2;
        }
        public static int Fail2WrongReturn(long __stack)
        {
            return 2;
        }



        [TestMethod]
        public void TestInsertCall()
        {
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchStatic)), transpiler: new(((Delegate)InsertCall).Method));
            int local0 = 8;
            TestTarget.PatchStatic(8, ref local0);
            Assert.AreEqual(2, local0);
            harmony.UnpatchAll();
        }
        public static IEnumerable<CodeInstruction> InsertCall(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            var tools = new TranspilerTool(instr, generator, original);
            tools.Seek(typeof(TestRemote), nameof(TestRemote.Static));
            tools.InsertAfter(PatchInsertCall);
            return tools;
        }
        public static int PatchInsertCall(int __stack, ref int number, [OriginalParameter("numberByRef")] ref int numberRename)
        {
            Assert.AreEqual(8, __stack);
            Assert.AreEqual(8, number);
            Assert.AreEqual(8, numberRename);
            number = 2;
            numberRename = 2;
            return 2;
        }



        [TestMethod]
        public void TestInsertReturnFail()
        {
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchReturn)), transpiler: new(((Delegate)InsertReturnFail).Method));
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchInstance)), transpiler: new(((Delegate)InsertReturnFail).Method));
            harmony.UnpatchAll();
        }
        public static IEnumerable<CodeInstruction> InsertReturnFail(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            TranspilerTool tools;

            // PatchReturn
            if ((original as MethodInfo).ReturnType != typeof(void))
            {
                tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
                Assert.ThrowsException<ExceptionMissingResult>(() => tools.InsertReturn(Fail3MissingResult, true));

                tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
                Assert.ThrowsException<ExceptionMissingRef>(() => tools.InsertReturn(Fail3MissingRef, true));

                tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
                Assert.ThrowsException<ExceptionInvalidReturn>(() => tools.InsertReturn(Fail3InvalidReturn, true));

                tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
                Assert.ThrowsException<InvalidCastException>(() => tools.InsertReturn(Fail3InvalidResultType, true));

                return instr;
            }

            // PatchInstance
            tools = new TranspilerTool(AccessTools.MakeDeepCopy<List<CodeInstruction>>(instr), generator, original);
            Assert.ThrowsException<ExceptionInvalidResult>(() => tools.InsertReturn(Fail3InvalidResult, true));

            return instr;
        }
        public static void Fail3MissingResult()
        {
        }
        public static void Fail3MissingRef(long __result)
        {
        }
        public static int Fail3InvalidReturn(ref long __result)
        {
            return 2;
        }
        public static void Fail3InvalidResultType(ref int __result)
        {
        }
        public static void Fail3InvalidResult(ref long __result)
        {
        }



        [TestMethod]
        public void TestInsertReturnVoid()
        {
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchStatic)), transpiler: new(((Delegate)InsertReturnVoid).Method));
            int local0 = 8;
            TestTarget.PatchStatic(8, ref local0);
            Assert.AreEqual(2, local0);
            harmony.UnpatchAll();
        }
        public static IEnumerable<CodeInstruction> InsertReturnVoid(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            var tools = new TranspilerTool(instr, generator, original);
            tools.Seek(typeof(TestRemote), nameof(TestRemote.Static));
            tools.Offset(1);
            tools.InsertReturn(PatchInsertReturnVoid, false);
            return tools;
        }
        public static void PatchInsertReturnVoid(ref int number, ref int numberByRef)
        {
            Assert.AreEqual(8, number);
            Assert.AreEqual(8, numberByRef);
            numberByRef = 2;
        }



        [TestMethod]
        public void TestInsertReturnVoidConditional()
        {
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchCondition)), transpiler: new(((Delegate)InsertReturnVoidConditional).Method));
            int local0 = 2;
            new TestTarget().PatchCondition(8, ref local0);
            Assert.AreEqual(2, local0);
            harmony.UnpatchAll();
        }
        public static IEnumerable<CodeInstruction> InsertReturnVoidConditional(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            var tools = new TranspilerTool(instr, generator, original);
            tools.InsertReturn(PatchInsertReturnVoidConditional, true);
            return tools;
        }
        public static bool PatchInsertReturnVoidConditional(TestTarget __instance)
        {
            Assert.IsNotNull(__instance);
            return false;
        }



        [TestMethod]
        public void TestInsertReturnConditionalFalse()
        {
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchReturn)), transpiler: new(((Delegate)InsertReturnConditionalFalse).Method));
            var result = TestTarget.PatchReturn();
            Assert.AreEqual(2, result);
            harmony.UnpatchAll();
        }
        public static IEnumerable<CodeInstruction> InsertReturnConditionalFalse(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            var tools = new TranspilerTool(instr, generator, original);
            tools.InsertReturn(PatchInsertReturnConditionalFalse, true);
            return tools;
        }
        public static bool PatchInsertReturnConditionalFalse(ref long __result)
        {
            __result = 2;
            return false;
        }



        [TestMethod]
        public void TestInsertReturnConditionalTrue()
        {
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchReturn)), transpiler: new(((Delegate)transpiler).Method));
            var result = TestTarget.PatchReturn();
            Assert.AreEqual(8, result);
            harmony.UnpatchAll();

            static IEnumerable<CodeInstruction> transpiler(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
            {
                var tools = new TranspilerTool(instr, generator, original);
                tools.InsertReturn(patch, true);
                return tools;
            }

            static bool patch(ref long __result)
            {
                __result = 2;
                return true;
            }
        }



        [TestMethod]
        public void TestInsertJump()
        {
            harmony.Patch(AccessTools.Method(typeof(TestTarget), nameof(TestTarget.PatchCondition)), transpiler: new(((Delegate)transpiler).Method));
            int local0 = 10;
            new TestTarget().PatchCondition(8, ref local0);
            Assert.AreEqual(10, local0);
            new TestTarget().PatchCondition(3, ref local0);
            Assert.AreEqual(6, local0);
            harmony.UnpatchAll();

            static IEnumerable<CodeInstruction> transpiler(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
            {
                var tools = new TranspilerTool(instr, generator, original);
                var label = tools.GetLabel(tools.Code.Last());
                tools.Seek(OpCodes.Ldarg_2);
                tools.InsertJump(patch, label, true);
                return tools;
            }

            static bool patch(int number)
            {
                if (number == 8)
                    return true;
                else
                    return false;
            }
        }



        public class TestTarget
        {
            /// <summary>challenge: change local variables to 2 or return value to 2</summary>
            public void PatchInstance()
            {
                List<int> local0 = new List<int>() { 8 };
                int local1 = 8;
                int calc = new TestRemote().Instance(3, 5);
                Assert.AreEqual(10, local0.First() + calc);
                Assert.AreEqual(10, local1 + calc);
            }

            /// <summary>challenge: change local variables to 2 or return value to 2; call with 2's or change parameters to 2</summary>
            public static void PatchStatic(int number, ref int numberByRef)
            {
                List<int> local0 = new List<int>() { 8 };
                int local1 = 8;
                int calc = TestRemote.Static(3, 5);
                TestRemote.ReferenceTest(8, ref calc);
                Assert.AreEqual(2, number);
                Assert.AreEqual(2, numberByRef);
                Assert.AreEqual(10, local0.First() + calc, $"local0 {local0.First()} {calc}");
                Assert.AreEqual(10, local1 + calc, $"local1 {local1} {calc}");
            }

            /// <summary>challenge: change original parameters; call all with 8's, then patch to 2</summary>
            public long PatchParameter(int number, ref int numberByRef, List<int> numbers, ref List<int> numbersByRef)
            {
                new TestRemote().Instance(3, 5);
                Assert.AreEqual(2, number);
                Assert.AreEqual(2, numberByRef);
                Assert.AreEqual(2, numbers.First());
                Assert.AreEqual(2, numbersByRef.First());
                return 8L;
            }

            /// <summary>challenge: change return value</summary>
            public static long PatchReturn()
            {
                return 8L;
            }

            public void PatchCondition(int number, ref int numberByRef)
            {
                if (number == 8)
                    numberByRef = 8;
                else
                    numberByRef = number * 2;
            }

            /// <summary>challenge: change static field call to 2</summary>
            public static void PatchReplaceField()
            {
                int local0 = TestRemote.FieldStatic;
                Assert.AreEqual(2, local0);
            }

            /// <summary>challenge: change instance field call to 2</summary>
            public void PatchReplaceField2()
            {
                int local0 = new TestRemote().FieldInstance;
                Assert.AreEqual(2, local0);
            }
        }

        public class TestRemote
        {
            public int FieldInstance = 8;
            public static int FieldStatic = 8;

            public int Instance(int number1, int number2)
            {
                return number1 + number2;
            }

            public static int Static(int number1, int number2)
            {
                return number1 + number2;
            }

            public static void ReferenceTest(int number_noref, ref int number_ref)
            {

            }
        }

        public class SuperList<T> : List<T>
        {
            // this is just to test inheritance
        }
    }
}