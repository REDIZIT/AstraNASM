namespace Astra.Compilation;

public abstract class PtrGet_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    protected abstract TypeInfo GetReturnType();
    
    public Variable Generate(Generator.Context ctx, string pointerVariableName)
    {
        ctx.gen.Space();
        ctx.gen.Comment($"Get value from {pointerVariableName}");


        var pointerVar = ctx.gen.GetVariable(pointerVariableName);

        TypeInfo pointedType = GetReturnType();

        var result = ctx.gen.Allocate(pointedType);

        ctx.gen.PtrGet(pointerVar, result);

        ctx.gen.Space();

        return result;
    }
}

public class PtrGet_Byte : PtrGet_EmbeddedFunctionInfo
{
    protected override TypeInfo GetReturnType() => PrimitiveTypes.BYTE;
}

public class PtrGet_Short : PtrGet_EmbeddedFunctionInfo
{
    protected override TypeInfo GetReturnType() => PrimitiveTypes.SHORT;
}

public class PtrGet_Int : PtrGet_EmbeddedFunctionInfo
{
    protected override TypeInfo GetReturnType() => PrimitiveTypes.INT;
}

public class PtrGet_Long : PtrGet_EmbeddedFunctionInfo
{
    protected override TypeInfo GetReturnType() => PrimitiveTypes.LONG;
}