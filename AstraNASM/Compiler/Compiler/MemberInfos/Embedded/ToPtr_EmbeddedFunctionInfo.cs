namespace Astra.Compilation;

public class ToPtr_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public Variable Generate(Generator.Context ctx, string valueName)
    {
        Variable result = ctx.AllocateStackVariable(PrimitiveTypeInfo.PTR);
        Variable valueVariable = ctx.GetVariable(valueName);

        if (valueVariable.type is PrimitiveTypeInfo)
        {
            ctx.b.CommentLine($"ToPtr {valueName}");
            ctx.b.Line($"mov rbx, rbp");
            ctx.b.Line($"add rbx, {valueVariable.rbpOffset}");
            ctx.b.Line($"mov {result.GetRBP()}, rbx");
        }
        else
        {
            ctx.b.CommentLine($"ToPtr {valueName} (heap data)");
            ctx.b.Line($"mov rbx, rbp");
            ctx.b.Line($"add rbx, {valueVariable.rbpOffset}");
            ctx.b.Line($"mov {result.GetRBP()}, [rbx]");
        }

        return result;
    }
}
