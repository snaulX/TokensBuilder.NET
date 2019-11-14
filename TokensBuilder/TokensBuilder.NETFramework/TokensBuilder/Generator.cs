using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace TokensBuilder
{
    public class Generator
    {
        public ContextInfo context;
        public List<Expression> expressions;

        public Generator()
        {
            expressions = new List<Expression>();
            context = new ContextInfo();
        }

        public void GenerateIL(string assembly_name, string code)
        {
            AssemblyNameDefinition assemblyName = new AssemblyNameDefinition(assembly_name, new Version());
        }
    }

    public class ContextInfo
    {
        public Version version;
        public AssemblyNameDefinition assemblyName;
        public AssemblyDefinition assembly;
        public MethodDefinition method;
        public TypeDefinition type;

        public ContextInfo(string assembly_name, Version version)
        {
            this.version = version;
            assemblyName = new AssemblyNameDefinition(assembly_name, version);
        }

        public ContextInfo()
        {
            version = new Version();
        }
    }
}
