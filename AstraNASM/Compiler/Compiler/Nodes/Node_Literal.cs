public class Node_Literal : Node
{
    public Token_Constant constant;

    public override void RegisterRefs(RawModule module)
    {
    }
    public override void ResolveRefs(ResolvedModule module)
    {
    }
    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        PrimitiveTypeInfo literalType = PrimitiveTypeInfo.INT;

        //generatedVariableName = ctx.NextPointerVariableName(literalType);
        generatedVariableName = ctx.AllocateStackVariable(literalType, "literal");

        //ctx.b.Line($"{generatedVariableName} = alloca {literalType.asmName}");
        //ctx.b.Line($"store {literalType.asmName} {constant.value}, {PrimitiveTypeInfo.PTR} {generatedVariableName}");
        ctx.b.Line($"sub rsp, 8");
        ctx.b.Line($"mov [rbp-8], 42");

        ctx.b.Space();
    }
}
