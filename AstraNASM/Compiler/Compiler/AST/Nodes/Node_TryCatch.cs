namespace Astra.Compilation;

public class Node_TryCatch : Node
{
    public Node tryBlock;
    public Node catchBlock;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return tryBlock;
        yield return catchBlock;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        string catchLabel = ctx.gen.RegisterLabel("trycatch_catch");
        string endLabel = ctx.gen.RegisterLabel("trycatch_end");
        
        ctx.gen.Space(1);
        ctx.gen.Comment("TryCatch block", 8);
        
        ctx.gen.PushExceptionHandler(catchLabel);
        
        ctx.gen.Comment("Try block", 6);
        tryBlock.Generate(ctx);
        ctx.gen.JumpToLabel(endLabel);
        
        ctx.gen.Space(1);
        ctx.gen.Comment("Catch block", 6);
        ctx.gen.Label(catchLabel);
        catchBlock.Generate(ctx);
        
        ctx.gen.Space(1);
        ctx.gen.Label(endLabel);
        ctx.gen.Space(1);
    }
}