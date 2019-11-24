using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using TokensAPI;
using System.Linq;
using System.Text;

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
                string line = lines[i];
                int j = 0;
                Token token = (Token)Enum.Parse(typeof(Token), 
                (string)  line.TakeWhile((cur) => 
                {
                    j++;
                    return char.IsWhiteSpace(cur);
                }
                ));
                List<Identifer> args = new List<Identifer>();
                byte priority = 0;
                StringBuilder buffer = new StringBuilder();
                for (j = j; j < line.Length; j++)
                {
                    char cur = line[j];
                    if (cur == '(' && priority >= 0)
                    {
                        buffer.Append(cur);
                        priority++;
                    }
                    else if (cur == ')' && priority > 0)
                    {
                        buffer.Append(cur);
                        priority--;
                    }
                    else if (char.IsWhiteSpace(cur) && priority == 0)
                    {
                        args.Add(Identifer.GetIdentifer(buffer.ToString()));
                        buffer.Clear();
                    }
                    else
                    {
                        buffer.Append(cur);
                    }
                }
            }

            //variables for building
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
                        context.ILGenerator.MarkLabel(label);
                        break;
                    case Token.DIRECTIVA:
                        string directiva_name = e.args[0].GetValue();
                        if (directiva_name == "outtype")
                        {
                            context.outputType = (OutputType) Enum.Parse(typeof(OutputType), e.args[1].GetValue(), true);
                        }
                        else if (directiva_name == "version")
                        {
                            context.assembly.GetName().Version = new Version(e.args[1].GetValue());
                        }
                        else
                        {
                            throw new NotSupportedException($"Directiva by name {directiva_name} not found (TokensError in line {i})");
                        }
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
        public ConstructorBuilder constructor;
        public ILGenerator ILGenerator
        {
            get => method.GetILGenerator();
        }

        public ContextInfo()
        {
            appName = "";
            outputType = OutputType.Console;
            assemblyName = new AssemblyName();
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
