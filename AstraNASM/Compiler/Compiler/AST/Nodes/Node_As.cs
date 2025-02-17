namespace Astra.Compilation;

public class Node_As : Node
{
    public Node left;
    public Token_Identifier typeToken;
    
    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return left;
    }

    public override void Generate(Generator.Context ctx)
    {
        left.Generate(ctx);

        TypeInfo type = ctx.module.GetType(typeToken.name);
        
        
        if (PrimitiveTypes.IsPrimitiveOrPtr(left.result.type) == false || PrimitiveTypes.IsPrimitiveOrPtr(type) == false)
        {
            throw new Exception("Using 'as' for non-primitive types is not allowed");
        }
        
        
        result = ctx.gen.Allocate(type);
        
        ctx.gen.Cast(left.result, result);
    }
}