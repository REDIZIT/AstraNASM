using ManyConsole;
using Newtonsoft.Json;

namespace Astra.BuildSystem;

public class Init_Command : ConsoleCommand
{
    public Init_Command()
    {
        IsCommand("init");
    }

    public override int Run(string[] remainingArguments)
    {
        Directory.CreateDirectory("src");
        Directory.CreateDirectory("bin");

        ProjectSettings settings = new();
        File.WriteAllText("project.json", JsonConvert.SerializeObject(settings, Formatting.Indented));
        
        Console.WriteLine("Done");

        return 0;
    }
}