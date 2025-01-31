
namespace Astra.Compilation;

public class Node_Class : Node
{
    public string name;
    public Node_Block body;

    public ClassTypeInfo classInfo;

    public override IEnumerable<Node> EnumerateChildren()
    {
        yield return body;
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        foreach (Node statement in body.children)
        {
            if (statement is Node_Function)
            {
                statement.Generate(ctx);
            }
            else if (statement is Node_VariableDeclaration == false)
            {
                throw new Exception($"For class generation expected only {nameof(Node_Function)} or {nameof(Node_VariableDeclaration)} but got {statement}");
            }
        }
    }
}
