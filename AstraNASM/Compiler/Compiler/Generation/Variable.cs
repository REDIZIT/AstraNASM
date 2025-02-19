using AVM.Compiler;

namespace Astra.Compilation;

public class Variable
{
    public string name;
    public TypeInfo type;
    public int inscopeRbpOffset;
    
    public readonly Scope_GenerationPhase scope;

    public Variable(Scope_GenerationPhase scope)
    {
        this.scope = scope;
    }
}