public class Node_Return : Node
{
    public Node expr;

    public override void RegisterRefs(RawModule module)
    {
        expr?.RegisterRefs(module);
    }
    public override void ResolveRefs(ResolvedModule module)
    {
        expr?.ResolveRefs(module);
    }
    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        ctx.b.Space(2);

        if (expr != null)
        {
            expr.Generate(ctx);

            ctx.b.Line($"mov rax, {expr.result.GetRBP()}");
        }

        ctx.b.Line("mov rsp, rbp");
        ctx.b.Line("pop rbp");
        ctx.b.Line("ret");
    }
}