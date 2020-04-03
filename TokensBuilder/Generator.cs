using System;
using System.Collections.Generic;
using System.IO;
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
        public Dictionary<string, Action> directives = new Dictionary<string, Action>();
        byte needEndStatement = 0, needEndSequence = 0, needEndBlock = 0;
        List<TokensError> errors = new List<TokensError>();
        //flags
        private bool isDirective = false, needEnd = false, extends = false, implements = false;
        private bool? isActual = null; //need three values

        public Generator()
        {
            directives.Add("extends", () =>
            {
                TokenType curToken = reader.tokens.Peek();
                if (curToken == TokenType.LITERAL)
                {
                    if (Config.header == HeaderType.CLASS)
                    {
                        Context.mainType.SetParent(Type.GetType(reader.string_values.Peek()));
                    }
                    else
                    {
                        errors.Add(new InvalidHeaderError(line, Config.header, "extends directive can be only with class header"));
                    }
                }
                else
                {
                    errors.Add(new InvalidTokenError(line, curToken));
                }
            });
            directives.Add("implements", () =>
            {
                TokenType tokenType = TokenType.LITERAL;
                while (tokenType != TokenType.NEWLN)
                {
                    if (Config.header == HeaderType.CLASS)
                        Context.mainType.AddInterfaceImplementation(Type.GetType(reader.string_values.Peek()));
                    else
                        errors.Add(new InvalidHeaderError(line, Config.header, "implements directive can be only with class header"));
                }
                isDirective = false;
            });
            directives.Add("if", () =>
            {
                //something
            });
            directives.Add("endif", () =>
            {
                //something
            });
            usingNamespaces.Add(""); //empty namespace
            reader = new TokensReader();
        }

        public void Generate()
        {
            reader.GetHeaderAndTarget(out byte h, out Config.platform);
            Config.header = (HeaderType)h;
            reader.ReadTokens();
            reader.EndWork();
            while (reader.tokens.Count > 0) ParseToken(reader.tokens.Peek());
            if (needEndBlock > 0) errors.Add(new NeedEndError(line, "Need " + needEndBlock + " end of blocks"));
            if (needEndSequence > 0) errors.Add(new NeedEndError(line, "Need " + needEndSequence + " end of sequences"));
            if (needEndStatement > 0) errors.Add(new NeedEndError(line, "Need " + needEndStatement + " end of statements"));
            foreach (TokensError error in errors)
            {
                Console.Error.WriteLine(error);
            }
        }

        private void CheckOnAllClosed()
        {
            if (needEndBlock > 0) errors.Add(new NeedEndError(line, "Need end of block"));
            else if (needEndSequence > 0) errors.Add(new NeedEndError(line, "Need end of array"));
            else if (needEndStatement > 0) errors.Add(new NeedEndError(line, "Need end of statement"));
        }

        private bool IsEnd(TokenType token)
        {
            return token == TokenType.EXPRESSION_END || (token == TokenType.BLOCK && reader.bool_values[0]);
        }

        public void ParseToken(TokenType token)
        {
            if (needEnd)
            {
                if (!IsEnd(token)) errors.Add(new TokensError(line, "End of expression with breakpoint not found"));
                else if (token == TokenType.BLOCK) needEndBlock++;
                needEnd = false;
            }
            else if (extends)
            {
                if (token == TokenType.LITERAL)
                    Context.typeBuilder.SetParent(Context.GetTypeByName(reader.string_values.Peek(), usingNamespaces));
                else
                    errors.Add(new InvalidTokenError(line, TokenType.LITERAL));
                extends = false;
                needEnd = true;
            }
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
                        Context.classType = reader.class_types.Peek();
                        if (Context.classType == ClassType.FINAL) typeAttributes |= TypeAttributes.Sealed;
                        else if (Context.classType == ClassType.INTERFACE) typeAttributes = TypeAttributes.Interface;
                        if (securityDegree == SecurityDegree.PRIVATE) typeAttributes |= TypeAttributes.NotPublic;
                        else if (securityDegree == SecurityDegree.PUBLIC) typeAttributes |= TypeAttributes.Public;
                        Context.typeBuilder = Context.moduleBuilder.DefineType(
                            currentNamespace + reader.string_values.Peek(), typeAttributes);
                        initClass = true;
                        break;
                    case TokenType.FUNCTION:
                        break;
                    case TokenType.VAR:
                        break;
                    case TokenType.BLOCK:
                        if (reader.bool_values.Peek())
                        {
                            implements = false;
                            extends = false;
                            needEndBlock++;
                        }
                        else needEndBlock--;
                        break;
                    case TokenType.STATEMENT:
                        if (reader.bool_values.Peek()) needEndStatement++;
                        else needEndStatement--;
                        break;
                    case TokenType.SEQUENCE:
                        if (reader.bool_values.Peek()) needEndSequence++;
                        else needEndSequence--;
                        break;
                    case TokenType.LITERAL:
                        string literal = reader.string_values.Peek();
                        if (isDirective)
                        {
                            try
                            {
                                directives[literal].Invoke();
                            }
                            catch (KeyNotFoundException)
                            {
                                errors.Add(new DirectiveError(line, $"Directive by name {literal} not found"));
                            }
                        }
                        else if (implements)
                        {
                            Context.typeBuilder.AddInterfaceImplementation(Context.GetInterfaceByName(literal, usingNamespaces));
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
                        Assembly.LoadFrom(reader.string_values.Peek());
                        break;
                    case TokenType.BREAKPOINT:
                        Context.generator.Emit(OpCodes.Break);
                        needEnd = true;
                        break;
                    case TokenType.IMPLEMENTS:
                        CheckOnAllClosed();
                        implements = true;
                        break;
                    case TokenType.EXTENDS:
                        CheckOnAllClosed();
                        extends = true;
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
            TokensReader tokensReader = new TokensReader();
            try
            {
                if (path.StartsWith("<")) tokensReader.SetPath(path.Remove(path.Length - 2) + ".tokens");
                else tokensReader.SetPath(AppDomain.CurrentDomain.BaseDirectory + path + ".tokens");
            }
            catch
            {
                errors.Add(new TokensLibraryError(line, $"Tokens library by path {path} not found"));
                return;
            }
            tokensReader.GetHeaderAndTarget(out byte header, out _);
            if (header != 5) throw new InvalidHeaderException(header);
            reader.ReadTokens();
            reader.EndWork();
            treader.Add(tokensReader);
        }
    }
}
