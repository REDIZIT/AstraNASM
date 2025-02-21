using System.Text;
using AVM;
using NUnit.Framework.Internal;

namespace Astra.Tests;

[TestFixture]
public class FileTests
{
    private static IEnumerable<TestCaseData> GetTextFiles()
    {
        string folderPath = "../../../CodeExamples";
        foreach (var filePath in Directory.GetFiles(folderPath, "*.as"))
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



        byte[] data = Compilation.Compiler.Compile_Astra(code, CompileTarget.Simulator);
        
        string[] nasm = Encoding.UTF8.GetString(data).Split('\n');


        TextWriter outStream = new StringWriter();
        TextWriter prevOut = Console.Out;
        
        Console.SetOut(outStream);
        VM vm = new();
        vm.Load(data);
        vm.Execute();
        
        Console.SetOut(prevOut);
        prevOut.Write(outStream.ToString());

        CompareResults(vm, returnResult, outStream);
    }

    private void CompareResults(VM sim, string expectedComment, TextWriter outStream)
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
                int actualExitCode = sim.memory.ReadInt(0);
                if (actualExitCode != expectedExitCode)
                {
                    Assert.Fail($"Bad simulation exit code. Expected: {expectedExitCode}. Got: {actualExitCode}");
                }
            }
            else
            {
                List<string> split = Utils.Split_StringSafe(line);

                string cmd = split[0];
                
                if (cmd == "vga")
                {
                    string text = split[2].Substring(1, split[2].Length - 2);
                    byte[] expectedTextBytes = Encoding.ASCII.GetBytes(text);
                    byte[] vgaTextBytes = new byte[expectedTextBytes.Length];
                
                    for (int i = 0; i < expectedTextBytes.Length; i++)
                    {
                        vgaTextBytes[i] = sim.memory.Read(0xB8000 + i * 2);
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
                else if (cmd == "exitcode")
                {
                    int expected = int.Parse(split[2]);
                    int actual = sim.exitCode;

                    if (actual != expected)
                    {
                        Assert.Fail($"Bad simulation exit code. Expected: {expected}. Got: {actual}");
                    }
                }
                else if (cmd == "console")
                {
                    string expected = string.Concat(split[2..]);
                    expected = expected.Substring(1, expected.Length - 2);

                    string[] outSplit = outStream.ToString().Split('\n');
                    string actual = string.Concat(outSplit[..(outSplit.Length - 2)]).Trim('\r');
                    
                    if (actual != expected)
                    {
                        Assert.Fail($"Bad simulation console output. Expected: '{expected}'. Got: '{actual}'");
                    }
                }
            }
        }

       

        Assert.Pass();
    }
}