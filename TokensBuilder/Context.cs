using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace TokensBuilder
{
    public static class Context
    {
        public static TypeBuilder typeBuilder = null;
        public static MethodBuilder methodBuilder = null;
        public static FieldBuilder fieldBuilder = null;
        public static ILGenerator generator
        {
            get => methodBuilder.GetILGenerator();
        }
        public static LocalBuilder localBuilder = null;
        public static Dictionary<string, Label> labels = new Dictionary<string, Label>();
    }
}
