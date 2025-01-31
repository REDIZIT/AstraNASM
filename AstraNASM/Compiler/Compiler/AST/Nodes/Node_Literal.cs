
namespace Astra.Compilation;

public class Node_Literal : Node
{
    public Token_Constant constant;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield break;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        TypeInfo literalType = PrimitiveTypes.INT;

        result = ctx.AllocateStackVariable(literalType);

        ctx.b.Space();
        ctx.b.CommentLine("Literal = " +constant.value);
        ctx.b.Line($"sub rsp, 8");
        ctx.b.Line($"mov qword {result.RBP}, {constant.value}");

        ctx.b.Space();
    }
}
