public class Node_Binary : Node
{
    public Node left, right;
    public Token_Operator @operator;

    public override void RegisterRefs(RawModule module)
    {
        left.RegisterRefs(module);
        right.RegisterRefs(module);
    }
    public override void ResolveRefs(ResolvedModule module)
    {
        left.ResolveRefs(module);
        right.ResolveRefs(module);
    }
    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        left.Generate(ctx);
        right.Generate(ctx);

        string leftName = left.generatedVariableName;
        string rightName = right.generatedVariableName;

        TypeInfo resultType = ctx.module.GetType(@operator.ResultType);

        generatedVariableName = ctx.AllocateStackVariable(resultType);

        ctx.b.Line($"mov rax, {leftName}");
        ctx.b.Line($"mov rbx, {rightName}");
        ctx.b.Line($"{@operator.asmOperatorName} rax, rbx");
        ctx.b.Line($"mov {generatedVariableName}, rax");

        ctx.b.Space();
    }
}

public class Node_Unary : Node
{
    public Node right;
    public Token_Operator @operator;

    public override void RegisterRefs(RawModule module)
    {
        right.RegisterRefs(module);
    }
    public override void ResolveRefs(ResolvedModule module)
    {
        right.ResolveRefs(module);
    }
    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        right.Generate(ctx);

        string rightName = Utils.SureNotPointer(right.generatedVariableName, ctx);


        // Logical not
        if (@operator.asmOperatorName == "not")
        {
            TypeInfo rightType = ctx.GetVariableType(rightName);
            string tempName = ctx.NextTempVariableName(PrimitiveTypeInfo.BOOL);
            ctx.b.Line($"{tempName} = icmp sle {rightType} {rightName}, 0");

            generatedVariableName = tempName;
        }
        else if (@operator.asmOperatorName == "sub")
        {
            TypeInfo rightType = ctx.GetVariableType(rightName);
            string tempName = ctx.NextTempVariableName(rightType);
            ctx.b.Line($"{tempName} = sub {rightType} 0, {rightName}");

            generatedVariableName = tempName;
        }

        ctx.b.Space();
    }
}