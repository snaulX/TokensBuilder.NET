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

        public Generator()
        {
            reader = new TokensReader();
        }

        public Generator(string path)
        {
            reader = new TokensReader(path);
        }

        public void Generate()
        {
            reader.GetHeaderAndTarget(out Config.header, out Config.platform);
            reader.ReadTokens();
            reader.EndWork();
            foreach (TokenType token in reader.tokens)
            {
                switch (token)
                {
                    //pass
                }
            }
        }
    }
}
