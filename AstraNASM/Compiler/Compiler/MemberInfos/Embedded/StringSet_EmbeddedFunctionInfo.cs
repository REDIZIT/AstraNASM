namespace Astra.Compilation;

public class StringSet_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public void Generate(Generator.Context ctx, Variable stringVariable, Variable indexVariabe, Variable charVariabe)
    {
        if (charVariabe.type != PrimitiveTypes.BYTE)
        {
            throw new Exception($"Failed to genereate StringSet function: Expected value type '{PrimitiveTypes.BYTE.name}' but got '{charVariabe.type.name}'");
        }
        
        Variable ptr = ctx.gen.Allocate(PrimitiveTypes.PTR);
        ctx.gen.ToPtr_Heap(stringVariable, ptr);
        
        ctx.gen.PtrShift(ptr, indexVariabe, 4);
        
        ctx.gen.PtrSet(ptr, charVariabe);
    }
}