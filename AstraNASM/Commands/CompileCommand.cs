using Astra.Compilation;
using ManyConsole;

public class CompileCommand : ConsoleCommand
{
    public string inputPath;
    public string outputPath;
    public CompileTarget target;
    public bool isProject;
    public string copyOutputPath;

    public CompileCommand()
    {
        IsCommand("compile", "Compile Astra to NASM code");

        HasOption("i|input=", "Path to Astra file or to folder with Astra files. If option is empty, current working folder will be used.", p => inputPath = p);
        HasRequiredOption("o|output=", "Path of output NASM file or to output folder", p => outputPath = p);

        HasOption("t|target=", "Compilation target. Simulation-ready = 0 (default) or Machine-ready = 1",
            t => target = t == null ? CompileTarget.Simulator : (CompileTarget)int.Parse(t));

        HasOption("p|project", "Bundle all Astra files into one nasm file", s => { isProject = true; });

        HasOption("co|copy-output=", "Path to folder, where to copy output", s => { copyOutputPath = s; });
    }

    public override int Run(string[] remainingArguments)
    {
        if (isProject)
        {
            return CompileProject();
        }
        else
        {
            return CompileSingles();
        }
    }

    private int CompileSingles()
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
                    string nasmFileName = Path.GetFileNameWithoutExtension(inputPath) + ".nasm";
                    string nasmFilePath = outputPath + "/" + nasmFileName;

                    Compile(filePath, nasmFilePath);

                    if (copyOutputPath != null)
                    {
                        File.Copy(nasmFilePath, copyOutputPath + "/" + nasmFileName, true);
                    }
                }
            }
            else
            {
                //  string fileContent = File.ReadAllText("C:\\Users\\REDIZIT\\Documents\\GitHub\\Astra-Rider-extension\\vscode extension\\astralanguage\\test\\example.ac");

                string nasmFileName = Path.GetFileNameWithoutExtension(inputPath) + ".nasm";
                string nasmFilePath = outputPath + "/" + nasmFileName;

                Compile(inputPath, nasmFilePath);

                if (copyOutputPath != null)
                {
                    File.Copy(nasmFilePath, copyOutputPath + "/" + nasmFileName, true);
                }
            }

            PrintSuccess();
        }
        catch (Exception err)
        {
            PrintFail(err);
            return 1;
        }


        return 0;
    }
    private int CompileProject()
    {
        // try
        // {
            if (string.IsNullOrEmpty(inputPath))
            {
                inputPath = Environment.CurrentDirectory;
            }

            if (Directory.Exists(outputPath) == false)
            {
                Directory.CreateDirectory(outputPath);
            }


            string nasmFileName = "project.nasm";
            string nasmFilePath = outputPath + "/" + nasmFileName;

            Compile(Directory.GetFiles(inputPath, "*.ac").ToList(), nasmFilePath);

            if (copyOutputPath != null)
            {
                File.Copy(nasmFilePath, copyOutputPath + "/" + nasmFileName, true);
            }


            PrintSuccess();
            return 0;
        // }
        // catch (Exception err)
        // {
        //     PrintFail(err);
        //     return 1;
        // }
    }

    private void Compile(string astraFilePath, string nasmFilePath)
    {
        string fileContent = File.ReadAllText(astraFilePath);
        string nasmCode = Compiler.Compile_Astra_to_NASM(fileContent, target);
        File.WriteAllText(nasmFilePath, nasmCode);
    }
    private void Compile(List<string> astraFiles, string nasmFilePath)
    {
        List<string> astraFileContent = new List<string>();

        foreach (string path in astraFiles)
        {
            astraFileContent.Add(File.ReadAllText(path));
        }

        string nasmCode = Compiler.Compile_AstraProject(astraFileContent, target);
        File.WriteAllText(nasmFilePath, nasmCode);
    }

    private void PrintSuccess()
    {
        Console.BackgroundColor = ConsoleColor.DarkGreen;
        Console.Write(" Sucesss ");
        Console.ResetColor();

        Console.WriteLine("\n");

        Console.WriteLine($"Absolute output path: '{Path.GetFullPath(outputPath)}'");
    }
    private void PrintFail(Exception err)
    {
        Console.BackgroundColor = ConsoleColor.Red;
        Console.Write(" Compilation failed ");
        Console.ResetColor();

        Console.WriteLine("\n");

        Console.WriteLine(err.Message);
        Console.WriteLine(err.StackTrace);
    }
}