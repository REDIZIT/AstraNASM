
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

        // Logical not
        if (@operator.asmOperatorName == "not")
        {
            result = ctx.gen.Allocate(PrimitiveTypes.BOOL);
            ctx.gen.LogicalNOT(right.result, this.result);
        }
        else if (@operator.asmOperatorName == "sub")
        {
            result = ctx.gen.Allocate(right.result.type);
            ctx.gen.Negate(right.result, this.result);
        }

        ctx.gen.Space();
    }
}