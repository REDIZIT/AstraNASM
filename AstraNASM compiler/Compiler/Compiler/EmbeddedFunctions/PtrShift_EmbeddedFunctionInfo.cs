namespace Astra.Compilation
{
    public class PtrShift_EmbeddedFunctionInfo : EmbeddedFunctionInfo
    {
        public Variable Generate(Generator.Context ctx, string pointerVariableName, Variable shiftVariable)
        {
            ctx.b.Space();
            ctx.b.CommentLine($"Shift pointer {pointerVariableName} by {shiftVariable.name}");

            var pointerVariable = ctx.GetVariable(pointerVariableName);

            ctx.b.Line($"mov rax, {pointerVariable.GetRBP()}");
            ctx.b.Line($"mov rbx, {shiftVariable.GetRBP()}");
            ctx.b.Line($"add rax, rbx");
            ctx.b.Line($"mov {pointerVariable.GetRBP()}, rax");

            ctx.b.Space();

            return null;
        }
    }
}