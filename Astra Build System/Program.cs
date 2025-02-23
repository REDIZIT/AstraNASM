using ManyConsole;
using Newtonsoft.Json;

namespace Astra.BuildSystem;

public static class Program
{
    public static ProjectSettings settings;
    
    public static int Main(string[] args)
    {
        var commands = GetCommands();

        if (File.Exists("project.json"))
        {
            settings = JsonConvert.DeserializeObject<ProjectSettings>(File.ReadAllText("project.json"));   
        }
        else
        {
            settings = null;
        }

        return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
    }

    private static IEnumerable<ConsoleCommand> GetCommands()
    {
        return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
    }
}