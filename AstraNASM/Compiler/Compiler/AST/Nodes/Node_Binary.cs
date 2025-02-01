
namespace Astra.Compilation;

public class Node_Binary : Node
{
    public Node left, right;
    public Token_Operator @operator;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return left;
        yield return right;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);
        
        left.Generate(ctx);
        right.Generate(ctx);

        
        TypeInfo resultType = ctx.module.GetType(@operator.ResultType);
        

        result = ctx.gen.Allocate(resultType);
        

        if (@operator is Token_Comprassion || @operator is Token_Equality)
        {
            ctx.gen.Compare(left.result, right.result, @operator, this.result);
        }
        else
        {
            ctx.gen.Calculate(left.result, right.result, @operator, this.result);
        }

        ctx.gen.Space();
    }
}