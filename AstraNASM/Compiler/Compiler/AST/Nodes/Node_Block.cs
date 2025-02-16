namespace Astra.Compilation;

public class Node_Block : Node
{
    public List<Node> children = new();

    public override IEnumerable<Node> EnumerateChildren()
    {
        foreach (var child in children)
        {
            yield return child;
        }
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);
        foreach (Node child in children) child.Generate(ctx);
    }
}