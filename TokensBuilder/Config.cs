using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace TokensBuilder
{
    public class Config
    {
        public PEFileKinds outputType;
        public string appName, platform;
        public byte header;

        public Config()
        {
            header = 0;
            appName = "";
            platform = "";
            outputType = PEFileKinds.ConsoleApplication;
        }
    }
}
