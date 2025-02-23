using Astra.Compilation;
using ManyConsole;

namespace Astra.BuildSystem;

public class Build_Command : ConsoleCommand
{
    public Build_Command()
    {
        IsCommand("build");
    }

    public override int Run(string[] remainingArguments)
    {
        List<string> contents = new();
        foreach (string filepath in Directory.GetFiles("src", "*.as"))
        {
            string content = File.ReadAllText(filepath);
            contents.Add(content);
        }
        
        byte[] bytes = Compiler.Compile_AstraProject(contents, CompileTarget.AVM);
        
        File.WriteAllBytes("bin/" + Program.settings.outputFileName, bytes);

        return 0;
    }
}