namespace Astra.Compilation;

public class PtrSet_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public Variable Generate(Generator.Context ctx, string pointerVariableName, Variable targetVar)
    {
        ctx.gen.Space();
        ctx.gen.Comment($"PtrSet {targetVar.name} to {pointerVariableName}");

        var pointerVar = ctx.gen.GetVariable(pointerVariableName);

        ctx.gen.PtrSet(pointerVar, targetVar);

        ctx.gen.Space();

        return null;
    }
}