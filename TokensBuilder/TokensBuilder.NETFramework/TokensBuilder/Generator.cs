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

        public void GenerateIL(string assembly_name)
        {
            AssemblyNameDefinition assemblyName = new AssemblyNameDefinition(assembly_name, new Version());
        }
    }
}
