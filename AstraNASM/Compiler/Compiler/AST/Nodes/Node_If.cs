
namespace Astra.Compilation;

public class Node_If : Node
{
    public Node condition, thenBranch, elseBranch;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return condition;
        yield return thenBranch;

        if (elseBranch != null) yield return elseBranch;
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
            ctx.b.Line($"mov rbx, {condition.result.RBP}");
            ctx.b.Line($"cmp rbx, 0");
            ctx.b.Line($"jle if_false");

            thenBranch.Generate(ctx);

            ctx.b.Line($"if_false:");
        }
        else
        {
            ctx.b.Line($"mov rbx, {condition.result.RBP}");
            ctx.b.Line($"cmp rbx, 0");
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