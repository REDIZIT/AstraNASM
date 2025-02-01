
namespace Astra.Compilation;

public class Node_Binary : Node
{
    public Node left, right;
    public Token_Operator @operator;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return left;
        yield return right;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        string leftResult, rightResult;

        if (left is Node_Literal leftLiteral)
        {
            leftResult = leftLiteral.constant.value;
        }
        else
        {
            left.Generate(ctx);
            leftResult = left.result.RBP;
        }
        
        if (right is Node_Literal rightLiteral)
        {
            rightResult = rightLiteral.constant.value;
        }
        else
        {
            right.Generate(ctx);
            rightResult = right.result.RBP;
        }

        
        TypeInfo resultType = ctx.module.GetType(@operator.ResultType);

        result = ctx.AllocateStackVariable(resultType);
        ctx.b.Line($"sub rsp, 8");

        ctx.b.Line($"mov rbx, {leftResult}");
        ctx.b.Line($"mov rdx, {rightResult}");

        if (@operator is Token_Comprassion || @operator is Token_Equality)
        {
            ctx.b.Line($"cmp rbx, rdx");
            ctx.b.Line($"mov rbx, 0");
            ctx.b.Line($"set{@operator.asmOperatorName} bl");
            ctx.b.Line($"mov {result.GetRBP()}, rbx");
        }
        else
        {
            ctx.b.Line($"{@operator.asmOperatorName} rbx, rdx");
            ctx.b.Line($"mov {result.GetRBP()}, rbx");
        }

        ctx.b.Space();
    }
}