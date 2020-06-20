using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TokensAPI;

namespace TokensBuilder
{
    public abstract class Loop
    {
        public TokensReader statementCode;
        protected ILGenerator gen => Context.functionBuilder.generator;
        public Label startLoop;
        public LocalBuilder statementVar;

        public virtual bool IsEmpty() => statementCode == new TokensReader();
        public abstract void EndLoop();
    }
}
