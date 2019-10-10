using System;
using System.Reflection;
using System.Reflection.Emit;

namespace TokensBuilder
{
    public class TokensBuilder
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
                        break;
                    default:
                        break;
                }
            }
        }

        public void Build(string assembly_name, string code)
        {
            AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assembly_name), AssemblyBuilderAccess.Run);
        }
    }
}
