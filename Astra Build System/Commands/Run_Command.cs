using System.Diagnostics;
using ManyConsole;

namespace Astra.BuildSystem;

public class Run_Command : ConsoleCommand
{
    public Run_Command()
    {
        IsCommand("run");
    }

    public override int Run(string[] remainingArguments)
    {
        Build_Command build = new();
        build.Run(null);
        
        string strCmdText = $"/C avm bin/{Program.settings.outputFileName}";
        Process p = Process.Start("cmd.exe", strCmdText);
        p.WaitForExit();
        
        return 0;
    }
}