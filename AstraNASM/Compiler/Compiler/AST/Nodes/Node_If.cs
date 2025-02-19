
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

        string falseLabel = ctx.gen.RegisterLabel("if_false");

        if (elseBranch == null)
        {
            ctx.gen.JumpIfFalse(condition.result, falseLabel);

            thenBranch.Generate(ctx);

            ctx.gen.Label(falseLabel);
        }
        else
        {
            string endLabel = ctx.gen.RegisterLabel("if_end");

            ctx.gen.JumpIfFalse(condition.result, falseLabel);

            thenBranch.Generate(ctx);

            ctx.gen.JumpToLabel(endLabel);
            ctx.gen.Label(falseLabel);

            elseBranch.Generate(ctx);

            ctx.gen.Label(endLabel);
        }

        ctx.gen.Space();
    }
}