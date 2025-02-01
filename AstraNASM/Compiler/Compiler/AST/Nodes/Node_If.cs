
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

        ctx.gen.Space();
        ctx.gen.Comment($"if {condition.result.name}");


        if (elseBranch == null)
        {
            ctx.gen.JumpIfFalse(condition.result);

            thenBranch.Generate(ctx);

            ctx.gen.Label("if_false");
        }
        else
        {
            ctx.gen.JumpIfFalse(condition.result);

            thenBranch.Generate(ctx);

            ctx.gen.JumpToLabel("if_end");
            ctx.gen.Label("if_false");

            elseBranch.Generate(ctx);

            ctx.gen.Label("if_end");
        }

        ctx.gen.Space();
    }
}