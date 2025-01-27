public class Node_Binary : Node
{
    public Node left, right;
    public Token_Operator @operator;

    public override void RegisterRefs(RawModule module)
    {
        left.RegisterRefs(module);
        right.RegisterRefs(module);
    }
    public override void ResolveRefs(ResolvedModule module)
    {
        left.ResolveRefs(module);
        right.ResolveRefs(module);
    }
    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        left.Generate(ctx);
        right.Generate(ctx);

        TypeInfo resultType = ctx.module.GetType(@operator.ResultType);

        result = ctx.AllocateStackVariable(resultType);

        ctx.b.Line($"mov rax, {left.result.GetRBP()}");
        ctx.b.Line($"mov rbx, {right.result.GetRBP()}");
        ctx.b.Line($"{@operator.asmOperatorName} rax, rbx");
        ctx.b.Line($"mov {result.GetRBP()}, rax");

        ctx.b.Space();
    }
}
