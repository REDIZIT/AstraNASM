public class Node_FunctionCall : Node
{
    public Node caller;
    public List<Node> arguments;
    public string functionName;

    public FunctionInfo function;

    public override void RegisterRefs(RawModule module)
    {
    }
    public override void ResolveRefs(ResolvedModule module)
    {
        function = module.functionInfoByName[functionName];
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        if (function is EmbeddedFunctionInfo embeddedFunctionInfo)
        {
            if (embeddedFunctionInfo is ToPtr_EmbeddedFunctionInfo toPtr)
            {
                Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
                string valueVariableName = variable.variableName;
                result = toPtr.Generate(ctx, valueVariableName);
            }
            else if (embeddedFunctionInfo is PtrSet_EmbeddedFunctionInfo ptrSet)
            {
                Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
                string pointerVariableName = variable.variableName;
                arguments[0].Generate(ctx);
                result = ptrSet.Generate(ctx, pointerVariableName, arguments[0].result);
            }
            else if (embeddedFunctionInfo is PtrGet_EmbeddedFunctionInfo ptrGet)
            {
                Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
                string pointerVariableName = variable.variableName;
                result = ptrGet.Generate(ctx, pointerVariableName);
            }
            else if (embeddedFunctionInfo is PtrShift_EmbeddedFunctionInfo ptrShift)
            {
                Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
                string pointerVariableName = variable.variableName;
                arguments[0].Generate(ctx);
                result = ptrShift.Generate(ctx, pointerVariableName, arguments[0].result);
            }
            else if (embeddedFunctionInfo is Print_EmbeddedFunctionInfo print)
            {
                Node variable = arguments[0];
                variable.Generate(ctx);

                print.Generate(ctx, variable.result);
            }
            else
            {
                throw new Exception($"Unknown EmbeddedFunctionInfo '{embeddedFunctionInfo}'");
            }
        }
        else
        {
            ctx.b.Space();
            ctx.b.CommentLine($"{caller}.{function.name}");

            //var functionCtx = ctx.CreateSubContext();

            for (int i = 0; i < arguments.Count; i++)
            {
                Node node = arguments[i];
                FieldInfo argInfo = function.arguments[0];

                node.Generate(ctx);

                ctx.AllocateStackVariable(node.result.type, argInfo.name);
                ctx.b.Line($"mov rax, {node.result.GetRBP()}");
                ctx.b.Line($"push rax");
            }


            if (function.returns.Count > 0)
            {
                throw new Exception("Not upgraded");
                //TypeInfo returnValueType = function.returns[0];

                //string tempName = ctx.NextTempVariableName(returnValueType);
                //ctx.b.Line($"{tempName} = call {returnValueType} @{functionName}({paramsStr})");
                //generatedVariableName = tempName;
            }
            else
            {
                //ctx.b.Line($"call void @{functionName}({paramsStr})");
                ctx.b.Line($"call {function.name}");
            }
        }
    }
}