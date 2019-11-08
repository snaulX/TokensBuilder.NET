using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TokensBuilder
{
    [Flags]
    public enum BuildOptions
    {
        //pass
    }

    public class Generator
    {
        public List<Expression> expressions;
        public string output_il_code;

        public Generator()
        {
            expressions = new List<Expression>();
            output_il_code = "";
        }

        public void Build(string assembly_name, string code)
        {
            //pass
        }

        public void CreateILFile(string directory, string filename)
        {
            using (StreamWriter writer = new StreamWriter(directory + filename + ".il", false, Encoding.Default))
            {
                writer.Write(output_il_code);
                writer.Close();
            }
        }

        public void GeneratePE(string fileName)
        {
            System.Diagnostics.Process.Start("ilasm", fileName);
        }
    }
}
