using Astra.Compilation;

public enum CompileTarget
{
    Simulator,
    NASM
}

public static class Program
{
    public static CompileTarget target;
    
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            string cmd = args[0];

            if (cmd == "compile")
            {
                if (args.Length > 1)
                {
                    target = (CompileTarget)args[1][0];
                }
                else
                {
                    target = CompileTarget.Simulator;
                }
                
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
                //string fileContent = File.ReadAllText("C:\\Users\\REDIZIT\\Documents\\GitHub\\Astra-Rider-extension\\try2\\test\\example.ac");
                string fileContent = File.ReadAllText("C:\\Users\\REDIZIT\\Documents\\GitHub\\Astra-Rider-extension\\vscode extension\\astralanguage\\test\\example.ac");
                string nasmCode = Compiler.Compile_Astra_to_NASM(fileContent);

                // Simulator sim = new();
                // sim.Execute(nasmCode.Split('\n'));
                //
                // Console.WriteLine(sim.regs.rax.value);
            }

            //CmdExecutor.CompileRunAndCheck("../../../Compiler", "output.ll", 123);
        }
    }
}