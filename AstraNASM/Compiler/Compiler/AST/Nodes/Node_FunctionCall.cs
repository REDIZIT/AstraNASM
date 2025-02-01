﻿
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

        bool isStatic = function.owner == null;

        Variable[] argumentsResults = new Variable[isStatic ? arguments.Count : arguments.Count + 1];

        for (int i = 0; i < arguments.Count; i++)
        {
            Node node = arguments[i];
            // if (node is Node_Literal) continue;

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

            selfVar = ctx.gen.GetVariable(variableNode.variableName);
            // ctx.AllocateStackVariable(selfVar.type);
            
            ctx.gen.PushToStack(selfVar, "self");

            argumentsResults[0] = selfVar;
        }

        for (int i = 0; i < arguments.Count; i++)
        {
            Variable result = argumentsResults[isStatic ? 0 : 1];
            FieldInfo argInfo = function.arguments[i];

            if (arguments[i] is Node_Literal literal)
            {
                // ctx.b.Line($"mov rbx, {literal.constant.value} ; arg[{i}] = {argInfo.name}");
                ctx.gen.PushToStack(literal.constant.value, $"arg[{i}] = {argInfo.name}");
            }
            else
            {
                // ctx.AllocateStackVariable(result.type, argInfo.name);
                // ctx.b.Line($"mov rbx, {result.GetRBP()} ; arg[{i}] = {argInfo.name}");
                ctx.gen.PushToStack(result, $"arg[{i}] = {argInfo.name}");
            }

            // ctx.b.Line($"push rbx");
        }



        ctx.gen.Call(function.name);



        int argumentsSizeInBytes = 0;
        for (int i = argumentsResults.Length - 1; i >= 0; i--)
        {
            Variable arg = argumentsResults[i];

            if (arg != selfVar)
            {
                ctx.gen.Unregister_FunctionArgumentVariable(arg);
            }
            // else
            // {
            //     ctx.gen.Deallocate(arg);
            // }

            argumentsSizeInBytes += 8;
        }
        
        // ctx.b.Line($"add rsp, {argumentsSizeInBytes}");
        ctx.gen.Deallocate(argumentsSizeInBytes);
    }



    private void GenerateEmbedded(Generator.Context ctx, EmbeddedFunctionInfo embeddedFunctionInfo)
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
}