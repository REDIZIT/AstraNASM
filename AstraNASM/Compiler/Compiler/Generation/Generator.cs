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

        public CodeGenerator_ByteCode gen;
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

    public static byte[] Generate(List<Node> statements, ResolvedModule module, CompileTarget target)
    {
        Context ctx = new()
        {
            module = module,
            gen = new()
            {
                b = new()
            }
        };


        foreach (Node node in statements)
        {
            InjectEnvironmentVariables(node, ctx.gen);
        }
        
        
        
        if (module.stringValueByID.Count > 0)
        {
            ctx.gen.SectionData();
            
            foreach (KeyValuePair<string, string> kv in module.stringValueByID)
            {
                ctx.gen.BufferString(kv.Key, kv.Value);
            }

            ctx.gen.Space(1);
            ctx.gen.SectionText();
        }
        

        ctx.gen.PrologueForSimulation(target, module);
        

        foreach (Node statement in statements)
        {
            statement.Generate(ctx);
        }
        
        return ctx.gen.Build();
    }

    private static void InjectEnvironmentVariables(Node node, CodeGeneratorBase gen)
    {
        if (node is Node_VariableDeclaration decl)
        {
            if (decl.fieldInfo.name == "_binary_src_font_psf_start")
            {
                decl.initValue = new Node_Literal()
                {
                    constant = new Token_Constant("_binary_src_font_psf_start")
                };

                gen.Extern(decl.fieldInfo.name);
            }
        }
        
        foreach (Node child in node.EnumerateChildren())
        {
            InjectEnvironmentVariables(child, gen);
        }
    }
}