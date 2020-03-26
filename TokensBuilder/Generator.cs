using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using TokensAPI;

namespace TokensBuilder
{
    public class Generator
    {
        TokensReader reader;
        public Config config;

        public Generator()
        {
            reader = new TokensReader();
            config = new Config();
        }

        public Generator(string path)
        {
            reader = new TokensReader(path);
            config = new Config();
        }

        public void Generate()
        {
            reader.GetHeaderAndTarget(out config.header, out config.platform);
            reader.ReadTokens();
            reader.EndWork();
            foreach (TokenType token in reader.tokens)
            {
                ParseToken(token);
            }
        }

        public void ParseToken(TokenType token)
        {
            switch (token)
            {
                case TokenType.NEWLN:
                    break;
                case TokenType.CLASS:
                    break;
                case TokenType.FUNCTION:
                    break;
                case TokenType.VAR:
                    break;
                case TokenType.BLOCK:
                    break;
                case TokenType.STATEMENT:
                    break;
                case TokenType.SEQUENCE:
                    break;
                case TokenType.LITERAL:
                    break;
                case TokenType.SEPARATOR:
                    break;
                case TokenType.EXPRESSION_END:
                    break;
                case TokenType.LOOP:
                    break;
                case TokenType.LABEL:
                    break;
                case TokenType.GOTO:
                    break;
                case TokenType.LOOP_OPERATOR:
                    break;
                case TokenType.OPERATOR:
                    break;
                case TokenType.VALUE:
                    break;
                case TokenType.NULLABLE:
                    break;
                case TokenType.SWITCH:
                    break;
                case TokenType.CASE:
                    break;
                case TokenType.DIRECTIVE:
                    break;
                case TokenType.NEW:
                    break;
                case TokenType.ANNOTATION:
                    break;
                case TokenType.THROW:
                    break;
                case TokenType.TRY:
                    break;
                case TokenType.CATCH:
                    break;
                case TokenType.FINALLY:
                    break;
                case TokenType.IF:
                    break;
                case TokenType.ELSE:
                    break;
                case TokenType.RETURN:
                    break;
                case TokenType.ACTUAL:
                    break;
                case TokenType.TYPEOF:
                    break;
                case TokenType.NAMESPACE:
                    break;
                case TokenType.IMPORT_LIBRARY:
                    break;
                case TokenType.USING_NAMESPACE:
                    break;
                case TokenType.INCLUDE:
                    break;
                case TokenType.BREAKPOINT:
                    break;
                case TokenType.CONVERT:
                    break;
                case TokenType.IMPLEMENTS:
                    break;
                case TokenType.EXTENDS:
                    break;
                case TokenType.INSTANCEOF:
                    break;
                case TokenType.WITH:
                    break;
                case TokenType.YIELD:
                    break;
                case TokenType.LAMBDA:
                    break;
            }
        }
    }
}
