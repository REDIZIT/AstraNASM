namespace Astra.Compilation;

public class Node_FieldAccess : Node
{
    public Node target;
    public string targetFieldName;

    public FieldInfo field;


    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return target;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        target.Generate(ctx);

        if (field is EmbeddedFieldInfo)
        {
            if (field is PtrAddress_EmbeddedFieldInfo ptrAddress)
            {
                result = ctx.AllocateStackVariable(PrimitiveTypeInfo.PTR);

                ctx.b.Space();
                ctx.b.CommentLine($"{target.result.name}.{targetFieldName}");
                ctx.b.Line($"sub rsp, 8");
                ctx.b.Line($"mov rbx, rbp");
                ctx.b.Line($"add rbx, {target.result.rbpOffset} ; offset to target ptr data cell");
                ctx.b.Line($"mov {result.GetRBP()}, rbx ; now {result.GetRBP()} is pointer to {target.result.name} (.address)");
            }
            else
            {
                throw new Exception($"Failed to generate unknown EmbeddedFieldInfo '{field}'");
            }
        }
        else
        {
            int fieldOffsetInBytes = 0;

            result = ctx.AllocateStackVariable(PrimitiveTypeInfo.PTR);

            int totalOffset = target.result.rbpOffset + fieldOffsetInBytes;


            ctx.b.Space();
            ctx.b.CommentLine($"{target.result.name}.{targetFieldName}");
            ctx.b.Line($"sub rsp, 8");

            if (target is Node_FieldAccess)
            {
                ctx.b.Line($"mov rbx, [rbp{totalOffset}]");
            }
            else
            {
                ctx.b.Line($"mov rbx, rbp");
                ctx.b.Line($"add rbx, {totalOffset}");
            }

            ctx.b.Line($"mov {result.GetRBP()}, rbx");
        }
    }
}