﻿using System.Text;

namespace Astra.Compilation;

public abstract class CodeGeneratorBase
{
    public CodeStringBuilder b = new();

    public Scope_GenerationPhase currentScope;
    
    protected int addressOfHeapSize = 0x100; // On this address located a long, that describes the heap size
    protected int heapBaseAddress = 0x110; // The start (zero address) of heap
    private int thrownExceptionAddress = 0x104000;
    public virtual int ExceptionPointerAddress => thrownExceptionAddress;
    private int exceptionsHandlersStackAddress => thrownExceptionAddress + 8;
    
    private int anonVariableNameIndex;

    
    public void BeginSubScope()
    {
        // Prologue before sub scope creation
        Prologue();
        
        currentScope = currentScope.CreateSubScope(null);
    }

    public void DropSubScope()
    {
        currentScope = currentScope.parent;
        
        // Epilogue after sub scope drop
        Epilogue();
    }
    
    public virtual void Label(string labelName)
    {
        if (b.labels.Contains(labelName) == false)
        {
            throw new Exception($"Failed to generate label code because label '{labelName}' is not registered yet. You must register label before it's generation.");
        }

        b.Line(labelName + ":");
    }

    public virtual string RegisterLabel(string labelName)
    {
        if (b.labels.Contains(labelName))
        {
            labelName = NextLabel(labelName, 2);
        }

        b.labels.Add(labelName);
        return labelName;
    }

    private string NextLabel(string originalLabelName, int i)
    {
        string uniqueLabelName = originalLabelName + "_" + i;
        if (b.labels.Contains(uniqueLabelName))
        {
            return NextLabel(originalLabelName, i + 1);
        }
        return uniqueLabelName;
    }

    public abstract void Prologue();
    public abstract void Epilogue();
    public abstract void PrologueForSimulation(CompileTarget target, ResolvedModule module);
    public abstract void Return();
    public abstract void Out_Variable(FunctionInfo function, Variable variable);

    public virtual Variable Allocate(TypeInfo type)
    {
        anonVariableNameIndex++;
        return Allocate(type, $"anon_{anonVariableNameIndex}");
    }

    public abstract Variable Allocate(TypeInfo type, string name);
    
    public abstract void Deallocate(Variable variable);
    public abstract void Deallocate(int bytesToDellocate);

    public abstract void AllocateHeap(Variable storageOfPointerToHeap, Variable bytesToAllocateVariable);
    public abstract void AllocateHeap(Variable storageOfPointerToHeap, int bytesToAllocate);
    
    public abstract void SetValue(Variable variable, string value);
    public abstract void SetValue(Variable destination, Variable value);
    public abstract void SetValue(Variable destination, int address);
    public abstract void SetValueBehindPointer(Variable destination, Variable value);
    public abstract void SetValueBehindPointer(Variable destination, string value);

    public abstract void FieldAccess(int baseOffset, TypeInfo fieldType, int fieldOffset, Variable result, bool isGetter);

    public abstract void JumpIfFalse(Variable condition, string label);
    public abstract void JumpIfFalse(string reg, string label);
    public abstract void JumpToLabel(string label);

    public abstract void Compare(Variable a, Variable b, Token_Operator @operator, Variable result);

    public abstract void Calculate(Variable a, Variable b, Token_Operator @operator, Variable result);

    public abstract void LogicalNOT(Variable a, Variable result);

    public abstract void Negate(Variable a, Variable result);

    public abstract void ToPtr_Primitive(Variable askedVariable, Variable result);
    public abstract void ToPtr_Heap(Variable askedVariable, Variable result);
    public abstract void PtrAddress(Variable pointer, Variable result, bool isGetter);
    public abstract void PtrGet(Variable pointerVariable, Variable result);
    public abstract void PtrSet(Variable pointerVariable, Variable targetVariable);
    public abstract void PtrShift(Variable pointerVariable, Variable shiftVariable, int additionalShift = 0);
    public abstract void PtrShift(Variable pointerVariable, int shift);

    public abstract void Print(Variable variable);
    

    public abstract void PushToStack(Variable variable);
    public abstract void PushToStack(string str, TypeInfo type);

    public abstract void Call(string functionName);

    public abstract void PushExceptionHandler(string catchLabel);
    public abstract void ThrowException(Variable exception);

    public abstract void Cast(Variable variable, Variable result);

    public virtual void SectionData()
    {
        b.Line("section .data");
    }

    public virtual void SectionText()
    {
        b.Line("section .text");
    }

    public virtual void BufferString(string id, string value)
    {
        b.Line($"{id} db {value.Length}, \"{value}\", 0");
    }

    public virtual void Extern(string variableName)
    {
        b.Line($"extern {variableName}");
    }

    public abstract void Space(int lines = 1);
    public abstract void Comment(string comment);
    public abstract void Comment(string comment, int bookmarkDistance);

    public abstract byte[] Build();
}