namespace Astra.Compilation;

public class PtrShift_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public Variable Generate(Generator.Context ctx, string pointerVariableName, Variable shiftVariable)
    {
        ctx.b.Space();
        ctx.b.CommentLine($"Shift pointer {pointerVariableName} by {shiftVariable.name}");

        var pointerVariable = ctx.GetVariable(pointerVariableName);

        ctx.b.Line($"mov rbx, {pointerVariable.GetRBP()}");
        ctx.b.Line($"mov rdx, {shiftVariable.GetRBP()}");
        ctx.b.Line($"add rbx, rdx");
        ctx.b.Line($"mov {pointerVariable.GetRBP()}, rbx");

        ctx.b.Space();

        return null;
    }
}