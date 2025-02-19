using AVM.Compiler;

namespace Astra.Compilation;

public class Scope_GenerationPhase
{
    public Scope_StaticAnalysis staticScope;
    
    public Dictionary<string, Variable> variableByName = new();
    public Stack<Variable> variableStack = new();

    public Scope_GenerationPhase parent;

    public UniqueGenerator uniqueGenerator;

    public int CurrentRbpOffset
    {
        get
        {
            if (variableStack.Count == 0) return 0;
            
            Variable lastLocalVariable = variableStack.Peek();
            return lastLocalVariable.inscopeRbpOffset + lastLocalVariable.type.refSizeInBytes;
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
        child.uniqueGenerator = uniqueGenerator;

        return child;
    }

    public Variable RegisterLocalVariable(TypeInfo type, string name)
    {
        Variable variable = new Variable(this, CurrentRbpOffset)
        {
            name = name,
            type = type,
        };
        variableByName.Add(variable.name, variable);
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

    public int GetRelativeRBP(Variable askedVariable)
    {
        if (variableByName.ContainsValue(askedVariable))
        {
            // Asked variable is local variable of current scope
            // Return positive rbp offset
            return askedVariable.inscopeRbpOffset;
        }
        
        int relativeRBP = 0;
        Scope_GenerationPhase scope = parent;

        while (scope != null)
        {
            relativeRBP -= Constants.RBP_REG_SIZE;
            
            for (int i = 0; i < scope.variableStack.Count; i++)
            {
                Variable variable = scope.variableStack.ElementAt(i); // where Stack.ElementAt(0) is Stack.Peek()
                relativeRBP -= variable.type.refSizeInBytes;

                if (variable == askedVariable)
                {
                    return relativeRBP;
                }
            }
            
            scope = scope.parent;
        }
        
        throw new Exception($"Variable '{askedVariable.name}' not found in current or parents scope");
    }
}