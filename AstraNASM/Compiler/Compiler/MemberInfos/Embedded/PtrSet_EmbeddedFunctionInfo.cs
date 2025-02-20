namespace Astra.Compilation;

public class PtrSet_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public Variable Generate(Generator.Context ctx, Variable pointerVariable, Variable targetVariable)
    {
        ctx.gen.Space();
        ctx.gen.Comment($"PtrSet {targetVariable.name} to {pointerVariable.name}");
        
        ctx.gen.PtrSet(pointerVariable, targetVariable);

        ctx.gen.Space();

        return null;
    }
}