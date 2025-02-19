
namespace Astra.Compilation;

public class Node_FunctionCall : Node
{
    public Node caller;
    public List<Node> arguments;
    public string functionName;

    public FunctionInfo function;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return caller;

        foreach (Node arg in arguments)
        {
            yield return arg;
        }
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        if (function is EmbeddedFunctionInfo embeddedFunctionInfo)
        {
            GenerateEmbedded(ctx, embeddedFunctionInfo);
        }
        else
        {
            GenerateRegular(ctx);
        }
    }

    private void GenerateRegular(Generator.Context ctx)
    {
        ctx.gen.Space();
        ctx.gen.Comment($"{caller}.{function.name}()");




        ctx.gen.Comment($"arguments generation");

        bool isStatic = function.isStatic;

        Variable[] argumentsResults = new Variable[isStatic ? arguments.Count : arguments.Count + 1];

        for (int i = 0; i < arguments.Count; i++)
        {
            Node node = arguments[i];
            
            if (node is Node_Literal lit)
            {
                ctx.gen.Comment("skip literal = " + lit.constant.value);
                continue;
            }

            node.Generate(ctx);
            argumentsResults[i + (isStatic ? 0 : 1)] = node.result;
        }


        if (function.returns.Count > 0)
        {
            if (function.returns.Count > 1) throw new Exception("Not supported yet");

            result = ctx.gen.Allocate(function.returns[0]);
            
            // ctx.gen.Deallocate(result);
        }


        ctx.gen.Comment($"arguments pushing");

        Variable selfVar = null;

        if (isStatic == false)
        {
            Node_VariableUse variableNode = (Node_VariableUse)((Node_FieldAccess)caller).target;
            selfVar = variableNode.result;
            
            ctx.gen.PushToStack(selfVar, "self");

            argumentsResults[0] = selfVar;
        }

        for (int i = 0; i < arguments.Count; i++)
        {
            Variable result = argumentsResults[isStatic ? 0 : 1 + i];
            FieldInfo argInfo = function.arguments[i];

            if (arguments[i] is Node_Literal literal)
            {
                ctx.gen.PushToStack(literal.constant.value, argInfo.type, $"arg[{i}] = {argInfo.name}");
            }
            else
            {
                ctx.gen.PushToStack(result, $"arg[{i}] = {argInfo.name}");
            }
        }



        ctx.gen.Call(function.GetCombinedName());
        
        for (int i = 0; i < argumentsResults.Length; i++)
        {
            Variable argument = argumentsResults[i];
            if (argument == null) continue;
            
            ctx.gen.Deallocate(argument);
        }
    }



    private void GenerateEmbedded(Generator.Context ctx, EmbeddedFunctionInfo embeddedFunctionInfo)
    {
        if (embeddedFunctionInfo is ToPtr_EmbeddedFunctionInfo toPtr)
        {
            Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
            result = toPtr.Generate(ctx, variable.result);
        }
        else if (embeddedFunctionInfo is PtrSet_EmbeddedFunctionInfo ptrSet)
        {
            Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
            arguments[0].Generate(ctx);
            result = ptrSet.Generate(ctx, variable.result, arguments[0].result);
        }
        else if (embeddedFunctionInfo is PtrGet_EmbeddedFunctionInfo ptrGet)
        {
            Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
            result = ptrGet.Generate(ctx, variable.result);
        }
        else if (embeddedFunctionInfo is PtrShift_EmbeddedFunctionInfo ptrShift)
        {
            Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
            arguments[0].Generate(ctx);
            result = ptrShift.Generate(ctx, variable.result, arguments[0].result);
        }
        else if (embeddedFunctionInfo is Print_EmbeddedFunctionInfo print)
        {
            Node variable = arguments[0];
            variable.Generate(ctx);

            print.Generate(ctx, variable.result);
        }
        else if (embeddedFunctionInfo is StringGet_EmbeddedFunctionInfo stringGet)
        {
            Node_VariableUse variableNode = (Node_VariableUse)((Node_FieldAccess)caller).target;
            Variable stringVariable = variableNode.result;
            
            Node indexNode = arguments[0];
            indexNode.Generate(ctx);
            Variable indexVariable = indexNode.result;
            
            result = stringGet.Generate(ctx, stringVariable, indexVariable);
        }
        else if (embeddedFunctionInfo is StringLength_EmbeddedFunctionInfo stringLength)
        {
            Node_VariableUse variableNode = (Node_VariableUse)((Node_FieldAccess)caller).target;
            Variable stringVariable = variableNode.result;

            result = stringLength.Generate(ctx, stringVariable);
        }
        else if (embeddedFunctionInfo is Alloc_EmbeddedFunctionInfo alloc)
        {
            Node bytesCountNode = arguments[0];
            bytesCountNode.Generate(ctx);
            Variable bytesCountVariable = bytesCountNode.result;

            result = alloc.Generate(ctx, bytesCountVariable);
        }
        else
        {
            throw new Exception($"Unknown EmbeddedFunctionInfo '{embeddedFunctionInfo}'");
        }
    }
}