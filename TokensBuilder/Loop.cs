using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TokensAPI;

namespace TokensBuilder
{
    public class Loop
    {
        public LoopType type;
        public TokensReader statementCode;
        public Label startLoop;
        public LocalBuilder statementVar;
        private ILGenerator gen => Context.functionBuilder.generator;

        public Loop(LoopType _type)
        {
            type = _type;
            statementCode = new TokensReader();
            statementVar = gen.DeclareLocal(typeof(bool));
            startLoop = gen.DefineLabel();
            gen.MarkLabel(startLoop);
        }

        /// <summary>
        /// Return true if statementCode is empty
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty() => statementCode == new TokensReader();

        public void EndLoop()
        {
            Generator generator = new Generator();
            generator.reader = statementCode;
            while (generator.reader.tokens.Count > 0)
            {
                if (generator.tryDirective)
                {
                    int errlen = generator.errors.Count;
                    generator.ParseToken(generator.reader.tokens.Peek());
                    if (generator.errors.Count > errlen)
                        generator.errors.RemoveRange(errlen, generator.errors.Count);
                    //tryDirective = false;
                }
                else
                {
                    TokenType tt = generator.reader.tokens.Peek();
                    generator.ParseToken(tt);
                    generator.prev = tt;
                }
            }
            gen.Emit(OpCodes.Stloc, statementVar);
            gen.Emit(OpCodes.Ldloc, statementVar);
            gen.Emit(OpCodes.Brtrue_S, startLoop);
        }
    }
}
