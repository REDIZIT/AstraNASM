namespace Astra.Compilation
{
    public class Node_Unary : Node
    {
        public Node right;
        public Token_Operator @operator;

        public override void RegisterRefs(RawModule module)
        {
            right.RegisterRefs(module);
        }
        public override void ResolveRefs(ResolvedModule module)
        {
            right.ResolveRefs(module);
        }
        public override void Generate(Generator.Context ctx)
        {
            base.Generate(ctx);

            right.Generate(ctx);
            Variable rightResult = right.result;


            // Logical not
            if (@operator.asmOperatorName == "not")
            {
                result = ctx.AllocateStackVariable(PrimitiveTypeInfo.BOOL);

                ctx.b.Line($"mov rax, {rightResult.GetRBP()}");
                ctx.b.Line($"test rax, rax");
                ctx.b.Line($"xor rax, rax"); // reset rax to zero
                ctx.b.Line($"sete al"); // set last byte of reg to 1 or 0
                ctx.b.Line($"mov {result.GetRBP()}, rax");
            }
            else if (@operator.asmOperatorName == "sub")
            {
                result = ctx.AllocateStackVariable(rightResult.type);

                ctx.b.Line($"mov rax, {rightResult.GetRBP()}");
                ctx.b.Line($"neg rax");
                ctx.b.Line($"mov {result.GetRBP()}, rax");
            }

            ctx.b.Space();
        }
    }
}