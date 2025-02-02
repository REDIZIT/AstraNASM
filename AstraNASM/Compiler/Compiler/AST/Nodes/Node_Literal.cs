
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

        TypeInfo literalType = PrimitiveTypes.LONG;
        
        ctx.gen.Space();
        ctx.gen.Comment("Literal = " + constant.value);
        result = ctx.gen.Allocate(literalType);
        ctx.gen.SetValue(result, constant.value);

        ctx.gen.Space();
    }
}