using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TokensAPI;
using TokensBuilder.Errors;

namespace TokensBuilder
{
    public sealed class ClassBuilder
    {
        public bool IsEmpty => typeBuilder == null;
        public bool? actual = null;
        public Dictionary<string, FieldBuilder> constants = new Dictionary<string, FieldBuilder>();
        public FunctionBuilder methodBuilder = null;
        public FieldBuilder fieldBuilder = null;
        private TypeBuilder typeBuilder;
        public ClassType type = ClassType.DEFAULT;
        public SecurityDegree security = SecurityDegree.PUBLIC;
        public ConstructorBuilder defaultCtor;
        public PropertyBuilder propertyBuilder;
        public EnumBuilder enumBuilder;
        private Generator gen => TokensBuilder.gen;

        public ClassBuilder(string name, string nameSpace = "",
            ClassType classType = ClassType.DEFAULT, SecurityDegree securityDegree = SecurityDegree.PUBLIC)
        {
            type = classType;
            security = securityDegree;
            TypeAttributes typeAttributes = TypeAttributes.Class;

            //select type of class
            if (classType == ClassType.DEFAULT) typeAttributes = TypeAttributes.Class;
            else if (classType == ClassType.FINAL) typeAttributes |= TypeAttributes.Sealed;
            else if (classType == ClassType.INTERFACE) typeAttributes = TypeAttributes.Interface;

            //select security of class
            if (securityDegree == SecurityDegree.PRIVATE) typeAttributes |= TypeAttributes.NotPublic;
            else if (securityDegree == SecurityDegree.PUBLIC) typeAttributes |= TypeAttributes.Public;

            if (nameSpace.IsEmpty())
                typeBuilder = Context.moduleBuilder.DefineType(name, typeAttributes);
            else
                typeBuilder = Context.moduleBuilder.DefineType(nameSpace + "." + name, typeAttributes);
        }

        public void CreateDefaultConstructorArgs(IEnumerable<TokenType> tokens)
        {
            if (!tokens.IsEmpty())
            {
                bool needVar = false;
                foreach (TokenType token in tokens)
                {
                    if (needVar)
                    {
                        if (token != TokenType.VAR)
                            TokensBuilder.Error(new InvalidTokenError(gen.line, token));
                    }
                    switch (token)
                    {
                        case TokenType.LITERAL:
                            break;
                        case TokenType.VAR:
                            needVar = false;
                            Context.CreateField();
                            break;
                        case TokenType.ACTUAL:
                            bool _actual = gen.reader.bool_values.Peek();
                            if (!actual.HasValue)
                                TokensBuilder.Error(new TokensError(gen.line,
                                    "Cannot be use " + (_actual ? "actual" : "expect") + " fields in default class"));
                            else
                            {
                                if (!actual.GetValueOrDefault() && _actual)
                                    TokensBuilder.Error(new PlatformImplementationError(gen.line,
                                        "Cannot be actual members in expect class"));
                                else if (actual.GetValueOrDefault() && !_actual)
                                    TokensBuilder.Error(new PlatformImplementationError(gen.line,
                                        "Cannot be expect members in actual class"));
                                else
                                    needVar = true;
                            }
                            break;
                    }
                }
            }
        }

        public bool TryEndField()
        {
            if (fieldBuilder != null)
            {
                if (fieldBuilder.IsLiteral)
                    constants.Add(fieldBuilder.Name, fieldBuilder);
                fieldBuilder = null;
                return true;
            }
            else return false;
        }

        public FunctionBuilder CreateMethod(string name, string typeName = "", FuncType type = FuncType.DEFAULT,
            SecurityDegree security = SecurityDegree.PUBLIC)
        {
            MethodAttributes attributes;

            //function security
            if (security == SecurityDegree.INTERNAL) attributes = MethodAttributes.Assembly;
            else if (security == SecurityDegree.PRIVATE) attributes = MethodAttributes.Private;
            else if (security == SecurityDegree.PROTECTED) attributes = MethodAttributes.Family;
            else attributes = MethodAttributes.Public;

            //function type
            if (type == FuncType.STATIC) attributes |= MethodAttributes.Static;
            else if (type == FuncType.ABSTRACT) attributes |= MethodAttributes.Abstract;
            else if (type == FuncType.FINAL) attributes |= MethodAttributes.Final;
            else if (type == FuncType.VIRTUAL) attributes |= MethodAttributes.Virtual;

            methodBuilder = new FunctionBuilder(typeBuilder.DefineMethod(name, attributes, CallingConventions.Standard));
            return methodBuilder;
        }

        public void Extends(string superTypeName) => typeBuilder.SetParent(Context.GetTypeByName(superTypeName, gen.usingNamespaces));

        public void Implements(string interfaceName) => typeBuilder.AddInterfaceImplementation(
            Context.GetInterfaceByName(interfaceName, gen.usingNamespaces));

        public void SetAttribute(CustomAttributeBuilder attr) => typeBuilder.SetCustomAttribute(attr);

        internal void DefineField(string name, string typeName, FieldAttributes fieldAttributes) =>
            fieldBuilder = typeBuilder.DefineField(name, Context.GetTypeByName(typeName, gen.usingNamespaces), fieldAttributes);

        public Type End()
        {
            Type rt = typeBuilder.CreateType();
            typeBuilder = null;
            return rt;
        }
    }
}
