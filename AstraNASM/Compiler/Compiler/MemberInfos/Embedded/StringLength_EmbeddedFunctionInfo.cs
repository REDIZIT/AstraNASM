namespace Astra.Compilation;

public class StringLength_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public Variable Generate(Generator.Context ctx, Variable stringVariable)
    {
        ctx.gen.Space();
        ctx.gen.Comment($"String get length {stringVariable.name}");

        Variable ptr = ctx.gen.Allocate(PrimitiveTypes.PTR);
        ctx.gen.ToPtr_Heap(stringVariable, ptr);

        Variable result = ctx.gen.Allocate(PrimitiveTypes.BYTE);
        ctx.gen.PtrGet(ptr, result);

        return result;
    }
}