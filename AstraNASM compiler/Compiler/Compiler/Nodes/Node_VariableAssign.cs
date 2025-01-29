namespace Astra.Compilation
{

    public class Node_VariableAssign : Node
    {
        public Node target; // variable or field get
        public Node value;

        public override void RegisterRefs(RawModule module)
        {
            value.RegisterRefs(module);
        }
        public override void ResolveRefs(ResolvedModule module)
        {
            value.ResolveRefs(module);
        }

        public override void Generate(Generator.Context ctx)
        {
            base.Generate(ctx);

            target.Generate(ctx);
            value.Generate(ctx);

            ctx.b.Space();
            ctx.b.CommentLine($"{target.result.name} = {value.result.name}");

            if (target is Node_FieldAccess)
            {
                ctx.b.Line($"mov rax, {target.result.GetRBP()}");
                ctx.b.Line($"mov [rax], {value.result.GetRBP()}");
            }
            else
            {
                ctx.b.Line($"mov {target.result.GetRBP()}, {value.result.GetRBP()}");
            }

            //Utils.MoveValue(value.generatedVariableName, target.generatedVariableName, ctx);
        }
    }
}