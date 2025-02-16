using System.Text;

namespace Astra.Compilation;

public abstract class CodeGeneratorBase
{
    public CodeGeneratorBase parent;
    public List<CodeGeneratorBase> children = new();
    public CodeStringBuilder b;
    protected int rbpOffset;
    protected Dictionary<string, Variable> variableByName = new();
    protected Dictionary<int, Variable> variableByRBPOffset = new();
    protected Stack<Variable> variableStack = new();
    protected int anonVariableNameIndex;
    protected int addressOfHeapSize = 0x100; // On this address located a long, that describes the heap size
    protected int heapBaseAddress = 0x110; // The start (zero address) of heap
    private int thrownExceptionAddress = 0x104000;
    public virtual int ExceptionPointerAddress => thrownExceptionAddress;
    private int exceptionsHandlersStackAddress => thrownExceptionAddress + 8;

    public virtual void Label(string labelName)
    {
        if (b.labels.Contains(labelName) == false)
        {
            throw new Exception("Failed to generate label code because label '' is not registered yet. You must register label before it's generation.");
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
    public abstract void Return_Void();
    public abstract void Return_Variable(FunctionInfo function, Variable variable);

    public virtual Variable Allocate(TypeInfo type)
    {
        anonVariableNameIndex++;
        return Allocate(type, $"anon_{anonVariableNameIndex}");
    }

    public abstract Variable Allocate(TypeInfo type, string name);

    public abstract int AllocateRSPSaver();
    public abstract void RestoreRSPSaver(int saverRBPOffset);
    public abstract void DeallocateRSPSaver();

    public virtual void Deallocate(Variable variable)
    {
        if (variable == null)
        {
            throw new Exception($"Failed to deallocate null variable.");
        }
        if (variableByName.ContainsKey(variable.name) == false)
        {
            throw new Exception($"Failed to deallocate '{variable.name}' because it is not even allocated (or already deallocated) on stack.");
        }
        if (variableStack.Peek() != variable)
        {
            throw new Exception($"Failed to deallocate variable '{variable.name}' because it is not the last variable on stack. Only last variable can be deallocated on stack.'");
        }
        
        variableByName.Remove(variable.name);
        variableByRBPOffset.Remove(variable.rbpOffset);
        variableStack.Pop();

        int sizeInBytes = 8;
        b.CommentLine($"return allocation");
        b.Line($"add rsp, {sizeInBytes}");
    }

    public abstract void Deallocate(int sizeInBytes);

    public abstract void AllocateHeap(Variable storageOfPointerToHeap, Variable bytesToAllocateVariable);
    public abstract void AllocateHeap(Variable storageOfPointerToHeap, int bytesToAllocate);

    public Variable Register_FunctionArgumentVariable(FieldInfo info, int rbpOffset)
    {
        Variable variable = new Variable()
        {
            name = info.name,
            type = info.type,
            rbpOffset = rbpOffset
        };
        
        variableByName.Add(variable.name, variable);
        variableByRBPOffset.Add(variable.rbpOffset, variable);

        return variable;
    }

    public void Unregister_FunctionArgumentVariable(Variable variable)
    {
        variableByName.Remove(variable.name);
        variableByRBPOffset.Remove(variable.rbpOffset);
    }

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

    public virtual void LogicalNOT(Variable a, Variable result)
    {
        b.Line($"mov rbx, {a.RBP}");
        b.Line($"test rbx, rbx");
        b.Line($"xor rbx, rbx"); // reset rbx to zero
        b.Line($"sete bl"); // set last byte of reg to 1 or 0
        b.Line($"mov {result.RBP}, rbx");
    }

    public abstract void Negate(Variable a, Variable result);

    public abstract void ToPtr_Primitive(Variable askedVariable, Variable result);
    public abstract void ToPtr_Heap(Variable askedVariable, Variable result);
    public abstract void PtrAddress(Variable pointer, Variable result, bool isGetter);
    public abstract void PtrGet(Variable pointerVariable, Variable result);
    public abstract void PtrSet(Variable pointerVariable, Variable targetVariable);
    public abstract void PtrShift(Variable pointerVariable, Variable shiftVariable, int additionalShift = 0);
    public abstract void PtrShift(Variable pointerVariable, int shift);

    public virtual void Print(Variable variable)
    {
        b.Space();
        b.CommentLine($"print {variable.name}");
        b.Line($"mov rbx, {variable.RBP}");
        b.Line($"print [rbx]");
    }
    

    public abstract void PushToStack(Variable variable, string comment = null);
    public abstract void PushToStack(string str, TypeInfo type, string comment = null);

    public abstract void Call(string functionName);

    public virtual void PushExceptionHandler(string catchLabel)
    {
        string addressOfStackSize = $"0x{exceptionsHandlersStackAddress.ToString("x")}";
        string addressOfStackBegin = $"0x{(exceptionsHandlersStackAddress + 8).ToString("x")}";
        
        b.Space();
        b.CommentLine($"Push exception handler: catch '{catchLabel}'");
        b.Line($"mov rbx, [{addressOfStackSize}]");
        
        b.Line($"mov rdx, rbx");
        b.Line($"add rdx, {addressOfStackBegin}");
        b.Line($"mov qword [rdx], {catchLabel}");
        
        b.Line($"add rbx, 8");
        b.Line($"mov [{addressOfStackSize}], rbx");
        b.Space();
    }

    public virtual void ThrowException(Variable exception)
    {
        b.Space();
        b.CommentLine($"Throw exception");
        
        string addressOfStackSize = $"0x{exceptionsHandlersStackAddress.ToString("x")}";
        string addressOfStackBegin = $"0x{(exceptionsHandlersStackAddress + 8).ToString("x")}";
        string addressOfException = $"0x{(thrownExceptionAddress).ToString("x")}";
        
        // Place exception into memory
        b.Line($"mov rdx, {addressOfException}");
        b.Line($"mov rbx, {exception.RBP}");
        b.Line($"mov [rdx], rbx");
        
        // Pop stack
        b.Line($"mov rbx, [{addressOfStackSize}]");
        
        // b.Line($"mov rbx, rdx");
        b.Line($"sub rbx, 8");
        b.Line($"mov [{addressOfStackSize}], rbx");
        
        b.Line($"add rbx, {addressOfStackBegin}");
        b.Line($"mov rbx, [rbx]");
        
        b.Line($"jmp rbx");
        b.Space();
    }

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

    public virtual Variable GetVariable(string name)
    {
        if (variableByName.TryGetValue(name, out Variable var)) return var;

        if (parent != null) return parent.GetVariable(name);
        
        throw new Exception($"Variable '{name}' not found in scope");
    }

    public abstract byte[] Build();
}