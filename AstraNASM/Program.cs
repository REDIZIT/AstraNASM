using Astra.Compilation;
using ManyConsole;

public static class Program
{    
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            //string fileContent = File.ReadAllText("C:/Users/REDIZIT/Documents/GitHub/Astra-Rider-extension/vscode extension/astralanguage/test/example.ac");
            //Compiler.Compile_Astra_to_NASM(fileContent, CompileTarget.Simulator);

            var cmd = new CompileCommand()
            {
                inputPath = "C:/Users/REDIZIT/Documents/GitHub/AstraOS/main/vscode project",
                outputPath = "C:/Users/REDIZIT/Documents/GitHub/AstraOS/main/vscode project/build",
                target = CompileTarget.NASM,
                isProject = true,
            };

            return cmd.Run(null);
        }

        var commands = GetCommands();

        return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
    }

    private static IEnumerable<ConsoleCommand> GetCommands()
    {
        return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
    }
}