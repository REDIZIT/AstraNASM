using System.Text;
using NUnit.Framework.Internal;

namespace Astra.Tests;

[TestFixture]
public class FileTests
{
    private static IEnumerable<TestCaseData> GetTextFiles()
    {
        string folderPath = "../../../CodeExamples";
        foreach (var filePath in Directory.GetFiles(folderPath, "*.ac"))
        {
            yield return new TestCaseData(filePath).SetName(Path.GetFileName(filePath));
        }
    }

    [TestCaseSource(nameof(GetTextFiles))]
    public void TestFileProcessing(string filepath)
    {
        string testContent = File.ReadAllText(filepath);

        int testIndex = testContent.LastIndexOf("---");

        string code = testContent.Substring(0, testIndex).Trim();
        string returnResult = testContent.Substring(testIndex + 3, testContent.Length - testIndex - 3).Trim();



        byte[] data = Compilation.Compiler.Compile_Astra_to_NASM(code, CompileTarget.Simulator);
        
        string[] nasm = Encoding.UTF8.GetString(data).Split('\n');


        Simulator sim = new();
        sim.Execute(nasm, 1024);

        CompareResults(sim, returnResult);
    }

    private void CompareResults(Simulator sim, string expectedComment)
    {
        string[] lines = expectedComment.Split('\n');
        
        if (lines.Length == 0)
        {
            Assert.Fail("Test has no any expectations");
            return;
        }

        foreach (string line in lines)
        {
            if (int.TryParse(line, out int expectedExitCode))
            {
                int actualExitCode = sim.regs.rax.Get32();
                if (actualExitCode != expectedExitCode)
                {
                    Assert.Fail($"Bad simulation exit code. Expected: {expectedExitCode}. Got: {actualExitCode}");
                }
            }
            else
            {
                List<string> split = Utils.Split_StringSafe(line);

                if (sim.regs.TryGetReg(split[0], out Reg64 reg))
                {
                    int expectedValue = int.Parse(split[2]);
                    if (reg.value != expectedValue)
                    {
                        Assert.Fail($"Bad simulation {split[0]} value. Expected: {expectedValue}. Got: {reg.value}");
                    }
                }
                else if (split[0] == "vga")
                {
                    string text = split[2].Substring(1, split[2].Length - 2);
                    byte[] expectedTextBytes = Encoding.ASCII.GetBytes(text);
                    byte[] vgaTextBytes = new byte[expectedTextBytes.Length];

                    for (int i = 0; i < expectedTextBytes.Length; i++)
                    {
                        vgaTextBytes[i] = sim.ram.ReadByte(0xB8000 + i * 2);
                    }
                    
                    Console.WriteLine(text);
                    Console.WriteLine(Encoding.ASCII.GetString(vgaTextBytes));

                    for (int i = 0; i < expectedTextBytes.Length; i++)
                    {
                        byte expectedByte = expectedTextBytes[i];
                        byte actualByte = vgaTextBytes[i];

                        if (actualByte != expectedByte)
                        {
                            string actualText = Encoding.ASCII.GetString(vgaTextBytes);
                            Assert.Fail($"Bad simulation vga text content. Expected: {text}. Got: {actualText}. Invalid byte found at index: {i}");
                        }
                    }
                }
            }
        }

       

        Assert.Pass();
    }
}