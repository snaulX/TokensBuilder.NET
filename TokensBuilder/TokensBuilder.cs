using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

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
                int index = Array.IndexOf(args, "-target");
                if (index > 1) Config.outputType = (PEFileKinds) Enum.Parse(typeof(PEFileKinds), args[index + 1]);
                gen = new Generator(args[0]);
                gen.Generate();
            }
        }

        public static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            return collection.Count() == 0;
        }
    }
}
