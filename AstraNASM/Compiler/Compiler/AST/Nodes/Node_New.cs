
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
        result = ctx.gen.Allocate(PrimitiveTypes.LONG);
        int typeSizeInBytes = 8;

        ctx.gen.Space();
        ctx.gen.Comment($"new {classInfo.name}");
        ctx.gen.Allocate(classInfo);


        // If ref type
        if (classInfo.isStruct == false)
        {
            ctx.gen.AllocateHeap(result);
        }
    }
}