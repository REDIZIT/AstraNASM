﻿public class Node_Literal : Node
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

        generatedVariableName = ctx.AllocateStackVariable(literalType);

        ctx.b.Line($"sub rsp, 8");
        ctx.b.Line($"mov {generatedVariableName}, {constant.value}");

        ctx.b.Space();
    }
}
