namespace Astra.Compilation
{
    public class Print_EmbeddedFunctionInfo : EmbeddedFunctionInfo
    {
        public void Generate(Generator.Context ctx, Variable variable)
        {
            ctx.b.Space();
            ctx.b.CommentLine($"print {variable.name}");
            ctx.b.Line($"mov rax, {variable.GetRBP()}");
            ctx.b.Line($"print [rax]");
        }
    }
}