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
                    instructions[i] = instruction.Replace(kv.Key, kv.Value.ToString());
                }
            }
        }

        return instructions;
    }
}