namespace Astra.Compilation
{
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
            ctx.b.Line($"sub rsp, 8");

            ctx.b.Line($"mov rax, {left.result.GetRBP()}");
            ctx.b.Line($"mov rbx, {right.result.GetRBP()}");

            if (@operator is Token_Comprassion || @operator is Token_Equality)
            {
                ctx.b.Line($"cmp rax, rbx");
                ctx.b.Line($"mov rax, 0");
                ctx.b.Line($"set{@operator.asmOperatorName} al");
                ctx.b.Line($"mov {result.GetRBP()}, rax");
            }
            else
            {
                ctx.b.Line($"{@operator.asmOperatorName} rax, rbx");
                ctx.b.Line($"mov {result.GetRBP()}, rax");
            }

            ctx.b.Space();
        }
    }
}
