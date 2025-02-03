
namespace Astra.Compilation;

public class Node_Literal : Node
{
    public Token_Constant constant;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield break;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);
        
        ctx.gen.Space();

        if (constant is Token_Char)
        {
            ctx.gen.Comment($"Literal = '{constant.value}'");
            result = ctx.gen.Allocate(PrimitiveTypes.LONG);
            ctx.gen.SetValue(result, "'" + constant.value + "'");
        }
        else
        {
            ctx.gen.Comment("Literal = " + constant.value);
            result = ctx.gen.Allocate(PrimitiveTypes.LONG);
            ctx.gen.SetValue(result, constant.value);
        }

        ctx.gen.Space();
    }
}