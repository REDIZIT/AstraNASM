namespace Astra.Compilation;

public class Node_For : Node
{
    public Node declaration, condition, advance, body;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return declaration;
        yield return condition;
        yield return advance;
        yield return body;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        string endLabel = ctx.gen.RegisterLabel("for_end");
        string conditionLabel = ctx.gen.RegisterLabel("for_condition");

        ctx.gen.Comment("Node_For begin", 6);

        ctx.gen.Comment("for.declaration", 4);
        declaration.Generate(ctx);

        ctx.gen.Space();
        ctx.gen.Comment("for.condition", 4);
        ctx.gen.Label(conditionLabel);
        int rspRbpOffset = ctx.gen.AllocateRSPSaver();
        condition.Generate(ctx);


        ctx.gen.Space();
        ctx.gen.Comment("for.condition_check", 4);
        ctx.gen.JumpIfFalse(condition.result, endLabel);



        ctx.gen.Space();
        ctx.gen.Comment("for.body", 4);
        body.Generate(ctx);


        ctx.gen.Space();
        ctx.gen.Comment("for.advance", 4);
        advance.Generate(ctx);

        ctx.gen.Space();
        ctx.gen.RestoreRSPSaver(rspRbpOffset);
        ctx.gen.JumpToLabel(conditionLabel);


        ctx.gen.Space();
        ctx.gen.Label(endLabel);
        ctx.gen.DeallocateRSPSaver();
        ctx.gen.Comment("Node_For end", 6);
    }
}
