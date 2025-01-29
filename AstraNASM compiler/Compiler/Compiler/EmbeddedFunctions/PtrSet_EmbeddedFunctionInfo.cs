namespace Astra.Compilation
{
    public class PtrSet_EmbeddedFunctionInfo : EmbeddedFunctionInfo
    {
        public Variable Generate(Generator.Context ctx, string pointerVariableName, Variable targetVar)
        {
            ctx.b.Space();
            ctx.b.CommentLine($"Set {targetVar.name} to {pointerVariableName}");

            var pointerVar = ctx.GetVariable(pointerVariableName);

            ctx.b.Line($"mov rax, {pointerVar.GetRBP()}");
            ctx.b.Line($"mov [rax], {targetVar.GetRBP()}");

            ctx.b.Space();

            return null;
        }
    }
}
