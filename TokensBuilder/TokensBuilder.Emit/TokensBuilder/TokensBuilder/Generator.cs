using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using TokensAPI;
using System.Linq;

namespace TokensBuilder
{
    public enum OutputType
    {
        Console,
        Library,
        Winexe
    }

    public class Generator
    {
        public ContextInfo context;
        public List<Expression> expressions;

        public Generator()
        {
            expressions = new List<Expression>();
            context = new ContextInfo();
        }

        public void GenerateIL(string assembly_name, string code)
        {
            //create context
            context.appName = assembly_name;
            context.assemblyName.Name = assembly_name;
            context.CreateAssembly();

            //parse code to expressions
            string[] lines = code.Split('\n', '\r');
            for (int i = 0; i < lines.Length; i++)
            {
                string[] ids = lines[i].Split(' ', '\t');
                Expression expr = new Expression();
                expr.token = Main.GetToken(ids[0]);
                for (int j = 1; j < ids.Length; j++)
                {
                    expr.args.Add(Identifer.GetIdentifer(ids[j]));
                }
            }

            //variables for building
            Dictionary<string, string> labels = new Dictionary<string, string>();
            short ifLabels = 0, whileLabels = 0;
            string namespace_name = "";

            //parse expressions
            for (int i = 0; i < expressions.Count; i++)
            {
                Expression e = expressions[i];
                switch (e.token)
                {
                    case Token.NULL:
                        context.ILGenerator.Emit(OpCodes.Nop);
                        break;
                    case Token.USE:
                        context.ILGenerator.UsingNamespace(e.args[0].GetValue());
                        break;
                    case Token.WRITEVAR:
                        break;
                    case Token.NEWCLASS:
                        context.type = context.module.DefineType(e.args[0].GetValue());
                        break;
                    case Token.NEWVAR:
                        break;
                    case Token.NEWFUNC:
                        break;
                    case Token.END:
                        context.ILGenerator.Emit(OpCodes.Br);
                        break;
                    case Token.GETCLASS:
                        break;
                    case Token.GETVAR:
                        break;
                    case Token.GETFUNC:
                        break;
                    case Token.RUNFUNC:
                        break;
                    case Token.WHILE:
                        break;
                    case Token.FOR:
                        break;
                    case Token.FOREACH:
                        break;
                    case Token.BREAK:
                        context.ILGenerator.Emit(OpCodes.Br);
                        break;
                    case Token.CONTINUE:
                        break;
                    case Token.RETURN:
                        break;
                    case Token.IF:
                        break;
                    case Token.ELSE:
                        break;
                    case Token.ELIF:
                        break;
                    case Token.GOTO:
                        break;
                    case Token.LABEL:
                        break;
                    case Token.YIELD:
                        break;
                    case Token.GETLINK:
                        break;
                    case Token.WRITEINPOINTER:
                        break;
                    case Token.NEWSTRUCT:
                        break;
                    case Token.NEWINTERFACE:
                        break;
                    case Token.NEWENUM:
                        break;
                    case Token.NEWMODULE:
                        break;
                    case Token.NEWCONSTRUCTOR:
                        break;
                    case Token.NEWATTRIBUTE:
                        break;
                    case Token.GETATTRIBUTE:
                        break;
                    case Token.GETCONSTRUCTOR:
                        break;
                    case Token.OPCODEADD:
                        break;
                    case Token.NEWEVENT:
                        break;
                    case Token.GETEVENT:
                        break;
                    case Token.TRY:
                        break;
                    case Token.CATCH:
                        break;
                    case Token.IMPLEMENTS:
                        break;
                    case Token.THROW:
                        context.ILGenerator.ThrowException(Type.GetType(e.args[0].GetValue()));
                        break;
                    case Token.CALLCONSTRUCTOR:
                        break;
                    case Token.OVERRIDE:
                        context.type.SetParent(Type.GetType(e.args[0].GetValue()));
                        break;
                    case Token.GET:
                        break;
                    case Token.SET:
                        break;
                    case Token.TYPEOF:
                        break;
                    case Token.CONST:
                        break;
                    case Token.OPERATOR:
                        break;
                    case Token.ASYNC:
                        break;
                    case Token.AWAIT:
                        break;
                    case Token.SWITCH:
                        break;
                    case Token.CASE:
                        break;
                    case Token.DEFAULT:
                        break;
                    case Token.NEWPOINTER:
                        break;
                    case Token.STARTBLOCK:
                        Label label = context.ILGenerator.DefineLabel();
                        switch (expressions[i - 1].token)
                        {
                            case Token.IF:
                                labels.Add("IF_" + ++ifLabels, "IL_" + ifLabels);
                                context.ILGenerator.MarkLabel(label);
                                break;
                        }
                        break;
                    case Token.DIRECTIVA:
                        break;
                    case Token.ENDCLASS:
                        break;
                    case Token.ENDMETHOD:
                        context.EndMethod();
                        break;
                    case Token.NAMESPACE:
                        namespace_name = e.args[0].GetValue();
                        break;
                    case Token.ENDNAMESPACE:
                        namespace_name = "";
                        break;
                }
            }
        }

        public void CreatePE(string full_name) => context.assembly.Save(full_name);
    }

    public class ContextInfo
    {
        public string appName;
        public OutputType outputType;
        public AssemblyBuilder assembly;
        public AssemblyName assemblyName;
        public ModuleBuilder module;
        public TypeBuilder type;
        public MethodBuilder method, script;
        public FieldBuilder field;
        public ILGenerator ILGenerator
        {
            get => method.GetILGenerator();
        }

        public ContextInfo()
        {
            appName = "";
            outputType = OutputType.Console;
            assemblyName = new AssemblyName(appName);
            method = null;
        }

        public void EndMethod()
        {
            //pass
        }

        public void CreateAssembly()
        {
            assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            module = assembly.DefineDynamicModule(appName, appName + ".dll");
        }
    }
}
