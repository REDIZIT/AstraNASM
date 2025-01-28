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
            ctx.b.CommentLine($"{caller}.{function.name}()");


            ctx.b.CommentLine($"arguments generation");

            Variable[] argumentsResults = new Variable[arguments.Count];

            for (int i = 0; i < arguments.Count; i++)
            {
                Node node = arguments[i];

                node.Generate(ctx);

                argumentsResults[i] = node.result;                
            }

            ctx.b.CommentLine($"arguments pushing");

            if (function.owner != null)
            {
                Node_VariableUse variableNode = (Node_VariableUse)((Node_FieldAccess)caller).target;

                Variable variable = ctx.GetVariable(variableNode.variableName);
                ctx.AllocateStackVariable(variable.type);

                ctx.b.Line($"mov rax, {variable.RBP} ; self");
                ctx.b.Line($"push rax");
            }

            for (int i = 0; i < arguments.Count; i++)
            {
                Variable result = argumentsResults[i];
                FieldInfo argInfo = function.arguments[i];

                ctx.AllocateStackVariable(result.type, argInfo.name);
                ctx.b.Line($"mov rax, {result.GetRBP()} ; arg[{i}] = {argInfo.name}");
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