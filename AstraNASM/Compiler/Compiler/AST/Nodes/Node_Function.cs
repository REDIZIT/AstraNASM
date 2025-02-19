
namespace Astra.Compilation;

public class Node_Function : Node
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
        
        ctx.gen.BeginSubScope(staticScope);
        body.Generate(ctx);
        ctx.gen.EndSubScope();
    }
}