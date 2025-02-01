namespace Astra.Compilation;

public class Node_Return : Node
{
    public Node expr;
    public FunctionInfo function;

    public override IEnumerable<Node> EnumerateChildren()
    {
        if (expr != null) yield return expr;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        ctx.gen.Space(2);

        if (function.returns.Count > 0)
        {
            if (function.returns.Count > 1) throw new Exception("Not supported yet");
            
            if (expr != null)
            {
                expr.Generate(ctx);

                ctx.gen.Space();
                ctx.gen.Return_Variable(function, expr.result);
            }
            else
            {
                throw new Exception("Syntax error: Function has return type, but return node does not have any expression to return.");
            }
        }
        
        ctx.gen.Epilogue();
        ctx.gen.Return_Void();
    }
}