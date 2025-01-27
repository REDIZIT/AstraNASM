public class ToPtr_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public Variable Generate(Generator.Context ctx, string valueName)
    {
        Variable result = ctx.AllocateStackVariable(PrimitiveTypeInfo.PTR);
        Variable valueVariable = ctx.GetVariable(valueName);

        ctx.b.Line($"mov rax, rbp");
        ctx.b.Line($"add rax, {valueVariable.rbpOffset}");
        ctx.b.Line($"mov {result.GetRBP()}, rax");

        return result;
    }
}
