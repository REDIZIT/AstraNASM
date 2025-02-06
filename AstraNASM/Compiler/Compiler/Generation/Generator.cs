namespace Astra.Compilation;

public class Variable
{
    public string name;
    public TypeInfo type;
    public int rbpOffset;

    public string RBP
    {
        get
        {
            if (rbpOffset > 0) return $"[rbp+{rbpOffset}]";
            else return $"[rbp{rbpOffset}]";
        }
    }
}

public static class Generator
{
    public class Context
    {
        public Context parent;

        public CodeGenerator gen;
        public ResolvedModule module;

        public Context CreateSubContext()
        {
            Context ctx = new()
            {
                parent = this,
                module = module,
            };

            ctx.gen = new()
            {
                parent = gen,
                b = gen.b
            };
            
            gen.children.Add(ctx.gen);

            return ctx;
        }
    }

    public static string Generate(List<Node> statements, ResolvedModule module, CompileTarget target)
    {
        Context ctx = new()
        {
            module = module,
            gen = new()
            {
                b = new()
            }
        };
        
        
        if (module.strings.Count > 0)
        {
            ctx.gen.SectionData();
            
            foreach (string str in module.strings)
            {
                ctx.gen.BufferString(str);
            }

            ctx.gen.Space(1);
            ctx.gen.SectionText();
        }
        

        ctx.gen.PrologueForSimulation(target);
        

        foreach (Node statement in statements)
        {
            statement.Generate(ctx);
        }

        return FormatNASM(ctx.gen.BuildString());
    }

    private static string FormatNASM(string nasm)
    {
        string[] lines = nasm.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (line.Contains(":") == false && line.StartsWith(';') == false)
            {
                lines[i] = '\t' + line;
            }
        }

        return string.Join('\n', lines);
    }
}