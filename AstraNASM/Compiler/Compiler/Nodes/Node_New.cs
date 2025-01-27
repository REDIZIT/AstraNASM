public class Node_New : Node
{
    public string className;

    public ClassTypeInfo classInfo;


    public override void RegisterRefs(RawModule module)
    {
        
    }
    public override void ResolveRefs(ResolvedModule resolved)
    {
        classInfo = resolved.classInfoByName[className];
    }
    public override void Generate(Generator.Context ctx)
    {
        result = ctx.AllocateStackVariable(PrimitiveTypeInfo.LONG, "address");
        int typeSizeInBytes = 8;

        ctx.b.Space();
        ctx.b.CommentLine($"new {classInfo.name}");
        ctx.b.Line($"sub rsp, {typeSizeInBytes}");
    }
}