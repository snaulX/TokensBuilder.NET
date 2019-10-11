using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace TokensBuilder
{
    public static class TokensBuilder
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                //пока ничего не делать
            }
            else
            {
                switch (args[0])
                {
                    case "-o":
                        string filename = args[1];
                        using (StreamReader file = File.OpenText(Path.GetFullPath(filename)))
                        {
                            Build(filename.Remove(filename.LastIndexOf('.')), file.ReadToEnd());
                        }
                        break;
                    default:
                        filename = args[0];
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
            AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assembly_name), AssemblyBuilderAccess.RunAndCollect);
        }
    }
}
