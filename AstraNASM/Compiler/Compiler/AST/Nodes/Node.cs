namespace Astra.Compilation;

public abstract class Node
{
    public Variable result;
    public Scope_StaticAnalysis scope;

    public List<Token> consumedTokens;


    public virtual void Generate(Generator.Context ctx)
    {
    }

    public abstract IEnumerable<Node> EnumerateChildren();
}