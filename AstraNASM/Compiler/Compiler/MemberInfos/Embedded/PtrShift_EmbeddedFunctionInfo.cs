namespace Astra.Compilation;

public class PtrShift_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public Variable Generate(Generator.Context ctx, string pointerVariableName, Variable shiftVariable)
    {
        ctx.gen.Space();
        ctx.gen.Comment($"Shift pointer {pointerVariableName} by {shiftVariable.name}");

        var pointerVariable = ctx.gen.GetVariable(pointerVariableName);

        ctx.gen.PtrShift(pointerVariable, shiftVariable);

        ctx.gen.Space();

        return null;
    }
}