namespace Astra.Compilation;

public class ToPtr_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public Variable Generate(Generator.Context ctx, Variable valueVariable)
    {
        Variable result = ctx.gen.Allocate(PrimitiveTypes.PTR);

        if (valueVariable.type is PrimitiveTypes)
        {
            ctx.gen.ToPtr_Primitive(valueVariable, result);
        }
        else
        {
            ctx.gen.ToPtr_Heap(valueVariable, result);
        }

        return result;
    }
}