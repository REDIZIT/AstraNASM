
using AVM;

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

        for (int i = 0; i < arguments.Count; i++)
        {
            Node node = arguments[i];
            
            if (node is Node_Literal lit)
            {
                ctx.gen.Comment("skip literal = " + lit.constant.value);
                continue;
            }

            node.Generate(ctx);
        }

        

        if (function.returns.Count > 0)
        {
            if (function.returns.Count > 1) throw new Exception("Not supported yet");

            result = ctx.gen.Allocate(function.returns[0]);
        }


        ctx.gen.Comment($"arguments pushing");

        // Collect info about pushed arguments and write byte-code for pushing
        // This info is required to write byte-code for deallocation
        
        // Here, we DO NOT PROMISE/PROVIDE any arguments
        // Here, we only write byte-code for pushing these arguments

        int bytesAllocated = 0;

        if (function.isStatic == false)
        {
            Node_VariableUse variableNode = (Node_VariableUse)((Node_FieldAccess)caller).target;
            variableNode.Generate(ctx);
            ctx.gen.PushToStack(variableNode.result);

            bytesAllocated += variableNode.result.type.refSizeInBytes;
        }

        for (int i = 0; i < arguments.Count; i++)
        {
            Node argument = arguments[i];
            FieldInfo argumentInfo = function.arguments[i];
            
            TypeInfo passedType;

            if (argument is Node_Literal literal)
            {
                passedType = PrimitiveTypes.INT;
                ctx.gen.PushToStack(literal.constant.value, passedType);
            }
            else
            {
                ctx.gen.PushToStack(argument.result);
                passedType = argument.result.type;
            }

            if (passedType != argumentInfo.type)
            {
                throw new Exception($"Inside function call passed argument with type {passedType.name} but expected {argumentInfo.type}");
            }

            bytesAllocated += passedType.refSizeInBytes;
        }


        ctx.gen.Call(function.GetCombinedName());
        
        
        // Write byte-code for deallocation of pushed arguments
        // Here, we DO NOT DEALLOCATE variables, but bytes because
        // we DID NOT give any PROMISES/PROVIDED variables, but only write byte-code
        ctx.gen.Deallocate(bytesAllocated);
    }



    private void GenerateEmbedded(Generator.Context ctx, EmbeddedFunctionInfo embeddedFunctionInfo)
    {
        if (embeddedFunctionInfo is ToPtr_EmbeddedFunctionInfo toPtr)
        {
            Variable variable = GetVariable(ctx);
            result = toPtr.Generate(ctx, variable);
        }
        else if (embeddedFunctionInfo is PtrSet_EmbeddedFunctionInfo ptrSet)
        {
            Variable variable = GetVariable(ctx);
            arguments[0].Generate(ctx);
            result = ptrSet.Generate(ctx, variable, arguments[0].result);
        }
        else if (embeddedFunctionInfo is PtrGet_EmbeddedFunctionInfo ptrGet)
        {
            Variable variable = GetVariable(ctx);
            result = ptrGet.Generate(ctx, variable);
        }
        else if (embeddedFunctionInfo is PtrShift_EmbeddedFunctionInfo ptrShift)
        {
            Variable variable = GetVariable(ctx);
            
            arguments[0].Generate(ctx);
            result = ptrShift.Generate(ctx, variable, arguments[0].result);
        }
        else if (embeddedFunctionInfo is Print_EmbeddedFunctionInfo print)
        {
            Node variable = arguments[0];
            variable.Generate(ctx);

            print.Generate(ctx, variable.result);
        }
        else if (embeddedFunctionInfo is StringGet_EmbeddedFunctionInfo stringGet)
        {
            Variable stringVariable = GetVariable(ctx);
            
            Node indexNode = arguments[0];
            indexNode.Generate(ctx);
            Variable indexVariable = indexNode.result;
            
            result = stringGet.Generate(ctx, stringVariable, indexVariable);
        }
        else if (embeddedFunctionInfo is StringLength_EmbeddedFunctionInfo stringLength)
        {
            Variable stringVariable = GetVariable(ctx);

            result = stringLength.Generate(ctx, stringVariable);
        }
        else if (embeddedFunctionInfo is Alloc_EmbeddedFunctionInfo alloc)
        {
            Node bytesCountNode = arguments[0];
            bytesCountNode.Generate(ctx);
            Variable bytesCountVariable = bytesCountNode.result;

            result = alloc.Generate(ctx, bytesCountVariable);
        }
        else if (embeddedFunctionInfo is Command_EmbeddedFunctionInfo cmdInfo)
        {
            Node_Literal cmdLit = (Node_Literal)arguments[0];

            string str = ctx.module.data.stringByAddress[int.Parse(cmdLit.constant.value)];
            VMCommand_Cmd cmd = Enum.Parse<VMCommand_Cmd>(str);
            
            foreach (Node node in arguments.Skip(1))
            {
                node.Generate(ctx);
            }

            result = cmdInfo.Generate(ctx, cmd, arguments.Skip(1).Select(n => n.result).ToArray());
        }
        else
        {
            throw new Exception($"Unknown EmbeddedFunctionInfo '{embeddedFunctionInfo}'");
        }
    }

    private Variable GetVariable(Generator.Context ctx)
    {
        Node_VariableUse variableNode = (Node_VariableUse)((Node_FieldAccess)caller).target;
        variableNode.Generate(ctx);
        return variableNode.result;
    }
}