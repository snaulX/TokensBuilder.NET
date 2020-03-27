using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
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
                    "GitHub repository: https://github.com/snaulX/tokensbuilder.net \n");
            }
            else
            {
                PEFileKinds kind = PEFileKinds.ConsoleApplication;
                string appName = "";
                int index = Array.IndexOf(args, "-type");
                if (index > 1) kind = (PEFileKinds)Enum.Parse(typeof(PEFileKinds), args[index + 1]);
                index = Array.IndexOf(args, "-o");
                if (index > 1) appName = args[index + 1];
                foreach (string fileName in args[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    gen = new Generator(fileName);
                    gen.config.appName = appName.IsEmpty() ? System.IO.Path.GetFileNameWithoutExtension(fileName) : appName;
                    gen.config.outputType = kind;
                    gen.Generate();
                }
            }
        }

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
    }
}
