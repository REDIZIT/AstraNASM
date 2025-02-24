using Astra.Shared;

namespace Astra.Compilation;

public static class Generator
{
    public class Context
    {
        public CodeGeneratorBase gen;
        public ResolvedModule module;
    }
    
    public static byte[] Generate(List<Node> statements, ResolvedModule module, CompileTarget target)
    {
        Scope_GenerationPhase globalScope = new Scope_GenerationPhase(null)
        {
            uniqueGenerator = new()
        };
        
        Context ctx = new()
        {
            gen = new CodeGenerator_ByteCode()
            {
                currentScope = globalScope
            },
            module = module,
        };

        foreach (Node node in statements)
        {
            InjectEnvironmentVariables(node, ctx.gen);
        }
        
        
        module.data.Generate(ctx);
        
        ctx.gen.SectionText();
        ctx.gen.PrologueForSimulation(target, module);
        

        foreach (Node statement in statements)
        {
            statement.Generate(ctx);
        }

        if (ctx.gen.currentScope != globalScope)
        {
            throw new Exception($"Generation failed due to disbalanced scopes.");
        }
        
        List<byte> byteCode = ctx.gen.Build();

        CompiledModule file = CompiledModuleExtensions.Compile(module, byteCode.ToArray());
        byte[] bytes = BinarySerializer.Serialize(file).ToArray();
        
        return bytes;
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