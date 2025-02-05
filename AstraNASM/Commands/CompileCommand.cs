using Astra.Compilation;
using ManyConsole;
using System.Drawing;

public class CompileCommand : ConsoleCommand
{
    private string inputPath;
    private string outputPath;
    private CompileTarget target;

    public CompileCommand()
    {
        IsCommand("compile", "Compile Astra to NASM code");

        HasOption("i|input=", "Path to Astra file or to folder with Astra files. If option is empty, current working folder will be used.", p => inputPath = p);
        HasRequiredOption("o|output=", "Path of output NASM file or to output folder", p => outputPath = p);

        HasOption("t|target=", "Compilation target. Simulation-ready = 0 (default) or Machine-ready = 1",
            t => target = t == null ? CompileTarget.Simulator : (CompileTarget)int.Parse(t));
    }

    public override int Run(string[] remainingArguments)
    {
        if (string.IsNullOrEmpty(inputPath))
        {
            inputPath = Environment.CurrentDirectory;
        }

        if (Path.Exists(inputPath) == false)
        {
            throw new Exception($"Input path does not exists '{inputPath}'");
        }

        

        bool isInputFolder = string.IsNullOrWhiteSpace(Path.GetExtension(inputPath));
        bool isOutputFolder = string.IsNullOrWhiteSpace(Path.GetExtension(outputPath));

        if (isInputFolder != isOutputFolder)
        {
            if (isInputFolder) throw new Exception("Input path is folder, but output is not. Only file-to-file and folder-to-folder compilation allowed.");
            else throw new Exception("Output path is folder, but input is not. Only file-to-file and folder-to-folder compilation allowed.");
        }

        if (isInputFolder)
        {
            if (Directory.Exists(inputPath) == false) throw new Exception($"Input folder not found '{inputPath}'");
        }
        else
        {
            if (File.Exists(inputPath) == false) throw new Exception($"Input file not found '{inputPath}'");
        }

        bool isFolderMode = isInputFolder && isOutputFolder;

        if (isOutputFolder)
        {
            Directory.CreateDirectory(outputPath);
        }


        try
        {
            if (isFolderMode)
            {
                foreach (string filePath in Directory.GetFiles(inputPath, "*.ac"))
                {
                    Compile(filePath, outputPath + "/" + Path.GetFileNameWithoutExtension(filePath) + ".nasm");
                }
            }
            else
            {
                //  string fileContent = File.ReadAllText("C:\\Users\\REDIZIT\\Documents\\GitHub\\Astra-Rider-extension\\vscode extension\\astralanguage\\test\\example.ac");
                Compile(inputPath, outputPath + "/" + Path.GetFileNameWithoutExtension(inputPath) + ".nasm");
            }

            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.Write(" Sucesss ");
            Console.ResetColor();

            Console.WriteLine("\n");

            Console.WriteLine($"Absolute output path: '{Path.GetFullPath(outputPath)}'");
        }
        catch (Exception err)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.Write(" Compilation failed ");
            Console.ResetColor();

            Console.WriteLine("\n");

            Console.WriteLine(err.Message);
            Console.WriteLine(err.StackTrace);

            return 1;
        }
       
       

        return 0;
    }

    private void Compile(string astraFilePath, string nasmFilePath)
    {
        string fileContent = File.ReadAllText(astraFilePath);
        string nasmCode = Compiler.Compile_Astra_to_NASM(fileContent, target);
        File.WriteAllText(nasmFilePath, nasmCode);
    }
}
