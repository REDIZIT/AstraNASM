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


        string endLabel = ctx.gen.RegisterLabel("while_end");
        string conditionLabel = ctx.gen.RegisterLabel("while_condition");


        ctx.gen.Label(conditionLabel);
        int rspRbpOffset = ctx.gen.AllocateRSPSaver();
        condition.Generate(ctx);


        ctx.gen.Space();
        ctx.gen.JumpIfFalse(condition.result, endLabel);


        ctx.gen.Space();
        body.Generate(ctx);


        ctx.gen.RestoreRSPSaver(rspRbpOffset);
        ctx.gen.JumpToLabel(conditionLabel);


        ctx.gen.Space();
        ctx.gen.Label(endLabel);
        ctx.gen.DeallocateRSPSaver();
    }
}