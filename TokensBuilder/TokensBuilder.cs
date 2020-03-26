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
                foreach (string fileName in args[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    gen = new Generator(fileName);
                    //int index = Array.IndexOf(args, "-target");
                    //if (index > 1) gen.config.outputType = (PEFileKinds)Enum.Parse(typeof(PEFileKinds), args[index + 1]);
                    gen.Generate();
                }
            }
        }

        public static bool IsEmpty<T>(this IEnumerable<T> collection) => collection.Count() == 0;
    }
}
