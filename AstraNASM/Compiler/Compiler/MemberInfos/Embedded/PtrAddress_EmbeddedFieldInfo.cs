using Astra.Compilation;

public class EmbeddedFieldInfo : FieldInfo
{
}
public class PtrAddress_EmbeddedFieldInfo : EmbeddedFieldInfo
{
    public void Generate(Generator.Context ctx, Variable variable)
    {
        ctx.b.Space();
        ctx.b.CommentLine($"print {variable.name}");
        ctx.b.Line($"mov qword rbx, {variable.GetRBP()}");
        ctx.b.Line($"print [rbx]");
    }
}