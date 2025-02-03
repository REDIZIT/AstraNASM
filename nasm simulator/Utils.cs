public static class Utils
{
    public static string[] Split(string line)
    {
        return line.Split(';')[0].Replace(",", " ").Replace("  ", " ").Trim().Split(" ");
    }

    public static List<string> Split_StringSafe(string line)
    {
        List<string> args = new();

        List<char> arg = new();
        bool isCollectingString = false;
        
        for (int i = 0; i < line.Length; i++)
        {
            char currentChar = line[i];

            if (currentChar == ' ' || currentChar == ',')
            {
                if (isCollectingString)
                {
                    arg.Add(currentChar);
                }
                else
                {
                    if (arg.Count > 0)
                    {
                        args.Add(string.Concat(arg));
                        arg.Clear();   
                    }
                }
            }
            else if (currentChar == '"')
            {
                arg.Add(currentChar);
                
                if (isCollectingString)
                {
                    isCollectingString = false;
                    args.Add(string.Concat(arg));
                    arg.Clear();
                }
                else
                {
                    isCollectingString = true;
                }
            }
            else
            {
                arg.Add(currentChar);
            }
        }

        if (arg.Count > 0)
        {
            args.Add(string.Concat(arg));
            arg.Clear();
        }

        return args;
    }
    
    public static long ParseDec(string expression, Regs regs)
    {
        if (long.TryParse(expression, out long dec))
        {
            return dec;
        }
        else if (long.TryParse(expression.Substring(2, expression.Length - 2), System.Globalization.NumberStyles.HexNumber, null, out long hex))
        {
            return hex;
        }
        else if (expression.StartsWith('[') && expression.EndsWith(']'))
        {
            string body = expression.Substring(1, expression.Length - 2);
            return ParseMathExpression(body, regs);
        }
        else if (expression.Contains('+') || expression.Contains('-') || expression.Contains('*') || expression.Contains('/'))
        {
            return ParseMathExpression(expression, regs);
        }
        else if (regs.TryGet(expression, out object regValue))
        {
            return Convert.ToInt64(regValue);
        }
        else if (expression.StartsWith('"') && expression.EndsWith('"'))
        {
            return (long)expression[1];
        }
        else if (expression.StartsWith('\'') && expression.EndsWith('\''))
        {
            return (long)expression[1];
        }

        throw new($"Failed to parse dec expression '{expression}'");
    }
    private static long ParseMathExpression(string body, Regs regs)
    {
        string[] args = body.Replace('-', '+').Replace('*', '+').Replace('/', '+').Split('+');
        char op = body.Contains('+') ? '+' : body.Contains('-') ? '-' : body.Contains('*') ? '*' : '/';

        if (args.Length > 1)
        {
            long a = ParseDec(args[0], regs);
            long b = ParseDec(args[1], regs);

            if (op == '+') return a + b;
            if (op == '-') return a - b;
            if (op == '*') return a * b;
            if (op == '/') return a / b;
        }
        else
        {
            return ParseDec(args[0], regs);
        }

        throw new Exception($"Failed to parse dec math expression '{body}'");
    }
}