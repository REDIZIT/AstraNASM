using System.Diagnostics;
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

        string compiledFilePath = Path.ChangeExtension(filepath, "asc");

        try
        {
            byte[] data = Compilation.Compiler.Compile_Astra(code, CompileTarget.Simulator);
            File.WriteAllBytes(compiledFilePath, data);
            
            Console.WriteLine(compiledFilePath);
        
            Process p = Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd",
                Arguments = $"/c avm \"{compiledFilePath}\" 1000",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });

            p.WaitForExit();

            string output = p.StandardOutput.ReadToEnd();
            Console.WriteLine(output);

            CompareResults(output, returnResult);
        }
        finally
        {
            File.Delete(compiledFilePath);
        }
    }
    
    private void CompareResults(string vmOutput, string expectedComment)
    {
        string[] lines = expectedComment.Split('\n');
        
        if (lines.Length == 0)
        {
            Assert.Fail("Test has no any expectations");
            return;
        }

        int exitCode;
        try
        {
            exitCode = int.Parse(vmOutput.Split('\n').Last(s => s.StartsWith("Successful executed in")).Split(' ').Last());
        }
        catch
        {
            exitCode = -1;
        }
        
    
        foreach (string line in lines)
        {
            if (int.TryParse(line, out int expectedExitCode))
            {
                int actualExitCode = exitCode;
                if (actualExitCode != expectedExitCode)
                {
                    Assert.Fail($"Bad simulation exit code. Expected: {expectedExitCode}. Got: {actualExitCode}");
                }
            }
            else
            {
                List<string> split = Utils.Split_StringSafe(line);
    
                string cmd = split[0];
                
                if (cmd == "exitcode")
                {
                    int expected = int.Parse(split[2]);
                    int actual = exitCode;
    
                    if (actual != expected)
                    {
                        Assert.Fail($"Bad simulation exit code. Expected: {expected}. Got: {actual}");
                    }
                }
                else if (cmd == "console")
                {
                    string expected = string.Concat(split[2..]);
                    expected = expected.Substring(1, expected.Length - 2);
    
                    string[] outSplit = vmOutput.Split('\n');
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