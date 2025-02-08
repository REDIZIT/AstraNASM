namespace Astra.Compilation;

public class Scope
{
    public List<FieldInfo> variables = new();
    public TypeInfo typeInfo;
    public FunctionInfo functionInfo;

    public Scope parent;
    public List<Scope> children = new();

    public Scope CreateSubScope()
    {
        Scope child = new();
        child.parent = this;
        children.Add(child);

        return child;
    }

    public Scope Find(Func<Scope, bool> predicate)
    {
        if (predicate(this)) return this;
        if (parent == null) throw new Exception("Failed to find scope for predicate");
        return parent.Find(predicate);
    }

    public FieldInfo FindVariable(string variableName)
    {
        FieldInfo variable = variables.FirstOrDefault(f => f.name == variableName);
        if (variable != null) return variable;
        if (parent == null) throw new Exception($"Failed to find variable '{variableName}' in scopes");
        return parent.FindVariable(variableName);
    }
}