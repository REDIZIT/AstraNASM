namespace Astra.Compilation;

public abstract class Node
{
    public Variable result;
    public Scope scope;

    public abstract void RegisterRefs(RawModule module);
    public abstract void ResolveRefs(ResolvedModule resolved);

    public virtual void Generate(Generator.Context ctx)
    {
    }

    public abstract IEnumerable<Node> EnumerateChildren();
}