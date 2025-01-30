public static class Program
{
    public static void Main(string[] args)
    {
        string nasmFolder = Environment.CurrentDirectory + "/build";
        Simulator.ramDumpFilepath = Environment.CurrentDirectory + "/dumps/ram.bin";

        Console.WriteLine($"Simulating nasm files inside: '{nasmFolder}'");


        foreach (string filepath in Directory.GetFiles(nasmFolder, "*.nasm"))
        {
            Console.WriteLine("\n");
            RunTest(filepath, false);
        }

        //if (args.Length > 0)
        //{
            
        //}
        //else
        //{
        //    string[] testFiles = Directory.GetFiles("../../../Tests");

        //    RunTest(testFiles[7], false);

        //    //for (int i = 0; i < testFiles.Length; i++)
        //    //{
        //    //    if (RunTest(testFiles[i], true) == false)
        //    //    {
        //    //        throw new Exception($"Test '{Path.GetFileName(testFiles[i])}' failed");
        //    //    }
        //    //}

        //    Console.WriteLine("Simulations end");
        //    Console.ReadLine();
        //}
        
    }

    private static bool RunTest(string testFilePath, bool silent)
    {
        if (!silent) Console.WriteLine($"{Path.GetFileName(testFilePath)}:");

        string[] lines = File.ReadAllLines(testFilePath);

        int testResultsLine = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] == "---")
            {
                testResultsLine = i;
                break;
            }
        }


        string[] instructions = testResultsLine > 0 ? lines[0..testResultsLine] : lines;
        Simulator sim = new();
        sim.Execute(instructions);


        bool isSuccess = true;
        if (!silent) Console.WriteLine("\n---\n");

        if (testResultsLine != 0)
        {
            for (int i = testResultsLine + 1; i < lines.Length; i++)
            {
                string[] split = Utils.Split(lines[i].Trim(' ', '\t'));

                string regName = split[0];
                bool isEqual = false;

                if (regName == "ram")
                {
                    long address = long.Parse(split[1]);
                    byte[] awaitedValue = split[3].ToCharArray().Skip(1).SkipLast(1).Select(c => (byte)c).ToArray();
                    byte[] realValue = sim.ram.ReadBytes(address, awaitedValue.Length);

                    isEqual = true;
                    for (int j = 0; j < awaitedValue.Length; j++)
                    {
                        if (awaitedValue[j] != realValue[j])
                        {
                            isEqual = false;
                            break;
                        }
                    }

                    if (!silent) Console.WriteLine($"{(isEqual ? "[Good]" : "[Bad] ")} ram {address} = " + string.Concat(realValue.Select(b => (char)b)));
                }
                else
                {
                    long awaitedValue = Utils.ParseDec(split[2], sim.regs);
                    object readValue = sim.regs.Get(regName);
                    long realValue = Convert.ToInt64(readValue);

                    Reg64 reg = sim.regs.GetReg(regName);

                    isEqual = awaitedValue == realValue;

                    if (!silent) Console.WriteLine($"{(isEqual ? "[Good]" : "[Bad] ")} {regName} = " + reg.ToString(32));
                }

                if (isEqual == false) isSuccess = false;
            }
        }

        if (!silent) Console.WriteLine($"Exit code (rax) = {sim.regs.rax.ToString(32)}");
        return isSuccess;
    }
}