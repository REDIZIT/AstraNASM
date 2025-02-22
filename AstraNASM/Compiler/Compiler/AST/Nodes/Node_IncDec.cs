namespace Astra.Compilation;

public class Node_IncDec : Node
{
    public Node target;
    public Token_Operator @operator;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield break;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);
        
        target.Generate(ctx);

        if (@operator.asmOperatorName == "++")
        {
            ctx.gen.Increment(target.result);
            this.result = target.result;
        }
        else if (@operator.asmOperatorName == "--")
        {
            ctx.gen.Decrement(target.result);
            this.result = target.result;
        }

        ctx.gen.Space();
    }
}