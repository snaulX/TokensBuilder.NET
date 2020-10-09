using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TokensAPI;

namespace TokensBuilder
{
    enum CallType : byte
    {
        LoadObject,
        CallMethod,
        LoadField,
        SetField,
        LoadLocal,
        SetLocal,
        LoadOperator,
        NewObject,
        BrEndIf,
        Brfalse
    }

    public static class LaterCalls
    {
        static List<CallType> queueCalls = new List<CallType>();
        static List<FieldInfo> loadFields = new List<FieldInfo>(),
            setFields = new List<FieldInfo>();
        static List<LocalBuilder> loadLocals = new List<LocalBuilder>(),
            setLocals = new List<LocalBuilder>();
        static List<object> loadObjects = new List<object>();
        static List<Type> loadCallerTypesOperators = new List<Type>();
        static List<OperatorType> loadOperators = new List<OperatorType>();
        static List<bool> dontPops = new List<bool>();
        static List<MethodInfo> callMethods = new List<MethodInfo>();
        static List<ConstructorInfo> newObjects = new List<ConstructorInfo>();
        public static Stack<Label> endIfElseLabels = new Stack<Label>(), endIfLabels = new Stack<Label>();
        public static bool brEndIf = false, isLoop = false;

        /// <summary>
        /// Seek. Suffix Sk means that is seek on something
        /// </summary>
        static int orderSk = 0, loadFldSk = 0, setFldSk = 0, loadLclSk = 0, setLclSk = 0, loadObjSk = 0, loadCTOSk = 0,
            loadOpSk = 0, dontPopSk = 0, callMtdSk = 0, newObjSk = 0;

        private static ILGenerator gen => Context.functionBuilder.generator;

        #region Operations
        public static void LoadObject(object value, bool seek = false)
        {
            if (seek)
            {
                queueCalls.Insert(orderSk, CallType.LoadObject);
                loadObjects.Insert(loadObjSk, value);
            }
            else
            {
                queueCalls.Add(CallType.LoadObject);
                loadObjects.Add(value);
            }
        }

        public static void CallMethod(MethodInfo method, bool dontPop = true, bool seek = false)
        {
            if (seek)
            {
                queueCalls.Insert(orderSk, CallType.CallMethod);
                callMethods.Insert(callMtdSk, method);
                dontPops.Insert(dontPopSk, dontPop);
            }
            else
            {
                queueCalls.Add(CallType.CallMethod);
                callMethods.Add(method);
                dontPops.Add(dontPop);
            }
        }

        public static void LoadField(FieldInfo field, bool seek = false)
        {
            if (seek)
            {
                queueCalls.Insert(orderSk, CallType.LoadField);
                loadFields.Insert(loadFldSk, field);
            }
            else
            {
                queueCalls.Add(CallType.LoadField);
                loadFields.Add(field);
            }
        }

        public static void SetField(FieldInfo field, bool seek = false)
        {
            if (seek)
            {
                queueCalls.Insert(0, CallType.SetField);
                setFields.Insert(0, field);
            }
            else 
            { 
                queueCalls.Add(CallType.SetField);
                setFields.Add(field);
            }
        }

        public static void LoadLocal(LocalBuilder local, bool seek = false)
        {
            if (seek)
            {
                queueCalls.Insert(orderSk, CallType.LoadLocal);
                loadLocals.Insert(loadLclSk, local);
            }
            else
            {
                queueCalls.Add(CallType.LoadLocal);
                loadLocals.Add(local);
            }
        }

        public static void SetLocal(LocalBuilder local, bool seek = false)
        {
            if (seek)
            {
                queueCalls.Insert(orderSk, CallType.SetLocal);
                setLocals.Insert(setLclSk, local);
            }
            else
            {
                queueCalls.Add(CallType.SetLocal);
                setLocals.Add(local);
            }
        }

        public static void LoadOperator(Type callerType, OperatorType op, bool seek = false)
        {
            if (seek)
            {
                queueCalls.Insert(orderSk, CallType.LoadOperator);
                loadCallerTypesOperators.Insert(loadCTOSk, callerType);
                loadOperators.Insert(loadOpSk, op);
            }
            else
            {
                queueCalls.Add(CallType.LoadOperator);
                loadCallerTypesOperators.Add(callerType);
                loadOperators.Add(op);
            }
        }

        public static void NewObject(ConstructorInfo ctor, bool seek = false)
        {
            if (seek)
            {
                queueCalls.Insert(orderSk, CallType.NewObject);
                newObjects.Insert(newObjSk, ctor);
            }
            else
            {
                queueCalls.Add(CallType.NewObject);
                newObjects.Add(ctor);
            }
        }
        #endregion

        #region Utility methods (create ifs, loops and etc.)
        public static void CreateEndIfLabel()
        {
            brEndIf = true;
            endIfElseLabels.Push(gen.DefineLabel());
        }

        public static void BrEndIf() => queueCalls.Add(CallType.BrEndIf);

        public static void Brfalse(Label endIfLabel)
        {
            queueCalls.Add(CallType.Brfalse);
            endIfLabels.Push(endIfLabel);
        }

        public static void StartLoop()
        {
            Label startLabel = gen.DefineLabel();
            gen.MarkLabel(startLabel);
            endIfElseLabels.Push(startLabel);
            isLoop = true;
        }
        #endregion

        public static void Call()
        {
            foreach (CallType callType in queueCalls)
            {
                switch (callType)
                {
                    case CallType.LoadObject:
                        Context.LoadObject(loadObjects.Pop());
                        break;
                    case CallType.CallMethod:
                        Context.CallMethod(callMethods.Pop(), dontPops.Pop());
                        break;
                    case CallType.LoadField:
                        Context.LoadField(loadFields.Pop());
                        break;
                    case CallType.SetField:
                        Context.SetField(setFields.Pop());
                        break;
                    case CallType.LoadLocal:
                        Context.LoadLocal(loadLocals.Pop());
                        break;
                    case CallType.SetLocal:
                        Context.SetLocal(setLocals.Pop());
                        break;
                    case CallType.LoadOperator:
                        Context.LoadOperator(loadCallerTypesOperators.Pop(), loadOperators.Pop());
                        break;
                    case CallType.NewObject:
                        Context.NewObject(newObjects.Pop());
                        break;
                    case CallType.BrEndIf:
                        if (brEndIf || isLoop)
                        {
                            gen.Emit(OpCodes.Br, endIfElseLabels.Peek());
                        }
                        gen.MarkLabel(endIfLabels.Pop());
                        break;
                    case CallType.Brfalse:
                        gen.Emit(OpCodes.Brfalse, endIfLabels.Peek());
                        break;
                }
            }
            if (brEndIf)
                gen.MarkLabel(endIfElseLabels.Pop()); // mark label there are
            queueCalls.Clear();
        }

        public static void RemoveLast()
        {
            if (!queueCalls.IsEmpty())
            {
                switch (queueCalls.RemoveLast())
                {
                    case CallType.LoadObject:
                        loadObjects.RemoveLast();
                        break;
                    case CallType.CallMethod:
                        callMethods.RemoveLast();
                        dontPops.RemoveLast();
                        break;
                    case CallType.LoadField:
                        loadFields.RemoveLast();
                        break;
                    case CallType.SetField:
                        setFields.RemoveLast();
                        break;
                    case CallType.LoadLocal:
                        loadLocals.RemoveLast();
                        break;
                    case CallType.SetLocal:
                        setLocals.RemoveLast();
                        break;
                    case CallType.LoadOperator:
                        loadOperators.RemoveLast();
                        loadCallerTypesOperators.RemoveLast();
                        break;
                    case CallType.NewObject:
                        newObjects.RemoveLast();
                        break;
                }
            }
        }

        public static void Seek()
        {
            if (!queueCalls.IsEmpty()) orderSk = queueCalls.Count - 1;
            else orderSk = 0;
            if (!loadLocals.IsEmpty()) loadLclSk = loadLocals.Count - 1;
            else loadLclSk = 0;
            if (!loadFields.IsEmpty()) loadFldSk = loadFields.Count - 1;
            else loadFldSk = 0;
            if (!newObjects.IsEmpty()) newObjSk = newObjects.Count - 1;
            else newObjSk = 0;
            if (!loadCallerTypesOperators.IsEmpty()) 
                loadCTOSk = loadCallerTypesOperators.Count - 1;
            else loadCTOSk = 0;
            if (!loadOperators.IsEmpty()) loadOpSk = loadOperators.Count - 1;
            else loadOpSk = 0;
            if (!loadObjects.IsEmpty()) loadObjSk = loadObjects.Count - 1;
            else loadObjSk = 0;
            if (!setLocals.IsEmpty()) setLclSk = setLocals.Count - 1;
            else setLclSk = 0;
            if (!setFields.IsEmpty()) setFldSk = setFields.Count - 1;
            else setFldSk = 0;
            if (!callMethods.IsEmpty()) callMtdSk = callMethods.Count - 1;
            else callMtdSk = 0;
            if (!dontPops.IsEmpty()) dontPopSk = dontPops.Count - 1;
            else dontPopSk = 0;
        }
    }
}
