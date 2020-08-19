using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TokensAPI;
using TokensBuilder.Errors;

namespace TokensBuilder
{
    public class DoWhileLoop : Loop
    {
        private Generator g => TokensBuilder.gen;

        public DoWhileLoop()
        {
            statementCode = new TokensReader();
            startLoop = gen.DefineLabel();
            gen.MarkLabel(startLoop);
        }

        public override void EndLoop()
        {
            if (g.reader.tokens.Pop() == TokenType.LOOP && g.reader.loops.Pop() == LoopType.WHILE)
            {
                if (g.reader.tokens.Pop() == TokenType.STATEMENT && g.reader.bool_values.Pop())
                {
                    g.needEndStatement++;
                    g.loopStatement = g.needEndStatement;
                    while (!g.ParseLoopStatement(g.reader.tokens.Pop())) { }
                    g.loopStatement = 0;
                    if (g.reader.tokens.Pop() != TokenType.EXPRESSION_END)
                        g.errors.Add(new InvalidTokenError(g.line, "After do-while loop must stay end of expression"));

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

                    gen.Emit(OpCodes.Brfalse_S, startLoop);
                }
                else
                {
                    g.errors.Add(new InvalidTokenError(g.line,
                        "After WHILE token must stay open statement"));
                }
            }
            else
            {
                g.errors.Add(new InvalidTokenError(g.line, 
                    "After block DO must stay WHILE token"));
            }
        }
    }
}
