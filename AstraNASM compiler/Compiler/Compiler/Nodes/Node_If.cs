namespace Astra.Compilation
{
    public class Node_If : Node
    {
        public Node condition, thenBranch, elseBranch;

        public override void RegisterRefs(RawModule module)
        {
            condition.RegisterRefs(module);
            thenBranch.RegisterRefs(module);
            elseBranch?.RegisterRefs(module);
        }
        public override void ResolveRefs(ResolvedModule module)
        {
            condition.ResolveRefs(module);
            thenBranch.ResolveRefs(module);
            elseBranch?.ResolveRefs(module);
        }
        public override void Generate(Generator.Context ctx)
        {
            base.Generate(ctx);

            condition.Generate(ctx);


            //if (condition.result.type != PrimitiveTypeInfo.BOOL)
            //{
            //    string castedConditionVariable = ctx.NextTempVariableName(PrimitiveTypeInfo.BOOL);
            //    ctx.b.Line($"{castedConditionVariable} = trunc {ctx.GetVariableType(valueConditionVariable)} {valueConditionVariable} to i1");
            //    valueConditionVariable = castedConditionVariable;
            //}

            ctx.b.Space();
            ctx.b.CommentLine($"if {condition.result.name}");


            if (elseBranch == null)
            {
                ctx.b.Line($"mov rax, {condition.result.RBP}");
                ctx.b.Line($"cmp rax, 0");
                ctx.b.Line($"jle if_false");

                thenBranch.Generate(ctx);

                ctx.b.Line($"if_false:");
            }
            else
            {
                ctx.b.Line($"mov rax, {condition.result.RBP}");
                ctx.b.Line($"cmp rax, 0");
                ctx.b.Line($"jle if_false");

                thenBranch.Generate(ctx);

                ctx.b.Line($"jmp if_end");
                ctx.b.Line($"if_false:");

                elseBranch.Generate(ctx);

                ctx.b.Line("if_end:");
            }

            ctx.b.Space();
        }
    }
}