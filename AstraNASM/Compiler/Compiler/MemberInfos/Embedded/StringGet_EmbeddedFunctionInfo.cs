namespace Astra.Compilation;

public class StringGet_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public Variable Generate(Generator.Context ctx, Variable stringVariable, Variable indexVariabe)
    {
        Variable ptr = ctx.gen.Allocate(PrimitiveTypes.PTR);
        ctx.gen.ToPtr_Heap(stringVariable, ptr);
        
        ctx.gen.PtrShift(ptr, indexVariabe, 4);

        Variable result = ctx.gen.Allocate(PrimitiveTypes.INT);
        ctx.gen.PtrGet(ptr, result);

        return result;
    }
}