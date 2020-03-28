using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TokensAPI;
using TokensBuilder.Errors;

namespace TokensBuilder
{
    public class Generator
    {
        public uint line = 0;
        public TokensReader reader;
        public string currentNamespace = "";
        public List<string> usingNamespaces = new List<string>();
        public bool initClass = false;
        public Config config;
        public Dictionary<string, Action> directives = new Dictionary<string, Action>();
        List<TokensError> errors = new List<TokensError>();
        //flags
        private bool isDirective = false, needEnd = false;
        private bool? isActual = null; //need three values

        public Generator()
        {
            directives.Add("extends", () => {
                TokenType curToken = reader.tokens.Peek();
                if (curToken == TokenType.LITERAL)
                {
                    string baseClassName = reader.string_values.Peek();
                }
                else
                {
                    errors.Add(new InvalidTokenError(line, curToken));
                }
            });
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
            while (reader.tokens.Count > 0) ParseToken(reader.tokens.Peek());
        }

        public void ParseToken(TokenType token)
        {
            if (needEnd && token == TokenType.EXPRESSION_END) 
                needEnd = false;
            else if (needEnd && token != TokenType.EXPRESSION_END) 
                errors.Add(new TokensError(line, "End of expression with breakpoint not found"));
            else
            {
                switch (token)
                {
                    case TokenType.NEWLN:
                        isDirective = false;
                        line++;
                        break;
                    case TokenType.CLASS:
                        TypeAttributes typeAttributes = TypeAttributes.Class;
                        SecurityDegree securityDegree = reader.securities.Peek();
                        if (securityDegree == SecurityDegree.PRIVATE) typeAttributes |= TypeAttributes.NotPublic;
                        else if (securityDegree == SecurityDegree.PUBLIC) typeAttributes |= TypeAttributes.Public;
                        Context.typeBuilder = Context.moduleBuilder.DefineType(reader.string_values.Peek(), typeAttributes);
                        initClass = true;
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
                        if (isDirective)
                        {
                            directives[reader.string_values.Peek()].Invoke();
                        }
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
                        isDirective = true;
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
                        isActual = reader.bool_values.Peek();
                        break;
                    case TokenType.TYPEOF:
                        break;
                    case TokenType.NAMESPACE:
                        currentNamespace = reader.string_values.Peek();
                        break;
                    case TokenType.IMPORT_LIBRARY:
                        ParseTokensLibrary(reader.string_values.Peek(), ref reader);
                        break;
                    case TokenType.USING_NAMESPACE:
                        usingNamespaces.Add(reader.string_values.Peek());
                        break;
                    case TokenType.INCLUDE:
                        break;
                    case TokenType.BREAKPOINT:
                        Context.generator.Emit(OpCodes.Break);
                        needEnd = true;
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

        public void ParseTokensLibrary(string path, ref TokensReader treader)
        {
            TokensReader tokensReader = new TokensReader(path);
            tokensReader.GetHeaderAndTarget(out byte header, out _);
            if (header != 5) throw new InvalidHeaderException(header);
            reader.ReadTokens();
            reader.EndWork();
            treader.Add(tokensReader);
        }
    }
}
