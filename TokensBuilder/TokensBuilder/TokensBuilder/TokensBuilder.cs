using System;
using System.Reflection.Emit;
using System.Reflection;
using System.IO;
using TokensAPI;
using System.Collections.Generic;
using System.Resources;

namespace TokensBuilder
{
    public class TokensBuilder
    {
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
            AssemblyName aName = new AssemblyName(assembly_name);
            ab = AppDomain.CurrentDomain.DefineDynamicAssembly(
                    aName,
                    AssemblyBuilderAccess.RunAndSave);
        }
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

        public void Parse()
        {
            switch (token)
            {
                case Token.NULL:
                    //nothing
                    break;
                case Token.USE:
                    Identifer id = arguments[0];
                    if (id is TokensAPI.identifers.Array)
                    {
                        ResourceWriter writer = (ResourceWriter)TokensBuilder.ab.DefineResource("ArrayLibs", "Library", "ArrayLibs.res");
                        foreach (Identifer ide in ((TokensAPI.identifers.Array) id).elements)
                        {
                            writer.AddResource(ide.GetValue(), Assembly.LoadFrom(Path.GetFullPath(ide.GetValue())));
                        }
                    }
                    else
                    {
                        ResourceWriter writer = (ResourceWriter)TokensBuilder.ab.DefineResource(id.GetValue(), "Library", id.GetValue() + ".res");
                        writer.AddResource(id.GetValue(), Assembly.LoadFrom(Path.GetFullPath(id.GetValue())));
                    }
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
                    TokensBuilder.ab.DefineDynamicModule(arguments[0].GetValue());
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
                case Token.ABSTRACT:
                    break;
                case Token.STATIC:
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
                case Token.INTERNAL:
                    break;
                case Token.SEALED:
                    break;
                case Token.EXTERNAL:
                    break;
                case Token.PUBLIC:
                    break;
                case Token.PRIVATE:
                    break;
                case Token.PROTECTED:
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
            }
        }
    }
}
