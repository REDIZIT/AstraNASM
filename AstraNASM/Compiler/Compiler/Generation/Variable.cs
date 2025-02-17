namespace Astra.Compilation;

public class Variable
{
    public string name;
    public TypeInfo type;
    public int rbpOffset;
    public readonly Scope scope;

    public Variable(Scope scope)
    {
        this.scope = scope;
    }

    public string RBP
    {
        get
        {
            if (rbpOffset > 0) return $"[rbp+{rbpOffset}]";
            else return $"[rbp{rbpOffset}]";
        }
    }
}