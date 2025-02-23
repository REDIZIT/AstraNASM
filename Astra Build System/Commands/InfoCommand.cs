using ManyConsole;

public class InfoCommand : ConsoleCommand
{
    public InfoCommand()
    {
        IsCommand("info");
    }

    public override int Run(string[] remainingArguments)
    {
        Console.WriteLine("Astra Build System");
        return 0;
    }
}