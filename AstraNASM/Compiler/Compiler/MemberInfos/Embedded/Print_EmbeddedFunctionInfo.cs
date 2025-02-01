namespace Astra.Compilation;

public class Print_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public void Generate(Generator.Context ctx, Variable variable)
    {
        ctx.gen.Print(variable);
    }
}