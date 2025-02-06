public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            string nasmFolder = Environment.CurrentDirectory + "/build";
            Simulator.ramDumpFilepath = Environment.CurrentDirectory + "/dumps/ram.bin";

            Console.WriteLine($"Simulating nasm files inside: '{nasmFolder}'");


            int stackAddress = 256;
            if (args.Length > 1 && args[1].StartsWith("stack="))
            {
                stackAddress = int.Parse(args[1].Substring("stack=".Length));
            }


            foreach (string filepath in Directory.GetFiles(nasmFolder, "*.nasm"))
            {
                Console.WriteLine("\n");
                RunTest(filepath, false, stackAddress);
            }
        }
        else
        {
            string[] testFiles = Directory.GetFiles("../../../Tests");

            RunTest(testFiles.Last(), false, 1024);

            //for (int i = 0; i < testFiles.Length; i++)
            //{
            //    if (RunTest(testFiles[i], true) == false)
            //    {
            //        throw new Exception($"Test '{Path.GetFileName(testFiles[i])}' failed");
            //    }
            //}

            Console.WriteLine("Simulations end");
            Console.ReadLine();
        }
    }

    private static bool RunTest(string testFilePath, bool silent, int stackAddress)
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
        sim.Execute(instructions, stackAddress);


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

        if (!silent)
        {
            Console.WriteLine($"Exit code (rax) = {sim.regs.rax.ToString(32)}");

            Console.WriteLine($"\nVGA text:");
            long vgaAddress = 0xB8000;

            for (int y = 0; y < 25; y++)
            {
                for (int x = 0; x < 80; x++)
                {
                    byte character = sim.ram.ReadByte(vgaAddress + 2 * (y * 80 + x));
                    byte color = sim.ram.ReadByte(vgaAddress + 2 * (y * 80 + x) + 1);

                    if (character == 0)
                    {
                        Console.Write(' ');
                    }
                    else
                    {
                        Console.Write((char)character);
                    }
                }

                Console.WriteLine();
            }
        }
        return isSuccess;
    }
}