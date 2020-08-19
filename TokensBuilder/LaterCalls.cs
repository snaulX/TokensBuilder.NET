using System;
using System.Collections.Generic;
using System.Linq;
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
        NewObject
    }

    public static class LaterCalls
    {
        static List<CallType> orderCalls = new List<CallType>();
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

        #region Operations
        public static void LoadObject(object value)
        {
            orderCalls.Add(CallType.LoadObject);
            loadObjects.Add(value);
        }

        public static void CallMethod(MethodInfo method, bool dontPop = true)
        {
            orderCalls.Add(CallType.CallMethod);
            callMethods.Add(method);
            dontPops.Add(dontPop);
        }

        public static void LoadField(FieldInfo field)
        {
            orderCalls.Add(CallType.LoadField);
            loadFields.Add(field);
        }

        public static void SetField(FieldInfo field)
        {
            orderCalls.Add(CallType.SetField);
            setFields.Add(field);
        }

        public static void LoadLocal(LocalBuilder local)
        {
            orderCalls.Add(CallType.LoadLocal);
            loadLocals.Add(local);
        }

        public static void SetLocal(LocalBuilder local)
        {
            orderCalls.Add(CallType.SetLocal);
            setLocals.Add(local);
        }

        public static void LoadOperator(Type callerType, OperatorType op)
        {
            orderCalls.Add(CallType.LoadOperator);
            loadCallerTypesOperators.Add(callerType);
            loadOperators.Add(op);
        }

        public static void NewObject(ConstructorInfo ctor)
        {
            orderCalls.Add(CallType.NewObject);
            newObjects.Add(ctor);
        }
        #endregion

        public static void Call()
        {
            foreach (CallType callType in orderCalls)
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
                }
            }
            orderCalls.Clear();
        }

        public static void RemoveLast()
        {
            switch (orderCalls.RemoveLast())
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
}
