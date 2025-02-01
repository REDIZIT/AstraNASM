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

        
        
        string[] nasm = Compilation.Compiler.Compile_Astra_to_NASM(code).Split('\n');

        
        
        Simulator sim = new();
        sim.Execute(nasm, 1024);

        if (sim.regs.rax.Get32() != int.Parse(returnResult))
        {
            Assert.Fail($"Bad simulation exit code. Expected: {returnResult}. Got: {sim.regs.rax.Get32()}");
        }
    }
}