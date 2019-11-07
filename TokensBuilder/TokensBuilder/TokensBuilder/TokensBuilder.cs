using System;
using System.Reflection.Emit;
using System.Reflection;
using System.IO;
using TokensAPI;
using System.Collections.Generic;
using System.CodeDom.Compiler;

namespace TokensBuilder
{
    public enum OutputType
    {
        ConsoleApp,
        ConsoleLibrary
    }

    public static class TokensBuilder
    {
        public static AppDomain cd;
        public static AssemblyBuilder ab;

        public static string info
        {
            get => "TokensBuilder by snaulX\n" +
                $"Version - {Assembly.GetExecutingAssembly().GetName().Version}\n" +
                "For get info write \"TokensBuilder -info\" in your command line";
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(info);
                Console.ReadKey();
            }
            else
            {
                switch (args[0])
                {
                    case "-o":
                        using (StreamReader file = File.OpenText(Path.GetFullPath(args[1])))
                        {
                            try
                            {
                                if (args[3] == "-config")
                                {
                                    StreamReader config_reader = File.OpenText(Path.GetFullPath(args[4]));
                                    ParseConfig(config_reader.ReadToEnd());
                                }
                            }
                            catch
                            {
                                //nothing
                            }
                            Build(args[2], file.ReadToEnd());
                        }
                        break;
                    case "-info":
                        Console.WriteLine(info);
                        break;
                    default:
                        string filename = args[0];
                        using (StreamReader file = File.OpenText(Path.GetFullPath(filename)))
                        {
                            Build(filename.Remove(filename.LastIndexOf('.')), file.ReadToEnd());
                        }
                        break;
                }
            }
        }

        public static void Build(string assembly_name, string code)
        {
            List<Expression> expressions = new List<Expression>();
            string[] lines = code.Split('\n', '\r');
            for (int i = 0; i < lines.Length; i++)
            {
                string[] ids = lines[i].Split(' ', '\t');
                Expression expr = new Expression();
                expr.token = TokensAPI.Main.GetToken(ids[0]);
                for (int j = 1; j < ids.Length; j++)
                {
                    expr.arguments.Add(Identifer.GetIdentifer(ids[j]));
                }
            }
            Build(assembly_name, expressions);
        }

        public static void Build(string assembly_name, List<Expression> expressions)
        {
            AppDomainSetup ads = new AppDomainSetup();
            ads.ApplicationName = assembly_name + "TokensApplication";
            cd = AppDomain.CreateDomain("CompilerDomain");
            AssemblyName aName = new AssemblyName(assembly_name);
            ab = cd.DefineDynamicAssembly(
                    aName,
                    AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder main_module = ab.DefineDynamicModule("main");
            TypeBuilder main_type = main_module.DefineType("Main");
            main_type.CreateType();
            MethodBuilder main_method = main_type.DefineMethod("Main", MethodAttributes.Static, CallingConventions.Any);
            ContextInfo context = new ContextInfo { tb = main_type, mb = main_method, modb = main_module };
            for (int i = 0; i < expressions.Count; i++)
            {
                expressions[i].Parse(ref context);
            }
        }

        public static void ParseConfig(string config)
        {
            //while nothing
        }
    }

    public struct ContextInfo
    {
        public ModuleBuilder modb;
        public TypeBuilder tb;
        public MethodBuilder mb;
    }

    public class Expression
    {
        public Token token;
        public List<Identifer> arguments;

        public Expression()
        {
            token = Token.NULL;
            arguments = new List<Identifer>();
        }

        public void Parse(ref ContextInfo context)
        {
            switch (token)
            {
                case Token.NULL:
                    //do nothing
                    break;
                case Token.USE:
                    foreach (Identifer ide in arguments)
                    {
                        Assembly.LoadFile(ide.GetValue() + ".dll");
                    }
                    break;
                case Token.WRITEVAR:
                    break;
                case Token.NEWCLASS:
                    TypeBuilder newtype = TokensBuilder.ab.GetDynamicModule(arguments[0].GetValue())
                        .DefineType(arguments[1].GetValue(),
                        (TypeAttributes)Enum.Parse(typeof(TypeAttributes), arguments[2].GetValue()));
                    newtype.CreateType();
                    context.tb = newtype;
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
                    ModuleBuilder modb = TokensBuilder.ab.DefineDynamicModule(arguments[0].GetValue());
                    context.modb = modb;
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
                    context.tb.AddInterfaceImplementation(Type.GetType(arguments[0].GetValue()));
                    break;
                case Token.THROW:
                    break;
                case Token.CALLCONSTRUCTOR:
                    break;
                case Token.OVERRIDE:
                    context.tb.SetParent(Type.GetType(arguments[0].GetValue()));
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
                    if (arguments[0].GetValue() == "version")
                    {
                        Assembly.GetEntryAssembly().GetName().Version = new Version(arguments[1].GetValue());
                    }
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
}
