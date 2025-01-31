
namespace Astra.Compilation;

public class Node_New : Node
{
    public string className;

    public ClassTypeInfo classInfo;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield break;
    }

    public override void Generate(Generator.Context ctx)
    {
        result = ctx.AllocateStackVariable(PrimitiveTypes.LONG, "address");
        int typeSizeInBytes = 8;

        ctx.b.Space();
        ctx.b.CommentLine($"new {classInfo.name}");
        ctx.b.Line($"sub rsp, {typeSizeInBytes}");


        // If ref type
        if (classInfo.isStruct == false)
        {
            ctx.b.CommentLine($"heap alloc");
            ctx.b.Line($"mov {result.RBP}, 0x110"); // result.RBP - pointer to object table, 0x110 - pointer to real data
            ctx.b.Line($"mov rbx, [0x100]");
            ctx.b.Line($"add rbx, 1");
            ctx.b.Line($"mov [0x100], rbx");
        }
    }
}