using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TokensAPI;

namespace TokensBuilder
{
    public class FunctionBuilder
    {
        public bool IsEmpty => methodBuilder == null;
        public MethodBuilder methodBuilder = null;
        public LocalBuilder localBuilder = null;
        public FuncType type = FuncType.DEFAULT;
    }
}
