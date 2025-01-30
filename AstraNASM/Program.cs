using Astra.Compilation;

public static class Program
{
    public static void Main(string[] args)
    {
        bool doRecompile = true;

        if (doRecompile)
        {
            string fileContent = File.ReadAllText("../../../Compiler/source.ac");
            string nasmCode = Compiler.Compile_Astra_to_NASM(fileContent);
            File.WriteAllText("../../../Compiler/output.asm", nasmCode);
        }

        //CmdExecutor.CompileRunAndCheck("../../../Compiler", "output.ll", 123);
    }
}
