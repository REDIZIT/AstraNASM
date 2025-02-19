namespace Astra.Compilation;

public class Scope_GenerationPhase
{
    public Scope_StaticAnalysis staticScope;
    
    public Dictionary<string, Variable> variableByName = new();
    public Stack<Variable> variableStack = new();

    public Scope_GenerationPhase parent;

    public int CurrentRbpOffset
    {
        get
        {
            if (variableStack.Count == 0) return 0;
            
            Variable lastLocalVariable = variableStack.Peek();
            return lastLocalVariable.inscopeRbpOffset + lastLocalVariable.type.sizeInBytes;
        }
    }

    public Scope_GenerationPhase(Scope_StaticAnalysis staticScope)
    {
        this.staticScope = staticScope;
    }

    public Scope_GenerationPhase CreateSubScope(Scope_StaticAnalysis subStaticScope)
    {
        Scope_GenerationPhase child = new(subStaticScope);
        child.parent = this;

        return child;
    }

    public Variable RegisterLocalVariable(TypeInfo type, string name)
    {
        if (IsOverlapStackAllocated(CurrentRbpOffset, type.sizeInBytes))
        {
            throw new Exception();
        }

        int rbp = CurrentRbpOffset;
        
        Variable variable = new Variable(this, rbp)
        {
            name = name,
            type = type,
        };
        variableByName.Add(name, variable);
        variableStack.Push(variable);
        
        

        return variable;
    }

    public void UnregisterLocalVariable(Variable variable)
    {
        if (variable == null)
            throw new Exception($"Failed to deallocate null variable.");
        
        if (variableByName.ContainsKey(variable.name) == false)
            throw new Exception($"Failed to deallocate '{variable.name}' because it is not even allocated (or already deallocated) on stack.");
        
        if (variableStack.Peek() != variable)
            throw new Exception($"Failed to deallocate variable '{variable.name}' because it is not the last variable on stack, last is '{variableStack.Peek().name}'. Only last variable can be deallocated on stack.'");

        variableStack.Pop();
        variableByName.Remove(variable.name);
    }

    public void UnregisterLocalVariable(string name)
    {
        UnregisterLocalVariable(variableByName[name]);
    }

    public Variable GetVariable(string name)
    {
        if (variableByName.TryGetValue(name, out Variable var)) return var;

        if (parent != null) return parent.GetVariable(name);
        throw new Exception($"Variable '{name}' not found in current or parents scope");
    }
    
    private bool IsOverlapStackAllocated(int askAddress, int askSizeInBytes)
    {
        Range askRange = new Range(askAddress, askAddress + askSizeInBytes);
        
        foreach (Variable var in variableStack)
        {
            Range varRange = new Range(var.inscopeRbpOffset, var.inscopeRbpOffset + var.type.sizeInBytes);
            if (varRange.IsOverlap(askRange)) return true;
        }

        return false;
    }
}