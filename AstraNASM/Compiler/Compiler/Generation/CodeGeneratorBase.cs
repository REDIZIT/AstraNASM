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
    private int addressOfHeapSize = 0x100; // On this address located a long, that describes the heap size
    private int heapBaseAddress = 0x110; // The start (zero address) of heap
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

    public virtual void Prologue()
    {
        b.Line("push rbp");
        b.Line("mov rbp, rsp");
    }

    public virtual void Epilogue()
    {
        b.Line("mov rsp, rbp");
        b.Line("pop rbp");
    }

    public abstract void PrologueForSimulation(CompileTarget target);
    public abstract void Return_Void();
    public abstract void Return_Variable(FunctionInfo function, Variable variable);

    public virtual void Return_Field(FunctionInfo function, Variable variable)
    {
        int rbpOffset = 16 + function.arguments.Count * 8;
        if (function.owner != null) rbpOffset += 8;

        b.Line($"mov rbx, {variable.RBP} ; rbx - address of address to primitive");
        // b.Line($"mov rbx, [rbx] ; rbx - address to primitive");
        b.Line($"mov [rbp+{rbpOffset}], [rbx] ; [rbx] - value of primitive");
    }

    public virtual Variable Allocate(TypeInfo type)
    {
        anonVariableNameIndex++;
        return Allocate(type, $"anon_{anonVariableNameIndex}");
    }

    public abstract Variable Allocate(TypeInfo type, string name);

    public virtual int AllocateRSPSaver()
    {
        rbpOffset -= 8;
        b.Line($"push rsp ; allocate saver at [rbp{rbpOffset}]");
        return rbpOffset;
    }

    public virtual void RestoreRSPSaver(int saverRBPOffset)
    {
        b.Line($"mov rsp, [rbp{saverRBPOffset}] ; restore saver");
    }

    public virtual void DeallocateRSPSaver()
    {
        rbpOffset -= 8;
        b.Line($"pop rsp ; deallocate saver");
    }

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

    public virtual void Deallocate(int sizeInBytes)
    {
        b.Line($"add rsp, {sizeInBytes}");
    }

    public virtual void AllocateHeap(Variable storageOfPointerToHeap, Variable bytesToAllocateVariable)
    {
        AllocateHeap(storageOfPointerToHeap);
        
        b.Line($"mov rbx, [{addressOfHeapSize}]"); 
        b.Line($"mov rdx, {bytesToAllocateVariable.RBP}");
        b.Line($"add rbx, [rdx]"); // TODO: Why double depoint required? Think about that in daytime
        b.Line($"mov [{addressOfHeapSize}], rbx");
    }

    public virtual void AllocateHeap(Variable storageOfPointerToHeap, int bytesToAllocate)
    {
        AllocateHeap(storageOfPointerToHeap);
        
        b.Line($"mov rbx, [{addressOfHeapSize}]");
        b.Line($"add rbx, {bytesToAllocate}");
        b.Line($"mov [{addressOfHeapSize}], rbx");
    }

    private void AllocateHeap(Variable storageOfPointerToHeap)
    {
        b.CommentLine($"heap alloc");
        b.Line($"mov rbx, [{addressOfHeapSize}]"); // rbx - next heap byte address
        
        b.Line($"mov rdx, {heapBaseAddress}");
        b.Line($"add rdx, rbx");
        
        b.Line($"mov qword {storageOfPointerToHeap.RBP}, rdx");
    }

    public virtual Variable Register_FunctionArgumentVariable(FieldInfo info, int index)
    {
        Variable variable = new Variable()
        {
            name = info.name,
            type = info.type,
            rbpOffset = 16 + index * 8
        };
        
        variableByName.Add(variable.name, variable);
        variableByRBPOffset.Add(variable.rbpOffset, variable);

        return variable;
    }

    public virtual void Unregister_FunctionArgumentVariable(Variable variable)
    {
        variableByName.Remove(variable.name);
        variableByRBPOffset.Remove(variable.rbpOffset);
    }

    public abstract void SetValue(Variable variable, string value);
    public abstract void SetValue(Variable destination, Variable value);

    public virtual void SetValue(Variable destination, int address)
    {
        b.Line($"mov qword rbx, [{address}]");
        b.Line($"mov qword {destination.RBP}, rbx");
    }

    public virtual void SetValueBehindPointer(Variable destination, Variable value)
    {
        b.Line($"mov rbx, {destination.RBP}");
        b.Line($"mov rdx, {value.RBP}");
        b.Line($"mov qword [rbx], rdx");
    }

    public virtual void SetValueBehindPointer(Variable destination, string value)
    {
        b.Line($"mov rbx, {destination.RBP}");
        b.Line($"mov qword [rbx], {value}");
    }

    public virtual void SetValueFromReg(Variable destination, string sourceReg)
    {
        b.Line($"mov {destination.RBP}, {sourceReg}");
    }

    public virtual void SetValueToReg(string destReg, Variable source)
    {
        b.Line($"mov {destReg}, {source.RBP}");
    }

    public virtual void FieldAccess(int baseOffset, TypeInfo fieldType, int fieldOffset, Variable result, bool isGetter)
    {
        if (fieldOffset < 0) throw new Exception("Negative fieldOffset is not allowed.");
        
        // Load from ram address to ref-type inside heap
        string rbp = baseOffset > 0 ? "+" + baseOffset : baseOffset.ToString();
        b.Line($"mov rbx, [rbp{rbp}]");
        
        // If we accessing not first field
        if (fieldOffset != 0)
        {
            // Add offset to rbx to go to field inside ref-type
            b.Line($"add rbx, {fieldOffset}");
        }
        
        // Now rbx is pointing to valid address of ref-type.field
        
        // If we don't need a pointer (like setter), but want to get a value (like getter)
        if (isGetter)
        {
            // Depoint rbx to get actual field value
            
            string nasmType = Utils.GetNASMType(fieldType);
            b.Line($"mov {nasmType} {Utils.ClampRegister(nasmType, "rbx")}, [rbx] ; depoint one more time due to getter");
            
            // b.Line($"mov rbx, [rbx] ; depoint one more time due to getter");
        }
        
        // Put result (ref for setter and value for getter) inside result variable
        b.Line($"mov {result.RBP}, rbx");
        
        
    }

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

    public virtual void CalculateAddress_RBP_Shift(Variable shiftInBytes, Variable result)
    {
        b.Line($"mov rbx, rbp");
        b.Line($"add rbx, {shiftInBytes.RBP}");
        SetValueFromReg(result, "rbx");
    }

    public virtual void CalculateAddress_RBP_Shift(int shiftInBytes, Variable result)
    {
        b.Line($"mov rbx, rbp");
        b.Line($"add rbx, {shiftInBytes}");
        SetValueFromReg(result, "rbx");
    }

    public virtual void PushToStack(Variable variable, string comment = null)
    {
        PushToStack(variable.RBP, comment);
    }

    public virtual void PushToStack(string value, string comment = null)
    {
        b.Line($"mov rbx, {value}" + (string.IsNullOrWhiteSpace(comment) ? "" : " ; " + comment));
        b.Line($"push rbx");
    }

    public virtual void PushRegToStack(string regName, string comment = null)
    {
        b.Line($"push {regName}" + (string.IsNullOrWhiteSpace(comment) ? "" : " ; " + comment));
    }

    public virtual void PopRegFromStack(string regName, string comment = null)
    {
        b.Line($"pop {regName}" + (string.IsNullOrWhiteSpace(comment) ? "" : " ; " + comment));
    }

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