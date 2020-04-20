using System;
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
            get => _className.IsEmpty() ? 
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(appName) + "TokenClass" : _className;
            set => _className = value;
        }
        public static string FileName => appName + (outputType == PEFileKinds.Dll ? ".dll" : ".exe");
        private static string _className = "";
    }
}
