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

        string conditionLabel = ctx.gen.RegisterLabel("while_condition");
        string endLabel = ctx.gen.RegisterLabel("while_end");
        
        ctx.gen.Label(conditionLabel);
        condition.Generate(ctx);
        
        ctx.gen.JumpIfFalse(condition.result, endLabel);
        
        body.Generate(ctx);
        ctx.gen.JumpToLabel(conditionLabel);
        
        ctx.gen.Label(endLabel);
    }
}