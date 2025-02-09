namespace Astra.Compilation;

public class Node_Throw : Node
{
    public Node exception;
    
    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return exception;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);
        
        exception.Generate(ctx);
        
        ctx.gen.ThrowException(exception.result);
    }
}