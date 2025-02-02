namespace Astra.Compilation;

public class Node_While : Node
{
    public Node condition, body;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return condition;
        yield return body;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);
        
        ctx.gen.Label("while_condition");
        condition.Generate(ctx);
        
        ctx.gen.JumpIfFalse(condition.result, "while_end");
        
        body.Generate(ctx);
        ctx.gen.JumpToLabel("while_condition");
        
        ctx.gen.Label("while_end");
    }
}