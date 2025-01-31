namespace Astra.Compilation;

public class Node_VariableAssign : Node
{
    public Node target; // variable or field get
    public Node value;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return target;
        yield return value;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        target.Generate(ctx);
        value.Generate(ctx);

        ctx.b.Space();
        ctx.b.CommentLine($"{target.result.name} = {value.result.name}");

        if (target is Node_FieldAccess)
        {
            ctx.b.Line($"mov rbx, {target.result.RBP}");
            ctx.b.Line($"mov rdx, {value.result.RBP}");
            ctx.b.Line($"mov qword [rbx], rdx");
        }
        else
        {
            ctx.b.Line($"mov qword rbx, {value.result.RBP}");
            ctx.b.Line($"mov qword {target.result.RBP}, rbx");
        }
    }
}