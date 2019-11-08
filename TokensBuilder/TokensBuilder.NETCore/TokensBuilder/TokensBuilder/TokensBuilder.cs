using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

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
                            generator.Build(args[2], file.ReadToEnd());
                            generator.CreateILFile(Path.GetFullPath(args[1]).Replace('\\' + args[1], string.Empty), args[2]);
                            generator.GeneratePE(Path.GetFullPath(args[2] + ".il"));
                        }
                        break;
                    case "-info":
                        Console.WriteLine(info);
                        break;
                    default:
                        string filename = args[0];
                        using (StreamReader file = File.OpenText(Path.GetFullPath(filename)))
                        {
                            generator.Build(filename.Remove(filename.LastIndexOf('.')), file.ReadToEnd());
                        }
                        generator.CreateILFile(Path.GetPathRoot(filename), Path.GetFileNameWithoutExtension(filename));
                        generator.GeneratePE(Path.GetFileNameWithoutExtension(filename) + ".il");
                        break;
                }
            }
        }
    }
}
