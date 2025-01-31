
namespace Astra.Compilation;

public class Node_Unary : Node
{
    public Node right;
    public Token_Operator @operator;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return right;
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

            ctx.b.Line($"mov rbx, {rightResult.GetRBP()}");
            ctx.b.Line($"test rbx, rbx");
            ctx.b.Line($"xor rbx, rbx"); // reset rbx to zero
            ctx.b.Line($"sete bl"); // set last byte of reg to 1 or 0
            ctx.b.Line($"mov {result.GetRBP()}, rbx");
        }
        else if (@operator.asmOperatorName == "sub")
        {
            result = ctx.AllocateStackVariable(rightResult.type);

            ctx.b.Line($"mov rbx, {rightResult.GetRBP()}");
            ctx.b.Line($"neg rbx");
            ctx.b.Line($"mov {result.GetRBP()}, rbx");
        }

        ctx.b.Space();
    }
}