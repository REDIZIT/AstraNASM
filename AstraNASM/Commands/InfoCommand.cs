using ManyConsole;

public class InfoCommand : ConsoleCommand
{
    public InfoCommand()
    {
        IsCommand("info");
    }

    public override int Run(string[] remainingArguments)
    {
        Console.WriteLine("Astra compiler v1.0.0");
        return 0;
    }
}
