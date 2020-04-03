using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace TokensBuilder
{
    public static class Config
    {
        public static Version version = new Version();
        public static PEFileKinds outputType = PEFileKinds.ConsoleApplication;
        public static string appName = "", platform = "DOTNET";
        public static HeaderType header = HeaderType.SCRIPT;
        public static string MainClassName
        {
            get => _className.IsEmpty() ? appName + "TokenClass" : _className;
            set => _className = value;
        }
        private static string _className = "";
    }
}
