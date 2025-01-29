namespace Astra.Compilation
{
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


            //Node_FunctionCall heapCall = new Node_FunctionCall()
            //{
            //    functionName = "heapAlloc",

            //};

            ctx.b.CommentLine($"heap alloc");
            ctx.b.Line($"mov {result.RBP}, 0x110"); // result.RBP - pointer to object table, 0x110 - pointer to real data
            ctx.b.Line($"mov rax, [0x100]");
            ctx.b.Line($"add rax, 1");
            ctx.b.Line($"mov [0x100], rax");
        }
    }
}