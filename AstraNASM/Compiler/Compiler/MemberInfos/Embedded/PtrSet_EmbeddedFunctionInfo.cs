namespace Astra.Compilation;

public class PtrSet_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public Variable Generate(Generator.Context ctx, string pointerVariableName, Variable targetVar)
    {
        ctx.b.Space();
        ctx.b.CommentLine($"PtrSet {targetVar.name} to {pointerVariableName}");

        var pointerVar = ctx.GetVariable(pointerVariableName);

        ctx.b.Line($"mov rbx, {pointerVar.GetRBP()}");
        ctx.b.Line($"mov rdx, {targetVar.GetRBP()}");
        ctx.b.Line($"mov qword [rbx], rdx");

        ctx.b.Space();

        return null;
    }
}
