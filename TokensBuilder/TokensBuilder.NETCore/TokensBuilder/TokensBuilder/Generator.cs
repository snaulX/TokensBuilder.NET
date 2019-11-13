using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using TokensAPI;
using System.Reflection;

namespace TokensBuilder
{
    [Flags]
    public enum BuildOptions
    {
        OUTPUTEXE,
        OUTPUTDLL
    }

    public struct SwitchContext
    {
        string end_address;
        List<Expression> code;

        public SwitchContext(string end_address, List<Expression> code)
        {
            this.end_address = end_address ?? throw new ArgumentNullException(nameof(end_address));
            this.code = code ?? throw new ArgumentNullException(nameof(code));
        }

        public bool IsNotEmpty()
        {
            return false;
        }
    }

    public class Generator
    {
        public List<Expression> expressions;
        public string output_il_code;
        public Version version;
        public BuildOptions buildOptions;

        public Generator()
        {
            expressions = new List<Expression>();
            output_il_code = "";
            version = new Version();
            buildOptions = BuildOptions.OUTPUTEXE;
        }

        public void Build(string assembly_name, string code)
        {
            string[] lines = code.Split('\n', '\r');
            for (int i = 0; i < lines.Length; i++)
            {
                string[] ids = lines[i].Split(' ', '\t');
                Expression expr = new Expression();
                expr.token = Main.GetToken(ids[0]);
                for (int j = 1; j < ids.Length; j++)
                {
                    expr.args.Add(Identifer.GetIdentifer(ids[j]));
                }
            }
            Build(assembly_name);
        }

        public void Build(string assembly_name)
        {
            //List<Assembly> assemblies = new List<Assembly>();
            List<string> namespaces_check = new List<string>();
            StringBuilder code_builder = new StringBuilder(), localstack_builder = new StringBuilder();
            int start_of_block = 0;
            SwitchContext switchContext = new SwitchContext();
            Dictionary<string, string> labels = new Dictionary<string, string>(), local_vars = new Dictionary<string, string>();
            for (int i = 0; i < expressions.Count; i++)
            {
                Expression expression = expressions[i];
                switch (expression.token)
                {
                    case Token.NULL:
                        code_builder.AppendLine("nop");
                        break;
                    case Token.USE:
                        namespaces_check.Add(expression.args[0].GetValue());
                        break;
                    case Token.WRITEVAR:
                        break;
                    case Token.NEWCLASS:
                        break;
                    case Token.NEWVAR:
                        if (expression.args[1].GetValue() != "public")
                        {
                            localstack_builder.Append("");
                        }
                        else
                        {
                            //else
                        }
                        break;
                    case Token.NEWFUNC:
                        code_builder.Append(".method ");
                        code_builder.AppendLine(" cil managed");
                        start_of_block = i + 1;
                        break;
                    case Token.GETCLASS:
                        break;
                    case Token.GETVAR:
                        break;
                    case Token.GETFUNC:
                        break;
                    case Token.RUNFUNC:
                        code_builder.Append("call  ");
                        break;
                    case Token.WHILE:
                        break;
                    case Token.FOR:
                        break;
                    case Token.FOREACH:
                        break;
                    case Token.BREAK:
                        code_builder.AppendLine("break");
                        break;
                    case Token.CONTINUE:
                        break;
                    case Token.RETURN:
                        break;
                    case Token.IF:
                        break;
                    case Token.ELSE:
                        break;
                    case Token.ELIF:
                        break;
                    case Token.GOTO:
                        break;
                    case Token.LABEL:
                        code_builder.Append("IL_" + Convert.ToString(i, 16) + ":");
                        labels.TryAdd(expression.args[0].GetValue(), "IL_" + Convert.ToString(i, 16));
                        break;
                    case Token.YIELD:
                        break;
                    case Token.GETLINK:
                        break;
                    case Token.WRITEINPOINTER:
                        break;
                    case Token.NEWSTRUCT:
                        break;
                    case Token.NEWINTERFACE:
                        break;
                    case Token.NEWENUM:
                        break;
                    case Token.NEWMODULE:
                        break;
                    case Token.NEWCONSTRUCTOR:
                        break;
                    case Token.NEWATTRIBUTE:
                        break;
                    case Token.GETATTRIBUTE:
                        break;
                    case Token.GETCONSTRUCTOR:
                        break;
                    case Token.OPCODEADD:
                        break;
                    case Token.NEWEVENT:
                        break;
                    case Token.GETEVENT:
                        break;
                    case Token.TRY:
                        code_builder.AppendLine(".try\n{");
                        break;
                    case Token.CATCH:
                        break;
                    case Token.IMPLEMENTS:
                        break;
                    case Token.THROW:
                        break;
                    case Token.CALLCONSTRUCTOR:
                        break;
                    case Token.OVERRIDE:
                        break;
                    case Token.GET:
                        break;
                    case Token.SET:
                        break;
                    case Token.TYPEOF:
                        break;
                    case Token.CONST:
                        break;
                    case Token.OPERATOR:
                        break;
                    case Token.ASYNC:
                        break;
                    case Token.AWAIT:
                        break;
                    case Token.SWITCH:
                        break;
                    case Token.CASE:
                        break;
                    case Token.DEFAULT:
                        break;
                    case Token.NEWPOINTER:
                        break;
                    case Token.STARTBLOCK:
                        break;
                    case Token.DIRECTIVA:
                        string directiva_name = expression.args[0].GetValue();
                        if (directiva_name == "entrypoint") code_builder.AppendLine(".entrypoint");
                        else if (directiva_name == "version") version = new Version(expression.args[1].GetValue());
                        else if (directiva_name == "include")
                        {
                            string dllName = expression.args[1].GetValue();
                            Assembly.LoadFile(dllName);
                            output_il_code.Insert(0, ".assembly extern " + dllName + " {}\n");
                        }
                        break;
                    case Token.ENDCLASS:
                    case Token.END:
                    case Token.ENDMETHOD:
                        if (switchContext.IsNotEmpty())
                        {
                            //pass
                        }
                        else
                        {
                            code_builder.AppendLine("}");
                        }
                        break;
                    case Token.LIB:
                        break;
                    case Token.VIRTUAL:
                        break;
                }
            }
            output_il_code += $".assembly {assembly_name}{{\n" +
                $"  .ver {version.ToString().Replace('.', ':')} //version {version}\n" +
                "}}\n.assembly extern mscorlib {}\n";
            output_il_code += code_builder.ToString();
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
