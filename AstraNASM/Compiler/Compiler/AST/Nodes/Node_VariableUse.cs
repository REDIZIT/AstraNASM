
namespace Astra.Compilation;

public class Node_VariableUse : Node
{
    public string variableName;
    public TypeInfo type;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield break;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        result = ctx.gen.currentScope.GetVariable(variableName);
    }
}