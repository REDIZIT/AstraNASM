
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
            result = ctx.gen.Allocate(PrimitiveTypes.INT);
            ctx.gen.SetValue(result, "'" + constant.value + "'");
        }
        else if (constant is Token_String)
        {
            result = ctx.gen.Allocate(PrimitiveTypes.STRING);
            ctx.gen.SetValue(result, constant.value);
        }
        else
        {
            result = ctx.gen.Allocate(PrimitiveTypes.INT);
            ctx.gen.SetValue(result, constant.value);
        }

        ctx.gen.Space();
    }
}