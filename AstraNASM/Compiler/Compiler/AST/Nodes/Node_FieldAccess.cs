
namespace Astra.Compilation;

public class Node_FieldAccess : Node
{
    public Node target;
    public string targetFieldName;


    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return target;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        target.Generate(ctx);

        int fieldOffsetInBytes = 0;

        result = ctx.AllocateStackVariable(PrimitiveTypeInfo.PTR);

        int totalOffset = target.result.rbpOffset + fieldOffsetInBytes;
       

        ctx.b.Space();
        ctx.b.CommentLine($"{target.result.name}.{targetFieldName}");
        ctx.b.Line($"sub rsp, 8");
        //ctx.b.Line($"mov rax, rbp");
        //ctx.b.Line($"add rax, {}");

        if (target is Node_FieldAccess)
        {
            ctx.b.Line($"mov rax, [rbp{totalOffset}]");
        }
        else
        {
            ctx.b.Line($"mov rax, rbp");
            ctx.b.Line($"add rax, {totalOffset}");
        }

        ctx.b.Line($"mov {result.GetRBP()}, rax");
    }
}