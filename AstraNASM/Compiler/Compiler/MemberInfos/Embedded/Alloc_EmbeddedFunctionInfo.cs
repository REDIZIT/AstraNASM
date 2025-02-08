namespace Astra.Compilation;

public class Alloc_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public Variable Generate(Generator.Context ctx, Variable bytesToAllocateVariable)
    {
        ctx.gen.Space();
        
        Variable pointerOnStack = ctx.gen.Allocate(PrimitiveTypes.PTR);
        
        ctx.gen.AllocateHeap(pointerOnStack, bytesToAllocateVariable);

        return pointerOnStack;
    }
}