namespace Astra.Compilation;

public class PtrGet_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public Variable Generate(Generator.Context ctx, string pointerVariableName)
    {
        ctx.gen.Space();
        ctx.gen.Comment($"Get value from {pointerVariableName}");


        var pointerVar = ctx.gen.GetVariable(pointerVariableName);

        TypeInfo pointedType = PrimitiveTypes.LONG; // TODO

        var result = ctx.gen.Allocate(pointedType);

        ctx.gen.PtrGet(pointerVar, result);

        ctx.gen.Space();

        return result;
    }
}