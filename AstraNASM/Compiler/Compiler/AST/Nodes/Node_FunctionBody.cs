
namespace Astra.Compilation;

public class Node_FunctionBody : Node
{
    public string name;
    public Node body;
    public List<VariableRawData> parameters = new();
    public List<VariableRawData> returnValues = new();
    public bool isStatic;

    public FunctionInfo functionInfo;

    public Scope_StaticAnalysis staticScope;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return body;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        if (returnValues.Count > 1)
        {
            throw new NotImplementedException("Function has 1+ return values. Generation for this is not supported yet");
        }

        string functionLabel = ctx.gen.RegisterLabel(functionInfo.GetCombinedName());
        ctx.gen.Label(functionLabel);
        
        
        
        // Register arguments and returns before creating function body subscope
        // Registered arguments will have negative RBP offset due to body subscope and current scope are different

        // We promise/provide named arguments inside function body
        
        Stack<Variable> pushedVariables = new();

        if (functionInfo.isStatic == false)
        {
            Variable pushedVariable = ctx.gen.currentScope.RegisterLocalVariable(PrimitiveTypes.PTR, "self");
            pushedVariables.Push(pushedVariable);
        }
        foreach (FieldInfo arg in functionInfo.arguments)
        {
            Variable pushedVariable = ctx.gen.currentScope.RegisterLocalVariable(arg.type, arg.name);
            pushedVariables.Push(pushedVariable);
        }


        Variable callPushed = ctx.gen.currentScope.RegisterLocalVariable(PrimitiveTypes.PTR, "call_pushed_instruction");
        
        
        // Creating function body subscope (all arguments, returns genereted not in sub scope, but in current scope)
        ctx.gen.BeginSubScope();

        ctx.gen.BindFunction(functionInfo);
        body.Generate(ctx);
        
        ctx.gen.DropSubScope();
        
        
        // Delete our promises/provided named arguments, because they will not be accessable outside the function body
        ctx.gen.currentScope.UnregisterLocalVariable(callPushed);
        foreach (Variable var in pushedVariables)
        {
            ctx.gen.currentScope.UnregisterLocalVariable(var);
        }
    }
}