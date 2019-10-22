using System;
using System.Reflection.Emit;
using System.Reflection;
using System.IO;
using TokensAPI;
using System.Collections.Generic;

namespace TokensBuilder
{
    public class TokensBuilder
    {
        public static string info
        {
            get => "TokensBuilder\n" +
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
            AssemblyBuilder ab =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
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
    }
}
