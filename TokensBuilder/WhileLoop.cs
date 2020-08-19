using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TokensAPI;

namespace TokensBuilder
{
    public class WhileLoop : Loop
    {
        public Label endLoop;

        public WhileLoop()
        {
            statementCode = new TokensReader();
            statementVar = gen.DeclareLocal(typeof(bool));
            startLoop = gen.DefineLabel();
            endLoop = gen.DefineLabel();
            gen.Emit(OpCodes.Br_S, endLoop);
            gen.MarkLabel(startLoop);
        }

        public override void EndLoop()
        {
            gen.MarkLabel(endLoop);
            Generator generator = new Generator();
            generator.reader = statementCode;
            generator.putLoopStatement = true;
            generator.parameterTypes.Push(new List<Type>());
            while (generator.reader.tokens.Count > 0)
            {
                if (generator.tryDirective)
                {
                    int errlen = generator.errors.Count;
                    generator.ParseToken(generator.reader.tokens.Pop());
                    if (generator.errors.Count > errlen)
                        generator.errors.RemoveRange(errlen, generator.errors.Count);
                    //tryDirective = false;
                }
                else
                {
                    TokenType tt = generator.reader.tokens.Pop();
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
