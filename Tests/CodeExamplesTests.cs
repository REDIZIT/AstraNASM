using System.Diagnostics;

namespace Astra.Tests;
public class CodeExamplesTests
{
    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public void Test()
    {
        string folder = "../../../CodeExamples";
        string[] files = Directory.GetFiles(folder, "*.ac");

        try
        {
            foreach (string filepath in files)
            {
                string testContent = File.ReadAllText(filepath);

                string[] split = testContent.Split("---");

                string code = split[0].Trim();
                string returnResult = split[1].Trim();

                string[] nasm;

                try
                {
                    nasm = Compilation.Compiler.Compile_Astra_to_NASM(code).Split('\n');
                }
                catch
                {
                    string message = $"Compilation failed: '{Path.GetFileName(filepath)}'";

                    Console.WriteLine(message);
                    throw;
                }

                Simulator sim = new();
                sim.Execute(nasm);

                if (sim.regs.rax.Get32() != int.Parse(returnResult))
                {
                    Assert.Fail($"Run failed: '{Path.GetFileName(filepath)}'\nExpected: {returnResult}\nGot: {sim.regs.rax.Get32()}");
                }
            }
        }
        finally
        {
            File.Delete(folder + "/temp.ll");
            File.Delete(folder + "/temp.exe");
        }

        Assert.Pass($"{files.Length} code examples checked");
    }

    private Process ExecuteCommand(string folder, string cmd)
    {
        string strCmdText;
        strCmdText = $"/C {cmd.Trim().Replace("\n", "&")}";

        ProcessStartInfo info = new()
        {
            FileName = "CMD.exe",
            Arguments = strCmdText,
            RedirectStandardOutput = true,
            WorkingDirectory = folder,
        };

        Process process = Process.Start(info);

        //string strOutput = process.StandardOutput.ReadToEnd();
        //process.WaitForExit();

        return process;
    }
}