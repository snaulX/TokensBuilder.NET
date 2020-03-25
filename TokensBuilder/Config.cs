using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace TokensBuilder
{
    public static class Config
    {
        public static PEFileKinds outputType;
        public static string appName, platform;
        public static byte header;
    }
}
