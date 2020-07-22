using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TokensAPI;

namespace TokensBuilder
{
    public static class TokensBuilder
    {
        public static Generator gen;

        private static void Main(string[] args)
        {
            if (args.IsEmpty())
            {
                Console.WriteLine("TokensBuilder.NET (c) 2020-2020\n" +
                    "Author: snaulX\n" +
                    "GitHub repository: https://github.com/mino-lang/TokensBuilder.NET \n" +
                    "For get help use command -h\n");
            }
            else if (args[0] == "-h")
            {
                if (args.Length == 1)
                {
                    Console.WriteLine("TokensBuilder (name of tokens file[s] written how one cmd argument) [commands]\n" +
                        "Commands:\n" +
                        "\t-type (kind of building application)\tSet type of building application. Types:\n" +
                        $"\t\t{string.Join("\n\t\t", Enum.GetValues(typeof(PEFileKinds)))}\n" +
                        "\t-o (name of output file)\tSet name of output file (application name).\n" +
                        "\t-name (name of main class)\tSet name of main auto-generated class." +
                        " Will work only if tokens header is Script or Class\n");
                }
            }
            else
            {
                PEFileKinds kind = PEFileKinds.ConsoleApplication;
                string appName = "";
                int index = Array.IndexOf(args, "-type");
                if (index > 1) kind = (PEFileKinds)Enum.Parse(typeof(PEFileKinds), args[index + 1]);
                index = Array.IndexOf(args, "-o");
                if (index > 1) appName = args[index + 1];
                index = Array.IndexOf(args, "-name");
                if (index > 1) Config.MainClassName = args[index + 1];
                foreach (string fileName in args[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    gen = new Generator();
                    gen.reader.SetPath(fileName);
                    Config.appName = appName.IsEmpty() ? System.IO.Path.GetFileNameWithoutExtension(fileName) : appName;
                    Config.outputType = kind;
                    Context.CreateAssembly(true);
                    gen.Generate();
                }
                Context.Finish();
            }
        }

        public static void Error(TokensError error) => gen.errors.Add(error);

        public static void Add(this TokensReader a, TokensReader b)
        {
            a.tokens.AddRange(b.tokens);
            a.var_types.AddRange(b.var_types);
            a.values.AddRange(b.values);
            a.string_values.AddRange(b.string_values);
            a.securities.AddRange(b.securities);
            a.operators.AddRange(b.operators);
            a.loops.AddRange(b.loops);
            a.function_types.AddRange(b.function_types);
            a.byte_values.AddRange(b.byte_values);
            a.bool_values.AddRange(b.bool_values);
        }

        public static bool IsEmpty<T>(this IEnumerable<T> collection) => collection.Count() == 0;

        public static T Peek<T>(this List<T> collection)
        {
            T elem = collection[0];
            collection.RemoveAt(0);
            return elem;
        }

        public static bool IsSimpleDataType(this Type t)
            => t == typeof(byte) || t == typeof(sbyte) || t == typeof(int) || t == typeof(short)
            || t == typeof(float) || t == typeof(long) || t == typeof(double) || t == typeof(bool);
    }
}
