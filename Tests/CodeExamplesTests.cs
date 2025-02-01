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

                int testIndex = testContent.LastIndexOf("---");

                string code = testContent.Substring(0, testIndex).Trim();
                string returnResult = testContent.Substring(testIndex + 3, testContent.Length - testIndex - 3).Trim();

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

                try
                {
                    Simulator sim = new();
                    sim.Execute(nasm, 1024);

                    if (sim.regs.rax.Get32() != int.Parse(returnResult))
                    {
                        Assert.Fail($"Run failed: '{Path.GetFileName(filepath)}'\nExpected: {returnResult}\nGot: {sim.regs.rax.Get32()}");
                    }
                }
                catch
                {
                    string message = $"Simulation failed: '{Path.GetFileName(filepath)}'";

                    Console.WriteLine(message);
                    throw;
                }
            }
        }
        finally
        {
        }

        Assert.Pass($"{files.Length} code examples checked");
    }
}