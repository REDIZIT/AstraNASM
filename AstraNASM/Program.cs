using Astra.Compilation;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            string cmd = args[0];

            if (cmd == "compile")
            {
                string sourceFolder = Environment.CurrentDirectory;
                string outputFolder = sourceFolder + "/build";

                Directory.CreateDirectory(outputFolder);

                foreach (string filePath in Directory.GetFiles(sourceFolder, "*.ac"))
                {
                    string fileContent = File.ReadAllText(filePath);
                    string nasmCode = Compiler.Compile_Astra_to_NASM(fileContent);
                    File.WriteAllText(outputFolder + "/" + Path.GetFileNameWithoutExtension(filePath) + ".nasm", nasmCode);
                }
            }
            else
            {
                Console.WriteLine("Invalid arguments");
            }
        }
        else
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
}
