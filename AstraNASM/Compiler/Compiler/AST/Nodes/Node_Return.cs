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

                
                if (CanReturn(function.returns[0], expr.result.type) == false)
                {
                    throw new Exception($"Failed to generate return node: function returns {function.returns[0]}, but keyword return try to return {expr.result.type}");
                }

                ctx.gen.Space();
                ctx.gen.Out_Variable(function, expr.result);
            }
            else
            {
                throw new Exception("Failed to generate return node: Function has return type, but return node does not have any expression to return.");
            }
        }
        
        ctx.gen.Epilogue();
        ctx.gen.Return();
    }

    private bool CanReturn(TypeInfo declaratedType, TypeInfo retType)
    {
        bool isDeclPtrOtNonPrimitive = declaratedType == PrimitiveTypes.PTR || !PrimitiveTypes.IsPrimitive(declaratedType);
        bool isRetPtrOtNonPrimitive = retType == PrimitiveTypes.PTR || !PrimitiveTypes.IsPrimitive(retType);

        if (isDeclPtrOtNonPrimitive && isRetPtrOtNonPrimitive)
        {
            return true;
        }
        else
        {
            return declaratedType == retType;
        }
    }
}