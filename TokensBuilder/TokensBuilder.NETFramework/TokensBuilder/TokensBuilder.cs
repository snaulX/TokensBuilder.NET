using System;
using System.Reflection.Emit;
using System.Reflection;
using System.IO;
using TokensAPI;
using System.Collections.Generic;
using TokensAPI.identifers;

namespace TokensBuilder
{
    public static class TokensBuilder
    {
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
                Generator generator = new Generator();
                switch (args[0])
                {
                    case "-o":
                        using (StreamReader file = File.OpenText(Path.GetFullPath(args[1])))
                        {
                            generator.GenerateIL(args[2], file.ReadToEnd());
                        }
                        break;
                    case "-info":
                        Console.WriteLine(info);
                        break;
                    default:
                        string filename = args[0];
                        using (StreamReader file = File.OpenText(Path.GetFullPath(filename)))
                        {
                            generator.GenerateIL(filename.Remove(filename.LastIndexOf('.')), file.ReadToEnd());
                        }
                        break;
                }
            }
        }
    }
}
