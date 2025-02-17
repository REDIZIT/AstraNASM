
namespace Astra.Compilation;

public class Node_New : Node
{
    public string className;

    public TypeInfo classInfo;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield break;
    }

    public override void Generate(Generator.Context ctx)
    {
        // If ref type
        if (classInfo.isStruct)
        {
            throw new Exception("Can not use 'new' for value-types");
        }
        
        // Allocate pointer to heap
        result = ctx.gen.Allocate(PrimitiveTypes.PTR);

        ctx.gen.Space();
        ctx.gen.Comment($"new {classInfo.name}");
        ctx.gen.Allocate(classInfo);
        
        ctx.gen.AllocateHeap(result, 16); // TODO: Calculate bytesToAllocate
    }
}