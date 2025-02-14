using System.Text;

namespace Astra.Compilation;

public class CodeGenerator_NASM : CodeGeneratorBase
{
    public override byte[] Build()
    {
        string nasm = FormatNASM(string.Join("\n", b.BuildString()));
        return Encoding.UTF8.GetBytes(nasm);
    }
    
    private static string FormatNASM(string nasm)
    {
        string[] lines = nasm.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (line.Contains(":") == false && line.StartsWith(';') == false)
            {
                lines[i] = '\t' + line;
            }
        }

        return string.Join('\n', lines);
    }
}