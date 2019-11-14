using System;
using System.Collections.Generic;
using Mono.Cecil;
using TokensAPI;

namespace TokensBuilder
{
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

            //parse expressions
            for (int i = 0; i < expressions.Count; i++)
            {
                Expression e = expressions[i];
                switch (e.token)
                {
                    case Token.NULL:
                        break;
                    case Token.USE:
                        break;
                    case Token.WRITEVAR:
                        break;
                    case Token.NEWCLASS:
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
                    case Token.ENDMODULE:
                        break;
                    case Token.ENDCLASS:
                        break;
                    case Token.ENDMETHOD:
                        break;
                }
            }
        }

        public void CreatePE(string full_name) => context.assembly.Write(full_name);
    }

    public class ContextInfo
    {
        public Version version;
        public AssemblyNameDefinition assemblyName;
        public AssemblyDefinition assembly;
        public MethodDefinition method;
        public TypeDefinition type;
        public ModuleDefinition module;
        public ModuleKind kind;
        public string moduleName;

        public ContextInfo(string assembly_name, Version version): this()
        {
            this.version = version;
            assemblyName = new AssemblyNameDefinition(assembly_name, version);
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
    }
}
