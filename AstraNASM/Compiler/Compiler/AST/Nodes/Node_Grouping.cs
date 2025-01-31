
namespace Astra.Compilation;

public class Node_Grouping : Node
{
    public Node expression;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return expression;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        expression.Generate(ctx);
        result = expression.result;
    }
}
