namespace Astra.Simulation;

public static class Compiler
{
    public static List<string> Compile_NASM_to_Instructions(string[] nasm)
    {
        List<string> instructions = new();
        Dictionary<string, int> indexByTag = new();

        for (int i = 0; i < nasm.Length; i++)
        {
            string line = nasm[i];
            string instruction = line.Split(';')[0];
            if (string.IsNullOrWhiteSpace(instruction)) continue;

            if (instruction.EndsWith(':'))
            {
                indexByTag.Add(instruction.Substring(0, instruction.Length - 1), instructions.Count);
            }
            else
            {
                instructions.Add(instruction.TrimStart(' ').TrimStart('\t'));
            }
        }


        for (int i = 0; i < instructions.Count; i++)
        {
            string instruction = instructions[i];

            foreach (KeyValuePair<string, int> kv in indexByTag)
            {
                if (instruction.Contains(kv.Key))
                {
                    int index = instruction.IndexOf(kv.Key);
                    int endIndex = index + kv.Key.Length;

                    bool isLeftSep = index == 0 || index > 1 && (instruction[index - 1] == ' ' || instruction[index - 1] == '\t');
                    bool isRightSep = endIndex == instruction.Length || endIndex < instruction.Length && (instruction[endIndex] == ' ' || instruction[endIndex] == '\t');

                    if (isLeftSep && isRightSep)
                    {
                        //Console.WriteLine($"Replace label '{kv.Key}' with '{kv.Value.ToString()}' at '{instruction}'");
                        instructions[i] = instruction.Replace(kv.Key, kv.Value.ToString());
                    }
                }
            }
        }

        return instructions;
    }
}