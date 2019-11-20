using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using TokensAPI;

namespace TokensBuilder
{
    public class Generator
    {
        public Instruction GetValue(string value)
        {
            return context.ILGenerator.Create(OpCodes.Ldnull);
        }

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
            context.assemblyName = new AssemblyNameDefinition(assembly_name, new Version());

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
            List<string> check_namespaces = new List<string>();
            Dictionary<string, string> labels = new Dictionary<string, string>();
            short ifLabels = 0, whileLabels = 0;
            string namespace_name = "";

            //parse expressions
            for (int i = 0; i < expressions.Count; i++)
            {
                Expression e = expressions[i];
                switch (e.token)
                {
                    case Token.USE:
                        check_namespaces.Add(e.args[0].GetValue());
                        break;
                    case Token.WRITEVAR:
                        break;
                    case Token.NEWCLASS:
                        context.type = new TypeDefinition(namespace_name, e.args[0].GetValue(), TypeAttributes.NotPublic, context.@void);
                        break;
                    case Token.NEWVAR:
                        break;
                    case Token.NEWFUNC:
                        break;
                    case Token.END:
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
                        break;
                    case Token.CONTINUE:
                        break;
                    case Token.RETURN:
                        context.ILGenerator.Append(GetValue(e.args[0].GetValue()));
                        context.ILGenerator.Emit(OpCodes.Ret);
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
                        context.ILGenerator.Emit(OpCodes.Throw, e.args[0].GetValue());
                        break;
                    case Token.CALLCONSTRUCTOR:
                        break;
                    case Token.OVERRIDE:
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

        public void CreatePE(string full_name) => context.assembly.Write(full_name);
    }

    public class ContextInfo
    {
        public TypeReference @void
        {
            get => module.ImportReference(typeof(void));
        }
        public Version version;
        public AssemblyNameDefinition assemblyName;
        public AssemblyDefinition assembly;
        public MethodDefinition method, entrypoint;
        public TypeDefinition type;
        public ModuleDefinition module
        {
            get => assembly.MainModule;
        }
        public FieldDefinition field;
        public ModuleKind kind;
        public string moduleName;
        public ILProcessor ILGenerator
        {
            get => method.Body.GetILProcessor();
        }

        public ContextInfo(string assembly_name, Version version): this()
        {
            this.version = version;
            assemblyName = new AssemblyNameDefinition(assembly_name, version);
            entrypoint = new MethodDefinition("Main", MethodAttributes.Static | MethodAttributes.HideBySig, @void);
            method = entrypoint;
        }

        public ContextInfo()
        {
            kind = ModuleKind.Console;
            version = new Version();
        }

        public void GenerateAssembly()
        {
            assembly = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, kind);
        }

        public void EndMethod()
        {
            if (method == entrypoint)
            {
                method = null;
            }
            else
            {
                method = entrypoint;
            }
        }
    }
}
